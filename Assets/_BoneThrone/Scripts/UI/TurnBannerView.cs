using BoneThrone.Core;
using BoneThrone.Turns;
using TMPro;
using UnityEngine;

namespace BoneThrone.UI
{
    /// <summary>
    /// Displays the current turn phase, role reservation, and turn index.
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

            TurnPhase phase = turnManager.CurrentPhase;
            RoleId role = turnManager.CurrentRole;
            int index = turnManager.CurrentTurnIndex;
            turnText.text = "Turn: " + phase + " | Role: " + role + " | Index: " + index;
        }
    }
}
