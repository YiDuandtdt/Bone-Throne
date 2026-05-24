using BoneThrone.Combat;
using BoneThrone.Units;

namespace BoneThrone.Skills
{
    /// <summary>
    /// Phase 12 first-pass Ranger skill effects.
    /// </summary>
    internal static class RangerSkillEffects
    {
        public static bool TryExecute(Unit caster, Unit target, SkillData skill, DamageResolver damageResolver, out bool targetDied, out string resultLog)
        {
            targetDied = false;
            resultLog = "No Ranger Phase 12 skill matched.";

            if (!SkillEffectExecutor.SkillNameMatchesAny(skill, "ranger_precision_shot", "Precision Shot"))
            {
                return false;
            }

            int damage = skill.GuaranteedDamage + 2;
            targetDied = damageResolver.ApplyDamage(target, damage);
            resultLog = "Ranger Precision Shot dealt guaranteed damage " + damage + ".";
            return true;
        }
    }
}
