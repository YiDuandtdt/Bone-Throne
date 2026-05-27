# Phase 14.5 - Inspector Binding and Regression Checklist

Date: 2026-05-27

## Scope

Phase 14.5 created documentation-only Inspector binding and Play Mode regression checklists for the current Bone Throne Unity 6.3 LTS project.

This phase did not implement fixes.

## Files changed

- Added `Docs/Phase14_InspectorBindingChecklist.md`
- Added `Docs/Phase14_RegressionChecklist.md`
- Added `Docs/DevLogs/Phase14.5_InspectorRegressionChecklist.md`
- `Docs/ACTIVE_TASK.md` was already set to Phase 14.5 and did not require a content update.

## Source material

- `AGENTS.md`
- `Docs/ACTIVE_TASK.md`
- `Docs/Phase14_ProjectStateAudit.md`
- `Docs/Phase14_StabilizationPlan.md`
- `Docs/骸骨王座_系统设计文档_Unity6.3LTS_v2.2_CurrentProjectState.md`
- `Docs/骸骨王座_Codex完整即用Vibecoding开发文档_Unity6.3LTS_v1.2_CurrentProjectState.md`
- `Docs/DevLogs/`

## Inspector checklist coverage

The Inspector checklist covers:

- `GridManager.initialTiles`
- `BattleHUDController.playerUnits`
- `TurnManager.playerUnits`
- `LevelProgressionService.playerUnits`
- `BattleHUDController.enemyUnits`
- `UIActionModeController.enemyUnits`
- `SkillEffectExecutor.knownUnits`
- `RoomEnemyActivator.assignedEnemies`
- `RoomEnemyActivator.spawnTiles`
- `LevelProgressionService.requiredClearedRooms`
- `KeyItem.progressionService`
- `InteractableStairs.progressionService`
- `InteractableStairs.selectionManager`

It also covers:

- Enemy Floating HP Bar Unit reference, HP refresh, death hiding, raycast disabled behavior, and prefab/scene override risk.
- Visual do-not-touch checks for `Skeleton_Rogue`, `Skeleton_Golem`, Player Ranger Adventurers Rogue visual, and KayKit original resources.
- Failure symptom to likely binding cause mapping.
- Inspector-only validation workflow.

## Regression checklist coverage

The regression checklist covers:

- Select player unit
- Move mode
- Basic Attack mode
- Skill Slot 0 mode
- Mage Fireball splash
- CombatLog structured entries
- Enemy Floating HP Bar refresh
- Enemy HP Bar death hide
- Room trigger
- Room shadow hide
- Enemy activation
- Room clear
- Key pickup
- Stairs hover / second click
- LevelUp
- Enemy AI turn

Each test includes:

- Test ID
- System
- Scene
- Preconditions
- Inspector bindings to verify first
- Steps
- Expected result
- Failure symptoms
- Likely causes
- Screenshot required
- Console required
- Safety rules checked
- Phase 14.5 action allowed: document only / no fix

## Safety rules repeated

The checklist repeats these Phase 14 safety rules:

- UI must not directly subtract HP.
- UI must not directly change cooldown.
- UI must not directly call `MarkActed` or `MarkMoved`.
- Highlight preview must not call `TryBasicAttack`.
- Highlight preview must not call `TryUseSkill`.
- `DamageResolver` must not be casually modified.
- `SkillEffectExecutor` skill formulas must not be casually modified.
- `CombatLog` must not be implemented by parsing strings.
- `Skeleton_Rogue` is the ordinary enemy.
- `Skeleton_Golem` is reserved for future Boss / heavy Boss only.
- Player Ranger uses Adventurers Rogue visual.
- KayKit original resources must not be modified.

## Validation

Documentation-only validation:

- No Unity Play Mode was run.
- No gameplay code was written.
- No fixes were implemented.
- No `Assets`, `Packages`, or `ProjectSettings` changes were intentionally made.
- No scene, prefab, ScriptableObject asset, C# script, or KayKit original resource was intentionally modified.

Recommended verification commands:

```powershell
git status --short
git diff --name-only
git diff -- Docs
```

## Recommended next phase

Recommended next phase:

- Phase 14.6 - Minimal Stabilization Implementation Proposal Only

Phase 14.6 must remain proposal-only. It must not write code or implement fixes by default. Any real implementation requires a later separate phase and explicit user approval.

## Rollback

To roll back this phase only, remove:

- `Docs/Phase14_InspectorBindingChecklist.md`
- `Docs/Phase14_RegressionChecklist.md`
- `Docs/DevLogs/Phase14.5_InspectorRegressionChecklist.md`

Do not use broad reset commands if other Phase 14 documentation work is still in progress.
