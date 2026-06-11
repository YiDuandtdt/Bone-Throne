# Phase 14.15 - Turn System Completion Implementation

## Scope

Phase 14.15 implements the minimal local turn system completion planned in Phase 14.14.

The implementation keeps the current single-player GridTest flow and does not add Defend, Potion, enemy skills, initiative, AP, networking, behavior trees, or Boss logic.

## Files changed

- `Assets/_BoneThrone/Scripts/Turns/TurnManager.cs`
- `Assets/_BoneThrone/Scripts/Turns/ActionPermissionService.cs`
- `Assets/_BoneThrone/Scripts/AI/EnemyAIController.cs`
- `Assets/_BoneThrone/Scripts/AI/EnemyTurnRunner.cs`
- `Assets/_BoneThrone/Scripts/AI/EnemyTurnRunner.cs.meta`
- `Docs/ACTIVE_TASK.md`
- `Docs/DevLogs/Phase14.15_TurnSystemCompletionImplementation.md`

## TurnManager changes

`TurnManager` now coordinates an explicit local actor lifecycle:

1. `StartPlayerRound()` resets player unit turn state and begins Fighter.
2. `BeginActorTurn(role)` sets `PlayerTurn`, sets the current role, finds the matching player unit, and starts that actor turn.
3. Missing or dead player actors are skipped safely.
4. `EndCurrentActorTurn()` advances to the next fixed role.
5. Enemy role enters `BeginEnemyTurn()`.
6. `BeginEnemyTurn()` delegates orchestration to `EnemyTurnRunner`.
7. `EndEnemyTurn()` returns to `StartPlayerRound()`.

The fixed order remains:

- Fighter
- Ranger
- Mage
- Barbarian
- Enemy

`TurnManager` does not contain concrete enemy AI decisions.

## Cooldown tick integration

Cooldowns tick only at player actor turn start:

- `BeginActorTurn(role)` finds the current alive player actor.
- If the actor has `SkillRuntime`, `TurnManager` calls `SkillSystem.TickCooldownsForUnit(actor)`.
- `SkillSystem.TryUseSkill(...)` remains unchanged and still starts cooldown immediately after a successful skill.
- Cooldowns are not ticked every frame.
- Cooldowns are not ticked at actor turn end.
- Player cooldowns are not batch-ticked during EnemyTurn.

## ActionPermissionService expansion

`ActionPermissionService.CanMove(...)` and `CanAct(...)` now support:

- `PlayerTurn + UnitFaction.Player`
- `EnemyTurn + UnitFaction.Enemy`

The service still checks:

- unit exists
- turn manager exists
- current phase allows the unit faction
- unit is alive
- `UnitTurnState` exists
- `HasMoved` for movement
- `HasActed` for actions

`RequireCurrentRole` remains a player-role restriction only, so player UI cannot use the wrong actor during `PlayerTurn`. Enemy units are only allowed during `EnemyTurn`.

## EnemyTurnRunner

Added `EnemyTurnRunner` as a lightweight EnemyTurn orchestrator.

Responsibilities:

- collect active alive player and enemy units
- prefer `ActiveUnitProvider` when present
- fall back to serialized arrays if provider data is unavailable
- reset each enemy's move/action allowance before its decision
- call `EnemyAIController` once per active alive enemy
- notify `TurnManager.EndEnemyTurn()` after all enemies are processed
- guard against re-entry with an `isRunning` flag

Boundaries:

- no enemy skills
- no behavior tree
- no Boss logic
- no room activation changes
- no scene or prefab binding changes

If no `EnemyTurnRunner` is bound in the scene, `TurnManager` resolves an existing one or adds one to the `TurnManager` GameObject at runtime. This avoids a `GridTest.unity` scene edit.

## EnemyAIController changes

`EnemyAIController` remains responsible for one enemy's single decision.

Minimal additions:

- Optional `ActionPermissionService` and `TurnManager` parameters were added to `TryRunAction(...)`.
- Movement checks `CanMove(...)` before path execution when turn gating is configured.
- Attack checks `CanAct(...)` before calling `CombatSystem.TryBasicAttack(...)` when turn gating is configured.
- If turn gating is not supplied, it still respects local `UnitTurnState` moved/acted flags for test helper compatibility.
- Movement success still marks `MarkMoved()`.
- Basic attack action consumption still comes from `CombatSystem.TryBasicAttack(...)`.

