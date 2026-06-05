using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace BoneThrone.UI
{
    /// <summary>
    /// Minimal local start-menu controller for scene entry and quit.
    /// </summary>
    public sealed class StartMenuController : MonoBehaviour
    {
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text subtitleText;
        [SerializeField] private TMP_Text noteText;
        [SerializeField] private Button startButton;
        [SerializeField] private Button quitButton;
        [SerializeField] private string gameplaySceneName = "Level_1";
        [SerializeField] private bool debugLogging;

        private void Awake()
        {
            RefreshText();
        }

        private void OnEnable()
        {
            BindButtons();
        }

        private void OnDisable()
        {
            UnbindButtons();
        }

        private void RefreshText()
        {
            if (titleText != null)
            {
                titleText.text = "骸骨王座";
            }

            if (subtitleText != null)
            {
                subtitleText.text = "2.5D 回合制战术冒险";
            }

            if (noteText != null)
            {
                noteText.text = "当前入口场景：" + gameplaySceneName;
            }
        }

        private void BindButtons()
        {
            if (startButton != null)
            {
                startButton.onClick.RemoveListener(HandleStartClicked);
                startButton.onClick.AddListener(HandleStartClicked);
            }

            if (quitButton != null)
            {
                quitButton.onClick.RemoveListener(HandleQuitClicked);
                quitButton.onClick.AddListener(HandleQuitClicked);
            }
        }

        private void UnbindButtons()
        {
            if (startButton != null)
            {
                startButton.onClick.RemoveListener(HandleStartClicked);
            }

            if (quitButton != null)
            {
                quitButton.onClick.RemoveListener(HandleQuitClicked);
            }
        }

        private void HandleStartClicked()
        {
            if (!Application.CanStreamedLevelBeLoaded(gameplaySceneName))
            {
                Debug.LogWarning("StartMenuController cannot load scene '" + gameplaySceneName + "' because it is not in Build Settings.", this);
                return;
            }

            Log("Loading scene '" + gameplaySceneName + "'.");
            SceneManager.LoadScene(gameplaySceneName, LoadSceneMode.Single);
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
                Debug.Log("StartMenuController: " + message, this);
            }
        }
    }
}
