# ACTIVE_TASK.md

## Current phase
Phase 14.15-C - Free Player Turn Order and Ended Unit Rules

## Goal
Change the current single-player `PlayerTurn` from fixed Fighter -> Ranger -> Mage -> Barbarian actor order to free player-unit selection.

New local rules:

- PlayerTurn has no fixed player order.
- Players may select any alive player unit that has not ended.
- End Turn ends the currently selected player unit.
- Ended player units cannot be selected, moved, or used for actions again this round.
- EnemyTurn begins only after all alive player units have ended.
- EnemyTurn completion starts a new PlayerTurn and resets all player turn state.
- Hero panels show Not Started / Active / Ended.
- End Turn clears targeting, selection, and highlight state.

## Allowed files
- `Assets/_BoneThrone/Scripts/Turns/UnitTurnState.cs`
- `Assets/_BoneThrone/Scripts/Turns/TurnManager.cs`
- `Assets/_BoneThrone/Scripts/Turns/ActionPermissionService.cs`
- `Assets/_BoneThrone/Scripts/Movement/SelectionManager.cs`
- `Assets/_BoneThrone/Scripts/Movement/PlayerMovementController.cs`
- `Assets/_BoneThrone/Scripts/UI/BattleHUDController.cs`
- `Assets/_BoneThrone/Scripts/UI/HeroPanelView.cs`
- `Assets/_BoneThrone/Scripts/UI/TurnBannerView.cs`
- `Assets/_BoneThrone/Scripts/UI/UIActionModeController.cs`
- `Docs/DevLogs/Phase14.15C_FreePlayerTurnOrder.md`
- `Docs/ACTIVE_TASK.md`

## Forbidden changes
- Do not modify `DamageResolver.cs`.
- Do not modify `SkillEffectExecutor.cs`.
- Do not modify `SkillSystem.cs`.
- Do not modify `SkillTargetingService.cs`.
- Do not modify `CombatSystem.cs`.
- Do not modify SkillData assets.
- Do not modify Player prefabs.
- Do not modify enemy prefabs.
- Do not modify scene files, including `GridTest.unity`.
- Do not modify KayKit original assets.
- Do not modify `Skeleton_Rogue`.
- Do not modify `Skeleton_Golem`.
- Do not modify Ranger visual or identity.
- Do not implement Defend.
- Do not implement Potion.
- Do not rebuild skills.
- Do not introduce initiative, AP, networking, behavior trees, or complex UI art.

## Required output
After implementation, report:

1. Actual modified files.
2. Whether `MovementDebugHighlighter` was modified.
3. What was added to `UnitTurnState`.
4. How `TurnManager` detects all players ended.
5. How End Turn ends the selected unit.
6. How ended units are blocked from selection.
7. How Hero UI shows Not Started / Active / Ended.
8. Cooldown tick rule in free player order.
9. How player ground highlight returns to default state.
10. Confirmation that forbidden files were not changed.
11. Unity Play Mode test steps.
12. Risks and rollback.

## Validation
Implementation phase.

Manual checks:

1. Confirm only allowed files changed.
2. Confirm no scene, prefab, SkillData, Packages, ProjectSettings, Library, Temp, Obj, Logs, or UserSettings changes.
3. Open `Assets/_BoneThrone/Scenes/GridTest.unity`.
4. Enter Play Mode and validate free player turn order.
