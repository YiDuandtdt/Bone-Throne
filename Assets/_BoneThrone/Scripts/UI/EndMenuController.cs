using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace BoneThrone.UI
{
    /// <summary>
    /// Minimal local end-menu controller for return-to-menu and quit actions.
    /// </summary>
    public sealed class EndMenuController : MonoBehaviour
    {
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text summaryText;
        [SerializeField] private Button returnToMenuButton;
        [SerializeField] private Button quitButton;
        [SerializeField] private string startMenuSceneName = "StartMenu";
        [SerializeField] private bool debugLogging;

        private void Awake()
        {
            if (titleText != null)
            {
                titleText.text = "王座静候";
            }

            if (summaryText != null)
            {
                summaryText.text = "本次冒险已结束。";
            }
        }

        private void OnEnable()
        {
            BindButtons();
        }

        private void OnDisable()
        {
            UnbindButtons();
        }

        private void BindButtons()
        {
            if (returnToMenuButton != null)
            {
                returnToMenuButton.onClick.RemoveListener(HandleReturnToMenuClicked);
                returnToMenuButton.onClick.AddListener(HandleReturnToMenuClicked);
            }

            if (quitButton != null)
            {
                quitButton.onClick.RemoveListener(HandleQuitClicked);
                quitButton.onClick.AddListener(HandleQuitClicked);
            }
        }

        private void UnbindButtons()
        {
            if (returnToMenuButton != null)
            {
                returnToMenuButton.onClick.RemoveListener(HandleReturnToMenuClicked);
            }

            if (quitButton != null)
            {
                quitButton.onClick.RemoveListener(HandleQuitClicked);
            }
        }

        private void HandleReturnToMenuClicked()
        {
            if (!Application.CanStreamedLevelBeLoaded(startMenuSceneName))
            {
                Debug.LogWarning("EndMenuController cannot load scene '" + startMenuSceneName + "' because it is not in Build Settings.", this);
                return;
            }

            Log("Loading scene '" + startMenuSceneName + "'.");
            SceneManager.LoadScene(startMenuSceneName, LoadSceneMode.Single);
        }

        private void HandleQuitClicked()
        {
            Log("Quit requested.");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void Log(string message)
        {
            if (debugLogging)
            {
                Debug.Log("EndMenuController: " + message, this);
            }
        }
    }
}
