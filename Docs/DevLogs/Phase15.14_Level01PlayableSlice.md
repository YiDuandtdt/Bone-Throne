# Phase 15.14 - Level_01 Playable Slice

## Summary

Phase 15.14 created the first narrow playable-slice wiring pass for `Assets/_BoneThrone/Scenes/Level_01.unity`.

This phase only touched the approved scene and documentation scope. `GridTest.unity` remains the regression baseline and was not modified.

## Phase 15.15 Scope Correction

After review, the user confirmed that the Phase 15.14 `Level_01` wiring result is not suitable as a formal usable level baseline.

This devlog remains a historical record of what Codex attempted in Phase 15.14. It should not be read as approval to continue Codex-owned formal level scene construction.

From Phase 15.15 onward:

- `Level_01` formal scene content is user-owned.
- `Level_02` and `Level_03` formal scene content is user-owned.
- Codex must not directly create, modify, wire, or extend formal level scenes.
- Codex may provide planning, checklists, reviews, DevLogs, and non-scene system support.

## Unity-Skills Status

Unity-Skills was available for the main implementation and verification pass:

- Loaded `Level_01.unity`.
- Queried hierarchy and scene state.
- Instantiated approved player, enemy, interactable, UI, and environment prefabs.
- Re-loaded the scene after YAML wiring.
- Verified key scene object presence and selected serialized values.
- Ran scene validation and compile status checks.

During the final Play Mode attempt, the Unity-Skills REST service became unavailable after entering Play Mode. Because the REST service did not recover within the validation window, full Play Mode interaction validation is deferred.

## Preflight

Before modifying the scene:

```powershell
git status --short --untracked-files=all
git diff -- Assets/_BoneThrone/Scenes/GridTest.unity
```

Results:

- Working tree had no blocking unrelated scene changes.
- `GridTest.unity` had no diff.

## Modified Files

- `Assets/_BoneThrone/Scenes/Level_01.unity`
- `Docs/DevLogs/Phase15.14_Level01PlayableSlice.md`
- `Docs/ACTIVE_TASK.md`

## Scene Structure

`Level_01` now keeps the Phase 15.13 formal hierarchy groups and wires gameplay content into the existing roots:

- `01_Systems_Managers`
- `02_Cameras_Lighting`
- `03_Grid_Blockout`
- `04_Rooms_Level_01_Entrance_Combat_Key_Exit`
- `05_Units`
- `06_Interactables`
- `07_Dressing_Environment`
- `08_Debug_None_Do_Not_Copy_GridTest_Testers`
- `99_Deferred_Boss_LAN_Victory_Defeat_Not_Implemented`

The playable room pass uses:

- `Room_01_Start`
- `Room_02_Combat`
- `Room_03_Key`
- `Room_04_Stairs`
- `Room_05_Deferred_Optional_Potion_Pocket`

## Gameplay Wiring

Created a compact 14 x 4 tile blockout under:

- `03_Grid_Blockout/Tiles`

The following systems are present on `GameSystems`:

- `GridManager`
- `SelectionManager`
- `MovementRangeFinder`
- `Pathfinder`
- `UnitMover`
- `MovementDebugHighlighter`
- `PlayerMovementController`
- `TurnOrderService`
- `ActionPermissionService`
- `TurnManager`
- `D20Roller`
- `DamageResolver`
- `CombatLog`
- `AttackRangeService`
- `CombatSystem`
- `DefendSystem`
- `SkillTargetingService`
- `SkillEffectExecutor`
- `SkillSystem`
- `PotionSystem`
- `ActiveUnitProvider`
- `EnemyTurnRunner`
- `LevelManager`
- `LevelProgressionService`

The single-player `TurnManager` remains free-order:

- `currentPhase = PlayerTurn`
- `currentRole = None`
- `currentTurnIndex = -1`
- `ActionPermissionService.requireCurrentRole = false`

## Prefab Placement

Players:

- `Fighter` at `Tile_0_0`
- `Ranger` at `Tile_1_0`
- `Mage` at `Tile_0_1`
- `Barbarian` at `Tile_1_1`

Enemies:

- `Skeleton_Minion`
- `Skeleton_Warrior`

Enemies are assigned to `Room_02_Combat` via `RoomEnemyActivator` and remain unplaced until the room activates them.

Interactables:

- `HealthPotion_Level01`
- `Key_Level01`
- `Stairs_Level01`

UI:

- `BattleHUD` prefab is placed under `BattleHUD_UI_To_Wire`.

Environment dressing:

- `Env_Crate_Start`
- `Env_Barrel_Combat`
- `Env_Torch_Exit`

## Room Shadow And Activation

`Room_02_Combat`, `Room_03_Key`, and `Room_04_Stairs` now have:

- `RoomController`
- room trigger helper
- room shadow helper

`Room_02_Combat` also has:

- `RoomEnemyActivator`
- assigned enemies
- spawn tiles

Room shadows continue the existing simple `RoomShadowController` pattern. This is not a new fog-of-war implementation.

## Validation

Unity-Skills scene reload after wiring:

- `Level_01.unity` loaded successfully.
- Scene was not dirty after reload.

Key checks:

- `Tile_13_3` reports grid position `(13, 3)`.
- `Fighter`, `Ranger`, `Mage`, and `Barbarian` report correct `CurrentTile` values.
- `Skeleton_Minion` reports `CurrentTile = null`, as expected before room activation.
- `TurnManager` reports `PlayerTurn`, `None`, `-1`.

Scene validation:

- No missing script errors.
- No missing reference errors.
- Validation reported only informational duplicate-name and empty-placeholder findings from nested model rigs and explicit placeholder groups.

Compile status:

- Unity reported `isCompiling = false`.

Console notes:

- Existing Unity Connect token exchange errors were observed before the Play Mode attempt.
- MeshCollider convex warnings appeared from instantiated character prefabs.

Play Mode:

- Play Mode validation was attempted.
- The Unity-Skills REST service became unavailable during or after the Play Mode transition.
- Full interaction validation is deferred until the Unity-Skills service is reachable again or manual Unity Play Mode testing is performed.

## Forbidden Scope Check

Not modified:

- `Assets/_BoneThrone/Scenes/GridTest.unity`
- `Assets/_BoneThrone/Scenes/Level_02.unity`
- `Assets/_BoneThrone/Scenes/Level_03.unity`
- C# code
- Prefab assets
- SkillData
- KayKit source
- ProjectSettings
- Packages

Not implemented:

- Boss fight
- BossDoor / BossKey
- SupplyPoint
- Chest loot
- Door lock / unlock
- Victory / Defeat / Retry
- LAN / Networking
- Turn / Combat / Skill / Potion refactors

## Deferred

- Any decision to keep, alter, replace, or discard the Phase 15.14 `Level_01` scene contents is deferred to the user.
- Formal level scene construction is no longer assigned to Codex.
- Future Codex work should focus on non-scene preparation, documentation, review, and narrow system / prefab / code fixes when explicitly approved.

## Rollback

To roll back Phase 15.14 scene work:

```powershell
git restore -- Assets/_BoneThrone/Scenes/Level_01.unity Docs/DevLogs/Phase15.14_Level01PlayableSlice.md Docs/ACTIVE_TASK.md
```

`GridTest.unity` should remain unchanged before and after rollback.
