# ACTIVE TASK

## Current Phase

Phase 15.11 - Formal Data Assetization Plan

## Status

Phase 15.10 - GridTest Scene Assembly Kit Validation has passed Unity 6.3 Play Mode manual validation and is closed.

`Assets/_BoneThrone/Scenes/GridTest.unity` remains the regression baseline. It is not a formal level and must not be converted into `Level_01`, `Level_02`, or `Level_03`.

## Phase 15.11 Goal

Create the formal data assetization plan for the production foundation built through Phase 15.10.

Phase 15.11 is a planning phase. It should identify which gameplay, unit, interactable, level-assembly, progression, and configuration data should eventually become formal project-owned data assets, and it should define safe migration order and boundaries.

Phase 15.11 must not directly create formal Level scenes or implement formal level gameplay.

## Allowed Scope

Phase 15.11 should start as docs-only unless a later prompt explicitly approves implementation.

Expected planning inputs:

- `Docs/Phase15_Plan.md`
- `Docs/Phase15_AssetInventoryAndPrefabizationPlan.md`
- `Docs/Phase15_EnvironmentPrefabizationSummary.md`
- `Docs/DevLogs/Phase15.10_GridTestSceneAssemblyKitValidation.md`
- Current project-owned prefabs and data assets, read-only
- Current gameplay scripts, read-only

Possible docs output:

- `Docs/Phase15_FormalDataAssetizationPlan.md`
- `Docs/DevLogs/Phase15.11_FormalDataAssetizationPlan.md`

## Forbidden Changes

- Do not create `Level_01`, `Level_02`, or `Level_03`.
- Do not modify gameplay code.
- Do not modify prefabs.
- Do not modify scenes.
- Do not modify `GridTest.unity`.
- Do not modify SkillData unless the Phase 15.11 plan explicitly approves a later data migration step.
- Do not modify KayKit source assets.
- Do not modify animation controllers or animation clips.
- Do not modify weapon visual attachments.
- Do not modify Turn, Combat, Skill, Potion, or LAN systems.
- Do not implement Boss, BossDoor, BossKey, SupplyPoint, or formal level flow.
- Do not change the single-player free-order PlayerTurn rule.
- Do not use Fighter -> Ranger -> Mage -> Barbarian fixed-order for single-player.

## Next Step

Begin Phase 15.11 with a plan-only prompt:

1. Scan current Docs and data / prefab / gameplay configuration state.
2. Identify which systems are still scene-bound, prefab-bound, or hard-coded.
3. Propose a formal data assetization map.
4. Define what should remain untouched until later phases.
5. Do not modify files until the Phase 15.11 implementation scope is explicitly approved.
