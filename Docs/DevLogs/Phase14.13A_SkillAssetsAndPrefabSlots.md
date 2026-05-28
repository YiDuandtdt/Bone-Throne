# Phase 14.13-A SkillData Assets and Player Prefab Slot Wiring

## Purpose and scope

Phase 14.13-A adds the missing Slot 1 and Slot 2 SkillData assets for the four current player roles and wires those assets into the existing Player prefabs.

This phase is asset and prefab wiring only. It does not implement the new skill effects, does not change UI slot behavior, and does not change gameplay execution code.

## Files changed

- `Assets/_BoneThrone/Data/OnlyTest/Skills/fighter_guard_strike.asset`
- `Assets/_BoneThrone/Data/OnlyTest/Skills/fighter_guard_strike.asset.meta`
- `Assets/_BoneThrone/Data/OnlyTest/Skills/fighter_crushing_challenge.asset`
- `Assets/_BoneThrone/Data/OnlyTest/Skills/fighter_crushing_challenge.asset.meta`
- `Assets/_BoneThrone/Data/OnlyTest/Skills/ranger_quick_shot.asset`
- `Assets/_BoneThrone/Data/OnlyTest/Skills/ranger_quick_shot.asset.meta`
- `Assets/_BoneThrone/Data/OnlyTest/Skills/ranger_piercing_arrow.asset`
- `Assets/_BoneThrone/Data/OnlyTest/Skills/ranger_piercing_arrow.asset.meta`
- `Assets/_BoneThrone/Data/OnlyTest/Skills/mage_frost_bolt.asset`
- `Assets/_BoneThrone/Data/OnlyTest/Skills/mage_frost_bolt.asset.meta`
- `Assets/_BoneThrone/Data/OnlyTest/Skills/mage_arcane_burst.asset`
- `Assets/_BoneThrone/Data/OnlyTest/Skills/mage_arcane_burst.asset.meta`
- `Assets/_BoneThrone/Data/OnlyTest/Skills/barbarian_rage_strike.asset`
- `Assets/_BoneThrone/Data/OnlyTest/Skills/barbarian_rage_strike.asset.meta`
- `Assets/_BoneThrone/Data/OnlyTest/Skills/barbarian_blood_fury_slash.asset`
- `Assets/_BoneThrone/Data/OnlyTest/Skills/barbarian_blood_fury_slash.asset.meta`
- `Assets/_BoneThrone/Prefabs/Units/Players/Fighter.prefab`
- `Assets/_BoneThrone/Prefabs/Units/Players/Ranger.prefab`
- `Assets/_BoneThrone/Prefabs/Units/Players/Mage.prefab`
- `Assets/_BoneThrone/Prefabs/Units/Players/Barbarian.prefab`
- `Docs/DevLogs/Phase14.13A_SkillAssetsAndPrefabSlots.md`

## New SkillData assets

| Asset | GUID | skillId | displayName | unlockLevel | range | cooldownTurns | guaranteedDamage | targetType |
| --- | --- | ---: | --- | ---: | ---: | ---: | ---: | --- |
| `fighter_guard_strike.asset` | `6e4a0f6f7d7d4c9e9a29a8f68f22e501` | 5 | `fighter_guard_strike` | 2 | 1 | 1 | 2 | Enemy |
| `fighter_crushing_challenge.asset` | `8bd2df7aa97a4b60a5013b82a7d621a4` | 6 | `fighter_crushing_challenge` | 3 | 1 | 3 | 4 | Enemy |
| `ranger_quick_shot.asset` | `1fb8a5bd4231437b8f81d958f2f6c0cd` | 7 | `ranger_quick_shot` | 2 | 4 | 1 | 2 | Enemy |
| `ranger_piercing_arrow.asset` | `3c87ad3bfbea4c15a64d462e06f7a6f2` | 8 | `ranger_piercing_arrow` | 3 | 5 | 3 | 3 | Enemy |
| `mage_frost_bolt.asset` | `94f7df6c51e0474fbfdaf0555d353b0c` | 9 | `mage_frost_bolt` | 2 | 3 | 1 | 2 | Enemy |
| `mage_arcane_burst.asset` | `d46a50858ec0478ab05b25e0792c79f1` | 10 | `mage_arcane_burst` | 3 | 3 | 3 | 3 | Enemy |
| `barbarian_rage_strike.asset` | `b8b67302b73d409590a3572d6ce4cc1a` | 11 | `barbarian_rage_strike` | 2 | 1 | 1 | 3 | Enemy |
| `barbarian_blood_fury_slash.asset` | `fdce09bb49074e72b19f54224ca8054c` | 12 | `barbarian_blood_fury_slash` | 3 | 1 | 3 | 4 | Enemy |

`targetType` is serialized as `0`, matching the current Enemy enum value used by the existing Slot 0 SkillData assets.

## Player prefab slot wiring

| Prefab | Slot 0 | Slot 1 | Slot 2 |
| --- | --- | --- | --- |
| `Fighter.prefab` | `fighter_shield_bash` | `fighter_guard_strike` | `fighter_crushing_challenge` |
| `Ranger.prefab` | `ranger_precision_shot` | `ranger_quick_shot` | `ranger_piercing_arrow` |
| `Mage.prefab` | `mage_fireball` | `mage_frost_bolt` | `mage_arcane_burst` |
| `Barbarian.prefab` | `barbarian_heavy_cleave` | `barbarian_rage_strike` | `barbarian_blood_fury_slash` |

## Explicit non-changes

- No C# files were modified.
- `DamageResolver` was not modified.
- `SkillEffectExecutor` was not modified.
- `FighterSkillEffects`, `RangerSkillEffects`, `MageSkillEffects`, and `BarbarianSkillEffects` were not modified.
- `SkillSystem` and `SkillTargetingService` were not modified.
- `CombatSystem` was not modified.
- UI scripts were not modified.
- No skill formula or skill execution semantics were changed.
- No enemy prefabs were modified.
- No scenes were modified.
- No KayKit original assets were modified.
- Phase 14.10 camera controls were not modified.
- Phase 14.11 ActiveUnitProvider provider/fallback logic was not modified.

## Unity manual check steps

1. Open the project in Unity 6.3 LTS.
2. Confirm the eight new SkillData assets import without missing script warnings.
3. Select each new SkillData asset and verify its fields match the table above.
4. Open each Player prefab and inspect the `SkillRuntime` component.
5. Confirm Slot 0 remains unchanged for each role.
6. Confirm Slot 1 and Slot 2 are wired as listed in the prefab slot wiring table.
7. Do not expect Slot 1 / Slot 2 to be fully usable in battle UI until a later UI and execution integration phase.

## Risks

- Unity may reserialize the prefab files when they are opened. Only the intended `SkillRuntime.skillSlots` references should change.
- Slot 1 / Slot 2 assets now exist and are wired, but effects and UI selection are not implemented in this phase.
- The machine-id-style `displayName` values are intentionally retained to stay aligned with the current effect matching pattern.

## Rollback

1. Delete the eight new SkillData `.asset` files and their `.meta` files.
2. Restore the four Player prefab Slot 1 and Slot 2 entries to `{fileID: 0}`.
3. Delete `Docs/DevLogs/Phase14.13A_SkillAssetsAndPrefabSlots.md`.
4. Reopen Unity and confirm no missing asset references remain on the Player prefabs.

## Recommended next phase

Proceed to Phase 14.13-B - UI Slot 1/2 Integration, only after confirming the new assets and prefab slot references import cleanly in Unity.
