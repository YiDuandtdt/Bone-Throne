# ACTIVE TASK

## Current Phase

Phase 15.13 - Level_01 / Level_02 / Level_03 Scene Setup

## Status

Phase 15.12 - Formal Level Scene Plan is complete.

The project now has a docs-only formal level scene plan:

- `Docs/Phase15_FormalLevelScenePlan.md`
- `Docs/DevLogs/Phase15.12_FormalLevelScenePlan.md`

No formal Level scene was created during Phase 15.12. No `GridTest.unity`, code, prefab, SkillData, KayKit source, ScriptableObject, material, animation, ProjectSettings, or Packages file was modified.

## Phase 15.13 Goal

Begin narrow formal scene setup for:

- `Level_01`
- `Level_02`
- `Level_03`

Phase 15.13 should create scene structure only when explicitly approved by the user. It must keep `GridTest.unity` as the regression baseline and must not convert it into a formal level.

## References

Phase 15.13 should reference:

- `Docs/Phase15_FormalLevelScenePlan.md`
- `Docs/Phase15_FormalDataAssetizationPlan.md`
- `Docs/Phase15_AssetInventoryAndPrefabizationPlan.md`
- `Docs/Phase15_EnvironmentPrefabizationSummary.md`
- `Docs/DevLogs/Phase15.10_GridTestSceneAssemblyKitValidation.md`
- `Docs/Phase15_Plan.md`

## Allowed Scope

Phase 15.13 should start with an implementation plan before creating or modifying scenes.

Potential implementation scope, only after explicit approval:

- `Assets/_BoneThrone/Scenes/Level_01.unity`
- `Assets/_BoneThrone/Scenes/Level_02.unity`
- `Assets/_BoneThrone/Scenes/Level_03.unity`
- `Docs/DevLogs/Phase15.13_LevelSceneSetup.md`

## Forbidden Changes Until Explicitly Approved

- Do not modify `GridTest.unity`.
- Do not modify C# code.
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

Begin Phase 15.13 with a plan-only prompt:

1. Decide whether scenes should be created from scratch or copied from a minimal GridTest-safe template.
2. Identify exactly which manager / system objects are allowed to migrate.
3. Identify which test-only objects must not be copied.
4. Define the initial scene setup order.
5. Define validation criteria for each scene.
6. Do not create scenes until the user explicitly approves the implementation scope.
