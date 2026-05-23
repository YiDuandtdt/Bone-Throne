using BoneThrone.AI;
using BoneThrone.Combat;
using BoneThrone.Grid;
using BoneThrone.Movement;
using BoneThrone.Units;
using UnityEngine;

namespace BoneThrone.Tests
{
    /// <summary>
    /// Temporary Phase 8 ContextMenu helper for manually validating basic enemy AI in Play Mode.
    /// This is not formal UI, click input, enemy spawning, room flow, or networking.
    /// </summary>
    public sealed class EnemyAITester : MonoBehaviour
    {
        [SerializeField] private GridManager gridManager;
        [SerializeField] private Pathfinder pathfinder;
        [SerializeField] private UnitMover unitMover;
        [SerializeField] private AttackRangeService attackRangeService;
        [SerializeField] private CombatSystem combatSystem;
        [SerializeField] private Unit selectedEnemy;
        [SerializeField] private Unit[] enemies;
        [SerializeField] private Unit[] players;

        private readonly EnemyAIController enemyAIController = new EnemyAIController();

        [ContextMenu("Phase 8/Run Selected Enemy Action")]
        public void RunSelectedEnemyAction()
        {
            EnemyAIResult result = enemyAIController.TryRunAction(
                selectedEnemy,
                players,
                gridManager,
                pathfinder,
                unitMover,
                attackRangeService,
                combatSystem);

            LogResult(result);
        }

        [ContextMenu("Phase 8/Run All Enemy Actions")]
        public void RunAllEnemyActions()
        {
            if (enemies == null || enemies.Length == 0)
            {
                Debug.LogWarning("EnemyAITester skipped because no enemies are assigned.", this);
                return;
            }

            for (int i = 0; i < enemies.Length; i++)
            {
                EnemyAIResult result = enemyAIController.TryRunAction(
                    enemies[i],
                    players,
                    gridManager,
                    pathfinder,
                    unitMover,
                    attackRangeService,
                    combatSystem);

                LogResult(result);
            }
        }

        private void LogResult(EnemyAIResult result)
        {
            Object context = result.Enemy != null ? result.Enemy : this;
            string enemyText = result.Enemy != null ? result.Enemy.UnitId.ToString() : "none";
            string targetText = result.Target != null ? result.Target.UnitId.ToString() : "none";

            string message = "EnemyAITester result: Action="
                + result.ActionType
                + " Success=" + result.Success
                + " Enemy=" + enemyText
                + " Target=" + targetText
                + " Message=" + result.Message;

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
