using BoneThrone.Core;
using BoneThrone.Units;
using UnityEngine;

namespace BoneThrone.UI
{
    /// <summary>
    /// Lightweight local outcome evaluator for isolated validation scenes.
    /// It polls a small tracked unit set until victory or defeat has been decided.
    /// </summary>
    public sealed class BattleOutcomeAutoEvaluator : MonoBehaviour
    {
        [Header("Outcome")]
        [SerializeField] private GameOutcomeService outcomeService;
        [SerializeField] private bool triggerDefeatWhenAllTrackedPlayersDie = true;
        [SerializeField] private bool triggerVictoryWhenAllTrackedEnemiesDie = true;
        [SerializeField] private string defeatReason = "The party has fallen.";
        [SerializeField] private string victoryReason = "Victory.";

        [Header("Tracked Units")]
        [SerializeField] private Unit[] trackedPlayerUnits;
        [SerializeField] private Unit[] trackedVictoryUnits;
        [SerializeField] private bool autoFindPlayersByFaction = true;
        [SerializeField] private bool autoFindVictoryUnitsByEnemyFaction;

        [Header("Debug")]
        [SerializeField] private bool debugLogging;

        private void Awake()
        {
            ResolveOutcomeService();
            ResolveTrackedUnitsIfNeeded();
        }

        private void Update()
        {
            ResolveOutcomeService();
            if (outcomeService == null || outcomeService.HasOutcome)
            {
                return;
            }

            ResolveTrackedUnitsIfNeeded();

            if (triggerDefeatWhenAllTrackedPlayersDie && HasTrackedUnits(trackedPlayerUnits) && AreAllTrackedUnitsDefeated(trackedPlayerUnits))
            {
                Log("Auto defeat triggered.");
                outcomeService.SetDefeat(defeatReason);
                return;
            }

            if (triggerVictoryWhenAllTrackedEnemiesDie && HasTrackedUnits(trackedVictoryUnits) && AreAllTrackedUnitsDefeated(trackedVictoryUnits))
            {
                Log("Auto victory triggered.");
                outcomeService.SetVictory(victoryReason);
            }
        }

        private void ResolveOutcomeService()
        {
            if (outcomeService == null)
            {
                outcomeService = Object.FindFirstObjectByType<GameOutcomeService>();
            }
        }

        private void ResolveTrackedUnitsIfNeeded()
        {
            if (autoFindPlayersByFaction && !HasTrackedUnits(trackedPlayerUnits))
            {
                trackedPlayerUnits = FindUnitsByFaction(UnitFaction.Player);
            }

            if (autoFindVictoryUnitsByEnemyFaction && !HasTrackedUnits(trackedVictoryUnits))
            {
                trackedVictoryUnits = FindUnitsByFaction(UnitFaction.Enemy);
            }
        }

        private static Unit[] FindUnitsByFaction(UnitFaction faction)
        {
            Unit[] units = Object.FindObjectsByType<Unit>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            int count = 0;
            for (int i = 0; i < units.Length; i++)
            {
                if (units[i] != null && units[i].Faction == faction)
                {
                    count++;
                }
            }

            if (count == 0)
            {
                return new Unit[0];
            }

            Unit[] result = new Unit[count];
            int writeIndex = 0;
            for (int i = 0; i < units.Length; i++)
            {
                Unit unit = units[i];
                if (unit != null && unit.Faction == faction)
                {
                    result[writeIndex] = unit;
                    writeIndex++;
                }
            }

            return result;
        }

        private static bool HasTrackedUnits(Unit[] units)
        {
            if (units == null || units.Length == 0)
            {
                return false;
            }

            for (int i = 0; i < units.Length; i++)
            {
                if (units[i] != null)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool AreAllTrackedUnitsDefeated(Unit[] units)
        {
            if (!HasTrackedUnits(units))
            {
                return false;
            }

            for (int i = 0; i < units.Length; i++)
            {
                Unit unit = units[i];
                if (unit != null && unit.IsAlive)
                {
                    return false;
                }
            }

            return true;
        }

        private void Log(string message)
        {
            if (debugLogging)
            {
                Debug.Log("BattleOutcomeAutoEvaluator: " + message, this);
            }
        }
    }
}
