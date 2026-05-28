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
    /// Executes one Phase 8 enemy action: select target, basic attack, move closer, or skip.
    /// This class does not run a formal enemy round, UI flow, room system, or networking flow.
    /// </summary>
    public sealed class EnemyAIController
    {
        private readonly EnemyTargetSelector targetSelector = new EnemyTargetSelector();
        private readonly EnemyMovementPlanner movementPlanner = new EnemyMovementPlanner();

        public EnemyAIResult TryRunAction(
            Unit enemy,
            IReadOnlyList<Unit> playerCandidates,
            GridManager gridManager,
            Pathfinder pathfinder,
            UnitMover unitMover,
            AttackRangeService attackRangeService,
            CombatSystem combatSystem,
            ActionPermissionService actionPermissionService = null,
            TurnManager turnManager = null)
        {
            EnemyAIResult guardResult;
            if (!TryValidateEnemy(enemy, out guardResult))
            {
                return guardResult;
            }

            Unit target;
            if (!targetSelector.TrySelectNearestAlivePlayer(enemy, playerCandidates, out target))
            {
                return EnemyAIResult.Skipped(enemy, null, "Enemy AI skipped because no alive player target was found.");
            }

            if (attackRangeService == null)
            {
                return EnemyAIResult.Skipped(enemy, target, "Enemy AI skipped because AttackRangeService is missing.");
            }

            if (attackRangeService.IsInBasicAttackRange(enemy, target))
            {
                if (combatSystem == null)
                {
                    return EnemyAIResult.Skipped(enemy, target, "Enemy AI skipped attack because CombatSystem is missing.");
                }

                if (!CanAct(enemy, actionPermissionService, turnManager))
                {
                    return EnemyAIResult.Skipped(enemy, target, "Enemy AI skipped attack because action permission rejected the enemy.");
                }

                bool attacked = combatSystem.TryBasicAttack(enemy, target);
                return attacked
                    ? EnemyAIResult.Attacked(enemy, target)
                    : EnemyAIResult.Skipped(enemy, target, "Enemy AI skipped because CombatSystem rejected the basic attack.");
            }

            if (gridManager == null || pathfinder == null || unitMover == null)
            {
                return EnemyAIResult.Skipped(enemy, target, "Enemy AI skipped movement because a movement service reference is missing.");
            }

            if (!CanMove(enemy, actionPermissionService, turnManager))
            {
                return EnemyAIResult.Skipped(enemy, target, "Enemy AI skipped movement because action permission rejected the enemy.");
            }

            List<GridPosition> path;
            if (!movementPlanner.TryBuildMoveTowardTarget(enemy, target, gridManager, pathfinder, out path))
            {
                return EnemyAIResult.Skipped(enemy, target, "Enemy AI skipped because no valid movement path toward the target exists.");
            }

            GridPosition destination = path[path.Count - 1];
            if (!unitMover.TryMove(enemy, gridManager, path))
            {
                return EnemyAIResult.Skipped(enemy, target, "Enemy AI skipped because UnitMover rejected the planned path.");
            }

            MarkMoved(enemy);
            return EnemyAIResult.Moved(enemy, target, destination);
        }

        private static bool TryValidateEnemy(Unit enemy, out EnemyAIResult result)
        {
            if (enemy == null)
            {
                result = EnemyAIResult.Skipped(null, null, "Enemy AI skipped because enemy Unit is missing.");
                return false;
            }

            if (enemy.Faction != UnitFaction.Enemy)
            {
                result = EnemyAIResult.Skipped(enemy, null, "Enemy AI skipped because the Unit is not in the Enemy faction.");
                return false;
            }

            if (!enemy.IsAlive)
            {
                result = EnemyAIResult.Skipped(enemy, null, "Enemy AI skipped because enemy Unit is dead.");
                return false;
            }

            if (enemy.CurrentTile == null)
            {
                result = EnemyAIResult.Skipped(enemy, null, "Enemy AI skipped because enemy Unit has no current tile.");
                return false;
            }

            result = default(EnemyAIResult);
            return true;
        }

        private static bool CanMove(Unit enemy, ActionPermissionService actionPermissionService, TurnManager turnManager)
        {
            if (actionPermissionService == null && turnManager == null)
            {
                return !HasMoved(enemy);
            }

            if (actionPermissionService == null || turnManager == null)
            {
                Debug.LogWarning("Enemy AI movement gate is partially configured. Bind both ActionPermissionService and TurnManager, or leave both empty.", enemy);
                return false;
            }

            return actionPermissionService.CanMove(enemy, turnManager);
        }

        private static bool CanAct(Unit enemy, ActionPermissionService actionPermissionService, TurnManager turnManager)
        {
            if (actionPermissionService == null && turnManager == null)
            {
                return !HasActed(enemy);
            }

            if (actionPermissionService == null || turnManager == null)
            {
                Debug.LogWarning("Enemy AI action gate is partially configured. Bind both ActionPermissionService and TurnManager, or leave both empty.", enemy);
                return false;
            }

            return actionPermissionService.CanAct(enemy, turnManager);
        }

        private static bool HasMoved(Unit enemy)
        {
            UnitTurnState turnState = enemy != null ? enemy.GetComponent<UnitTurnState>() : null;
            return turnState != null && turnState.HasMoved;
        }

        private static bool HasActed(Unit enemy)
        {
            UnitTurnState turnState = enemy != null ? enemy.GetComponent<UnitTurnState>() : null;
            return turnState != null && turnState.HasActed;
        }

        private static void MarkMoved(Unit enemy)
        {
            UnitTurnState turnState = enemy != null ? enemy.GetComponent<UnitTurnState>() : null;
            if (turnState != null)
            {
                turnState.MarkMoved();
            }
        }
    }
}
