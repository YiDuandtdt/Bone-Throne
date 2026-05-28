# ACTIVE_TASK.md

## Current Phase

Phase 15.4 - Environment Prefabization Pass

## Phase 15.3 Completion Note

Phase 15.3 - Asset Inventory and Prefabization Plan is complete.

Phase 15.3 scanned the current imported assets and documented the asset inventory and prefabization plan in:

- `Docs/Phase15_AssetInventoryAndPrefabizationPlan.md`
- `Docs/DevLogs/Phase15.3_AssetInventoryAndPrefabizationPlan.md`

Phase 15.4 must use `Docs/Phase15_AssetInventoryAndPrefabizationPlan.md` as the source of truth.

## Goal

Implement only the Phase 15.4 slice: Environment Prefabization Pass.

Phase 15.4 should create project-owned environment wrappers or prefab variants from actual scanned environment assets, especially the environment candidates found under the KayKit Dungeon Remastered package.

## Phase 15.4 Scope Guardrails

- Phase 15.4 can only prefabize environment assets that actually exist in the scanned repository.
- Phase 15.4 must not invent or assume asset categories that were not found in the scan.
- Phase 15.4 must not modify KayKit original resources.
- Phase 15.4 can only create project-owned wrappers or prefab variants under `_BoneThrone/Prefabs`.
- Phase 15.4 does not process Interactables. Those belong to Phase 15.5.
- Phase 15.4 does not process Characters. Those belong to Phase 15.6.
- Phase 15.4 does not process Weapons or Equipment. Those belong to Phase 15.7.
- Phase 15.4 does not process Animation Controllers or animation state machines. Those belong to Phase 15.8.
- Phase 15.4 does not modify gameplay code.
- Phase 15.4 does not modify Turn, Combat, Skill, Potion, LAN, or Networking systems.
- Phase 15.4 does not create formal Level scenes.
- `GridTest.unity` remains the regression baseline and must not be converted into a formal level.

## Allowed Files

Phase 15.4 allowed files must be confirmed before implementation.

Expected direction:

- Project-owned environment prefab or prefab variant paths under `Assets/_BoneThrone/Prefabs/`
- A Phase 15.4 DevLog under `Docs/DevLogs/`

Do not expand this scope without first explaining why.

## Forbidden Changes

- Do not modify C# code.
- Do not create or modify prefab files unless they are explicitly approved Phase 15.4 project-owned environment wrappers or variants.
- Do not modify scenes.
- Do not modify materials.
- Do not modify SkillData.
- Do not modify KayKit original resources.
- Do not create formal Level scenes.
- Do not change the single-player PlayerTurn rule.
- Do not use Fighter -> Ranger -> Mage -> Barbarian fixed-order for single-player.
- Do not process Interactables in Phase 15.4.
- Do not process Characters in Phase 15.4.
- Do not process Weapons or Equipment in Phase 15.4.
- Do not process Animation Controllers in Phase 15.4.
- Do not modify LAN / Networking.
- Do not implement Boss content.

## Validation Direction

Phase 15.4 validation should be defined before implementation.

Expected validation direction:

1. Confirm the selected environment prefab candidates come from the Phase 15.3 scan.
2. Confirm new project-owned wrappers or prefab variants live under `_BoneThrone/Prefabs`.
3. Confirm no KayKit original resources were modified.
4. Confirm no gameplay code, SkillData, materials, scenes, or formal Level scenes were modified.
5. Confirm `GridTest.unity` remains the regression baseline and is not converted into a formal level.
6. Confirm single-player free-order PlayerTurn remains unchanged.
