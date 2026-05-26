using BoneThrone.Core;
using BoneThrone.Turns;
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

        public void Refresh(TurnManager turnManager)
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

            turnText.text = FormatTurnText(turnManager.CurrentPhase, turnManager.CurrentRole);
        }

        private static string FormatTurnText(TurnPhase phase, RoleId role)
        {
            if (phase == TurnPhase.EnemyTurn)
            {
                return "Turn: Enemy Turn";
            }

            if (phase == TurnPhase.PlayerTurn)
            {
                if (role == RoleId.None)
                {
                    return "Turn: Player Turn | Actor: Free Select";
                }

                return "Turn: Player Turn | Actor: " + role;
            }

            return "Turn: -- | Actor: --";
        }
    }
}
