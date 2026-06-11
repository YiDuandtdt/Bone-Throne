using BoneThrone.Core;
using BoneThrone.Turns;
using BoneThrone.Units;
using TMPro;
using UnityEngine;

namespace BoneThrone.UI
{
    /// <summary>
    /// Displays the current turn phase with UI-only fallbacks for local single-player selection.
    /// </summary>
    public sealed class TurnBannerView : MonoBehaviour
    {
        [SerializeField] private TMP_Text turnText;

        public void Bind(TMP_Text text)
        {
            turnText = text;
        }

        public void Refresh(TurnManager turnManager, Unit selectedUnit)
        {
            if (turnText == null)
            {
                return;
            }

            if (turnManager == null)
            {
                turnText.text = "Turn: Unbound";
                return;
            }

            turnText.text = FormatTurnText(turnManager.CurrentPhase, turnManager.CurrentRole, selectedUnit);
        }

        private static string FormatTurnText(TurnPhase phase, RoleId role, Unit selectedUnit)
        {
            if (phase == TurnPhase.EnemyTurn)
            {
                return "Turn: Enemy Turn";
            }

            if (phase == TurnPhase.PlayerTurn)
            {
                if (selectedUnit != null)
                {
                    string displayName = string.IsNullOrEmpty(selectedUnit.DisplayName)
                        ? selectedUnit.RoleId.ToString()
                        : selectedUnit.DisplayName;
                    return "Turn: Player Turn | Selected: " + displayName;
                }

                return "Turn: Player Turn - Select a unit";
            }

            return "Turn: -- | Actor: --";
        }
    }
}
