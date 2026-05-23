using BoneThrone.Units;
using UnityEngine;

namespace BoneThrone.Movement
{
    /// <summary>
    /// Stores the currently selected player unit for temporary local movement testing.
    /// </summary>
    public sealed class SelectionManager : MonoBehaviour
    {
        private Unit selectedUnit;

        public Unit SelectedUnit
        {
            get { return selectedUnit; }
        }

        public bool HasSelection
        {
            get { return selectedUnit != null; }
        }

        public bool TrySelect(Unit unit)
        {
            if (unit == null)
            {
                Debug.LogWarning("Selection failed because the clicked object has no Unit.", this);
                return false;
            }

            if (unit.Faction != UnitFaction.Player)
            {
                Debug.LogWarning("Selection failed because only player units can be selected.", unit);
                return false;
            }

            if (!unit.IsAlive)
            {
                Debug.LogWarning("Selection failed because dead units cannot be selected.", unit);
                return false;
            }

            selectedUnit = unit;
            Debug.Log("Selected unit " + unit.UnitId + ".", unit);
            return true;
        }

        public void ClearSelection()
        {
            selectedUnit = null;
        }
    }
}
