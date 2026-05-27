using System.Collections.Generic;
using BoneThrone.Grid;
using UnityEngine;

namespace BoneThrone.Movement
{
    /// <summary>
    /// Simple Play Mode debug highlighter for reachable tiles.
    /// </summary>
    public sealed class MovementDebugHighlighter : MonoBehaviour
    {
        [SerializeField] private Color selectedColor = new Color(0.2f, 0.45f, 1f, 1f);
        [SerializeField] private Color reachableColor = new Color(0.25f, 0.85f, 0.35f, 1f);
        [SerializeField] private Color attackColor = new Color(0.95f, 0.18f, 0.16f, 1f);

        private readonly Dictionary<Renderer, Color> originalColors = new Dictionary<Renderer, Color>();
        private readonly HashSet<Renderer> actionRenderers = new HashSet<Renderer>();
        private Renderer selectedRenderer;
        private Color actionColor;

        private void Awake()
        {
            actionColor = reachableColor;
        }

        public void ShowSelected(Tile tile)
        {
            Renderer renderer = GetTileRenderer(tile);
            if (selectedRenderer == renderer)
            {
                Repaint();
                return;
            }

            selectedRenderer = renderer;
            if (selectedRenderer != null)
            {
                RememberOriginalColor(selectedRenderer);
            }

            Repaint();
        }

        public void ClearSelected()
        {
            selectedRenderer = null;
            Repaint();
        }

        public void ShowReachable(GridManager gridManager, IEnumerable<GridPosition> positions)
        {
            ShowMoveRange(gridManager, positions);
        }

        public void ShowMoveRange(GridManager gridManager, IEnumerable<GridPosition> positions)
        {
            actionRenderers.Clear();
            actionColor = reachableColor;
            Repaint();

            if (gridManager == null || positions == null)
            {
                return;
            }

            foreach (GridPosition position in positions)
            {
                Tile tile;
                if (!gridManager.TryGetTile(position, out tile) || tile == null)
                {
                    continue;
                }

                Renderer renderer = GetTileRenderer(tile);
                if (renderer == null || renderer.material == null)
                {
                    continue;
                }

                RememberOriginalColor(renderer);
                actionRenderers.Add(renderer);
            }

            Repaint();
        }

        public void ShowAttackTargets(IEnumerable<Tile> tiles)
        {
            actionRenderers.Clear();
            actionColor = attackColor;
            Repaint();

            if (tiles == null)
            {
                return;
            }

            foreach (Tile tile in tiles)
            {
                Renderer renderer = GetTileRenderer(tile);
                if (renderer == null || renderer.material == null)
                {
                    continue;
                }

                RememberOriginalColor(renderer);
                actionRenderers.Add(renderer);
            }

            Repaint();
        }

        public void ClearActionHighlights()
        {
            actionRenderers.Clear();
            Repaint();
        }

        public void Clear()
        {
            actionRenderers.Clear();
            ClearSelected();
        }

        private void Repaint()
        {
            foreach (KeyValuePair<Renderer, Color> pair in originalColors)
            {
                if (pair.Key != null)
                {
                    SetRendererColor(pair.Key, pair.Value);
                }
            }

            if (selectedRenderer != null)
            {
                SetRendererColor(selectedRenderer, selectedColor);
            }

            foreach (Renderer renderer in actionRenderers)
            {
                if (renderer != null)
                {
                    SetRendererColor(renderer, actionColor);
                }
            }
        }

        private void RememberOriginalColor(Renderer renderer)
        {
            if (originalColors.ContainsKey(renderer))
            {
                return;
            }

            originalColors.Add(renderer, GetRendererColor(renderer));
        }

        private static Renderer GetTileRenderer(Tile tile)
        {
            return tile != null ? tile.GetComponentInChildren<Renderer>() : null;
        }

        private static Color GetRendererColor(Renderer renderer)
        {
            if (renderer.material.HasProperty("_BaseColor"))
            {
                return renderer.material.GetColor("_BaseColor");
            }

            if (renderer.material.HasProperty("_Color"))
            {
                return renderer.material.GetColor("_Color");
            }

            return Color.white;
        }

        private static void SetRendererColor(Renderer renderer, Color color)
        {
            if (renderer.material.HasProperty("_BaseColor"))
            {
                renderer.material.SetColor("_BaseColor", color);
                return;
            }

            if (renderer.material.HasProperty("_Color"))
            {
                renderer.material.SetColor("_Color", color);
            }
        }
    }
}
