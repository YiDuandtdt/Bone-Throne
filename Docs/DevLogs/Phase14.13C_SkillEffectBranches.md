# Phase 14.13-C Skill Effect Branches

## Purpose and scope

Phase 14.13-C adds simplified class-specific effect branches for the eight Slot 1 / Slot 2 skills created in Phase 14.13-A and wired through UI in Phase 14.13-B.

This phase only updates `SkillEffectExecutor` branch handling. It does not modify SkillData assets, Player prefabs, scenes, UI scripts, combat execution, targeting validation, or damage resolution.

## Files changed

- `Assets/_BoneThrone/Scripts/Skills/SkillEffectExecutor.cs`
- `Docs/DevLogs/Phase14.13C_SkillEffectBranches.md`

## New simplified skill effects

| Role | Skill | Effect |
| --- | --- | --- |
| Fighter | `fighter_guard_strike` | Single target damage = `guaranteedDamage + 1` |
| Fighter | `fighter_crushing_challenge` | Single target damage = `guaranteedDamage + 2`; no taunt or AI control |
| Ranger | `ranger_quick_shot` | Single target damage = `guaranteedDamage` |
| Ranger | `ranger_piercing_arrow` | Single target damage = `guaranteedDamage + 1`; no line pierce |
| Mage | `mage_frost_bolt` | Single target damage = `guaranteedDamage`; no slow/status |
| Mage | `mage_arcane_burst` | Primary damage = `guaranteedDamage`; splash 1 damage to adjacent active alive enemies |
| Barbarian | `barbarian_rage_strike` | Single target damage = `guaranteedDamage + 1` |
| Barbarian | `barbarian_blood_fury_slash` | Single target damage = `guaranteedDamage + 2`; +1 if caster is at or below half HP |

## Slot 0 behavior preserved

Existing Slot 0 role effect files and formulas were not modified:

- `fighter_shield_bash`: `guaranteedDamage + 1`
- `ranger_precision_shot`: `guaranteedDamage + 2`
- `mage_fireball`: primary `guaranteedDamage` plus adjacent splash 1
- `barbarian_heavy_cleave`: `guaranteedDamage + 2`, plus half-HP bonus +1

The new Phase 14.13-C branches run only after the existing role effect files fail to match. Unknown skills still fall through to the existing fallback guaranteed-damage path.

## Arcane Burst splash

`mage_arcane_burst` reuses the current `SkillEffectExecutor` unit source:

1. `ActiveUnitProvider.GetActiveAliveUnits()` when available.
2. Serialized `knownUnits` fallback when provider data is unavailable.

Splash candidates must be:

- Not null.
- Not the primary target.
- Active in hierarchy.
- Alive.
- Different faction from the caster.
- On a tile.
- Manhattan distance 1 from the primary target.

Each splash target receives 1 damage through `DamageResolver.ApplyDamage(...)` and is recorded as a non-primary `SkillEffectResult` damage entry.

Fireball splash behavior was not modified.

## Blood Fury Slash half-HP bonus

`barbarian_blood_fury_slash` checks the existing safe runtime data:

- `caster.RuntimeState.CurrentHp`
- `caster.Stats.GetClampedMaxHp()`

If either `RuntimeState` or `Stats` is missing, the bonus is not applied. The effect does not throw or create new state.

## Complex versions intentionally not implemented

- No taunt or AI control for Fighter Crushing Challenge.
- No piercing line or multi-tile projectile for Ranger Piercing Arrow.
- No slow/status system for Mage Frost Bolt.
- No buff/status system.
- No displacement.
- No mana.
- No skill tree.

## Explicit non-changes

- No `SkillData` assets were modified.
- No Player prefabs were modified.
- No enemy prefabs were modified.
- No scene files were modified.
- No UI scripts were modified.
- `SkillSystem` was not modified.
- `SkillTargetingService` was not modified.
- `DamageResolver` was not modified.
- `CombatSystem` was not modified.
- `FighterSkillEffects.cs` was not modified.
- `RangerSkillEffects.cs` was not modified.
- `MageSkillEffects.cs` was not modified.
- `BarbarianSkillEffects.cs` was not modified.
- No KayKit original assets were modified.
- Phase 14.10 camera controls were not modified.
- Phase 14.11 `ActiveUnitProvider` behavior was not modified.
- Phase 14.13-B UI slot wiring was not modified.

## Structured feedback

All new effects:

- Apply damage through `DamageResolver.ApplyDamage(...)`.
- Record damage through `SkillEffectResult.AddDamage(...)`.
- Leave CombatLog output to `SkillSystem`, which already reads structured damage entries.
- Do not parse strings.
- Do not modify cooldown or acted state.

## Play Mode test steps

1. Open `Assets/_BoneThrone/Scenes/GridTest.unity`.
2. Confirm there are no compile errors.
3. Enter Play Mode.
4. Test Fighter Slot 0 and confirm Shield Bash is unchanged.
5. Test Fighter Slot 1 Guard Strike.
6. Test Fighter Slot 2 Crushing Challenge.
7. Test Ranger Slot 0 and confirm Precision Shot is unchanged.
8. Test Ranger Slot 1 Quick Shot.
9. Test Ranger Slot 2 Piercing Arrow.
10. Test Mage Slot 0 and confirm Fireball primary/splash behavior is unchanged.
11. Test Mage Slot 1 Frost Bolt.
12. Test Mage Slot 2 Arcane Burst with adjacent active alive enemies.
13. Test Barbarian Slot 0 and confirm Heavy Cleave half-HP behavior is unchanged.
14. Test Barbarian Slot 1 Rage Strike.
15. Test Barbarian Slot 2 Blood Fury Slash above half HP and at/below half HP.
16. Confirm CombatLog receives structured skill damage, cooldown, and death feedback.
17. Confirm Enemy Floating HP Bars refresh and hide on death.
18. Confirm ActiveUnitProvider target/splash behavior still works.
19. Confirm Phase 14.13-B UI Slot 1 / Slot 2 targeting still works.
20. Confirm Phase 14.10 camera controls still work.
21. Confirm Console has no new red errors.

## Risks

- The eight new effects are implemented in `SkillEffectExecutor.cs` to respect the Phase 14.13-C file whitelist. A later cleanup phase may move them into role-specific files if approved.
- Arcane Burst splash depends on active unit provider or knownUnits data. If no candidates are available, it safely damages only the primary target.
- Blood Fury Slash half-HP bonus depends on existing `RuntimeState` and `Stats`; missing data safely disables the bonus.

## Rollback

To roll back this phase:

1. Revert `Assets/_BoneThrone/Scripts/Skills/SkillEffectExecutor.cs`.
2. Delete `Docs/DevLogs/Phase14.13C_SkillEffectBranches.md`.

No asset, prefab, scene, UI, CombatSystem, DamageResolver, SkillSystem, SkillTargetingService, role skill effect file, KayKit, camera, or ActiveUnitProvider rollback is needed because those files were not modified.

## Recommended next phase

Proceed to Phase 14.13-D - Final Regression After Four Role Skill Set, after confirming compilation and running the Play Mode test steps above.
