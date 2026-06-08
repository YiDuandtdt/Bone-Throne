using BoneThrone.Grid;
using BoneThrone.Units;
using UnityEngine;

namespace BoneThrone.Rooms
{
    /// <summary>
    /// Spawns configured enemy prefabs for one room and handles their Tile occupancy.
    /// Legacy pre-placed enemies are still supported as a fallback for older test scenes.
    /// </summary>
    public sealed class RoomEnemyActivator : MonoBehaviour
    {
        [SerializeField] private Unit[] enemyPrefabs;
        [SerializeField] private Unit[] assignedEnemies;
        [SerializeField] private Tile[] spawnTiles;
        [SerializeField] private Transform spawnedEnemyParent;
        [SerializeField] private bool deactivateEnemiesAtStart = true;
        [Header("Scene-Deployed Boss")]
        [SerializeField] private bool keepBossLikeAssignedEnemiesActiveAtStart = true;
        [SerializeField] private string bossNameContains = "boss";
        [SerializeField] private string golemNameContains = "golem";

        private Unit[] spawnedEnemies;
        private bool hasSpawnedEnemies;

        public Unit[] AssignedEnemies
        {
            get
            {
                if (hasSpawnedEnemies && spawnedEnemies != null)
                {
                    return spawnedEnemies;
                }

                return assignedEnemies;
            }
        }

        public Unit[] EnemyPrefabs
        {
            get { return enemyPrefabs; }
        }

        public bool HasConfiguredEnemies
        {
            get
            {
                return HasAnyUnit(enemyPrefabs)
                    || HasAnyUnit(assignedEnemies)
                    || HasAnyUnit(spawnedEnemies);
            }
        }

