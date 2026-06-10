using System.Collections.Generic;
using System.Reflection;
using BoneThrone.Grid;
using UnityEngine;

namespace BoneThrone.Tests
{
    /// <summary>
    /// Temporary GridTest scene helper for building a small manual-test grid.
    /// This is not a room, level, random map, combat, AI, or networking system.
    /// </summary>
    public sealed class GridTestBuilder : MonoBehaviour
    {
        [SerializeField] private GridManager gridManager;
        [SerializeField] private Tile tilePrefab;
        [SerializeField] private Transform tileParent;
        [SerializeField] private int width = 5;
        [SerializeField] private int height = 5;
        [SerializeField] private float spacing = 1.25f;

        private const string GeneratedRootName = "GeneratedTestTiles";

        [ContextMenu("Phase 5/Build Test Grid")]
        public void BuildTestGrid()
        {
            if (gridManager == null)
            {
                Debug.LogWarning("GridTestBuilder needs a GridManager reference.", this);
                return;
            }

            if (tilePrefab == null)
            {
                Debug.LogWarning("GridTestBuilder needs a Tile prefab reference.", this);
                return;
            }

            if (width <= 0 || height <= 0)
            {
                Debug.LogWarning("GridTestBuilder width and height must be greater than zero.", this);
                return;
            }

            ClearGeneratedTiles();

            Transform parent = GetOrCreateGeneratedRoot();
            List<Tile> generatedTiles = new List<Tile>();

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Tile tile = CreateTile(parent, x, y);
                    if (tile == null)
                    {
                        continue;
                    }

                    generatedTiles.Add(tile);
                }
            }

            AssignInitialTiles(generatedTiles.ToArray());
            gridManager.RegisterInitialTiles();
            Debug.Log("GridTestBuilder generated " + generatedTiles.Count + " test tiles.", this);
        }

        [ContextMenu("Phase 5/Clear Generated Tiles")]
        public void ClearGeneratedTiles()
        {
            Transform parent = GetGeneratedRoot();
            if (parent == null)
            {
                Debug.Log("GridTestBuilder cleared 0 generated test tiles.", this);
                return;
            }

            List<GameObject> objectsToRemove = new List<GameObject>();

            for (int i = 0; i < parent.childCount; i++)
            {
                Transform child = parent.GetChild(i);
                objectsToRemove.Add(child.gameObject);
            }

            for (int i = 0; i < objectsToRemove.Count; i++)
            {
                DestroyGeneratedObject(objectsToRemove[i]);
            }

            if (objectsToRemove.Count > 0)
            {
                AssignInitialTiles(new Tile[0]);
                if (gridManager != null)
                {
                    gridManager.RegisterInitialTiles();
                }
            }

            Debug.Log("GridTestBuilder cleared " + objectsToRemove.Count + " generated test tiles.", this);
        }

        private Tile CreateTile(Transform parent, int x, int y)
        {
            Tile tile = Instantiate(tilePrefab, parent);
            tile.name = "Tile_" + x + "_" + y;
            tile.transform.localPosition = new Vector3(x * spacing, 0f, y * spacing);
            tile.Initialize(new GridPosition(x, y));
            EnsureCollider(tile);
            return tile;
        }

        private Transform GetOrCreateGeneratedRoot()
        {
            Transform existingRoot = GetGeneratedRoot();
            if (existingRoot != null)
            {
                return existingRoot;
            }

            Transform rootParent = tileParent != null ? tileParent : transform;
            GameObject rootObject = new GameObject(GeneratedRootName);
            rootObject.transform.SetParent(rootParent);
            rootObject.transform.localPosition = Vector3.zero;
            return rootObject.transform;
        }

        private Transform GetGeneratedRoot()
        {
            Transform rootParent = tileParent != null ? tileParent : transform;
            return rootParent.Find(GeneratedRootName);
        }

        private static void EnsureCollider(Tile tile)
        {
            Collider existingCollider = tile.GetComponentInChildren<Collider>();
            if (existingCollider != null)
            {
                return;
            }

            BoxCollider boxCollider = tile.gameObject.AddComponent<BoxCollider>();
            boxCollider.size = Vector3.one;
        }

        private void AssignInitialTiles(Tile[] tiles)
        {
            FieldInfo field = typeof(GridManager).GetField("initialTiles", BindingFlags.Instance | BindingFlags.NonPublic);
            if (field == null)
            {
                Debug.LogWarning("GridTestBuilder could not access GridManager.initialTiles. Drag generated tiles into GridManager manually.", this);
                return;
            }

            field.SetValue(gridManager, tiles);
        }

        private static void DestroyGeneratedObject(GameObject target)
        {
            if (target == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Destroy(target);
            }
            else
            {
                DestroyImmediate(target);
            }
        }
    }
}
