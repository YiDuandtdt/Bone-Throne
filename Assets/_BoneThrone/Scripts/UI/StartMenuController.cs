using BoneThrone.Audio;
using BoneThrone.Core;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace BoneThrone.UI
{
    /// <summary>
    /// Binds a manually-authored start-menu prefab and only handles page switching,
    /// level unlock state, volume persistence, and scene loading.
    /// </summary>
    public sealed class StartMenuController : MonoBehaviour
    {
        private enum MenuPage
        {
            Home,
            About,
            LevelSelect,
            Settings
        }

        [Header("Pages")]
        [SerializeField] private GameObject homePageRoot;
        [SerializeField] private GameObject aboutPageRoot;
        [SerializeField] private GameObject levelSelectPageRoot;
        [SerializeField] private GameObject settingsPageRoot;
        [SerializeField] private SettingsPageView settingsPageView;

        [Header("Home Buttons")]
        [SerializeField] private Button startGameButton;
        [SerializeField] private Button continueButton;
        [SerializeField] private Button aboutButton;
        [SerializeField] private Button homeSettingsButton;
        [SerializeField] private Button quitButton;

        [Header("Shared Settings Entry")]
        [SerializeField] private Button[] commonSettingsButtons;

        [Header("Level Select")]
        [SerializeField] private Button level1Button;
        [SerializeField] private Button level2Button;
        [SerializeField] private Button level3Button;
        [SerializeField] private GameObject level1LockedOverlay;
        [SerializeField] private GameObject level2LockedOverlay;
        [SerializeField] private GameObject level3LockedOverlay;

        [Header("Settings")]
        [SerializeField] private Slider bgmSlider;
        [SerializeField] private Slider sfxSlider;
        [SerializeField] private Button returnHomeButton;
        [SerializeField] private Button settingsQuitButton;

        [Header("Scene Flow")]
        [SerializeField] private string introSceneName = "IntroStory";
        [SerializeField] private string firstLevelSceneName = "Level_1";
        [SerializeField] private string secondLevelSceneName = "Level_2";
        [SerializeField] private string thirdLevelSceneName = "boss_test";

        [Header("Debug")]
        [SerializeField] private bool debugLogging;

        private bool settingSliderValuesSilently;
        private MenuPage currentPage;
        private Coroutine lockedLevelShakeRoutine;
        private RectTransform lockedLevelShakeRect;
        private Vector2 lockedLevelShakeOriginalPosition;

        private void Awake()
        {
            SceneBlackCoverService.HideImmediately();
            EnsureSettingsPageView();
            ConfigureSettingsPageView();
            currentPage = MenuPage.Home;
            ShowCurrentPage();
            RefreshLevelButtons();
            ApplyStoredAudioValues();
        }

        private void OnEnable()
        {
            SceneBlackCoverService.HideImmediately();
            EnsureSettingsPageView();
            ConfigureSettingsPageView();
            BindButtons();
            BindSliders();
            RefreshLevelButtons();
            ApplyStoredAudioValues();
        }

        private void OnDisable()
        {
            UnbindButtons();
            UnbindSliders();
        }

        private void BindButtons()
        {
            BindButton(startGameButton, HandleStartGameClicked);
            BindButton(continueButton, HandleContinueClicked);
            BindButton(aboutButton, HandleAboutClicked);
            BindButton(homeSettingsButton, HandleSettingsClicked);
            BindButton(quitButton, HandleQuitClicked);
            BindButton(level1Button, HandleLevel1Clicked);
            BindButton(level2Button, HandleLevel2Clicked);
            BindButton(level3Button, HandleLevel3Clicked);
            BindButton(GetSettingsPrimaryButton(), HandleReturnHomeClicked);
            BindButton(GetSettingsSecondaryButton(), HandleQuitClicked);

            if (commonSettingsButtons == null)
            {
                return;
            }

            for (int i = 0; i < commonSettingsButtons.Length; i++)
            {
                BindButton(commonSettingsButtons[i], HandleSettingsClicked);
            }
        }

        private void UnbindButtons()
        {
            UnbindButton(startGameButton, HandleStartGameClicked);
            UnbindButton(continueButton, HandleContinueClicked);
            UnbindButton(aboutButton, HandleAboutClicked);
            UnbindButton(homeSettingsButton, HandleSettingsClicked);
            UnbindButton(quitButton, HandleQuitClicked);
            UnbindButton(level1Button, HandleLevel1Clicked);
            UnbindButton(level2Button, HandleLevel2Clicked);
            UnbindButton(level3Button, HandleLevel3Clicked);
            UnbindButton(GetSettingsPrimaryButton(), HandleReturnHomeClicked);
            UnbindButton(GetSettingsSecondaryButton(), HandleQuitClicked);

            if (commonSettingsButtons == null)
            {
                return;
            }

            for (int i = 0; i < commonSettingsButtons.Length; i++)
            {
                UnbindButton(commonSettingsButtons[i], HandleSettingsClicked);
            }
        }

        private void BindSliders()
        {
            Slider resolvedBgmSlider = GetBgmSlider();
            if (resolvedBgmSlider != null)
            {
                resolvedBgmSlider.onValueChanged.RemoveListener(HandleBgmSliderChanged);
                resolvedBgmSlider.onValueChanged.AddListener(HandleBgmSliderChanged);
            }

            Slider resolvedSfxSlider = GetSfxSlider();
            if (resolvedSfxSlider != null)
            {
                resolvedSfxSlider.onValueChanged.RemoveListener(HandleSfxSliderChanged);
                resolvedSfxSlider.onValueChanged.AddListener(HandleSfxSliderChanged);
            }
        }

        private void UnbindSliders()
        {
            Slider resolvedBgmSlider = GetBgmSlider();
            if (resolvedBgmSlider != null)
            {
                resolvedBgmSlider.onValueChanged.RemoveListener(HandleBgmSliderChanged);
            }

            Slider resolvedSfxSlider = GetSfxSlider();
            if (resolvedSfxSlider != null)
            {
                resolvedSfxSlider.onValueChanged.RemoveListener(HandleSfxSliderChanged);
            }
        }

        private void HandleStartGameClicked()
        {
            PlayButtonClick();
            MenuProgressionState.StartNewGame();
            LoadConfiguredScene(introSceneName);
        }

        private void HandleContinueClicked()
        {
            PlayButtonClick();
            RefreshLevelButtons();
            SetCurrentPage(MenuPage.LevelSelect, true);
        }

        private void HandleAboutClicked()
        {
            PlayButtonClick();
            SetCurrentPage(MenuPage.About, true);
        }

        private void HandleSettingsClicked()
        {
            PlayButtonClick();
            ApplyStoredAudioValues();
            SetCurrentPage(MenuPage.Settings, true);
        }

        private void HandleReturnHomeClicked()
        {
            PlayButtonClick();
            SetCurrentPage(MenuPage.Home, true);
        }

        private void HandleLevel1Clicked()
        {
            TryLoadUnlockedLevel(1);
        }

        private void HandleLevel2Clicked()
        {
            TryLoadUnlockedLevel(2);
        }

        private void HandleLevel3Clicked()
        {
            TryLoadUnlockedLevel(3);
        }

        private void TryLoadUnlockedLevel(int levelIndex)
        {
            if (!MenuProgressionState.IsLevelUnlocked(levelIndex))
            {
                BTAudioService.PlaySfx(BTAudioCueId.InvalidAction);
                ShakeLockedLevelOverlay(levelIndex);
                return;
            }

            PlayButtonClick();
            LoadConfiguredScene(GetSceneNameForLevel(levelIndex));
        }

        private void HandleQuitClicked()
        {
            PlayButtonClick();
            Log("Quit requested.");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void HandleBgmSliderChanged(float value)
        {
            if (settingSliderValuesSilently)
            {
                return;
            }

            BTAudioService.SetBgmVolume(SettingsPageView.SnapAudioValue(value));
        }

        private void HandleSfxSliderChanged(float value)
        {
            if (settingSliderValuesSilently)
            {
                return;
            }

            BTAudioService.SetSfxVolume(SettingsPageView.SnapAudioValue(value));
        }

        private void SetCurrentPage(MenuPage page, bool playPageSfx)
        {
            SceneBlackCoverService.HideImmediately();
            currentPage = page;
            ShowCurrentPage();

            if (playPageSfx)
            {
                BTAudioService.PlaySfx(BTAudioCueId.Page);
            }
        }

        private void ShowCurrentPage()
        {
            SetPageVisible(homePageRoot, currentPage == MenuPage.Home);
            SetPageVisible(aboutPageRoot, currentPage == MenuPage.About);
            SetPageVisible(levelSelectPageRoot, currentPage == MenuPage.LevelSelect);
            SetPageVisible(settingsPageRoot, currentPage == MenuPage.Settings);
        }

        private void RefreshLevelButtons()
        {
            RefreshLevelButton(level1Button, level1LockedOverlay, 1);
            RefreshLevelButton(level2Button, level2LockedOverlay, 2);
            RefreshLevelButton(level3Button, level3LockedOverlay, 3);
        }

        private static void RefreshLevelButton(Button button, GameObject lockedOverlay, int levelIndex)
        {
            bool unlocked = MenuProgressionState.IsLevelUnlocked(levelIndex);

            if (button != null)
            {
                button.interactable = true;
            }

            if (lockedOverlay != null)
            {
                lockedOverlay.SetActive(!unlocked);
                SetGraphicRaycastTargets(lockedOverlay, false);
            }
        }

        private void ShakeLockedLevelOverlay(int levelIndex)
        {
            GameObject lockedOverlay = GetLockedOverlay(levelIndex);
            RectTransform target = lockedOverlay != null ? lockedOverlay.GetComponent<RectTransform>() : null;
            if (target == null)
            {
                return;
            }

            if (lockedLevelShakeRoutine != null)
            {
                StopCoroutine(lockedLevelShakeRoutine);
                if (lockedLevelShakeRect != null)
                {
                    lockedLevelShakeRect.anchoredPosition = lockedLevelShakeOriginalPosition;
                }
            }

            lockedLevelShakeRect = target;
            lockedLevelShakeOriginalPosition = target.anchoredPosition;
            lockedLevelShakeRoutine = StartCoroutine(ShakeLockedLevelOverlayRoutine(target, lockedLevelShakeOriginalPosition));
        }

        private IEnumerator ShakeLockedLevelOverlayRoutine(RectTransform target, Vector2 originalPosition)
        {
            const float duration = 0.22f;
            const float amplitude = 8f;

            float elapsed = 0f;
            while (elapsed < duration)
            {
                if (target == null)
                {
                    lockedLevelShakeRoutine = null;
                    yield break;
                }

                elapsed += Time.unscaledDeltaTime;
                float normalized = Mathf.Clamp01(elapsed / duration);
                float offset = Mathf.Sin(normalized * Mathf.PI * 6f) * amplitude * (1f - normalized);
                target.anchoredPosition = originalPosition + new Vector2(offset, 0f);
                yield return null;
            }

            if (target != null)
            {
                target.anchoredPosition = originalPosition;
            }

            lockedLevelShakeRoutine = null;
        }

        private GameObject GetLockedOverlay(int levelIndex)
        {
            switch (levelIndex)
            {
                case 1:
                    return level1LockedOverlay;
                case 2:
                    return level2LockedOverlay;
                case 3:
                    return level3LockedOverlay;
                default:
                    return null;
            }
        }

        private static void SetGraphicRaycastTargets(GameObject root, bool raycastTarget)
        {
            Graphic[] graphics = root.GetComponentsInChildren<Graphic>(true);
            for (int i = 0; i < graphics.Length; i++)
            {
                if (graphics[i] != null)
                {
                    graphics[i].raycastTarget = raycastTarget;
                }
            }
        }

        private void ApplyStoredAudioValues()
        {
            settingSliderValuesSilently = true;

            if (settingsPageView != null)
            {
                settingsPageView.SetAudioValuesWithoutNotify(
                    BTAudioService.GetBgmVolume(),
                    BTAudioService.GetSfxVolume());
            }
            else
            {
                Slider resolvedBgmSlider = GetBgmSlider();
                if (resolvedBgmSlider != null)
                {
                    resolvedBgmSlider.SetValueWithoutNotify(BTAudioService.GetBgmVolume());
                }

                Slider resolvedSfxSlider = GetSfxSlider();
                if (resolvedSfxSlider != null)
                {
                    resolvedSfxSlider.SetValueWithoutNotify(BTAudioService.GetSfxVolume());
                }
            }

            settingSliderValuesSilently = false;
        }

        private void EnsureSettingsPageView()
        {
            if (settingsPageView == null && settingsPageRoot != null)
            {
                settingsPageView = settingsPageRoot.GetComponentInChildren<SettingsPageView>(true);
            }
        }

        private void ConfigureSettingsPageView()
        {
            if (settingsPageView != null)
            {
                settingsPageView.SetButtonLabels("返回主页", "退出游戏");
            }
        }

        private Slider GetBgmSlider()
        {
            return settingsPageView != null && settingsPageView.BgmSlider != null
                ? settingsPageView.BgmSlider
                : bgmSlider;
        }

        private Slider GetSfxSlider()
        {
            return settingsPageView != null && settingsPageView.SfxSlider != null
                ? settingsPageView.SfxSlider
                : sfxSlider;
        }

        private Button GetSettingsPrimaryButton()
        {
            return settingsPageView != null && settingsPageView.PrimaryButton != null
                ? settingsPageView.PrimaryButton
                : returnHomeButton;
        }

        private Button GetSettingsSecondaryButton()
        {
            return settingsPageView != null && settingsPageView.SecondaryButton != null
                ? settingsPageView.SecondaryButton
                : settingsQuitButton;
        }

        private string GetSceneNameForLevel(int levelIndex)
        {
            switch (levelIndex)
            {
                case 1:
                    return firstLevelSceneName;
                case 2:
                    return secondLevelSceneName;
                case 3:
                    return thirdLevelSceneName;
                default:
                    return string.Empty;
            }
        }

        private void LoadConfiguredScene(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName))
            {
                Debug.LogWarning("StartMenuController cannot load an empty scene name.", this);
                return;
            }

            if (!Application.CanStreamedLevelBeLoaded(sceneName))
            {
                Debug.LogWarning("StartMenuController cannot load scene '" + sceneName + "' because it is not in Build Settings.", this);
                return;
            }

            Log("Loading scene '" + sceneName + "'.");
            SceneBlackCoverService.LoadSceneWithCover(sceneName);
        }

        private static void SetPageVisible(GameObject pageRoot, bool visible)
        {
            if (pageRoot != null)
            {
                pageRoot.SetActive(visible);
            }
        }

        private void PlayButtonClick()
        {
            BTAudioService.PlaySfx(BTAudioCueId.ButtonClick);
        }

        private static void BindButton(Button button, UnityEngine.Events.UnityAction action)
        {
            if (button == null)
            {
                return;
            }

            button.onClick.RemoveListener(action);
            button.onClick.AddListener(action);
        }

        private static void UnbindButton(Button button, UnityEngine.Events.UnityAction action)
        {
            if (button != null)
            {
                button.onClick.RemoveListener(action);
            }
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
