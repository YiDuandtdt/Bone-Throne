using UnityEngine;

namespace BoneThrone.Core
{
    /// <summary>
    /// Lightweight front-end progression persistence for intro completion and level-select unlocks.
    /// </summary>
    public static class MenuProgressionState
    {
        private const string HighestUnlockedLevelKey = "BoneThrone.Menu.HighestUnlockedLevel";
        private const string IntroCompletedKey = "BoneThrone.Menu.IntroCompleted";
        private const int MinLevelIndex = 1;
        private const int MaxLevelIndex = 3;

        public static int HighestUnlockedLevel
        {
            get { return Mathf.Clamp(PlayerPrefs.GetInt(HighestUnlockedLevelKey, 0), 0, MaxLevelIndex); }
        }

        public static bool HasCompletedIntro
        {
            get { return PlayerPrefs.GetInt(IntroCompletedKey, 0) == 1; }
        }

        public static void StartNewGame()
        {
            SetHighestUnlockedLevel(0);
            SetIntroCompleted(false);
            PlayerPrefs.Save();
        }

        public static void CompleteIntroAndUnlockFirstLevel()
        {
            UnlockLevel(MinLevelIndex);
            SetIntroCompleted(true);
            PlayerPrefs.Save();
        }

        public static void UnlockLevel(int levelIndex)
        {
            if (levelIndex < MinLevelIndex || levelIndex > MaxLevelIndex)
            {
                return;
            }

            if (levelIndex <= HighestUnlockedLevel)
            {
                return;
            }

            SetHighestUnlockedLevel(levelIndex);
            PlayerPrefs.Save();
        }

        public static bool IsLevelUnlocked(int levelIndex)
        {
            return levelIndex >= MinLevelIndex
                && levelIndex <= MaxLevelIndex
                && levelIndex <= HighestUnlockedLevel;
        }

        public static void RegisterVisitedScene(string sceneName)
        {
            int levelIndex = GetLevelIndexForScene(sceneName);
            if (levelIndex > 0)
            {
                UnlockLevel(levelIndex);
            }
        }

        public static int GetLevelIndexForScene(string sceneName)
        {
            string normalized = NormalizeSceneName(sceneName);
            switch (normalized)
            {
                case "level_1":
                    return 1;
                case "level_2":
                    return 2;
                case "level_3":
                case "level_3_final":
                    return 3;
                case "boss_test":
                    return 3;
                default:
                    return 0;
            }
        }

        public static string GetSceneNameForLevel(int levelIndex)
        {
            switch (levelIndex)
            {
                case 1:
                    return "Level_1";
                case 2:
                    return "Level_2";
                case 3:
                    return "Level_3_final";
                default:
                    return string.Empty;
            }
        }

        private static void SetHighestUnlockedLevel(int value)
        {
            PlayerPrefs.SetInt(HighestUnlockedLevelKey, Mathf.Clamp(value, 0, MaxLevelIndex));
        }

        private static void SetIntroCompleted(bool completed)
        {
            PlayerPrefs.SetInt(IntroCompletedKey, completed ? 1 : 0);
        }

        private static string NormalizeSceneName(string sceneName)
        {
            return string.IsNullOrEmpty(sceneName) ? string.Empty : sceneName.Trim().ToLowerInvariant();
        }
    }
}
