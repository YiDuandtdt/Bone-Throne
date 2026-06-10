using UnityEngine;

namespace BoneThrone.Combat
{
    /// <summary>
    /// One-opportunity stun state. Stunned units skip their next move and action.
    /// </summary>
    public sealed class UnitStunState : MonoBehaviour
    {
        [SerializeField] private bool hasStun;

        public bool HasStun
        {
            get { return hasStun; }
        }

        public bool IsStunned
        {
            get { return hasStun; }
        }

        public void ApplyStun()
        {
            hasStun = true;
        }

        public bool TryConsumeStun()
        {
            if (!hasStun)
            {
                return false;
            }

            hasStun = false;
            return true;
        }
    }
}
