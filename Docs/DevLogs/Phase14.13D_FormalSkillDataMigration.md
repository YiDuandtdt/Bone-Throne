# Phase 14.13-D Formal SkillData Migration

## Purpose and scope

Phase 14.13-D formalizes the 12 validated player `SkillData` assets by creating formal copies under:

- `Assets/_BoneThrone/Data/Skills/`

The four Player prefabs now reference these formal assets in `SkillRuntime.skillSlots`.

This phase does not change skill field values, formulas, C# code, UI behavior, scenes, enemy prefabs, or KayKit assets.

## New formal directory

- `Assets/_BoneThrone/Data/Skills/`
- `Assets/_BoneThrone/Data/Skills.meta`

## Formal SkillData assets

| Asset | GUID | skillId | displayName | unlockLevel | range | cooldownTurns | guaranteedDamage | targetType |
| --- | --- | ---: | --- | ---: | ---: | ---: | ---: | --- |
| `fighter_shield_bash.asset` | `3f93c0dcf50e4e7a98dcd7b8fd85a601` | 1 | `fighter_shield_bash` | 1 | 1 | 2 | 3 | Enemy |
| `fighter_guard_strike.asset` | `927ec2eeb7194b54a052a1f46dcd4f4a` | 5 | `fighter_guard_strike` | 2 | 1 | 1 | 2 | Enemy |
| `fighter_crushing_challenge.asset` | `c00fa874bcde4ef9ab2bbf97c3d88e3a` | 6 | `fighter_crushing_challenge` | 3 | 1 | 3 | 4 | Enemy |
| `ranger_precision_shot.asset` | `a390f0bf9eed4ac6b82f28e6b4cb1b21` | 2 | `ranger_precision_shot` | 1 | 4 | 2 | 3 | Enemy |
| `ranger_quick_shot.asset` | `5aaf28d0467743d4b2e0b0af0d767ae0` | 7 | `ranger_quick_shot` | 2 | 4 | 1 | 2 | Enemy |
| `ranger_piercing_arrow.asset` | `f25ea4b7d3904625a6e52749a7c5b5f7` | 8 | `ranger_piercing_arrow` | 3 | 5 | 3 | 3 | Enemy |
| `mage_fireball.asset` | `78c302a301734517a9e1fb3a4d77b51c` | 4 | `mage_fireball` | 1 | 3 | 2 | 3 | Enemy |
| `mage_frost_bolt.asset` | `e96c7f58e5754418a3eed2d8f05af0d7` | 9 | `mage_frost_bolt` | 2 | 3 | 1 | 2 | Enemy |
| `mage_arcane_burst.asset` | `4d76c992365a49c58c5ab45ee99b7a64` | 10 | `mage_arcane_burst` | 3 | 3 | 3 | 3 | Enemy |
| `barbarian_heavy_cleave.asset` | `97e45d4188474718b31d2082d8a040cd` | 3 | `barbarian_heavy_cleave` | 1 | 1 | 2 | 3 | Enemy |
| `barbarian_rage_strike.asset` | `aa686f57df6849a6a868701c4a1dbb45` | 11 | `barbarian_rage_strike` | 2 | 1 | 1 | 3 | Enemy |
| `barbarian_blood_fury_slash.asset` | `cd0dc7b7798d4103851d4e9146a028c0` | 12 | `barbarian_blood_fury_slash` | 3 | 1 | 3 | 4 | Enemy |

`targetType` is serialized as `0`, matching `SkillTargetType.Enemy`.

## Player prefab slot migration

| Prefab | Slot 0 | Slot 1 | Slot 2 |
| --- | --- | --- | --- |
| `Fighter.prefab` | formal `fighter_shield_bash` | formal `fighter_guard_strike` | formal `fighter_crushing_challenge` |
| `Ranger.prefab` | formal `ranger_precision_shot` | formal `ranger_quick_shot` | formal `ranger_piercing_arrow` |
| `Mage.prefab` | formal `mage_fireball` | formal `mage_frost_bolt` | formal `mage_arcane_burst` |
| `Barbarian.prefab` | formal `barbarian_heavy_cleave` | formal `barbarian_rage_strike` | formal `barbarian_blood_fury_slash` |

Only the `SkillRuntime.skillSlots` GUID references were changed on the Player prefabs.

## OnlyTest assets retained

The original assets under `Assets/_BoneThrone/Data/OnlyTest/Skills/` were retained and were not modified or deleted.

Reason:

- They may still be referenced by hidden test tools or historical validation flows.
- Deleting or archiving test assets should be handled in a later explicit cleanup phase.

## Explicit non-changes

- No C# files were modified.
- `SkillEffectExecutor` was not modified.
- `SkillSystem` was not modified.
- `SkillTargetingService` was not modified.
- `DamageResolver` was not modified.
- `CombatSystem` was not modified.
- UI scripts were not modified.
- Scene files were not modified.
- Enemy prefabs were not modified.
- KayKit original assets were not modified.
- `Assets/_BoneThrone/Data/OnlyTest/Skills/` assets were not modified.
- No skill formulas were changed.
- No skill field values were changed.
- Ranger gameplay skill names remain `ranger_*`; they were not renamed to Rogue.

## Unity manual validation steps

1. Open Unity and wait for import.
2. Confirm `Assets/_BoneThrone/Data/Skills/` exists.
3. Confirm the 12 formal SkillData assets exist and have `.meta` files.
4. Select each formal SkillData asset and confirm fields match the table above.
5. Confirm `Assets/_BoneThrone/Data/OnlyTest/Skills/` still exists and still contains the original 12 assets.
6. Open each Player prefab and inspect `SkillRuntime.skillSlots`.
7. Confirm Fighter Slot 0 / 1 / 2 reference formal Fighter skills.
8. Confirm Ranger Slot 0 / 1 / 2 reference formal Ranger skills.
9. Confirm Mage Slot 0 / 1 / 2 reference formal Mage skills.
10. Confirm Barbarian Slot 0 / 1 / 2 reference formal Barbarian skills.
11. Confirm no C# files changed.
12. Confirm no scene files changed.
13. Open `Assets/_BoneThrone/Scenes/GridTest.unity`.
14. Enter Play Mode.
15. Test four roles Slot 0 / 1 / 2.
16. Confirm CombatLog, Enemy HP Bar, ActiveUnitProvider, and camera controls still work.
17. Confirm Console has no new red errors.

## Risks

- A mistyped formal GUID in a Player prefab would create a missing skill reference.
- A copied field mismatch would change the previously validated skill baseline.
- Unity may reserialize prefab files when opened; only `SkillRuntime.skillSlots` should differ.

## Rollback

To roll back this phase:

1. Delete `Assets/_BoneThrone/Data/Skills/` and `Assets/_BoneThrone/Data/Skills.meta`.
2. Restore the four Player prefab `SkillRuntime.skillSlots` GUIDs to the previous `OnlyTest/Skills` GUIDs.
3. Delete `Docs/DevLogs/Phase14.13D_FormalSkillDataMigration.md`.
4. Reopen Unity and confirm the Player prefabs no longer have missing skill references.

## Recommended next phase

Proceed to a final regression pass for the formal SkillData migration before considering any cleanup of `OnlyTest` assets.
