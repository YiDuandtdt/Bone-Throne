using BoneThrone.Units;
using UnityEngine;

namespace BoneThrone.Combat
{
    /// <summary>
    /// Temporary combat feedback sink for Phase 7 manual testing.
    /// This intentionally logs to Console only and does not implement HUD/UI.
    /// </summary>
    public sealed class CombatLog : MonoBehaviour
    {
        public void LogRejected(string reason, Object context)
        {
            Debug.LogWarning("Basic attack rejected: " + reason, context);
        }

        public void LogAttackAttempt(Unit attacker, Unit target, int roll, int attackModifier, int defense)
        {
            Debug.Log(
                "Unit " + attacker.UnitId + " attacks Unit " + target.UnitId
                + ". D20=" + roll
                + " AttackModifier=" + attackModifier
                + " Total=" + (roll + attackModifier)
                + " TargetDefense=" + defense + ".",
                attacker);
        }

        public void LogHit(Unit attacker, Unit target, int damage, int remainingHp)
        {
            Debug.Log(
                "Basic attack hit. Unit " + attacker.UnitId
                + " dealt " + damage
                + " damage to Unit " + target.UnitId
                + ". TargetHP=" + remainingHp + ".",
                target);
        }

        public void LogMiss(Unit attacker, Unit target)
        {
            Debug.Log(
                "Basic attack missed. Unit " + attacker.UnitId
                + " dealt no damage to Unit " + target.UnitId + ".",
                attacker);
        }

        public void LogDeath(Unit target)
        {
            Debug.Log("Unit " + target.UnitId + " died and released its tile.", target);
        }
    }
}
