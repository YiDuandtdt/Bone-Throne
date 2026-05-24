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
        [SerializeField] private TurnManager turnManager;
        [SerializeField] private ActionPermissionService actionPermissionService;

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
            bool targetDied = damageResolver.ApplyDamage(target, skill.GuaranteedDamage);
            runtime.StartCooldown(slotIndex);
            MarkCasterActed(caster);

            Debug.Log(
                "SkillSystem: unit "
                + caster.UnitId
                + " used skill "
                + skill.DisplayName
                + " on unit "
                + target.UnitId
                + " for guaranteed damage "
                + skill.GuaranteedDamage
                + ". TargetDied="
                + targetDied
                + " Cooldown="
                + runtime.GetCooldown(slotIndex)
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
            Debug.LogWarning("Skill rejected: " + reason, context);
        }
    }
}
