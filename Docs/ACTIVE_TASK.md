# ACTIVE_TASK.md

## Current phase
Phase 14.5 - Inspector Binding and Regression Checklist

## Goal
Create a documentation-only Inspector binding checklist and Play Mode regression checklist for the current Bone Throne Unity 6.3 LTS project.

This phase must help future testing and stabilization by documenting:
- required scene object references
- required prefab references
- required manually assigned arrays
- expected Play Mode test steps
- expected results
- failure symptoms
- likely cause if a test fails

This phase must not implement any fixes.

## Allowed files
- Docs/Phase14_InspectorBindingChecklist.md
- Docs/Phase14_RegressionChecklist.md
- Docs/DevLogs/Phase14.5_InspectorRegressionChecklist.md
- Docs/ACTIVE_TASK.md

## Forbidden changes
- Do not modify gameplay code.
- Do not modify scenes.
- Do not modify prefabs.
- Do not modify ScriptableObject assets.
- Do not modify Packages or ProjectSettings.
- Do not modify KayKit original assets.
- Do not modify DamageResolver.
- Do not modify SkillEffectExecutor.
- Do not modify CombatSystem.TryBasicAttack.
- Do not modify SkillSystem.TryUseSkill.
- Do not rename Skeleton_Rogue.
- Do not use Skeleton_Golem as a normal enemy.
- Do not change Ranger visual back to Adventurers Ranger.
- Do not implement fixes in this phase.

## Required output
Create documentation-only checklists covering:
1. Inspector binding checklist.
2. GridTest.unity scene object checklist.
3. Player unit checklist.
4. Enemy unit checklist.
5. UI / HUD checklist.
6. Combat / Skill binding checklist.
7. Room / Key / Stairs / LevelUp binding checklist.
8. Enemy Floating HP Bar checklist.
9. Play Mode regression checklist.
10. Failure symptom to likely cause mapping.

Required Inspector fields include:
- SkillEffectExecutor.knownUnits
- BattleHUDController.enemyUnits
- UIActionModeController.enemyUnits
- BattleHUDController.playerUnits
- TurnManager.playerUnits
- LevelProgressionService.playerUnits
- RoomEnemyActivator.assignedEnemies
- RoomEnemyActivator.spawnTiles
- LevelProgressionService.requiredClearedRooms
- GridManager.initialTiles
- KeyItem.progressionService
- InteractableStairs.progressionService
- InteractableStairs.selectionManager

Required regression tests include:
- Select player unit
- Move mode
- Basic Attack mode
- Skill Slot 0 mode
- Mage Fireball splash
- CombatLog entries
- Enemy Floating HP Bar refresh
- Enemy death and HP bar hide
- Room trigger
- Room shadow hide
- Enemy activation
- Room clear
- Key pickup
- Stairs interaction
- LevelUp
- Enemy AI turn

## Validation
Documentation-only phase.

Manual checks:
1. git diff only shows Docs changes.
2. No Assets, Packages, ProjectSettings, Library, Temp, Obj, Logs, or UserSettings changes.
3. No code implementation is included.
4. Checklist does not ask Codex to rewrite completed systems.