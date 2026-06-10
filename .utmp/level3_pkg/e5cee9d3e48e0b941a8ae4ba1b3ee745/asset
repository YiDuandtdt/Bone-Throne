using BoneThrone.Levels;
using BoneThrone.Movement;
using BoneThrone.Units;
using UnityEngine;

namespace BoneThrone.Interactables
{
    /// <summary>
    /// Minimal Phase 10 shared key pickup.
    /// It records a single party key state and does not implement inventory, key IDs, or rewards.
    /// </summary>
    public sealed class KeyItem : MonoBehaviour
    {
        [SerializeField] private LevelProgressionService progressionService;
        [SerializeField] private SelectionManager selectionManager;
        [SerializeField] private float pickupRange = 1.5f;
        [SerializeField] private bool consumeOnCollect = true;
        [SerializeField] private bool collected;

        public bool Collected
        {
            get { return collected; }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other == null)
            {
                return;
            }

            Unit collector = other.GetComponentInParent<Unit>();
            TryCollect(collector);
        }

        private void OnMouseDown()
        {
            ResolveReferences();
            Unit collector = selectionManager != null ? selectionManager.SelectedUnit : null;
            TryCollectFromClick(collector);
        }

        public bool TryCollectFromClick(Unit collector)
        {
            if (collector == null)
            {
                collector = FindNearestLivingPlayerInPickupRange();
                if (collector == null)
                {
                    Debug.LogWarning("KeyItem click pickup ignored because no selected or nearby living player unit is within pickup range " + pickupRange + ".", this);
                    return false;
                }
            }

            if (!IsWithinPickupRange(collector))
            {
                Debug.LogWarning("KeyItem click pickup ignored because selected unit " + collector.UnitId + " is outside pickup range " + pickupRange + ".", collector);
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

            if (collector != null && (collector.Faction != UnitFaction.Player || !collector.IsAlive))
            {
                Debug.LogWarning("KeyItem ignored collection because the collector is not a living player unit.", collector);
                return false;
            }

            if (progressionService == null)
            {
                Debug.LogWarning("KeyItem cannot be collected because LevelProgressionService is missing.", this);
                return false;
            }

            collected = true;
            progressionService.CollectSharedKey(this);

            if (consumeOnCollect)
            {
                gameObject.SetActive(false);
            }

            return true;
        }

        private bool IsWithinPickupRange(Unit collector)
        {
            if (collector == null)
            {
                return false;
            }

            float clampedRange = Mathf.Max(0f, pickupRange);
            float sqrDistance = (collector.transform.position - transform.position).sqrMagnitude;
            return sqrDistance <= clampedRange * clampedRange;
        }

        private Unit FindNearestLivingPlayerInPickupRange()
        {
            Unit[] units = Object.FindObjectsByType<Unit>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            Unit nearestUnit = null;
            float pickupRangeSqr = Mathf.Max(0f, pickupRange);
            pickupRangeSqr *= pickupRangeSqr;
            float nearestDistanceSqr = pickupRangeSqr;

            for (int i = 0; i < units.Length; i++)
            {
                Unit unit = units[i];
                if (unit == null
                    || !unit.gameObject.activeInHierarchy
                    || unit.Faction != UnitFaction.Player
                    || !unit.IsAlive)
                {
                    continue;
                }

                float sqrDistance = (unit.transform.position - transform.position).sqrMagnitude;
                if (sqrDistance <= nearestDistanceSqr)
                {
                    nearestDistanceSqr = sqrDistance;
                    nearestUnit = unit;
                }
            }

            return nearestUnit;
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
        }

        [ContextMenu("Phase 10/Collect Key For Test")]
        public void CollectForTest()
        {
            TryCollect(null);
        }
    }
}
