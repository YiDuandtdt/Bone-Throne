# Phase 15.11 - Formal Data Assetization Plan

## Summary

Phase 15.11 produced a docs-only formal data assetization plan for the project foundation validated through Phase 15.10.

This phase scanned current data assets, prefab-bound configuration, scene-bound configuration, and script default / hard-coded configuration, then planned future data asset types and safe migration order.

## Actual Added / Modified Files

Added:

- `Docs/Phase15_FormalDataAssetizationPlan.md`
- `Docs/DevLogs/Phase15.11_FormalDataAssetizationPlan.md`

Modified:

- `Docs/ACTIVE_TASK.md`

## Main Data Sources Scanned

Documentation:

- `Docs/ACTIVE_TASK.md`
- `Docs/Phase15_Plan.md`
- `Docs/Phase15_AssetInventoryAndPrefabizationPlan.md`
- `Docs/Phase15_EnvironmentPrefabizationSummary.md`
- `Docs/DevLogs/Phase15.10_GridTestSceneAssemblyKitValidation.md`

Project data:

- `Assets/_BoneThrone/Data/Skills/`
- `Assets/_BoneThrone/Data/OnlyTest/Skills/`

Project prefabs:

- `Assets/_BoneThrone/Prefabs/Units/`
- `Assets/_BoneThrone/Prefabs/Interactables/`
- `Assets/_BoneThrone/Prefabs/Environment/`

Scene / scripts reviewed as data-source references:

- `Assets/_BoneThrone/Scenes/GridTest.unity`
- `Assets/_BoneThrone/Scripts/Units/Unit.cs`
- `Assets/_BoneThrone/Scripts/Units/UnitStats.cs`
- `Assets/_BoneThrone/Scripts/Units/UnitData.cs`
- `Assets/_BoneThrone/Scripts/Units/UnitAnimationController.cs`
- `Assets/_BoneThrone/Scripts/Items/UnitPotionState.cs`
- `Assets/_BoneThrone/Scripts/Items/PotionSystem.cs`
- `Assets/_BoneThrone/Scripts/Combat/AttackRangeService.cs`
- `Assets/_BoneThrone/Scripts/Combat/DefendSystem.cs`
- `Assets/_BoneThrone/Scripts/Combat/UnitDefenseState.cs`
- `Assets/_BoneThrone/Scripts/Skills/SkillData.cs`
- `Assets/_BoneThrone/Scripts/Skills/SkillRuntime.cs`
- `Assets/_BoneThrone/Scripts/Skills/SkillSystem.cs`
- `Assets/_BoneThrone/Scripts/Levels/LevelManager.cs`
- `Assets/_BoneThrone/Scripts/Levels/LevelProgressionService.cs`
- `Assets/_BoneThrone/Scripts/Rooms/RoomController.cs`
- `Assets/_BoneThrone/Scripts/Rooms/RoomShadowController.cs`
- `Assets/_BoneThrone/Scripts/Rooms/RoomEnemyActivator.cs`
- `Assets/_BoneThrone/Scripts/Interactables/HealthPotionPickup.cs`
- `Assets/_BoneThrone/Scripts/Interactables/KeyItem.cs`
- `Assets/_BoneThrone/Scripts/Interactables/InteractableStairs.cs`
- `Assets/_BoneThrone/Scripts/AI/EnemyMovementPlanner.cs`
- `Assets/_BoneThrone/Scripts/AI/EnemyTurnRunner.cs`
- `Assets/_BoneThrone/Scripts/Turns/TurnManager.cs`

## Planned Future Data Types

The plan proposes these future data asset types:

- `CharacterData`
- `EnemyData`
- `UnitVisualData`
- `LevelData`
- `RoomData`
- `EncounterData`
- `InteractableData`
- `RewardData`
- `ProgressionData`
- `AnimationProfileData`

The plan also records that existing `SkillData` already exists and should not be modified or migrated during Phase 15.11.

## Non-Changes

This phase did not:

- create ScriptableObject assets
- create data directories
- modify C# code
- modify prefabs
- modify scenes
- modify `GridTest.unity`
- modify SkillData
- modify KayKit source assets
- modify materials
- modify animation clips or Animator Controllers
- create `Level_01`, `Level_02`, or `Level_03`
- implement Boss, BossDoor, BossKey, SupplyPoint, LAN, or formal Level progression
- change the single-player free-order PlayerTurn rule

## Next Phase

Next phase:

- Phase 15.12 - Formal Level Scene Plan

Phase 15.12 should use the data assetization plan as context, but it should remain a formal level planning phase and should not directly build `Level_01`, `Level_02`, or `Level_03` unless a later implementation phase explicitly approves it.

## Risks

- Premature data assetization may add complexity before formal levels exist.
- Prefab and data double-source conflicts can occur if ownership rules are not defined first.
- SkillData and CharacterData responsibilities can overlap around skill loadout and unlock logic.
- Scene references can be broken if scene-bound wiring is migrated too early.
- GridTest regression may be disrupted by unnecessary data plumbing.
- LevelData may be invalidated if created before formal level planning.
- AnimationProfileData could overlap with Animator Controller responsibilities if introduced too early.

## Rollback

Rollback Phase 15.11 docs changes with:

```powershell
git restore -- Docs/ACTIVE_TASK.md
Remove-Item -LiteralPath "Docs/Phase15_FormalDataAssetizationPlan.md" -Force
Remove-Item -LiteralPath "Docs/DevLogs/Phase15.11_FormalDataAssetizationPlan.md" -Force
```

Before rollback, inspect the worktree:

```powershell
git status --short
```
