# Phase 13 DevLog - Gameplay Prefab Wrapping

## Date
2026-05-24

## Scope
Created the first small Phase 12.6 prefab-wrapping slice only. No C# scripts, scenes, KayKit source prefabs, UI, audio, VFX, networking, combat, turn, skill, AI, room, or level systems were modified.

## Created prefabs
- `Assets/_BoneThrone/Prefabs/Units/Players/Fighter.prefab`
- `Assets/_BoneThrone/Prefabs/Units/Enemies/Skeleton_Minion.prefab`
- `Assets/_BoneThrone/Prefabs/Interactables/Key.prefab`
- `Assets/_BoneThrone/Prefabs/Interactables/Stairs.prefab`

## Prefab component summary
- `Fighter`
  - Root gameplay components: `Unit`, `UnitTurnState`, `SkillRuntime`, `Rigidbody`, `CapsuleCollider`.
  - Visual child source: KayKit Adventurers `Knight.prefab`, nested as `Visual`.
  - `Unit.currentTile` is intentionally empty and must be assigned by placement/runtime scene flow.
- `Skeleton_Minion`
  - Root gameplay components: `Unit`, `UnitTurnState`, `Rigidbody`, `SphereCollider`.
  - Visual child source: KayKit Skeletons `Skeleton_Minion.prefab`, nested as `Visual`.
  - `unitId` is `1001` as a placeholder only. Scene instances or future spawners should override IDs so multiple enemies do not share the same tile occupant ID.
- `Key`
  - Root gameplay components: `KeyItem`, `BoxCollider`.
  - Visual child source: KayKit Dungeon `key_gold.prefab`, nested as `Visual`.
  - `KeyItem.progressionService` is intentionally empty for scene-instance Inspector binding.
- `Stairs`
  - Root gameplay components: `InteractableStairs`, `BoxCollider`.
  - Visual child source: KayKit Dungeon `stairs.prefab`, nested as `Visual`.
  - `progressionService`, `selectionManager`, `feedbackRenderers`, `normalMaterial`, and `hoverMaterial` are intentionally empty for scene-instance Inspector binding.

## Inspector binding needed
- Bind `Fighter.SkillRuntime.skillSlots` if more Fighter skills are ready beyond the current first slot.
- Override `Skeleton_Minion.Unit.unitId` per scene instance if more than one enemy instance exists.
- Bind `Key.KeyItem.progressionService` to the scene `LevelProgressionService`.
- Bind `Stairs.InteractableStairs.progressionService` to the scene `LevelProgressionService`.
- Bind `Stairs.InteractableStairs.selectionManager` to the scene `SelectionManager`.
- Add Visual child renderers to `Stairs.InteractableStairs.feedbackRenderers` if hover material feedback is required.
- Assign `normalMaterial` and `hoverMaterial` only when lightweight hover feedback materials are available.

## Manual Unity 6.3 Play Mode tests
- Open Unity 6.3 LTS and confirm there are no compile errors.
- Open each new prefab and confirm there are no missing scripts or missing nested KayKit visuals.
- Temporarily place `Fighter.prefab` in `GridTest`, bind it to existing testers if needed, and verify unit placement.
- Verify selecting and moving the placed Fighter still works through existing movement flow.
- Temporarily place `Skeleton_Minion.prefab`, override `unitId` if needed, and verify basic attack / D20 and enemy AI tester flows.
- Temporarily place `Key.prefab`, bind `progressionService`, and verify pickup through click or trigger.
- Temporarily place `Stairs.prefab`, bind progression and selection references, then verify hover, confirmation, level transition, and upgrade flow.

## Known risks
- Nested KayKit prefab references must be reimported by Unity to validate exact visual scale and orientation.
- `Skeleton_Minion.unitId` is a placeholder and can conflict if multiple instances are placed without override.
- `Stairs.feedbackRenderers` is empty by design, so hover material feedback will not appear until scene-instance renderers and materials are assigned.
- Key and Stairs cross-scene service references are intentionally empty in prefab assets.
- Collider shape and visual mesh may need Inspector adjustment after visual scale is checked in Unity.

## Rollback
- Remove or revert the created prefab folders/files:
  - `Assets/_BoneThrone/Prefabs/Units/Players/Fighter.prefab`
  - `Assets/_BoneThrone/Prefabs/Units/Enemies/Skeleton_Minion.prefab`
  - `Assets/_BoneThrone/Prefabs/Interactables/Key.prefab`
  - `Assets/_BoneThrone/Prefabs/Interactables/Stairs.prefab`
  - Their `.meta` files and any newly added folder `.meta` files.
- Remove or revert this DevLog:
  - `Docs/DevLogs/Phase12_6_Gameplay_Prefab_Wrapping.md`
- Example single-file rollback:
  - `git checkout -- Assets/_BoneThrone/Prefabs/Units/Players/Fighter.prefab`

  ## Additional test assets and scene updates
- Added `Assets/_BoneThrone/Data/OnlyTest/Skills/` test SkillData assets used by the gameplay wrapper prefabs.
- Updated `Assets/_BoneThrone/Scenes/GridTest.unity` to keep the verified prefab regression setup.
- The scene change is intentionally kept because the new prefab wrappers were verified through the existing GridTest workflow.
- Updated KayKit material assets used by the wrapped prefabs so their `_MainTex` / color settings correctly display the imported visual models in the current URP project setup.