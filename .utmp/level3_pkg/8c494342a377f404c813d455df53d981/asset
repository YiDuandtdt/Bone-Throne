using BoneThrone.Grid;
using BoneThrone.Units;
using UnityEngine;

namespace BoneThrone.Skills
{
    /// <summary>
    /// Validates Phase 11 skill unlock, cooldown, target type, and grid range.
    /// It does not apply damage, consume actions, or mutate cooldowns.
    /// </summary>
    public sealed class SkillTargetingService : MonoBehaviour
    {
        public bool CanUseSkill(Unit caster, Unit target, SkillRuntime runtime, int slotIndex, out string reason)
        {
            SkillData skill = runtime != null ? runtime.GetSkill(slotIndex) : null;
            if (!ValidateBase(caster, target, runtime, skill, slotIndex, out reason))
            {
                return false;
            }

            if (!runtime.IsUnlocked(caster, slotIndex))
            {
                reason = "Skill slot " + slotIndex + " is locked for unit level " + caster.Stats.Level + ". Required level=" + skill.UnlockLevel + ".";
                return false;
            }

            if (runtime.IsOnCooldown(slotIndex))
            {
                reason = "Skill slot " + slotIndex + " is on cooldown. Remaining=" + runtime.GetCooldown(slotIndex) + ".";
                return false;
            }

            if (!IsValidTarget(caster, target, skill, out reason))
            {
                return false;
            }

            if (!IsInRange(caster, target, skill))
            {
                reason = "Target is out of skill range. Distance=" + GetManhattanDistance(caster, target) + " Range=" + skill.Range + ".";
                return false;
            }

            reason = "Skill target is valid.";
            return true;
        }

        public bool IsValidTarget(Unit caster, Unit target, SkillData skill, out string reason)
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

            if (skill == null)
            {
                reason = "SkillData is missing.";
                return false;
            }

            switch (skill.TargetType)
            {
                case SkillTargetType.Enemy:
                    if (caster == target)
                    {
                        reason = "Enemy skill cannot target the caster.";
                        return false;
                    }

                    if (caster.Faction == target.Faction)
                    {
                        reason = "Enemy skill cannot target a unit with the same faction.";
                        return false;
                    }

                    break;

                case SkillTargetType.Ally:
                    if (caster.Faction != target.Faction)
                    {
                        reason = "Ally skill cannot target an enemy faction.";
                        return false;
                    }

                    break;

                case SkillTargetType.Self:
                    if (caster != target)
                    {
                        reason = "Self skill can only target the caster.";
                        return false;
                    }

                    break;

                case SkillTargetType.AnyUnit:
                    break;

                default:
                    reason = "Unsupported skill target type: " + skill.TargetType + ".";
                    return false;
            }

            reason = "Target type is valid.";
            return true;
        }

        public bool IsInRange(Unit caster, Unit target, SkillData skill)
        {
            if (skill == null)
            {
                return false;
            }

            return GetManhattanDistance(caster, target) <= skill.Range;
        }

        public int GetManhattanDistance(Unit caster, Unit target)
        {
            if (caster == null || target == null || caster.CurrentTile == null || target.CurrentTile == null)
            {
                return int.MaxValue;
            }

            GridPosition casterPosition = caster.CurrentTile.Position;
            GridPosition targetPosition = target.CurrentTile.Position;
            return Mathf.Abs(casterPosition.X - targetPosition.X) + Mathf.Abs(casterPosition.Y - targetPosition.Y);
        }

        private static bool ValidateBase(Unit caster, Unit target, SkillRuntime runtime, SkillData skill, int slotIndex, out string reason)
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

            if (runtime == null)
            {
                reason = "SkillRuntime is missing on caster.";
                return false;
            }

            if (slotIndex < 0 || slotIndex >= runtime.SlotCount)
            {
                reason = "Skill slot index " + slotIndex + " is outside runtime slot count " + runtime.SlotCount + ".";
                return false;
            }

            if (skill == null)
            {
                reason = "Skill slot " + slotIndex + " has no SkillData.";
                return false;
            }

            if (caster.Stats == null)
            {
                reason = "Caster UnitStats is missing.";
                return false;
            }

            if (!caster.IsAlive)
            {
                reason = "Caster is dead.";
                return false;
            }

            if (!target.IsAlive)
            {
                reason = "Target is dead.";
                return false;
            }

            if (caster.CurrentTile == null)
            {
                reason = "Caster has no current tile.";
                return false;
            }

            if (target.CurrentTile == null)
            {
                reason = "Target has no current tile.";
                return false;
            }

            reason = "Base skill validation passed.";
            return true;
        }
    }
}
