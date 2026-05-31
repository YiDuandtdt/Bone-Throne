# Phase 15.5 - Interactable Prefab Completion Pass

## Summary

Phase 15.5 first batch reviewed the existing Interactable prefabs and added two visual-only future interactable candidates.

This phase stayed within the approved Interactable prefab scope:

- `Assets/_BoneThrone/Prefabs/Interactables/`
- `Docs/DevLogs/Phase15.5_InteractablePrefabCompletionPass.md`

No C# code, scenes, materials, SkillData, KayKit source assets, Turn, Combat, Skill, Potion, LAN, Environment, Character, Weapon, Animation, BossDoor, BossKey, or SupplyPoint systems were modified.

## Modified / Added Files

Added:

- `Assets/_BoneThrone/Prefabs/Interactables/Chest.prefab`
- `Assets/_BoneThrone/Prefabs/Interactables/Chest.prefab.meta`
- `Assets/_BoneThrone/Prefabs/Interactables/Doorway.prefab`
- `Assets/_BoneThrone/Prefabs/Interactables/Doorway.prefab.meta`
- `Docs/DevLogs/Phase15.5_InteractablePrefabCompletionPass.md`

Reviewed only:

- `Assets/_BoneThrone/Prefabs/Interactables/HealthPotion.prefab`
- `Assets/_BoneThrone/Prefabs/Interactables/Key.prefab`
- `Assets/_BoneThrone/Prefabs/Interactables/Stairs.prefab`

## Existing Interactable Review

### HealthPotion

Path:

- `Assets/_BoneThrone/Prefabs/Interactables/HealthPotion.prefab`

Review result:

- Keeps existing `HealthPotionPickup`.
- Has `potionAmount: 1`.
- Has `pickupRange: 1.5`.
- Has `consumeOnCollect` enabled.
- Has a `BoxCollider` for click / pickup interaction.

No changes were made.

### Key

Path:

- `Assets/_BoneThrone/Prefabs/Interactables/Key.prefab`

Review result:

- Keeps existing `KeyItem`.
- Uses shared key progression through `LevelProgressionService`.
- Does not implement multi-key IDs, BossKey, or key inventory.
- Has a `MeshCollider`.

No changes were made.

### Stairs

Path:

- `Assets/_BoneThrone/Prefabs/Interactables/Stairs.prefab`

Review result:

- Keeps existing `InteractableStairs`.
- Uses `LevelProgressionService`.
- Keeps two-click confirmation behavior.
- Does not implement formal `Level_01`, `Level_02`, or `Level_03` scene switching.
- Has a `MeshCollider`.

No changes were made.

## Created Visual-Only Candidates

### Chest

Source:

- `Assets/_BoneThrone/Art/KayKit - Dungeon Remastered Pack (for Unity)/Prefabs/chest.prefab`

Target:

- `Assets/_BoneThrone/Prefabs/Interactables/Chest.prefab`

Role:

- Visual-only future interactable candidate.

Implementation notes:

- Root name set to `Chest`.
- No script added.
- No loot logic added.
- No reward logic added.
- No inventory logic added.
- No material override added.
- Source KayKit prefab was not modified.
- Text inspection found GameObject, Transform, MeshFilter, and MeshRenderer components only.
- No Collider component was found, so future click area / collider refinement is required before gameplay use.

### Doorway

Source:

- `Assets/_BoneThrone/Art/KayKit - Dungeon Remastered Pack (for Unity)/Prefabs/wall_doorway.prefab`

Target:

- `Assets/_BoneThrone/Prefabs/Interactables/Doorway.prefab`

Role:

- Visual-only future doorway / door interactable candidate.

Implementation notes:

- Root name set to `Doorway`.
- No script added.
- No open / close logic added.
- No lock / unlock logic added.
- No BossDoor logic added.
- No material override added.
- Source KayKit prefab was not modified.
- Text inspection found GameObject, Transform, MeshFilter, and MeshRenderer components only.
- No Collider component was found, so future click area / collider refinement is required before gameplay use.

## Skipped / Deferred Candidates

No selected source prefab was missing.

Deferred by scope:

- BossDoor
- BossKey
- Chest loot / reward / inventory behavior
- Door open / close / lock / unlock behavior
- SupplyPoint system
- Formal Level scene progression

SupplyPoint was deferred because this repository does not currently have a clearly approved SupplyPoint source prefab and existing SupplyPoint gameplay script within the Phase 15.5 scope. The full Boss Door / Boss Key / SupplyPoint preparation belongs to Phase 15.16.

## Non-Changes

This phase did not modify:

- KayKit original resources
- C# code
- Scenes
- Materials
- SkillData
- `GridTest.unity`
- TurnManager / Turn system
- CombatSystem
- SkillSystem / SkillData
- PotionSystem
- LAN / Networking
- Environment prefabs
- Characters
- Weapons / Equipment
- Animations / Animator Controllers
- Formal Level scenes

## Unity Play Mode

Unity Play Mode was not run because this first batch only reviewed existing prefabs and added visual-only prefab assets without runtime gameplay changes.

Recommended Unity 6.3 checks:

1. Open `Assets/_BoneThrone/Prefabs/Interactables/`.
2. Open `HealthPotion.prefab` and confirm `HealthPotionPickup`, `potionAmount`, `pickupRange`, and collider are present.
3. Open `Key.prefab` and confirm `KeyItem` and collider are present.
4. Open `Stairs.prefab` and confirm `InteractableStairs` and collider are present.
5. Open `Chest.prefab` and `Doorway.prefab` in Prefab Mode and confirm they render as visual-only candidates.
6. Confirm `Chest.prefab` and `Doorway.prefab` have no scripts, no loot, no lock, no BossDoor, and no SupplyPoint logic.
7. If prefabs are temporarily dragged into a scene for visual inspection, do not save the scene.

## Risks

- `Chest.prefab` and `Doorway.prefab` currently have no Collider, so they are not ready for click or trigger gameplay.
- Chest / Doorway behavior could easily expand into systems that are out of scope; keep gameplay behavior deferred unless explicitly approved later.
- Existing `Key.prefab` and `Stairs.prefab` depend on `LevelProgressionService` references in scene context.
- `HealthPotion.prefab` should continue to be validated against the Phase 15.1 pickup flow before broader use.

## Rollback

Rollback this first batch by removing the new visual-only prefab candidates and this DevLog:

```powershell
Remove-Item -LiteralPath "Assets/_BoneThrone/Prefabs/Interactables/Chest.prefab" -Force
Remove-Item -LiteralPath "Assets/_BoneThrone/Prefabs/Interactables/Chest.prefab.meta" -Force
Remove-Item -LiteralPath "Assets/_BoneThrone/Prefabs/Interactables/Doorway.prefab" -Force
Remove-Item -LiteralPath "Assets/_BoneThrone/Prefabs/Interactables/Doorway.prefab.meta" -Force
Remove-Item -LiteralPath "Docs/DevLogs/Phase15.5_InteractablePrefabCompletionPass.md" -Force
```

Before rollback, run:

```powershell
git status --short
```

Only remove files created by Phase 15.5.
