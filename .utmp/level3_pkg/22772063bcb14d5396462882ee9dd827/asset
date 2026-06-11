using BoneThrone.Levels;
using UnityEngine;

namespace BoneThrone.Interactables
{
    /// <summary>
    /// Minimal boss-gate key pickup. It is separate from ordinary shared floor keys.
    /// </summary>
    public sealed class BossKeyItem : MonoBehaviour
    {
        [SerializeField] private BossGateProgressionState progressionState;
        [SerializeField] private bool consumeOnCollect = true;
        [SerializeField] private bool collected;
        [SerializeField] private bool debugLogging;

        private bool missingProgressionWarningLogged;

        public bool IsCollected
        {
            get { return collected; }
        }

        private void OnMouseDown()
        {
            TryCollect();
        }

        public bool TryCollect()
        {
            if (collected)
            {
                Log("Collection ignored because this boss key is already collected.");
                return false;
            }

            ResolveProgressionState();
            if (progressionState == null)
            {
                LogMissingProgressionWarning();
                return false;
            }

            bool collectedByState = progressionState.CollectBossKey();
            if (!collectedByState)
            {
                return false;
            }

            collected = true;
            Log("Boss key collected.");

            if (consumeOnCollect)
            {
                gameObject.SetActive(false);
            }

            return true;
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
            Debug.LogWarning("BossKeyItem cannot be collected because BossGateProgressionState is missing.", this);
        }

        private void Log(string message)
        {
            if (debugLogging)
            {
                Debug.Log("BossKeyItem: " + message, this);
            }
        }
    }
}
