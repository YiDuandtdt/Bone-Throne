using BoneThrone.Combat;
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
        [SerializeField] private SkillTargetingService targetingService;
        [SerializeField] private DamageResolver damageResolver;
        [SerializeField] private SkillEffectExecutor effectExecutor;
        [SerializeField] private TurnManager turnManager;
        [SerializeField] private ActionPermissionService actionPermissionService;
        [SerializeField] private CombatLog combatLog;

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

            runtime.StartCooldown(slotIndex);
            MarkCasterActed(caster);

            int cooldown = runtime.GetCooldown(slotIndex);
            if (combatLog != null)
            {
                for (int i = 0; i < effectResult.DamageEntries.Count; i++)
                {
                    SkillDamageLogEntry damageEntry = effectResult.DamageEntries[i];
                    combatLog.LogSkillDamage(caster, damageEntry.Target, skill, damageEntry.Damage, damageEntry.RemainingHp, damageEntry.IsPrimaryTarget);
                    if (damageEntry.TargetDied)
                    {
                        combatLog.LogDeath(damageEntry.Target);
                    }
                }

                if (cooldown > 0)
                {
                    combatLog.LogSkillCooldown(caster, skill, cooldown);
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
                + effectResult.Summary
                + ". TargetDied="
                + targetDied
                + " Cooldown="
                + cooldown
                + ".",
                this);

            return true;
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
                return false;
            }

            return actionPermissionService.CanAct(caster, turnManager);
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
            if (combatLog != null)
            {
                combatLog.LogSkillRejected(reason, context);
                return;
            }

            Debug.LogWarning("Skill rejected: " + reason, context);
        }
    }
}
