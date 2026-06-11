using UnityEngine;

namespace BoneThrone.Combat
{
    /// <summary>
    /// One-hit incoming damage amplification. Reapplying refreshes the amount.
    /// </summary>
    public sealed class UnitDamageAmplifyState : MonoBehaviour
    {
        [SerializeField] private bool hasAmplify;
        [SerializeField] private int bonusDamage = 1;

        public bool HasAmplify
        {
            get { return hasAmplify; }
        }

        public int BonusDamage
        {
            get { return Mathf.Max(0, bonusDamage); }
        }

        public void ApplyAmplify(int bonus)
        {
            bonusDamage = Mathf.Max(0, bonus);
            hasAmplify = bonusDamage > 0;
        }

        public bool TryConsumeAmplify(out int bonus)
        {
            bonus = 0;
            if (!hasAmplify)
            {
                return false;
            }

            bonus = BonusDamage;
            hasAmplify = false;
            return bonus > 0;
        }
    }
}
