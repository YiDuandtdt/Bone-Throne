using System.Collections.Generic;
using BoneThrone.Grid;
using UnityEngine;

namespace BoneThrone.Movement
{
    /// <summary>
    /// Finds a four-direction A* path on the registered grid.
    /// </summary>
    public sealed class Pathfinder : MonoBehaviour
    {
        private static readonly GridPosition[] Directions =
        {
            new GridPosition(1, 0),
            new GridPosition(-1, 0),
            new GridPosition(0, 1),
            new GridPosition(0, -1)
        };

        public bool TryFindPath(GridManager gridManager, GridPosition start, GridPosition target, out List<GridPosition> path)
        {
            path = new List<GridPosition>();

            if (gridManager == null)
            {
                Debug.LogWarning("Pathfinding failed because GridManager is missing.", this);
                return false;
            }

            if (!gridManager.ContainsPosition(start))
            {
                Debug.LogWarning("Pathfinding failed because the start tile does not exist: " + start + ".", this);
                return false;
            }

            if (!gridManager.CanEnter(target))
            {
                Debug.LogWarning("Pathfinding failed because the target tile cannot be entered: " + target + ".", this);
                return false;
            }

            List<GridPosition> openSet = new List<GridPosition>();
            HashSet<GridPosition> closedSet = new HashSet<GridPosition>();
            Dictionary<GridPosition, GridPosition> cameFrom = new Dictionary<GridPosition, GridPosition>();
            Dictionary<GridPosition, int> gScore = new Dictionary<GridPosition, int>();
            Dictionary<GridPosition, int> fScore = new Dictionary<GridPosition, int>();

            openSet.Add(start);
            gScore[start] = 0;
            fScore[start] = GetManhattanDistance(start, target);

            while (openSet.Count > 0)
            {
                GridPosition current = GetLowestScorePosition(openSet, fScore);
                if (current == target)
                {
                    path = ReconstructPath(cameFrom, current);
                    return path.Count > 0;
                }

                openSet.Remove(current);
                closedSet.Add(current);

                for (int i = 0; i < Directions.Length; i++)
                {
                    GridPosition next = Add(current, Directions[i]);
                    if (closedSet.Contains(next))
                    {
                        continue;
                    }

                    if (next != start && !gridManager.CanEnter(next))
                    {
                        continue;
                    }

                    int tentativeGScore = gScore[current] + 1;
                    int existingGScore;
                    if (gScore.TryGetValue(next, out existingGScore) && tentativeGScore >= existingGScore)
                    {
                        continue;
                    }

                    cameFrom[next] = current;
                    gScore[next] = tentativeGScore;
                    fScore[next] = tentativeGScore + GetManhattanDistance(next, target);

                    if (!openSet.Contains(next))
                    {
                        openSet.Add(next);
                    }
                }
            }

            Debug.LogWarning("Pathfinding failed because no four-direction path exists from " + start + " to " + target + ".", this);
            return false;
        }

        private static GridPosition GetLowestScorePosition(List<GridPosition> positions, Dictionary<GridPosition, int> fScore)
        {
            GridPosition best = positions[0];
            int bestScore = GetScoreOrMax(fScore, best);

            for (int i = 1; i < positions.Count; i++)
            {
                GridPosition candidate = positions[i];
                int candidateScore = GetScoreOrMax(fScore, candidate);
                if (candidateScore < bestScore)
                {
                    best = candidate;
                    bestScore = candidateScore;
                }
            }

            return best;
        }

        private static int GetScoreOrMax(Dictionary<GridPosition, int> scoreByPosition, GridPosition position)
        {
            int score;
            return scoreByPosition.TryGetValue(position, out score) ? score : int.MaxValue;
        }

        private static List<GridPosition> ReconstructPath(Dictionary<GridPosition, GridPosition> cameFrom, GridPosition current)
        {
            List<GridPosition> path = new List<GridPosition>();
            path.Add(current);

            while (cameFrom.ContainsKey(current))
            {
                current = cameFrom[current];
                path.Add(current);
            }

            path.Reverse();
            return path;
        }

        private static int GetManhattanDistance(GridPosition a, GridPosition b)
        {
            return Mathf.Abs(a.X - b.X) + Mathf.Abs(a.Y - b.Y);
        }

        private static GridPosition Add(GridPosition position, GridPosition offset)
        {
            return new GridPosition(position.X + offset.X, position.Y + offset.Y);
        }
    }
}
