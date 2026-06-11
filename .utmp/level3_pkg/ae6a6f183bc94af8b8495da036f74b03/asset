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

        private string overrideText;
        private float overrideExpiresAt = -1f;

        public void Bind(TMP_Text text)
        {
            promptText = text;
        }

        public void ShowOverride(string message)
        {
            ShowOverride(message, 0f);
        }

        public void ShowOverride(string message, float duration)
        {
            overrideText = message;
            overrideExpiresAt = duration > 0f ? Time.time + duration : -1f;
            SetPrompt(message);
        }

        public void ClearOverride()
        {
            overrideText = null;
            overrideExpiresAt = -1f;
        }

        public void Refresh(Unit selectedUnit, LevelProgressionService progressionService, InteractableStairs stairs)
        {
            if (promptText == null)
            {
                return;
            }

            if (!string.IsNullOrEmpty(overrideText))
            {
                if (overrideExpiresAt <= 0f || Time.time < overrideExpiresAt)
                {
                    promptText.text = overrideText;
                    return;
                }

                ClearOverride();
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

        private void SetPrompt(string message)
        {
            if (promptText != null)
            {
                promptText.text = message;
            }
        }
    }
}
