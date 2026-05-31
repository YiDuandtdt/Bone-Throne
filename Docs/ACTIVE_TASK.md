# ACTIVE TASK

## Current Phase

Phase 15.16 - Boss Door / Boss Key / Supply Point Preparation

## Status

Phase 15.16 is a documentation-only preparation pass for future BossDoor, BossKey, and SupplyPoint work.

No BossDoor, BossKey, or SupplyPoint runtime feature has been implemented.

No formal level scene was modified.

Formal level scene content remains user-owned.

`GridTest.unity` remains the regression baseline and must not be converted into a formal level.

## Phase 15.16 Result

Created planning documentation for:

- BossKey future responsibility
- BossDoor future responsibility
- SupplyPoint future responsibility
- relationship to `KeyItem`, `InteractableStairs`, `LevelProgressionService`, `LevelManager`, `HealthPotionPickup`, `PotionSystem`, and room systems
- future possible script / prefab / data scope
- deferred Boss fight / formal scene integration work
- risks and scene ownership boundaries

Phase 15.16 output:

- `Docs/Phase15_BossDoorBossKeySupplyPointPreparation.md`
- `Docs/DevLogs/Phase15.16_BossDoorBossKeySupplyPointPreparation.md`

## Codex Scene Boundary

Codex no longer owns:

- creating formal level scenes
- modifying formal level scenes
- wiring level scene managers
- building room / tile / encounter / interactable scene content
- making `Level_01`, `Level_02`, or `Level_03` playable slices
- extending `Level_02` / `Level_03` progression scenes
- modifying `GridTest` as a formal level
- automatically copying `GridTest` structure into formal levels

Codex may still help with:

- documentation planning
- checklist creation
- review notes
- DevLogs
- narrow code fixes
- narrow prefab fixes when explicitly approved
- non-scene rules / data / UI / system preparation
- scene inspection advice for user-made levels, without direct scene modification

## References

Current references:

- `Docs/Phase15_FormalLevelScenePlan.md`
- `Docs/Phase15_FormalDataAssetizationPlan.md`
- `Docs/Phase15_AssetInventoryAndPrefabizationPlan.md`
- `Docs/Phase15_EnvironmentPrefabizationSummary.md`
- `Docs/DevLogs/Phase15.10_GridTestSceneAssemblyKitValidation.md`
- `Docs/DevLogs/Phase15.13_LevelSceneSetup.md`
- `Docs/DevLogs/Phase15.14_Level01PlayableSlice.md`
- `Docs/DevLogs/Phase15.15_ManualLevelSceneOwnership.md`
- `Docs/DevLogs/Phase15.16_BossDoorBossKeySupplyPointPreparation.md`
- `Docs/Phase15_BossDoorBossKeySupplyPointPreparation.md`
- `Docs/Phase15_Plan.md`

## Next Phase Candidate

Phase 15.17 - BossDoor / BossKey / SupplyPoint Minimal Contract Review

Phase 15.17 should remain non-scene. It may review minimal API contracts, checklist requirements, and future implementation order for BossDoor / BossKey / SupplyPoint. It should not create runtime scripts, prefabs, data assets, or scene placements unless a later prompt explicitly approves a narrow non-scene implementation.

## Forbidden Scene Changes

- Do not create formal level scenes.
- Do not modify formal level scenes.
- Do not modify `GridTest.unity`.
- Do not modify `Level_01.unity`, `Level_02.unity`, or `Level_03.unity` unless the user explicitly overrides this boundary.
- Do not wire level scene managers.
- Do not build room / tile / encounter / interactable scene content.
- Do not create playable slices for formal levels.
- Do not auto-copy `GridTest` scene structure into formal levels.

## General Forbidden Changes Until Explicitly Approved

- Do not modify C# code unless a phase prompt explicitly approves a narrow fix.
- Do not modify SkillData.
- Do not modify KayKit source assets.
- Do not implement Boss fight.
- Do not place BossDoor / BossKey.
- Do not place SupplyPoint.
- Do not implement Victory / Defeat / Retry.
- Do not implement LAN / Networking.
- Do not create ScriptableObject assets unless a later data phase explicitly approves it.
- Do not change the single-player free-order PlayerTurn rule.
- Do not use Fighter -> Ranger -> Mage -> Barbarian fixed-order for single-player.
