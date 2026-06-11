# Phase 14.15-C - Free Player Turn Order and Ended Unit Rules

## Scope

Phase 14.15-C changes the local single-player PlayerTurn rule from fixed player role order to free player-unit selection.

The fixed Fighter -> Ranger -> Mage -> Barbarian -> Enemy order remains a future LAN design target, but it no longer controls the current single-player PlayerTurn.

## Files changed

- `Assets/_BoneThrone/Scripts/Turns/UnitTurnState.cs`
- `Assets/_BoneThrone/Scripts/Turns/TurnManager.cs`
- `Assets/_BoneThrone/Scripts/Turns/ActionPermissionService.cs`
- `Assets/_BoneThrone/Scripts/Movement/SelectionManager.cs`
- `Assets/_BoneThrone/Scripts/Movement/PlayerMovementController.cs`
- `Assets/_BoneThrone/Scripts/UI/BattleHUDController.cs`
- `Assets/_BoneThrone/Scripts/UI/HeroPanelView.cs`
- `Assets/_BoneThrone/Scripts/UI/TurnBannerView.cs`
- `Assets/_BoneThrone/Scripts/UI/UIActionModeController.cs`
- `Docs/ACTIVE_TASK.md`
- `Docs/DevLogs/Phase14.15C_FreePlayerTurnOrder.md`

`MovementDebugHighlighter.cs` was not modified.

## Free player order rule

PlayerTurn now allows the player to choose any alive player unit that has not ended this round.

End Turn ends the selected player unit only. If other alive player units have not ended, the phase remains `PlayerTurn` and waits for the next selection.

EnemyTurn begins only after every alive player unit has ended.

## UnitTurnState ended state

`UnitTurnState` now tracks:

- `HasMoved`
- `HasActed`
- `HasEnded`

New API:

- `MarkEnded()`

`ResetForNewRound()` now clears moved, acted, and ended state.

End Turn marks only ended state. It does not mark moved or acted.

## TurnManager changes

`StartPlayerRound()` now:

- resets all player unit turn states
- disables current-role enforcement for local free player order
- ticks cooldowns for alive player units once at player round start
- sets `CurrentPhase = PlayerTurn`
- sets `CurrentRole = None`
- waits for player selection

`TryEndPlayerUnitTurn(Unit unit)` now:

- validates `PlayerTurn`
- validates alive player unit
- validates `UnitTurnState`
- rejects units that already ended
- calls `MarkEnded()`
- checks whether all alive players have ended
- calls `BeginEnemyTurn()` only when all alive players have ended

`AreAllAlivePlayersEnded()` returns true only when at least one alive player exists and every alive player has `HasEnded`.

`EndCurrentActorTurn()` remains for future fixed-order/debug use, but the End Turn UI no longer uses it.

## Cooldown tick rule

Free player order no longer has a reliable actor turn start for each player because selection order is player-controlled.

The new local rule is:

- player skill cooldowns tick once at the start of each new PlayerTurn round
- each alive player with `SkillRuntime` is ticked once
- cooldowns do not tick on every selection
- cooldowns do not tick when clicking the same unit repeatedly
- cooldowns do not tick when pressing End Turn

This avoids duplicate cooldown ticks in free selection mode while preserving one cooldown reduction per player round.

## ActionPermissionService changes

PlayerTurn now allows alive Player faction units that have not ended.

Movement is denied when:

- unit has ended
- unit has already moved

Action is denied when:

- unit has ended
- unit has already acted

EnemyTurn still allows Enemy faction units for EnemyTurnRunner / EnemyAIController.

Current-role enforcement is ignored while `TurnManager.CurrentRole == None`, which is the single-player free-order state.

## Selection and movement changes

`SelectionManager.TrySelect(...)` now rejects player units that have already ended.

`PlayerMovementController` now ignores selection clicks outside `PlayerTurn`, and movement refuses selected units that have already ended.

The movement algorithm, pathfinding, and `UnitMover` were not changed.

## End Turn UI changes

`BattleHUDController.HandleEndTurnClicked()` now reads `selectionManager.SelectedUnit`.

It rejects:

- missing selection
- non-player unit
- dead unit
- missing `UnitTurnState`
- already-ended unit
- non-PlayerTurn phase

On success, it:

1. calls `UIActionModeController.CancelTargetingForExternalAction()`
2. calls `TurnManager.TryEndPlayerUnitTurn(selectedUnit)`
3. clears selection
4. clears movement/action highlights

