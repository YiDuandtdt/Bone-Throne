# ACTIVE_TASK.md

## Current phase
Phase 14.13-E - Complete Skill Set Regression

## Goal
Run and record a complete skill set regression after formal SkillData migration.

This phase must validate all 12 formal player skills after:
- Phase 14.13-A SkillData assets and prefab slot wiring
- Phase 14.13-B UI Slot 1 / Slot 2 wiring
- Phase 14.13-C SkillEffectExecutor branches
- Phase 14.13-D formal SkillData migration

This phase is documentation and manual validation only. It must not implement fixes.

## Allowed files
- Docs/Phase14_CompleteSkillSetRegression.md
- Docs/DevLogs/Phase14.13E_CompleteSkillSetRegression.md
- Docs/ACTIVE_TASK.md

## Forbidden changes
- Do not modify C# files.
- Do not modify SkillData assets.
- Do not modify player prefabs.
- Do not modify enemy prefabs.
- Do not modify scene files.
- Do not modify DamageResolver.
- Do not modify SkillEffectExecutor.
- Do not modify SkillSystem.
- Do not modify SkillTargetingService.
- Do not modify UI scripts.
- Do not modify CombatSystem.
- Do not modify KayKit original assets.
- Do not implement fixes in this phase.

## Required validation
Test all 12 formal player skills:

Fighter:
- Slot 0: fighter_shield_bash
- Slot 1: fighter_guard_strike
- Slot 2: fighter_crushing_challenge

Ranger:
- Slot 0: ranger_precision_shot
- Slot 1: ranger_quick_shot
- Slot 2: ranger_piercing_arrow

Mage:
- Slot 0: mage_fireball
- Slot 1: mage_frost_bolt
- Slot 2: mage_arcane_burst

Barbarian:
- Slot 0: barbarian_heavy_cleave
- Slot 1: barbarian_rage_strike
- Slot 2: barbarian_blood_fury_slash

Also validate:
- Skill UI Slot 0 / 1 / 2 display and click behavior
- locked / unlocked behavior by level
- cooldown behavior
- CombatLog structured entries
- Enemy HP Bar refresh and death hide
- ActiveUnitProvider target collection
- Mage Fireball splash
- Mage Arcane Burst splash
- Barbarian Blood Fury half-HP bonus
- Camera controls
- Console has no new red errors

## Required output
Create a regression report with:
1. Environment snapshot.
2. Git status before test.
3. Formal SkillData reference check.
4. Player prefab slot reference check.
5. 12-skill result table.
6. Individual notes for each skill.
7. Cross-system regression results.
8. Console errors / warnings.
9. Fail / Blocked mapping if any.
10. Final conclusion.
11. Whether Phase 14 can proceed to full final regression.