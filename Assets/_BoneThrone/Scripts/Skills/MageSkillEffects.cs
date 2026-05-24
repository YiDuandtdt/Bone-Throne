using BoneThrone.Combat;
using BoneThrone.Units;
using UnityEngine;

namespace BoneThrone.Skills
{
    /// <summary>
    /// Phase 12 first-pass Mage skill effects.
    /// </summary>
    internal static class MageSkillEffects
    {
        public static bool TryExecute(
            Unit caster,
            Unit target,
            SkillData skill,
            DamageResolver damageResolver,
            Unit[] knownUnits,
            Object logContext,
            out bool targetDied,
            out string resultLog)
        {
            targetDied = false;
            resultLog = "No Mage Phase 12 skill matched.";

            if (!SkillEffectExecutor.SkillNameMatchesAny(skill, "mage_fireball", "Fireball"))
            {
                return false;
            }

            targetDied = damageResolver.ApplyDamage(target, skill.GuaranteedDamage);
            int splashHits = ApplySplashDamage(caster, target, damageResolver, knownUnits, logContext);
            resultLog = "Mage Fireball dealt guaranteed damage " + skill.GuaranteedDamage + " and splash hits " + splashHits + ".";
            return true;
        }

        private static int ApplySplashDamage(Unit caster, Unit target, DamageResolver damageResolver, Unit[] knownUnits, Object logContext)
        {
            if (knownUnits == null || knownUnits.Length == 0)
            {
                Debug.LogWarning("Mage Fireball has no knownUnits bound, so only the primary target was damaged.", logContext);
                return 0;
            }

            int splashHits = 0;
            for (int i = 0; i < knownUnits.Length; i++)
            {
                Unit candidate = knownUnits[i];
                string rejectionReason;
                bool isValidCandidate = IsValidSplashTarget(caster, target, candidate, out rejectionReason);
                int distanceToMainTarget = candidate != null
                    ? SkillEffectExecutor.GetManhattanDistance(target, candidate)
                    : int.MaxValue;

                if (!isValidCandidate)
                {
                    continue;
                }

                if (distanceToMainTarget == 1)
                {
                    damageResolver.ApplyDamage(candidate, 1);
                    splashHits++;
                }
            }

            return splashHits;
        }

        private static bool IsValidSplashTarget(Unit caster, Unit primaryTarget, Unit candidate, out string rejectionReason)
        {
            if (caster == null || primaryTarget == null || candidate == null)
            {
                rejectionReason = "caster, primary target, or candidate is missing";
                return false;
            }

            if (candidate == primaryTarget)
            {
                rejectionReason = "candidate is the main target";
                return false;
            }

            if (!candidate.IsAlive)
            {
                rejectionReason = "candidate is not alive";
                return false;
            }

            if (candidate.Faction == caster.Faction)
            {
                rejectionReason = "candidate has the same faction as caster";
                return false;
            }

            if (candidate.CurrentTile == null)
            {
                rejectionReason = "candidate CurrentTile is null";
                return false;
            }

            rejectionReason = "none";
            return true;
        }
    }
}
