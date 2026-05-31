# ACTIVE TASK

## Current Phase

Phase 15.12 - Formal Level Scene Plan

## Status

Phase 15.11 - Formal Data Assetization Plan is complete.

The project now has a docs-only plan for future data assetization:

- `Docs/Phase15_FormalDataAssetizationPlan.md`
- `Docs/DevLogs/Phase15.11_FormalDataAssetizationPlan.md`

No ScriptableObject assets were created during Phase 15.11. No code, prefab, scene, SkillData, KayKit source, material, animation, ProjectSettings, or Packages file was modified.

## Phase 15.12 Goal

Create the formal level scene plan for the future three-floor structure.

Phase 15.12 is a planning phase. It should define how formal levels should be structured, what scenes will eventually be needed, what each floor should contain, and what dependencies must be resolved before scene construction begins.

Phase 15.12 must not directly build `Level_01`, `Level_02`, or `Level_03`.

## References

Phase 15.12 should reference:

- `Docs/Phase15_FormalDataAssetizationPlan.md`
- `Docs/Phase15_AssetInventoryAndPrefabizationPlan.md`
- `Docs/Phase15_EnvironmentPrefabizationSummary.md`
- `Docs/DevLogs/Phase15.10_GridTestSceneAssemblyKitValidation.md`
- `Docs/Phase15_Plan.md`

## Allowed Scope

Phase 15.12 should start as docs-only unless a later prompt explicitly approves implementation.

Possible docs output:

- `Docs/Phase15_FormalLevelScenePlan.md`
- `Docs/DevLogs/Phase15.12_FormalLevelScenePlan.md`

## Forbidden Changes

- Do not create `Level_01`, `Level_02`, or `Level_03`.
- Do not create formal level scenes unless a later implementation phase explicitly approves it.
- Do not modify C# code.
- Do not modify prefabs.
- Do not modify scenes.
- Do not modify `GridTest.unity`.
- Do not modify SkillData.
- Do not create ScriptableObject assets.
- Do not modify KayKit source assets.
- Do not modify animation controllers or animation clips.
- Do not modify Turn, Combat, Skill, Potion, or LAN systems.
- Do not implement Boss fight, BossDoor, BossKey, SupplyPoint, or formal Level progression.
- Do not change the single-player free-order PlayerTurn rule.
- Do not use Fighter -> Ranger -> Mage -> Barbarian fixed-order for single-player.

## Next Step

Begin Phase 15.12 with a plan-only prompt:

1. Scan the Phase 15 data assetization plan and prefabization summaries.
2. Define the intended `Level_01`, `Level_02`, and `Level_03` goals and boundaries.
3. Plan scene structure, room structure, encounter placement strategy, stairs / key flow, and validation criteria.
4. Identify what remains deferred to Phase 15.13+.
5. Do not create scenes or modify files until the Phase 15.12 implementation scope is explicitly approved.
