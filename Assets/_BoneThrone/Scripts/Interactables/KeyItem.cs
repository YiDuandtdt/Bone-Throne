using BoneThrone.Audio;
using BoneThrone.Levels;
using BoneThrone.Movement;
using BoneThrone.UI;
using BoneThrone.Units;
using UnityEngine;

namespace BoneThrone.Interactables
{
    /// <summary>
    /// Minimal Phase 10 floor pass-key pickup.
    /// It records a single party key state and does not implement inventory, key IDs, or rewards.
    /// </summary>
    public sealed class KeyItem : MonoBehaviour
    {
        private const float MinimumPracticalPickupRange = 2.5f;
        private const float ProximityPollInterval = 0.15f;

        [SerializeField] private LevelProgressionService progressionService;
        [SerializeField] private SelectionManager selectionManager;
        [SerializeField] private PromptView promptView;
        [SerializeField] private float pickupRange = 1.5f;
        [SerializeField] private bool createRuntimePickupTrigger = true;
        [SerializeField] private bool consumeOnCollect = true;
        [SerializeField] private bool collected;

        private float nextProximityPollTime;
        private bool hasCachedInteractionCenter;
        private Vector3 cachedLocalInteractionCenter;

        public bool Collected
        {
            get { return collected; }
        }

        private void Awake()
        {
            ResolveReferences();
            CacheInteractionCenter();
            EnsureRuntimePickupTrigger();
        }

        private void OnEnable()
        {
            nextProximityPollTime = 0f;
        }

