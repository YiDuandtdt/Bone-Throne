using System.Collections;
using BoneThrone.Audio;
using BoneThrone.Core;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace BoneThrone.UI
{
    /// <summary>
    /// Plays a manually-authored intro story page with typewriter text and a fading black cover.
    /// </summary>
    public sealed class IntroStoryController : MonoBehaviour
    {
        private const float FirstLevelBgmFadeInSeconds = 4f;

        [Header("View")]
        [SerializeField] private TMP_Text storyText;
        [SerializeField] private Image blackCoverImage;

        [Header("Scene Flow")]
        [SerializeField] private string firstLevelSceneName = "Level_1";
        [SerializeField] [Min(0f)] private float sceneTransitionFadeSeconds = 0.4f;
        [SerializeField] private bool allowClickToSkip = true;

        [Header("Black Cover")]
        [SerializeField] [Range(0f, 1f)] private float initialBlackCoverAlpha = 1f;
        [SerializeField] [Range(0f, 1f)] private float blackCoverFadeStartNormalized = 0.45f;

        [Header("Story")]
        [SerializeField] [TextArea(6, 16)] private string introStoryText =
            "灰烬掠过王座阶梯，像一场没有尽头的寒冬。\n\n" +
            "骸骨王座之下，沉睡着曾将王国联结在一起的古老誓约。\n\n" +
            "今夜，四名冒险者推开封印之门，走向黑暗中等待他们的命运。";
        [SerializeField] [Min(0.01f)] private float introCharacterInterval = 0.05f;
        [SerializeField] [Min(0f)] private float introEndPauseSeconds = 2f;

        [Header("Debug")]
        [SerializeField] private bool debugLogging;

        private Coroutine introRoutine;
        private bool skipRequested;
        private bool transitionStarted;

        private void Awake()
        {
            if (storyText != null)
            {
                storyText.text = introStoryText;
                storyText.maxVisibleCharacters = 0;
            }

            ApplyBlackCoverAlpha(initialBlackCoverAlpha);
        }

        private void Start()
        {
            if (!Application.isPlaying || introRoutine != null)
            {
                return;
            }

            introRoutine = StartCoroutine(PlayIntroStoryRoutine());
        }

        private void Update()
        {
            if (!allowClickToSkip || introRoutine == null || transitionStarted)
            {
                return;
            }

            if (Input.GetMouseButtonDown(0) || Input.touchCount > 0 || Input.anyKeyDown)
            {
                skipRequested = true;
            }
        }

        private IEnumerator PlayIntroStoryRoutine()
        {
            if (storyText == null)
            {
                yield return TransitionAndLoadFirstLevel();
                yield break;
            }

            storyText.text = introStoryText;
            storyText.maxVisibleCharacters = 0;
            storyText.ForceMeshUpdate();

            int visibleCharacterTarget = storyText.textInfo.characterCount;
            if (visibleCharacterTarget <= 0)
            {
                yield return WaitForPauseOrSkip(introEndPauseSeconds);
                yield return TransitionAndLoadFirstLevel();
                yield break;
            }

            for (int visibleCharacters = 1; visibleCharacters <= visibleCharacterTarget; visibleCharacters++)
            {
                if (skipRequested)
                {
                    yield return TransitionAndLoadFirstLevel();
                    yield break;
                }

                storyText.maxVisibleCharacters = visibleCharacters;
                RefreshStoryBlackCover(visibleCharacters, visibleCharacterTarget);
                yield return new WaitForSeconds(introCharacterInterval);
            }

            ApplyBlackCoverAlpha(0f);
            yield return WaitForPauseOrSkip(introEndPauseSeconds);
            yield return TransitionAndLoadFirstLevel();
        }

        private IEnumerator WaitForPauseOrSkip(float seconds)
        {
            float remainingSeconds = Mathf.Max(0f, seconds);
            while (remainingSeconds > 0f)
            {
                if (skipRequested)
                {
                    yield break;
                }

                remainingSeconds -= Time.deltaTime;
                yield return null;
            }
        }

        private IEnumerator TransitionAndLoadFirstLevel()
        {
            if (transitionStarted)
            {
                yield break;
            }

            transitionStarted = true;
            if (storyText != null)
            {
                storyText.maxVisibleCharacters = int.MaxValue;
            }

            float duration = Mathf.Max(0f, sceneTransitionFadeSeconds);
            if (duration > 0f && blackCoverImage != null)
            {
                float elapsed = 0f;
                Color color = blackCoverImage.color;
                float startAlpha = color.a;
                while (elapsed < duration)
                {
                    elapsed += Time.deltaTime;
                    float normalized = Mathf.Clamp01(elapsed / duration);
                    ApplyBlackCoverAlpha(Mathf.Lerp(startAlpha, 1f, normalized));
                    yield return null;
                }
            }

            ApplyBlackCoverAlpha(1f);
            LoadFirstLevel();
        }

        private void LoadFirstLevel()
        {
            MenuProgressionState.CompleteIntroAndUnlockFirstLevel();

            if (string.IsNullOrEmpty(firstLevelSceneName))
            {
                Debug.LogWarning("IntroStoryController cannot load an empty first-level scene name.", this);
                return;
            }

            if (!Application.CanStreamedLevelBeLoaded(firstLevelSceneName))
            {
                Debug.LogWarning("IntroStoryController cannot load scene '" + firstLevelSceneName + "' because it is not in Build Settings.", this);
                return;
            }

            BTAudioService.RequestNextSceneBgmFadeIn(FirstLevelBgmFadeInSeconds);
            Log("Loading scene '" + firstLevelSceneName + "'.");
            SceneManager.LoadScene(firstLevelSceneName, LoadSceneMode.Single);
        }

        private void ApplyBlackCoverAlpha(float alpha)
        {
            if (blackCoverImage == null)
            {
                return;
            }

            Color color = blackCoverImage.color;
            color.a = Mathf.Clamp01(alpha);
            blackCoverImage.color = color;
        }

        private void RefreshStoryBlackCover(int visibleCharacters, int visibleCharacterTarget)
        {
            if (blackCoverImage == null || visibleCharacterTarget <= 0)
            {
                return;
            }

            float start = Mathf.Clamp01(blackCoverFadeStartNormalized);
            float progress = Mathf.Clamp01((float)visibleCharacters / visibleCharacterTarget);
            float fadeProgress = Mathf.InverseLerp(start, 1f, progress);
            ApplyBlackCoverAlpha(Mathf.Lerp(initialBlackCoverAlpha, 0f, fadeProgress));
        }

        private void Log(string message)
        {
            if (debugLogging)
            {
                Debug.Log("IntroStoryController: " + message, this);
            }
        }
    }
}
