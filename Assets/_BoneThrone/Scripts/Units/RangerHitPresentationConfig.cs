using BoneThrone.Core;
using BoneThrone.Skills;
using BoneThrone.Turns;
using UnityEngine;

namespace BoneThrone.Units
{
    /// <summary>
    /// Holds Ranger-specific hit presentation assets and tuning so the prefab can drive arrow and skill visuals.
    /// </summary>
    public sealed class RangerHitPresentationConfig : MonoBehaviour
    {
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorId = Shader.PropertyToID("_Color");
        private static readonly int TintColorId = Shader.PropertyToID("_TintColor");
        private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

        [Header("Applicability")]
        [SerializeField] [Tooltip("Keep off for the player Ranger. Enable only on enemy archer-like units that should reuse this arrow presentation.")]
        private bool allowAnyUnitWithThisComponent;

        [Header("Arrow Prefabs")]
        [SerializeField] [Tooltip("Arrow prefab for Ranger basic attacks. Replace this if you want to swap to the skeleton arrow model.")]
        private GameObject basicAttackArrowPrefab;
        [SerializeField] [Tooltip("Arrow prefab for Ranger skills. Leave empty to reuse the basic attack arrow prefab.")]
        private GameObject skillArrowPrefab;

        [Header("Arrow Flight")]
        [SerializeField] [Tooltip("How far in front of the target the arrow starts before it flies into the body. 0.5 means half a tile in the current grid scale.")]
        [Min(0f)]
        private float arrowStartDistanceFromTarget = 0.5f;
        [SerializeField] [Tooltip("Travel time for the short impact arrow flight.")]
        [Min(0f)]
        private float arrowFlightDuration = 0.12f;
        [SerializeField] [Tooltip("Rotation offset applied after the arrow is aligned to the attack direction. Use this when the mesh looks sideways or backwards.")]
        private Vector3 arrowRotationOffsetEuler = Vector3.zero;
        [SerializeField] [Tooltip("World scale applied to spawned arrows.")]
        private Vector3 arrowWorldScale = Vector3.one;

        [Header("Embedded Arrow")]
        [SerializeField] [Tooltip("Local offset on the target body where the arrow stays embedded after impact.")]
        private Vector3 embeddedArrowLocalOffset = new Vector3(0f, 1.05f, 0f);
        [SerializeField] [Tooltip("How many enemy turns the embedded arrow remains before it disappears.")]
        [Min(1)]
        private int embeddedArrowLifetimeEnemyTurns = 2;
        [SerializeField] [Tooltip("Fallback lifetime used when TurnManager is not available in a test scene.")]
        [Min(0.1f)]
        private float fallbackArrowLifetimeSeconds = 5f;

        [Header("Skill Effects")]
        [SerializeField] [Tooltip("Effect attached to the target for ranger_precision_shot.")]
        private GameObject precisionShotEffectPrefab;
        [SerializeField] private Vector3 precisionShotLocalOffset = new Vector3(0f, 1.05f, 0f);
        [SerializeField] private Vector3 precisionShotLocalEulerAngles = Vector3.zero;
        [SerializeField] private Vector3 precisionShotLocalScale = Vector3.one;
        [SerializeField] private Color precisionShotTint = new Color(0.95f, 0.28f, 0.06f, 1f);

        [SerializeField] [Tooltip("Effect attached to the target for ranger_quick_shot.")]
        private GameObject quickShotEffectPrefab;
        [SerializeField] private Vector3 quickShotLocalOffset = new Vector3(0f, 1.05f, 0f);
        [SerializeField] private Vector3 quickShotLocalEulerAngles = Vector3.zero;
        [SerializeField] private Vector3 quickShotLocalScale = Vector3.one;
        [SerializeField] private Color quickShotTint = new Color(0.85f, 0.08f, 0.95f, 1f);

        [SerializeField] [Tooltip("Effect attached to the target for ranger_piercing_arrow.")]
        private GameObject piercingArrowEffectPrefab;
        [SerializeField] private Vector3 piercingArrowLocalOffset = new Vector3(0f, 1.05f, 0f);
        [SerializeField] private Vector3 piercingArrowLocalEulerAngles = Vector3.zero;
        [SerializeField] private Vector3 piercingArrowLocalScale = Vector3.one;
        [SerializeField] private Color piercingArrowTint = new Color(0.05f, 0.42f, 1f, 1f);

        [SerializeField] [Tooltip("Default cleanup lifetime for attached skill effects when the prefab does not destroy itself.")]
        [Min(0.1f)]
        private float defaultSkillEffectLifetimeSeconds = 2.5f;
        [SerializeField] [Tooltip("Emission multiplier for Ranger 1/2/3 hit effects. Increase if the hit particles still read too faintly in the battle camera.")]
        [Min(0f)]
        private float skillEffectEmissionMultiplier = 1.8f;

