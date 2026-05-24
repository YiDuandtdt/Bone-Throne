# Phase 11 DevLog - Skill Framework

## Date
2026-05-24

## Branch
phase/11-skill-framework

## Unity version
Unity 6.3 LTS

## Goal
Implement the minimum single-player skill framework for skill data, runtime skill state, level-based unlocks, target validation, guaranteed-hit execution, and cooldown handling.

## Files changed
- Assets/_BoneThrone/Scripts/Skills/SkillData.cs
- Assets/_BoneThrone/Scripts/Skills/SkillRuntime.cs
- Assets/_BoneThrone/Scripts/Skills/SkillTargetingService.cs
- Assets/_BoneThrone/Scripts/Skills/SkillSystem.cs
- Assets/_BoneThrone/Scripts/Tests/Phase11SkillTester.cs

## Implementation summary
- Added SkillData as a ScriptableObject-based skill definition.
- Added SkillRuntime for unit skill slots and cooldown state.
- Added SkillTargetingService for unlock, cooldown, target type, alive state, tile, and range validation.
- Added SkillSystem for guaranteed-hit skill execution and cooldown start.
- Added Phase11SkillTester for manual ContextMenu validation.
- Kept cooldown logic inside SkillRuntime.
- Did not add SkillCooldownService.
- Did not implement full role-specific skills.
- Did not add mana, skill trees, buffs, debuffs, VFX, networking, or formal skill UI.

## Unity test result
- Compile: Pass
- Play Mode: Pass
- Scene: local Phase 11 test setup / current gameplay scene

## Passed tests
- [x] Project opens without compile errors.
- [x] Test SkillData can be created locally.
- [x] SkillRuntime can hold skill slots.
- [x] Level 1 unit can use level 1 unlocked skill.
- [x] Higher-level skill is rejected before the unit reaches the required level.
- [x] Phase 10 level-up allows higher-level skill unlock testing.
- [x] Skill range validation works.
- [x] Invalid targets are rejected.
- [x] Valid skill executes as guaranteed-hit damage.
- [x] Skill enters cooldown after use.
- [x] Skill cannot be reused while on cooldown.
- [x] Cooldown can tick down through the tester.
- [x] Existing D20 basic attack still works.
- [x] Existing movement still works.
- [x] Existing key, stairs, level progression, and level-up still work.

## Known issues
- Cooldown is not automatically connected to full turn-end flow yet; it can be ticked manually through the tester or SkillSystem method.
- Ally and Self target types still use the same minimal guaranteed-damage execution path in Phase 11.
- Formal skill bar UI is deferred to Phase 13.
- Full Fighter / Ranger / Mage / Barbarian skill effects are deferred to Phase 12.
- Temporary SkillData assets used for testing were not committed.

## Next phase notes
- Phase 12 should implement the four characters' actual skill effects using this framework.
- Keep Phase 11 framework stable.
- Do not import full art assets until phase/12.5-art-asset-import.