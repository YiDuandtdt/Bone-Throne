using BoneThrone.Combat;
using BoneThrone.Units;

namespace BoneThrone.Skills
{
    /// <summary>
    /// Phase 12 first-pass Ranger skill effects.
    /// </summary>
    internal static class RangerSkillEffects
    {
        public static bool TryExecute(Unit caster, Unit target, SkillData skill, DamageResolver damageResolver, SkillEffectResult result)
        {
            result.Summary = "No Ranger Phase 12 skill matched.";

            if (!SkillEffectExecutor.SkillNameMatchesAny(skill, "ranger_precision_shot", "Precision Shot"))
            {
                return false;
            }

            int damage = skill.GuaranteedDamage + 2;
            bool targetDied = damageResolver.ApplyDamage(target, damage);
            int remainingHp = target.RuntimeState != null ? target.RuntimeState.CurrentHp : 0;
            result.AddDamage(target, damage, remainingHp, targetDied, true);
            result.Summary = "Ranger Precision Shot dealt guaranteed damage " + damage + ".";
            return true;
        }
    }
}
