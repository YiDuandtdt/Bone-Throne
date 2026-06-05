using BoneThrone.Core;
using BoneThrone.Levels;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BoneThrone.UI
{
    /// <summary>
    /// Local retry executor that reloads the active scene after a retry request.
    /// </summary>
    public sealed class RetryCurrentSceneController : MonoBehaviour
    {
        [SerializeField] private GameOutcomeService outcomeService;
        [SerializeField] private bool clearPartyProgressionStateOnRetry = true;
        [SerializeField] private bool debugLogging;

        private bool subscribed;

        private void OnEnable()
        {
            ResolveOutcomeService();
            Subscribe();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        private void ResolveOutcomeService()
        {
            if (outcomeService == null)
            {
                outcomeService = Object.FindFirstObjectByType<GameOutcomeService>();
            }
        }

        private void Subscribe()
        {
            if (subscribed || outcomeService == null)
            {
                return;
            }

            outcomeService.RetryRequested += HandleRetryRequested;
            subscribed = true;
        }

        private void Unsubscribe()
        {
            if (!subscribed || outcomeService == null)
            {
                return;
            }

            outcomeService.RetryRequested -= HandleRetryRequested;
            subscribed = false;
        }

        private void HandleRetryRequested()
        {
            if (clearPartyProgressionStateOnRetry)
            {
                PartyProgressionState.Clear();
            }

            Scene activeScene = SceneManager.GetActiveScene();
            Log("Reloading active scene '" + activeScene.name + "'.");
            SceneManager.LoadScene(activeScene.name, LoadSceneMode.Single);
        }

        private void Log(string message)
        {
            if (debugLogging)
            {
                Debug.Log("RetryCurrentSceneController: " + message, this);
            }
        }
    }
}
