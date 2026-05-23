using System.Collections.Generic;
using BoneThrone.Grid;
using BoneThrone.Units;
using UnityEngine;

namespace BoneThrone.Movement
{
    /// <summary>
    /// Temporary Play Mode controller that wires click selection, BFS range, A* pathfinding, and unit movement.
    /// </summary>
    public sealed class PlayerMovementController : MonoBehaviour
    {
        [SerializeField] private Camera inputCamera;
        [SerializeField] private LayerMask inputLayerMask = ~0;
        [SerializeField] private float maxRayDistance = 500f;
        [SerializeField] private GridManager gridManager;
        [SerializeField] private SelectionManager selectionManager;
        [SerializeField] private MovementRangeFinder movementRangeFinder;
        [SerializeField] private Pathfinder pathfinder;
        [SerializeField] private UnitMover unitMover;
        [SerializeField] private MovementDebugHighlighter debugHighlighter;

        private readonly HashSet<GridPosition> reachablePositions = new HashSet<GridPosition>();

        private void Update()
        {
            if (!Input.GetMouseButtonDown(0))
            {
                return;
            }

            RaycastHit hit;
            if (!TryGetInputHit(out hit))
            {
                return;
            }

            Unit clickedUnit = hit.collider.GetComponentInParent<Unit>();
            if (clickedUnit != null)
            {
                HandleUnitClick(clickedUnit);
                return;
            }

            Tile clickedTile = hit.collider.GetComponentInParent<Tile>();
            if (clickedTile != null)
            {
                HandleTileClick(clickedTile);
            }
        }

        private void HandleUnitClick(Unit unit)
        {
            if (!HasRequiredReferences())
            {
                return;
            }

            if (!selectionManager.TrySelect(unit))
            {
                return;
            }

            RefreshReachablePositions();
        }

        private void HandleTileClick(Tile tile)
        {
            if (!HasRequiredReferences())
            {
                return;
            }

            if (!selectionManager.HasSelection)
            {
                Debug.LogWarning("Tile click ignored because no player unit is selected.", tile);
                return;
            }

            GridPosition targetPosition = tile.Position;
            if (!reachablePositions.Contains(targetPosition))
            {
                Debug.LogWarning("Tile click ignored because " + targetPosition + " is not in the selected unit's reachable range.", tile);
                return;
            }

            Unit selectedUnit = selectionManager.SelectedUnit;
            if (selectedUnit == null || selectedUnit.CurrentTile == null)
            {
                Debug.LogWarning("Movement ignored because the selected unit has no current tile.", this);
                return;
            }

            List<GridPosition> path;
            if (!pathfinder.TryFindPath(gridManager, selectedUnit.CurrentTile.Position, targetPosition, out path))
            {
                Debug.LogWarning("Movement ignored because no valid A* path was found.", tile);
                return;
            }

            if (unitMover.TryMove(selectedUnit, gridManager, path))
            {
                RefreshReachablePositions();
            }
        }

        private void RefreshReachablePositions()
        {
            reachablePositions.Clear();

            Unit selectedUnit = selectionManager.SelectedUnit;
            if (selectedUnit == null || selectedUnit.CurrentTile == null)
            {
                if (debugHighlighter != null)
                {
                    debugHighlighter.Clear();
                }

                return;
            }

            int moveRange = selectedUnit.Stats != null ? selectedUnit.Stats.MoveRange : 0;
            HashSet<GridPosition> range = movementRangeFinder.FindReachablePositions(
                gridManager,
                selectedUnit.CurrentTile.Position,
                moveRange);

            foreach (GridPosition position in range)
            {
                reachablePositions.Add(position);
            }

            if (debugHighlighter != null)
            {
                debugHighlighter.ShowReachable(gridManager, reachablePositions);
            }

            Debug.Log("Reachable tile count for unit " + selectedUnit.UnitId + ": " + reachablePositions.Count + ".", selectedUnit);
        }

        private bool TryGetInputHit(out RaycastHit hit)
        {
            Camera cameraToUse = inputCamera != null ? inputCamera : Camera.main;
            if (cameraToUse == null)
            {
                Debug.LogWarning("PlayerMovementController needs an input camera or a MainCamera-tagged camera.", this);
                hit = default(RaycastHit);
                return false;
            }

            Ray ray = cameraToUse.ScreenPointToRay(Input.mousePosition);
            return Physics.Raycast(ray, out hit, maxRayDistance, inputLayerMask);
        }

        private bool HasRequiredReferences()
        {
            if (gridManager == null)
            {
                Debug.LogWarning("PlayerMovementController needs a GridManager reference.", this);
                return false;
            }

            if (selectionManager == null)
            {
                Debug.LogWarning("PlayerMovementController needs a SelectionManager reference.", this);
                return false;
            }

            if (movementRangeFinder == null)
            {
                Debug.LogWarning("PlayerMovementController needs a MovementRangeFinder reference.", this);
                return false;
            }

            if (pathfinder == null)
            {
                Debug.LogWarning("PlayerMovementController needs a Pathfinder reference.", this);
                return false;
            }

            if (unitMover == null)
            {
                Debug.LogWarning("PlayerMovementController needs a UnitMover reference.", this);
                return false;
            }

            return true;
        }
    }
}
