using BoneThrone.Turns;
using BoneThrone.Units;
using TMPro;
using UnityEngine;

namespace BoneThrone.UI
{
    /// <summary>
    /// Displays a single player unit's readable battle status.
    /// </summary>
    public sealed class HeroPanelView : MonoBehaviour
    {
        [SerializeField] private TMP_Text statusText;

        public void Bind(TMP_Text text)
        {
            statusText = text;
        }

        public void Refresh(Unit unit)
        {
            Refresh(unit, null);
        }

        public void Refresh(Unit unit, Unit selectedUnit)
        {
            if (statusText == null)
            {
                return;
            }

            if (unit == null)
            {
                statusText.text = "Hero: Unbound\nHP: -- / --\nLevel: --\nMoved: -- | Acted: --";
                return;
            }

            string displayName = string.IsNullOrEmpty(unit.DisplayName) ? unit.RoleId.ToString() : unit.DisplayName;
            string currentHp = unit.RuntimeState != null ? unit.RuntimeState.CurrentHp.ToString() : "--";
            string maxHp = unit.Stats != null ? unit.Stats.GetClampedMaxHp().ToString() : "--";
            string level = unit.Stats != null ? unit.Stats.Level.ToString() : "--";
            UnitTurnState turnState = unit.GetComponent<UnitTurnState>();
            string moved = turnState != null ? turnState.HasMoved.ToString() : "--";
            string acted = turnState != null ? turnState.HasActed.ToString() : "--";
            string turnStatus = FormatTurnStatus(unit, selectedUnit, turnState);
            string life = unit.IsAlive ? "Alive" : "Dead";

            statusText.text = displayName
                + "\nHP: " + currentHp + " / " + maxHp
                + "\nLevel: " + level
                + "\nMoved: " + moved + " | Acted: " + acted
                + "\nTurn: " + turnStatus
                + "\nState: " + life;
        }

        private static string FormatTurnStatus(Unit unit, Unit selectedUnit, UnitTurnState turnState)
        {
            if (unit == null || turnState == null)
            {
                return "--";
            }

            if (turnState.HasEnded)
            {
                return "Ended";
            }

            if (unit == selectedUnit || turnState.HasMoved || turnState.HasActed)
            {
                return "Active";
            }

            return "Not Started";
        }
    }
}
