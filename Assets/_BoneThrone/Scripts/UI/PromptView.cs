using BoneThrone.Interactables;
using BoneThrone.Levels;
using BoneThrone.Units;
using TMPro;
using UnityEngine;

namespace BoneThrone.UI
{
    /// <summary>
    /// Displays lightweight local prompts derived from readable gameplay state.
    /// </summary>
    public sealed class PromptView : MonoBehaviour
    {
        [SerializeField] private TMP_Text promptText;

        public void Bind(TMP_Text text)
        {
            promptText = text;
        }

        public void Refresh(Unit selectedUnit, LevelProgressionService progressionService, InteractableStairs stairs)
        {
            if (promptText == null)
            {
                return;
            }

            if (stairs != null && stairs.ConfirmationPending)
            {
                promptText.text = "Stairs: click again to enter next level.";
                return;
            }

            if (progressionService != null)
            {
                string reason;
                if (!progressionService.CanEnterNextLevel(out reason))
                {
                    promptText.text = "Progression: " + reason;
                    return;
                }
            }

            if (selectedUnit == null)
            {
                string stairsState = stairs == null ? " Stairs: Unbound." : string.Empty;
                promptText.text = "Select a player unit." + stairsState;
                return;
            }

            string displayName = string.IsNullOrEmpty(selectedUnit.DisplayName)
                ? selectedUnit.RoleId.ToString()
                : selectedUnit.DisplayName;
            promptText.text = "Selected: " + displayName + ".";
        }
    }
}
