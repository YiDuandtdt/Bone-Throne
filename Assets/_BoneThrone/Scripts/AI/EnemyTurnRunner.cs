using System.Collections.Generic;
using BoneThrone.Combat;
using BoneThrone.Grid;
using BoneThrone.Movement;
using BoneThrone.Turns;
using BoneThrone.Units;
using UnityEngine;

namespace BoneThrone.AI
{
    /// <summary>
    /// Orchestrates one complete local EnemyTurn. Individual decisions stay in EnemyAIController.
    /// </summary>
    public sealed class EnemyTurnRunner : MonoBehaviour
    {
        [SerializeField] private ActiveUnitProvider activeUnitProvider;
        [SerializeField] private Unit[] enemyUnits;
        [SerializeField] private Unit[] playerUnits;
        [SerializeField] private GridManager gridManager;
        [SerializeField] private Pathfinder pathfinder;
        [SerializeField] private UnitMover unitMover;
        [SerializeField] private AttackRangeService attackRangeService;
        [SerializeField] private CombatSystem combatSystem;
        [SerializeField] private ActionPermissionService actionPermissionService;
        [SerializeField] private DamageResolver damageResolver;
        [SerializeField] private CombatLog combatLog;

        private readonly EnemyAIController enemyAIController = new EnemyAIController();
        private readonly List<Unit> activeEnemies = new List<Unit>();
        private readonly List<Unit> activePlayers = new List<Unit>();
        private bool isRunning;

        private void Awake()
        {
            ResolveReferences();
        }

        public void RunEnemyTurn(TurnManager turnManager)
        {
            if (isRunning)
            {
                Debug.LogWarning("EnemyTurnRunner ignored a re-entry request while EnemyTurn is already running.", this);
                return;
            }

            if (turnManager == null)
            {
                Debug.LogWarning("EnemyTurnRunner cannot run because TurnManager is missing.", this);
                return;
            }

            isRunning = true;
            try
            {
                ResolveReferences();
                CollectActivePlayers();
                CollectActiveEnemies();

                if (activeEnemies.Count == 0)
                {
                    Debug.Log("EnemyTurnRunner completed with no active alive enemies.", this);
                    return;
                }

                for (int i = 0; i < activeEnemies.Count; i++)
                {
                    Unit enemy = activeEnemies[i];
                    if (!IsActiveAliveEnemy(enemy))
                    {
                        continue;
                    }

                    TickBleed(enemy);
                    if (!IsActiveAliveEnemy(enemy))
                    {
                        continue;
                    }

                    ResetEnemyAllowance(enemy);
                    if (actionPermissionService != null && actionPermissionService.TryConsumeStunForAction(enemy, turnManager))
                    {
                        Debug.Log("EnemyTurnRunner skipped stunned enemy " + enemy.UnitId + ".", enemy);
                        continue;
                    }

                    EnemyAIResult result = enemyAIController.TryRunAction(
                        enemy,
                        activePlayers,
                        gridManager,
                        pathfinder,
                        unitMover,
                        attackRangeService,
                        combatSystem,
                        actionPermissionService,
                        turnManager);

                    LogResult(result);
                }
            }
            finally
            {
                isRunning = false;
                turnManager.EndEnemyTurn();
            }
        }

        private void CollectActiveEnemies()
        {
            activeEnemies.Clear();
            if (activeUnitProvider != null)
            {
                activeUnitProvider.FillActiveAliveEnemies(activeEnemies);
            }

            if (activeEnemies.Count == 0)
            {
                FillFromFallback(enemyUnits, UnitFaction.Enemy, activeEnemies);
            }
        }