## Explicit non-changes

No changes were made to:

- `DamageResolver.cs`
- `SkillEffectExecutor.cs`
- `SkillSystem.cs`
- `SkillTargetingService.cs`
- `CombatSystem.cs`
- SkillData assets
- Player prefabs
- enemy prefabs
- scene files, including `GridTest.unity`
- UI scripts
- KayKit original resources
- `Skeleton_Rogue`
- `Skeleton_Golem`
- Ranger visual / identity
- Phase 14.10 camera controls
- Phase 14.11 `ActiveUnitProvider`
- Phase 14.13 skill assets or skill formulas

## Unity Play Mode test steps

1. Open `Assets/_BoneThrone/Scenes/GridTest.unity`.
2. Confirm there are no compile errors.
3. Enter Play Mode.
4. Confirm turn order: Fighter -> Ranger -> Mage -> Barbarian -> Enemy -> Fighter.
5. Confirm each player actor can move only once per round.
6. Confirm Basic Attack consumes action.
7. Confirm Skill Slot 0 / 1 / 2 consumes action.
8. Confirm Basic Attack and Skill cannot both be used by the same actor in the same turn.
9. Confirm successful skill use starts cooldown.
10. Confirm cooldown ticks when that same actor starts a later turn.
11. Confirm EnemyTurn automatically processes active alive enemies.
12. Confirm dead or inactive enemies do not act.
13. Confirm enemy movement is not rejected by the player-only gate.
14. Confirm enemy basic attack is not rejected by the player-only gate.
15. Confirm EnemyTurn completes and returns to Fighter / PlayerTurn.
16. Confirm UI Slot 0 / 1 / 2 targeting still works.
17. Confirm ActiveUnitProvider target collection still works.
18. Confirm CombatLog structured entries still work.
19. Confirm Enemy Floating HP Bar refresh / death hide still works.
20. Confirm camera middle drag, wheel zoom, and right-drag rotation still work.
21. Confirm Room / Key / Stairs / LevelUp still work.
22. Confirm Console has no new red errors.

## Validation notes

`dotnet build Assembly-CSharp.csproj` was attempted as a C# check, but the generated Unity `.csproj` does not yet include the newly added `EnemyTurnRunner.cs`, so the external build failed with a missing `EnemyTurnRunner` type. The `.csproj` file was not modified because generated IDE project files are outside this phase's allowed files and Unity normally regenerates them after importing new scripts.

Unity Play Mode validation still needs to be run in the Editor after Unity imports the new script.

## Risks

- Cooldown timing now formally happens at actor turn start, which changes any previous manual expectation that cooldowns only tick from tester calls.
- Automatic EnemyTurn can expose missing scene service references sooner because enemies now try to act as part of the turn loop.
- If `TurnManager.playerUnits` is incomplete, actor turn lookup may skip a role.
- If no alive player units remain, `TurnManager` stops the local turn loop instead of recursively cycling forever.
- Runtime-added `EnemyTurnRunner` avoids scene edits, but explicit scene binding may be clearer in a later approved setup phase.

## Rollback

To roll back Phase 14.15:

1. Revert `Assets/_BoneThrone/Scripts/Turns/TurnManager.cs`.
2. Revert `Assets/_BoneThrone/Scripts/Turns/ActionPermissionService.cs`.
3. Revert `Assets/_BoneThrone/Scripts/AI/EnemyAIController.cs`.
4. Delete `Assets/_BoneThrone/Scripts/AI/EnemyTurnRunner.cs`.
5. Delete `Assets/_BoneThrone/Scripts/AI/EnemyTurnRunner.cs.meta`.
6. Revert `Docs/ACTIVE_TASK.md`.
7. Delete `Docs/DevLogs/Phase14.15_TurnSystemCompletionImplementation.md`.

Do not roll back SkillData assets, prefabs, scenes, `DamageResolver`, `SkillEffectExecutor`, `SkillSystem`, `SkillTargetingService`, `CombatSystem`, UI scripts, camera controls, or `ActiveUnitProvider`, because they were not changed in this phase.
