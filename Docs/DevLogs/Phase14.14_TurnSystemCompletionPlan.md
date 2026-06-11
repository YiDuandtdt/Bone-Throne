# Phase 14.14 - Turn System Completion Plan DevLog

## Scope

Phase 14.14 is documentation-only.

This phase records the future plan for completing the turn system after Phase 14.13-E identified that the current action economy is still incomplete.

## Files changed

- `Docs/Phase14_TurnSystemCompletionPlan.md`
- `Docs/DevLogs/Phase14.14_TurnSystemCompletionPlan.md`

`Docs/ACTIVE_TASK.md` was already synchronized to Phase 14.14 before this phase execution and was not changed in this pass.

## What this phase did

- Documented current `TurnManager`, `UnitTurnState`, `ActionPermissionService`, and `EnemyAIController` state.
- Documented the recommended one move + one action rule.
- Chose actor turn start as the recommended cooldown tick timing.
- Planned player actor lifecycle.
- Planned enemy turn lifecycle.
- Planned future `ActionPermissionService` expansion.
- Planned future `TurnManager` extensions.
- Planned the `EnemyAIController` / `EnemyTurnRunner` split.
- Reserved future Defend / Potion action economy integration.
- Listed future allowed and forbidden files.
- Listed future Phase 14.15 through Phase 14 Final Regression sequence.

## What this phase did not do

- Did not write gameplay code.
- Did not modify Assets.
- Did not modify Packages.
- Did not modify ProjectSettings.
- Did not modify C# files.
- Did not modify SkillData assets.
- Did not modify prefabs.
- Did not modify scenes.
- Did not implement Turn System Completion.
- Did not implement Defend.
- Did not implement Potion.

## Cooldown tick conclusion

Recommended future rule:

- Tick the current actor's cooldown at actor turn start.
- `TryUseSkill` should still start cooldown immediately after a successful skill.
- The cooldown should tick again the next time that actor begins a turn.

This avoids immediate end-turn cooldown reduction and avoids duplicate player cooldown ticks during a multi-enemy `EnemyTurn`.

## EnemyTurnRunner conclusion

Future implementation should split responsibilities:

- `EnemyAIController` remains responsible for a single enemy decision.
- `EnemyTurnRunner` should orchestrate the whole `EnemyTurn`.

The first implementation should stay simple:

- no behavior tree
- no enemy skills
- no complex priority system

## ActionPermissionService conclusion

Future implementation should keep `CanMove` and `CanAct`, but expand them to support:

- `PlayerTurn + UnitFaction.Player`
- `EnemyTurn + UnitFaction.Enemy`

It must continue to check alive state, `UnitTurnState`, `hasMoved`, and `hasActed`.

UI must not directly mark moved or acted.

## Defend / Potion conclusion

Defend future integration:

- Defend is an action.
- Future `DefendSystem.TryDefend` should check `CanAct`.
- It should set lightweight defend state, mark acted, and write structured CombatLog feedback.
- It should not implement taunt.

Potion future integration:

- Potion is an action.
- Future `PotionSystem.TryUsePotion` should check `CanAct`.
- It should apply simple healing, mark acted, and write structured CombatLog feedback.
- It should not implement a backpack system.

## Validation

No Unity Play Mode test was run in this documentation-only phase.

Future Phase 14.15 implementation should run the checklist in `Docs/Phase14_TurnSystemCompletionPlan.md`.

## Rollback

To roll back Phase 14.14 documentation:

1. Delete `Docs/Phase14_TurnSystemCompletionPlan.md`.
2. Delete `Docs/DevLogs/Phase14.14_TurnSystemCompletionPlan.md`.

## Recommended next phase

Proceed to:

- Phase 14.15 - Turn System Completion Implementation
