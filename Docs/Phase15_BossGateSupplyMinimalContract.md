# Phase 15.17 - Boss Gate / Supply Minimal Contract

## 1. Goal and Boundary

Phase 15.17 defines a minimal future contract for BossKey, BossDoor, and SupplyPoint.

This document originally did not implement runtime behavior. It narrowed the future API and prefab expectations so later scripts, prefabs, or review checklists would not expand into formal scene ownership.

Phase 15.16-15.19 combined closure later approved a narrow non-scene implementation of this contract. The implementation keeps the same scene boundary: reusable scripts and prefab assets exist, but Codex did not place or wire them in formal scenes.

Hard boundaries:

- Do not modify formal level scenes.
- Do not modify `GridTest.unity`.
- Do not create or modify C# scripts in this phase.
- Do not create or modify prefabs in this phase.
- Do not create ScriptableObject assets in this phase.
- Do not implement Boss fight, Boss AI, Boss skills, Victory, Defeat, Retry, LAN, or Networking.
- Do not change single-player free-order PlayerTurn.

Formal `Level_01`, `Level_02`, and `Level_03` scene content remains user-owned.

## 2. Contract Principles

Future BossKey, BossDoor, and SupplyPoint code should follow these principles:

- Keep each component single-purpose.
- Prefer explicit serialized references over broad scene searches.
- Avoid hard-coded scene names, room names, or floor numbers.
- Return clear success / failure results instead of silently changing state.
- Keep Boss fight behavior outside BossDoor.
- Keep Potion action behavior inside `PotionSystem`.
- Keep ordinary floor key behavior inside existing `KeyItem`.
- Avoid adding formal scene wiring to Codex tasks.

## 3. Shared Result Contract

Future components should use a small, inspectable result model.

Suggested result shape:

```csharp
public readonly struct InteractionResult
{
    public bool Success { get; }
    public string Reason { get; }
}
```

If a shared struct is considered too much for the first implementation, each component may instead expose:

- `bool Try...(...)`
- `string LastFailureReason`

Recommended failure reason strings:

- `MissingProgressionState`
- `AlreadyCollected`
- `AlreadyOpen`
- `AlreadyUsed`
- `RequiresBossKey`
- `InvalidInteractor`
- `InteractorDead`
- `NoEligiblePartyMembers`
- `DeferredBossFight`

Do not introduce localization, formal UI prompt data, or event channels in the first pass.

## 4. BossKey Minimal Contract

### Responsibility

BossKey should only represent acquisition of the future boss gate key.

It should:

- validate that the collector is a living player unit when a collector is supplied
- set boss-key progression state
- mark itself collected
- optionally deactivate itself after collection
- expose read-only collected state

It should not:

- open BossDoor directly
- start Boss fight
- modify ordinary shared key state
- handle inventory UI
- handle rewards
- handle scene loading

### Implemented Component Contract

Implemented component:

- `BossKeyItem`

Public surface:

```csharp
public bool IsCollected { get; }
public bool TryCollect();
```

Serialized fields:

- `BossGateProgressionState progressionState`
- `bool consumeOnCollect = true`
- `bool collected`
- `bool debugLogging`

The first pass uses click collection and a one-time fallback lookup for `BossGateProgressionState`. It does not validate a specific interactor because formal scene interaction ownership remains manual.

### Relationship To Existing KeyItem

`KeyItem` should remain ordinary floor-key behavior.

BossKey should not be implemented by expanding `KeyItem` unless design explicitly chooses a single shared key type for both stairs and boss gate progression.

Default recommendation:

- ordinary floor key: `KeyItem`
- boss gate key: future `BossKeyItem`

## 5. BossDoor Minimal Contract

### Responsibility

BossDoor should only gate entry to a future boss route or boss room.

It should:

- check a boss-key or progression condition
- report locked reason when requirements are not met
- open or allow passage when requirements are met
- optionally toggle visual / collider blockers
- expose read-only open state

It should not:

- start Boss fight
- own Boss AI
- own Boss skills
- reveal rooms directly unless explicitly connected by user-owned scene setup
- modify `TurnManager`
- modify `CombatSystem`
- hard-code `Level_03`
- load scenes

### Implemented Component Contract

Implemented component:

- `BossDoor`

Public surface:

```csharp
public bool IsOpened { get; }
public bool CanOpen();
public bool TryOpen();
public void SetOpenedVisual(bool isOpened);
```

Serialized fields:

- `BossGateProgressionState progressionState`
- `Collider doorBlocker`
- `GameObject lockedVisual`
- `GameObject openedVisual`
- `bool opened`
- `bool debugLogging`

The first pass toggles a local collider and optional local visual objects only. It does not start a boss encounter, reveal rooms, or load scenes.

### Relationship To Room Systems

BossDoor may sit before a user-owned Boss room trigger, but it should not replace `RoomTrigger`.

Recommended separation:

- `BossDoor`: gate condition and blocker state
- `RoomTrigger`: room entry event
- `RoomController`: room reveal / room state
- `RoomEnemyActivator`: enemy activation
- Boss fight controller: deferred

