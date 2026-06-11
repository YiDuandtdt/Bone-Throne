# Phase 14.19-D - Revert SkillBar Layout Hotfix

## Goal
Fix the remaining SkillBar overflow by reverting the Phase 14.19-C two-row `GridLayoutGroup` runtime layout while preserving the working player foot tile white / ended grey behavior.

## Actual Modified Files
- `Assets/_BoneThrone/Scripts/UI/BattleHUDController.cs`
- `Docs/DevLogs/Phase14.19D_RevertSkillBarLayoutHotfix.md`
- `Docs/ACTIVE_TASK.md`

`SkillBarView.cs` was reviewed but did not need another change in this phase.

## UI Overflow Cause
The Phase 14.19-C runtime SkillBar switched to a fixed 2 row x 4 column `GridLayoutGroup` with a fixed panel height. In Play Mode this still overflowed in the user's setup, especially vertically and around the panel bounds.

## GridLayoutGroup Reverted
The runtime SkillBar no longer uses `GridLayoutGroup`. It is back to a simple single-row `HorizontalLayoutGroup`.

## Final SkillBar Layout
Single row, bottom anchored, stretch-width panel:

`Move | Attack | Skill 0 | Skill 1 | Skill 2 | Defend | Potion | End`

The panel is anchored from left to right at the bottom with a small horizontal margin via `sizeDelta.x = -36`, rather than using a large fixed width. Buttons use flexible width with a small minimum width, reduced spacing, and 13 point label text.

## Preserved Tile Fix
The Phase 14.19-C tile fix remains untouched: `MovementDebugHighlighter.Clear()` still refreshes player foot tile baselines immediately after End Turn, so ended players repaint grey without waiting for auto refresh.

## Unmodified Systems
No `MovementDebugHighlighter.cs`, scene, prefab, SkillData asset, combat system, skill formula, turn rule, End Turn rule, EnemyTurn logic, Defend/Potion logic, camera control, or ActiveUnitProvider behavior was changed.

## Unity Play Mode Test Steps
1. Open `Assets/_BoneThrone/Scenes/GridTest.unity`.
2. Enter Play Mode at the problematic resolution.
3. Confirm the SkillBar is a single bottom row and all eight buttons remain visible.
4. Confirm no SkillBar panel or button spills beyond screen edges.
5. Confirm Skill 0/1/2 Empty, Locked, Cooldown, and Ready states still enable/disable correctly.
6. Confirm Move, Attack, Defend, Potion, and End still fire their existing actions.
7. End a player turn and confirm the player foot tile still immediately turns grey.

## Risk and Rollback
Risk is limited to runtime HUD sizing. If the single row is still too cramped at very narrow resolutions, reduce spacing/font size further or tune the bottom margin. Rollback is limited to `BattleHUDController.CreateSkillBar()`.
