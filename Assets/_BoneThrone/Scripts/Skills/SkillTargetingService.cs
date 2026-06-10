using System.Collections.Generic;
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
            if (skill == null || caster == null || target == null || caster.CurrentTile == null || target.CurrentTile == null)
            {
                return false;
            }

            return IsPositionInRange(caster, skill, target.CurrentTile.Position);
        }

        public bool TryFillRangeTiles(Unit caster, SkillData skill, GridManager gridManager, List<Tile> results, out string reason)
        {
            if (results != null)
            {
                results.Clear();
            }

            if (results == null)
            {
                reason = "Range result list is missing.";
                return false;
            }

            if (caster == null || caster.CurrentTile == null)
            {
                reason = "Caster or caster tile is missing.";
                return false;
            }

            if (skill == null)
            {
                reason = "SkillData is missing.";
                return false;
            }

            if (gridManager == null)
            {
                reason = "GridManager is missing.";
                return false;
            }

            List<Tile> registeredTiles = new List<Tile>();
            gridManager.FillRegisteredTiles(registeredTiles);
            for (int i = 0; i < registeredTiles.Count; i++)
            {
                Tile tile = registeredTiles[i];
                if (tile != null && tile != caster.CurrentTile && IsPositionInRange(caster, skill, tile.Position))
                {
                    results.Add(tile);
                }
            }

            reason = "Skill range tiles resolved.";
            return true;
        }

        public bool IsPositionInRange(Unit caster, SkillData skill, GridPosition targetPosition)
        {
            if (caster == null || caster.CurrentTile == null || skill == null)
            {
                return false;
            }

            GridPosition casterPosition = caster.CurrentTile.Position;
            return IsPositionInRange(casterPosition, targetPosition, ResolveRange(caster, skill), ResolveRangeShape(caster, skill));
        }

        public SkillRangeShape ResolveRangeShape(Unit caster, SkillData skill)
        {
            if (skill == null)
            {
                return SkillRangeShape.Manhattan;
            }

            if (skill.RangeShape != SkillRangeShape.Automatic)
            {
                return skill.RangeShape;
            }

            string key = NormalizeSkillName(skill.DisplayName);
            SkillRangeShape skillShape;
            if (TryResolveKnownSkillRangeShape(key, out skillShape))
            {
                return skillShape;
            }

            return SkillRangeShape.Manhattan;
        }

        public int ResolveRange(Unit caster, SkillData skill)
        {
            if (skill == null)
            {
                return 0;
            }

            if (skill.RangeShape != SkillRangeShape.Automatic)
            {
                return skill.Range;
            }

            string key = NormalizeSkillName(skill.DisplayName);
            SkillRangeShape ignoredShape;
            int resolvedRange;
            if (TryResolveKnownSkillRangeRule(key, out ignoredShape, out resolvedRange))
            {
                return resolvedRange;
            }

            return skill.Range;
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

        private static bool IsPositionInRange(GridPosition origin, GridPosition target, int range, SkillRangeShape shape)
        {
            int dx = Mathf.Abs(target.X - origin.X);
            int dy = Mathf.Abs(target.Y - origin.Y);
            int clampedRange = Mathf.Max(0, range);

            if (dx == 0 && dy == 0)
            {
                return true;
            }

            switch (shape)
            {
                case SkillRangeShape.Chebyshev:
                    return Mathf.Max(dx, dy) <= clampedRange;
                case SkillRangeShape.ManhattanWithCloseDiagonals:
                    return dx + dy <= clampedRange || (clampedRange >= 1 && dx == 1 && dy == 1);
                case SkillRangeShape.ManhattanWithCardinalTips:
                    return dx + dy <= clampedRange
                        || (clampedRange >= 1 && ((dx == clampedRange + 1 && dy == 0) || (dy == clampedRange + 1 && dx == 0)));
                case SkillRangeShape.Manhattan:
                case SkillRangeShape.Automatic:
                default:
                    return dx + dy <= clampedRange;
            }
        }

        private static bool TryResolveKnownSkillRangeShape(string normalizedSkillName, out SkillRangeShape shape)
        {
            int resolvedRange;
            return TryResolveKnownSkillRangeRule(normalizedSkillName, out shape, out resolvedRange);
        }

        private static bool TryResolveKnownSkillRangeRule(string normalizedSkillName, out SkillRangeShape shape, out int resolvedRange)
        {
            switch (normalizedSkillName)
            {
                case "fightershieldbash":
                    shape = SkillRangeShape.Manhattan;
                    resolvedRange = 1;
                    return true;
                case "fighterguardstrike":
                    shape = SkillRangeShape.ManhattanWithCloseDiagonals;
                    resolvedRange = 1;
                    return true;
                case "fightercrushingchallenge":
                    shape = SkillRangeShape.ManhattanWithCardinalTips;
                    resolvedRange = 2;
                    return true;
                case "rangerprecisionshot":
                    shape = SkillRangeShape.ManhattanWithCardinalTips;
                    resolvedRange = 4;
                    return true;
                case "rangerquickshot":
                    shape = SkillRangeShape.ManhattanWithCloseDiagonals;
                    resolvedRange = 4;
                    return true;
                case "rangerpiercingarrow":
                    shape = SkillRangeShape.Manhattan;
                    resolvedRange = 5;
                    return true;
                case "magefireball":
                    shape = SkillRangeShape.Manhattan;
                    resolvedRange = 3;
                    return true;
                case "magefrostbolt":
                    shape = SkillRangeShape.ManhattanWithCardinalTips;
                    resolvedRange = 4;
                    return true;
                case "magearcaneburst":
                    shape = SkillRangeShape.Manhattan;
                    resolvedRange = 4;
                    return true;
                case "barbarianheavycleave":
                    shape = SkillRangeShape.ManhattanWithCloseDiagonals;
                    resolvedRange = 1;
                    return true;
                case "barbarianragestrike":
                    shape = SkillRangeShape.Manhattan;
                    resolvedRange = 1;
                    return true;
                case "barbarianbloodfuryslash":
                    shape = SkillRangeShape.ManhattanWithCloseDiagonals;
                    resolvedRange = 2;
                    return true;
                default:
                    shape = SkillRangeShape.Manhattan;
                    resolvedRange = 0;
                    return false;
            }
        }

        private static string NormalizeSkillName(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            string lower = value.ToLowerInvariant();
            char[] buffer = new char[lower.Length];
            int count = 0;
            for (int i = 0; i < lower.Length; i++)
            {
                char c = lower[i];
                if ((c >= 'a' && c <= 'z') || (c >= '0' && c <= '9'))
                {
                    buffer[count] = c;
                    count++;
                }
            }

            return new string(buffer, 0, count);
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
