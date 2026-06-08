using BoneThrone.Units;
using BoneThrone.Turns;
using UnityEngine;

namespace BoneThrone.Movement
{
    /// <summary>
    /// Stores the currently selected player unit for temporary local movement testing.
    /// </summary>
    public sealed class SelectionManager : MonoBehaviour
    {
        [SerializeField] private TurnManager turnManager;

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

            if (!TryAutoEndMovedUnitBeforeSwitch(unit))
            {
                return false;
            }

            UnitTurnState turnState = unit.GetComponent<UnitTurnState>();
            if (turnState != null && turnState.HasEnded)
            {
                Debug.LogWarning("Selection failed because unit " + unit.UnitId + " has already ended this turn.", unit);
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

        private bool TryAutoEndMovedUnitBeforeSwitch(Unit nextUnit)
        {
            if (selectedUnit == null || selectedUnit == nextUnit)
            {
                return true;
            }

            UnitTurnState previousTurnState = selectedUnit.GetComponent<UnitTurnState>();
            if (previousTurnState == null
                || previousTurnState.HasEnded
                || !previousTurnState.HasMoved
                || previousTurnState.HasActed)
            {
                return true;
            }

            if (turnManager == null)
            {
                turnManager = Object.FindFirstObjectByType<TurnManager>();
            }

            if (turnManager == null || turnManager.CurrentPhase != TurnPhase.PlayerTurn)
            {
                Debug.LogWarning("Selection switch could not auto-end the previous unit because TurnManager is missing or not in PlayerTurn.", this);
                return false;
            }

            Unit previousUnit = selectedUnit;
            if (!turnManager.TryEndPlayerUnitTurn(previousUnit))
            {
                Debug.LogWarning("Selection switch could not auto-end unit " + previousUnit.UnitId + ".", previousUnit);
                return false;
            }

            selectedUnit = null;
            return true;
        }
    }
}