        private void CollectActivePlayers()
        {
            activePlayers.Clear();
            if (activeUnitProvider != null)
            {
                activeUnitProvider.FillActiveAliveUnits(activePlayers);
                for (int i = activePlayers.Count - 1; i >= 0; i--)
                {
                    if (activePlayers[i].Faction != UnitFaction.Player)
                    {
                        activePlayers.RemoveAt(i);
                    }
                }
            }

            if (activePlayers.Count == 0)
            {
                FillFromFallback(playerUnits, UnitFaction.Player, activePlayers);
            }
        }

        private static void FillFromFallback(Unit[] units, UnitFaction faction, List<Unit> results)
        {
            if (units == null || results == null)
            {
                return;
            }

            for (int i = 0; i < units.Length; i++)
            {
                Unit unit = units[i];
                if (unit != null
                    && unit.gameObject.activeInHierarchy
                    && unit.IsAlive
                    && unit.Faction == faction)
                {
                    results.Add(unit);
                }
            }
        }

        private static bool IsActiveAliveEnemy(Unit enemy)
        {
            return enemy != null
                && enemy.gameObject.activeInHierarchy
                && enemy.IsAlive
                && enemy.Faction == UnitFaction.Enemy;
        }

        private static void ResetEnemyAllowance(Unit enemy)
        {
            UnitTurnState turnState = enemy != null ? enemy.GetComponent<UnitTurnState>() : null;
            if (turnState != null)
            {
                turnState.ResetForNewRound();
            }
        }

        private void ResolveReferences()
        {
            if (activeUnitProvider == null)
            {
                activeUnitProvider = Object.FindFirstObjectByType<ActiveUnitProvider>();
            }

            if (gridManager == null)
            {
                gridManager = Object.FindFirstObjectByType<GridManager>();
            }

            if (pathfinder == null)
            {
                pathfinder = Object.FindFirstObjectByType<Pathfinder>();
            }

            if (unitMover == null)
            {
                unitMover = Object.FindFirstObjectByType<UnitMover>();
            }

            if (attackRangeService == null)
            {
                attackRangeService = Object.FindFirstObjectByType<AttackRangeService>();
            }

            if (combatSystem == null)
            {
                combatSystem = Object.FindFirstObjectByType<CombatSystem>();
            }

            if (actionPermissionService == null)
            {
                actionPermissionService = Object.FindFirstObjectByType<ActionPermissionService>();
            }

            if (damageResolver == null)
            {
                damageResolver = Object.FindFirstObjectByType<DamageResolver>();
            }

            if (combatLog == null)
            {
                combatLog = Object.FindFirstObjectByType<CombatLog>();
            }
        }

        private void TickBleed(Unit enemy)
        {
            if (enemy == null || damageResolver == null)
            {
                return;
            }

            UnitBleedState bleedState = enemy.GetComponent<UnitBleedState>();
            if (bleedState == null || !bleedState.HasBleed)
            {
                return;
            }

            int bleedDamage;
            if (!bleedState.TryConsumeTick(out bleedDamage))
            {
                return;
            }

            bool died = damageResolver.ApplyDamage(enemy, bleedDamage);
            int remainingHp = enemy.RuntimeState != null ? enemy.RuntimeState.CurrentHp : 0;
            if (combatLog != null)
            {
                combatLog.LogBleedTick(enemy, bleedDamage, remainingHp, bleedState.RemainingTurns);
                if (died)
                {
                    combatLog.LogDeath(enemy);
                }
            }
        }

        private void LogResult(EnemyAIResult result)
        {
            Object context = result.Enemy != null ? result.Enemy : this;
            string enemyText = result.Enemy != null ? result.Enemy.UnitId.ToString() : "none";
            string targetText = result.Target != null ? result.Target.UnitId.ToString() : "none";
            string message = "EnemyTurnRunner result: Action="
                + result.ActionType
                + " Success="
                + result.Success
                + " Enemy="
                + enemyText
                + " Target="
                + targetText
                + " Message="
                + result.Message;

            if (result.Success)
            {
                Debug.Log(message, context);
            }
            else
            {
                Debug.LogWarning(message, context);
            }
        }
    }
}
