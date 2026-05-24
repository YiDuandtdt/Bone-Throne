# Phase 12.6 DevLog - Gameplay Prefab Wrapping

## Date
2026-05-24

## Scope
Created the first small Phase 12.6 prefab-wrapping slice only. No C# scripts, scenes, KayKit source prefabs, UI, audio, VFX, networking, combat, turn, skill, AI, room, or level systems were modified.

## Created prefabs
- `Assets/_BoneThrone/Prefabs/Units/Players/Fighter.prefab`
- `Assets/_BoneThrone/Prefabs/Units/Players/Ranger.prefab`
- `Assets/_BoneThrone/Prefabs/Units/Players/Mage.prefab`
- `Assets/_BoneThrone/Prefabs/Units/Players/Barbarian.prefab`
- `Assets/_BoneThrone/Prefabs/Units/Enemies/Skeleton_Minion.prefab`
- `Assets/_BoneThrone/Prefabs/Units/Enemies/Skeleton_Warrior.prefab`
- `Assets/_BoneThrone/Prefabs/Units/Enemies/Skeleton_Mage.prefab`
- `Assets/_BoneThrone/Prefabs/Units/Enemies/Skeleton_Rogue.prefab`
- `Assets/_BoneThrone/Prefabs/Interactables/Key.prefab`
- `Assets/_BoneThrone/Prefabs/Interactables/Stairs.prefab`

## Collider replacement patch
- Replaced the primitive gameplay-root colliders on the four representative prefabs with `MeshCollider`.
- `Fighter` now uses the KayKit Knight `Knight_Body` mesh on the root `MeshCollider`.
- `Skeleton_Minion` now uses the KayKit Skeleton `Skeleton_Minion_Body` mesh on the root `MeshCollider`.
- `Key` now uses the KayKit Dungeon `key_gold` mesh on the root `MeshCollider`.
- `Stairs` now uses the KayKit Dungeon `stairs` mesh on the root `MeshCollider`.
- `Fighter` and `Skeleton_Minion` keep root `Rigidbody` components, so their `MeshCollider` components are set to `convex=true`.
- `Key` and `Stairs` keep the previous non-trigger raycast/click interaction shape from their original `BoxCollider` setup; their `MeshCollider` components are non-convex because they do not have root `Rigidbody` components and are not trigger colliders.

## Player prefab expansion patch
- Added `Ranger.prefab`.
  - Root gameplay components: `Unit`, `UnitTurnState`, `SkillRuntime`, `Rigidbody`, `MeshCollider`.
  - Visual child source: KayKit Adventurers `Rogue.prefab`, nested as `Visual`.
  - Ranger gameplay intentionally uses Adventurers Rogue visual as the final preferred visual direction.
  - MeshCollider source: `Rogue_Body`, `convex=true`.
  - SkillRuntime slot 0: `Assets/_BoneThrone/Data/OnlyTest/Skills/ranger_precision_shot.asset`.
  - Stats: HP 24, move 5, attack modifier 5, defense 12, base damage 6.
  - Attack range from the design document is not stored on `UnitStats` in the current codebase, so no prefab field was changed for range.
- Added `Mage.prefab`.
  - Root gameplay components: `Unit`, `UnitTurnState`, `SkillRuntime`, `Rigidbody`, `MeshCollider`.
  - Visual child source: KayKit Adventurers `Mage.prefab`, nested as `Visual`.
  - MeshCollider source: `Mage_Body`, `convex=true`.
  - SkillRuntime slot 0: `Assets/_BoneThrone/Data/OnlyTest/Skills/mage_fireball.asset`.
  - Stats: HP 20, move 4, attack modifier 4, defense 11, base damage 4.
  - Attack range from the design document is not stored on `UnitStats` in the current codebase, so no prefab field was changed for range.
- Added `Barbarian.prefab`.
  - Root gameplay components: `Unit`, `UnitTurnState`, `SkillRuntime`, `Rigidbody`, `MeshCollider`.
  - Visual child source: KayKit Adventurers `Barbarian.prefab`, nested as `Visual`.
  - MeshCollider source: `Barbarian_Body`, `convex=true`.
  - SkillRuntime slot 0: `Assets/_BoneThrone/Data/OnlyTest/Skills/barbarian_heavy_cleave.asset`.
  - Stats: HP 28, move 4, attack modifier 5, defense 13, base damage 8.
  - Attack range from the design document is not stored on `UnitStats` in the current codebase, so no prefab field was changed for range.

## Enemy prefab expansion patch
- Added `Skeleton_Warrior.prefab`.
  - Root gameplay components: `Unit`, `UnitTurnState`, `Rigidbody`, `MeshCollider`.
  - Visual child source: KayKit Skeletons `Skeleton_Warrior.prefab`, nested as `Visual`.
  - MeshCollider source: `Skeleton_Warrior_Body`, `convex=true`.
  - `unitId` placeholder: `1002`.
