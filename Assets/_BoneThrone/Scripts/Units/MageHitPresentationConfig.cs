using BoneThrone.Skills;
using UnityEngine;

namespace BoneThrone.Units
{
    /// <summary>
    /// Holds Mage-style hit effects that appear on the target when damage triggers the Hit animation.
    /// </summary>
    public sealed class MageHitPresentationConfig : MonoBehaviour
    {
        [Header("普通攻击")]
        [SerializeField] [Tooltip("普通攻击造成伤害时，在目标身上生成的命中特效。")]
        private GameObject basicAttackImpactEffectPrefab;
        [SerializeField] private Vector3 basicAttackLocalOffset = new Vector3(0f, 1.05f, 0f);
        [SerializeField] private Vector3 basicAttackLocalEulerAngles = Vector3.zero;
        [SerializeField] private Vector3 basicAttackLocalScale = Vector3.one;

        [Header("Skill Effects")]
        [SerializeField] [Tooltip("Effect attached to the target for mage_fireball.")]
        private GameObject fireballImpactEffectPrefab;
        [SerializeField] private Vector3 fireballLocalOffset = new Vector3(0f, 1.05f, 0f);
        [SerializeField] private Vector3 fireballLocalEulerAngles = Vector3.zero;
        [SerializeField] private Vector3 fireballLocalScale = Vector3.one;

        [SerializeField] [Tooltip("Effect attached to the target for mage_frost_bolt.")]
        private GameObject frostBoltImpactEffectPrefab;
        [SerializeField] private Vector3 frostBoltLocalOffset = new Vector3(0f, 1.05f, 0f);
        [SerializeField] private Vector3 frostBoltLocalEulerAngles = Vector3.zero;
        [SerializeField] private Vector3 frostBoltLocalScale = Vector3.one;

        [SerializeField] [Tooltip("Effect attached to the target for mage_arcane_burst.")]
        private GameObject arcaneBurstImpactEffectPrefab;
        [SerializeField] private Vector3 arcaneBurstLocalOffset = new Vector3(0f, 1.05f, 0f);
        [SerializeField] private Vector3 arcaneBurstLocalEulerAngles = Vector3.zero;
        [SerializeField] private Vector3 arcaneBurstLocalScale = Vector3.one;

        [SerializeField] [Tooltip("Optional extra area effect for mage_arcane_burst.")]
        private GameObject arcaneBurstAreaEffectPrefab;
        [SerializeField] private Vector3 arcaneBurstAreaLocalOffset = Vector3.zero;
        [SerializeField] private Vector3 arcaneBurstAreaLocalEulerAngles = Vector3.zero;
        [SerializeField] private Vector3 arcaneBurstAreaLocalScale = Vector3.one;

        [SerializeField] [Tooltip("Default cleanup lifetime for attached Mage hit effects when the prefab does not destroy itself.")]
        [Min(0.1f)]
        private float defaultEffectLifetimeSeconds = 2.5f;

        public void TryPlayBasicAttackImpactEffect(Unit attacker, Unit target)
        {
            if (attacker == null || target == null)
            {
                return;
            }

            TrySpawnAttachedEffect(
                target,
                basicAttackImpactEffectPrefab,
                basicAttackLocalOffset,
                basicAttackLocalEulerAngles,
                basicAttackLocalScale);
        }

        public void TryPlaySkillImpactEffect(Unit caster, Unit target, SkillData skill)
        {
            if (caster == null || target == null || skill == null)
            {
                return;
            }

            string normalizedSkillName = NormalizeSkillName(skill.DisplayName);
            if (normalizedSkillName == NormalizeSkillName("mage_fireball"))
            {
                TrySpawnAttachedEffect(target, fireballImpactEffectPrefab, fireballLocalOffset, fireballLocalEulerAngles, fireballLocalScale);
                return;
            }

            if (normalizedSkillName == NormalizeSkillName("mage_frost_bolt"))
            {
                TrySpawnAttachedEffect(target, frostBoltImpactEffectPrefab, frostBoltLocalOffset, frostBoltLocalEulerAngles, frostBoltLocalScale);
                return;
            }

            if (normalizedSkillName == NormalizeSkillName("mage_arcane_burst"))
            {
                TrySpawnAttachedEffect(target, arcaneBurstImpactEffectPrefab, arcaneBurstLocalOffset, arcaneBurstLocalEulerAngles, arcaneBurstLocalScale);
                TrySpawnAttachedEffect(target, arcaneBurstAreaEffectPrefab, arcaneBurstAreaLocalOffset, arcaneBurstAreaLocalEulerAngles, arcaneBurstAreaLocalScale);
            }
        }

