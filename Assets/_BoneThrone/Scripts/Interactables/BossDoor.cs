using BoneThrone.Audio;
using BoneThrone.Grid;
using BoneThrone.Levels;
using BoneThrone.Units;
using UnityEngine;

namespace BoneThrone.Interactables
{
    /// <summary>
    /// Minimal boss gate component. It only checks boss-key state and toggles local blockers / visuals.
    /// </summary>
    public sealed class BossDoor : MonoBehaviour
    {
        [SerializeField] private BossGateProgressionState progressionState;
        [SerializeField] private Collider doorBlocker;
        [SerializeField] private GameObject lockedVisual;
        [SerializeField] private GameObject openedVisual;
        [SerializeField] private bool opened;
        [Header("Visual Fallback")]
        [SerializeField] private bool hideFallbackClosedVisualWhenOpened = true;
        [SerializeField] private GameObject fallbackClosedVisual;
        [Header("Grid Blocking")]
        [SerializeField] private bool autoDetectBlockedTiles = true;
        [SerializeField] [Min(0.1f)] private float blockedTileDetectionRadius = 1.25f;
        [SerializeField] private Tile[] blockedTiles;
        [Header("Auto Open")]
        [SerializeField] private bool autoOpenWhenPlayerWithKeyNearby = true;
        [SerializeField] [Min(0.1f)] private float autoOpenRadius = 3.25f;
        [SerializeField] [Min(0.05f)] private float autoOpenCheckInterval = 0.2f;
        [SerializeField] private bool debugLogging;

        private bool missingProgressionWarningLogged;
        private Tile[] runtimeBlockedTiles;
        private bool[] runtimeBlockedTileInitialWalkable;
        private float nextAutoOpenCheckTime;

        public bool IsOpened
        {
            get { return opened; }
        }

        private void Awake()
        {
            ResolveFallbackClosedVisual();
            ResolveBlockedTiles();
            SetOpenedVisual(opened);
        }

        private void Update()
        {
            if (opened)
            {
                return;
            }

            SetBlockedTilesWalkable(false);
            TryAutoOpenForNearbyPlayer();
        }

        private void OnMouseDown()
        {
            TryOpen();
        }

        public bool CanOpen()
        {
            ResolveProgressionState();
            if (progressionState == null)
            {
                LogMissingProgressionWarning();
                return false;
            }

            return progressionState.CanOpenBossDoor();
        }

        public bool TryOpen()
        {
            if (opened)
            {
                Log("Open ignored because the boss door is already open.");
                return false;
            }

            if (!CanOpen())
            {
                Log("Open rejected because boss key requirements are not met.");
                BTAudioService.PlaySfx(BTAudioCueId.InvalidAction);
                return false;
            }

            if (!progressionState.OpenBossDoor())
            {
                return false;
            }

            opened = true;
            SetOpenedVisual(true);
            BTAudioService.PlaySfx(BTAudioCueId.KeyPickup);
            Log("Boss door opened.");
            return true;
        }

        public void SetOpenedVisual(bool isOpened)
        {
            if (doorBlocker != null)
            {
                doorBlocker.enabled = !isOpened;
            }

            SetBlockedTilesWalkable(isOpened);

            if (lockedVisual != null)
            {
                lockedVisual.SetActive(!isOpened);
            }

            if (openedVisual != null)
            {
                openedVisual.SetActive(isOpened);
            }

            if (lockedVisual == null
                && openedVisual == null
                && hideFallbackClosedVisualWhenOpened
                && ResolveFallbackClosedVisual() != null)
            {
                fallbackClosedVisual.SetActive(!isOpened);
            }
        }

        private void ResolveBlockedTiles()
        {
            if (HasAnyTile(blockedTiles))
            {
                runtimeBlockedTiles = blockedTiles;
                CacheRuntimeBlockedTileInitialWalkable();
                return;
            }

            if (!autoDetectBlockedTiles)
            {
                runtimeBlockedTiles = null;
                runtimeBlockedTileInitialWalkable = null;
                return;
            }

            Tile[] allTiles = Object.FindObjectsByType<Tile>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            if (allTiles == null || allTiles.Length == 0)
            {
                runtimeBlockedTiles = null;
                runtimeBlockedTileInitialWalkable = null;
                return;
            }

            int count = 0;
            float radiusSqr = blockedTileDetectionRadius * blockedTileDetectionRadius;
            Vector3 doorPosition = transform.position;
            for (int i = 0; i < allTiles.Length; i++)
            {
                Tile tile = allTiles[i];
                if (tile == null)
                {
                    continue;
                }

                Vector3 delta = tile.transform.position - doorPosition;
                delta.y = 0f;
                if (delta.sqrMagnitude <= radiusSqr)
                {
                    count++;
                }
            }

            if (count == 0)
            {
                runtimeBlockedTiles = null;
                runtimeBlockedTileInitialWalkable = null;
                return;
            }

            runtimeBlockedTiles = new Tile[count];
            int writeIndex = 0;
            for (int i = 0; i < allTiles.Length; i++)
            {
                Tile tile = allTiles[i];
                if (tile == null)
                {
                    continue;
                }

                Vector3 delta = tile.transform.position - doorPosition;
                delta.y = 0f;
                if (delta.sqrMagnitude <= radiusSqr)
                {
                    runtimeBlockedTiles[writeIndex] = tile;
                    writeIndex++;
                }
            }

            CacheRuntimeBlockedTileInitialWalkable();
        }

