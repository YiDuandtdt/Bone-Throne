using System.Collections.Generic;
using BoneThrone.Units;
using UnityEngine;

namespace BoneThrone.AI
{
    /// <summary>
    /// Selects the nearest alive player Unit by four-direction Manhattan distance.
    /// It does not implement threat tables, health weighting, role weighting, or aggro memory.
    /// </summary>
    public sealed class EnemyTargetSelector
    {
        public bool TrySelectNearestAlivePlayer(Unit enemy, IReadOnlyList<Unit> candidates, out Unit target)
        {
            target = null;

            if (enemy == null || enemy.CurrentTile == null || candidates == null)
            {
                return false;
            }

            int bestDistance = int.MaxValue;
            int bestUnitId = int.MaxValue;

            for (int i = 0; i < candidates.Count; i++)
            {
                Unit candidate = candidates[i];
                if (!IsValidPlayerTarget(candidate))
                {
                    continue;
                }

                int distance = GetManhattanDistance(enemy, candidate);
                int unitId = candidate.UnitId;
                if (distance < bestDistance || (distance == bestDistance && unitId < bestUnitId))
                {
                    bestDistance = distance;
                    bestUnitId = unitId;
                    target = candidate;
                }
            }

            return target != null;
        }

        private static bool IsValidPlayerTarget(Unit unit)
        {
            return unit != null
                && unit.Faction == UnitFaction.Player
                && unit.IsAlive
                && unit.CurrentTile != null;
        }

        private static int GetManhattanDistance(Unit enemy, Unit target)
        {
            int dx = Mathf.Abs(enemy.CurrentTile.Position.X - target.CurrentTile.Position.X);
            int dy = Mathf.Abs(enemy.CurrentTile.Position.Y - target.CurrentTile.Position.Y);
            return dx + dy;
        }
    }
}
