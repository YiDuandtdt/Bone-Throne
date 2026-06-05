using System.Collections.Generic;
using BoneThrone.Combat;
using BoneThrone.Grid;
using BoneThrone.Movement;
using BoneThrone.Turns;
using BoneThrone.UI;
using BoneThrone.Units;
using System.Collections;
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
        [SerializeField] private TurnTransitionPopupView turnTransitionPopupView;
        [SerializeField] private TurnPacingSettings turnPacingSettings;
        [SerializeField] private float enemyActionDelay = 0.4f;

        private readonly EnemyAIController enemyAIController = new EnemyAIController();
        private readonly List<Unit> activeEnemies = new List<Unit>();
        private readonly List<Unit> activePlayers = new List<Unit>();
        private Coroutine runningRoutine;
        private TurnManager runningTurnManager;
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
            runningTurnManager = turnManager;
            runningRoutine = StartCoroutine(RunEnemyTurnRoutine(turnManager));
        }

        private void OnDisable()
        {
            if (runningRoutine != null)
            {
                StopCoroutine(runningRoutine);
                runningRoutine = null;
            }

            TurnManager turnManager = runningTurnManager;
            runningTurnManager = null;
            bool wasRunning = isRunning;
            isRunning = false;

            if (wasRunning && turnManager != null && turnManager.CurrentPhase == TurnPhase.EnemyTurn)
            {
                turnManager.EndEnemyTurn();
            }
        }

        private IEnumerator RunEnemyTurnRoutine(TurnManager turnManager)
        {
            ResolveReferences();
            CollectActivePlayers();
            CollectActiveEnemies();
            SortActiveEnemies();

            if (activePlayers.Count == 0)
            {
                Debug.Log("EnemyTurnRunner completed with no active alive players.", this);
            }
            else if (activeEnemies.Count == 0)
            {
                Debug.Log("EnemyTurnRunner completed with no active alive enemies.", this);
            }
            else
            {
                yield return PlayEnemyTurnIntro();

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
                        yield return WaitForEnemyActionDelay();
                        continue;
                    }

                    ResetEnemyAllowance(enemy);
                    if (actionPermissionService != null && actionPermissionService.TryConsumeStunForAction(enemy, turnManager))
                    {
                        Debug.Log("EnemyTurnRunner skipped stunned enemy " + enemy.UnitId + ".", enemy);
                        yield return WaitForEnemyActionDelay();
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
                    yield return WaitForEnemyActionDelay();
                }

                yield return WaitForDelay(GetAfterEnemyRoundDelay());
                yield return PlayPlayerTurnIntro();
            }

            runningRoutine = null;
            runningTurnManager = null;
            isRunning = false;
            turnManager.EndEnemyTurn();
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

        private void SortActiveEnemies()
        {
            activeEnemies.Sort(CompareEnemiesForTurnOrder);
        }

        private static int CompareEnemiesForTurnOrder(Unit a, Unit b)
        {
            if (ReferenceEquals(a, b))
            {
                return 0;
            }

            if (a == null)
            {
                return 1;
            }

            if (b == null)
            {
                return -1;
            }

            bool aHasTile = a.CurrentTile != null;
            bool bHasTile = b.CurrentTile != null;
            if (aHasTile != bHasTile)
            {
                return aHasTile ? -1 : 1;
            }

            if (aHasTile && bHasTile)
            {
                int yCompare = a.CurrentTile.Position.Y.CompareTo(b.CurrentTile.Position.Y);
                if (yCompare != 0)
                {
                    return yCompare;
                }

                int xCompare = a.CurrentTile.Position.X.CompareTo(b.CurrentTile.Position.X);
                if (xCompare != 0)
                {
                    return xCompare;
                }
            }

            int idCompare = a.UnitId.CompareTo(b.UnitId);
            if (idCompare != 0)
            {
                return idCompare;
            }

            return a.GetInstanceID().CompareTo(b.GetInstanceID());
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

            if (turnTransitionPopupView == null)
            {
                turnTransitionPopupView = Object.FindFirstObjectByType<TurnTransitionPopupView>();
            }

            if (turnPacingSettings == null && turnTransitionPopupView != null)
            {
                turnPacingSettings = turnTransitionPopupView.PacingSettings;
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

        private WaitForSeconds WaitForEnemyActionDelay()
        {
            return new WaitForSeconds(Mathf.Max(0f, GetEnemyActionInterval()));
        }

        private IEnumerator PlayEnemyTurnIntro()
        {
            yield return WaitForDelay(GetBeforeEnemyTurnBannerDelay());

            if (turnTransitionPopupView != null)
            {
                yield return turnTransitionPopupView.PlayEnemyTurnAnnouncement();
            }
            else
            {
                yield return WaitForDelay(GetEnemyTurnBannerHoldDuration());
            }

            yield return WaitForDelay(GetAfterEnemyTurnBannerDelay());
        }

        private IEnumerator PlayPlayerTurnIntro()
        {
            yield return WaitForDelay(GetBeforePlayerTurnBannerDelay());

            if (turnTransitionPopupView != null)
            {
                yield return turnTransitionPopupView.PlayPlayerTurnAnnouncement();
            }
            else
            {
                yield return WaitForDelay(GetPlayerTurnBannerHoldDuration());
            }

            yield return WaitForDelay(GetAfterPlayerTurnBannerDelay());
        }

        private IEnumerator WaitForDelay(float duration)
        {
            if (duration > 0f)
            {
                yield return new WaitForSeconds(duration);
            }
        }

        private float GetBeforeEnemyTurnBannerDelay()
        {
            return turnPacingSettings != null ? turnPacingSettings.BeforeEnemyTurnBannerDelay : 0f;
        }

        private float GetEnemyTurnBannerHoldDuration()
        {
            return turnPacingSettings != null ? turnPacingSettings.EnemyTurnBannerHoldDuration : 0f;
        }

        private float GetAfterEnemyTurnBannerDelay()
        {
            return turnPacingSettings != null ? turnPacingSettings.AfterEnemyTurnBannerDelay : 0f;
        }

        private float GetEnemyActionInterval()
        {
            return turnPacingSettings != null ? turnPacingSettings.EnemyActionInterval : enemyActionDelay;
        }

        private float GetAfterEnemyRoundDelay()
        {
            return turnPacingSettings != null ? turnPacingSettings.AfterEnemyRoundDelay : 0f;
        }

        private float GetBeforePlayerTurnBannerDelay()
        {
            return turnPacingSettings != null ? turnPacingSettings.BeforePlayerTurnBannerDelay : 0f;
        }

        private float GetPlayerTurnBannerHoldDuration()
        {
            return turnPacingSettings != null ? turnPacingSettings.PlayerTurnBannerHoldDuration : 0f;
        }

        private float GetAfterPlayerTurnBannerDelay()
        {
            return turnPacingSettings != null ? turnPacingSettings.AfterPlayerTurnBannerDelay : 0f;
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
