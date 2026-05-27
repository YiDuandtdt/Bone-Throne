# ACTIVE_TASK.md

## Current phase
Phase 14.3 - Vibecoding Development Document Update

## Goal
Update the Bone Throne Codex Vibecoding development document so future Codex tasks continue from the current real project state instead of restarting from the original from-zero roadmap.

The updated document must:
- preserve Unity 6.3 LTS / URP / uGUI + TMP / NGO + Unity Transport long-term rules
- reflect the actual completed Phase 0-13 systems
- reflect the Phase 14.1 project audit
- reflect the Phase 14.2 current-state system design document
- rewrite future task prompts around current project stabilization
- add strict safety rules for UI, combat, skills, Skeleton/Ranger visual rules, and manual Inspector dependencies

## Allowed files
- Docs/骸骨王座_Codex完整即用Vibecoding开发文档_Unity6.3LTS_v1.2_CurrentProjectState.md
- Docs/DevLogs/Phase14.3_VibecodingDocumentUpdate.md
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
- Do not change Player Ranger from Adventurers Rogue visual back to Ranger visual.

## Required output
Create a new Markdown Vibecoding development document that:
1. Treats Phase 0-13 as already completed.
2. Treats Phase 14 as Documentation Finalization and Project Stabilization.
3. Uses the current-state system design document v2.2 as the new reference.
4. Gives Codex prompts for Phase 14.4 onward.
5. Adds prompt rules requiring Codex to read DevLogs before planning.
6. Adds regression checklist for Move / Basic Attack / Skill0 / Fireball splash / CombatLog / Enemy HP Bar / Room / Key / Stairs / LevelUp / Enemy AI.
7. Adds Inspector binding checklist for knownUnits, enemyUnits, playerUnits, assignedEnemies, spawnTiles, requiredClearedRooms, initialTiles.
8. Adds do-not-touch rules for Skeleton_Golem, Skeleton_Rogue, Ranger Rogue visual, KayKit original assets, UI safety, CombatLog, DamageResolver, SkillEffectExecutor.

## Validation
Documentation-only phase.

Manual checks:
1. git diff only shows Docs changes.
2. No Assets, Packages, ProjectSettings, Library, Temp, Obj, Logs, or UserSettings changes.
3. New document does not ask Codex to rebuild already completed systems from scratch.
4. New document clearly separates documentation phases, stabilization phases, and future feature phases.