        private void Update()
        {
            if (collected || Time.time < nextProximityPollTime)
            {
                return;
            }

            nextProximityPollTime = Time.time + ProximityPollInterval;

            Unit collector = FindNearestLivingPlayerInPickupRange();
            if (collector != null)
            {
                TryCollect(collector);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            TryCollectFromTrigger(other);
        }

        private void OnTriggerStay(Collider other)
        {
            TryCollectFromTrigger(other);
        }

        private void OnMouseDown()
        {
            ResolveReferences();
            Unit collector = selectionManager != null ? selectionManager.SelectedUnit : null;
            if (!TryCollectFromClick(collector))
            {
                ShowPrompt("让任意存活角色靠近通行钥匙即可拾取。", 2f);
                BTAudioService.PlaySfx(BTAudioCueId.InvalidAction);
            }
        }

        public bool TryCollectFromClick(Unit collector)
        {
            if (!IsValidCollector(collector) || !IsWithinPickupRange(collector))
            {
                Unit nearbyCollector = FindNearestLivingPlayerInPickupRange();
                collector = nearbyCollector != null ? nearbyCollector : collector;
            }

            if (!IsValidCollector(collector))
            {
                Debug.LogWarning("KeyItem click pickup ignored because no living player unit is close enough to the pass key.", this);
                return false;
            }

            if (!IsWithinPickupRange(collector))
            {
                Debug.LogWarning("KeyItem click pickup ignored because selected unit " + collector.UnitId + " is outside pickup range " + GetEffectivePickupRange() + ".", collector);
                return false;
            }

            return TryCollect(collector);
        }

        public bool TryCollect(Unit collector)
        {
            ResolveReferences();

            if (collected)
            {
                Debug.Log("KeyItem collection ignored because this key is already collected.", this);
                return false;
            }

            if (collector != null && !IsValidCollector(collector))
            {
                Debug.LogWarning("KeyItem ignored collection because the collector is not a living player unit.", collector);
                return false;
            }

            if (progressionService == null)
            {
                Debug.LogWarning("KeyItem cannot be collected because LevelProgressionService is missing.", this);
                ShowPrompt("通行钥匙暂时无法生效：关卡进度未绑定。", 2f);
                return false;
            }

            collected = true;
            progressionService.CollectSharedKey(this);
            BTAudioService.PlaySfx(BTAudioCueId.KeyPickup);
            BTInteractionVfxService.PlayKeyPickup(GetInteractionPosition());
            ShowPrompt("已获得通行钥匙，请前往楼梯。", 2f);

            if (consumeOnCollect)
            {
                gameObject.SetActive(false);
            }

            return true;
        }

        private void TryCollectFromTrigger(Collider other)
        {
            if (other == null || collected)
            {
                return;
            }

            Unit collector = other.GetComponentInParent<Unit>();
            if (!IsValidCollector(collector))
            {
                return;
            }

            TryCollect(collector);
        }

        private bool IsWithinPickupRange(Unit collector)
        {
            if (collector == null)
            {
                return false;
            }

            float clampedRange = GetEffectivePickupRange();
            Vector3 collectorPosition = collector.transform.position;
            Vector3 keyPosition = GetInteractionPosition();
            float deltaX = collectorPosition.x - keyPosition.x;
            float deltaZ = collectorPosition.z - keyPosition.z;
            float sqrDistance = deltaX * deltaX + deltaZ * deltaZ;
            return sqrDistance <= clampedRange * clampedRange;
        }

        private Unit FindNearestLivingPlayerInPickupRange()
        {
            Unit[] units = Object.FindObjectsByType<Unit>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            Unit nearestUnit = null;
            float pickupRangeSqr = GetEffectivePickupRange();
            pickupRangeSqr *= pickupRangeSqr;
            float nearestDistanceSqr = pickupRangeSqr;

            for (int i = 0; i < units.Length; i++)
            {
                Unit unit = units[i];
                if (!IsValidCollector(unit) || !unit.gameObject.activeInHierarchy)
                {
                    continue;
                }

                Vector3 unitPosition = unit.transform.position;
                Vector3 keyPosition = GetInteractionPosition();
                float deltaX = unitPosition.x - keyPosition.x;
                float deltaZ = unitPosition.z - keyPosition.z;
                float sqrDistance = deltaX * deltaX + deltaZ * deltaZ;
                if (sqrDistance <= nearestDistanceSqr)
                {
                    nearestDistanceSqr = sqrDistance;
                    nearestUnit = unit;
                }
            }

            return nearestUnit;
        }

        private bool IsValidCollector(Unit collector)
        {
            return collector != null
                && collector.Faction == UnitFaction.Player
                && collector.IsAlive;
        }

        private float GetEffectivePickupRange()
        {
            return Mathf.Max(MinimumPracticalPickupRange, pickupRange);
        }

        private void EnsureRuntimePickupTrigger()
        {
            if (!createRuntimePickupTrigger)
            {
                return;
            }

            SphereCollider pickupTrigger = GetComponent<SphereCollider>();
            if (pickupTrigger == null)
            {
                pickupTrigger = gameObject.AddComponent<SphereCollider>();
            }

            pickupTrigger.isTrigger = true;
            pickupTrigger.center = GetLocalInteractionCenter();
            pickupTrigger.radius = GetEffectivePickupRange();

            if (GetComponent<Rigidbody>() == null)
            {
                Rigidbody body = gameObject.AddComponent<Rigidbody>();
                body.isKinematic = true;
                body.useGravity = false;
            }
        }

        private Vector3 GetInteractionPosition()
        {
            return transform.TransformPoint(GetLocalInteractionCenter());
        }

        private Vector3 GetLocalInteractionCenter()
        {
            if (!hasCachedInteractionCenter)
            {
                CacheInteractionCenter();
            }

            return cachedLocalInteractionCenter;
        }

        private void CacheInteractionCenter()
        {
            Renderer[] renderers = GetComponentsInChildren<Renderer>(true);
            bool hasBounds = false;
            Bounds bounds = default;

            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                if (renderer == null)
                {
                    continue;
                }

                if (!hasBounds)
                {
                    bounds = renderer.bounds;
                    hasBounds = true;
                }
                else
                {
                    bounds.Encapsulate(renderer.bounds);
                }
            }

            cachedLocalInteractionCenter = hasBounds ? transform.InverseTransformPoint(bounds.center) : Vector3.zero;
            hasCachedInteractionCenter = true;
        }

        private void ResolveReferences()
        {
            if (selectionManager == null)
            {
                selectionManager = Object.FindFirstObjectByType<SelectionManager>();
            }

            if (progressionService == null)
            {
                progressionService = Object.FindFirstObjectByType<LevelProgressionService>();
            }

            if (promptView == null)
            {
                promptView = Object.FindFirstObjectByType<PromptView>();
            }
        }

        private void ShowPrompt(string message, float duration)
        {
            ResolveReferences();

            if (promptView != null)
            {
                promptView.ShowOverride(message, duration);
            }
        }

        [ContextMenu("Phase 10/Collect Key For Test")]
        public void CollectForTest()
        {
            TryCollect(null);
        }
    }
}
