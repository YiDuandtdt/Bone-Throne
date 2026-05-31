# Phase 12 DevLog - Four Character Skills

## Date
2026-05-24

## Branch
phase/12-character-skills

## Unity version
Unity 6.3 LTS

## Goal
Implement the first playable version of the four fixed characters' role-specific skill effects using the Phase 11 skill framework.

## Files changed
- Assets/_BoneThrone/Scripts/Skills/SkillSystem.cs
- Assets/_BoneThrone/Scripts/Skills/SkillEffectExecutor.cs
- Assets/_BoneThrone/Scripts/Skills/FighterSkillEffects.cs
- Assets/_BoneThrone/Scripts/Skills/RangerSkillEffects.cs
- Assets/_BoneThrone/Scripts/Skills/MageSkillEffects.cs
- Assets/_BoneThrone/Scripts/Skills/BarbarianSkillEffects.cs

## Implementation summary
- Added SkillEffectExecutor as the Phase 12 role-skill dispatch entry.
- Connected SkillSystem to the optional SkillEffectExecutor.
- Preserved Phase 11 fallback guaranteed-damage behavior when no custom effect is matched.
- Implemented Fighter Shield Bash.
- Implemented Ranger Precision Shot.
- Implemented Mage Fireball with optional 1-tile splash damage.
- Implemented Barbarian Heavy Cleave with low-HP bonus damage.
- Kept the implementation limited to one representative skill per character.
- Did not add skill bar UI, VFX, mana, buffs, debuffs, networking, or full art assets.

## Unity test result
- Compile: Pass
- Play Mode: Pass
- Scene: local Phase 12 test setup / current gameplay scene

## Passed tests
- [x] Project opens without compile errors.
- [x] Fighter Shield Bash executes.
- [x] Ranger Precision Shot executes.
- [x] Mage Fireball executes.
- [x] Mage Fireball can splash 1 damage to an adjacent enemy when Known Units are bound.
- [x] Mage Fireball safely falls back to primary target only when Known Units are empty.
- [x] Barbarian Heavy Cleave executes.
- [x] Barbarian Heavy Cleave gains extra damage when current HP is at or below half MaxHP.
- [x] Skill cooldown still works through the Phase 11 framework.
- [x] Skill unlock level still works through the Phase 11 framework.
- [x] Phase 11 generic fallback skill behavior still works.
- [x] Existing D20 basic attack still works.
- [x] Existing movement still works.
- [x] Existing key, stairs, level progression, and level-up still work.

## Known issues
- Skill matching currently uses SkillData.DisplayName for the Phase 12 representative skills because the current SkillData.SkillId is integer-based.
- Formal role skill assets are not committed yet; temporary SkillData assets were used only for local testing.
- Only one representative skill per character is implemented in this phase.
- Full skill bar UI is deferred to Phase 13.
- Full art asset import is deferred to phase/12.5-art-asset-import.

## Next phase notes
- Phase 12.5 should isolate full art asset import into its own branch.
- Phase 13 should add UI and feedback around the existing skill framework.
- Consider replacing DisplayName matching with stable string IDs in a future data cleanup pass.