# Phase 15.16 - Boss Door / Boss Key / Supply Point Preparation

## Summary

Phase 15.16 began as a documentation-only preparation pass for future BossDoor, BossKey, and SupplyPoint work.

Phase 15.16-15.19 combined closure later implemented the minimal non-scene script and prefab support approved by the user. Formal scene placement is still deferred to manual user work.

## Scope

Allowed files:

- `Docs/Phase15_BossDoorBossKeySupplyPointPreparation.md`
- `Docs/DevLogs/Phase15.16_BossDoorBossKeySupplyPointPreparation.md`
- `Docs/ACTIVE_TASK.md`

The original Phase 15.16 pass intentionally did not modify:

- formal level scenes
- `GridTest.unity`
- C# scripts
- prefab assets
- SkillData
- KayKit source assets
- ScriptableObject assets
- ProjectSettings
- Packages

The later combined closure added:

- `BossGateProgressionState`
- `BossKeyItem`
- `BossDoor`
- `SupplyPoint`
- `BossKey.prefab`
- `BossDoor.prefab`
- `SupplyPoint.prefab`

## Context

Phase 15.15 changed formal scene ownership:

- `Level_01`, `Level_02`, and `Level_03` formal scene content is user-owned.
- Codex must not create, modify, wire, or extend formal level scenes.
- Codex may provide planning, checklists, review notes, DevLogs, and narrow non-scene fixes when explicitly approved.

Phase 15.16 follows that boundary.

## Current System Findings

Reviewed related systems:

- `KeyItem`
- `InteractableStairs`
- `LevelProgressionService`
- `LevelManager`
- `HealthPotionPickup`
- `PotionSystem`
- `UnitPotionState`
- `RoomController`
- `RoomTrigger`
- `RoomShadowController`
- `RoomEnemyActivator`

Reviewed relevant prefab availability:

- `HealthPotion`
- `Key`
- `Stairs`
- `Chest`
- `Doorway`
- `Skeleton_Golem_Boss`
- regular enemy and player prefabs

Key conclusions:

- `KeyItem` is suitable for ordinary shared floor key behavior only.
- `InteractableStairs` is suitable for current floor progression only.
- `LevelProgressionService` is minimal and should not be expanded into broad Boss progression until rules stabilize.
- `HealthPotionPickup` and `PotionSystem` should remain distinct from SupplyPoint rules.
- `Chest` and `Doorway` remain visual-only / future candidates.
- `Skeleton_Golem_Boss` remains a Boss placeholder only.

## Planning Results

BossKey:

- Future BossKey should likely be distinct from ordinary `KeyItem`.
- Future `BossKeyItem` may be appropriate.
- BossKey state should wait until BossDoor requirements are stable.

BossDoor:

- Future BossDoor should be a narrow gate component.
- It should check BossKey or progression condition, block entry when locked, and provide feedback.
- It should not own Boss fight, Boss AI, Turn, or Combat logic.
- It should not be hard-coded to `Level_03`.

SupplyPoint:

- Future SupplyPoint should be a controlled pre-Boss / pre-hard-room recovery point.
- Minimal first behavior should favor potion restoration.
- Heal, revive, or rest behavior should remain later design decisions.
- It should not alter current `PotionSystem` or `HealthPotionPickup` behavior.

## Deferred

Deferred content remains:

- Boss fight
- Boss AI
- Boss skills
- Boss room scene placement
- BossDoor placement
- BossKey placement
- SupplyPoint placement
- Victory / Defeat / Retry
- LAN / Networking
- formal `LevelData`, `RoomData`, or `EncounterData` instances
- save / load progression
- chest loot
- door lock / unlock beyond future BossDoor gate

## Risks

- BossDoor / BossKey implementation can bind unstable manual level structure too early.
- Ordinary key and BossKey responsibilities can become confused if `KeyItem` is expanded prematurely.
- BossDoor responsibilities can overlap `RoomTrigger` and `RoomShadowController`.
- SupplyPoint can destabilize resource economy.
- Formal scene edits by Codex can create large uncontrolled diffs.

## Validation

The original Phase 15.16 pass was docs-only. The combined closure was validated separately.

Validation performed:

- Confirmed target docs did not already exist before creation.
- Updated `ACTIVE_TASK.md` to point to the next phase and preserve Phase 15.15 scene ownership boundaries.
- No Unity-Skills scene operation was needed or used.

## Rollback

To roll back Phase 15.16 documentation changes:

```powershell
git restore -- Docs/ACTIVE_TASK.md Docs/Phase15_BossDoorBossKeySupplyPointPreparation.md Docs/DevLogs/Phase15.16_BossDoorBossKeySupplyPointPreparation.md
```
