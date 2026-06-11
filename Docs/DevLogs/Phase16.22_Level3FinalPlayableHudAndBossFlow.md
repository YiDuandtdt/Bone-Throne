# Phase 16.22 - Level 3 Final Playable HUD And Boss Flow

Date: 2026-06-10

## Scope

The imported `Level_3_final` scene needed final playable wiring:

- Battle HUD was present but inactive.
- Battle HUD gameplay references were empty.
- `ActiveUnitProvider` existed as a named GameObject but had no component.
- The final boss room was blocked by BossDoor-specific checks even though this scene does not use the formal BossDoor gate flow.
- The final win condition should be stairs interaction after the boss is defeated, not automatic victory on boss death.

## Changes

- Enabled the `BattleHUD` prefab instance in `Assets/_BoneThrone/Scenes/Level_3_final.unity`.
- Added an `ActiveUnitProvider` component to the `ActiveUnitProvider` GameObject in `Level_3_final`.
- Set `Room_D_Trigger.requireOpenedBossDoorEntryForBossRooms` to false.
- Set `Room_D_LargeCombatHall.requireBossDoorOpenedForBossRooms` to false.
- Kept `LevelProgressionService.requiredBossDefeatedRooms` pointed at `Room_D_LargeCombatHall`.
- Kept `SceneLevelTransition.nextSceneName` set to `EndMenu`.
- Added one-time runtime fallback binding in `BattleHUDController` for missing scene references.
- Added one-time player unit auto-binding in `TurnManager` when `playerUnits` is empty.
- Made `EnemyTurnRunner` create/use an `ActiveUnitProvider` if none exists.
- Stopped `RoomController` from creating a new BossGate state just by entering a boss room.
- Let boss enemies participate in enemy turns when no explicit BossDoor/BossGate state exists.

## Verification

- `dotnet build .\Assembly-CSharp-Editor.csproj --no-restore` passed, 0 errors.
- `dotnet build .\Assembly-CSharp.csproj --no-restore` passed, 0 errors.
- `ProjectSettings/EditorBuildSettings.asset` includes `Assets/_BoneThrone/Scenes/Level_3_final.unity`.
- `StartMenuController` and `UI_StartMenuCanvas.prefab` point level 3 to `Level_3_final`.
- `Level_3_final` contains `Skeleton_Golem_Boss` and `Room_D_LargeCombatHall` references it as the required boss-defeated room.

## Play Mode Steps

1. Open `Assets/_BoneThrone/Scenes/Level_3_final.unity`.
2. Press Play.
3. Confirm the battle HUD is visible immediately.
4. Select a player unit and move toward `Room_D_Trigger`.
5. Enter the middle boss hall; enemies including `Skeleton_Golem_Boss` should activate.
6. End player turns and confirm enemies can take turns.
7. Kill the boss.
8. Use the stairs twice if confirmation is required.
9. Expected result: the scene transitions to `EndMenu`.

## Risks

- The enemy list still uses scene-deployed units as activation sources. This matches the imported scene setup, but a later cleanup pass could replace them with prefab assets if level production wants fully reusable spawn prefabs.
- Third-level final balancing was not changed.
- Scene lighting and URP warnings were not touched.

## Rollback

- Revert `Assets/_BoneThrone/Scenes/Level_3_final.unity`.
- Revert the narrow runtime fallback changes in:
  - `Assets/_BoneThrone/Scripts/UI/BattleHUDController.cs`
  - `Assets/_BoneThrone/Scripts/Turns/TurnManager.cs`
  - `Assets/_BoneThrone/Scripts/AI/EnemyTurnRunner.cs`
  - `Assets/_BoneThrone/Scripts/Rooms/RoomController.cs`
