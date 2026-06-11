# Phase 07 DevLog - D20 Basic Attack Combat

## Date
2026-05-23

## Branch
phase/07-d20-basic-combat

## Unity version
Unity 6000.3.10f1 / Unity 6.3 LTS

## Goal
Implement the minimal D20 basic attack combat loop:
range validation, D20 roll, hit/miss resolution, base damage, death release, and acted-state marking.

## Files changed
- Assets/_BoneThrone/Scripts/Combat/D20Roller.cs
- Assets/_BoneThrone/Scripts/Combat/CombatLog.cs
- Assets/_BoneThrone/Scripts/Combat/AttackRangeService.cs
- Assets/_BoneThrone/Scripts/Combat/DamageResolver.cs
- Assets/_BoneThrone/Scripts/Combat/CombatSystem.cs
- Assets/_BoneThrone/Scripts/Tests/CombatInputTester.cs

## Unity test result
- Compile: Pass
- Play Mode: Pass
- Scene: Assets/_BoneThrone/Scenes/GridTest.unity

## Passed tests
- [x] Attacker can attack target within basic attack range.
- [x] Out-of-range attack is rejected.
- [x] D20 roll is logged.
- [x] Hit is resolved by D20 + attackModifier >= target defense.
- [x] On hit, target HP decreases by attacker baseDamage.
- [x] On miss, target HP does not decrease.
- [x] Valid attack attempt marks attacker HasActed.
- [x] Miss also marks attacker HasActed.
- [x] Invalid attack does not mark attacker HasActed.
- [x] Target death calls existing Unit death/release logic.
- [x] Dead target releases Tile occupancy.
- [x] A Unit that has already acted cannot attack again when turn permission is bound.
- [x] No skills, cooldowns, enemy AI, rooms, UI HUD, networking, or NetworkManager was implemented.

## Notes
Combat is currently tested through CombatInputTester ContextMenu. D20 rolling is centralized in D20Roller for future Host-authoritative networking, but no networking is implemented in this phase.

## Known issues
- Combat has no formal UI yet.
- CombatInputTester is temporary and only used for Play Mode validation.
- Attack range currently uses simple Manhattan distance.
- No line of sight, AOE, skills, buffs, critical hits, advantage/disadvantage, or enemy AI are implemented.

## Next phase notes
Phase 8 should introduce basic enemy AI using existing Movement, Turn, and Combat systems without adding complex behavior trees.