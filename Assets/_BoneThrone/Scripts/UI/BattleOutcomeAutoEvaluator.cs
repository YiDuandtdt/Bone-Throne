using System.Collections;
using BoneThrone.Core;
using BoneThrone.Levels;
using BoneThrone.Units;
using UnityEngine;
using UnityEngine.SceneManagement;

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
        [SerializeField] [Min(0f)] private float defeatDelaySeconds = 1.2f;
        [SerializeField] [Min(0f)] private float victoryDelaySeconds = 1.2f;
        [SerializeField] private bool victoryRequiresBossUnit = true;
        [SerializeField] private bool victoryRequiresBossFightStarted = true;
        [SerializeField] private bool loadEndMenuOnVictory = true;
        [SerializeField] private string victorySceneName = "EndMenu";
        [SerializeField] private string bossNameContains = "Boss";

        [Header("Tracked Units")]
        [SerializeField] private Unit[] trackedPlayerUnits;
        [SerializeField] private Unit[] trackedVictoryUnits;
        [SerializeField] private bool autoFindPlayersByFaction = true;
        [SerializeField] private bool autoFindVictoryUnitsByEnemyFaction;

        [Header("Debug")]
        [SerializeField] private bool debugLogging;

        private Coroutine pendingDefeatRoutine;
        private Coroutine pendingVictoryRoutine;

        private void Awake()
        {
            ResolveOutcomeService();
            ResolveTrackedUnitsIfNeeded();
        }

        private void OnDisable()
        {
            if (pendingDefeatRoutine != null)
            {
                StopCoroutine(pendingDefeatRoutine);
                pendingDefeatRoutine = null;
            }

            if (pendingVictoryRoutine != null)
            {
                StopCoroutine(pendingVictoryRoutine);
                pendingVictoryRoutine = null;
            }
        }

        public void ConfigureBossTest(
            GameOutcomeService service,
            Unit[] players,
            Unit[] victoryUnits,
            float outcomeDelaySeconds)
        {
            outcomeService = service != null ? service : outcomeService;
            trackedPlayerUnits = players;
            trackedVictoryUnits = victoryUnits;
            autoFindPlayersByFaction = true;
            autoFindVictoryUnitsByEnemyFaction = true;
            victoryRequiresBossUnit = true;
            victoryRequiresBossFightStarted = true;
            loadEndMenuOnVictory = true;
            victorySceneName = "EndMenu";
            bossNameContains = "Boss";
            defeatReason = "Demo defeat.";
            victoryReason = "Boss defeated.";
            defeatDelaySeconds = Mathf.Max(0f, outcomeDelaySeconds);
            victoryDelaySeconds = Mathf.Max(0f, outcomeDelaySeconds);
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
                StartDefeatAfterDelay();
                return;
            }

            if (triggerVictoryWhenAllTrackedEnemiesDie && HasTrackedUnits(trackedVictoryUnits) && AreVictoryConditionMet())
            {
                StartVictoryAfterDelay();
            }
        }

        private void StartDefeatAfterDelay()
        {
            if (pendingDefeatRoutine != null)
            {
                return;
            }

            pendingDefeatRoutine = StartCoroutine(TriggerDefeatAfterDelay());
        }

        private void StartVictoryAfterDelay()
        {
            if (pendingVictoryRoutine != null)
            {
                return;
            }

            pendingVictoryRoutine = StartCoroutine(TriggerVictoryAfterDelay());
        }

        private IEnumerator TriggerDefeatAfterDelay()
        {
            float delay = Mathf.Max(0f, defeatDelaySeconds);
            if (delay > 0f)
            {
                yield return new WaitForSeconds(delay);
            }

            pendingDefeatRoutine = null;
            if (outcomeService != null && !outcomeService.HasOutcome)
            {
                Log("Auto defeat triggered.");
                outcomeService.SetDefeat(defeatReason);
            }
        }

        private IEnumerator TriggerVictoryAfterDelay()
        {
            float delay = Mathf.Max(0f, victoryDelaySeconds);
            if (delay > 0f)
            {
                yield return new WaitForSeconds(delay);
            }

            pendingVictoryRoutine = null;
            if (outcomeService != null && !outcomeService.HasOutcome && AreVictoryConditionMet())
            {
                Log("Auto victory triggered.");
                if (outcomeService.SetVictory(victoryReason))
                {
                    LoadVictorySceneIfNeeded();
                }
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
            return FindUnitsByFaction(faction, FindObjectsInactive.Exclude);
        }

        private static Unit[] FindUnitsByFaction(UnitFaction faction, FindObjectsInactive inactiveMode)
        {
            Unit[] units = Object.FindObjectsByType<Unit>(inactiveMode, FindObjectsSortMode.None);
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

        private bool AreVictoryConditionMet()
        {
            if (!victoryRequiresBossUnit)
            {
                return AreAllTrackedUnitsDefeated(trackedVictoryUnits);
            }

            Unit boss = FindBossUnit(trackedVictoryUnits);
            if (boss == null)
            {
                boss = FindBossUnit(FindUnitsByFaction(UnitFaction.Enemy, FindObjectsInactive.Include));
            }

            if (boss == null)
            {
                return false;
            }

            if (victoryRequiresBossFightStarted && !IsBossFightStarted())
            {
                return false;
            }

            return !boss.IsAlive;
        }

        private bool IsBossFightStarted()
        {
            BossGateProgressionState progressionState = Object.FindFirstObjectByType<BossGateProgressionState>();
            return progressionState == null || progressionState.ShouldExposeBossFightRuntime();
        }

        private void LoadVictorySceneIfNeeded()
        {
            if (!loadEndMenuOnVictory)
            {
                return;
            }

            if (string.IsNullOrEmpty(victorySceneName))
            {
                Debug.LogWarning("BattleOutcomeAutoEvaluator cannot load the victory scene because Victory Scene Name is empty.", this);
                return;
            }

            if (!Application.CanStreamedLevelBeLoaded(victorySceneName))
            {
                Debug.LogWarning("BattleOutcomeAutoEvaluator cannot load victory scene '" + victorySceneName + "' because it is not in Build Settings.", this);
                return;
            }

            SceneManager.LoadScene(victorySceneName, LoadSceneMode.Single);
        }

        private Unit FindBossUnit(Unit[] units)
        {
            if (units == null || units.Length == 0)
            {
                return null;
            }

            string needle = string.IsNullOrEmpty(bossNameContains) ? "boss" : bossNameContains.ToLowerInvariant();
            for (int i = 0; i < units.Length; i++)
            {
                Unit unit = units[i];
                if (unit == null)
                {
                    continue;
                }

                string objectName = unit.name != null ? unit.name.ToLowerInvariant() : string.Empty;
                string displayName = unit.DisplayName != null ? unit.DisplayName.ToLowerInvariant() : string.Empty;
                if (objectName.Contains(needle) || displayName.Contains(needle))
                {
                    return unit;
                }
            }

            return null;
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
