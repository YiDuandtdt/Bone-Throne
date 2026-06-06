using System.Collections;
using BoneThrone.Audio;
using BoneThrone.Combat;
using BoneThrone.Core;
using BoneThrone.Turns;
using BoneThrone.Units;
using UnityEngine;

namespace BoneThrone.Skills
{
    /// <summary>
    /// Executes minimal Phase 11 guaranteed-hit skills after validation.
    /// It does not roll D20, apply buffs, spawn VFX, drive UI, or use networking.
    /// </summary>
    public sealed class SkillSystem : MonoBehaviour
    {
        private static bool AnimationDebug { get { return false; } }

        [SerializeField] private SkillTargetingService targetingService;
        [SerializeField] private DamageResolver damageResolver;
        [SerializeField] private SkillEffectExecutor effectExecutor;
        [SerializeField] private TurnManager turnManager;
        [SerializeField] private ActionPermissionService actionPermissionService;
        [SerializeField] private CombatLog combatLog;
        [Header("Presentation Timing")]
        [SerializeField] [Min(0f)] private float skillImpactDelay = 0.2f;
        [SerializeField] [Min(0f)] private float meleeSkillImpactDelay = 0.65f;
        [SerializeField] [Min(0f)] private float barbarianSkillSfxDelay = 0.5f;
        [SerializeField] [Min(0f)] private float fighterFirstSkillSfxDelay = 0.3f;
        [SerializeField] [Min(0f)] private float fighterFirstSkillSfxVolumeScale = 2f;

        public bool CanUseSkillOnTarget(Unit caster, Unit target, int slotIndex, out string reason)
        {
            if (caster == null)
            {
                reason = "Caster is missing.";
                return false;
            }

            if (target == null)
            {
                reason = "Target is missing.";
                return false;
            }

            if (!caster.IsAlive)
            {
                reason = "Caster is dead.";
                return false;
            }

            SkillRuntime runtime = caster.GetComponent<SkillRuntime>();
            if (runtime == null)
            {
                reason = "Caster has no SkillRuntime component.";
                return false;
            }

            if (!CanPassTurnGate(caster, out reason))
            {
                return false;
            }

            if (targetingService == null)
            {
                reason = "SkillTargetingService is missing.";
                return false;
            }

            if (damageResolver == null)
            {
                reason = "DamageResolver is missing.";
                return false;
            }

            return targetingService.CanUseSkill(caster, target, runtime, slotIndex, out reason);
        }

        public bool TryUseSkill(Unit caster, Unit target, int slotIndex)
        {
            if (caster == null)
            {
                LogRejected("caster is missing.", this);
                return false;
            }

            SkillRuntime runtime = caster.GetComponent<SkillRuntime>();
            if (runtime == null)
            {
                LogRejected("caster has no SkillRuntime component.", caster);
                return false;
            }

            if (!ValidateTurnGate(caster))
            {
                return false;
            }

            if (targetingService == null)
            {
                LogRejected("SkillTargetingService is missing.", this);
                return false;
            }

            if (damageResolver == null)
            {
                LogRejected("DamageResolver is missing.", this);
                return false;
            }

            string reason;
            if (!targetingService.CanUseSkill(caster, target, runtime, slotIndex, out reason))
            {
                LogRejected(reason, caster);
                return false;
            }

            SkillData skill = runtime.GetSkill(slotIndex);
            UnitAnimationController casterAnimation = caster.GetComponent<UnitAnimationController>();
            LogAnimationDebug(caster, casterAnimation, "PlaySkill");
            if (casterAnimation != null)
            {
                if (target != null)
                {
                    casterAnimation.FaceTowards(target.transform.position);
                }

                casterAnimation.PlaySkill();
            }
            PlaySkillSfx(caster, skill, slotIndex);

            runtime.StartCooldown(slotIndex);
            MarkCasterActed(caster);
            StartCoroutine(ResolveSkillImpact(caster, target, skill));

            int cooldown = runtime.GetCooldown(slotIndex);
            if (combatLog != null && cooldown > 0)
            {
                combatLog.LogSkillCooldown(caster, skill, cooldown);
            }

            return true;
        }

