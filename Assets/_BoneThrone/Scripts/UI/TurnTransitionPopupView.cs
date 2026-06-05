using System.Collections;
using TMPro;
using UnityEngine;

namespace BoneThrone.UI
{
    /// <summary>
    /// Plays a lightweight centered popup for player/enemy round transitions.
    /// </summary>
    public sealed class TurnTransitionPopupView : MonoBehaviour
    {
        [SerializeField] private TMP_Text messageText;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private RectTransform animatedRoot;
        [SerializeField] private TurnPacingSettings pacingSettings;
        [SerializeField] private string enemyTurnText = "敌方回合";
        [SerializeField] private string playerTurnText = "我方回合";

        private Vector2 restingAnchoredPosition;

        public TurnPacingSettings PacingSettings => pacingSettings;

        private void Awake()
        {
            ResolveBindings();
            HideImmediate();
        }

        public void Bind(TMP_Text text, CanvasGroup group, RectTransform root)
        {
            messageText = text;
            canvasGroup = group;
            animatedRoot = root;
            ResolveBindings();
            HideImmediate();
        }

        public void SetPacingSettings(TurnPacingSettings settings)
        {
            pacingSettings = settings;
        }

        public IEnumerator PlayEnemyTurnAnnouncement()
        {
            yield return PlayAnnouncement(enemyTurnText, GetEnemyHoldDuration());
        }

        public IEnumerator PlayPlayerTurnAnnouncement()
        {
            yield return PlayAnnouncement(playerTurnText, GetPlayerHoldDuration());
        }

        public void HideImmediate()
        {
            ResolveBindings();
            if (canvasGroup == null || animatedRoot == null)
            {
                return;
            }

            restingAnchoredPosition = animatedRoot.anchoredPosition;
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            animatedRoot.anchoredPosition = restingAnchoredPosition;
        }

        private IEnumerator PlayAnnouncement(string message, float holdDuration)
        {
            ResolveBindings();
            if (canvasGroup == null || animatedRoot == null || messageText == null)
            {
                yield break;
            }

            messageText.text = message;
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            float drift = GetPopupDriftDistance();
            Vector2 enterPosition = restingAnchoredPosition + Vector2.down * drift;
            Vector2 exitPosition = restingAnchoredPosition + Vector2.up * drift;
            animatedRoot.anchoredPosition = enterPosition;

            yield return Animate(enterPosition, restingAnchoredPosition, 0f, 1f, GetPopupFadeInDuration());

            if (holdDuration > 0f)
            {
                yield return new WaitForSeconds(holdDuration);
            }

            yield return Animate(restingAnchoredPosition, exitPosition, 1f, 0f, GetPopupFadeOutDuration());
            animatedRoot.anchoredPosition = restingAnchoredPosition;
            canvasGroup.alpha = 0f;
        }

        private IEnumerator Animate(Vector2 fromPosition, Vector2 toPosition, float fromAlpha, float toAlpha, float duration)
        {
            if (duration <= 0f)
            {
                animatedRoot.anchoredPosition = toPosition;
                canvasGroup.alpha = toAlpha;
                yield break;
            }

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float eased = 1f - Mathf.Pow(1f - t, 3f);
                animatedRoot.anchoredPosition = Vector2.LerpUnclamped(fromPosition, toPosition, eased);
                canvasGroup.alpha = Mathf.LerpUnclamped(fromAlpha, toAlpha, eased);
                yield return null;
            }

            animatedRoot.anchoredPosition = toPosition;
            canvasGroup.alpha = toAlpha;
        }

        private void ResolveBindings()
        {
            if (animatedRoot == null)
            {
                animatedRoot = GetComponent<RectTransform>();
            }

            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
            }

            if (messageText == null)
            {
                messageText = GetComponentInChildren<TMP_Text>(true);
            }

            if (animatedRoot != null)
            {
                restingAnchoredPosition = animatedRoot.anchoredPosition;
            }
        }

        private float GetEnemyHoldDuration()
        {
            return pacingSettings != null ? pacingSettings.EnemyTurnBannerHoldDuration : 0.85f;
        }

        private float GetPlayerHoldDuration()
        {
            return pacingSettings != null ? pacingSettings.PlayerTurnBannerHoldDuration : 0.85f;
        }

        private float GetPopupFadeInDuration()
        {
            return pacingSettings != null ? pacingSettings.PopupFadeInDuration : 0.14f;
        }

        private float GetPopupFadeOutDuration()
        {
            return pacingSettings != null ? pacingSettings.PopupFadeOutDuration : 0.18f;
        }

        private float GetPopupDriftDistance()
        {
            return pacingSettings != null ? pacingSettings.PopupDriftDistance : 22f;
        }
    }
}
