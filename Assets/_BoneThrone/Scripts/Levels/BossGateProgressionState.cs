using UnityEngine;

namespace BoneThrone.Levels
{
    /// <summary>
    /// Small local state holder for future user-placed boss gate pieces.
    /// It does not own boss fights, scene loading, save/load, or level progression.
    /// </summary>
    public sealed class BossGateProgressionState : MonoBehaviour
    {
        [SerializeField] private bool hasBossKey;
        [SerializeField] private bool isBossDoorOpened;
        [SerializeField] private bool hasUsedSupplyPoint;
        [SerializeField] private bool debugLogging;

        public bool HasBossKey
        {
            get { return hasBossKey; }
        }

        public bool IsBossDoorOpened
        {
            get { return isBossDoorOpened; }
        }

        public bool HasUsedSupplyPoint
        {
            get { return hasUsedSupplyPoint; }
        }

        public bool CollectBossKey()
        {
            if (hasBossKey)
            {
                Log("Boss key collection ignored because the party already has it.");
                return false;
            }

            hasBossKey = true;
            Log("Boss key collected.");
            return true;
        }

        public bool OpenBossDoor()
        {
            if (!hasBossKey)
            {
                Log("Boss door open rejected because the boss key is missing.");
                return false;
            }

            if (isBossDoorOpened)
            {
                Log("Boss door open ignored because it is already opened.");
                return false;
            }

            isBossDoorOpened = true;
            Log("Boss door opened.");
            return true;
        }

        public bool MarkSupplyPointUsed()
        {
            if (hasUsedSupplyPoint)
            {
                Log("Supply point use ignored because it was already used.");
                return false;
            }

            hasUsedSupplyPoint = true;
            Log("Supply point marked used.");
            return true;
        }

        public void ResetState()
        {
            hasBossKey = false;
            isBossDoorOpened = false;
            hasUsedSupplyPoint = false;
            Log("Boss gate progression state reset.");
        }

        private void Log(string message)
        {
            if (debugLogging)
            {
                Debug.Log("BossGateProgressionState: " + message, this);
            }
        }
    }
}
