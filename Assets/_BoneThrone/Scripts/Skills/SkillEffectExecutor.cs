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
            SkillEffectResult result;
            bool targetDied = TryExecute(caster, target, skill, damageResolver, out result);
            resultLog = result != null ? result.Summary : "Skill effect did not run.";
            return targetDied;
        }

        public bool TryExecute(Unit caster, Unit target, SkillData skill, DamageResolver damageResolver, out SkillEffectResult result)
        {
            result = new SkillEffectResult { Summary = "Skill effect did not run." };

            if (caster == null || target == null || skill == null || damageResolver == null)
            {
                result.Summary = "SkillEffectExecutor missing caster, target, SkillData, or DamageResolver.";
                Debug.LogWarning(result.Summary, this);
                return false;
            }

            switch (caster.RoleId)
            {
                case RoleId.Fighter:
                    if (FighterSkillEffects.TryExecute(caster, target, skill, damageResolver, result))
                    {
                        return result.PrimaryTargetDied;
                    }

                    break;

                case RoleId.Ranger:
                    if (RangerSkillEffects.TryExecute(caster, target, skill, damageResolver, result))
                    {
                        return result.PrimaryTargetDied;
                    }

                    break;

                case RoleId.Mage:
                    if (MageSkillEffects.TryExecute(caster, target, skill, damageResolver, knownUnits, this, result))
                    {
                        return result.PrimaryTargetDied;
                    }

                    break;

                case RoleId.Barbarian:
                    if (BarbarianSkillEffects.TryExecute(caster, target, skill, damageResolver, result))
                    {
                        return result.PrimaryTargetDied;
                    }

                    break;
            }

            ApplyFallbackDamage(target, skill, damageResolver, result);
            result.Summary = "Phase 11 fallback guaranteed damage " + skill.GuaranteedDamage + ".";
            return result.PrimaryTargetDied;
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

        private static void ApplyFallbackDamage(Unit target, SkillData skill, DamageResolver damageResolver, SkillEffectResult result)
        {
            bool targetDied = damageResolver.ApplyDamage(target, skill.GuaranteedDamage);
            int remainingHp = target.RuntimeState != null ? target.RuntimeState.CurrentHp : 0;
            result.AddDamage(target, skill.GuaranteedDamage, remainingHp, targetDied, true);
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
