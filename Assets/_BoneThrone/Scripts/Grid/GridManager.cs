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

            if (initialTiles == null)
            {
                return;
            }

            for (int i = 0; i < initialTiles.Length; i++)
            {
                RegisterTile(initialTiles[i]);
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
