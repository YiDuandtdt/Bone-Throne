# ACTIVE_TASK.md

## Current phase
Phase 14.12 - Skill SO Cleanup

## Goal
Audit and clean up the current Slot 0 SkillData ScriptableObject assets and their prefab references.

This phase must ensure the four current representative skills are correctly named, referenced, and documented without changing gameplay formulas or expanding the skill system.

Current Slot 0 skills:
- Fighter: Shield Bash
- Ranger: Precision Shot
- Mage: Fireball
- Barbarian: Heavy Cleave

## Allowed files
- Assets/_BoneThrone/Data/OnlyTest/Skills/*.asset, only for correcting obvious SkillData field errors
- Assets/_BoneThrone/Prefabs/Units/Players/*.prefab, only if Slot 0 SkillRuntime references are missing or incorrect
- Docs/DevLogs/Phase14.12_SkillSOCleanup.md
- Docs/ACTIVE_TASK.md

## Forbidden changes
- Do not modify DamageResolver.
- Do not modify SkillEffectExecutor formulas.
- Do not modify MageSkillEffects formulas.
- Do not modify SkillSystem execution semantics.
- Do not modify CombatSystem.
- Do not change skill damage formulas unless explicitly approved.
- Do not create Skill Slot 1 / Slot 2 assets in this phase.
- Do not implement new skills.
- Do not modify enemy prefabs.
- Do not modify KayKit original assets.
- Do not rename Skeleton_Rogue.
- Do not use Skeleton_Golem as a normal enemy.
- Do not change Ranger visual back to Adventurers Ranger.
- Do not modify Phase 14.10 camera controls.
- Do not modify Phase 14.11 ActiveUnitProvider behavior.

## Required behavior
1. SkillData audit:
   - Verify the four current Slot 0 SkillData assets exist.
   - Verify each has clear displayName.
   - Verify each has expected role usage.
   - Verify unlockLevel is appropriate for Slot 0.
   - Verify range / cooldown / guaranteedDamage are not accidentally empty or nonsensical.

2. Prefab reference audit:
   - Verify Fighter prefab Slot 0 references Fighter Shield Bash.
   - Verify Ranger prefab Slot 0 references Ranger Precision Shot.
   - Verify Mage prefab Slot 0 references Mage Fireball.
   - Verify Barbarian prefab Slot 0 references Barbarian Heavy Cleave.

3. Safety:
   - Preserve formulas.
   - Preserve SkillEffectResult.
   - Preserve Fireball splash behavior.
   - Preserve ActiveUnitProvider provider/fallback behavior.
   - Preserve current regression-passing behavior.

## Validation
Manual Unity checks:
1. Open Unity and confirm no compile errors.
2. Inspect four SkillData assets.
3. Inspect four Player prefabs.
4. Enter GridTest Play Mode.
5. Test Fighter Slot 0.
6. Test Ranger Slot 0.
7. Test Mage Fireball Slot 0 and splash.
8. Test Barbarian Slot 0.
9. Confirm CombatLog structured feedback still works.
10. Confirm Console has no new red errors.