# ACTIVE_TASK.md

## Current phase
Phase 14.11 - Active Enemy Provider and Auto Enemy Collection

## Goal
Reduce manual enemy array dependency in GridTest by adding a lightweight active enemy provider.

The provider should help UI targeting and skill splash logic access current alive active enemies without requiring repeated manual enemyUnits / knownUnits setup.

This phase must preserve current gameplay behavior and must not rewrite combat, skill, unit, room, level, UI action, or damage systems.

## Allowed files
- Assets/_BoneThrone/Scripts/Units/ActiveUnitProvider.cs
- Assets/_BoneThrone/Scripts/UI/BattleHUDController.cs, only if needed to read active enemies from provider
- Assets/_BoneThrone/Scripts/UI/UIActionModeController.cs, only if needed to read active enemies from provider without changing action execution semantics
- Assets/_BoneThrone/Scripts/Skills/SkillEffectExecutor.cs, only if needed to read known units from provider without changing skill formulas
- Assets/_BoneThrone/Scenes/GridTest.unity, only if needed to add/configure provider
- Docs/DevLogs/Phase14.11_ActiveEnemyProvider.md
- Docs/ACTIVE_TASK.md

## Forbidden changes
- Do not modify DamageResolver.
- Do not change skill damage formulas.
- Do not change SkillEffectExecutor formula behavior.
- Do not change CombatSystem.TryBasicAttack execution semantics.
- Do not change SkillSystem.TryUseSkill execution semantics.
- Do not make UI directly modify HP, cooldown, acted, or moved state.
- Do not call TryBasicAttack or TryUseSkill for highlight.
- Do not modify player prefabs.
- Do not modify enemy prefabs unless explicitly required and approved.
- Do not modify SkillData assets.
- Do not modify KayKit original assets.
- Do not rename Skeleton_Rogue.
- Do not use Skeleton_Golem as a normal enemy.
- Do not change Ranger visual back to Adventurers Ranger.

## Required behavior
1. Active enemy collection:
   - Collect alive active enemy units in the current scene.
   - Ignore dead units.
   - Ignore inactive units.
   - Ignore player units.
   - Provide a read-only list or method for consumers.

2. UI compatibility:
   - Basic Attack target highlight still works.
   - Skill target highlight still works.
   - UI still does not execute gameplay during highlight.

3. Skill compatibility:
   - Mage Fireball splash still works.
   - Skill formulas must not change.
   - SkillEffectResult structured feedback must remain.

4. Backward compatibility:
   - Existing manually assigned arrays should remain usable as fallback.
   - Current GridTest setup must still pass regression.

## Validation
Manual Unity Play Mode tests:
1. Open GridTest.unity.
2. Confirm no compile errors.
3. Enter Play Mode.
4. Basic Attack highlight includes active enemies.
5. Skill Slot 0 highlight includes active enemies.
6. Mage Fireball splash still hits adjacent valid enemy.
7. Dead enemies are not treated as valid active targets.
8. Room-triggered enemies become available after activation.
9. CombatLog structured entries still work.
10. Enemy HP Bar still works.
11. Room / Key / Stairs / LevelUp still work.
12. Camera middle drag, wheel zoom, and right drag rotation still work.
13. Console has no new red errors.