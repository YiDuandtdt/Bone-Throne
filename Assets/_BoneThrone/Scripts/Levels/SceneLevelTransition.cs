using UnityEngine;
using UnityEngine.SceneManagement;

namespace BoneThrone.Levels
{
    /// <summary>
    /// Minimal scene-based floor transition bridge.
    /// It only loads an explicitly configured next scene after progression validation.
    /// </summary>
    public sealed class SceneLevelTransition : MonoBehaviour
    {
        [SerializeField] private string nextSceneName;
        [SerializeField] private LoadSceneMode loadSceneMode = LoadSceneMode.Single;
        [SerializeField] private bool debugLogging = true;

        public string NextSceneName
        {
            get { return nextSceneName; }
        }

        public bool HasNextScene
        {
            get { return !string.IsNullOrEmpty(nextSceneName); }
        }

        public bool CanLoadNextScene(out string reason)
        {
            if (string.IsNullOrEmpty(nextSceneName))
            {
                reason = "Next scene name is not configured.";
                return false;
            }

            if (!Application.CanStreamedLevelBeLoaded(nextSceneName))
            {
                reason = "Next scene '" + nextSceneName + "' is not available in Build Settings.";
                return false;
            }

            reason = "Next scene can be loaded.";
            return true;
        }

        public bool TryLoadNextScene()
        {
            string reason;
            if (!CanLoadNextScene(out reason))
            {
                Debug.LogWarning("Scene transition rejected: " + reason, this);
                return false;
            }

            Log("Loading next scene '" + nextSceneName + "'.");
            SceneManager.LoadScene(nextSceneName, loadSceneMode);
            return true;
        }

        [ContextMenu("Levels/Load Next Scene For Test")]
        public void LoadNextSceneForTest()
        {
            TryLoadNextScene();
        }

        private void Log(string message)
        {
            if (debugLogging)
            {
                Debug.Log("SceneLevelTransition: " + message, this);
            }
        }
    }
}
