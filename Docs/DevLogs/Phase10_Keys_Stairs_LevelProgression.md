# Phase 10 DevLog - Keys, Stairs, Level Switching and Level Up Progression

## Date
2026-05-24

## Branch
phase/10-keys-stairs-levels

## Unity version
Unity 6.3 LTS

## Goal
Implement the minimum single-player progression loop for shared key collection, stairs interaction, placeholder next-floor transition, and automatic party level-up.

## Files changed
- Assets/_BoneThrone/Scripts/Units/UnitStats.cs
- Assets/_BoneThrone/Scripts/Levels/LevelManager.cs
- Assets/_BoneThrone/Scripts/Levels/LevelProgressionService.cs
- Assets/_BoneThrone/Scripts/Interactables/KeyItem.cs
- Assets/_BoneThrone/Scripts/Interactables/InteractableStairs.cs
- Assets/_BoneThrone/Scripts/Tests/Phase10ProgressionTester.cs

## Implementation summary
- Added minimal unit level-up support to UnitStats.
- Added shared key progression state.
- Added placeholder next-floor progression logic.
- Added stairs interaction with hover feedback and first-click / second-click confirmation.
- Added key pickup logic.
- Added Phase 10 manual tester for local validation.
- Kept the implementation limited to single-player progression logic.
- Did not add networking, inventory, equipment, Boss door logic, complex UI, or large scene changes.

## Unity test result
- Compile: Pass
- Play Mode: Pass
- Scene: local Phase 10 test scene / current gameplay scene

## Passed tests
- [x] Project opens without compile errors.
- [x] Key can be collected.
- [x] Shared key state is recorded.
- [x] Stairs can detect interaction.
- [x] Stairs confirmation uses first click pending / second click confirm.
- [x] Progression is blocked before key collection.
- [x] Next-floor placeholder transition can be triggered after conditions are met.
- [x] Current level index increases after transition.
- [x] Living player units level up.
- [x] MaxHP increases by 2 on level-up.
- [x] Current HP refills after level-up.
- [x] Level does not exceed maxLevel 3.
- [x] Phase 9 room/shadow/enemy activation behavior still works.

## Known issues
- Formal uGUI confirmation panel is not implemented yet; this is deferred to Phase 13 UI and feedback.
- Full multi-level scene loading is not implemented yet; LevelManager currently supports placeholder root switching or index-only progression.
- Full art asset import is deferred to phase/12.5-art-asset-import.

## Next phase notes
- Phase 11 should implement the skill framework.
- Keep Phase 10 progression logic stable.
- Do not mix full art import into Phase 11 or Phase 12.