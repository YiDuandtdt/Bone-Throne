using BoneThrone.Items;
using BoneThrone.Movement;
using BoneThrone.Units;
using UnityEngine;

namespace BoneThrone.Interactables
{
    /// <summary>
    /// Minimal Phase 15.1 health potion pickup. It only adds potion count to the selected player unit.
    /// </summary>
    public sealed class HealthPotionPickup : MonoBehaviour
    {
        [SerializeField] private SelectionManager selectionManager;
        [SerializeField] private int potionAmount = 1;
        [SerializeField] private float pickupRange = 1.5f;
        [SerializeField] private bool consumeOnCollect = true;
        [SerializeField] private bool collected;

        public bool Collected
        {
            get { return collected; }
        }

        private void OnMouseDown()
        {
            ResolveReferences();
            Unit collector = selectionManager != null ? selectionManager.SelectedUnit : null;
            TryCollect(collector);
        }

        public bool TryCollect(Unit collector)
        {
            if (collected)
            {
                Debug.Log("HealthPotionPickup ignored because this potion is already collected.", this);
                return false;
            }

            if (collector == null)
            {
                Debug.LogWarning("HealthPotionPickup requires a selected player unit.", this);
                return false;
            }

            if (collector.Faction != UnitFaction.Player || !collector.IsAlive)
            {
                Debug.LogWarning("HealthPotionPickup rejected because the collector is not a living player unit.", collector);
                return false;
            }

            if (!IsCollectorInRange(collector))
            {
                Debug.LogWarning("HealthPotionPickup rejected because the selected unit is out of pickup range.", this);
                return false;
            }

            UnitPotionState potionState = collector.GetComponent<UnitPotionState>();
            if (potionState == null)
            {
                Debug.LogWarning("HealthPotionPickup added a runtime UnitPotionState to the selected unit. Player prefabs were not modified.", collector);
                potionState = collector.gameObject.AddComponent<UnitPotionState>();
            }

            potionState.AddPotions(potionAmount);
            collected = true;

            Debug.Log("HealthPotionPickup: unit " + collector.UnitId + " collected " + Mathf.Max(0, potionAmount) + " potion(s).", this);

            if (consumeOnCollect)
            {
                gameObject.SetActive(false);
            }

            return true;
        }

        private void ResolveReferences()
        {
            if (selectionManager == null)
            {
                selectionManager = Object.FindFirstObjectByType<SelectionManager>();
            }
        }

        private bool IsCollectorInRange(Unit collector)
        {
            if (collector == null)
            {
                return false;
            }

            float safeRange = Mathf.Max(0f, pickupRange);
            return Vector3.Distance(transform.position, collector.transform.position) <= safeRange;
        }
    }
}
