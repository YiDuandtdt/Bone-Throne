using BoneThrone.Combat;
using BoneThrone.Units;

namespace BoneThrone.Skills
{
    /// <summary>
    /// Phase 12 first-pass Fighter skill effects.
    /// </summary>
    internal static class FighterSkillEffects
    {
        public static bool TryExecute(Unit caster, Unit target, SkillData skill, DamageResolver damageResolver, SkillEffectResult result)
        {
            result.Summary = "No Fighter Phase 12 skill matched.";

            if (!SkillEffectExecutor.SkillNameMatchesAny(skill, "fighter_shield_bash", "Shield Bash"))
            {
                return false;
            }

            int damage = skill.GuaranteedDamage + 1;
            bool targetDied = damageResolver.ApplyDamage(target, damage);
            int remainingHp = target.RuntimeState != null ? target.RuntimeState.CurrentHp : 0;
            result.AddDamage(target, damage, remainingHp, targetDied, true);
            result.Summary = "Fighter Shield Bash dealt guaranteed damage " + damage + ".";
            return true;
        }
    }
}
