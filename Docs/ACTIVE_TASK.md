# ACTIVE_TASK.md

## Current phase
Phase 14.13-A - SkillData Assets and Player Prefab Slot Wiring

## Goal
Add Slot 1 and Slot 2 SkillData assets for the four player roles and wire them into the existing Player prefabs.

This phase must only create the missing SkillData assets and update Player prefab SkillRuntime slot references.

It must not modify gameplay code, UI code, combat code, skill execution code, or skill formulas.

## Allowed files
- Assets/_BoneThrone/Data/OnlyTest/Skills/*.asset
- Assets/_BoneThrone/Data/OnlyTest/Skills/*.asset.meta
- Assets/_BoneThrone/Prefabs/Units/Players/Fighter.prefab
- Assets/_BoneThrone/Prefabs/Units/Players/Ranger.prefab
- Assets/_BoneThrone/Prefabs/Units/Players/Mage.prefab
- Assets/_BoneThrone/Prefabs/Units/Players/Barbarian.prefab
- Docs/DevLogs/Phase14.13A_SkillAssetsAndPrefabSlots.md
- Docs/ACTIVE_TASK.md

## Forbidden changes
- Do not modify C# scripts.
- Do not modify SkillEffectExecutor.
- Do not modify FighterSkillEffects.
- Do not modify RangerSkillEffects.
- Do not modify MageSkillEffects.
- Do not modify BarbarianSkillEffects.
- Do not modify SkillSystem.
- Do not modify SkillTargetingService.
- Do not modify DamageResolver.
- Do not modify CombatSystem.
- Do not modify UI scripts.
- Do not modify enemy prefabs.
- Do not modify KayKit original assets.
- Do not rename Skeleton_Rogue.
- Do not use Skeleton_Golem as a normal enemy.
- Do not change Ranger visual back to Adventurers Ranger.
- Do not modify camera controls.
- Do not modify ActiveUnitProvider behavior.

## Required SkillData assets
Create the following 8 assets in Assets/_BoneThrone/Data/OnlyTest/Skills/:

Fighter:
- fighter_guard_strike.asset
- fighter_crushing_challenge.asset

Ranger:
- ranger_quick_shot.asset
- ranger_piercing_arrow.asset

Mage:
- mage_frost_bolt.asset
- mage_arcane_burst.asset

Barbarian:
- barbarian_rage_strike.asset
- barbarian_blood_fury_slash.asset

## Required SkillData values
Use Enemy target type for all new skills.

- fighter_guard_strike: id 5, displayName fighter_guard_strike, unlockLevel 2, range 1, cooldown 1, guaranteedDamage 2
- fighter_crushing_challenge: id 6, displayName fighter_crushing_challenge, unlockLevel 3, range 1, cooldown 3, guaranteedDamage 4
- ranger_quick_shot: id 7, displayName ranger_quick_shot, unlockLevel 2, range 4, cooldown 1, guaranteedDamage 2
- ranger_piercing_arrow: id 8, displayName ranger_piercing_arrow, unlockLevel 3, range 5, cooldown 3, guaranteedDamage 3
- mage_frost_bolt: id 9, displayName mage_frost_bolt, unlockLevel 2, range 3, cooldown 1, guaranteedDamage 2
- mage_arcane_burst: id 10, displayName mage_arcane_burst, unlockLevel 3, range 3, cooldown 3, guaranteedDamage 3
- barbarian_rage_strike: id 11, displayName barbarian_rage_strike, unlockLevel 2, range 1, cooldown 1, guaranteedDamage 3
- barbarian_blood_fury_slash: id 12, displayName barbarian_blood_fury_slash, unlockLevel 3, range 1, cooldown 3, guaranteedDamage 4

## Required Player prefab wiring
- Fighter Slot 1 -> fighter_guard_strike
- Fighter Slot 2 -> fighter_crushing_challenge
- Ranger Slot 1 -> ranger_quick_shot
- Ranger Slot 2 -> ranger_piercing_arrow
- Mage Slot 1 -> mage_frost_bolt
- Mage Slot 2 -> mage_arcane_burst
- Barbarian Slot 1 -> barbarian_rage_strike
- Barbarian Slot 2 -> barbarian_blood_fury_slash

## Validation
Manual Unity checks:
1. Open Unity and wait for import.
2. Confirm 8 new SkillData assets exist and have .meta files.
3. Inspect each asset field.
4. Inspect four Player prefabs and confirm Slot 1 / Slot 2 references.
5. Confirm no C# files changed.
6. Confirm no compile errors.