# Phase 14.19-E - Restore SkillBar UI From Last Known Good Baseline

## Goal
Restore the SkillBar UI files to the last known good UI baseline before the Phase 14.19-B/C/D working-tree layout changes, while preserving the working player foot tile white / ended grey behavior.

## Good Baseline
Git history shows the current committed baseline is:

- `1ddaa6d feat: complete Phase14.19 skill rebuild + balance + bugfix`

Phase 14.19-B/C/D were not committed; their UI changes existed only in the working tree. Therefore the good UI baseline is the UI file content from `HEAD` at `1ddaa6d`.

## Actual Modified Files
- `Assets/_BoneThrone/Scripts/UI/BattleHUDController.cs`
- `Assets/_BoneThrone/Scripts/UI/SkillBarView.cs`
- `Docs/DevLogs/Phase14.19E_RestoreSkillBarUIFromGoodBaseline.md`
- `Docs/ACTIVE_TASK.md`

`Assets/_BoneThrone/Scripts/UI/UIActionModeController.cs` had no relevant working-tree diff and was not changed.

## Restored UI Behavior
Restored `BattleHUDController.CreateSkillBar()` to the last known good runtime layout:
- Single row `HorizontalLayoutGroup`.
- Bottom centered SkillBar.
- Original `920 x 88` runtime panel size.
- Original spacing and padding.
- Original button labels: `Move`, `Basic Attack`, `Skill 0`, `Slot 1`, `Slot 2`, `Defend`, `Potion`, `End Turn`.
- Original 16 point action button label font.

Restored `SkillBarView` UI text behavior:
- Basic attack refresh text uses `Basic Attack`.
- Skill slots display the actual skill `DisplayName` plus state.

## Preserved Systems
The following remain preserved:
- Defend / Potion / End Turn event binding and handlers.
- Skill Slot Empty / Locked / Cooldown / Ready interactable and color logic.
- End Turn selected-unit flow.
- Player foot tile white / ended grey marker logic.
- End Turn immediate grey repaint from `MovementDebugHighlighter.Clear()`.
- New PlayerTurn white reset behavior.

## Unmodified Files and Systems
No `MovementDebugHighlighter.cs`, scene, prefab, SkillData asset, KayKit asset, DamageResolver, SkillEffectExecutor, SkillSystem, SkillTargetingService, CombatSystem, TurnManager, ActionPermissionService, EnemyTurnRunner, PlayerMovementController, skill formula, or turn rule was changed.

## Unity Play Mode Test Steps
1. Open `Assets/_BoneThrone/Scenes/GridTest.unity`.
2. Enter Play Mode at the resolution where UI was last known good before Phase 14.19-B.
3. Confirm the SkillBar visual layout matches the pre-14.19-B baseline.
4. Confirm all eight buttons are present: Move, Basic Attack, Skill 0, Slot 1, Slot 2, Defend, Potion, End Turn.
5. Confirm Skill Slot availability still works: Empty/Locked/Cooldown grey and not clickable, Ready clickable.
6. Confirm Defend, Potion, and End Turn still work.
7. Confirm player foot tiles still show white for not-ended alive players and grey immediately after End Turn.

## Risk and Rollback
Risk is low because the UI files were restored to the committed good baseline instead of tuned manually. If further UI problems appear, compare against `1ddaa6d` again and inspect only scene/canvas scale or runtime root layout assumptions without touching tile or gameplay systems.
