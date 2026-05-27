# ACTIVE_TASK.md

## Current phase
Phase 14.8 - Phase 14 Final Handover and Closure

## Goal
Create a final Phase 14 handover and closure document for the current Bone Throne Unity 6.3 LTS project.

This phase must summarize:
- Phase 14.1 project audit
- Phase 14.2 current-state system design update
- Phase 14.3 current-state Vibecoding document update
- Phase 14.4 stabilization plan
- Phase 14.5 Inspector and regression checklists
- Phase 14.6 minimal stabilization implementation proposal
- Phase 14.7 pre-feature regression audit results

This phase must confirm:
- all required Phase 14.7 regression tests passed
- no immediate stabilization implementation is currently required
- future implementation candidates remain deferred unless new evidence appears
- the project is ready to leave documentation/stabilization phase and choose the next feature phase separately

## Allowed files
- Docs/Phase14_FinalHandoverAndClosure.md
- Docs/DevLogs/Phase14.8_FinalHandoverAndClosure.md
- Docs/ACTIVE_TASK.md

## Forbidden changes
- Do not modify gameplay code.
- Do not modify scenes.
- Do not modify prefabs.
- Do not modify ScriptableObject assets.
- Do not modify Packages or ProjectSettings.
- Do not modify KayKit original assets.
- Do not implement fixes in this phase.
- Do not edit DamageResolver.
- Do not edit SkillEffectExecutor.
- Do not edit CombatSystem.TryBasicAttack.
- Do not edit SkillSystem.TryUseSkill.
- Do not rename Skeleton_Rogue.
- Do not use Skeleton_Golem as a normal enemy.
- Do not change Ranger visual back to Adventurers Ranger.

## Required output
Create a final Phase 14 handover document covering:
1. Current project state.
2. Phase 14.1-14.7 summary.
3. Documents created in Phase 14.
4. Regression audit result.
5. Current safe baseline.
6. Deferred stabilization candidates.
7. Do-not-touch rules.
8. Recommended next feature phase options.
9. Git / branch closing steps.
10. Instructions for opening a new conversation.

## Validation
Documentation-only phase.

Manual checks:
1. git diff only shows Docs changes.
2. No Assets, Packages, ProjectSettings, Library, Temp, Obj, Logs, or UserSettings changes.
3. No code implementation is included.