        private IEnumerator ResolveSkillImpact(Unit caster, Unit target, SkillData skill)
        {
            float delay = GetSkillImpactDelay(caster);
            if (delay > 0f)
            {
                yield return new WaitForSeconds(delay);
            }

            if (caster == null || target == null || skill == null)
            {
                TryAutoEndCasterTurn(caster);
                yield break;
            }

            if (!caster.IsAlive || !target.IsAlive)
            {
                TryAutoEndCasterTurn(caster);
                yield break;
            }

            RangerHitPresentationConfig rangerPresentation = caster.GetComponent<RangerHitPresentationConfig>();
            if (rangerPresentation != null)
            {
                float arrowFlightDelay = rangerPresentation.TryPlaySkillArrow(caster, target, turnManager);
                if (arrowFlightDelay > 0f)
                {
                    yield return new WaitForSeconds(arrowFlightDelay);
                }
            }

            if (!caster.IsAlive || !target.IsAlive)
            {
                TryAutoEndCasterTurn(caster);
                yield break;
            }

            if (rangerPresentation != null)
            {
                rangerPresentation.TryPlaySkillImpactEffect(caster, target, skill);
            }

            MageHitPresentationConfig magePresentation = caster.GetComponent<MageHitPresentationConfig>();
            if (magePresentation != null)
            {
                magePresentation.TryPlaySkillImpactEffect(caster, target, skill);
            }

            SkillImpactResolution resolution = ExecuteSkillImpact(caster, target, skill);
            if (combatLog != null && resolution.EffectResult != null)
            {
                for (int i = 0; i < resolution.EffectResult.DamageEntries.Count; i++)
                {
                    SkillDamageLogEntry damageEntry = resolution.EffectResult.DamageEntries[i];
                    combatLog.LogSkillDamage(caster, damageEntry.Target, skill, damageEntry.Damage, damageEntry.RemainingHp, damageEntry.IsPrimaryTarget);
                    if (damageEntry.TargetDied)
                    {
                        combatLog.LogDeath(damageEntry.Target);
                    }
                }
            }

            Debug.Log(
                "SkillSystem: unit "
                + caster.UnitId
                + " used skill "
                + skill.DisplayName
                + " on unit "
                + target.UnitId
                + ". "
                + resolution.EffectResult.Summary
                + ". TargetDied="
                + resolution.TargetDied
                + ".",
                this);
            TryAutoEndCasterTurn(caster);
        }

        private SkillImpactResolution ExecuteSkillImpact(Unit caster, Unit target, SkillData skill)
        {
            SkillEffectResult effectResult;
            bool targetDied;
            if (effectExecutor != null)
            {
                targetDied = effectExecutor.TryExecute(caster, target, skill, damageResolver, out effectResult);
            }
            else
            {
                effectResult = new SkillEffectResult { Summary = "Phase 11 fallback guaranteed damage " + skill.GuaranteedDamage + "." };
                targetDied = damageResolver.ApplyDamage(target, skill.GuaranteedDamage);
                int fallbackRemainingHp = target.RuntimeState != null ? target.RuntimeState.CurrentHp : 0;
                effectResult.AddDamage(target, skill.GuaranteedDamage, fallbackRemainingHp, targetDied, true);
            }

            return new SkillImpactResolution
            {
                TargetDied = targetDied,
                EffectResult = effectResult
            };
        }

        private float GetSkillImpactDelay(Unit caster)
        {
            if (IsMeleePresentation(caster))
            {
                return meleeSkillImpactDelay > 0f ? meleeSkillImpactDelay : 0.65f;
            }

            return skillImpactDelay > 0f ? skillImpactDelay : 0.2f;
        }

        private void PlaySkillSfx(Unit caster, SkillData skill, int slotIndex)
        {
            BTAudioCueId cue = BTAudioService.GetSkillCue(skill != null ? skill.DisplayName : null);
            float delay = GetSkillSfxDelay(caster, skill, slotIndex);
            float volumeScale = GetSkillSfxVolumeScale(caster, skill, slotIndex);

            if (delay <= 0f)
            {
                PlaySkillSfxNow(cue, volumeScale);
                return;
            }

            StartCoroutine(PlaySkillSfxAfterDelay(cue, volumeScale, delay));
        }

        private IEnumerator PlaySkillSfxAfterDelay(BTAudioCueId cue, float volumeScale, float delay)
        {
            yield return new WaitForSeconds(Mathf.Max(0f, delay));
            PlaySkillSfxNow(cue, volumeScale);
        }

        private static void PlaySkillSfxNow(BTAudioCueId cue, float volumeScale)
        {
            if (volumeScale > 1.001f)
            {
                BTAudioService.PlaySfx(cue, volumeScale, 1f);
                return;
            }

            BTAudioService.PlaySfx(cue);
        }

        private float GetSkillSfxDelay(Unit caster, SkillData skill, int slotIndex)
        {
            if (caster == null)
            {
                return 0f;
            }

            if (caster.RoleId == RoleId.Barbarian)
            {
                return Mathf.Max(0f, barbarianSkillSfxDelay);
            }

            if (IsFighterFirstSkill(caster, skill, slotIndex))
            {
                return Mathf.Max(0f, fighterFirstSkillSfxDelay);
            }

            return 0f;
        }

        private float GetSkillSfxVolumeScale(Unit caster, SkillData skill, int slotIndex)
        {
            return IsFighterFirstSkill(caster, skill, slotIndex)
                ? Mathf.Max(0f, fighterFirstSkillSfxVolumeScale)
                : 1f;
        }

        private static bool IsFighterFirstSkill(Unit caster, SkillData skill, int slotIndex)
        {
            if (caster == null || caster.RoleId != RoleId.Fighter)
            {
                return false;
            }

            if (slotIndex == 0)
            {
                return true;
            }

            return skill != null && skill.DisplayName == "fighter_shield_bash";
        }

