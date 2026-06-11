# Phase 16.21 - Level 3 Final Victory Flow

Date: 2026-06-10

## Scope

Adapted the imported `Level_3_final` scene into the active final Level 3 flow.

## Changes

- `LevelProgressionService` now supports scenes that do not require a shared key before stairs progression.
- `LevelProgressionService` can auto-resolve active player units when the scene field is empty, preserving party progression and level-up capture for imported scenes.
- `RoomController.CheckCleared` no longer treats an unentered room with configured enemies as cleared.
- `RoomController` and `RoomEnemyActivator` now expose a boss-like enemy defeat check for room-based progression gates.
- `Level_3_final` stairs progression now loads `EndMenu` only after the boss-like enemy in `Room_D_LargeCombatHall` is defeated.

## Inspector Setup

- `Level_3_final/LevelProgressionManager/LevelProgressionService`
  - `Skip Shared Key Requirement`: on
  - `Auto Find Player Units If Missing`: on
  - `Required Boss Defeated Rooms`: `Room_D_LargeCombatHall`
  - `Required Cleared Rooms`: empty
  - `Scene Transition`: `LevelProgressionManager/SceneLevelTransition`
- `Level_3_final/LevelProgressionManager/SceneLevelTransition`
  - `Next Scene Name`: `EndMenu`

## Verification

- `dotnet build .\Assembly-CSharp.csproj --no-restore`: passed with existing CS0649 warnings.
- `dotnet build .\Assembly-CSharp-Editor.csproj --no-restore`: passed with existing CS0649 warnings.

## Play Mode Steps

1. Open `Assets/_BoneThrone/Scenes/Level_3_final.unity`.
2. Enter the middle combat hall trigger.
3. Confirm the boss room enemies activate.
4. Try the stairs before defeating the boss: progression should be rejected.
5. Defeat `Skeleton_Golem_Boss`.
6. Click stairs twice if confirmation is enabled.

## Expected Result

- Killing the boss no longer auto-wins the scene.
- The player must use the stairs to finish Level 3.
- The stairs transition to `EndMenu` after the required middle boss is defeated.

## Risks

- The boss gate uses the existing boss/golem name matching in `RoomEnemyActivator`, so imported boss names must keep either the configured boss or golem keyword.
- Unity should reserialize the new `LevelProgressionService` fields after opening/saving scenes.

## Rollback

- Revert `LevelProgressionService.cs`, `RoomController.cs`, and the `LevelProgressionService` block in `Level_3_final.unity`.