        public float TryPlayBasicAttackArrow(Unit attacker, Unit target, TurnManager turnManager)
        {
            try
            {
                return TryPlayArrow(attacker, target, turnManager, basicAttackArrowPrefab);
            }
            catch (System.Exception exception)
            {
                Debug.LogWarning($"Failed to spawn Ranger basic attack arrow presentation: {exception.Message}", this);
                return 0f;
            }
        }

        public float TryPlaySkillArrow(Unit attacker, Unit target, TurnManager turnManager)
        {
            try
            {
                GameObject prefab = skillArrowPrefab != null ? skillArrowPrefab : basicAttackArrowPrefab;
                return TryPlayArrow(attacker, target, turnManager, prefab);
            }
            catch (System.Exception exception)
            {
                Debug.LogWarning($"Failed to spawn Ranger skill arrow presentation: {exception.Message}", this);
                return 0f;
            }
        }

        public void TryPlaySkillImpactEffect(Unit caster, Unit target, SkillData skill)
        {
            try
            {
                if (!ShouldUseRangerPresentation(caster) || target == null || skill == null)
                {
                    return;
                }

                AttachedEffectBinding binding;
                if (!TryResolveEffectBinding(skill, out binding) || binding.Prefab == null)
                {
                    return;
                }

                Transform targetAnchor;
                if (!TryGetTargetAnchor(target, out targetAnchor))
                {
                    return;
                }

                GameObject instance = InstantiatePrefabAsGameObject(binding.Prefab);
                if (instance == null)
                {
                    Debug.LogWarning($"Ranger skill effect '{binding.Prefab.name}' could not be instantiated as a GameObject.", this);
                    return;
                }

                Quaternion prefabRootRotation = instance.transform.rotation;
                instance.transform.SetParent(targetAnchor, false);
                instance.transform.localPosition = binding.LocalOffset;
                instance.transform.localRotation = Quaternion.Euler(binding.LocalEulerAngles) * prefabRootRotation;
                instance.transform.localScale = binding.LocalScale;
                PrepareSpawnedEffect(instance, binding.Tint, skillEffectEmissionMultiplier);
                Destroy(instance, GetSuggestedEffectLifetime(instance));
            }
            catch (System.Exception exception)
            {
                Debug.LogWarning($"Failed to spawn Ranger skill impact effect: {exception.Message}", this);
            }
        }

        private float TryPlayArrow(Unit attacker, Unit target, TurnManager turnManager, GameObject arrowPrefab)
        {
            if (!ShouldUseRangerPresentation(attacker) || target == null || arrowPrefab == null)
            {
                return 0f;
            }

            Transform targetAnchor;
            if (!TryGetTargetAnchor(target, out targetAnchor))
            {
                return 0f;
            }

            Vector3 impactLocalOffset = ResolveCenteredArrowLocalOffset();
            Vector3 impactWorld = targetAnchor.TransformPoint(impactLocalOffset);
            Vector3 direction = ResolveAttackDirection(attacker, impactWorld);
            Vector3 startWorld = impactWorld - direction * Mathf.Max(0f, arrowStartDistanceFromTarget);
            Quaternion arrowRotation = Quaternion.LookRotation(direction, Vector3.up) * Quaternion.Euler(arrowRotationOffsetEuler);

            GameObject instance = InstantiatePrefabAsGameObject(arrowPrefab);
            if (instance == null)
            {
                Debug.LogWarning($"Ranger arrow prefab '{arrowPrefab.name}' could not be instantiated as a GameObject.", this);
                return 0f;
            }

            instance.transform.SetPositionAndRotation(startWorld, arrowRotation);
            instance.transform.localScale = arrowWorldScale;
            EnsureRenderersEnabled(instance);

            RangerEmbeddedArrow embeddedArrow = instance.GetComponent<RangerEmbeddedArrow>();
            if (embeddedArrow == null)
            {
                embeddedArrow = instance.AddComponent<RangerEmbeddedArrow>();
            }

            embeddedArrow.Initialize(
                startWorld,
                targetAnchor,
                impactLocalOffset,
                arrowRotation,
                embeddedArrowLifetimeEnemyTurns,
                fallbackArrowLifetimeSeconds,
                arrowFlightDuration,
                turnManager);

            return Mathf.Max(0f, arrowFlightDuration);
        }

        private Vector3 ResolveCenteredArrowLocalOffset()
        {
            return new Vector3(0f, embeddedArrowLocalOffset.y, 0f);
        }

        private static GameObject InstantiatePrefabAsGameObject(GameObject prefabReference)
        {
            if (prefabReference == null)
            {
                return null;
            }

            Object rawInstance = Instantiate((Object)prefabReference);
            if (rawInstance is GameObject gameObject)
            {
                return gameObject;
            }

            if (rawInstance is Component component)
            {
                return component.gameObject;
            }

            return null;
        }

