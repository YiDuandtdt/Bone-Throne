using System.Collections;
using BoneThrone.Audio;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace BoneThrone.UI
{
    /// <summary>
    /// Keeps a full-screen black cover visible while switching scenes to avoid exposing the old scene for a frame.
    /// </summary>
    public sealed class SceneBlackCoverService : MonoBehaviour
    {
        private static SceneBlackCoverService instance;

        private Canvas canvas;
        private Image blackCoverImage;
        private bool loadingScene;

        public static void LoadSceneWithCover(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName))
            {
                return;
            }

            EnsureInstance().StartSceneLoad(sceneName);
        }

        public static void HideImmediately()
        {
            if (instance == null)
            {
                return;
            }

            instance.StopAllCoroutines();
            instance.loadingScene = false;
            instance.EnsureOverlay();
            instance.SetCoverVisible(false);
        }

        private static SceneBlackCoverService EnsureInstance()
        {
            if (instance != null)
            {
                return instance;
            }

            GameObject host = new GameObject("SceneBlackCoverService");
            DontDestroyOnLoad(host);
            instance = host.AddComponent<SceneBlackCoverService>();
            return instance;
        }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);
            EnsureOverlay();
            SetCoverVisible(false);
        }

        private void StartSceneLoad(string sceneName)
        {
            if (loadingScene)
            {
                return;
            }

            StartCoroutine(LoadSceneRoutine(sceneName));
        }

        private IEnumerator LoadSceneRoutine(string sceneName)
        {
            loadingScene = true;
            EnsureOverlay();
            SetCoverVisible(true);

            // Let the black cover render before unloading the current scene.
            yield return new WaitForEndOfFrame();

            SceneManager.LoadScene(sceneName, LoadSceneMode.Single);

            // Keep the cover for one extra frame in the destination scene to hide scene bootstrapping.
            yield return null;
            BTAudioService.PlaySceneBgmForCurrentScene();
            yield return new WaitForEndOfFrame();

            SetCoverVisible(false);
            loadingScene = false;
        }

        private void EnsureOverlay()
        {
            if (canvas != null && blackCoverImage != null)
            {
                return;
            }

            GameObject canvasObject = new GameObject(
                "BlackCoverCanvas",
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(transform, false);

            canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = short.MaxValue;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            GameObject imageObject = new GameObject(
                "BlackCover",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image));
            imageObject.transform.SetParent(canvasObject.transform, false);

            RectTransform rectTransform = imageObject.GetComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;

            blackCoverImage = imageObject.GetComponent<Image>();
            blackCoverImage.color = Color.black;
            blackCoverImage.raycastTarget = true;
        }

        private void SetCoverVisible(bool visible)
        {
            if (canvas != null && canvas.gameObject.activeSelf != visible)
            {
                canvas.gameObject.SetActive(visible);
            }
        }
    }
}
