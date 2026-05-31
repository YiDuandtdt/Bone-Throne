# Phase 14.12 - Skill SO Cleanup

Date: 2026-05-28

## Goal

Phase 14.12 audits the current four Slot 0 `SkillData` ScriptableObject assets and the four Player prefab `SkillRuntime` Slot 0 references.

This phase is documentation-only because the audit found no required asset or prefab correction.

## SkillData assets scanned

The current Slot 0 SkillData assets are:

- `Assets/_BoneThrone/Data/OnlyTest/Skills/fighter_shield_bash.asset`
- `Assets/_BoneThrone/Data/OnlyTest/Skills/ranger_precision_shot.asset`
- `Assets/_BoneThrone/Data/OnlyTest/Skills/mage_fireball.asset`
- `Assets/_BoneThrone/Data/OnlyTest/Skills/barbarian_heavy_cleave.asset`

## SkillData field audit

`targetType: 0` maps to `SkillTargetType.Enemy`.

| Asset | id | displayName | unlockLevel | range | cooldown | guaranteedDamage | targetType |
| --- | ---: | --- | ---: | ---: | ---: | ---: | --- |
| `fighter_shield_bash.asset` | 1 | `fighter_shield_bash` | 1 | 1 | 2 | 3 | Enemy |
| `ranger_precision_shot.asset` | 2 | `ranger_precision_shot` | 1 | 4 | 2 | 3 | Enemy |
| `barbarian_heavy_cleave.asset` | 3 | `barbarian_heavy_cleave` | 1 | 1 | 2 | 3 | Enemy |
| `mage_fireball.asset` | 4 | `mage_fireball` | 1 | 3 | 2 | 3 | Enemy |

## Player prefab Slot 0 reference audit

| Player prefab | Slot 0 expected skill | Result |
| --- | --- | --- |
| `Assets/_BoneThrone/Prefabs/Units/Players/Fighter.prefab` | `fighter_shield_bash` | Pass |
| `Assets/_BoneThrone/Prefabs/Units/Players/Ranger.prefab` | `ranger_precision_shot` | Pass |
| `Assets/_BoneThrone/Prefabs/Units/Players/Mage.prefab` | `mage_fireball` | Pass |
| `Assets/_BoneThrone/Prefabs/Units/Players/Barbarian.prefab` | `barbarian_heavy_cleave` | Pass |

Slot 1 and Slot 2 are currently empty on the four Player prefabs. This matches the current project state: only Slot 0 representative skills are implemented.

## Findings

No required fixes were found.

Not found:

- Missing `SkillData`.
- Missing Slot 0 Player prefab reference.
- Incorrect Slot 0 Player prefab reference.
- Duplicate skill id.
- Obvious empty field.
- Nonsensical numeric value.

## Files intentionally not modified

This phase did not modify:

- `SkillData` assets.
- Player prefabs.
- Enemy prefabs.
- Scene files.
- C# gameplay scripts.
- `DamageResolver`.
- `SkillEffectExecutor`.
- `MageSkillEffects`.
- `SkillSystem`.
- `CombatSystem`.
- Phase 14.10 camera controls.
- Phase 14.11 `ActiveUnitProvider`.
- KayKit original resources.

## Display name note

The current `displayName` values intentionally remain in machine id style:

- `fighter_shield_bash`
- `ranger_precision_shot`
- `mage_fireball`
- `barbarian_heavy_cleave`

They are not changed to UI-friendly names in this phase because the current `SkillEffectExecutor` matching path safely recognizes these identifiers. Renaming display names would be a behavior-risking asset change and is not needed for Phase 14.12.

## Unity manual validation steps

1. Open Unity 6.3 LTS.
2. Confirm there are no compile errors.
3. Inspect the four SkillData assets listed above.
4. Confirm each field matches the audit table.
5. Inspect the four Player prefabs.
6. Confirm each `SkillRuntime` Slot 0 reference matches the expected skill.
7. Confirm Slot 1 and Slot 2 remain empty.
8. Open `Assets/_BoneThrone/Scenes/GridTest.unity`.
9. Enter Play Mode.
10. Test Fighter Slot 0.
11. Test Ranger Slot 0.
12. Test Mage Slot 0 Fireball and splash.
13. Test Barbarian Slot 0.
14. Confirm CombatLog structured feedback still works.
15. Confirm Phase 14.11 provider/fallback target preview still works.
16. Confirm Phase 14.10 camera controls still work.
17. Confirm Console has no new red errors.

## Risks

- Changing `displayName` to UI-friendly names could affect current skill matching unless the matching path is reviewed separately.
- Changing skill numeric fields would change the current regression baseline.
- Changing Player prefab Slot 0 references would directly affect GridTest skill behavior.

Because no required fixes were found, this phase intentionally avoids all asset and prefab edits.

## Rollback

To roll back this phase, remove:

- `Docs/DevLogs/Phase14.12_SkillSOCleanup.md`

No asset, prefab, C# script, scene, or KayKit rollback is needed because none were modified.

## Recommended next phase

Proceed to:

- Phase 14.13 - Final Regression After Functional Changes
