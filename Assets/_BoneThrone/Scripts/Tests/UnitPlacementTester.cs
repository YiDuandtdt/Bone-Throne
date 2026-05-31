using BoneThrone.Grid;
using BoneThrone.Units;
using UnityEngine;

namespace BoneThrone.Tests
{
    /// <summary>
    /// Context menu helper for manually verifying Phase 4 unit tile occupancy in Play Mode.
    /// </summary>
    public sealed class UnitPlacementTester : MonoBehaviour
    {
        [SerializeField] private GridManager gridManager;
        [SerializeField] private Unit unit;
        [SerializeField] private Tile targetTile;
        [SerializeField] private int targetX;
        [SerializeField] private int targetY;

        [ContextMenu("Phase 4/Initialize Unit Runtime")]
        public void InitializeUnitRuntime()
        {
            if (unit == null)
            {
                Debug.LogWarning("UnitPlacementTester needs a Unit reference.", this);
                return;
            }

            unit.InitializeRuntime();
            Debug.Log("Initialized runtime state for unit " + unit.UnitId + ".", unit);
        }

        [ContextMenu("Phase 4/Place Unit On Target Tile")]
        public void PlaceUnitOnTargetTile()
        {
            if (unit == null)
            {
                Debug.LogWarning("UnitPlacementTester needs a Unit reference.", this);
                return;
            }

            bool placed = unit.TryPlaceOnTile(targetTile);
            Debug.Log("PlaceUnitOnTargetTile result: " + placed + ".", this);
        }

        [ContextMenu("Phase 4/Place Unit At Target Position")]
        public void PlaceUnitAtTargetPosition()
        {
            if (unit == null)
            {
                Debug.LogWarning("UnitPlacementTester needs a Unit reference.", this);
                return;
            }

            bool placed = unit.TryPlaceAt(gridManager, new GridPosition(targetX, targetY));
            Debug.Log("PlaceUnitAtTargetPosition result: " + placed + ".", this);
        }

        [ContextMenu("Phase 4/Mark Unit Dead And Release Tile")]
        public void MarkUnitDeadAndReleaseTile()
        {
            if (unit == null)
            {
                Debug.LogWarning("UnitPlacementTester needs a Unit reference.", this);
                return;
            }

            unit.MarkDeadAndReleaseTile();
            Debug.Log("Marked unit " + unit.UnitId + " dead and released its tile.", unit);
        }

        [ContextMenu("Phase 4/Release Unit Tile")]
        public void ReleaseUnitTile()
        {
            if (unit == null)
            {
                Debug.LogWarning("UnitPlacementTester needs a Unit reference.", this);
                return;
            }

            unit.ReleaseTile();
            Debug.Log("Released tile for unit " + unit.UnitId + ".", unit);
        }
    }
}
