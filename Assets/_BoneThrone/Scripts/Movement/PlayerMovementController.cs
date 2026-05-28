using System.Collections.Generic;
using BoneThrone.Grid;
using BoneThrone.Turns;
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
        [SerializeField] private TurnManager turnManager;
        [SerializeField] private ActionPermissionService actionPermissionService;

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

            if (turnManager != null && turnManager.CurrentPhase != TurnPhase.PlayerTurn)
            {
                Debug.LogWarning("Selection ignored because current phase is " + turnManager.CurrentPhase + ".", unit);
                return;
            }

            if (unit != null && selectionManager.SelectedUnit == unit)
            {
                selectionManager.ClearSelection();
                reachablePositions.Clear();
                if (debugHighlighter != null)
                {
                    debugHighlighter.Clear();
                }

                return;
            }

            if (!selectionManager.TrySelect(unit))
            {
                return;
            }

            reachablePositions.Clear();
            if (debugHighlighter != null)
            {
                debugHighlighter.ClearActionHighlights();
                debugHighlighter.ShowSelected(unit.CurrentTile);
            }
        }

        private void HandleTileClick(Tile tile)
        {
            Debug.LogWarning("Tile movement is controlled by the HUD Move action. Click Move before choosing a tile.", tile);
        }

        public HashSet<GridPosition> GetReachablePositionsForSelected()
        {
            RefreshReachablePositions(false);
            return new HashSet<GridPosition>(reachablePositions);
        }

        public bool TryMoveSelectedUnitTo(Tile tile)
        {
            if (!HasRequiredReferences())
            {
                return false;
            }

            if (tile == null)
            {
                Debug.LogWarning("Movement ignored because target tile is missing.", this);
                return false;
            }

            if (!selectionManager.HasSelection)
            {
                Debug.LogWarning("Tile click ignored because no player unit is selected.", tile);
                return false;
            }

            Unit selectedUnit = selectionManager.SelectedUnit;
            if (selectedUnit == null || selectedUnit.CurrentTile == null)
            {
                Debug.LogWarning("Movement ignored because the selected unit has no current tile.", this);
                return false;
            }

            UnitTurnState selectedTurnState = selectedUnit.GetComponent<UnitTurnState>();
            if (selectedTurnState != null && selectedTurnState.HasEnded)
            {
                Debug.LogWarning("Movement ignored because the selected unit has already ended this turn.", selectedUnit);
                return false;
            }

            if (IsTurnGatingConfigured() && actionPermissionService.TryConsumeStunForAction(selectedUnit, turnManager))
            {
                reachablePositions.Clear();
                if (debugHighlighter != null)
                {
                    debugHighlighter.ClearActionHighlights();
                    debugHighlighter.ShowSelected(selectedUnit.CurrentTile);
                }

                return false;
            }

            GridPosition targetPosition = tile.Position;
            if (!reachablePositions.Contains(targetPosition))
            {
                Debug.LogWarning("Tile click ignored because " + targetPosition + " is not in the selected unit's reachable range.", tile);
                return false;
            }

            if (!CanSelectedUnitMove(selectedUnit))
            {
                return false;
            }

            List<GridPosition> path;
            if (!pathfinder.TryFindPath(gridManager, selectedUnit.CurrentTile.Position, targetPosition, out path))
            {
                Debug.LogWarning("Movement ignored because no valid A* path was found.", tile);
                return false;
            }

            if (unitMover.TryMove(selectedUnit, gridManager, path))
            {
                if (IsTurnGatingConfigured())
                {
                    MarkSelectedUnitMoved(selectedUnit);
                }

                reachablePositions.Clear();
                if (debugHighlighter != null)
                {
                    debugHighlighter.RefreshPlayerFootTiles();
                    debugHighlighter.ClearActionHighlights();
                    debugHighlighter.ShowSelected(selectedUnit.CurrentTile);
                }

                return true;
            }

            return false;
        }

        private void RefreshReachablePositions(bool showHighlights)
        {
            reachablePositions.Clear();

            Unit selectedUnit = selectionManager.SelectedUnit;
            if (selectedUnit == null || selectedUnit.CurrentTile == null)
            {
                if (debugHighlighter != null)
                {
                    debugHighlighter.ClearActionHighlights();
                }

                return;
            }

            if (!CanSelectedUnitMove(selectedUnit))
            {
                if (debugHighlighter != null)
                {
                    debugHighlighter.ClearActionHighlights();
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

            if (showHighlights && debugHighlighter != null)
            {
                debugHighlighter.ShowMoveRange(gridManager, reachablePositions);
            }

            Debug.Log("Reachable tile count for unit " + selectedUnit.UnitId + ": " + reachablePositions.Count + ".", selectedUnit);
        }

        private bool CanSelectedUnitMove(Unit selectedUnit)
        {
            if (!IsTurnGatingConfigured() && turnManager == null && actionPermissionService == null)
            {
                return true;
            }

            if (turnManager == null || actionPermissionService == null)
            {
                Debug.LogWarning("PlayerMovementController turn gating is partially configured. Bind both TurnManager and ActionPermissionService, or leave both empty.", this);
                return false;
            }

            return actionPermissionService.CanMove(selectedUnit, turnManager);
        }

        private bool IsTurnGatingConfigured()
        {
            return turnManager != null && actionPermissionService != null;
        }

        private static void MarkSelectedUnitMoved(Unit selectedUnit)
        {
            UnitTurnState turnState = selectedUnit != null ? selectedUnit.GetComponent<UnitTurnState>() : null;
            if (turnState != null)
            {
                turnState.MarkMoved();
            }
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
            return Physics.Raycast(ray, out hit, maxRayDistance, inputLayerMask, QueryTriggerInteraction.Ignore);
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
