# ACTIVE_TASK.md

## Current phase
Phase 14.7 - Pre-Feature Regression Audit

## Goal
Perform a pre-feature regression audit for the current Bone Throne Unity 6.3 LTS project before approving any stabilization implementation or new gameplay feature work.

This phase must:
- use GridTest.unity as the only real integrated gameplay regression scene
- run the Phase 14.5 regression checklist manually in Unity Play Mode
- record pass / fail / blocked / not tested results
- record Console warnings or errors
- record Inspector binding issues
- identify which future stabilization candidates are actually supported by observed failures
- avoid implementing any fixes

## Allowed files
- Docs/Phase14_PreFeatureRegressionAudit.md
- Docs/DevLogs/Phase14.7_PreFeatureRegressionAudit.md
- Docs/ACTIVE_TASK.md

## Forbidden changes
- Do not modify gameplay code.
- Do not modify scenes unless the user explicitly approves Inspector-only binding corrections in a later phase.
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
Create a regression audit report template and result document covering:
1. Unity version and branch.
2. Git status before testing.
3. Scene tested.
4. Console status before Play Mode.
5. Inspector binding check results.
6. Regression test results from Phase14_RegressionChecklist.md.
7. Screenshots or notes needed.
8. Console errors/warnings.
9. Observed failures mapped to likely causes.
10. Stabilization candidates supported by actual observed evidence.
11. Stabilization candidates not yet supported by evidence.
12. Recommended next phase.

Required tests:
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

## Validation
Documentation and manual testing phase.

Manual checks:
1. Unity 6.3 LTS opens the project.
2. GridTest.unity opens.
3. Console state is recorded before Play Mode.
4. Play Mode results are recorded.
5. git diff only shows Docs changes.
6. No code implementation is included.