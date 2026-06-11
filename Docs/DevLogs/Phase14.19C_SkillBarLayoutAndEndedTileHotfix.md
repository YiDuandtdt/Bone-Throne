# Phase 14.19-C - SkillBar Layout and Ended Tile Hotfix

## Goal
Apply a minimal hotfix for two Play Mode issues:
- SkillBar action buttons overflowing off screen.
- Ended player foot tile briefly repainting white before turning grey.

## Actual Modified Files
- `Assets/_BoneThrone/Scripts/UI/BattleHUDController.cs`
- `Assets/_BoneThrone/Scripts/UI/SkillBarView.cs`
- `Assets/_BoneThrone/Scripts/Movement/MovementDebugHighlighter.cs`
- `Docs/DevLogs/Phase14.19C_SkillBarLayoutAndEndedTileHotfix.md`
- `Docs/ACTIVE_TASK.md`

## SkillBar Overflow Cause
The runtime SkillBar used one horizontal row for eight buttons:
`Move | BasicAttack | SkillSlot0 | SkillSlot1 | SkillSlot2 | Defend | Potion | End Turn`.

At narrower Play Mode resolutions the row could exceed available screen width, placing some buttons outside the visible area.

## SkillBar New Layout
Runtime SkillBar now uses `GridLayoutGroup` with fixed 4 columns and 2 rows:
- Row 1: Move | Attack | Skill 0 | Skill 1
- Row 2: Skill 2 | Defend | Potion | End

The runtime SkillBar size was reduced to fit better on common Play Mode widths, and action button labels use shorter text while preserving all button events and action meanings. Skill Slot 0/1/2 availability, Defend, Potion, and End Turn logic are unchanged.

## Ended Tile Immediate Grey
`MovementDebugHighlighter.Clear()` now clears action and selection highlights, then calls `RefreshPlayerFootTiles()` instead of repainting the cached baseline. This forces the foot tile baseline to be rebuilt from current unit state after End Turn.

Because `BattleHUDController.HandleEndTurnClicked()` already calls `TurnManager.TryEndPlayerUnitTurn(selectedUnit)` before `movementHighlighter.Clear()`, the unit's `UnitTurnState.HasEnded` is true by the time the highlighter refreshes. The tile therefore repaints grey immediately instead of waiting for the 0.25 second auto refresh.

## Preserved Behavior
- White baseline: alive player, not ended.
- Grey baseline: alive player, ended.
- Enemy tiles are ignored.
- Dead player tiles are ignored.
- Selected / move / attack / skill highlights can still temporarily cover the baseline.
- `originalColors` still stores only the original map color.

## Unmodified Systems
No scene, prefab, SkillData asset, material asset, KayKit asset, combat system, skill formula, turn free-order rule, End Turn rule, EnemyTurn logic, camera control, or ActiveUnitProvider behavior was changed.

## Unity Play Mode Test Steps
1. Open `Assets/_BoneThrone/Scenes/GridTest.unity`.
2. Enter Play Mode at the problematic resolution.
3. Confirm all eight SkillBar buttons are visible in two rows.
4. Confirm Skill 0/1/2 locked, cooldown, empty, and ready states still enable/disable correctly.
5. Confirm Defend, Potion, and End remain visible and clickable when valid.
6. Select a player and click End Turn.
7. Confirm the player's foot tile immediately becomes grey without waiting for auto refresh.
8. Confirm other not-ended alive players remain white.
9. Start a new PlayerTurn after EnemyTurn and confirm alive player foot tiles return to white.

## Risk and Rollback
Risk is limited to runtime HUD layout and debug tile repaint timing. To roll back the UI hotfix, restore `BattleHUDController.CreateSkillBar()` to the previous horizontal layout and restore the longer labels in `SkillBarView`. To roll back the tile hotfix, restore `MovementDebugHighlighter.Clear()` to clearing selected/action highlights without refreshing player foot tiles.