        private static bool IsMeleePresentation(Unit caster)
        {
            if (caster == null)
            {
                return true;
            }

            if (caster.GetComponent<RangerHitPresentationConfig>() != null
                || caster.GetComponent<MageHitPresentationConfig>() != null)
            {
                return false;
            }

            return caster.RoleId == RoleId.Fighter
                || caster.RoleId == RoleId.Barbarian
                || caster.RoleId == RoleId.Enemy
                || caster.RoleId == RoleId.None;
        }

        public void TickCooldownsForUnit(Unit unit)
        {
            if (unit == null)
            {
                Debug.LogWarning("SkillSystem cannot tick cooldowns because Unit is missing.", this);
                return;
            }

            SkillRuntime runtime = unit.GetComponent<SkillRuntime>();
            if (runtime == null)
            {
                Debug.LogWarning("SkillSystem cannot tick cooldowns because unit " + unit.UnitId + " has no SkillRuntime.", unit);
                return;
            }

            runtime.TickCooldowns();
            Debug.Log("SkillSystem ticked skill cooldowns for unit " + unit.UnitId + ".", unit);
        }

        public void TickCooldownsForUnits(Unit[] units)
        {
            if (units == null)
            {
                Debug.LogWarning("SkillSystem cannot tick cooldowns because the unit array is missing.", this);
                return;
            }

            for (int i = 0; i < units.Length; i++)
            {
                Unit unit = units[i];
                if (unit == null)
                {
                    continue;
                }

                TickCooldownsForUnit(unit);
            }
        }

        private bool CanPassTurnGate(Unit caster, out string reason)
        {
            bool hasTurnManager = turnManager != null;
            bool hasActionPermissionService = actionPermissionService != null;

            if (hasTurnManager != hasActionPermissionService)
            {
                reason = "Turn gating is partially configured. Bind both TurnManager and ActionPermissionService, or leave both empty for test mode.";
                return false;
            }

            if (!hasTurnManager)
            {
                reason = "Turn gating is not configured.";
                return true;
            }

            UnitTurnState turnState = caster != null ? caster.GetComponent<UnitTurnState>() : null;
            if (turnState == null)
            {
                reason = "Caster has no UnitTurnState.";
                return false;
            }

            if (turnManager.CurrentPhase != TurnPhase.PlayerTurn)
            {
                reason = "Current phase is " + turnManager.CurrentPhase + ".";
                return false;
            }

            if (caster.Faction != UnitFaction.Player)
            {
                reason = "Only player units can act during PlayerTurn.";
                return false;
            }

            if (turnState.HasActed)
            {
                reason = "Caster has already acted.";
                return false;
            }

            if (actionPermissionService.RequireCurrentRole && caster.RoleId != turnManager.CurrentRole)
            {
                reason = "Caster role " + caster.RoleId + " does not match current role " + turnManager.CurrentRole + ".";
                return false;
            }

            bool canAct = actionPermissionService.CanAct(caster, turnManager);
            reason = canAct ? "Turn gate passed." : "Caster cannot act.";
            return canAct;
        }

        private bool ValidateTurnGate(Unit caster)
        {
            bool hasTurnManager = turnManager != null;
            bool hasActionPermissionService = actionPermissionService != null;

            if (hasTurnManager != hasActionPermissionService)
            {
                LogRejected("turn gating is partially configured. Bind both TurnManager and ActionPermissionService, or leave both empty for test mode.", caster);
                return false;
            }

            if (!hasTurnManager)
            {
                return true;
            }

            if (actionPermissionService.TryConsumeStunForAction(caster, turnManager))
            {
                TryAutoEndCasterTurn(caster);
                return false;
            }

            return actionPermissionService.CanAct(caster, turnManager);
        }

        private void TryAutoEndCasterTurn(Unit caster)
        {
            if (turnManager != null)
            {
                turnManager.TryAutoEndPlayerUnitTurnIfNoAvailableActions(caster);
            }
        }

        private static void MarkCasterActed(Unit caster)
        {
            UnitTurnState turnState = caster != null ? caster.GetComponent<UnitTurnState>() : null;
            if (turnState != null)
            {
                turnState.MarkActed();
            }
        }

        private void LogRejected(string reason, Object context)
        {
            BTAudioService.PlaySfx(BTAudioCueId.InvalidAction);
            if (combatLog != null)
            {
                combatLog.LogSkillRejected(reason, context);
                return;
            }

            Debug.LogWarning("Skill rejected: " + reason, context);
        }

        private void LogAnimationDebug(Unit unit, UnitAnimationController animationController, string method)
        {
            if (!AnimationDebug)
            {
                return;
            }

            Debug.Log(
                "AnimationDebug SkillSystem: unit="
                + (unit != null ? unit.name : "null")
                + " UnitId="
                + (unit != null ? unit.UnitId.ToString() : "n/a")
                + " controller="
                + (animationController != null ? animationController.name : "null")
                + " method="
                + method
                + ".",
                unit);
        }
    }
}
