using UnityEngine;

namespace BoneThrone.Combat
{
    /// <summary>
    /// Runtime-only one-hit defend state for the local tactics loop.
    /// </summary>
    public sealed class UnitDefenseState : MonoBehaviour
    {
        [SerializeField] private bool isDefending;
        [SerializeField] private int flatReduction = 2;

        public bool IsDefending
        {
            get { return isDefending; }
        }

        public int FlatReduction
        {
            get { return Mathf.Max(0, flatReduction); }
        }

        public void SetDefending(int reduction)
        {
            flatReduction = Mathf.Max(0, reduction);
            isDefending = true;
        }

        public void ClearDefending()
        {
            isDefending = false;
        }

        public bool TryConsumeReduction(int incomingDamage, out int finalDamage, out int reducedAmount)
        {
            int clampedIncoming = Mathf.Max(0, incomingDamage);
            finalDamage = clampedIncoming;
            reducedAmount = 0;

            if (!isDefending)
            {
                return false;
            }

            if (clampedIncoming <= 0)
            {
                ClearDefending();
                return true;
            }

            finalDamage = Mathf.Max(1, clampedIncoming - FlatReduction);
            reducedAmount = Mathf.Max(0, clampedIncoming - finalDamage);
            ClearDefending();
            return true;
        }
    }
}
