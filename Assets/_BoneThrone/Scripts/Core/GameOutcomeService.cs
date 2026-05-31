using System;
using UnityEngine;

namespace BoneThrone.Core
{
    /// <summary>
    /// Minimal local result-state broadcaster. It does not reload scenes or reset gameplay state.
    /// </summary>
    public sealed class GameOutcomeService : MonoBehaviour
    {
        [SerializeField] private GameOutcome currentOutcome = GameOutcome.None;
        [SerializeField] private string lastReason;
        [SerializeField] private bool debugLogging;

        public event Action<GameOutcome, string> OutcomeChanged;
        public event Action RetryRequested;

        public GameOutcome CurrentOutcome
        {
            get { return currentOutcome; }
        }

        public bool HasOutcome
        {
            get { return currentOutcome != GameOutcome.None; }
        }

        public string LastReason
        {
            get { return lastReason; }
        }

        public bool SetVictory(string reason = null)
        {
            return SetOutcome(GameOutcome.Victory, reason);
        }

        public bool SetDefeat(string reason = null)
        {
            return SetOutcome(GameOutcome.Defeat, reason);
        }

        public void ClearOutcome()
        {
            currentOutcome = GameOutcome.None;
            lastReason = null;
            Log("Outcome cleared.");
            OutcomeChanged?.Invoke(currentOutcome, lastReason);
        }

        public void RequestRetry()
        {
            Log("Retry requested.");
            RetryRequested?.Invoke();
        }

        private bool SetOutcome(GameOutcome outcome, string reason)
        {
            if (HasOutcome)
            {
                Log("Outcome ignored because one is already set: " + currentOutcome + ".");
                return false;
            }

            currentOutcome = outcome;
            lastReason = reason;
            Log("Outcome set to " + currentOutcome + ".");
            OutcomeChanged?.Invoke(currentOutcome, lastReason);
            return true;
        }

        private void Log(string message)
        {
            if (debugLogging)
            {
                Debug.Log("GameOutcomeService: " + message, this);
            }
        }
    }
}
