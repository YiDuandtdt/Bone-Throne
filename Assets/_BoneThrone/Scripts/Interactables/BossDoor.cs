using BoneThrone.Levels;
using UnityEngine;

namespace BoneThrone.Interactables
{
    /// <summary>
    /// Minimal boss gate component. It only checks boss-key state and toggles local blockers / visuals.
    /// </summary>
    public sealed class BossDoor : MonoBehaviour
    {
        [SerializeField] private BossGateProgressionState progressionState;
        [SerializeField] private Collider doorBlocker;
        [SerializeField] private GameObject lockedVisual;
        [SerializeField] private GameObject openedVisual;
        [SerializeField] private bool opened;
        [SerializeField] private bool debugLogging;

        private bool missingProgressionWarningLogged;

        public bool IsOpened
        {
            get { return opened; }
        }

        private void Awake()
        {
            SetOpenedVisual(opened);
        }

        private void OnMouseDown()
        {
            TryOpen();
        }

        public bool CanOpen()
        {
            ResolveProgressionState();
            if (progressionState == null)
            {
                LogMissingProgressionWarning();
                return false;
            }

            return progressionState.HasBossKey;
        }

        public bool TryOpen()
        {
            if (opened)
            {
                Log("Open ignored because the boss door is already open.");
                return false;
            }

            if (!CanOpen())
            {
                Log("Open rejected because boss key requirements are not met.");
                return false;
            }

            if (!progressionState.OpenBossDoor())
            {
                return false;
            }

            opened = true;
            SetOpenedVisual(true);
            Log("Boss door opened.");
            return true;
        }

        public void SetOpenedVisual(bool isOpened)
        {
            if (doorBlocker != null)
            {
                doorBlocker.enabled = !isOpened;
            }

            if (lockedVisual != null)
            {
                lockedVisual.SetActive(!isOpened);
            }

            if (openedVisual != null)
            {
                openedVisual.SetActive(isOpened);
            }
        }

        private void ResolveProgressionState()
        {
            if (progressionState == null)
            {
                progressionState = Object.FindFirstObjectByType<BossGateProgressionState>();
            }
        }

        private void LogMissingProgressionWarning()
        {
            if (missingProgressionWarningLogged)
            {
                return;
            }

            missingProgressionWarningLogged = true;
            Debug.LogWarning("BossDoor cannot open because BossGateProgressionState is missing.", this);
        }

        private void Log(string message)
        {
            if (debugLogging)
            {
                Debug.Log("BossDoor: " + message, this);
            }
        }
    }
}
