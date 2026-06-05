using BoneThrone.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BoneThrone.UI
{
    /// <summary>
    /// Minimal local result panel bridge. Retry is an event request only; no scene reload is performed here.
    /// </summary>
    public sealed class GameResultPanelController : MonoBehaviour
    {
        [SerializeField] private GameOutcomeService outcomeService;
        [SerializeField] private GameObject root;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text reasonText;
        [SerializeField] private Button retryButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private string victoryTitle = "胜利";
        [SerializeField] private string defeatTitle = "失败";
        [SerializeField] private string defaultVictoryReason = "队伍取得了胜利。";
        [SerializeField] private string defaultDefeatReason = "队伍全灭了。";
        [SerializeField] private bool debugLogging;

        private bool missingOutcomeServiceWarningLogged;
        private bool subscribed;

        private void Awake()
        {
            if (root == null)
            {
                root = gameObject;
            }

            SetPanelVisible(false);
        }

        private void OnEnable()
        {
            ResolveOutcomeService();
            Subscribe();
            BindButtons();
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

        private void OnRetryClicked()
        {
            ResolveOutcomeService();
            if (outcomeService != null)
            {
                outcomeService.RequestRetry();
            }
        }

        private void OnCloseClicked()
        {
            SetPanelVisible(false);
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
            if (retryButton != null)
            {
                retryButton.onClick.RemoveListener(OnRetryClicked);
                retryButton.onClick.AddListener(OnRetryClicked);
            }

            if (closeButton != null)
            {
                closeButton.onClick.RemoveListener(OnCloseClicked);
                closeButton.onClick.AddListener(OnCloseClicked);
            }
        }

        private void UnbindButtons()
        {
            if (retryButton != null)
            {
                retryButton.onClick.RemoveListener(OnRetryClicked);
            }

            if (closeButton != null)
            {
                closeButton.onClick.RemoveListener(OnCloseClicked);
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
