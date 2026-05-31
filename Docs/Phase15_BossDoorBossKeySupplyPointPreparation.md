# Phase 15.16 - Boss Door / Boss Key / Supply Point Preparation

## 1. Goal and Boundary

Phase 15.16 defines non-scene preparation for future BossDoor, BossKey, and SupplyPoint work.

This phase is documentation only:

- Do not modify formal level scenes.
- Do not modify `GridTest.unity`.
- Do not create or modify prefabs.
- Do not create or modify C# scripts.
- Do not modify SkillData.
- Do not modify KayKit source assets.
- Do not create ScriptableObject assets.
- Do not implement Boss fight, Boss AI, Boss skills, Victory, Defeat, Retry, LAN, or Networking.
- Do not change the single-player free-order PlayerTurn rule.

From Phase 15.15 onward, formal `Level_01`, `Level_02`, and `Level_03` scene content is user-owned. Codex may prepare documents, checklists, reviews, and narrow non-scene fixes only when explicitly approved.

## 2. Current Related Systems

### KeyItem

`KeyItem` is a minimal shared key pickup.

Current responsibility:

- Detect collection by a living player unit or click.
- Call `LevelProgressionService.CollectSharedKey`.
- Mark itself collected.
- Optionally deactivate itself after collection.

Current limits:

- No key type.
- No key ID.
- No inventory.
- No BossKey state.
- No UI prompt beyond Debug logs.
- No door binding.

### InteractableStairs

`InteractableStairs` is the current floor progression interactable.

Current responsibility:

- Query `LevelProgressionService.CanEnterNextLevel`.
- Require optional second-click confirmation.
- Call `LevelProgressionService.TryEnterNextLevel`.
- Provide simple hover material feedback.

Current limits:

- No BossDoor behavior.
- No Boss room gate.
- No formal UI confirmation panel.
- No formal scene loading.
- No Victory / Defeat / Retry flow.

### LevelProgressionService

`LevelProgressionService` owns the current minimal progression state.

Current responsibility:

- Track one shared key.
- Check required cleared rooms.
- Ask `LevelManager` to switch to next placeholder level.
- Level up living party members once after placeholder transition.

Current limits:

- No inventory.
- No rewards.
- No BossKey.
- No BossDoor.
- No SupplyPoint.
- No formal scene loading.
- No networking.

### LevelManager

`LevelManager` is a placeholder floor switch helper.

Current responsibility:

- Track current placeholder level index.
- Optionally activate level roots.
- Optionally move party units to spawn points.

Current limits:

- No formal scene loading.
- No formal room ownership.
- No Boss progression ownership.
- Depends on scene wiring when used with level roots and spawn points.

### HealthPotionPickup

`HealthPotionPickup` is the current potion pickup interactable.

Current responsibility:

- Require a selected living player unit.
- Check pickup range.
- Add potion count through `UnitPotionState`.
- Optionally deactivate itself after collection.

Current limits:

- Does not heal immediately.
- Does not revive.
- Does not rest the party.
- Does not refill every party member.
- Does not own SupplyPoint rules.

### PotionSystem

`PotionSystem` handles the player's self-use potion action.

Current responsibility:

- Validate player turn action permission.
- Consume one potion from `UnitPotionState`.
- Heal the unit.
- Mark the unit as acted.
- Play potion animation and combat log feedback.

Current limits:

- Does not refill potion counts.
- Does not provide rest logic.
- Does not revive.
- Should not be changed just to support SupplyPoint placement.

### Room Systems

`RoomController`, `RoomTrigger`, `RoomShadowController`, and `RoomEnemyActivator` provide room reveal and room enemy activation support.

Current responsibility:

- Reveal room shadow on entry.
- Activate pre-assigned enemies.
- Track simple room state.
- Check whether assigned enemies are dead.

Current limits:

- No door lock / unlock rules.
- No BossDoor condition checks.
- No Boss fight logic.
- No reward or supply handling.

### Existing Prefab Candidates

Current interactable prefabs:

- `HealthPotion`
- `Key`
- `Stairs`
- `Chest`
- `Doorway`

Current boss placeholder:

- `Skeleton_Golem_Boss`

Planning rules:

- `Chest` remains visual-only / future loot candidate.
- `Doorway` remains visual-only / future door candidate.
- `Skeleton_Golem_Boss` remains Boss / heavy boss placeholder only, not a normal enemy.

## 3. BossKey Planning

Future BossKey responsibility:

- Represent a special progression token for a Boss route or Boss room gate.
- Avoid confusing ordinary floor progression keys with boss progression.
- Provide pickup feedback.
- Update a future boss progression state.
- Remain independent from Boss fight execution.

Recommended direction:

