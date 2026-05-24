# ACTIVE_TASK.md

## Current phase
Phase 11 - Skill Framework

## Goal
Implement the minimum single-player skill framework for selecting, validating, targeting, executing, and cooling down skills.

This phase must make the following behavior testable:
1. A skill can be represented as data.
2. A unit can have runtime skill state.
3. A skill can be locked or unlocked based on unit level.
4. A skill can have a range.
5. A skill can be selected.
6. A target can be validated.
7. A skill can be executed as a guaranteed-hit action.
8. A skill can enter cooldown after use.
9. Cooldowns can tick down through turns or a test method.
10. Existing movement, turn, D20 combat, room, key, and stairs progression systems remain stable.

## Authoritative documents
- Docs/骸骨王座_系统设计文档_Unity6.3LTS_v2.1.docx
- Docs/骸骨王座_Codex完整即用Vibecoding开发文档_Unity6.3LTS_v1.1.docx
- Docs/骸骨王座_Codex从零开发使用Guide_Unity6.3LTS_v1.1.docx

## Current project state
Phase 10 has been completed and merged into dev.

Already completed:
- Grid and coordinate system
- Unit system and tile occupancy
- Selection, BFS range, and A* movement
- Turn system
- D20 basic attack combat
- Basic enemy AI
- Room progression and semi-transparent room shadow
- Key collection, stairs interaction, placeholder level progression, and unit level-up

## Expected implementation files
Codex should propose the exact files after scanning the repository, but the expected scope is around:
- SkillData.cs
- SkillRuntime.cs
- SkillSystem.cs
- SkillTargetingService.cs
- SkillCooldownService.cs
- A small Phase 11 tester if needed

## Recommended files to inspect
- Assets/_BoneThrone/Scripts/Core/
- Assets/_BoneThrone/Scripts/Grid/
- Assets/_BoneThrone/Scripts/Units/
- Assets/_BoneThrone/Scripts/Movement/
- Assets/_BoneThrone/Scripts/Turns/
- Assets/_BoneThrone/Scripts/Combat/
- Assets/_BoneThrone/Scripts/Levels/
- Assets/_BoneThrone/Scripts/Interactables/
- Assets/_BoneThrone/Scripts/Skills/
- Assets/_BoneThrone/Scripts/Tests/

## Allowed changes
- Add or modify scripts directly required for the skill framework.
- Add a minimal SkillData representation.
- Add runtime cooldown/unlock state.
- Add simple target validation.
- Add guaranteed-hit skill execution.
- Add minimal hooks for unit level-based unlocking.
- Add a small tester script if needed.
- Add a DevLog after implementation and Unity validation.

## Forbidden changes
- Do not implement all four characters' full skill sets.
- Do not implement complex skill trees.
- Do not implement mana / MP / energy systems.
- Do not implement complex buffs or debuffs.
- Do not implement complex status stacking.
- Do not implement advanced VFX.
- Do not redesign battle UI.
- Do not implement full skill bar UI.
- Do not implement networking.
- Do not modify NetworkManager or Networking/.
- Do not rewrite TurnManager.
- Do not rewrite CombatSystem.
- Do not rewrite Unit.
- Do not rewrite LevelManager or Phase 10 progression.
- Do not modify RoomController or RoomShadowController core logic.
- Do not import full art assets.
- Do not modify scenes unless explicitly approved.
- Do not modify Library, Temp, Obj, Logs, UserSettings, or generated IDE files.

## Design constraints
- Skills are guaranteed-hit.
- Skills have cooldowns.
- Skills are unlocked by unit level.
- Level 1 should allow skill slot 1.
- Level 2 should allow skill slot 2.
- Level 3 should allow skill slot 3.
- This phase should build the framework only.
- Actual role-specific skills for Fighter, Ranger, Mage, and Barbarian are deferred to Phase 12.
- Formal UI skill bar is deferred to Phase 13.

## Acceptance tests in Unity 6.3 LTS
1. Project opens without compile errors.
2. A test unit can be assigned one or more test skills.
3. A level 1 unit can only use level 1 unlocked skills.
4. After Phase 10 level-up, a level 2 unit can use a level 2 skill.
5. Skill range validation works.
6. Invalid targets are rejected with a clear warning/log.
7. A valid skill can execute and apply a simple guaranteed-hit result.
8. Skill use marks the unit as having acted if integrated with current turn state.
9. Skill cooldown starts after use.
10. Cooldown can decrease through test method or turn-end hook.
11. Existing D20 basic attack still works.
12. Existing movement still works.
13. Existing room/shadow system still works.
14. Existing key/stairs/level-up progression still works.

## Codex instruction
Codex must first scan the repository and output a plan only.

Do not write code in the first response.
Do not modify files before the plan is reviewed.