        private void Start()
        {
            EnsureActiveAssignedEnemyTileOccupancyAtStart();

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

                if (keepBossLikeAssignedEnemiesActiveAtStart && IsBossLikeAssignedEnemy(enemy))
                {
                    EnsureCurrentTileOccupancy(enemy);
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
            if (HasEnemyPrefabs())
            {
                return SpawnEnemiesOnce();
            }

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
            Unit[] enemies = AssignedEnemies;
            if (enemies == null)
            {
                return false;
            }

            for (int i = 0; i < enemies.Length; i++)
            {
                Unit enemy = enemies[i];
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
            Unit[] enemies = AssignedEnemies;
            if (enemies == null)
            {
                return;
            }

            for (int i = 0; i < enemies.Length; i++)
            {
                Unit enemy = enemies[i];
                if (enemy == null)
                {
                    continue;
                }

                enemy.MarkDeadAndReleaseTile();
            }
        }

        private int SpawnEnemiesOnce()
        {
            if (hasSpawnedEnemies)
            {
                return CountAliveEnemies(spawnedEnemies);
            }

            spawnedEnemies = new Unit[enemyPrefabs.Length];
            hasSpawnedEnemies = true;

            int aliveEnemyCount = 0;
            for (int i = 0; i < enemyPrefabs.Length; i++)
            {
                Unit prefab = enemyPrefabs[i];
                if (prefab == null)
                {
                    Debug.LogWarning("RoomEnemyActivator skipped enemy prefab at index " + i + " because it is missing.", this);
                    continue;
                }

                Tile spawnTile = GetSpawnTile(i);
                if (spawnTile == null)
                {
                    Debug.LogWarning("RoomEnemyActivator skipped enemy prefab " + prefab.name + " because no matching spawn Tile is assigned.", this);
                    continue;
                }

                Unit spawnedEnemy = SpawnEnemyOnTile(prefab, spawnTile, i);
                spawnedEnemies[i] = spawnedEnemy;

                if (spawnedEnemy != null && spawnedEnemy.IsAlive)
                {
                    aliveEnemyCount++;
                }
            }

            return aliveEnemyCount;
        }

        private Unit SpawnEnemyOnTile(Unit prefab, Tile spawnTile, int index)
        {
            if (!spawnTile.CanEnter())
            {
                Debug.LogWarning("RoomEnemyActivator skipped enemy prefab " + prefab.name + " because spawn Tile " + spawnTile.Position + " cannot be entered.", spawnTile);
                return null;
            }

            Transform parent = spawnedEnemyParent != null ? spawnedEnemyParent : transform;
            Unit enemy = Object.Instantiate(prefab, spawnTile.transform.position, spawnTile.transform.rotation, parent);
            enemy.name = prefab.name + "_RoomSpawn_" + index;
            enemy.gameObject.SetActive(true);

            if (!enemy.TryPlaceOnTile(spawnTile))
            {
                Debug.LogWarning("RoomEnemyActivator destroyed spawned enemy " + enemy.name + " because Unit.TryPlaceOnTile rejected the spawn Tile.", enemy);
                Object.Destroy(enemy.gameObject);
                return null;
            }

            enemy.transform.position = spawnTile.transform.position;
            enemy.transform.rotation = spawnTile.transform.rotation;
            return enemy;
        }

        private bool TryActivateEnemy(Unit enemy, int index)
        {
            enemy.gameObject.SetActive(true);

            if (enemy.CurrentTile != null)
            {
                EnsureCurrentTileOccupancy(enemy);
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

        private void EnsureCurrentTileOccupancy(Unit enemy)
        {
            if (enemy == null || enemy.CurrentTile == null)
            {
                return;
            }

            Tile tile = enemy.CurrentTile;
            if (!tile.IsOccupied || tile.OccupantId == enemy.UnitId)
            {
                tile.SetOccupant(enemy.UnitId);
            }
        }

        private void EnsureActiveAssignedEnemyTileOccupancyAtStart()
        {
            if (assignedEnemies == null)
            {
                return;
            }

            for (int i = 0; i < assignedEnemies.Length; i++)
            {
                Unit enemy = assignedEnemies[i];
                if (enemy != null && enemy.gameObject.activeInHierarchy)
                {
                    EnsureCurrentTileOccupancy(enemy);
                }
            }
        }

        private Tile GetSpawnTile(int index)
        {
            if (spawnTiles == null || index < 0 || index >= spawnTiles.Length)
            {
                return null;
            }

            return spawnTiles[index];
        }

        private bool HasEnemyPrefabs()
        {
            return HasAnyUnit(enemyPrefabs);
        }

        private static bool HasAnyUnit(Unit[] units)
        {
            if (units == null || units.Length == 0)
            {
                return false;
            }

            for (int i = 0; i < units.Length; i++)
            {
                if (units[i] != null)
                {
                    return true;
                }
            }

            return false;
        }

        private static int CountAliveEnemies(Unit[] enemies)
        {
            if (enemies == null)
            {
                return 0;
            }

            int aliveEnemyCount = 0;
            for (int i = 0; i < enemies.Length; i++)
            {
                Unit enemy = enemies[i];
                if (enemy != null && enemy.gameObject.activeInHierarchy && enemy.IsAlive)
                {
                    aliveEnemyCount++;
                }
            }

            return aliveEnemyCount;
        }

        private bool IsBossLikeAssignedEnemy(Unit enemy)
        {
            if (enemy == null)
            {
                return false;
            }

            return ContainsBossNeedle(enemy.gameObject.name) || ContainsBossNeedle(enemy.DisplayName);
        }

        private bool ContainsBossNeedle(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return false;
            }

            string normalized = value.ToLowerInvariant();
            string bossNeedle = string.IsNullOrEmpty(bossNameContains) ? "boss" : bossNameContains.ToLowerInvariant();
            string golemNeedle = string.IsNullOrEmpty(golemNameContains) ? "golem" : golemNameContains.ToLowerInvariant();
            return normalized.Contains(bossNeedle) || normalized.Contains(golemNeedle);
        }
    }
}
