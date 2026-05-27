# ACTIVE_TASK.md

## Current phase
Phase 14.4 - Stabilization Plan Only

## Goal
Create a stabilization plan for the current Bone Throne Unity 6.3 LTS project based on Phase 14.1 audit, v2.2 current-state system design document, and v1.2 current-state Vibecoding document.

This phase must only decide what should be stabilized next. It must not implement code changes.

The plan must classify issues into:
- must fix before future feature work
- should fix soon
- can defer
- do not touch unless explicitly approved
- requires Unity Play Mode verification before changing

## Allowed files
- Docs/Phase14_StabilizationPlan.md
- Docs/DevLogs/Phase14.4_StabilizationPlan.md
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
Create a documentation-only stabilization plan that covers:
1. Current highest-risk systems.
2. Manual Inspector dependency risks.
3. Combat / Skill / UI safety risks.
4. Enemy AI and turn gate risks.
5. Room / Key / Stairs / LevelUp risks.
6. Fireball splash and knownUnits dependency.
7. Enemy HP bar regression risk.
8. GridTest.unity as the only integration scene.
9. Recommended next phase order.
10. Which fixes require explicit approval before implementation.

## Validation
Documentation-only phase.

Manual checks:
1. git diff only shows Docs changes.
2. No Assets, Packages, ProjectSettings, Library, Temp, Obj, Logs, or UserSettings changes.
3. No code implementation is included.
4. Plan does not ask Codex to rewrite completed systems.