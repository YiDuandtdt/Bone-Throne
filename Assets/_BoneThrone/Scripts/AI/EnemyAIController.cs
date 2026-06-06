using System.Collections;
using System.Collections.Generic;
using BoneThrone.Audio;
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

                if (actionPermissionService != null
                    && turnManager != null
                    && actionPermissionService.TryConsumeStunForAction(enemy, turnManager))
                {
                    return EnemyAIResult.Skipped(enemy, target, "Enemy AI skipped attack because stun consumed its action.");
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

    public enum BossAttackPattern
    {
        Cleave = 0,
        Slam = 1
    }

    public sealed class BossAttackIntent
    {
        public BossAttackPattern Pattern;
        public GridPosition Origin;
        public GridPosition Forward;
        public readonly List<Tile> AffectedTiles = new List<Tile>();
    }

    /// <summary>
    /// Small boss-specific AI layer: telegraphed melee AOE plus slow movement.
    /// It intentionally stays separate from ordinary minion AI.
    /// </summary>
    public sealed class BossEnemyAIController
    {
        private const int CleaveReach = 2;
        private const int CleaveHalfWidth = 1;
        private const int SlamReach = 3;
        private const int SlamHalfWidth = 2;
        private const float BossImpactDelay = 1.2f;
        private const float BossAttackRecoveryDelay = 0.75f;
        private const float BossPostMoveSettleDelay = 0.45f;
        private const float BossFacingSettleDelay = 0.28f;
        private const float BossAttackSoundDelay = 0.18f;
        private const float BossAttackAnimationSpeed = 0.46f;
        private const float BossAttackAnimationRestoreDelay = 2.45f;
        private const float BossAttackVolumeScale = 1.55f;
        private const float BossAttackPitch = 0.58f;

        private readonly EnemyTargetSelector targetSelector = new EnemyTargetSelector();
        private readonly EnemyMovementPlanner movementPlanner = new EnemyMovementPlanner();
        private static readonly Dictionary<int, int> AttackStepByBossInstance = new Dictionary<int, int>();
        private static readonly Dictionary<int, BossAttackIntent> CachedPreviewIntents = new Dictionary<int, BossAttackIntent>();

        public static bool IsBossLikeUnit(Unit unit)
        {
            if (unit == null || unit.Faction != UnitFaction.Enemy)
            {
                return false;
            }

            string objectName = unit.gameObject.name.ToLowerInvariant();
            string displayName = string.IsNullOrEmpty(unit.DisplayName) ? string.Empty : unit.DisplayName.ToLowerInvariant();
            return objectName.Contains("boss")
                || displayName.Contains("boss")
                || objectName.Contains("golem")
                || displayName.Contains("golem");
        }

        public int GetCurrentAttackStep(Unit boss)
        {
            if (boss == null)
            {
                return 0;
            }

            int step;
            return AttackStepByBossInstance.TryGetValue(boss.GetInstanceID(), out step) ? step : 0;
        }

        public IEnumerator RunActionRoutine(
            Unit boss,
            IReadOnlyList<Unit> playerCandidates,
            GridManager gridManager,
            Pathfinder pathfinder,
            UnitMover unitMover,
            DamageResolver damageResolver,
            CombatLog combatLog,
            ActionPermissionService actionPermissionService,
            TurnManager turnManager)
        {
            if (!IsActiveBoss(boss))
            {
                yield break;
            }

            Unit target;
            if (!targetSelector.TrySelectNearestAlivePlayer(boss, playerCandidates, out target))
            {
                yield break;
            }

            if (actionPermissionService != null
                && turnManager != null
                && actionPermissionService.TryConsumeStunForAction(boss, turnManager))
            {
                yield break;
            }

            int attackStep = GetCurrentAttackStep(boss);
            ClearCachedIntent(boss);

            BossAttackIntent currentIntent;
            BossIntentScore currentScore;
            TryBuildBestIntent(boss, playerCandidates, gridManager, attackStep, out currentIntent, out currentScore);

            List<GridPosition> movementPath;
            BossIntentScore projectedMoveScore;
            if (TryBuildBossMovePath(boss, playerCandidates, gridManager, pathfinder, out movementPath, out projectedMoveScore)
                && ShouldMoveBeforeAttack(currentScore, projectedMoveScore)
                && CanMove(boss, actionPermissionService, turnManager))
            {
                bool movementCompleted = false;
                System.Action<Unit> handleMoveCompleted = movedUnit =>
                {
                    if (movedUnit == boss)
                    {
                        movementCompleted = true;
                    }
                };

                if (unitMover != null)
                {
                    unitMover.MoveVisualCompleted += handleMoveCompleted;
                }

                bool moved = unitMover != null && unitMover.TryMove(boss, gridManager, movementPath);
                if (moved)
                {
                    MarkBossMoved(boss);
                    yield return WaitForBossMoveVisual(boss, unitMover, () => movementCompleted);
                    yield return WaitForBossDelay(BossPostMoveSettleDelay);
                }

                if (unitMover != null)
                {
                    unitMover.MoveVisualCompleted -= handleMoveCompleted;
                }
            }

            BossAttackIntent intent;
            BossIntentScore finalScore;
            if (!TryBuildBestIntent(boss, playerCandidates, gridManager, attackStep, out intent, out finalScore))
            {
                yield break;
            }

            List<Unit> targetsInIntent = BuildTargetsInIntent(playerCandidates, intent);
            if (targetsInIntent.Count == 0)
            {
                ClearCachedIntent(boss);
                yield break;
            }

            yield return PlayBossAttackPresentationRoutine(boss, intent, target);
            yield return WaitForBossDelay(BossImpactDelay);
            if (targetsInIntent.Count > 0)
            {
                ApplyBossDamage(boss, intent, targetsInIntent, damageResolver, combatLog);
            }

            MarkBossActed(boss);
            AdvanceAttackStep(boss);
            ClearCachedIntent(boss);
            yield return WaitForBossDelay(BossAttackRecoveryDelay);
        }

        public bool TryBuildPreviewIntent(Unit boss, IReadOnlyList<Unit> players, GridManager gridManager, out BossAttackIntent intent)
        {
            intent = null;
            if (!IsActiveBoss(boss))
            {
                return false;
            }

            BossIntentScore score;
            return TryBuildBestIntent(boss, players, gridManager, GetCurrentAttackStep(boss), out intent, out score);
        }

        public static void CachePreviewIntent(Unit boss, BossAttackIntent intent)
        {
            if (boss == null || intent == null)
            {
                return;
            }

            CachedPreviewIntents[boss.GetInstanceID()] = intent;
        }

        public static bool TryGetPreviewIntent(Unit boss, out BossAttackIntent intent)
        {
            return TryGetCachedIntent(boss, out intent);
        }

        private static bool TryGetCachedIntent(Unit boss, out BossAttackIntent intent)
        {
            intent = null;
            return boss != null && CachedPreviewIntents.TryGetValue(boss.GetInstanceID(), out intent);
        }

        private static void ClearCachedIntent(Unit boss)
        {
            if (boss != null)
            {
                CachedPreviewIntents.Remove(boss.GetInstanceID());
            }
        }

        private static bool TryBuildBestIntent(
            Unit boss,
            IReadOnlyList<Unit> players,
            GridManager gridManager,
            int attackStep,
            out BossAttackIntent intent,
            out BossIntentScore score)
        {
            intent = null;
            score = BossIntentScore.Empty;
            if (!IsActiveBoss(boss) || boss.CurrentTile == null || gridManager == null || players == null)
            {
                return false;
            }

            return TryBuildBestIntentFromOrigin(boss.CurrentTile.Position, players, gridManager, attackStep, out intent, out score);
        }

        private static bool TryBuildBestIntentFromOrigin(
            GridPosition origin,
            IReadOnlyList<Unit> players,
            GridManager gridManager,
            int attackStep,
            out BossAttackIntent intent,
            out BossIntentScore score)
        {
            intent = null;
            score = BossIntentScore.Empty;
            if (players == null || gridManager == null)
            {
                return false;
            }

            BossAttackPattern pattern = attackStep % 2 == 0 ? BossAttackPattern.Cleave : BossAttackPattern.Slam;
            GridPosition[] directions =
            {
                new GridPosition(1, 0),
                new GridPosition(-1, 0),
                new GridPosition(0, 1),
                new GridPosition(0, -1)
            };

            BossAttackIntent bestIntent = null;
            BossIntentScore bestScore = BossIntentScore.Empty;
            for (int i = 0; i < directions.Length; i++)
            {
                BossAttackIntent candidate = BuildIntentForDirection(origin, directions[i], pattern, gridManager);
                if (candidate == null)
                {
                    continue;
                }

                BossIntentScore candidateScore = ScoreIntent(candidate, players);
                if (bestIntent == null || candidateScore.IsBetterThan(bestScore))
                {
                    bestIntent = candidate;
                    bestScore = candidateScore;
                }
            }

            if (bestIntent == null)
            {
                return false;
            }

            if (bestScore.TargetCount == 0)
            {
                Unit focusTarget = FindBestFocusTarget(origin, players);
                if (focusTarget != null && focusTarget.CurrentTile != null)
                {
                    BossAttackIntent focusedIntent = BuildIntentForDirection(
                        origin,
                        GetCardinalForward(origin, focusTarget.CurrentTile.Position),
                        pattern,
                        gridManager);
                    if (focusedIntent != null)
                    {
                        bestIntent = focusedIntent;
                        bestScore = ScoreIntent(bestIntent, players);
                    }
                }
            }

            intent = bestIntent;
            score = bestScore;
            return intent.AffectedTiles.Count > 0;
        }

        private static BossAttackIntent BuildIntentForDirection(
            GridPosition origin,
            GridPosition forward,
            BossAttackPattern pattern,
            GridManager gridManager)
        {
            BossAttackIntent intent = new BossAttackIntent
            {
                Pattern = pattern,
                Origin = origin,
                Forward = forward
            };

            int reach = pattern == BossAttackPattern.Slam ? SlamReach : CleaveReach;
            int halfWidth = pattern == BossAttackPattern.Slam ? SlamHalfWidth : CleaveHalfWidth;
            AddForwardArcTiles(gridManager, intent, reach, halfWidth);
            return intent.AffectedTiles.Count > 0 ? intent : null;
        }

        private static Unit FindBestFocusTarget(GridPosition origin, IReadOnlyList<Unit> players)
        {
            Unit best = null;
            int bestHp = int.MaxValue;
            int bestDistance = int.MaxValue;
            int bestUnitId = int.MaxValue;
            for (int i = 0; i < players.Count; i++)
            {
                Unit candidate = players[i];
                if (candidate == null
                    || candidate.Faction != UnitFaction.Player
                    || !candidate.IsAlive
                    || candidate.CurrentTile == null)
                {
                    continue;
                }

                int hp = candidate.RuntimeState != null ? candidate.RuntimeState.CurrentHp : int.MaxValue;
                int distance = GetManhattanDistance(origin, candidate.CurrentTile.Position);
                if (hp < bestHp
                    || (hp == bestHp && distance < bestDistance)
                    || (hp == bestHp && distance == bestDistance && candidate.UnitId < bestUnitId))
                {
                    best = candidate;
                    bestHp = hp;
                    bestDistance = distance;
                    bestUnitId = candidate.UnitId;
                }
            }

            return best;
        }

        private static void AddForwardArcTiles(GridManager gridManager, BossAttackIntent intent, int reach, int halfWidth)
        {
            GridPosition side = new GridPosition(-intent.Forward.Y, intent.Forward.X);
            for (int distance = 1; distance <= reach; distance++)
            {
                for (int width = -halfWidth; width <= halfWidth; width++)
                {
                    GridPosition position = new GridPosition(
                        intent.Origin.X + intent.Forward.X * distance + side.X * width,
                        intent.Origin.Y + intent.Forward.Y * distance + side.Y * width);

                    Tile tile;
                    if (gridManager.TryGetTile(position, out tile) && tile != null && !intent.AffectedTiles.Contains(tile))
                    {
                        intent.AffectedTiles.Add(tile);
                    }
                }
            }
        }

        private static List<Unit> BuildTargetsInIntent(IReadOnlyList<Unit> players, BossAttackIntent intent)
        {
            List<Unit> targets = new List<Unit>();
            if (players == null || intent == null)
            {
                return targets;
            }

            for (int i = 0; i < players.Count; i++)
            {
                Unit player = players[i];
                if (player == null
                    || player.Faction != UnitFaction.Player
                    || !player.IsAlive
                    || player.CurrentTile == null)
                {
                    continue;
                }

                if (intent.AffectedTiles.Contains(player.CurrentTile))
                {
                    targets.Add(player);
                }
            }

            return targets;
        }

        private bool TryBuildBossMovePath(
            Unit boss,
            IReadOnlyList<Unit> players,
            GridManager gridManager,
            Pathfinder pathfinder,
            out List<GridPosition> path,
            out BossIntentScore projectedMoveScore)
        {
            path = null;
            projectedMoveScore = BossIntentScore.Empty;
            if (boss == null || boss.CurrentTile == null || players == null)
            {
                return false;
            }

            Unit target = FindBestFocusTarget(boss.CurrentTile.Position, players);
            if (target == null)
            {
                return false;
            }

            if (!movementPlanner.TryBuildMoveTowardTarget(boss, target, gridManager, pathfinder, out path)
                || path == null
                || path.Count < 2)
            {
                return false;
            }

            BossAttackIntent projectedIntent;
            return TryBuildBestIntentFromOrigin(
                path[path.Count - 1],
                players,
                gridManager,
                GetCurrentAttackStep(boss),
                out projectedIntent,
                out projectedMoveScore);
        }

        private static bool ShouldMoveBeforeAttack(BossIntentScore currentScore, BossIntentScore projectedMoveScore)
        {
            if (projectedMoveScore.TargetCount <= 0)
            {
                return currentScore.TargetCount <= 0;
            }

            if (projectedMoveScore.TargetCount > currentScore.TargetCount)
            {
                return true;
            }

            return currentScore.TargetCount <= 0
                || (projectedMoveScore.TargetCount == currentScore.TargetCount
                    && projectedMoveScore.LowestHp < currentScore.LowestHp);
        }

        private IEnumerator WaitForBossMoveVisual(Unit boss, UnitMover unitMover, System.Func<bool> isMovementCompleted)
        {
            if (boss == null || unitMover == null || isMovementCompleted == null)
            {
                yield break;
            }

            float timeout = 4f;
            while (boss != null && !isMovementCompleted() && timeout > 0f)
            {
                timeout -= Time.deltaTime;
                yield return null;
            }
        }

        private static bool CanMove(Unit boss, ActionPermissionService actionPermissionService, TurnManager turnManager)
        {
            if (boss == null)
            {
                return false;
            }

            if (actionPermissionService == null && turnManager == null)
            {
                UnitTurnState fallbackTurnState = boss.GetComponent<UnitTurnState>();
                return fallbackTurnState == null || !fallbackTurnState.HasMoved;
            }

            if (actionPermissionService == null || turnManager == null)
            {
                return false;
            }

            return actionPermissionService.CanMove(boss, turnManager);
        }

        private static void MarkBossMoved(Unit boss)
        {
            UnitTurnState turnState = boss != null ? boss.GetComponent<UnitTurnState>() : null;
            if (turnState != null)
            {
                turnState.MarkMoved();
            }
        }

        private static IEnumerator PlayBossAttackPresentationRoutine(Unit boss, BossAttackIntent intent, Unit fallbackTarget)
        {
            if (boss == null || intent == null)
            {
                yield break;
            }

            UnitAnimationController animationController = boss.GetComponent<UnitAnimationController>();
            if (animationController != null)
            {
                Vector3 faceDirection = GetWorldForward(intent);
                if (faceDirection.sqrMagnitude <= 0.001f && fallbackTarget != null)
                {
                    animationController.FaceTowards(fallbackTarget.transform.position);
                }
                else
                {
                    animationController.FaceTowardsDirection(faceDirection);
                }

                yield return WaitForBossDelay(BossFacingSettleDelay);

                if (intent.Pattern == BossAttackPattern.Slam)
                {
                    animationController.PlaySkill(BossAttackAnimationSpeed, BossAttackAnimationRestoreDelay);
                }
                else
                {
                    animationController.PlayBasicAttack(BossAttackAnimationSpeed, BossAttackAnimationRestoreDelay);
                }
            }

            yield return WaitForBossDelay(BossAttackSoundDelay);
            BTAudioService.PlaySfx(BTAudioCueId.AxeChop, BossAttackVolumeScale, BossAttackPitch);
        }

        private static IEnumerator WaitForBossDelay(float delay)
        {
            if (delay > 0f)
            {
                yield return new WaitForSeconds(delay);
            }
        }

        private static void ApplyBossDamage(Unit boss, BossAttackIntent intent, List<Unit> targets, DamageResolver damageResolver, CombatLog combatLog)
        {
            if (damageResolver == null || targets == null)
            {
                return;
            }

            int baseDamage = boss != null && boss.Stats != null ? boss.Stats.BaseDamage : 4;
            int damage = intent.Pattern == BossAttackPattern.Slam ? baseDamage + 2 : baseDamage;
            for (int i = 0; i < targets.Count; i++)
            {
                Unit target = targets[i];
                if (target == null || !target.IsAlive)
                {
                    continue;
                }

                bool died = damageResolver.ApplyDamage(target, damage);
                int remainingHp = target.RuntimeState != null ? target.RuntimeState.CurrentHp : 0;
                if (combatLog != null)
                {
                    combatLog.LogHit(boss, target, damage, remainingHp);
                    if (died)
                    {
                        combatLog.LogDeath(target);
                    }
                }
            }
        }

        private void AdvanceAttackStep(Unit boss)
        {
            if (boss == null)
            {
                return;
            }

            int instanceId = boss.GetInstanceID();
            AttackStepByBossInstance[instanceId] = GetCurrentAttackStep(boss) + 1;
        }

        private static void MarkBossActed(Unit boss)
        {
            UnitTurnState turnState = boss != null ? boss.GetComponent<UnitTurnState>() : null;
            if (turnState != null)
            {
                turnState.MarkActed();
            }
        }

        private static bool IsActiveBoss(Unit boss)
        {
            return IsBossLikeUnit(boss)
                && boss.gameObject.activeInHierarchy
                && boss.IsAlive
                && boss.CurrentTile != null;
        }

        private static GridPosition GetCardinalForward(GridPosition origin, GridPosition target)
        {
            int dx = target.X - origin.X;
            int dy = target.Y - origin.Y;
            if (Mathf.Abs(dx) >= Mathf.Abs(dy))
            {
                return new GridPosition(dx >= 0 ? 1 : -1, 0);
            }

            return new GridPosition(0, dy >= 0 ? 1 : -1);
        }

        private static int GetManhattanDistance(GridPosition a, GridPosition b)
        {
            return Mathf.Abs(a.X - b.X) + Mathf.Abs(a.Y - b.Y);
        }

        private static BossIntentScore ScoreIntent(BossAttackIntent intent, IReadOnlyList<Unit> players)
        {
            BossIntentScore score = BossIntentScore.Empty;
            if (intent == null || players == null)
            {
                return score;
            }

            for (int i = 0; i < players.Count; i++)
            {
                Unit player = players[i];
                if (player == null
                    || player.Faction != UnitFaction.Player
                    || !player.IsAlive
                    || player.CurrentTile == null
                    || !intent.AffectedTiles.Contains(player.CurrentTile))
                {
                    continue;
                }

                int hp = player.RuntimeState != null ? player.RuntimeState.CurrentHp : int.MaxValue;
                int distance = GetManhattanDistance(intent.Origin, player.CurrentTile.Position);
                score.TargetCount++;
                score.LowestHp = Mathf.Min(score.LowestHp, hp);
                if (hp != int.MaxValue)
                {
                    score.TotalHp = score.TotalHp == int.MaxValue ? hp : score.TotalHp + hp;
                }

                score.NearestDistance = Mathf.Min(score.NearestDistance, distance);
            }

            return score;
        }

        private static Vector3 GetWorldForward(BossAttackIntent intent)
        {
            return new Vector3(intent.Forward.X, 0f, intent.Forward.Y);
        }

        private struct BossIntentScore
        {
            public int TargetCount;
            public int LowestHp;
            public int TotalHp;
            public int NearestDistance;

            public static BossIntentScore Empty
            {
                get
                {
                    return new BossIntentScore
                    {
                        TargetCount = 0,
                        LowestHp = int.MaxValue,
                        TotalHp = int.MaxValue,
                        NearestDistance = int.MaxValue
                    };
                }
            }

            public bool IsBetterThan(BossIntentScore other)
            {
                if (TargetCount != other.TargetCount)
                {
                    return TargetCount > other.TargetCount;
                }

                if (LowestHp != other.LowestHp)
                {
                    return LowestHp < other.LowestHp;
                }

                if (TotalHp != other.TotalHp)
                {
                    return TotalHp < other.TotalHp;
                }

                return NearestDistance < other.NearestDistance;
            }
        }
    }
}
