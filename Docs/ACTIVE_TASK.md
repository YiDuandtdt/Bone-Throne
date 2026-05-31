# ACTIVE TASK

## Current Phase

Phase 15.14 - Level_01 Playable Slice

## Status

Phase 15.13 - Level_01 / Level_02 / Level_03 Scene Setup is complete.

The project now has initial formal scene skeletons:

- `Assets/_BoneThrone/Scenes/Level_01.unity`
- `Assets/_BoneThrone/Scenes/Level_02.unity`
- `Assets/_BoneThrone/Scenes/Level_03.unity`
- `Docs/DevLogs/Phase15.13_LevelSceneSetup.md`

The scenes are structure-only skeletons with clear hierarchy groups. They do not contain gameplay wiring yet.

No `GridTest.unity`, code, prefab, SkillData, KayKit source, ScriptableObject, material, animation, ProjectSettings, or Packages file was modified.

## Phase 15.14 Goal

Begin the first narrow playable slice for:

- `Level_01`

Phase 15.14 should make `Level_01` minimally playable while keeping `GridTest.unity` as the regression baseline and without converting `GridTest` into a formal level.

## References

Phase 15.14 should reference:

- `Docs/Phase15_FormalLevelScenePlan.md`
- `Docs/Phase15_FormalDataAssetizationPlan.md`
- `Docs/Phase15_AssetInventoryAndPrefabizationPlan.md`
- `Docs/Phase15_EnvironmentPrefabizationSummary.md`
- `Docs/DevLogs/Phase15.10_GridTestSceneAssemblyKitValidation.md`
- `Docs/DevLogs/Phase15.13_LevelSceneSetup.md`
- `Docs/Phase15_Plan.md`

## Allowed Scope

Phase 15.14 should start with an implementation plan before wiring playable systems.

Potential implementation scope, only after explicit approval:

- `Assets/_BoneThrone/Scenes/Level_01.unity`
- `Docs/DevLogs/Phase15.14_Level01PlayableSlice.md`
- `Docs/ACTIVE_TASK.md`

## Forbidden Changes Until Explicitly Approved

- Do not modify `GridTest.unity`.
- Do not modify C# code unless the Phase 15.14 prompt explicitly approves a narrow fix.
- Do not modify SkillData.
- Do not modify KayKit source assets.
- Do not implement Boss fight.
- Do not implement BossDoor / BossKey.
- Do not implement SupplyPoint.
- Do not implement Victory / Defeat / Retry.
- Do not implement LAN / Networking.
- Do not create ScriptableObject assets unless a later data phase explicitly approves it.
- Do not change the single-player free-order PlayerTurn rule.
- Do not use Fighter -> Ranger -> Mage -> Barbarian fixed-order for single-player.

## Next Step

Begin Phase 15.14 with a plan-first prompt:

1. Identify exactly which `Level_01` manager / system objects should be wired.
2. Identify which GridTest-only testers and temporary objects must not be copied.
3. Define the minimum playable room, tile, player, enemy, UI, HealthPotion, Key, and Stairs setup.
4. Define validation criteria for `Level_01`.
5. Keep `Level_02` and `Level_03` as skeletons unless explicitly approved.
6. Do not modify `GridTest.unity`.
