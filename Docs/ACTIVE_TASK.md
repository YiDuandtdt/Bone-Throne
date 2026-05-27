# ACTIVE_TASK.md

## Current phase
Phase 14.6 - Minimal Stabilization Implementation Proposal Only

## Goal
Create a documentation-only proposal for minimal stabilization implementation tasks based on Phase 14.4 stabilization plan and Phase 14.5 Inspector / regression checklists.

This phase must not implement fixes.

The proposal must:
- identify the smallest safe stabilization candidates
- rank them by risk and value
- define exact allowed files for each future implementation candidate
- define required Play Mode reproduction before implementation
- define rollback strategy for each candidate
- keep all actual code changes deferred to separately approved future phases

## Allowed files
- Docs/Phase14_MinimalStabilizationImplementationProposal.md
- Docs/DevLogs/Phase14.6_MinimalStabilizationProposal.md
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
Create a proposal document that includes:
1. Stabilization candidates.
2. Priority ranking.
3. Why each candidate matters.
4. Whether each candidate requires Play Mode reproduction first.
5. Exact future allowed files for each candidate.
6. Exact future forbidden files for each candidate.
7. Expected test steps.
8. Rollback method.
9. Whether the candidate should be implemented, deferred, or rejected.
10. A recommended order for future implementation phases.

Candidate areas must include:
- Inspector binding validation helper or documentation-only workflow.
- Enemy AI turn gate issue.
- Skill cooldown tick flow.
- Fireball splash knownUnits dependency.
- UI target list / enemyUnits dependency.
- Enemy Floating HP Bar robustness.
- Room activation assignedEnemies / spawnTiles validation.
- Key / Stairs / LevelUp placeholder boundary.

## Validation
Documentation-only phase.

Manual checks:
1. git diff only shows Docs changes.
2. No Assets, Packages, ProjectSettings, Library, Temp, Obj, Logs, or UserSettings changes.
3. No code implementation is included.
4. Proposal does not authorize implementation without a later explicit user approval.