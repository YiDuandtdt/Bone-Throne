using System.Collections;
using BoneThrone.Audio;
using BoneThrone.Levels;
using BoneThrone.Movement;
using BoneThrone.Units;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BoneThrone.Interactables
{
    /// <summary>
    /// Minimal Phase 10 stairs interaction with hover material feedback and two-click confirmation.
    /// It does not implement a formal UI confirmation panel, scene loading, boss doors, or networking.
    /// </summary>
    public sealed class InteractableStairs : MonoBehaviour
    {
        [SerializeField] private LevelProgressionService progressionService;
        [SerializeField] private SelectionManager selectionManager;
        [SerializeField] private Renderer[] feedbackRenderers;
        [SerializeField] private Material normalMaterial;
        [SerializeField] private Material hoverMaterial;
        [SerializeField] private bool requireSecondClickConfirmation = true;

        private bool confirmationPending;

        public bool ConfirmationPending
        {
            get { return confirmationPending; }
        }

        private void OnMouseEnter()
        {
            ApplyHoverFeedback(true);
        }

        private void OnMouseExit()
        {
            ApplyHoverFeedback(false);
        }

        private void OnMouseDown()
        {
            Unit interactor = selectionManager != null ? selectionManager.SelectedUnit : null;
            TryRequestEnterNextLevel(interactor);
        }

        public bool TryRequestEnterNextLevel(Unit interactor)
        {
            if (interactor != null && (interactor.Faction != UnitFaction.Player || !interactor.IsAlive))
            {
                Debug.LogWarning("Stairs interaction rejected because the interactor is not a living player unit.", interactor);
                return false;
            }

            if (progressionService == null)
            {
                Debug.LogWarning("Stairs interaction failed because LevelProgressionService is missing.", this);
                return false;
            }

            string reason;
            if (!progressionService.CanEnterNextLevel(out reason))
            {
                confirmationPending = false;
                Debug.LogWarning("Stairs interaction rejected: " + reason, this);
                return false;
            }

            if (requireSecondClickConfirmation && !confirmationPending)
            {
                confirmationPending = true;
                Debug.Log("Stairs confirmation pending. Click stairs again or use ContextMenu confirm to enter next level.", this);
                return false;
            }

            return ConfirmEnterNextLevelForTest();
        }

        [ContextMenu("Phase 10/Confirm Enter Next Level")]
        public void ConfirmEnterNextLevelContextMenu()
        {
            ConfirmEnterNextLevelForTest();
        }

        public bool ConfirmEnterNextLevelForTest()
        {
            if (progressionService == null)
            {
                Debug.LogWarning("Cannot confirm stairs because LevelProgressionService is missing.", this);
                return false;
            }

            string sceneBefore = SceneManager.GetActiveScene().name;
            BTAudioService.PlayLoop(BTAudioCueId.StairsLoop, this);
            bool entered = progressionService.TryEnterNextLevel();
            confirmationPending = false;
            if (!entered)
            {
                BTAudioService.StopLoop(this);
            }
            else
            {
                StartCoroutine(StopStairsLoopIfSceneDidNotChange(sceneBefore));
            }

            Debug.Log("Stairs confirm result: " + entered + ".", this);
            return entered;
        }

        private IEnumerator StopStairsLoopIfSceneDidNotChange(string sceneBefore)
        {
            yield return new WaitForSeconds(1f);
            if (SceneManager.GetActiveScene().name == sceneBefore)
            {
                BTAudioService.StopLoop(this);
            }
        }

        [ContextMenu("Phase 10/Cancel Pending Confirmation")]
        public void CancelConfirmation()
        {
            confirmationPending = false;
            Debug.Log("Stairs confirmation cancelled.", this);
        }

        private void ApplyHoverFeedback(bool isHovering)
        {
            if (feedbackRenderers == null || feedbackRenderers.Length == 0)
            {
                return;
            }

            Material targetMaterial = isHovering ? hoverMaterial : normalMaterial;
            if (targetMaterial == null)
            {
                return;
            }

            for (int i = 0; i < feedbackRenderers.Length; i++)
            {
                if (feedbackRenderers[i] != null)
                {
                    feedbackRenderers[i].material = targetMaterial;
                }
            }
        }
    }
}
