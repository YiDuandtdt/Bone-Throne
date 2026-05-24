# ACTIVE_TASK.md

## Current phase
Phase 12 - Four Character Skills

## Goal
Implement the first playable version of the four fixed characters' role-specific skill effects using the Phase 11 skill framework.

This phase must make the following behavior testable:
1. Fighter has role-specific skill effects.
2. Ranger has role-specific skill effects.
3. Mage has role-specific skill effects.
4. Barbarian has role-specific skill effects.
5. Skills reuse the existing SkillData / SkillRuntime / SkillSystem framework.
6. Skills remain guaranteed-hit unless explicitly defined otherwise by the existing framework.
7. Skills respect unlock level and cooldown.
8. Skill effects can be tested through manual tester or context menu.
9. Existing movement, turn, D20 basic attack, room, key, stairs, level-up, and skill framework systems remain stable.

## Authoritative documents
- Docs/骸骨王座_系统设计文档_Unity6.3LTS_v2.1.docx
- Docs/骸骨王座_Codex完整即用Vibecoding开发文档_Unity6.3LTS_v1.1.docx
- Docs/骸骨王座_Codex从零开发使用Guide_Unity6.3LTS_v1.1.docx

## Current project state
Phase 11 has been completed and merged into dev.

Already completed:
- Grid and coordinate system
- Unit system and tile occupancy
- Selection, BFS range, and A* movement
- Turn system
- D20 basic attack combat
- Basic enemy AI
- Room progression and semi-transparent room shadow
- Key collection, stairs interaction, placeholder level progression, and unit level-up
- SkillData, SkillRuntime, SkillTargetingService, SkillSystem, and Phase 11 skill testing tools

## Expected implementation files
Codex should propose the exact files after scanning the repository, but the expected scope is around:
- SkillEffectExecutor.cs
- FighterSkillEffects.cs
- RangerSkillEffects.cs
- MageSkillEffects.cs
- BarbarianSkillEffects.cs
- A small Phase 12 tester if needed

## Recommended files to inspect
- Assets/_BoneThrone/Scripts/Skills/
- Assets/_BoneThrone/Scripts/Units/
- Assets/_BoneThrone/Scripts/Grid/
- Assets/_BoneThrone/Scripts/Movement/
- Assets/_BoneThrone/Scripts/Turns/
- Assets/_BoneThrone/Scripts/Combat/
- Assets/_BoneThrone/Scripts/Levels/
- Assets/_BoneThrone/Scripts/Tests/

## Allowed changes
- Add scripts directly required for four-character skill effects.
- Extend the Phase 11 skill framework only if absolutely necessary and with minimal changes.
- Add a small Phase 12 tester if needed.
- Add simple role-specific effect execution.
- Add temporary local test SkillData assets only for manual validation, but do not commit them unless explicitly approved.
- Add a DevLog after Unity validation.

## Forbidden changes
- Do not rewrite SkillSystem.
- Do not rewrite SkillRuntime.
- Do not rewrite SkillTargetingService.
- Do not rewrite CombatSystem.
- Do not rewrite DamageResolver.
- Do not rewrite Unit.
- Do not rewrite TurnManager.
- Do not rewrite LevelManager or Phase 10 progression.
- Do not modify RoomController or RoomShadowController core logic.
- Do not implement full skill bar UI.
- Do not redesign battle HUD.
- Do not implement advanced VFX.
- Do not import full art assets.
- Do not implement networking.
- Do not modify NetworkManager or Networking/.
- Do not implement complex skill trees.
- Do not implement mana / MP / energy systems.
- Do not implement complex status stacking.
- Do not modify scenes unless explicitly approved.
- Do not modify Library, Temp, Obj, Logs, UserSettings, or generated IDE files.

## Design constraints
- Use the existing Phase 11 framework as much as possible.
- Each character should have clear role identity.
- Level 1 / 2 / 3 skills should match the existing unlock-level logic.
- If full three skills per role are too large, implement one representative skill per role first and leave the rest as simple configured damage skills or documented follow-up.
- Keep effects simple enough for a course demo.
- Prefer direct damage, simple range checks, simple self-effect, or simple cooldown behavior.
- Avoid complex buffs, chained effects, summons, terrain changes, or animation dependencies.

## Suggested first-pass skill direction
- Fighter: defensive frontliner / shield control.
- Ranger: ranged precision / mobility-oriented damage.
- Mage: area or high-impact magic damage.
- Barbarian: melee burst / rage-style damage.

## Acceptance tests in Unity 6.3 LTS
1. Project opens without compile errors.
2. Fighter skill effect can be executed.
3. Ranger skill effect can be executed.
4. Mage skill effect can be executed.
5. Barbarian skill effect can be executed.
6. Skill effects respect unlock level.
7. Skill effects respect cooldown.
8. Invalid target or range is rejected.
9. Valid target receives the intended simple effect.
10. Existing Phase 11 generic skill test still works.
11. Existing D20 basic attack still works.
12. Existing movement still works.
13. Existing room/shadow system still works.
14. Existing key/stairs/level-up progression still works.
15. No formal UI, networking, or art import is introduced.

## Deferred decision
Full art asset import is intentionally deferred.

Planned future branch:
phase/12.5-art-asset-import

Reason:
Phase 12 should finish gameplay skill effects first. Full art import should be isolated into its own branch between Phase 12 and Phase 13.

## Codex instruction
Codex must first scan the repository and output a plan only.

Do not write code in the first response.
Do not modify files before the plan is reviewed.