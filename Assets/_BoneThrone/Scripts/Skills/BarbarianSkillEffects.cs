using BoneThrone.Combat;
using BoneThrone.Units;

namespace BoneThrone.Skills
{
    /// <summary>
    /// Phase 12 first-pass Barbarian skill effects.
    /// </summary>
    internal static class BarbarianSkillEffects
    {
        public static bool TryExecute(Unit caster, Unit target, SkillData skill, DamageResolver damageResolver, SkillEffectResult result)
        {
            result.Summary = "No Barbarian Phase 12 skill matched.";

            if (!SkillEffectExecutor.SkillNameMatchesAny(skill, "barbarian_heavy_cleave", "Heavy Cleave"))
            {
                return false;
            }

            int damage = skill.GuaranteedDamage + 2;
            if (IsAtOrBelowHalfHp(caster))
            {
                damage++;
            }

            bool targetDied = damageResolver.ApplyDamage(target, damage);
            int remainingHp = target.RuntimeState != null ? target.RuntimeState.CurrentHp : 0;
            result.AddDamage(target, damage, remainingHp, targetDied, true);
            result.Summary = "Barbarian Heavy Cleave dealt guaranteed damage " + damage + ".";
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