The HUD does not directly call `MarkEnded()`, `MarkMoved()`, `MarkActed()`, cooldown ticks, `BeginEnemyTurn()`, or `EnemyTurnRunner`.

## Hero status UI

`HeroPanelView` now displays:

- `Not Started`
- `Active`
- `Ended`

Rules:

- Ended = `turnState.HasEnded`
- Active = selected unit, or moved/acted and not ended
- Not Started = not moved, not acted, not ended

`BattleHUDController` passes the selected unit into hero panel refresh so the selected player can display as Active before moving or acting.

## Turn banner

`TurnBannerView` now shows:

- `Turn: Player Turn - Select a unit`
- `Turn: Player Turn | Selected: <name>`
- `Turn: Enemy Turn`

It no longer presents Fighter / Ranger / Mage / Barbarian as a fixed current actor in local PlayerTurn.

## Ground highlight cleanup

End Turn clears targeting, selection, and `MovementDebugHighlighter.Clear()`.

This restores selected/action tile colors to their remembered default material color and prevents blue, green, red, or yellow highlights from lingering under player units.

No KayKit, prefab, material, scene, or tile asset changes were made.

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
- KayKit original assets
- `Skeleton_Rogue`
- `Skeleton_Golem`
- Ranger visual / identity
- Defend
- Potion
- skill formulas
- networking
- initiative
- AP
- behavior trees

## Unity Play Mode test steps

1. Open `Assets/_BoneThrone/Scenes/GridTest.unity`.
2. Confirm there are no compile errors.
3. Enter Play Mode.
4. Confirm TurnBanner says `Player Turn - Select a unit`.
5. Select any alive player unit, not necessarily Fighter.
6. Confirm selected hero panel shows `Active`.
7. Move or act with the selected unit, then click End Turn.
8. Confirm the unit shows `Ended`.
9. Confirm the unit is deselected and highlights clear.
10. Try selecting the ended unit again and confirm selection is rejected.
11. Select another unended player unit and confirm movement / Basic Attack / Skill Slot 0 / 1 / 2 still work.
12. End three alive player units and confirm the phase remains PlayerTurn.
13. End the final alive player unit and confirm EnemyTurn begins.
14. Confirm EnemyTurn runs and then returns to a new PlayerTurn.
15. Confirm all player hero panels return to `Not Started`.
16. Confirm moved, acted, and ended state reset for the new PlayerTurn.
17. Confirm cooldowns tick once at the start of the new PlayerTurn, not on repeated selection.
18. Confirm End Turn does not directly modify HP, moved, acted, or cooldown state.
19. Confirm Defend and Potion remain placeholders.
20. Confirm CombatLog, Enemy HP Bar, ActiveUnitProvider, camera controls, Room / Key / Stairs / LevelUp still work.
21. Confirm Console has no new red errors.

## Risks

- The cooldown timing changed from actor-turn start to player-round start for local free order. This is intentional to avoid repeated ticks from repeated selection.
- If `TurnManager.playerUnits` is incomplete, all-ended detection may be wrong.
- If a player unit lacks `UnitTurnState`, it cannot be ended and all-ended detection may remain false.
- Existing tests that assume fixed Fighter-first local order may need updating in a later test-maintenance phase.
- Runtime selection rejection for ended units depends on `UnitTurnState` being present on player prefabs/instances.

## Rollback

To roll back Phase 14.15-C:

1. Revert `Assets/_BoneThrone/Scripts/Turns/UnitTurnState.cs`.
2. Revert `Assets/_BoneThrone/Scripts/Turns/TurnManager.cs`.
3. Revert `Assets/_BoneThrone/Scripts/Turns/ActionPermissionService.cs`.
4. Revert `Assets/_BoneThrone/Scripts/Movement/SelectionManager.cs`.
5. Revert `Assets/_BoneThrone/Scripts/Movement/PlayerMovementController.cs`.
6. Revert `Assets/_BoneThrone/Scripts/UI/BattleHUDController.cs`.
7. Revert `Assets/_BoneThrone/Scripts/UI/HeroPanelView.cs`.
8. Revert `Assets/_BoneThrone/Scripts/UI/TurnBannerView.cs`.
9. Revert `Assets/_BoneThrone/Scripts/UI/UIActionModeController.cs`.
10. Revert `Docs/ACTIVE_TASK.md`.
11. Delete `Docs/DevLogs/Phase14.15C_FreePlayerTurnOrder.md`.

No scene, prefab, SkillData, combat, skill formula, KayKit, camera, or ActiveUnitProvider rollback is needed because those systems were not changed.
