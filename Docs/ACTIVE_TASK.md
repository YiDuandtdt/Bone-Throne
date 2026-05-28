# ACTIVE_TASK.md

## Current phase
Phase 14.14 - Turn System Completion Plan

## Goal
Create a plan for completing the current turn system and combat action economy before implementing Defend, Potion, or skill rebuild work.

This phase must only produce a plan. It must not modify gameplay code.

The plan must define:
- player round / actor turn lifecycle
- enemy turn lifecycle
- move/action consumption rules
- cooldown tick timing
- how Basic Attack / Skill / Defend / Potion share the same action economy
- how Enemy AI should use or bypass ActionPermissionService safely
- which files should be modified in the later implementation phase
- which systems must not be touched

## Allowed files
- Docs/Phase14_TurnSystemCompletionPlan.md
- Docs/DevLogs/Phase14.14_TurnSystemCompletionPlan.md
- Docs/ACTIVE_TASK.md

## Forbidden changes
- Do not modify C# files.
- Do not modify SkillData assets.
- Do not modify prefabs.
- Do not modify scene files.
- Do not modify Packages or ProjectSettings.
- Do not implement fixes in this phase.
- Do not modify DamageResolver.
- Do not modify CombatSystem.
- Do not modify SkillSystem.
- Do not modify SkillEffectExecutor.
- Do not modify UI scripts.
- Do not modify enemy prefabs.
- Do not modify KayKit original assets.
- Do not rename Skeleton_Rogue.
- Do not use Skeleton_Golem as a normal enemy.
- Do not change Ranger visual back to Adventurers Ranger.

## Required output
Create a turn system completion plan covering:
1. Current turn system state.
2. Current action economy state.
3. Problems to solve.
4. Proposed turn lifecycle.
5. Proposed cooldown tick timing.
6. Proposed EnemyTurn runner.
7. Proposed ActionPermissionService expansion.
8. How Defend should later consume action.
9. How Potion should later consume action.
10. Required implementation phases.
11. Allowed future files.
12. Forbidden future files.
13. Regression checklist.
14. Risks and rollback.

## Validation
Documentation-only phase.

Manual checks:
1. git diff only shows Docs changes.
2. No Assets, Packages, ProjectSettings, Library, Temp, Obj, Logs, or UserSettings changes.
3. No code implementation is included.