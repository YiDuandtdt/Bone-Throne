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
        [SerializeField] private ActiveUnitProvider activeUnitProvider;

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
                    if (MageSkillEffects.TryExecute(caster, target, skill, damageResolver, GetKnownUnitsForSkillEffects(), this, result))
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

            if (TryExecutePhase1413Skill(caster, target, skill, damageResolver, result))
            {
                return result.PrimaryTargetDied;
            }

            ApplyFallbackDamage(target, skill, damageResolver, result);
            result.Summary = "Phase 11 fallback guaranteed damage " + skill.GuaranteedDamage + ".";
            return result.PrimaryTargetDied;
        }

        private bool TryExecutePhase1413Skill(Unit caster, Unit target, SkillData skill, DamageResolver damageResolver, SkillEffectResult result)
        {
            switch (caster.RoleId)
            {
                case RoleId.Fighter:
                    return TryExecutePhase1413FighterSkill(target, skill, damageResolver, result);
                case RoleId.Ranger:
                    return TryExecutePhase1413RangerSkill(target, skill, damageResolver, result);
                case RoleId.Mage:
                    return TryExecutePhase1413MageSkill(caster, target, skill, damageResolver, result);
                case RoleId.Barbarian:
                    return TryExecutePhase1413BarbarianSkill(caster, target, skill, damageResolver, result);
                default:
                    return false;
            }
        }

        private static bool TryExecutePhase1413FighterSkill(Unit target, SkillData skill, DamageResolver damageResolver, SkillEffectResult result)
        {
            if (SkillNameMatches(skill, "fighter_guard_strike"))
            {
                int damage = skill.GuaranteedDamage + 1;
                ApplySingleTargetDamage(target, damage, damageResolver, result);
                result.Summary = "Fighter Guard Strike dealt guaranteed damage " + damage + ".";
                return true;
            }

            if (SkillNameMatches(skill, "fighter_crushing_challenge"))
            {
                int damage = skill.GuaranteedDamage + 2;
                ApplySingleTargetDamage(target, damage, damageResolver, result);
                result.Summary = "Fighter Crushing Challenge dealt guaranteed damage " + damage + ".";
                return true;
            }

            return false;
        }

        private static bool TryExecutePhase1413RangerSkill(Unit target, SkillData skill, DamageResolver damageResolver, SkillEffectResult result)
        {
            if (SkillNameMatches(skill, "ranger_quick_shot"))
            {
                int damage = skill.GuaranteedDamage;
                ApplySingleTargetDamage(target, damage, damageResolver, result);
                result.Summary = "Ranger Quick Shot dealt guaranteed damage " + damage + ".";
                return true;
            }

            if (SkillNameMatches(skill, "ranger_piercing_arrow"))
            {
                int damage = skill.GuaranteedDamage + 1;
                ApplySingleTargetDamage(target, damage, damageResolver, result);
                result.Summary = "Ranger Piercing Arrow dealt guaranteed damage " + damage + ".";
                return true;
            }

            return false;
        }

        private bool TryExecutePhase1413MageSkill(Unit caster, Unit target, SkillData skill, DamageResolver damageResolver, SkillEffectResult result)
        {
            if (SkillNameMatches(skill, "mage_frost_bolt"))
            {
                int damage = skill.GuaranteedDamage;
                ApplySingleTargetDamage(target, damage, damageResolver, result);
                result.Summary = "Mage Frost Bolt dealt guaranteed damage " + damage + ".";
                return true;
            }

            if (SkillNameMatches(skill, "mage_arcane_burst"))
            {
                int damage = skill.GuaranteedDamage;
                ApplySingleTargetDamage(target, damage, damageResolver, result);
                int splashHits = ApplyArcaneBurstSplash(caster, target, damageResolver, GetKnownUnitsForSkillEffects(), result);
                result.Summary = "Mage Arcane Burst dealt guaranteed damage " + damage + " and splash hits " + splashHits + ".";
                return true;
            }

            return false;
        }

        private static bool TryExecutePhase1413BarbarianSkill(Unit caster, Unit target, SkillData skill, DamageResolver damageResolver, SkillEffectResult result)
        {
            if (SkillNameMatches(skill, "barbarian_rage_strike"))
            {
                int damage = skill.GuaranteedDamage + 1;
                ApplySingleTargetDamage(target, damage, damageResolver, result);
                result.Summary = "Barbarian Rage Strike dealt guaranteed damage " + damage + ".";
                return true;
            }

            if (SkillNameMatches(skill, "barbarian_blood_fury_slash"))
            {
                int damage = skill.GuaranteedDamage + 2;
                if (IsAtOrBelowHalfHp(caster))
                {
                    damage++;
                }

                ApplySingleTargetDamage(target, damage, damageResolver, result);
                result.Summary = "Barbarian Blood Fury Slash dealt guaranteed damage " + damage + ".";
                return true;
            }

            return false;
        }

        private Unit[] GetKnownUnitsForSkillEffects()
        {
            ActiveUnitProvider provider = ResolveActiveUnitProvider();
            Unit[] activeUnits = provider != null ? provider.GetActiveAliveUnits() : null;
            if (activeUnits != null && activeUnits.Length > 0)
            {
                return activeUnits;
            }

            return knownUnits;
        }

        private ActiveUnitProvider ResolveActiveUnitProvider()
        {
            if (activeUnitProvider == null)
            {
                activeUnitProvider = Object.FindFirstObjectByType<ActiveUnitProvider>();
            }

            return activeUnitProvider;
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

        private static void ApplySingleTargetDamage(Unit target, int damage, DamageResolver damageResolver, SkillEffectResult result)
        {
            bool targetDied = damageResolver.ApplyDamage(target, damage);
            int remainingHp = target.RuntimeState != null ? target.RuntimeState.CurrentHp : 0;
            result.AddDamage(target, damage, remainingHp, targetDied, true);
        }

        private static int ApplyArcaneBurstSplash(Unit caster, Unit target, DamageResolver damageResolver, Unit[] knownUnits, SkillEffectResult result)
        {
            if (knownUnits == null || knownUnits.Length == 0)
            {
                return 0;
            }

            int splashHits = 0;
            for (int i = 0; i < knownUnits.Length; i++)
            {
                Unit candidate = knownUnits[i];
                if (!IsValidArcaneBurstSplashTarget(caster, target, candidate))
                {
                    continue;
                }

                if (GetManhattanDistance(target, candidate) == 1)
                {
                    bool targetDied = damageResolver.ApplyDamage(candidate, 1);
                    int remainingHp = candidate.RuntimeState != null ? candidate.RuntimeState.CurrentHp : 0;
                    result.AddDamage(candidate, 1, remainingHp, targetDied, false);
                    splashHits++;
                }
            }

            return splashHits;
        }

        private static bool IsValidArcaneBurstSplashTarget(Unit caster, Unit primaryTarget, Unit candidate)
        {
            return caster != null
                && primaryTarget != null
                && candidate != null
                && candidate != primaryTarget
                && candidate.gameObject.activeInHierarchy
                && candidate.IsAlive
                && candidate.Faction != caster.Faction
                && candidate.CurrentTile != null;
        }

        private static bool IsAtOrBelowHalfHp(Unit caster)
        {
            if (caster == null || caster.Stats == null || caster.RuntimeState == null)
            {
                return false;
            }

            int maxHp = caster.Stats.GetClampedMaxHp();
            return caster.RuntimeState.CurrentHp * 2 <= maxHp;
        }

        private static void ApplyFallbackDamage(Unit target, SkillData skill, DamageResolver damageResolver, SkillEffectResult result)
        {
            ApplySingleTargetDamage(target, skill.GuaranteedDamage, damageResolver, result);
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