## 6. SupplyPoint Minimal Contract

### Responsibility

SupplyPoint should only provide controlled pre-boss or pre-hard-room recovery.

First implementation should favor potion restoration only.

It should:

- validate eligible party units
- grant potion count to eligible living player units
- optionally heal living player units if explicitly enabled
- mark itself used if one-shot
- expose read-only used state

It should not:

- change `PotionSystem.TryUsePotion`
- change `HealthPotionPickup`
- end or advance turns
- revive units in the first implementation
- grant loot
- start encounters
- load scenes

### Implemented Component Contract

Implemented component:

- `SupplyPoint`

Public surface:

```csharp
public bool IsUsed { get; }
public bool TryUse();
```

Serialized fields:

- `BossGateProgressionState progressionState`
- `Unit[] targetUnits`
- `int potionGrantAmount = 1`
- `bool oneShot = true`
- `bool used`
- `bool debugLogging`

Recommended first implementation:

- `potionGrantAmount = 1`
- no revive
- one-shot

If `targetUnits` is empty, the component performs a safe fallback scan for living player `Unit` objects. Inspector references remain preferred for user-owned formal scenes.

### Relationship To Potion Systems

SupplyPoint may reuse:

- `UnitPotionState.AddPotions`
- `Unit.RuntimeState.SetCurrentHp`
- `Unit.Stats.GetClampedMaxHp`

SupplyPoint should not call:

- `PotionSystem.TryUsePotion`

`PotionSystem` remains the active-turn potion action; SupplyPoint is an out-of-combat interactable preparation hook.

## 7. Minimal State Holder Options

BossKey and BossDoor need a small shared state.

Preferred future options, from smallest to largest:

1. `BossGateProgressionState` MonoBehaviour with `HasBossKey`.
2. Narrow extension to existing `LevelProgressionService`, if the project wants one progression component.
3. Future `ProgressionData` / runtime progression state, only after data assetization is approved.

Recommendation:

- Prefer option 1 for the first implementation if BossDoor and BossKey need to communicate without bloating `LevelProgressionService`.
- Do not create data assets until formal progression data is approved.

Implemented state surface:

```csharp
public bool HasBossKey { get; }
public bool IsBossDoorOpened { get; }
public bool HasUsedSupplyPoint { get; }
public bool CollectBossKey();
public bool OpenBossDoor();
public bool MarkSupplyPointUsed();
public void ResetState();
```

This state holder should not know about Boss fight, Boss AI, or formal scene loading.

## 8. Prefab Contract

Future prefabs should be reusable scene pieces, not scene-specific wiring containers.

### BossKey Prefab

Required:

- root GameObject with future `BossKeyItem`
- collider suitable for interaction
- clear visual child

Optional:

- simple highlight / prompt anchor

Forbidden:

- references to `Level_03`
- references to formal scene room objects
- Boss fight references

### BossDoor Prefab

Required:

- root GameObject with future `BossDoor`
- optional blocker child
- optional visual feedback renderers

Optional:

- locked / open materials
- prompt anchor

Forbidden:

- hard-coded room trigger references
- Boss fight references
- scene-specific Level_03 assumptions

### SupplyPoint Prefab

Required:

- root GameObject with future `SupplyPoint`
- collider suitable for interaction
- readable supply visual

Optional:

- used / unused visual feedback
- prompt anchor

Forbidden:

- automatic party scene search unless explicitly approved
- revive behavior in first pass
- scene-specific references

## 9. Review Checklist For Future Implementation

Before writing code:

- Confirm BossKey is separate from ordinary shared key.
- Confirm whether boss key is consumed on opening.
- Confirm BossDoor only gates access and does not start Boss fight.
- Confirm SupplyPoint first pass grants potion only.
- Confirm no formal scenes are edited.

Before creating prefabs:

- Confirm scripts compile.
- Confirm prefab references are local to the prefab where possible.
- Confirm no formal scene object references are saved into prefab assets.
- Confirm visuals are reusable.

After user manually places objects:

- Review missing references.
- Review collider / trigger setup.
- Review prompt / feedback expectations.
- Review whether user-owned room trigger still handles room entry.
- Do not auto-wire formal scenes.

## 10. Deferred Content

Deferred:

- Boss fight
- Boss AI
- Boss skills
- Boss room scene placement
- BossDoor scene placement
- BossKey scene placement
- SupplyPoint scene placement
- Victory / Defeat / Retry
- LAN / Networking
- formal `LevelData`, `RoomData`, or `EncounterData` instances
- save / load progression
- chest loot
- general door lock / unlock system
- revive / rest economy

## 11. Rollback

This is a documentation contract. If future implementation diverges, roll back or amend this document before writing code.

For this phase, rollback is docs-only:

```powershell
git restore -- Docs/Phase15_BossGateSupplyMinimalContract.md Docs/DevLogs/Phase15.17_BossGateSupplyMinimalContractReview.md Docs/ACTIVE_TASK.md
```
