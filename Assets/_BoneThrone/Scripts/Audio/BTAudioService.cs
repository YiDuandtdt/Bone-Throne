using System.Collections;
using System.Collections.Generic;
using BoneThrone.Core;
using BoneThrone.Units;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BoneThrone.Audio
{
    public sealed class BTAudioService : MonoBehaviour
    {
        private const float TargetSfxRms = 0.14f;
        private const float TargetMusicRms = 0.09f;
        private const float MinAnalyzedRms = 0.01f;
        private const float MaxNormalizedSfxVolume = 1.25f;
        private const float MaxNormalizedMusicVolume = 0.55f;
        private const float DefaultSfxVolume = 0.85f;
        private const float DefaultBgmVolume = 0.35f;
        private const float ImmediateUiPressFeedbackSuppressionSeconds = 1f;
        private const string BgmVolumeKey = "BoneThrone.Audio.BgmVolume";
        private const string SfxVolumeKey = "BoneThrone.Audio.SfxVolume";

        private static BTAudioService instance;
        private static bool sceneHooksRegistered;
        private static bool pendingNextSceneBgmFadeIn;
        private static float pendingNextSceneBgmFadeDuration;

        private readonly Dictionary<BTAudioCueId, AudioClip> clipCache = new Dictionary<BTAudioCueId, AudioClip>();
        private readonly Dictionary<BTAudioCueId, float> sfxVolumeCache = new Dictionary<BTAudioCueId, float>();
        private readonly Dictionary<BTAudioCueId, float> musicVolumeCache = new Dictionary<BTAudioCueId, float>();
        private readonly Dictionary<int, AudioSource> activeLoops = new Dictionary<int, AudioSource>();
        private readonly Dictionary<int, BTAudioCueId> activeLoopCues = new Dictionary<int, BTAudioCueId>();

        private AudioSource sfxSource;
        private AudioSource bgmSource;
        private BTAudioCueId currentBgmCue = BTAudioCueId.None;
        private float userBgmVolume = 1f;
        private float userSfxVolume = 1f;
        private int lastButtonClickFrame = -1;
        private int lastInvalidActionFrame = -1;
        private bool userVolumeSettingsLoaded;
        private bool uiSfxWarmed;
        private bool suppressNextButtonClick;
        private float suppressNextButtonClickExpiresAt;
        private Coroutine bgmFadeRoutine;

        private static readonly Dictionary<BTAudioCueId, string> ClipPaths = new Dictionary<BTAudioCueId, string>
        {
            { BTAudioCueId.BgmMenu, "BoneThroneAudio/BGM/Menu/BTBGM_Menu_Main" },
            { BTAudioCueId.BgmBattle, "BoneThroneAudio/BGM/Battle/BTBGM_Battle_Normal" },
            { BTAudioCueId.BgmBoss, "BoneThroneAudio/BGM/Boss/BTBGM_Boss" },
            { BTAudioCueId.BgmVictory, "BoneThroneAudio/BGM/Result/BTBGM_Result_Victory" },
            { BTAudioCueId.BgmDefeat, "BoneThroneAudio/BGM/Result/BTBGM_Result_Defeat" },
            { BTAudioCueId.SwordSlash, "BoneThroneAudio/SFX/Combat/Attack/BTSFX_Sword_Slash" },
            { BTAudioCueId.AxeChop, "BoneThroneAudio/SFX/Combat/Attack/BTSFX_Axe_Chop" },
            { BTAudioCueId.MultiSlash, "BoneThroneAudio/SFX/Combat/Attack/BTSFX_Multi_Slash" },
            { BTAudioCueId.HeavyHit, "BoneThroneAudio/SFX/Combat/Attack/BTSFX_Heavy_Hit" },
            { BTAudioCueId.ArrowShoot, "BoneThroneAudio/SFX/Combat/Projectile/BTSFX_Arrow_Shoot" },
            { BTAudioCueId.SpecialArrow, "BoneThroneAudio/SFX/Combat/Projectile/BTSFX_Special_Arrow" },
            { BTAudioCueId.MultiArrow, "BoneThroneAudio/SFX/Combat/Projectile/BTSFX_Multi_Arrow" },
            { BTAudioCueId.HeavyArrow, "BoneThroneAudio/SFX/Combat/Projectile/BTSFX_Heavy_Arrow" },
            { BTAudioCueId.MagicBasic, "BoneThroneAudio/SFX/Combat/Magic/BTSFX_Magic_Basic" },
            { BTAudioCueId.Fireball, "BoneThroneAudio/SFX/Combat/Magic/BTSFX_Fireball" },
            { BTAudioCueId.Lightning, "BoneThroneAudio/SFX/Combat/Magic/BTSFX_Lightning" },
            { BTAudioCueId.MagicOne, "BoneThroneAudio/SFX/Combat/Magic/BTSFX_Magic_1" },
            { BTAudioCueId.MagicTwo, "BoneThroneAudio/SFX/Combat/Magic/BTSFX_Magic_2" },
            { BTAudioCueId.PlayerDeath, "BoneThroneAudio/SFX/Death/BTSFX_Player_Death" },
            { BTAudioCueId.MonsterDeath, "BoneThroneAudio/SFX/Death/BTSFX_Monster_Death" },
            { BTAudioCueId.LevelUp, "BoneThroneAudio/SFX/Interaction/LevelUp/BTSFX_Level_Up" },
            { BTAudioCueId.KeyPickup, "BoneThroneAudio/SFX/Interaction/Key/BTSFX_Key_Pickup" },
            { BTAudioCueId.StairsLoop, "BoneThroneAudio/SFX/Interaction/Stairs/BTSFX_Stairs_Loop" },
            { BTAudioCueId.FootstepLightLoop, "BoneThroneAudio/SFX/Movement/BTSFX_Footstep_Light_Loop" },
            { BTAudioCueId.FootstepHeavyLoop, "BoneThroneAudio/SFX/Movement/BTSFX_Footstep_Heavy_Loop" },
            { BTAudioCueId.FootstepLargeLoop, "BoneThroneAudio/SFX/Movement/BTSFX_Footstep_Large_Loop" },
            { BTAudioCueId.ButtonClick, "BoneThroneAudio/SFX/UI/BTSFX_Button_Click" },
            { BTAudioCueId.MouseClick, "BoneThroneAudio/SFX/UI/BTSFX_Mouse_Click" },
            { BTAudioCueId.InvalidAction, "BoneThroneAudio/SFX/UI/BTSFX_Invalid_Action" },
            { BTAudioCueId.Page, "BoneThroneAudio/SFX/UI/BTSFX_Page" }
        };

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            RegisterSceneHooks();
            EnsureInstance().PlaySceneBgm(SceneManager.GetActiveScene().name, true);
        }

        public static void PlaySfx(BTAudioCueId cue)
        {
            EnsureInstance().PlaySfxInternal(cue);
        }

        public static void PlaySfx(BTAudioCueId cue, float volumeScale, float pitch)
        {
            EnsureInstance().PlaySfxInternal(cue, volumeScale, pitch);
        }

        public static void PlayImmediateUiPressFeedback()
        {
            EnsureInstance().PlayImmediateUiPressFeedbackInternal();
        }

        public static void ClearPendingImmediateUiPressFeedbackSuppression()
        {
            if (instance != null)
            {
                instance.ClearPendingImmediateUiPressFeedbackSuppressionInternal();
            }
        }

        public static bool WasExplicitUiClickFeedbackPlayedSinceFrame(int frame)
        {
            return instance != null && instance.WasExplicitUiClickFeedbackPlayedSinceFrameInternal(frame);
        }

        public static void PlayDeathSfx(Unit unit)
        {
            if (unit == null)
            {
                PlaySfx(BTAudioCueId.MonsterDeath, 1.45f, 0.9f);
                return;
            }

            if (IsBossLikeUnit(unit))
            {
                PlaySfx(BTAudioCueId.MonsterDeath, 1.8f, 0.55f);
                return;
            }

            if (unit.Faction == UnitFaction.Player)
            {
                PlaySfx(BTAudioCueId.PlayerDeath, 1.05f, 1f);
                return;
            }

            PlaySfx(BTAudioCueId.MonsterDeath, 1.45f, 0.9f);
        }

        public static void PlayBgm(BTAudioCueId cue)
        {
            EnsureInstance().PlayBgmInternal(cue);
        }

        public static void PlayMusicOnce(BTAudioCueId cue)
        {
            EnsureInstance().PlayMusicOnceInternal(cue);
        }

        public static void PlaySceneBgmForCurrentScene()
        {
            EnsureInstance().PlaySceneBgm(SceneManager.GetActiveScene().name);
        }

        public static void RequestNextSceneBgmFadeIn(float durationSeconds)
        {
            EnsureInstance().RequestNextSceneBgmFadeInInternal(durationSeconds);
        }

        public static float GetBgmVolume()
        {
            return EnsureInstance().GetUserBgmVolumeInternal();
        }

        public static float GetSfxVolume()
        {
            return EnsureInstance().GetUserSfxVolumeInternal();
        }

        public static void SetBgmVolume(float value)
        {
            EnsureInstance().SetUserBgmVolumeInternal(value);
        }

        public static void SetSfxVolume(float value)
        {
            EnsureInstance().SetUserSfxVolumeInternal(value);
        }

        public static void PlayLoop(BTAudioCueId cue, Object owner)
        {
            EnsureInstance().PlayLoopInternal(cue, owner);
        }

        public static void StopLoop(Object owner)
        {
            if (instance != null)
            {
                instance.StopLoopInternal(owner);
            }
        }

        public static void StopAllNonBgmLoops()
        {
            if (instance != null)
            {
                instance.StopAllNonBgmLoopsInternal();
            }
        }

        public static BTAudioCueId GetBasicAttackCue(Unit attacker)
        {
            if (attacker == null)
            {
                return BTAudioCueId.HeavyHit;
            }

            if (attacker.GetComponent<RangerHitPresentationConfig>() != null)
            {
                return BTAudioCueId.ArrowShoot;
            }

            if (attacker.GetComponent<MageHitPresentationConfig>() != null)
            {
                return BTAudioCueId.MagicBasic;
            }

            if (attacker.Faction == UnitFaction.Enemy && IsAxeSkeleton(attacker))
            {
                return BTAudioCueId.AxeChop;
            }

            switch (attacker.RoleId)
            {
                case RoleId.Ranger:
                    return BTAudioCueId.ArrowShoot;
                case RoleId.Mage:
                    return BTAudioCueId.MagicBasic;
                case RoleId.Barbarian:
                    return BTAudioCueId.AxeChop;
                case RoleId.Fighter:
                    return BTAudioCueId.SwordSlash;
                default:
                    return BTAudioCueId.HeavyHit;
            }
        }

        public static BTAudioCueId GetFootstepCue(Unit unit)
        {
            if (unit == null)
            {
                return BTAudioCueId.FootstepLightLoop;
            }

            string unitName = Normalize(unit.name);
            if (unitName.Contains("boss") || unitName.Contains("golem") || unitName.Contains("large"))
            {
                return BTAudioCueId.FootstepLargeLoop;
            }

            if (unitName.Contains("knight") || unitName.Contains("warrior") || unitName.Contains("heavy"))
            {
                return BTAudioCueId.FootstepHeavyLoop;
            }

            if (unit.RoleId == RoleId.Fighter || unit.RoleId == RoleId.Barbarian)
            {
                return BTAudioCueId.FootstepHeavyLoop;
            }

            return BTAudioCueId.FootstepLightLoop;
        }

        public static BTAudioCueId GetSkillCue(string skillName)
        {
            string normalized = Normalize(skillName);
            if (normalized == "ranger_precision_shot")
            {
                return BTAudioCueId.SpecialArrow;
            }

            if (normalized == "ranger_quick_shot")
            {
                return BTAudioCueId.MultiArrow;
            }

            if (normalized == "ranger_piercing_arrow")
            {
                return BTAudioCueId.HeavyArrow;
            }

            if (normalized == "mage_fireball")
            {
                return BTAudioCueId.Fireball;
            }

            if (normalized == "mage_frost_bolt")
            {
                return BTAudioCueId.MagicTwo;
            }

            if (normalized == "mage_arcane_burst")
            {
                return BTAudioCueId.Lightning;
            }

            if (normalized == "fighter_shield_bash")
            {
                return BTAudioCueId.HeavyHit;
            }

            if (normalized == "fighter_guard_strike")
            {
                return BTAudioCueId.SwordSlash;
            }

            if (normalized == "fighter_crushing_challenge")
            {
                return BTAudioCueId.HeavyHit;
            }

            if (normalized == "barbarian_heavy_cleave")
            {
                return BTAudioCueId.AxeChop;
            }

            if (normalized == "barbarian_rage_strike")
            {
                return BTAudioCueId.HeavyHit;
            }

            if (normalized == "barbarian_blood_fury_slash")
            {
                return BTAudioCueId.MultiSlash;
            }

            return BTAudioCueId.MagicOne;
        }

        private static BTAudioService EnsureInstance()
        {
            if (instance != null)
            {
                return instance;
            }

            BTAudioService existing = Object.FindFirstObjectByType<BTAudioService>();
            if (existing != null)
            {
                instance = existing;
                instance.EnsureSources();
                return instance;
            }

            GameObject host = new GameObject("BTAudioService_Runtime");
            DontDestroyOnLoad(host);
            instance = host.AddComponent<BTAudioService>();
            instance.EnsureSources();
            return instance;
        }

        private static void RegisterSceneHooks()
        {
            if (sceneHooksRegistered)
            {
                return;
            }

            SceneManager.sceneLoaded += HandleSceneLoaded;
            sceneHooksRegistered = true;
        }

        private static void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            BTAudioService service = EnsureInstance();
            service.StopAllNonBgmLoopsInternal();
            service.PlaySceneBgm(scene.name, true);
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
            EnsureSources();
            RegisterSceneHooks();
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }

        private void EnsureSources()
        {
            LoadUserVolumeSettingsIfNeeded();

            if (sfxSource == null)
            {
                sfxSource = gameObject.AddComponent<AudioSource>();
                sfxSource.playOnAwake = false;
                sfxSource.loop = false;
                sfxSource.spatialBlend = 0f;
            }

            if (bgmSource == null)
            {
                bgmSource = gameObject.AddComponent<AudioSource>();
                bgmSource.playOnAwake = false;
                bgmSource.loop = true;
                bgmSource.spatialBlend = 0f;
                bgmSource.volume = DefaultBgmVolume * userBgmVolume;
            }

            WarmUpUiSfx();
        }

        private void LoadUserVolumeSettingsIfNeeded()
        {
            if (userVolumeSettingsLoaded)
            {
                return;
            }

            userBgmVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(BgmVolumeKey, 1f));
            userSfxVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(SfxVolumeKey, 1f));
            userVolumeSettingsLoaded = true;
        }

        private float GetUserBgmVolumeInternal()
        {
            LoadUserVolumeSettingsIfNeeded();
            return userBgmVolume;
        }

        private float GetUserSfxVolumeInternal()
        {
            LoadUserVolumeSettingsIfNeeded();
            return userSfxVolume;
        }

        private void RequestNextSceneBgmFadeInInternal(float durationSeconds)
        {
            pendingNextSceneBgmFadeIn = true;
            pendingNextSceneBgmFadeDuration = Mathf.Max(0f, durationSeconds);

            if (bgmSource != null)
            {
                bgmSource.volume = 0f;
            }
        }

        private void SetUserBgmVolumeInternal(float value)
        {
            LoadUserVolumeSettingsIfNeeded();
            userBgmVolume = Mathf.Clamp01(value);
            PlayerPrefs.SetFloat(BgmVolumeKey, userBgmVolume);
            PlayerPrefs.Save();
            ApplyUserBgmVolume();
        }

        private void SetUserSfxVolumeInternal(float value)
        {
            LoadUserVolumeSettingsIfNeeded();
            userSfxVolume = Mathf.Clamp01(value);
            PlayerPrefs.SetFloat(SfxVolumeKey, userSfxVolume);
            PlayerPrefs.Save();
            ApplyUserSfxVolumeToActiveLoops();
        }

        private void ApplyUserBgmVolume()
        {
            if (bgmSource == null || bgmSource.clip == null)
            {
                return;
            }

            if (bgmFadeRoutine != null)
            {
                return;
            }

            bgmSource.volume = GetTargetBgmVolume(currentBgmCue, bgmSource.clip);
        }

        private void ApplyUserSfxVolumeToActiveLoops()
        {
            foreach (KeyValuePair<int, AudioSource> pair in activeLoops)
            {
                AudioSource source = pair.Value;
                if (source == null || source.clip == null)
                {
                    continue;
                }

                BTAudioCueId cue;
                if (!activeLoopCues.TryGetValue(pair.Key, out cue))
                {
                    continue;
                }

                source.volume = GetNormalizedSfxVolume(cue, source.clip) * userSfxVolume;
            }
        }

        private void PlaySfxInternal(BTAudioCueId cue)
        {
            PlaySfxInternal(cue, ignoreButtonClickSuppression: false);
        }

        private void PlaySfxInternal(BTAudioCueId cue, bool ignoreButtonClickSuppression)
        {
            if (cue == BTAudioCueId.None)
            {
                return;
            }

            if (!ignoreButtonClickSuppression && ShouldSuppressImmediateButtonClickDuplicate(cue))
            {
                return;
            }

            EnsureSources();
            AudioClip clip = LoadClip(cue);
            if (clip == null)
            {
                return;
            }

            MarkUiClickFeedbackFrame(cue);
            sfxSource.PlayOneShot(clip, GetNormalizedSfxVolume(cue, clip) * userSfxVolume);
        }

        private void PlaySfxInternal(BTAudioCueId cue, float volumeScale, float pitch)
        {
            if (cue == BTAudioCueId.None)
            {
                return;
            }

            if (ShouldSuppressImmediateButtonClickDuplicate(cue))
            {
                return;
            }

            EnsureSources();
            AudioClip clip = LoadClip(cue);
            if (clip == null)
            {
                return;
            }

            MarkUiClickFeedbackFrame(cue);
            AudioSource source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = false;
            source.spatialBlend = 0f;
            source.pitch = Mathf.Clamp(pitch, 0.35f, 2f);
            source.PlayOneShot(clip, Mathf.Clamp(GetNormalizedSfxVolume(cue, clip) * userSfxVolume * Mathf.Max(0f, volumeScale), 0f, 1.8f));
            Destroy(source, Mathf.Max(0.05f, clip.length / Mathf.Max(0.1f, Mathf.Abs(source.pitch))) + 0.1f);
        }

        private bool WasExplicitUiClickFeedbackPlayedSinceFrameInternal(int frame)
        {
            return lastButtonClickFrame >= frame || lastInvalidActionFrame >= frame;
        }

        private void PlayImmediateUiPressFeedbackInternal()
        {
            PlaySfxInternal(BTAudioCueId.ButtonClick, ignoreButtonClickSuppression: true);
            suppressNextButtonClick = true;
            suppressNextButtonClickExpiresAt = Time.unscaledTime + ImmediateUiPressFeedbackSuppressionSeconds;
        }

        private bool ShouldSuppressImmediateButtonClickDuplicate(BTAudioCueId cue)
        {
            if (cue != BTAudioCueId.ButtonClick || !suppressNextButtonClick)
            {
                return false;
            }

            if (Time.unscaledTime > suppressNextButtonClickExpiresAt)
            {
                ClearPendingImmediateUiPressFeedbackSuppressionInternal();
                return false;
            }

            suppressNextButtonClick = false;
            MarkUiClickFeedbackFrame(cue);
            return true;
        }

        private void ClearPendingImmediateUiPressFeedbackSuppressionInternal()
        {
            suppressNextButtonClick = false;
            suppressNextButtonClickExpiresAt = 0f;
        }

        private void MarkUiClickFeedbackFrame(BTAudioCueId cue)
        {
            if (cue == BTAudioCueId.ButtonClick)
            {
                lastButtonClickFrame = Time.frameCount;
                return;
            }

            if (cue == BTAudioCueId.InvalidAction)
            {
                lastInvalidActionFrame = Time.frameCount;
            }
        }

        private void PlayBgmInternal(BTAudioCueId cue)
        {
            if (cue == BTAudioCueId.None || currentBgmCue == cue)
            {
                return;
            }

            EnsureSources();
            AudioClip clip = LoadClip(cue);
            if (clip == null)
            {
                return;
            }

            if (bgmFadeRoutine != null)
            {
                StopCoroutine(bgmFadeRoutine);
                bgmFadeRoutine = null;
            }

            currentBgmCue = cue;
            bgmSource.Stop();
            bgmSource.clip = clip;
            bgmSource.loop = true;
            bgmSource.volume = GetTargetBgmVolume(cue, clip);
            bgmSource.Play();
        }

        private void PlayMusicOnceInternal(BTAudioCueId cue)
        {
            if (cue == BTAudioCueId.None)
            {
                return;
            }

            EnsureSources();
            AudioClip clip = LoadClip(cue);
            if (clip == null)
            {
                return;
            }

            currentBgmCue = cue;
            bgmSource.Stop();
            bgmSource.clip = clip;
            bgmSource.loop = false;
            bgmSource.volume = GetTargetBgmVolume(cue, clip);
            bgmSource.Play();
        }

        private void PlaySceneBgm(string sceneName, bool forceRestart = false)
        {
            if (Normalize(sceneName).Contains("endmenu")
                && (currentBgmCue == BTAudioCueId.BgmVictory || currentBgmCue == BTAudioCueId.BgmDefeat)
                && bgmSource != null
                && bgmSource.isPlaying)
            {
                return;
            }

            BTAudioCueId cue = GetSceneBgmCue(sceneName);
            float fadeDuration = pendingNextSceneBgmFadeIn ? pendingNextSceneBgmFadeDuration : 0f;
            pendingNextSceneBgmFadeIn = false;
            pendingNextSceneBgmFadeDuration = 0f;
            if (cue == BTAudioCueId.None)
            {
                StopBgmInternal();
                return;
            }

            PlayBgmInternal(cue, fadeDuration, forceRestart);
        }

        private void StopBgmInternal()
        {
            if (bgmFadeRoutine != null)
            {
                StopCoroutine(bgmFadeRoutine);
                bgmFadeRoutine = null;
            }

            currentBgmCue = BTAudioCueId.None;
            if (bgmSource != null)
            {
                bgmSource.Stop();
                bgmSource.clip = null;
            }
        }

        private void PlayBgmInternal(BTAudioCueId cue, float fadeInSeconds, bool forceRestart = false)
        {
            if (cue == BTAudioCueId.None)
            {
                return;
            }

            EnsureSources();
            AudioClip clip = LoadClip(cue);
            if (clip == null)
            {
                return;
            }

            float clampedFade = Mathf.Max(0f, fadeInSeconds);
            float targetVolume = GetTargetBgmVolume(cue, clip);

            if (!forceRestart && clampedFade <= 0f && IsFadingSameBgm(cue, clip))
            {
                if (!bgmSource.isPlaying)
                {
                    bgmSource.Play();
                }

                return;
            }

            if (bgmFadeRoutine != null)
            {
                StopCoroutine(bgmFadeRoutine);
                bgmFadeRoutine = null;
            }

            bool shouldRestartClip = forceRestart || currentBgmCue != cue || bgmSource.clip != clip || !bgmSource.isPlaying;
            currentBgmCue = cue;
            bgmSource.loop = true;

            if (shouldRestartClip)
            {
                bgmSource.Stop();
                bgmSource.clip = clip;
            }

            if (clampedFade <= 0f)
            {
                bgmSource.volume = targetVolume;
                if (!bgmSource.isPlaying)
                {
                    bgmSource.Play();
                }

                return;
            }

            bgmSource.volume = 0f;
            if (!bgmSource.isPlaying)
            {
                bgmSource.Play();
            }

            bgmFadeRoutine = StartCoroutine(FadeInBgmRoutine(cue, clip, clampedFade));
        }

        private bool IsFadingSameBgm(BTAudioCueId cue, AudioClip clip)
        {
            return bgmFadeRoutine != null
                && bgmSource != null
                && bgmSource.clip == clip
                && currentBgmCue == cue;
        }

        private void PlayLoopInternal(BTAudioCueId cue, Object owner)
        {
            if (cue == BTAudioCueId.None || owner == null)
            {
                return;
            }

            AudioClip clip = LoadClip(cue);
            if (clip == null)
            {
                return;
            }

            int ownerId = owner.GetInstanceID();
            AudioSource source;
            if (!activeLoops.TryGetValue(ownerId, out source) || source == null)
            {
                source = gameObject.AddComponent<AudioSource>();
                source.playOnAwake = false;
                source.spatialBlend = 0f;
                activeLoops[ownerId] = source;
            }

            if (source.isPlaying && source.clip == clip)
            {
                return;
            }

            source.Stop();
            source.clip = clip;
            source.loop = true;
            source.pitch = cue == BTAudioCueId.FootstepLargeLoop ? 0.82f : 1f;
            source.volume = GetNormalizedSfxVolume(cue, clip) * userSfxVolume;
            source.Play();
            activeLoopCues[ownerId] = cue;
        }

        private void StopLoopInternal(Object owner)
        {
            if (owner == null)
            {
                return;
            }

            int ownerId = owner.GetInstanceID();
            AudioSource source;
            if (!activeLoops.TryGetValue(ownerId, out source))
            {
                return;
            }

            if (source != null)
            {
                source.Stop();
                Destroy(source);
            }

            activeLoops.Remove(ownerId);
            activeLoopCues.Remove(ownerId);
        }

        private void StopAllNonBgmLoopsInternal()
        {
            foreach (KeyValuePair<int, AudioSource> pair in activeLoops)
            {
                if (pair.Value != null)
                {
                    pair.Value.Stop();
                    Destroy(pair.Value);
                }
            }

            activeLoops.Clear();
            activeLoopCues.Clear();
        }

        private AudioClip LoadClip(BTAudioCueId cue)
        {
            AudioClip cached;
            if (clipCache.TryGetValue(cue, out cached))
            {
                return cached;
            }

            string path;
            if (!ClipPaths.TryGetValue(cue, out path))
            {
                Debug.LogWarning("BTAudioService has no Resources path for cue " + cue + ".", this);
                return null;
            }

            AudioClip clip = Resources.Load<AudioClip>(path);
            if (clip == null)
            {
                Debug.LogWarning("BTAudioService could not load audio cue " + cue + " at Resources path " + path + ".", this);
                return null;
            }

            clipCache[cue] = clip;
            return clip;
        }

        private void WarmUpUiSfx()
        {
            if (uiSfxWarmed)
            {
                return;
            }

            uiSfxWarmed = true;
            WarmUpSfxCue(BTAudioCueId.ButtonClick);
            WarmUpSfxCue(BTAudioCueId.MouseClick);
            WarmUpSfxCue(BTAudioCueId.InvalidAction);
            WarmUpSfxCue(BTAudioCueId.Page);
        }

        private void WarmUpSfxCue(BTAudioCueId cue)
        {
            AudioClip clip = LoadClip(cue);
            if (clip != null)
            {
                if (clip.loadState == AudioDataLoadState.Unloaded)
                {
                    clip.LoadAudioData();
                }

                GetNormalizedSfxVolume(cue, clip);
            }
        }

        private float GetNormalizedSfxVolume(BTAudioCueId cue, AudioClip clip)
        {
            float cached;
            if (sfxVolumeCache.TryGetValue(cue, out cached))
            {
                return cached;
            }

            float rms = TryCalculateRms(clip);
            float volume = rms > MinAnalyzedRms
                ? Mathf.Clamp(TargetSfxRms / rms, 0.15f, MaxNormalizedSfxVolume)
                : DefaultSfxVolume;
            if (cue == BTAudioCueId.FootstepLargeLoop)
            {
                volume = Mathf.Clamp(volume * 1.2f, 0.15f, MaxNormalizedSfxVolume);
            }

            sfxVolumeCache[cue] = volume;
            return volume;
        }

        private float GetNormalizedMusicVolume(BTAudioCueId cue, AudioClip clip)
        {
            float cached;
            if (musicVolumeCache.TryGetValue(cue, out cached))
            {
                return cached;
            }

            float rms = TryCalculateRms(clip);
            float volume = rms > MinAnalyzedRms
                ? Mathf.Clamp(TargetMusicRms / rms, 0.12f, MaxNormalizedMusicVolume)
                : DefaultBgmVolume;
            musicVolumeCache[cue] = volume;
            return volume;
        }

        private float GetTargetBgmVolume(BTAudioCueId cue, AudioClip clip)
        {
            return GetNormalizedMusicVolume(cue, clip) * userBgmVolume;
        }

        private IEnumerator FadeInBgmRoutine(BTAudioCueId cue, AudioClip clip, float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                if (bgmSource == null || bgmSource.clip != clip || currentBgmCue != cue)
                {
                    bgmFadeRoutine = null;
                    yield break;
                }

                elapsed += Time.unscaledDeltaTime;
                float normalized = Mathf.Clamp01(elapsed / duration);
                bgmSource.volume = Mathf.Lerp(0f, GetTargetBgmVolume(cue, clip), normalized);
                yield return null;
            }

            if (bgmSource != null && bgmSource.clip == clip && currentBgmCue == cue)
            {
                bgmSource.volume = GetTargetBgmVolume(cue, clip);
            }

            bgmFadeRoutine = null;
        }

        private static float TryCalculateRms(AudioClip clip)
        {
            if (clip == null || clip.samples <= 0 || clip.channels <= 0)
            {
                return 0f;
            }

            int sampleCount = Mathf.Min(clip.samples * clip.channels, 44100);
            float[] samples = new float[sampleCount];
            try
            {
                if (!clip.GetData(samples, 0))
                {
                    return 0f;
                }
            }
            catch (System.Exception)
            {
                return 0f;
            }

            double sum = 0d;
            for (int i = 0; i < samples.Length; i++)
            {
                sum += samples[i] * samples[i];
            }

            return Mathf.Sqrt((float)(sum / samples.Length));
        }

        private static BTAudioCueId GetSceneBgmCue(string sceneName)
        {
            string normalized = Normalize(sceneName);
            if (normalized.Contains("startmenu"))
            {
                return BTAudioCueId.BgmMenu;
            }

            if (normalized.Contains("introstory"))
            {
                return BTAudioCueId.BgmMenu;
            }

            if (normalized.Contains("endmenu"))
            {
                return BTAudioCueId.None;
            }

            return BTAudioCueId.BgmBattle;
        }

        private static bool IsBossLikeUnit(Unit unit)
        {
            if (unit == null)
            {
                return false;
            }

            string objectName = Normalize(unit.name);
            string displayName = Normalize(unit.DisplayName);
            return objectName.Contains("boss")
                || objectName.Contains("golem")
                || objectName.Contains("large")
                || displayName.Contains("boss")
                || displayName.Contains("golem")
                || displayName.Contains("large");
        }

        private static bool IsAxeSkeleton(Unit unit)
        {
            if (unit == null)
            {
                return false;
            }

            string objectName = Normalize(unit.name);
            string displayName = Normalize(unit.DisplayName);
            return ContainsSkeletonAxeName(objectName) || ContainsSkeletonAxeName(displayName);
        }

        private static bool ContainsSkeletonAxeName(string normalizedName)
        {
            if (string.IsNullOrEmpty(normalizedName))
            {
                return false;
            }

            return normalizedName.Contains("skeleton_warrior")
                || normalizedName.Contains("skeleton warrior")
                || normalizedName.Contains("skeleton_minion")
                || normalizedName.Contains("skeleton minion");
        }

        private static string Normalize(string value)
        {
            return string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToLowerInvariant();
        }
    }
}
