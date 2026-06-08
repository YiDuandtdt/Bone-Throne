using System.Collections;
using UnityEngine;

namespace BoneThrone.Audio
{
    public sealed class BTInteractionVfxService : MonoBehaviour
    {
        private const string LevelUpVfxPath = "BoneThroneVFX/Interaction/BT_VFX_LevelUp";
        private const string KeyPickupVfxPath = "BoneThroneVFX/Interaction/BT_VFX_KeyPickup";
        private const float LevelUpVisibleSeconds = 2f;
        private const float LevelUpFadeSeconds = 0.8f;
        private const float KeyPickupLifetimeSeconds = 1.5f;

        private static BTInteractionVfxService instance;

        public static void PlayLevelUp(Vector3 position)
        {
            EnsureInstance().PlayVfx(LevelUpVfxPath, position, LevelUpVisibleSeconds, LevelUpFadeSeconds);
        }

        public static void PlayKeyPickup(Vector3 position)
        {
            EnsureInstance().PlayVfx(KeyPickupVfxPath, position, KeyPickupLifetimeSeconds, 0.25f);
        }

        private static BTInteractionVfxService EnsureInstance()
        {
            if (instance != null)
            {
                return instance;
            }

            BTInteractionVfxService existing = Object.FindFirstObjectByType<BTInteractionVfxService>();
            if (existing != null)
            {
                instance = existing;
                return instance;
            }

            GameObject host = new GameObject("BTInteractionVfxService_Runtime");
            DontDestroyOnLoad(host);
            instance = host.AddComponent<BTInteractionVfxService>();
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
        }

        private void PlayVfx(string resourcesPath, Vector3 position, float visibleSeconds, float fadeSeconds)
        {
            GameObject prefab = Resources.Load<GameObject>(resourcesPath);
            if (prefab == null)
            {
                Debug.LogWarning("BTInteractionVfxService could not load VFX prefab at Resources path " + resourcesPath + ".", this);
                return;
            }

            GameObject instanceObject = Instantiate(prefab, position, Quaternion.identity);
            StartCoroutine(StopAndDestroyAfterDelay(instanceObject, visibleSeconds, fadeSeconds));
        }

        private IEnumerator StopAndDestroyAfterDelay(GameObject instanceObject, float visibleSeconds, float fadeSeconds)
        {
            if (instanceObject == null)
            {
                yield break;
            }

            float visibleDuration = Mathf.Max(0f, visibleSeconds);
            if (visibleDuration > 0f)
            {
                yield return new WaitForSeconds(visibleDuration);
            }

            ParticleSystem[] particleSystems = instanceObject.GetComponentsInChildren<ParticleSystem>(true);
            for (int i = 0; i < particleSystems.Length; i++)
            {
                ParticleSystem system = particleSystems[i];
                if (system != null)
                {
                    system.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                }
            }

            float fadeDuration = Mathf.Max(0f, fadeSeconds);
            if (fadeDuration > 0f)
            {
                yield return new WaitForSeconds(fadeDuration);
            }

            if (instanceObject != null)
            {
                Destroy(instanceObject);
            }
        }
    }
}