- Do not extend `KeyItem` prematurely.
- Prefer a future `BossKeyItem` if BossDoor rules require a distinct key type.
- Do not reuse the current shared key unless design explicitly decides that the demo has only one key type.
- Add explicit boss key state only after BossDoor requirements are stable.

Possible future state options:

- `LevelProgressionService.hasBossKey`, if keeping progression small.
- A future `ProgressionState` / `ProgressionData` if progression grows.
- A future `InteractableData` key type if data assetization becomes necessary.

This phase does not:

- implement BossKey
- modify `KeyItem`
- create `BossKeyItem`
- create `BossKey.prefab`
- add inventory
- add UI prompt code

## 4. BossDoor Planning

Future BossDoor responsibility:

- Check BossKey or future progression condition.
- Block player entry when conditions are not met.
- Allow entry when conditions are met.
- Provide prompt / feedback for locked and unlocked states.
- Coordinate with user-placed room triggers or room roots without owning Boss fight logic.

BossDoor should not:

- start Boss fight directly
- own Boss AI
- own Boss skills
- modify `TurnManager`
- modify `CombatSystem`
- hard-code itself to `Level_03`
- require Codex to wire formal scenes

Recommended future shape:

- `BossDoor.cs` as a small interactable / gate component.
- Serialized references for a progression source and optional blocker / visuals.
- Explicit methods such as `CanOpen`, `TryOpen`, and `SetOpen`.
- Optional prompt hook after UI prompt rules stabilize.

Integration should wait until:

- the user manually stabilizes Boss route / Boss room placement
- BossKey rules are known
- door visual and collider expectations are known
- prompt / feedback expectations are known

## 5. SupplyPoint Planning

Future SupplyPoint responsibility:

- Provide a controlled pre-Boss or pre-hard-room resource recovery point.
- Potentially restore potion count.
- Potentially provide a one-time heal.
- Potentially support partial rest or revive later, if design approves it.

Recommended minimal future behavior:

- one-shot interaction
- add potions to living player units
- optional small heal only after resource economy is reviewed
- no revive in the first implementation

Systems that may be reused:

- `UnitPotionState.AddPotions`
- current Unit runtime HP APIs
- current selection / interaction patterns from interactables

Systems that should not be changed for the first pass:

- `PotionSystem`
- `HealthPotionPickup`
- `TurnManager`
- `CombatSystem`

This phase does not:

- implement SupplyPoint
- modify `PotionSystem`
- modify `HealthPotionPickup`
- create `SupplyPoint.cs`
- create `SupplyPoint.prefab`
- place SupplyPoint in any scene

## 6. Future File Scope

If a later phase explicitly approves implementation, possible files may include:

- `Assets/_BoneThrone/Scripts/Interactables/BossKeyItem.cs`
- `Assets/_BoneThrone/Scripts/Interactables/BossDoor.cs`
- `Assets/_BoneThrone/Scripts/Interactables/SupplyPoint.cs`
- `Assets/_BoneThrone/Prefabs/Interactables/BossKey.prefab`
- `Assets/_BoneThrone/Prefabs/Interactables/BossDoor.prefab`
- `Assets/_BoneThrone/Prefabs/Interactables/SupplyPoint.prefab`
- future docs and DevLogs

This phase creates none of those runtime files.

## 7. Relationship To Manual Formal Level Work

Formal level scenes are user-owned.

Future intended workflow:

1. Codex prepares scripts / prefabs only when explicitly approved.
2. User manually places BossDoor, BossKey, and SupplyPoint in formal scenes.
3. Codex may review screenshots, hierarchy notes, missing reference reports, or checklists.
4. Codex must not directly edit `Level_01`, `Level_02`, `Level_03`, or `GridTest` for formal scene integration.

This keeps scene layout decisions with the user and prevents another large, hard-to-control scene diff.

## 8. Deferred Content

The following remain deferred:

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
- door lock / unlock beyond the future BossDoor gate

## 9. Risks

- Implementing BossDoor before the user finalizes Boss room layout may hard-bind unstable scene structure.
- Reusing `KeyItem` for BossKey can blur ordinary floor progression and boss progression.
- BossDoor can overlap with `RoomTrigger` / `RoomShadowController` if responsibilities are not kept narrow.
- SupplyPoint can distort resource economy if it restores too much before encounter difficulty is known.
- Adding data assets too early may create double-source conflicts with prefabs and scene references.
- Codex scene edits can create large, noisy diffs and conflict with the user's manual level ownership.

## 10. Recommended Next Step

If this plan is accepted, the next implementation phase should still avoid formal scenes.

Recommended sequence:

1. Decide whether BossKey is separate from ordinary shared key.
2. Draft minimal API contracts for `BossKeyItem`, `BossDoor`, and `SupplyPoint`.
3. Implement only the smallest approved script / prefab support.
4. Let the user manually place objects in formal scenes.
5. Use Codex only for review and checklist validation.