        private static void PrepareSpawnedEffect(GameObject effectInstance, Color tint, float emissionMultiplier)
        {
            if (effectInstance == null)
            {
                return;
            }

            effectInstance.SetActive(true);
            DisableRuntimeBehaviours(effectInstance);
            EnsureRenderersEnabled(effectInstance);
            ApplyEffectTint(effectInstance, tint, emissionMultiplier);
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

        private static void ApplyEffectTint(GameObject effectInstance, Color tint, float emissionMultiplier)
        {
            ParticleSystem[] particleSystems = effectInstance.GetComponentsInChildren<ParticleSystem>(true);
            for (int i = 0; i < particleSystems.Length; i++)
            {
                ParticleSystem system = particleSystems[i];
                if (system == null)
                {
                    continue;
                }

                ParticleSystem.MainModule main = system.main;
                main.startColor = new ParticleSystem.MinMaxGradient(tint);
            }

            Renderer[] renderers = effectInstance.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                if (renderer == null)
                {
                    continue;
                }

                MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
                renderer.GetPropertyBlock(propertyBlock);
                propertyBlock.SetColor(BaseColorId, tint);
                propertyBlock.SetColor(ColorId, tint);
                propertyBlock.SetColor(TintColorId, tint);
                propertyBlock.SetColor(EmissionColorId, tint * Mathf.Max(0f, emissionMultiplier));
                renderer.SetPropertyBlock(propertyBlock);
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

        private bool TryResolveEffectBinding(SkillData skill, out AttachedEffectBinding binding)
        {
            string normalizedSkillName = NormalizeSkillName(skill.DisplayName);
            if (normalizedSkillName == NormalizeSkillName("ranger_precision_shot"))
            {
                binding = new AttachedEffectBinding(precisionShotEffectPrefab, precisionShotLocalOffset, precisionShotLocalEulerAngles, precisionShotLocalScale, precisionShotTint);
                return true;
            }

            if (normalizedSkillName == NormalizeSkillName("ranger_quick_shot"))
            {
                binding = new AttachedEffectBinding(quickShotEffectPrefab, quickShotLocalOffset, quickShotLocalEulerAngles, quickShotLocalScale, quickShotTint);
                return true;
            }

            if (normalizedSkillName == NormalizeSkillName("ranger_piercing_arrow"))
            {
                binding = new AttachedEffectBinding(piercingArrowEffectPrefab, piercingArrowLocalOffset, piercingArrowLocalEulerAngles, piercingArrowLocalScale, piercingArrowTint);
                return true;
            }

            binding = default(AttachedEffectBinding);
            return false;
        }

        private bool ShouldUseRangerPresentation(Unit unit)
        {
            return unit != null && (allowAnyUnitWithThisComponent || unit.RoleId == RoleId.Ranger);
        }

        private bool TryGetTargetAnchor(Unit target, out Transform anchor)
        {
            anchor = null;
            if (target == null)
            {
                return false;
            }

            Animator animator = target.GetComponentInChildren<Animator>(true);
            anchor = animator != null ? animator.transform : target.transform;
            return anchor != null;
        }

        private Vector3 ResolveAttackDirection(Unit attacker, Vector3 targetWorldPosition)
        {
            Vector3 attackerWorldPosition = ResolveUnitAnchorPosition(attacker);
            Vector3 direction = targetWorldPosition - attackerWorldPosition;
            direction.y = 0f;
            if (direction.sqrMagnitude <= 0.0001f)
            {
                direction = transform.forward;
                direction.y = 0f;
            }

            return direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector3.forward;
        }

        private static Vector3 ResolveUnitAnchorPosition(Unit unit)
        {
            if (unit == null)
            {
                return Vector3.zero;
            }

            Animator animator = unit.GetComponentInChildren<Animator>(true);
            return animator != null ? animator.transform.position : unit.transform.position;
        }

        private float GetSuggestedEffectLifetime(GameObject effectInstance)
        {
            float suggestedLifetime = Mathf.Max(0.1f, defaultSkillEffectLifetimeSeconds);
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

                float startLifetime = GetCurveMax(main.startLifetime);
                suggestedLifetime = Mathf.Max(suggestedLifetime, main.duration + startLifetime + 0.35f);
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

        private readonly struct AttachedEffectBinding
        {
            public AttachedEffectBinding(GameObject prefab, Vector3 localOffset, Vector3 localEulerAngles, Vector3 localScale, Color tint)
            {
                Prefab = prefab;
                LocalOffset = localOffset;
                LocalEulerAngles = localEulerAngles;
                LocalScale = localScale;
                Tint = tint;
            }

            public GameObject Prefab { get; }
            public Vector3 LocalOffset { get; }
            public Vector3 LocalEulerAngles { get; }
            public Vector3 LocalScale { get; }
            public Color Tint { get; }
        }
    }
}
