using BoneThrone.Grid;
using BoneThrone.Units;
using UnityEngine;

namespace BoneThrone.Levels
{
    /// <summary>
    /// Minimal Phase 10 floor switch placeholder.
    /// It does not load scenes, manage rooms, sync networking, or own progression rules.
    /// </summary>
    public sealed class LevelManager : MonoBehaviour
    {
        [SerializeField] private int currentLevelIndex;
        [SerializeField] private GameObject[] levelRoots;
        [SerializeField] private Transform[] spawnPoints;

        public int CurrentLevelIndex
        {
            get { return currentLevelIndex; }
        }

        public bool SwitchToNextLevelPlaceholder(Unit[] partyUnits)
        {
            int nextLevelIndex = currentLevelIndex + 1;

            if (levelRoots == null || levelRoots.Length == 0)
            {
                currentLevelIndex = nextLevelIndex;
                Debug.Log("LevelManager advanced to placeholder level index " + currentLevelIndex + " because no levelRoots are configured.", this);
                return true;
            }

            if (spawnPoints == null || spawnPoints.Length == 0)
            {
                currentLevelIndex = nextLevelIndex;
                Debug.Log("LevelManager advanced to placeholder level index " + currentLevelIndex + " because no spawnPoints are configured.", this);
                return true;
            }

            if (nextLevelIndex >= levelRoots.Length)
            {
                Debug.LogWarning("LevelManager cannot switch to next level because next index " + nextLevelIndex + " is outside levelRoots length " + levelRoots.Length + ".", this);
                return false;
            }

            SetActiveLevel(nextLevelIndex);
            currentLevelIndex = nextLevelIndex;
            MovePartyToCurrentLevelSpawn(partyUnits);
            Debug.Log("LevelManager switched to placeholder level index " + currentLevelIndex + ".", this);
            return true;
        }

        public void SetActiveLevel(int levelIndex)
        {
            if (levelRoots == null || levelRoots.Length == 0)
            {
                currentLevelIndex = Mathf.Max(0, levelIndex);
                Debug.Log("LevelManager set current level index to " + currentLevelIndex + " without levelRoots.", this);
                return;
            }

            if (levelIndex < 0 || levelIndex >= levelRoots.Length)
            {
                Debug.LogWarning("LevelManager cannot set active level " + levelIndex + " because it is outside levelRoots length " + levelRoots.Length + ".", this);
                return;
            }

            for (int i = 0; i < levelRoots.Length; i++)
            {
                if (levelRoots[i] != null)
                {
                    levelRoots[i].SetActive(i == levelIndex);
                }
            }

            currentLevelIndex = levelIndex;
        }

        public void MovePartyToCurrentLevelSpawn(Unit[] partyUnits)
        {
            if (partyUnits == null || partyUnits.Length == 0)
            {
                Debug.LogWarning("LevelManager cannot move party because no party units are configured.", this);
                return;
            }

            if (spawnPoints == null || spawnPoints.Length == 0)
            {
                Debug.Log("LevelManager did not move party because no spawnPoints are configured.", this);
                return;
            }

            int movedCount = 0;
            for (int i = 0; i < partyUnits.Length; i++)
            {
                Unit unit = partyUnits[i];
                if (unit == null || !unit.IsAlive)
                {
                    continue;
                }

                Transform spawnPoint = spawnPoints[Mathf.Min(i, spawnPoints.Length - 1)];
                if (spawnPoint == null)
                {
                    Debug.LogWarning("LevelManager skipped unit " + unit.UnitId + " because its spawn point is missing.", unit);
                    continue;
                }

                Tile spawnTile = spawnPoint.GetComponentInParent<Tile>();
                if (spawnTile != null)
                {
                    if (!unit.TryPlaceOnTile(spawnTile))
                    {
                        Debug.LogWarning("LevelManager could not place unit " + unit.UnitId + " on spawn tile. Transform move skipped for this unit.", unit);
                        continue;
                    }
                }
                else
                {
                    unit.ReleaseTile();
                }

                unit.transform.position = spawnPoint.position;
                unit.transform.rotation = spawnPoint.rotation;
                movedCount++;
            }

            Debug.Log("LevelManager moved " + movedCount + " living party units to placeholder spawn points.", this);
        }
    }
}