- Added `Skeleton_Mage.prefab`.
  - Root gameplay components: `Unit`, `UnitTurnState`, `Rigidbody`, `MeshCollider`.
  - Visual child source: KayKit Skeletons `Skeleton_Mage.prefab`, nested as `Visual`.
  - MeshCollider source: `Skeleton_Mage_Body`, `convex=true`.
  - `unitId` placeholder: `1003`.
- Added `Skeleton_Rogue.prefab`.
  - Root gameplay components: `Unit`, `UnitTurnState`, `Rigidbody`, `MeshCollider`.
  - Visual child source: KayKit Skeletons `Skeleton_Rogue.prefab`, nested as `Visual`.
  - MeshCollider source: `Skeleton_Rogue_Body`, `convex=true`.
  - `unitId` placeholder: `1004`.
  - `Skeleton_Golem` name is reserved for a future Boss prefab. The normal enemy prefab is now `Skeleton_Rogue`.
- Enemy prefab `unitId` values are prefab placeholders only. Final scene placement or future spawners should override them with unique IDs to avoid tile occupant conflicts.

## Prefab component summary
- `Fighter`
  - Root gameplay components: `Unit`, `UnitTurnState`, `SkillRuntime`, `Rigidbody`, `MeshCollider`.
  - Visual child source: KayKit Adventurers `Knight.prefab`, nested as `Visual`.
  - Stats: HP 32, move 4, attack modifier 4, defense 15, base damage 6.
  - Attack range from the design document is not stored on `UnitStats` in the current codebase, so no prefab field was changed for range.
  - `Unit.currentTile` is intentionally empty and must be assigned by placement/runtime scene flow.
- `Ranger`
  - Root gameplay components: `Unit`, `UnitTurnState`, `SkillRuntime`, `Rigidbody`, `MeshCollider`.
  - Visual child source: KayKit Adventurers `Rogue.prefab`, nested as `Visual`.
  - Ranger gameplay intentionally uses Adventurers Rogue visual as the final preferred visual direction.
  - `Unit.currentTile` is intentionally empty and must be assigned by placement/runtime scene flow.
- `Mage`
  - Root gameplay components: `Unit`, `UnitTurnState`, `SkillRuntime`, `Rigidbody`, `MeshCollider`.
  - Visual child source: KayKit Adventurers `Mage.prefab`, nested as `Visual`.
  - `Unit.currentTile` is intentionally empty and must be assigned by placement/runtime scene flow.
- `Barbarian`
  - Root gameplay components: `Unit`, `UnitTurnState`, `SkillRuntime`, `Rigidbody`, `MeshCollider`.
  - Visual child source: KayKit Adventurers `Barbarian.prefab`, nested as `Visual`.
  - `Unit.currentTile` is intentionally empty and must be assigned by placement/runtime scene flow.
- `Skeleton_Minion`
  - Root gameplay components: `Unit`, `UnitTurnState`, `Rigidbody`, `MeshCollider`.
  - Visual child source: KayKit Skeletons `Skeleton_Minion.prefab`, nested as `Visual`.
  - `unitId` is `1001` as a placeholder only. Scene instances or future spawners should override IDs so multiple enemies do not share the same tile occupant ID.
- `Skeleton_Warrior`
  - Root gameplay components: `Unit`, `UnitTurnState`, `Rigidbody`, `MeshCollider`.
  - Visual child source: KayKit Skeletons `Skeleton_Warrior.prefab`, nested as `Visual`.
  - `unitId` is `1002` as a placeholder only. Scene instances or future spawners should override IDs.
- `Skeleton_Mage`
  - Root gameplay components: `Unit`, `UnitTurnState`, `Rigidbody`, `MeshCollider`.
  - Visual child source: KayKit Skeletons `Skeleton_Mage.prefab`, nested as `Visual`.
  - `unitId` is `1003` as a placeholder only. Scene instances or future spawners should override IDs.
- `Skeleton_Rogue`
  - Root gameplay components: `Unit`, `UnitTurnState`, `Rigidbody`, `MeshCollider`.
  - Visual child source: KayKit Skeletons `Skeleton_Rogue.prefab`, nested as `Visual`.
  - `unitId` is `1004` as a placeholder only. Scene instances or future spawners should override IDs.
  - `Skeleton_Golem` name is reserved for a future Boss prefab, not a normal enemy.
- `Key`
  - Root gameplay components: `KeyItem`, `MeshCollider`.
  - Visual child source: KayKit Dungeon `key_gold.prefab`, nested as `Visual`.
  - `KeyItem.progressionService` is intentionally empty for scene-instance Inspector binding.
