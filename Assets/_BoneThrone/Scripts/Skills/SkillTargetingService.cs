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
                reason = "技能尚未解锁。";
                return false;
            }

            if (runtime.IsOnCooldown(slotIndex))
            {
                reason = "技能正在冷却。";
                return false;
            }

            if (!IsValidTarget(caster, target, skill, out reason))
            {
                return false;
            }

            if (!IsInRange(caster, target, skill))
            {
                reason = "目标超出技能范围。";
                return false;
            }

            reason = "技能目标有效。";
            return true;
        }

        public bool IsValidTarget(Unit caster, Unit target, SkillData skill, out string reason)
        {
            if (caster == null)
            {
                reason = "施法者不存在。";
                return false;
            }

            if (target == null)
            {
                reason = "目标不存在。";
                return false;
            }

            if (skill == null)
            {
                reason = "技能数据缺失。";
                return false;
            }

            switch (skill.TargetType)
            {
                case SkillTargetType.Enemy:
                    if (caster == target)
                    {
                        reason = "敌方技能不能以自己为目标。";
                        return false;
                    }

                    if (caster.Faction == target.Faction)
                    {
                        reason = "敌方技能不能选择同阵营目标。";
                        return false;
                    }

                    break;

                case SkillTargetType.Ally:
                    if (caster.Faction != target.Faction)
                    {
                        reason = "友方技能不能选择敌方目标。";
                        return false;
                    }

                    break;

                case SkillTargetType.Self:
                    if (caster != target)
                    {
                        reason = "自身技能只能以自己为目标。";
                        return false;
                    }

                    break;

                case SkillTargetType.AnyUnit:
                    break;

                default:
                    reason = "不支持的技能目标类型。";
                    return false;
            }

            reason = "技能目标类型有效。";
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
                reason = "施法者不存在。";
                return false;
            }

            if (target == null)
            {
                reason = "目标不存在。";
                return false;
            }

            if (runtime == null)
            {
                reason = "当前角色没有技能组件。";
                return false;
            }

            if (slotIndex < 0 || slotIndex >= runtime.SlotCount)
            {
                reason = "技能槽位无效。";
                return false;
            }

            if (skill == null)
            {
                reason = "该技能槽没有技能。";
                return false;
            }

            if (caster.Stats == null)
            {
                reason = "施法者属性缺失。";
                return false;
            }

            if (!caster.IsAlive)
            {
                reason = "施法者已倒下。";
                return false;
            }

            if (!target.IsAlive)
            {
                reason = "目标已倒下。";
                return false;
            }

            if (caster.CurrentTile == null)
            {
                reason = "施法者没有所在格。";
                return false;
            }

            if (target.CurrentTile == null)
            {
                reason = "目标没有所在格。";
                return false;
            }

            reason = "技能基础校验通过。";
            return true;
        }
    }
}
