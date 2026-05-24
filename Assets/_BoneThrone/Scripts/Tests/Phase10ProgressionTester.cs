using BoneThrone.Interactables;
using BoneThrone.Levels;
using UnityEngine;

namespace BoneThrone.Tests
{
    /// <summary>
    /// Temporary Phase 10 ContextMenu helper for validating key pickup, stairs confirmation,
    /// placeholder level switching, and party level-up without production UI.
    /// </summary>
    public sealed class Phase10ProgressionTester : MonoBehaviour
    {
        [SerializeField] private LevelProgressionService progressionService;
        [SerializeField] private KeyItem keyItem;
        [SerializeField] private InteractableStairs stairs;

        [ContextMenu("Phase 10/Collect Key Test")]
        public void CollectKeyTest()
        {
            if (keyItem != null)
            {
                keyItem.CollectForTest();
                LogProgressionState();
                return;
            }

            if (progressionService == null)
            {
                Debug.LogWarning("Phase10ProgressionTester cannot collect key because both KeyItem and LevelProgressionService are missing.", this);
                return;
            }

            progressionService.CollectSharedKey(this);
            LogProgressionState();
        }

        [ContextMenu("Phase 10/Try Stairs First Click Test")]
        public void TryStairsTest()
        {
            if (!HasStairs())
            {
                return;
            }

            stairs.TryRequestEnterNextLevel(null);
            LogProgressionState();
        }

        [ContextMenu("Phase 10/Confirm Stairs Test")]
        public void ConfirmStairsTest()
        {
            if (!HasStairs())
            {
                return;
            }

            stairs.ConfirmEnterNextLevelForTest();
            LogProgressionState();
        }

        [ContextMenu("Phase 10/Force Next Level Test")]
        public void ForceNextLevelTest()
        {
            if (!HasProgressionService())
            {
                return;
            }

            progressionService.CollectSharedKey(this);
            progressionService.TryEnterNextLevel();
            LogProgressionState();
        }

        [ContextMenu("Phase 10/Log Progression State")]
        public void LogProgressionState()
        {
            if (!HasProgressionService())
            {
                return;
            }

            string reason;
            bool canEnter = progressionService.CanEnterNextLevel(out reason);
            Debug.Log(
                "Phase10ProgressionTester: HasSharedKey="
                + progressionService.HasSharedKey
                + " CurrentLevelIndex="
                + progressionService.CurrentLevelIndex
                + " CanEnterNextLevel="
                + canEnter
                + " Reason="
                + reason
                + ".",
                this);
        }

        [ContextMenu("Phase 10/Reset Phase 10 Test")]
        public void ResetPhase10Test()
        {
            if (!HasProgressionService())
            {
                return;
            }

            progressionService.ResetProgressionForTest();
            if (stairs != null)
            {
                stairs.CancelConfirmation();
            }

            LogProgressionState();
        }

        private bool HasProgressionService()
        {
            if (progressionService != null)
            {
                return true;
            }

            Debug.LogWarning("Phase10ProgressionTester needs a LevelProgressionService reference.", this);
            return false;
        }

        private bool HasStairs()
        {
            if (stairs != null)
            {
                return true;
            }

            Debug.LogWarning("Phase10ProgressionTester needs an InteractableStairs reference.", this);
            return false;
        }
    }
}
