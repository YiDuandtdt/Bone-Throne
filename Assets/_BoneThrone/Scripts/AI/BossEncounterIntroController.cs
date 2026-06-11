using System.Collections;
using System.Collections.Generic;
using BoneThrone.Audio;
using BoneThrone.Rooms;
using BoneThrone.Units;
using UnityEngine;
using UnityEngine.UI;

namespace BoneThrone.AI
{
    /// <summary>
    /// Plays the one-time boss reveal before the first enemy action in an entered boss room.
    /// </summary>
    public sealed class BossEncounterIntroController : MonoBehaviour
    {
        private static readonly HashSet<int> RevealedBossIds = new HashSet<int>();

        [SerializeField] private bool playIntro = true;
        [SerializeField] private Camera targetCamera;
        [SerializeField] private Vector3 revealCameraOffset = new Vector3(-8f, 9f, -8f);
        [SerializeField] private Vector3 revealLookOffset = new Vector3(0f, 1.2f, 0f);
        [SerializeField] [Min(0f)] private float blackHoldBeforeReveal = 0.12f;
        [SerializeField] [Min(0f)] private float blackFadeOutDuration = 0.35f;
        [SerializeField] [Min(0f)] private float bossRevealHoldDuration = 2.65f;
        [SerializeField] [Min(0f)] private float cameraReturnDuration = 2.75f;
        [SerializeField] [Min(0.1f)] private float revealOrthographicSize = 5.5f;

        private Canvas overlayCanvas;
        private Image blackImage;
        private bool introPlayed;
        private bool introRunning;
        private Unit bossToSkipThisEnemyTurn;
        private Coroutine cameraReturnRoutine;

        public static bool HasRevealedActiveBossEncounter()
        {
            if (RevealedBossIds.Count == 0)
            {
                return false;
            }

            Unit[] units = Object.FindObjectsByType<Unit>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            for (int i = 0; i < units.Length; i++)
            {
                Unit unit = units[i];
                if (unit != null
                    && unit.gameObject.activeInHierarchy
                    && unit.IsAlive
                    && BossEnemyAIController.IsBossLikeUnit(unit)
                    && RevealedBossIds.Contains(unit.GetInstanceID()))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool IsBossEncounterRevealed(Unit boss)
        {
            return boss != null && RevealedBossIds.Contains(boss.GetInstanceID());
        }

        public IEnumerator PlayIntroIfNeeded(IReadOnlyList<Unit> activeEnemies)
        {
            Unit boss;
            if (!ShouldPlayIntro(activeEnemies, out boss))
            {
                yield break;
            }

            introPlayed = true;
            introRunning = true;
            bossToSkipThisEnemyTurn = boss;
            RevealedBossIds.Add(boss.GetInstanceID());
            BTAudioService.PlayBgm(BTAudioCueId.BgmBoss);

            yield return PlayCameraRevealRoutine(boss);
            introRunning = false;
        }

        public bool ConsumeBossActionSkipThisTurn(Unit boss)
        {
            if (boss == null || bossToSkipThisEnemyTurn != boss)
            {
                return false;
            }

            bossToSkipThisEnemyTurn = null;
            return true;
        }

        public static bool IsAnyEnteredBossRoomActive()
        {
            RoomController[] rooms = Object.FindObjectsByType<RoomController>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int i = 0; i < rooms.Length; i++)
            {
                RoomController room = rooms[i];
                if (room == null || room.EnemyActivator == null || !room.EnemyActivator.HasConfiguredBossLikeEnemy)
                {
                    continue;
                }

                if (room.CurrentState == RoomState.Entered || room.CurrentState == RoomState.CombatActive)
                {
                    return true;
                }
            }

            return false;
        }

        private bool ShouldPlayIntro(IReadOnlyList<Unit> activeEnemies, out Unit boss)
        {
            boss = null;
            if (!playIntro || introPlayed || introRunning || !IsAnyEnteredBossRoomActive())
            {
                return false;
            }

            boss = FindActiveBoss(activeEnemies);
            return boss != null && !IsBossEncounterRevealed(boss);
        }

        private static Unit FindActiveBoss(IReadOnlyList<Unit> activeEnemies)
        {
            if (activeEnemies != null)
            {
                for (int i = 0; i < activeEnemies.Count; i++)
                {
                    Unit enemy = activeEnemies[i];
                    if (IsActiveBoss(enemy))
                    {
                        return enemy;
                    }
                }
            }

            Unit[] units = Object.FindObjectsByType<Unit>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            for (int i = 0; i < units.Length; i++)
            {
                if (IsActiveBoss(units[i]))
                {
                    return units[i];
                }
            }

            return null;
        }

        private static bool IsActiveBoss(Unit unit)
        {
            return unit != null
                && unit.gameObject.activeInHierarchy
                && unit.IsAlive
                && BossEnemyAIController.IsBossLikeUnit(unit);
        }

        private IEnumerator PlayCameraRevealRoutine(Unit boss)
        {
            Camera cameraToUse = targetCamera != null ? targetCamera : Camera.main;
            if (boss == null || cameraToUse == null)
            {
                yield break;
            }

            EnsureOverlay();
            SetBlackAlpha(1f);
            SetOverlayVisible(true);

            Transform cameraTransform = cameraToUse.transform;
            Vector3 originalPosition = cameraTransform.position;
            Quaternion originalRotation = cameraTransform.rotation;
            float originalOrthographicSize = cameraToUse.orthographicSize;
            Vector3 lookPosition = boss.transform.position + revealLookOffset;
            Vector3 revealPosition = lookPosition + revealCameraOffset;
            Quaternion revealRotation = Quaternion.LookRotation(lookPosition - revealPosition, Vector3.up);

            yield return Wait(blackHoldBeforeReveal);

            cameraTransform.position = revealPosition;
            cameraTransform.rotation = revealRotation;
            if (cameraToUse.orthographic)
            {
                cameraToUse.orthographicSize = revealOrthographicSize;
            }

            yield return FadeBlack(1f, 0f, blackFadeOutDuration);
            yield return Wait(bossRevealHoldDuration);
            SetOverlayVisible(false);

            if (cameraReturnRoutine != null)
            {
                StopCoroutine(cameraReturnRoutine);
            }

            cameraReturnRoutine = StartCoroutine(MoveCameraReturnRoutine(cameraToUse, revealPosition, revealRotation, originalPosition, originalRotation, originalOrthographicSize));
        }

        private IEnumerator MoveCameraReturnRoutine(
            Camera cameraToUse,
            Vector3 startPosition,
            Quaternion startRotation,
            Vector3 targetPosition,
            Quaternion targetRotation,
            float targetOrthographicSize)
        {
            float duration = Mathf.Max(0f, cameraReturnDuration);
            if (duration <= 0f)
            {
                ApplyCameraState(cameraToUse, targetPosition, targetRotation, targetOrthographicSize);
                cameraReturnRoutine = null;
                yield break;
            }

            float startOrthographicSize = cameraToUse != null ? cameraToUse.orthographicSize : targetOrthographicSize;
            float elapsed = 0f;
            while (cameraToUse != null && elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / duration));
                cameraToUse.transform.position = Vector3.Lerp(startPosition, targetPosition, t);
                cameraToUse.transform.rotation = Quaternion.Slerp(startRotation, targetRotation, t);
                if (cameraToUse.orthographic)
                {
                    cameraToUse.orthographicSize = Mathf.Lerp(startOrthographicSize, targetOrthographicSize, t);
                }

                yield return null;
            }

