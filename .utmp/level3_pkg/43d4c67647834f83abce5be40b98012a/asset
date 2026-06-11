using System.Collections.Generic;
using BoneThrone.Grid;
using UnityEngine;

namespace BoneThrone.Movement
{
    /// <summary>
    /// Calculates reachable grid positions with four-direction BFS. It does not move units.
    /// </summary>
    public sealed class MovementRangeFinder : MonoBehaviour
    {
        private static readonly GridPosition[] Directions =
        {
            new GridPosition(1, 0),
            new GridPosition(-1, 0),
            new GridPosition(0, 1),
            new GridPosition(0, -1)
        };

        public HashSet<GridPosition> FindReachablePositions(GridManager gridManager, GridPosition start, int moveRange)
        {
            HashSet<GridPosition> reachable = new HashSet<GridPosition>();

            if (gridManager == null)
            {
                Debug.LogWarning("Movement range calculation failed because GridManager is missing.", this);
                return reachable;
            }

            if (!gridManager.ContainsPosition(start))
            {
                Debug.LogWarning("Movement range calculation failed because the start tile does not exist: " + start + ".", this);
                return reachable;
            }

            if (moveRange <= 0)
            {
                return reachable;
            }

            Queue<GridPosition> frontier = new Queue<GridPosition>();
            Dictionary<GridPosition, int> distanceByPosition = new Dictionary<GridPosition, int>();

            frontier.Enqueue(start);
            distanceByPosition.Add(start, 0);

            while (frontier.Count > 0)
            {
                GridPosition current = frontier.Dequeue();
                int currentDistance = distanceByPosition[current];

                if (currentDistance >= moveRange)
                {
                    continue;
                }

                for (int i = 0; i < Directions.Length; i++)
                {
                    GridPosition next = Add(current, Directions[i]);
                    if (distanceByPosition.ContainsKey(next))
                    {
                        continue;
                    }

                    if (!gridManager.CanEnter(next))
                    {
                        continue;
                    }

                    distanceByPosition.Add(next, currentDistance + 1);
                    reachable.Add(next);
                    frontier.Enqueue(next);
                }
            }

            return reachable;
        }

        private static GridPosition Add(GridPosition position, GridPosition offset)
        {
            return new GridPosition(position.X + offset.X, position.Y + offset.Y);
        }
    }
}
