# Phase 15.2 - Sequential Enemy Turn Actions

## Summary
EnemyTurn now runs as a coroutine in `EnemyTurnRunner`, processing enemies one at a time with a short delay after each enemy. The existing single-enemy AI decision path remains unchanged.

## Actual Modified Files
- `Assets/_BoneThrone/Scripts/AI/EnemyTurnRunner.cs`
- `Docs/DevLogs/Phase15.2_SequentialEnemyTurnActions.md`

## What Changed
- Converted EnemyTurn orchestration from a same-frame loop into a coroutine.
- Added `enemyActionDelay`, default `0.4`, to make each enemy action observable in Play Mode.
- Added coroutine re-entry protection through the existing `isRunning` guard.
- Added stable enemy ordering before execution.
- Added local coroutine cleanup in `OnDisable`; if the runner is disabled during EnemyTurn, it stops its own coroutine and asks the current `TurnManager` to end EnemyTurn instead of leaving the phase stuck.

## What Did Not Change
- Did not modify `TurnManager`.
- Did not modify `EnemyAIController`.
- Did not rewrite enemy AI.
- Did not modify `CombatSystem`, `SkillSystem`, `SkillData`, or `PotionSystem`.
- Did not modify `GridTest.unity`.
- Did not modify LAN / Networking.
- Did not create formal Level scenes.
- Did not change single-player free-order PlayerTurn.
- Did not roll single-player back to Fighter -> Ranger -> Mage -> Barbarian fixed-order.

## Execution Rules Preserved
- `TurnManager.BeginEnemyTurn()` still calls `EnemyTurnRunner.RunEnemyTurn(TurnManager)`.
- `EnemyAIController.TryRunAction(...)` still handles a single enemy's move / attack / skip decision.
- Enemy bleed still ticks before that enemy acts.
- If bleed kills the enemy, AI action is skipped and the runner continues after the delay.
- Stun still consumes that enemy's move + action opportunity and skips AI action.
- After every enemy is processed, `TurnManager.EndEnemyTurn()` is called.

## Enemy Order
Enemies are sorted by:

1. Units with a current tile before units without a current tile.
2. Current tile grid Y.
3. Current tile grid X.
4. `UnitId`.
5. `GetInstanceID()` as a final fallback.

This avoids relying on scene hierarchy or unspecified `FindObjectsByType` order.

## Unity 6.3 Manual Test Steps
1. Open `Assets/_BoneThrone/Scenes/GridTest.unity`.
2. Confirm Unity compiles with no red Console errors.
3. Enter Play Mode.
4. End all alive player units.
5. Confirm EnemyTurn starts and enemies act one by one, with a visible delay.
6. Confirm dead or inactive enemies do not act.
7. Apply bleed to an enemy and confirm bleed ticks before that enemy acts.
8. Confirm an enemy killed by bleed does not move or attack.
9. Apply stun to an enemy and confirm it skips move + action while later enemies still act.
10. Confirm EnemyTurn returns to PlayerTurn after all enemies are processed.
11. Confirm the new PlayerTurn still allows free selection of any alive not-ended player.
12. Spot-check Potion, Skill, Defend, and Basic Attack still use their existing behavior.

## Risks
- If the runner is disabled during EnemyTurn, it ends the current EnemyTurn as a local recovery path; scene teardown or editor stop may still interrupt normal visual observation.
- If `enemyActionDelay` is set too high, EnemyTurn may feel slow.
- If `enemyActionDelay` is set to `0`, behavior becomes nearly instant again but still runs through coroutine steps.
- Existing AI movement and attack remain instant inside each enemy step.
- Victory / defeat flow is still deferred, so player wipe during EnemyTurn is not resolved by this phase.

## Rollback
Rollback this phase by reverting:

- `Assets/_BoneThrone/Scripts/AI/EnemyTurnRunner.cs`
- `Docs/DevLogs/Phase15.2_SequentialEnemyTurnActions.md`

No `TurnManager`, `EnemyAIController`, `CombatSystem`, `SkillSystem`, `PotionSystem`, `GridTest`, LAN, Boss, or formal Level rollback is required because those files and systems were not changed.
