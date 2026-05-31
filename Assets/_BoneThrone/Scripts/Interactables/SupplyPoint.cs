using BoneThrone.Items;
using BoneThrone.Levels;
using BoneThrone.Units;
using UnityEngine;

namespace BoneThrone.Interactables
{
    /// <summary>
    /// Minimal one-shot supply point. First pass grants potion counts only.
    /// </summary>
    public sealed class SupplyPoint : MonoBehaviour
    {
        [SerializeField] private BossGateProgressionState progressionState;
        [SerializeField] private Unit[] targetUnits;
        [SerializeField] private int potionGrantAmount = 1;
        [SerializeField] private bool oneShot = true;
        [SerializeField] private bool used;
        [SerializeField] private bool debugLogging;

        public bool IsUsed
        {
            get { return used; }
        }

        private void OnMouseDown()
        {
            TryUse();
        }

        public bool TryUse()
        {
            if (oneShot && used)
            {
                Log("Use ignored because this supply point is already used.");
                return false;
            }

            int grantAmount = Mathf.Max(0, potionGrantAmount);
            if (grantAmount <= 0)
            {
                Log("Use rejected because potion grant amount is zero.");
                return false;
            }

            Unit[] units = ResolveTargetUnits();
            int affectedCount = GrantPotions(units, grantAmount);
            if (affectedCount <= 0)
            {
                Log("Use rejected because no eligible living player units were found.");
                return false;
            }

            used = true;
            ResolveProgressionState();
            if (progressionState != null)
            {
                progressionState.MarkSupplyPointUsed();
            }

            Log("Supply point granted " + grantAmount + " potion(s) to " + affectedCount + " unit(s).");
            return true;
        }

        private Unit[] ResolveTargetUnits()
        {
            if (targetUnits != null && targetUnits.Length > 0)
            {
                return targetUnits;
            }

            return Object.FindObjectsByType<Unit>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        }

        private int GrantPotions(Unit[] units, int grantAmount)
        {
            if (units == null || units.Length == 0)
            {
                return 0;
            }

            int affectedCount = 0;
            for (int i = 0; i < units.Length; i++)
            {
                Unit unit = units[i];
                if (unit == null || unit.Faction != UnitFaction.Player || !unit.IsAlive)
                {
                    continue;
                }

                UnitPotionState potionState = unit.GetComponent<UnitPotionState>();
                if (potionState == null)
                {
                    potionState = unit.gameObject.AddComponent<UnitPotionState>();
                }

                potionState.AddPotions(grantAmount);
                affectedCount++;
            }

            return affectedCount;
        }

        private void ResolveProgressionState()
        {
            if (progressionState == null)
            {
                progressionState = Object.FindFirstObjectByType<BossGateProgressionState>();
            }
        }

        private void Log(string message)
        {
            if (debugLogging)
            {
                Debug.Log("SupplyPoint: " + message, this);
            }
        }
    }
}
