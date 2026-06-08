using BoneThrone.Audio;
using BoneThrone.Core;
using TMPro;
using UnityEngine;
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
        [SerializeField] private string returnToStartButtonText = "\u8FD4\u56DE\u4E3B\u9875";
        [SerializeField] private string retryButtonText = "\u91CD\u65B0\u5F00\u59CB";
        [SerializeField] private string quitButtonText = "\u8FD4\u56DE\u4E3B\u9875";
        [SerializeField] private string victoryTitle = "\u80DC\u5229";
        [SerializeField] private string defeatTitle = "\u5931\u8D25";
        [SerializeField] private string defaultVictoryReason = "\u961F\u4F0D\u53D6\u5F97\u4E86\u80DC\u5229\u3002";
        [SerializeField] private string defaultDefeatReason = "\u961F\u4F0D\u5168\u706D\u4E86\u3002";

        [Header("Replaceable Images")]
        [SerializeField] private Image panelBackgroundImage;
        [SerializeField] private Image frameImage;
        [SerializeField] private Image returnButtonImage;
        [SerializeField] private Image quitButtonImage;
        [SerializeField] private Sprite panelBackgroundSprite;
        [SerializeField] private Sprite frameSprite;
        [SerializeField] private Sprite returnButtonSprite;
        [SerializeField] private Sprite quitButtonSprite;

        [Header("Outcome Override Images")]
        [SerializeField] private Sprite victoryPanelBackgroundSprite;
        [SerializeField] private Sprite defeatPanelBackgroundSprite;
        [SerializeField] private Sprite victoryReturnButtonSprite;
        [SerializeField] private Sprite defeatRetryButtonSprite;
        [SerializeField] private Sprite defeatReturnHomeButtonSprite;

        [SerializeField] private bool debugLogging;

        private GameOutcome displayedOutcome = GameOutcome.None;
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
            RefreshButtonLabels(GameOutcome.None);
            SetPanelVisible(false);
        }

        private void OnEnable()
        {
            ResolveOutcomeService();
            Subscribe();
            BindButtons();
            RefreshButtonLabels(GameOutcome.None);
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
                displayedOutcome = GameOutcome.None;
                return;
            }

            displayedOutcome = outcome;
            bool victory = outcome == GameOutcome.Victory;
            ApplyOutcomeImages(outcome);
            SetText(titleText, victory ? victoryTitle : defeatTitle);
            SetText(reasonText, string.IsNullOrEmpty(reason) ? GetDefaultReason(outcome) : reason);
            SetOutcomeButtonVisibility(outcome);
            RefreshButtonLabels(outcome);
            SetPanelVisible(true);
            Log("Displayed outcome " + outcome + ".");
        }

        private void OnReturnToStartClicked()
        {
            BTAudioService.PlaySfx(BTAudioCueId.ButtonClick);
            BTOutcomePopupService.HideOutcome();

            if (displayedOutcome == GameOutcome.Defeat && outcomeService != null)
            {
                outcomeService.RequestRetry();
                return;
            }

            ReturnToStartMenu();
        }

        private void OnReturnHomeClicked()
        {
            BTAudioService.PlaySfx(BTAudioCueId.ButtonClick);
            BTOutcomePopupService.HideOutcome();
            ReturnToStartMenu();
        }

        private void ReturnToStartMenu()
        {
            if (!Application.CanStreamedLevelBeLoaded(startMenuSceneName))
            {
                Debug.LogWarning("GameResultPanelController cannot load start menu scene '" + startMenuSceneName + "' because it is not in Build Settings.", this);
                return;
            }

            SceneBlackCoverService.LoadSceneWithCover(startMenuSceneName);
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
                quitButton.onClick.RemoveListener(OnReturnHomeClicked);
                quitButton.onClick.AddListener(OnReturnHomeClicked);
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
                quitButton.onClick.RemoveListener(OnReturnHomeClicked);
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

        private void RefreshButtonLabels(GameOutcome outcome)
        {
            string primaryText = outcome == GameOutcome.Defeat ? retryButtonText : returnToStartButtonText;
            SetButtonLabel(returnToStartButton, primaryText);
            SetButtonLabel(quitButton, quitButtonText);
        }

        private void ApplyOutcomeImages(GameOutcome outcome)
        {
            Sprite nextPanelSprite = outcome == GameOutcome.Victory
                ? FirstAssigned(victoryPanelBackgroundSprite, panelBackgroundSprite)
                : FirstAssigned(defeatPanelBackgroundSprite, panelBackgroundSprite);

            Sprite nextPrimaryButtonSprite = outcome == GameOutcome.Defeat
                ? FirstAssigned(defeatRetryButtonSprite, returnButtonSprite)
                : FirstAssigned(victoryReturnButtonSprite, returnButtonSprite);

            Sprite nextQuitButtonSprite = outcome == GameOutcome.Defeat
                ? FirstAssigned(defeatReturnHomeButtonSprite, quitButtonSprite)
                : quitButtonSprite;

            ApplySprite(panelBackgroundImage, nextPanelSprite);
            ApplySprite(returnButtonImage, nextPrimaryButtonSprite);
            ApplySprite(quitButtonImage, nextQuitButtonSprite);
        }

        private void SetOutcomeButtonVisibility(GameOutcome outcome)
        {
            SetButtonVisible(returnToStartButton, true);
            SetButtonVisible(quitButton, outcome == GameOutcome.Defeat);
        }

        private static void ApplySprite(Image image, Sprite sprite)
        {
            if (image != null && sprite != null)
            {
                image.sprite = sprite;
                image.type = Image.Type.Simple;
            }
        }

        private static Sprite FirstAssigned(Sprite primary, Sprite fallback)
        {
            return primary != null ? primary : fallback;
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

        private static void SetButtonVisible(Button button, bool visible)
        {
            if (button != null && button.gameObject.activeSelf != visible)
            {
                button.gameObject.SetActive(visible);
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
