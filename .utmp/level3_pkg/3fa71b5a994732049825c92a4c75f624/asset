using BoneThrone.Grid;
using BoneThrone.Units;
using UnityEngine;

namespace BoneThrone.Combat
{
    /// <summary>
    /// Computes only Phase 7 basic attack range using four-direction Manhattan distance.
    /// It does not implement skills, AOE, line of sight, obstacle checks, or diagonal attacks.
    /// </summary>
    public sealed class AttackRangeService : MonoBehaviour
    {
        [SerializeField] private int basicAttackRange = 1;

        public int BasicAttackRange
        {
            get { return Mathf.Max(1, basicAttackRange); }
        }

        public int GetBasicAttackRange(Unit attacker)
        {
            if (attacker == null || attacker.Stats == null)
            {
                return BasicAttackRange;
            }

            return attacker.Stats.BasicAttackRange;
        }

        public bool IsInBasicAttackRange(Unit attacker, Unit target)
        {
            return GetManhattanDistance(attacker, target) <= GetBasicAttackRange(attacker);
        }

        public int GetManhattanDistance(Unit attacker, Unit target)
        {
            if (attacker == null || target == null || attacker.CurrentTile == null || target.CurrentTile == null)
            {
                return int.MaxValue;
            }

            GridPosition attackerPosition = attacker.CurrentTile.Position;
            GridPosition targetPosition = target.CurrentTile.Position;
            return Mathf.Abs(attackerPosition.X - targetPosition.X) + Mathf.Abs(attackerPosition.Y - targetPosition.Y);
        }
    }
}
