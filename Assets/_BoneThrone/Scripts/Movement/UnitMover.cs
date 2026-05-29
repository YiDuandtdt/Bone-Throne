using System.Collections.Generic;
using BoneThrone.Grid;
using BoneThrone.Units;
using UnityEngine;

namespace BoneThrone.Movement
{
    /// <summary>
    /// Applies a resolved movement path by placing the unit on the final tile.
    /// </summary>
    public sealed class UnitMover : MonoBehaviour
    {
        [SerializeField] private Vector3 worldPositionOffset = Vector3.zero;

        public bool TryMove(Unit unit, GridManager gridManager, IReadOnlyList<GridPosition> path)
        {
            if (unit == null)
            {
                Debug.LogWarning("Movement failed because Unit is missing.", this);
                return false;
            }

            if (gridManager == null)
            {
                Debug.LogWarning("Movement failed because GridManager is missing.", unit);
                return false;
            }

            if (path == null || path.Count < 2)
            {
                Debug.LogWarning("Movement failed because the path must include a start and target position.", unit);
                return false;
            }

            GridPosition targetPosition = path[path.Count - 1];
            Tile targetTile;
            if (!gridManager.TryGetTile(targetPosition, out targetTile))
            {
                Debug.LogWarning("Movement failed because the target tile does not exist: " + targetPosition + ".", unit);
                return false;
            }

            if (!gridManager.CanEnter(targetPosition))
            {
                Debug.LogWarning("Movement failed because the target tile cannot be entered: " + targetPosition + ".", targetTile);
                return false;
            }

            UnitAnimationController animationController = unit.GetComponent<UnitAnimationController>();
            animationController?.SetMoveSpeed(1f);

            Tile originalTile = unit.CurrentTile;
            bool moved = unit.TryPlaceOnTile(targetTile);
            if (!moved)
            {
                animationController?.SetMoveSpeed(0f);
                return false;
            }

            unit.transform.position = targetTile.transform.position + worldPositionOffset;
            animationController?.SetMoveSpeed(0f);

            if (originalTile != null && originalTile != targetTile && originalTile.IsOccupied)
            {
                Debug.LogWarning("Movement completed, but the original tile still appears occupied: " + originalTile.Position + ".", originalTile);
            }

            if (!targetTile.IsOccupied || targetTile.OccupantId != unit.UnitId)
            {
                Debug.LogWarning("Movement completed, but target occupancy does not match UnitId " + unit.UnitId + ".", targetTile);
            }

            Debug.Log("Moved unit " + unit.UnitId + " to " + targetPosition + ".", unit);
            return true;
        }
    }
}