            ApplyCameraState(cameraToUse, targetPosition, targetRotation, targetOrthographicSize);
            cameraReturnRoutine = null;
        }

        private static void ApplyCameraState(Camera cameraToUse, Vector3 position, Quaternion rotation, float orthographicSize)
        {
            if (cameraToUse == null)
            {
                return;
            }

            cameraToUse.transform.position = position;
            cameraToUse.transform.rotation = rotation;
            if (cameraToUse.orthographic)
            {
                cameraToUse.orthographicSize = orthographicSize;
            }
        }

        private IEnumerator FadeBlack(float fromAlpha, float toAlpha, float duration)
        {
            float clampedDuration = Mathf.Max(0f, duration);
            if (clampedDuration <= 0f)
            {
                SetBlackAlpha(toAlpha);
                yield break;
            }

            float elapsed = 0f;
            while (elapsed < clampedDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / clampedDuration);
                SetBlackAlpha(Mathf.Lerp(fromAlpha, toAlpha, t));
                yield return null;
            }

            SetBlackAlpha(toAlpha);
        }

        private static IEnumerator Wait(float duration)
        {
            float remaining = Mathf.Max(0f, duration);
            while (remaining > 0f)
            {
                remaining -= Time.unscaledDeltaTime;
                yield return null;
            }
        }

        private void EnsureOverlay()
        {
            if (overlayCanvas != null && blackImage != null)
            {
                return;
            }

            GameObject canvasObject = new GameObject("BossIntroBlackCoverCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(transform, false);

            overlayCanvas = canvasObject.GetComponent<Canvas>();
            overlayCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            overlayCanvas.sortingOrder = short.MaxValue - 1;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            GameObject imageObject = new GameObject("BossIntroBlackCover", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            imageObject.transform.SetParent(canvasObject.transform, false);

            RectTransform rectTransform = imageObject.GetComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;

            blackImage = imageObject.GetComponent<Image>();
            blackImage.color = Color.black;
            blackImage.raycastTarget = true;
            SetOverlayVisible(false);
        }

        private void SetOverlayVisible(bool visible)
        {
            if (overlayCanvas != null && overlayCanvas.gameObject.activeSelf != visible)
            {
                overlayCanvas.gameObject.SetActive(visible);
            }
        }

        private void SetBlackAlpha(float alpha)
        {
            if (blackImage == null)
            {
                return;
            }

            Color color = blackImage.color;
            color.a = Mathf.Clamp01(alpha);
            blackImage.color = color;
        }
    }
}
