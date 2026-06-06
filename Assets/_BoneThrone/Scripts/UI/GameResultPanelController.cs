using BoneThrone.Audio;
using BoneThrone.Core;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace BoneThrone.UI
{
    /// <summary>
    /// Local result panel bridge for victory/defeat popups.
    /// </summary>
    public sealed class GameResultPanelController : MonoBehaviour
    {
        [SerializeField] private GameOutcomeService outcomeService;
        [SerializeField] private GameObject root;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text reasonText;
        [SerializeField] [FormerlySerializedAs("retryButton")] private Button returnToStartButton;
        [SerializeField] [FormerlySerializedAs("closeButton")] private Button quitButton;
        [SerializeField] private string startMenuSceneName = "StartMenu";
        [SerializeField] private string returnToStartButtonText = "返回开始";
        [SerializeField] private string quitButtonText = "退出游戏";
        [SerializeField] private string victoryTitle = "胜利";
        [SerializeField] private string defeatTitle = "失败";
        [SerializeField] private string defaultVictoryReason = "队伍取得了胜利。";
        [SerializeField] private string defaultDefeatReason = "队伍全灭了。";

        [Header("Replaceable Images")]
        [SerializeField] private Image panelBackgroundImage;
        [SerializeField] private Image frameImage;
        [SerializeField] private Image returnButtonImage;
        [SerializeField] private Image quitButtonImage;
        [SerializeField] private Sprite panelBackgroundSprite;
        [SerializeField] private Sprite frameSprite;
        [SerializeField] private Sprite returnButtonSprite;
        [SerializeField] private Sprite quitButtonSprite;

        [SerializeField] private bool debugLogging;

        private bool missingOutcomeServiceWarningLogged;
        private bool subscribed;

        private void Awake()
        {
            if (root == null)
            {
                root = gameObject;
            }

            ResolveReplaceableImages();
            ApplyReplaceableImages();
            RefreshButtonLabels();
            SetPanelVisible(false);
        }

        private void OnEnable()
        {
            ResolveOutcomeService();
            Subscribe();
            BindButtons();
            RefreshButtonLabels();
            RefreshFromCurrentOutcome();
        }

        private void OnDisable()
        {
            Unsubscribe();
            UnbindButtons();
        }

        private void OnOutcomeChanged(GameOutcome outcome, string reason)
        {
            if (outcome == GameOutcome.None)
            {
                SetPanelVisible(false);
                return;
            }

            bool victory = outcome == GameOutcome.Victory;
            SetText(titleText, victory ? victoryTitle : defeatTitle);
            SetText(reasonText, string.IsNullOrEmpty(reason) ? GetDefaultReason(outcome) : reason);
            SetPanelVisible(true);
            Log("Displayed outcome " + outcome + ".");
        }

        private void OnReturnToStartClicked()
        {
            BTAudioService.PlaySfx(BTAudioCueId.ButtonClick);
            BTOutcomePopupService.HideOutcome();

            if (!Application.CanStreamedLevelBeLoaded(startMenuSceneName))
            {
                Debug.LogWarning("GameResultPanelController cannot load start menu scene '" + startMenuSceneName + "' because it is not in Build Settings.", this);
                return;
            }

            SceneManager.LoadScene(startMenuSceneName, LoadSceneMode.Single);
        }

        private void OnQuitClicked()
        {
            BTAudioService.PlaySfx(BTAudioCueId.ButtonClick);
            Log("Quit requested.");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void RefreshFromCurrentOutcome()
        {
            if (outcomeService == null)
            {
                LogMissingOutcomeServiceWarning();
                SetPanelVisible(false);
                return;
            }

            OnOutcomeChanged(outcomeService.CurrentOutcome, outcomeService.LastReason);
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

            outcomeService.OutcomeChanged += OnOutcomeChanged;
            subscribed = true;
        }

        private void Unsubscribe()
        {
            if (!subscribed || outcomeService == null)
            {
                return;
            }

            outcomeService.OutcomeChanged -= OnOutcomeChanged;
            subscribed = false;
        }

        private void BindButtons()
        {
            if (returnToStartButton != null)
            {
                returnToStartButton.onClick.RemoveListener(OnReturnToStartClicked);
                returnToStartButton.onClick.AddListener(OnReturnToStartClicked);
            }

            if (quitButton != null)
            {
                quitButton.onClick.RemoveListener(OnQuitClicked);
                quitButton.onClick.AddListener(OnQuitClicked);
            }
        }

        private void UnbindButtons()
        {
            if (returnToStartButton != null)
            {
                returnToStartButton.onClick.RemoveListener(OnReturnToStartClicked);
            }

            if (quitButton != null)
            {
                quitButton.onClick.RemoveListener(OnQuitClicked);
            }
        }

        private void SetPanelVisible(bool visible)
        {
            if (root != null)
            {
                root.SetActive(visible);
            }
        }

        private string GetDefaultReason(GameOutcome outcome)
        {
            return outcome == GameOutcome.Victory ? defaultVictoryReason : defaultDefeatReason;
        }

        private void ResolveReplaceableImages()
        {
            if (panelBackgroundImage == null && root != null)
            {
                panelBackgroundImage = root.GetComponent<Image>();
            }

            if (returnButtonImage == null && returnToStartButton != null)
            {
                returnButtonImage = returnToStartButton.targetGraphic as Image;
            }

            if (quitButtonImage == null && quitButton != null)
            {
                quitButtonImage = quitButton.targetGraphic as Image;
            }
        }

        private void ApplyReplaceableImages()
        {
            ApplySprite(panelBackgroundImage, panelBackgroundSprite);
            ApplySprite(frameImage, frameSprite);
            ApplySprite(returnButtonImage, returnButtonSprite);
            ApplySprite(quitButtonImage, quitButtonSprite);
        }

        private void RefreshButtonLabels()
        {
            SetButtonLabel(returnToStartButton, returnToStartButtonText);
            SetButtonLabel(quitButton, quitButtonText);
        }

        private static void ApplySprite(Image image, Sprite sprite)
        {
            if (image != null && sprite != null)
            {
                image.sprite = sprite;
                image.type = Image.Type.Sliced;
            }
        }

        private static void SetButtonLabel(Button button, string value)
        {
            if (button == null || string.IsNullOrEmpty(value))
            {
                return;
            }

            TMP_Text label = button.GetComponentInChildren<TMP_Text>(true);
            if (label != null)
            {
                label.text = value;
            }
        }

        private static void SetText(TMP_Text text, string value)
        {
            if (text != null)
            {
                text.text = value;
            }
        }

        private void LogMissingOutcomeServiceWarning()
        {
            if (missingOutcomeServiceWarningLogged)
            {
                return;
            }

            missingOutcomeServiceWarningLogged = true;
            Debug.LogWarning("GameResultPanelController cannot display outcomes because GameOutcomeService is missing.", this);
        }

        private void Log(string message)
        {
            if (debugLogging)
            {
                Debug.Log("GameResultPanelController: " + message, this);
            }
        }
    }
}
