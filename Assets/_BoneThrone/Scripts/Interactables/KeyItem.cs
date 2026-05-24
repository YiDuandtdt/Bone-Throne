using BoneThrone.Levels;
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
            TryCollect(null);
        }

        public bool TryCollect(Unit collector)
        {
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

        [ContextMenu("Phase 10/Collect Key For Test")]
        public void CollectForTest()
        {
            TryCollect(null);
        }
    }
}
