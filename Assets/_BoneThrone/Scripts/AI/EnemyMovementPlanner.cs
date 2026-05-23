using System.Collections.Generic;
using BoneThrone.Grid;
using BoneThrone.Movement;
using BoneThrone.Units;
using UnityEngine;

namespace BoneThrone.AI
{
    /// <summary>
    /// Plans a one-turn enemy move toward a target by reusing the Phase 5 Pathfinder.
    /// It only chooses among the target's four enterable neighbor tiles.
    /// </summary>
    public sealed class EnemyMovementPlanner
    {
        private static readonly GridPosition[] NeighborOffsets =
        {
            new GridPosition(1, 0),
            new GridPosition(-1, 0),
            new GridPosition(0, 1),
            new GridPosition(0, -1)
        };

        public bool TryBuildMoveTowardTarget(Unit enemy, Unit target, GridManager gridManager, Pathfinder pathfinder, out List<GridPosition> movementPath)
        {
            movementPath = null;

            if (enemy == null || target == null || gridManager == null || pathfinder == null)
            {
                return false;
            }

            if (enemy.CurrentTile == null || target.CurrentTile == null)
            {
                return false;
            }

            int moveRange = enemy.Stats != null ? enemy.Stats.MoveRange : 0;
            if (moveRange <= 0)
            {
                return false;
            }

            List<GridPosition> bestMovementPath = null;
            int bestMoveEndDistance = int.MaxValue;
            int bestFullPathCount = int.MaxValue;
            GridPosition start = enemy.CurrentTile.Position;
            GridPosition targetPosition = target.CurrentTile.Position;

            for (int i = 0; i < NeighborOffsets.Length; i++)
            {
                GridPosition candidate = Add(targetPosition, NeighborOffsets[i]);
                if (!gridManager.CanEnter(candidate))
                {
                    continue;
                }

                List<GridPosition> candidatePath;
                if (!pathfinder.TryFindPath(gridManager, start, candidate, out candidatePath))
                {
                    continue;
                }

                if (candidatePath == null || candidatePath.Count < 2)
                {
                    continue;
                }

                List<GridPosition> clampedPath = ClampPathToMoveRange(candidatePath, moveRange);
                if (clampedPath == null || clampedPath.Count < 2)
                {
                    continue;
                }

                GridPosition moveEnd = clampedPath[clampedPath.Count - 1];
                int moveEndDistance = GetManhattanDistance(moveEnd, targetPosition);
                if (moveEndDistance < bestMoveEndDistance
                    || (moveEndDistance == bestMoveEndDistance && candidatePath.Count < bestFullPathCount))
                {
                    bestMovementPath = clampedPath;
                    bestMoveEndDistance = moveEndDistance;
                    bestFullPathCount = candidatePath.Count;
                }
            }

            if (bestMovementPath == null)
            {
                return false;
            }

            movementPath = bestMovementPath;
            return true;
        }

        private static List<GridPosition> ClampPathToMoveRange(IReadOnlyList<GridPosition> fullPath, int moveRange)
        {
            if (fullPath == null || fullPath.Count < 2 || moveRange <= 0)
            {
                return null;
            }

            int lastIndex = Mathf.Min(moveRange, fullPath.Count - 1);
            List<GridPosition> clampedPath = new List<GridPosition>();
            for (int i = 0; i <= lastIndex; i++)
            {
                clampedPath.Add(fullPath[i]);
            }

            return clampedPath;
        }

        private static GridPosition Add(GridPosition position, GridPosition offset)
        {
            return new GridPosition(position.X + offset.X, position.Y + offset.Y);
        }

        private static int GetManhattanDistance(GridPosition a, GridPosition b)
        {
            return Mathf.Abs(a.X - b.X) + Mathf.Abs(a.Y - b.Y);
        }
    }
}