        private void SetBlockedTilesWalkable(bool walkable)
        {
            Tile[] tiles = runtimeBlockedTiles;
            if (tiles == null)
            {
                return;
            }

            for (int i = 0; i < tiles.Length; i++)
            {
                Tile tile = tiles[i];
                if (tile != null)
                {
                    tile.SetWalkable(walkable ? GetInitialWalkableState(i) : false);
                }
            }
        }

        private void CacheRuntimeBlockedTileInitialWalkable()
        {
            if (runtimeBlockedTiles == null)
            {
                runtimeBlockedTileInitialWalkable = null;
                return;
            }

            runtimeBlockedTileInitialWalkable = new bool[runtimeBlockedTiles.Length];
            for (int i = 0; i < runtimeBlockedTiles.Length; i++)
            {
                Tile tile = runtimeBlockedTiles[i];
                runtimeBlockedTileInitialWalkable[i] = tile != null && tile.IsWalkable;
            }
        }

        private bool GetInitialWalkableState(int index)
        {
            if (runtimeBlockedTileInitialWalkable == null
                || index < 0
                || index >= runtimeBlockedTileInitialWalkable.Length)
            {
                return true;
            }

            return runtimeBlockedTileInitialWalkable[index];
        }

        private void ResolveProgressionState()
        {
            if (progressionState == null)
            {
                progressionState = BossGateProgressionState.GetOrCreateSceneState();
            }
        }

        private GameObject ResolveFallbackClosedVisual()
        {
            if (fallbackClosedVisual != null)
            {
                return fallbackClosedVisual;
            }

            Transform visualChild = transform.Find("Visual");
            if (visualChild != null)
            {
                fallbackClosedVisual = visualChild.gameObject;
                return fallbackClosedVisual;
            }

            if (transform.childCount == 1)
            {
                fallbackClosedVisual = transform.GetChild(0).gameObject;
            }

            return fallbackClosedVisual;
        }

        private void TryAutoOpenForNearbyPlayer()
        {
            if (!autoOpenWhenPlayerWithKeyNearby || Time.time < nextAutoOpenCheckTime)
            {
                return;
            }

            nextAutoOpenCheckTime = Time.time + Mathf.Max(0.05f, autoOpenCheckInterval);
            ResolveProgressionState();
            if (progressionState == null || !progressionState.CanOpenBossDoor())
            {
                return;
            }

            Unit nearbyPlayer = FindNearbyLivingPlayer();
            if (nearbyPlayer != null)
            {
                TryOpen();
            }
        }

        private Unit FindNearbyLivingPlayer()
        {
            Unit[] units = Object.FindObjectsByType<Unit>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            float radiusSqr = autoOpenRadius * autoOpenRadius;
            Vector3 doorPosition = transform.position;
            for (int i = 0; i < units.Length; i++)
            {
                Unit unit = units[i];
                if (unit == null
                    || !unit.gameObject.activeInHierarchy
                    || unit.Faction != UnitFaction.Player
                    || !unit.IsAlive)
                {
                    continue;
                }

                Vector3 delta = unit.transform.position - doorPosition;
                delta.y = 0f;
                if (delta.sqrMagnitude <= radiusSqr)
                {
                    return unit;
                }
            }

            return null;
        }

        private static bool HasAnyTile(Tile[] tiles)
        {
            if (tiles == null)
            {
                return false;
            }

            for (int i = 0; i < tiles.Length; i++)
            {
                if (tiles[i] != null)
                {
                    return true;
                }
            }

            return false;
        }

        private void LogMissingProgressionWarning()
        {
            if (missingProgressionWarningLogged)
            {
                return;
            }

            missingProgressionWarningLogged = true;
            Debug.LogWarning("BossDoor cannot open because BossGateProgressionState is missing.", this);
        }

        private void Log(string message)
        {
            if (debugLogging)
            {
                Debug.Log("BossDoor: " + message, this);
            }
        }
    }
}
