using BoneThrone.Grid;
using BoneThrone.Units;
using UnityEngine;

namespace BoneThrone.Rooms
{
    /// <summary>
    /// Activates pre-placed enemies for one room and handles their Tile occupancy.
    /// This does not instantiate waves, randomize spawns, or read enemy data.
    /// </summary>
    public sealed class RoomEnemyActivator : MonoBehaviour
    {
        [SerializeField] private Unit[] assignedEnemies;
        [SerializeField] private Tile[] spawnTiles;
        [SerializeField] private bool deactivateEnemiesAtStart = true;

        public Unit[] AssignedEnemies
        {
            get { return assignedEnemies; }
        }

        private void Start()
        {
            if (deactivateEnemiesAtStart)
            {
                DeactivateEnemiesAtStart();
            }
        }

        public void DeactivateEnemiesAtStart()
        {
            if (assignedEnemies == null)
            {
                return;
            }

            for (int i = 0; i < assignedEnemies.Length; i++)
            {
                Unit enemy = assignedEnemies[i];
                if (enemy == null)
                {
                    continue;
                }

                if (enemy.CurrentTile != null)
                {
                    enemy.ReleaseTile();
                }

                enemy.gameObject.SetActive(false);
            }
        }

        public int ActivateEnemies()
        {
            int aliveEnemyCount = 0;

            if (assignedEnemies == null)
            {
                return aliveEnemyCount;
            }

            for (int i = 0; i < assignedEnemies.Length; i++)
            {
                Unit enemy = assignedEnemies[i];
                if (enemy == null)
                {
                    continue;
                }

                if (!TryActivateEnemy(enemy, i))
                {
                    continue;
                }

                if (enemy.IsAlive)
                {
                    aliveEnemyCount++;
                }
            }

            return aliveEnemyCount;
        }

        public bool HasAliveAssignedEnemy()
        {
            if (assignedEnemies == null)
            {
                return false;
            }

            for (int i = 0; i < assignedEnemies.Length; i++)
            {
                Unit enemy = assignedEnemies[i];
                if (enemy != null && enemy.gameObject.activeInHierarchy && enemy.IsAlive)
                {
                    return true;
                }
            }

            return false;
        }

        public bool AreAllAssignedEnemiesDead()
        {
            return !HasAliveAssignedEnemy();
        }

        public void MarkAssignedEnemiesDeadForTest()
        {
            if (assignedEnemies == null)
            {
                return;
            }

            for (int i = 0; i < assignedEnemies.Length; i++)
            {
                Unit enemy = assignedEnemies[i];
                if (enemy == null)
                {
                    continue;
                }

                enemy.MarkDeadAndReleaseTile();
            }
        }

        private bool TryActivateEnemy(Unit enemy, int index)
        {
            enemy.gameObject.SetActive(true);

            if (enemy.CurrentTile != null)
            {
                return true;
            }

            Tile spawnTile = GetSpawnTile(index);
            if (spawnTile == null)
            {
                Debug.LogWarning("RoomEnemyActivator skipped enemy " + enemy.UnitId + " because no matching spawn Tile is assigned.", enemy);
                enemy.gameObject.SetActive(false);
                return false;
            }

            if (!spawnTile.CanEnter())
            {
                Debug.LogWarning("RoomEnemyActivator skipped enemy " + enemy.UnitId + " because spawn Tile " + spawnTile.Position + " cannot be entered.", spawnTile);
                enemy.gameObject.SetActive(false);
                return false;
            }

            if (!enemy.TryPlaceOnTile(spawnTile))
            {
                Debug.LogWarning("RoomEnemyActivator skipped enemy " + enemy.UnitId + " because Unit.TryPlaceOnTile rejected the spawn Tile.", enemy);
                enemy.gameObject.SetActive(false);
                return false;
            }

            enemy.transform.position = spawnTile.transform.position;
            return true;
        }

        private Tile GetSpawnTile(int index)
        {
            if (spawnTiles == null || index < 0 || index >= spawnTiles.Length)
            {
                return null;
            }

            return spawnTiles[index];
        }
    }
}
