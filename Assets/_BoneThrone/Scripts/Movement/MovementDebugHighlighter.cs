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
        [SerializeField] private Color reachableColor = new Color(0.25f, 0.85f, 0.35f, 1f);

        private readonly Dictionary<Renderer, Color> originalColors = new Dictionary<Renderer, Color>();

        public void ShowReachable(GridManager gridManager, IEnumerable<GridPosition> positions)
        {
            Clear();

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

                Renderer renderer = tile.GetComponentInChildren<Renderer>();
                if (renderer == null || renderer.material == null)
                {
                    continue;
                }

                RememberOriginalColor(renderer);
                SetRendererColor(renderer, reachableColor);
            }
        }

        public void Clear()
        {
            foreach (KeyValuePair<Renderer, Color> pair in originalColors)
            {
                if (pair.Key != null)
                {
                    SetRendererColor(pair.Key, pair.Value);
                }
            }

            originalColors.Clear();
        }

        private void RememberOriginalColor(Renderer renderer)
        {
            if (originalColors.ContainsKey(renderer))
            {
                return;
            }

            originalColors.Add(renderer, GetRendererColor(renderer));
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
