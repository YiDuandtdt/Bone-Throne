# Phase 15.17 - BossDoor / BossKey / SupplyPoint Minimal Contract Review

## Summary

Phase 15.17 reviewed the Phase 15.16 BossDoor / BossKey / SupplyPoint preparation plan and narrowed it into a minimal future contract.

Phase 15.16-15.19 combined closure later implemented this contract as non-scene scripts and reusable prefabs.

## Scope

Allowed files:

- `Docs/Phase15_BossGateSupplyMinimalContract.md`
- `Docs/DevLogs/Phase15.17_BossGateSupplyMinimalContractReview.md`
- `Docs/ACTIVE_TASK.md`

The original Phase 15.17 pass intentionally did not modify:

- formal level scenes
- `GridTest.unity`
- C# scripts
- prefab assets
- SkillData
- KayKit source assets
- ScriptableObject assets
- ProjectSettings
- Packages

The later combined closure added the approved minimal runtime files without modifying formal scenes.

## Inputs Reviewed

Reviewed:

- `Docs/ACTIVE_TASK.md`
- `Docs/Phase15_BossDoorBossKeySupplyPointPreparation.md`
- `Docs/DevLogs/Phase15.16_BossDoorBossKeySupplyPointPreparation.md`

The Phase 15.15 scene ownership boundary remains active:

- formal `Level_01`, `Level_02`, and `Level_03` scene content is user-owned
- Codex must not create, modify, wire, or extend formal level scenes

## Contract Results

BossKey minimal contract:

- future component candidate: `BossKeyItem`
- public shape: `Collected`, `TryCollect(Unit collector)`
- responsibility: collect boss gate token only
- not responsible for door opening, Boss fight, inventory, ordinary shared key, or scene loading

BossDoor minimal contract:

- future component candidate: `BossDoor`
- public shape: `IsOpen`, `CanOpen(Unit interactor, out string reason)`, `TryOpen(Unit interactor)`, `SetOpen(bool open)`
- responsibility: gate access only
- not responsible for Boss fight, Boss AI, room reveal, turn/combat, or scene loading

SupplyPoint minimal contract:

- future component candidate: `SupplyPoint`
- public shape: `Used`, `TryUse(Unit interactor)`, `ApplyToParty(Unit[] partyUnits)`
- first-pass responsibility: one-shot potion restoration
- not responsible for `PotionSystem` action behavior, revive, loot, encounter start, or scene loading

Shared state option:

- recommended first future option: a small `BossGateProgressionState`
- keep it separate from Boss fight and formal scene loading
- avoid data assets until formal progression data is explicitly approved

## Key Decisions

- Do not expand `KeyItem` for BossKey by default.
- Do not make BossDoor hard-coded to `Level_03`.
- Do not make SupplyPoint call `PotionSystem.TryUsePotion`.
- Do not use Codex to place or wire these objects into formal level scenes.
- Let the user manually place future prefabs, then use Codex only for checklist / review.

## Deferred

Still deferred:

- Boss fight
- Boss AI
- Boss skills
- Boss room scene placement
- BossDoor / BossKey / SupplyPoint placement
- Victory / Defeat / Retry
- LAN / Networking
- formal `LevelData`, `RoomData`, or `EncounterData` instances
- save / load progression
- chest loot
- general door lock / unlock system
- revive / rest economy

## Validation

This was a docs-only phase.

Validation performed:

- Confirmed target Phase 15.17 docs did not already exist before creation.
- Updated `ACTIVE_TASK.md` to record Phase 15.17 output.
- No Unity-Skills scene operation was needed or used.

## Rollback

To roll back Phase 15.17 documentation changes:

```powershell
git restore -- Docs/ACTIVE_TASK.md Docs/Phase15_BossGateSupplyMinimalContract.md Docs/DevLogs/Phase15.17_BossGateSupplyMinimalContractReview.md
```