- `Stairs`
  - Root gameplay components: `InteractableStairs`, `MeshCollider`.
  - Visual child source: KayKit Dungeon `stairs.prefab`, nested as `Visual`.
  - `progressionService`, `selectionManager`, `feedbackRenderers`, `normalMaterial`, and `hoverMaterial` are intentionally empty for scene-instance Inspector binding.

## Inspector binding needed
- Bind `Fighter.SkillRuntime.skillSlots` if more Fighter skills are ready beyond the current first slot.
- `Ranger`, `Mage`, and `Barbarian` have their current OnlyTest representative skill bound in slot 0; slots 1 and 2 are intentionally empty.
- Override `Skeleton_Minion.Unit.unitId` per scene instance if more than one enemy instance exists.
- Override `Skeleton_Warrior`, `Skeleton_Mage`, and `Skeleton_Rogue` `Unit.unitId` values per scene instance or spawner output when multiple enemies exist.
- Bind `Key.KeyItem.progressionService` to the scene `LevelProgressionService`.
- Bind `Stairs.InteractableStairs.progressionService` to the scene `LevelProgressionService`.
- Bind `Stairs.InteractableStairs.selectionManager` to the scene `SelectionManager`.
- Add Visual child renderers to `Stairs.InteractableStairs.feedbackRenderers` if hover material feedback is required.
- Assign `normalMaterial` and `hoverMaterial` only when lightweight hover feedback materials are available.

## Manual Unity 6.3 Play Mode tests
- Open Unity 6.3 LTS and confirm there are no compile errors.
- Open each new prefab and confirm there are no missing scripts or missing nested KayKit visuals.
- Confirm each root `MeshCollider.sharedMesh` is assigned and the expected convex setting is preserved.
- Temporarily place `Fighter.prefab` in `GridTest`, bind it to existing testers if needed, and verify unit placement.
- Temporarily place `Fighter`, `Ranger`, `Mage`, and `Barbarian` prefabs in `GridTest`, bind them to existing testers if needed, and verify unit placement.
- Verify selecting, raycast clicking, and moving each placed player unit still works through existing movement flow.
- Verify `Ranger`, `Mage`, and `Barbarian` representative skills through `Phase11SkillTester`.
- Temporarily place `Skeleton_Minion.prefab`, override `unitId` if needed, and verify click targeting, basic attack / D20, and enemy AI tester flows.
- Temporarily place `Skeleton_Warrior`, `Skeleton_Mage`, and `Skeleton_Rogue`, override `unitId` values if needed, and verify click targeting, basic attack / D20, and enemy AI tester flows.
- Temporarily place `Key.prefab`, bind `progressionService`, and verify pickup through the existing click interaction.
- Temporarily place `Stairs.prefab`, bind progression and selection references, then verify hover raycast, confirmation, level transition, and upgrade flow.

## Known risks
- Nested KayKit prefab references must be reimported by Unity to validate exact visual scale and orientation.
- `Skeleton_Minion.unitId` is a placeholder and can conflict if multiple instances are placed without override.
- `Skeleton_Warrior`, `Skeleton_Mage`, and `Skeleton_Rogue` also use placeholder `unitId` values and can conflict if scene instances or spawners do not override them.
- `Stairs.feedbackRenderers` is empty by design, so hover material feedback will not appear until scene-instance renderers and materials are assigned.
- Key and Stairs cross-scene service references are intentionally empty in prefab assets.
- Collider shape and visual mesh may need Inspector adjustment after visual scale is checked in Unity.
- Character `MeshCollider` components use only the body mesh, not every visual accessory mesh, so click/collision coverage should be checked in Play Mode.
- Non-convex Key and Stairs mesh colliders preserve their previous non-trigger, static click/raycast interaction style.
- Newly added player and enemy prefab visual scale/orientation should be inspected in Unity because this change only wraps the KayKit source prefab as a child and does not author a final animation or presentation setup.

## Rollback
- Remove or revert the created prefab folders/files:
  - `Assets/_BoneThrone/Prefabs/Units/Players/Fighter.prefab`
  - `Assets/_BoneThrone/Prefabs/Units/Players/Ranger.prefab`
  - `Assets/_BoneThrone/Prefabs/Units/Players/Mage.prefab`
  - `Assets/_BoneThrone/Prefabs/Units/Players/Barbarian.prefab`
  - `Assets/_BoneThrone/Prefabs/Units/Enemies/Skeleton_Minion.prefab`
  - `Assets/_BoneThrone/Prefabs/Units/Enemies/Skeleton_Warrior.prefab`
  - `Assets/_BoneThrone/Prefabs/Units/Enemies/Skeleton_Mage.prefab`
  - `Assets/_BoneThrone/Prefabs/Units/Enemies/Skeleton_Rogue.prefab`
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
