using BoneThrone.Combat;
using BoneThrone.Units;

namespace BoneThrone.Skills
{
    /// <summary>
    /// Phase 12 first-pass Barbarian skill effects.
    /// </summary>
    internal static class BarbarianSkillEffects
    {
        public static bool TryExecute(Unit caster, Unit target, SkillData skill, DamageResolver damageResolver, out bool targetDied, out string resultLog)
        {
            targetDied = false;
            resultLog = "No Barbarian Phase 12 skill matched.";

            if (!SkillEffectExecutor.SkillNameMatchesAny(skill, "barbarian_heavy_cleave", "Heavy Cleave"))
            {
                return false;
            }

            int damage = skill.GuaranteedDamage + 2;
            if (IsAtOrBelowHalfHp(caster))
            {
                damage++;
            }

            targetDied = damageResolver.ApplyDamage(target, damage);
            resultLog = "Barbarian Heavy Cleave dealt guaranteed damage " + damage + ".";
            return true;
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
    }
}
