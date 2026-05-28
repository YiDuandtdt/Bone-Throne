using UnityEngine;

namespace BoneThrone.Items
{
    /// <summary>
    /// Minimal per-unit potion counter for the current local demo.
    /// </summary>
    public sealed class UnitPotionState : MonoBehaviour
    {
        [SerializeField] private int initialPotionCount = 1;
        [SerializeField] private int currentPotionCount = -1;

        public int CurrentPotionCount
        {
            get
            {
                EnsureInitialized();
                return Mathf.Max(0, currentPotionCount);
            }
        }

        public bool HasPotion
        {
            get { return CurrentPotionCount > 0; }
        }

        public void EnsureInitialized()
        {
            if (currentPotionCount < 0)
            {
                currentPotionCount = Mathf.Max(0, initialPotionCount);
            }
        }

        public bool TryConsumePotion()
        {
            EnsureInitialized();
            if (currentPotionCount <= 0)
            {
                return false;
            }

            currentPotionCount--;
            return true;
        }

        public void AddPotions(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            EnsureInitialized();
            currentPotionCount += amount;
        }

        [ContextMenu("Phase 14/Reset Potion Count")]
        public void ResetForTest()
        {
            currentPotionCount = Mathf.Max(0, initialPotionCount);
        }
    }
}
