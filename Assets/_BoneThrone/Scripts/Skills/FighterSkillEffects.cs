using BoneThrone.Combat;
using BoneThrone.Units;

namespace BoneThrone.Skills
{
    /// <summary>
    /// Phase 12 first-pass Fighter skill effects.
    /// </summary>
    internal static class FighterSkillEffects
    {
        public static bool TryExecute(Unit caster, Unit target, SkillData skill, DamageResolver damageResolver, out bool targetDied, out string resultLog)
        {
            targetDied = false;
            resultLog = "No Fighter Phase 12 skill matched.";

            if (!SkillEffectExecutor.SkillNameMatchesAny(skill, "fighter_shield_bash", "Shield Bash"))
            {
                return false;
            }

            int damage = skill.GuaranteedDamage + 1;
            targetDied = damageResolver.ApplyDamage(target, damage);
            resultLog = "Fighter Shield Bash dealt guaranteed damage " + damage + ".";
            return true;
        }
    }
}
