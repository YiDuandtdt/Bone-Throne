using System.Collections.Generic;
using UnityEngine;

namespace BoneThrone.Grid
{
    /// <summary>
    /// Registers scene tiles and provides minimal grid state queries.
    /// </summary>
    public sealed class GridManager : MonoBehaviour
    {
        [SerializeField] private Tile[] initialTiles;
        [SerializeField]
        [Tooltip("When the serialized tile list has no valid entries, scan scene Tile components once in Awake.")]
        private bool autoFindSceneTilesIfInitialTilesMissing = true;

        private readonly Dictionary<GridPosition, Tile> tilesByPosition = new Dictionary<GridPosition, Tile>();

        public int RegisteredTileCount
        {
            get { return tilesByPosition.Count; }
        }

        private void Awake()
        {
            RegisterInitialTiles();
        }

        public void RegisterInitialTiles()
        {
            tilesByPosition.Clear();

            int registeredTileCount = RegisterTiles(initialTiles);
            if (registeredTileCount > 0 || !autoFindSceneTilesIfInitialTilesMissing)
            {
                return;
            }

            int discoveredTileCount = RegisterSceneTiles();
            if (discoveredTileCount > 0)
            {
                Debug.Log(
                    "GridManager auto-registered " + discoveredTileCount + " scene tiles because Initial Tiles had no valid entries.",
                    this);
            }
        }

        public bool RegisterTile(Tile tile)
        {
            if (tile == null)
            {
                Debug.LogWarning("GridManager skipped a null tile registration.", this);
                return false;
            }

            GridPosition position = tile.Position;
            if (tilesByPosition.ContainsKey(position))
            {
                Debug.LogWarning("GridManager rejected duplicate tile coordinate " + position + " on " + tile.name + ".", tile);
                return false;
            }

            tilesByPosition.Add(position, tile);
            return true;
        }

        private int RegisterTiles(Tile[] tiles)
        {
            if (tiles == null)
            {
                return 0;
            }

            int registeredTileCount = 0;
            for (int i = 0; i < tiles.Length; i++)
            {
                Tile tile = tiles[i];
                if (tile == null)
                {
                    continue;
                }

                if (RegisterTile(tile))
                {
                    registeredTileCount++;
                }
            }

            return registeredTileCount;
        }

        private int RegisterSceneTiles()
        {
            Tile[] sceneTiles = Object.FindObjectsByType<Tile>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            return RegisterTiles(sceneTiles);
        }

        public bool UnregisterTile(Tile tile)
        {
            if (tile == null)
            {
                return false;
            }

            return tilesByPosition.Remove(tile.Position);
        }

        public bool TryGetTile(GridPosition position, out Tile tile)
        {
            return tilesByPosition.TryGetValue(position, out tile);
        }

        public bool ContainsPosition(GridPosition position)
        {
            return tilesByPosition.ContainsKey(position);
        }

        public bool IsWalkable(GridPosition position)
        {
            Tile tile;
            return TryGetTile(position, out tile) && tile.IsWalkable;
        }

        public bool IsOccupied(GridPosition position)
        {
            Tile tile;
            return TryGetTile(position, out tile) && tile.IsOccupied;
        }

        public bool CanEnter(GridPosition position)
        {
            Tile tile;
            return TryGetTile(position, out tile) && tile.CanEnter();
        }

        public void FillRegisteredTiles(List<Tile> results)
        {
            if (results == null)
            {
                return;
            }

            results.Clear();
            foreach (KeyValuePair<GridPosition, Tile> pair in tilesByPosition)
            {
                if (pair.Value != null)
                {
                    results.Add(pair.Value);
                }
            }
        }
    }
}
