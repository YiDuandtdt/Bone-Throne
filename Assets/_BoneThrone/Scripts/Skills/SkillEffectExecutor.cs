using BoneThrone.Combat;
using BoneThrone.Core;
using BoneThrone.Units;
using UnityEngine;

namespace BoneThrone.Skills
{
    /// <summary>
    /// Phase 12 role skill effect entry point. Unknown skills keep the Phase 11 damage behavior.
    /// </summary>
    public sealed class SkillEffectExecutor : MonoBehaviour
    {
        [SerializeField] private Unit[] knownUnits;

        public bool TryExecute(Unit caster, Unit target, SkillData skill, DamageResolver damageResolver, out string resultLog)
        {
            resultLog = "Skill effect did not run.";

            if (caster == null || target == null || skill == null || damageResolver == null)
            {
                resultLog = "SkillEffectExecutor missing caster, target, SkillData, or DamageResolver.";
                Debug.LogWarning(resultLog, this);
                return false;
            }

            bool targetDied;
            switch (caster.RoleId)
            {
                case RoleId.Fighter:
                    if (FighterSkillEffects.TryExecute(caster, target, skill, damageResolver, out targetDied, out resultLog))
                    {
                        return targetDied;
                    }

                    break;

                case RoleId.Ranger:
                    if (RangerSkillEffects.TryExecute(caster, target, skill, damageResolver, out targetDied, out resultLog))
                    {
                        return targetDied;
                    }

                    break;

                case RoleId.Mage:
                    if (MageSkillEffects.TryExecute(caster, target, skill, damageResolver, knownUnits, this, out targetDied, out resultLog))
                    {
                        return targetDied;
                    }

                    break;

                case RoleId.Barbarian:
                    if (BarbarianSkillEffects.TryExecute(caster, target, skill, damageResolver, out targetDied, out resultLog))
                    {
                        return targetDied;
                    }

                    break;
            }

            targetDied = ApplyFallbackDamage(target, skill, damageResolver);
            resultLog = "Phase 11 fallback guaranteed damage " + skill.GuaranteedDamage + ".";
            return targetDied;
        }

        internal static bool SkillNameMatches(SkillData skill, string expectedName)
        {
            if (skill == null)
            {
                return false;
            }

            return NormalizeSkillName(skill.DisplayName) == NormalizeSkillName(expectedName);
        }

        internal static bool SkillNameMatchesAny(SkillData skill, string firstName, string secondName)
        {
            return SkillNameMatches(skill, firstName) || SkillNameMatches(skill, secondName);
        }

        internal static int GetManhattanDistance(Unit first, Unit second)
        {
            if (first == null || second == null || first.CurrentTile == null || second.CurrentTile == null)
            {
                return int.MaxValue;
            }

            int dx = Mathf.Abs(first.CurrentTile.Position.X - second.CurrentTile.Position.X);
            int dy = Mathf.Abs(first.CurrentTile.Position.Y - second.CurrentTile.Position.Y);
            return dx + dy;
        }

        private static bool ApplyFallbackDamage(Unit target, SkillData skill, DamageResolver damageResolver)
        {
            return damageResolver.ApplyDamage(target, skill.GuaranteedDamage);
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
    }
}