        private void TrySpawnAttachedEffect(Unit target, GameObject prefab, Vector3 localOffset, Vector3 localEulerAngles, Vector3 localScale)
        {
            try
            {
                if (prefab == null || target == null)
                {
                    return;
                }

                Transform targetAnchor = ResolveTargetAnchor(target);
                if (targetAnchor == null)
                {
                    return;
                }

                GameObject instance = Instantiate(prefab);
                if (instance == null)
                {
                    return;
                }

                instance.transform.SetParent(targetAnchor, false);
                instance.transform.localPosition = localOffset;
                instance.transform.localRotation = Quaternion.Euler(localEulerAngles);
                instance.transform.localScale = localScale;
                PrepareSpawnedEffect(instance);
                Destroy(instance, GetSuggestedEffectLifetime(instance));
            }
            catch (System.Exception exception)
            {
                Debug.LogWarning($"Failed to spawn Mage hit effect: {exception.Message}", this);
            }
        }

        private static Transform ResolveTargetAnchor(Unit target)
        {
            if (target == null)
            {
                return null;
            }

            Animator animator = target.GetComponentInChildren<Animator>(true);
            return animator != null ? animator.transform : target.transform;
        }

        private static void PrepareSpawnedEffect(GameObject effectInstance)
        {
            if (effectInstance == null)
            {
                return;
            }

            effectInstance.SetActive(true);
            DisableRuntimeBehaviours(effectInstance);
            EnsureRenderersEnabled(effectInstance);
            ParticleSystem[] particleSystems = effectInstance.GetComponentsInChildren<ParticleSystem>(true);
            for (int i = 0; i < particleSystems.Length; i++)
            {
                ParticleSystem system = particleSystems[i];
                if (system == null)
                {
                    continue;
                }

                system.gameObject.SetActive(true);
                system.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                system.Clear(true);
                system.Play(true);
            }
        }

        private static void DisableRuntimeBehaviours(GameObject effectInstance)
        {
            MonoBehaviour[] behaviours = effectInstance.GetComponentsInChildren<MonoBehaviour>(true);
            for (int i = 0; i < behaviours.Length; i++)
            {
                MonoBehaviour behaviour = behaviours[i];
                if (behaviour != null)
                {
                    behaviour.enabled = false;
                }
            }
        }

        private static void EnsureRenderersEnabled(GameObject instance)
        {
            Renderer[] renderers = instance.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                if (renderer != null)
                {
                    renderer.enabled = true;
                }
            }
        }

        private float GetSuggestedEffectLifetime(GameObject effectInstance)
        {
            float suggestedLifetime = Mathf.Max(0.1f, defaultEffectLifetimeSeconds);
            ParticleSystem[] particleSystems = effectInstance.GetComponentsInChildren<ParticleSystem>(true);
            for (int i = 0; i < particleSystems.Length; i++)
            {
                ParticleSystem system = particleSystems[i];
                if (system == null)
                {
                    continue;
                }

                ParticleSystem.MainModule main = system.main;
                if (main.loop)
                {
                    continue;
                }

                suggestedLifetime = Mathf.Max(suggestedLifetime, main.duration + GetCurveMax(main.startLifetime) + 0.35f);
            }

            return suggestedLifetime;
        }

        private static float GetCurveMax(ParticleSystem.MinMaxCurve curve)
        {
            switch (curve.mode)
            {
                case ParticleSystemCurveMode.Constant:
                    return curve.constant;
                case ParticleSystemCurveMode.TwoConstants:
                    return Mathf.Max(curve.constantMin, curve.constantMax);
                case ParticleSystemCurveMode.Curve:
                    return curve.curve != null ? curve.curve.Evaluate(1f) * curve.curveMultiplier : curve.curveMultiplier;
                case ParticleSystemCurveMode.TwoCurves:
                    float minValue = curve.curveMin != null ? curve.curveMin.Evaluate(1f) : 0f;
                    float maxValue = curve.curveMax != null ? curve.curveMax.Evaluate(1f) : 0f;
                    return Mathf.Max(minValue, maxValue) * curve.curveMultiplier;
                default:
                    return 0f;
            }
        }

        private static string NormalizeSkillName(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            string lower = value.ToLowerInvariant();
            char[] buffer = new char[lower.Length];
            int count = 0;
            for (int i = 0; i < lower.Length; i++)
            {
                char c = lower[i];
                if ((c >= 'a' && c <= 'z') || (c >= '0' && c <= '9'))
                {
                    buffer[count] = c;
                    count++;
                }
            }

            return new string(buffer, 0, count);
        }
    }
}
