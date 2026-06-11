# Phase 15.1 - Health Potion Prefab and Pickup

## Summary
Implemented the minimum reusable Health Potion pickup flow for GridTest validation.

The pickup only adds potion count to the currently selected player unit's `UnitPotionState`. It does not heal directly. Healing still uses the existing Potion button and `PotionSystem.TryUsePotion()`.

## Actual Modified Files
- `Assets/_BoneThrone/Scripts/Items/UnitPotionState.cs`
- `Assets/_BoneThrone/Scripts/Interactables/HealthPotionPickup.cs`
- `Assets/_BoneThrone/Prefabs/Interactables/HealthPotion.prefab`
- `Assets/_BoneThrone/Scenes/GridTest.unity`
- `Docs/DevLogs/Phase15.1_HealthPotionPrefabAndPickup.md`

Unity `.meta` files were added for the new script and prefab so their GUID references remain stable.

## What Changed
- Added `UnitPotionState.AddPotions(int amount)` as a minimal safe API for pickup rewards.
- Added `HealthPotionPickup`, a click-based pickup that uses the current `SelectionManager.SelectedUnit`.
- `HealthPotionPickup` follows the existing `PotionSystem` pattern: if a selected player unit lacks `UnitPotionState`, it adds the component at runtime only and does not modify player prefabs.
- Added `HealthPotion.prefab` with a project-owned wrapper around the Art bottle visual, clickable `BoxCollider`, and `HealthPotionPickup`.
- Added project-owned `HealthPotion_Red` material and applied it as a prefab override. KayKit original bottle prefabs and materials were not modified.
- Added one Health Potion prefab instance to `GridTest.unity` near the player start area for validation.

## What Did Not Change
- Did not modify `PotionSystem` or `PotionSystem.TryUsePotion()`.
- Did not modify TurnManager or Turn system behavior.
- Did not modify SkillSystem or SkillData.
- Did not modify CombatSystem or DamageResolver.
- Did not modify LAN / Networking.
- Did not add Boss content.
- Did not create formal Level scenes.
- Did not convert `GridTest.unity` into a formal level.
- Did not modify KayKit original resources.
- Did not change single-player free-order PlayerTurn.
- Did not add team-shared inventory or a new interactable framework.

## HealthPotion Inspector Setup
- `Selection Manager`: bound to the existing GridTest `SelectionManager` on the scene instance.
- `Potion Amount`: `1`.
- `Pickup Range`: `1.5`.
- `Consume On Collect`: enabled.
- `Collected`: false by default.
- Collider: non-trigger `BoxCollider` for click pickup.
- Visual: wraps `Art/KayKit - Dungeon Remastered Pack (for Unity)/Prefabs/bottle_A_labeled_green.prefab`.
- Material: overridden with project-owned `Assets/_BoneThrone/Materials/HealthPotion_Red.mat`.

## Unity 6.3 Manual Test Steps
1. Open `Assets/_BoneThrone/Scenes/GridTest.unity`.
2. Confirm the project compiles with no red Console errors.
3. Enter Play Mode.
4. Select a living player unit near the Health Potion.
5. Click the Health Potion.
6. Expected: the Health Potion disables itself and the selected unit's Potion button count increases by 1.
7. Click the same potion location again.
8. Expected: potion count does not increase again.
9. Damage the selected unit.
10. Click the existing Potion button.
11. Expected: the existing Potion action heals through `PotionSystem`, consumes one potion, consumes action, and does not automatically End Turn.
12. Confirm free-order PlayerTurn still allows selecting any alive not-ended player.
13. Confirm End Turn and EnemyTurn still behave as in the Phase 14.20 baseline.

## Risks
- If the scene `SelectionManager` reference is missing, pickup falls back to runtime lookup; if lookup fails, pickup is rejected.
- If a player prefab or scene instance lacks `UnitPotionState`, pickup adds it at runtime like `PotionSystem`; the prefab asset is not modified.
- The visual is a project-owned wrapper around an Art bottle prefab; later prefabization phases may choose a different bottle variant or final material.
- Pickup range is world-distance based and intentionally simple for Phase 15.1.

## Rollback
Rollback this phase by reverting or deleting:

- `Assets/_BoneThrone/Scripts/Items/UnitPotionState.cs`
- `Assets/_BoneThrone/Scripts/Interactables/HealthPotionPickup.cs`
- `Assets/_BoneThrone/Scripts/Interactables/HealthPotionPickup.cs.meta`
- `Assets/_BoneThrone/Prefabs/Interactables/HealthPotion.prefab`
- `Assets/_BoneThrone/Prefabs/Interactables/HealthPotion.prefab.meta`
- `Assets/_BoneThrone/Scenes/GridTest.unity`
- `Docs/DevLogs/Phase15.1_HealthPotionPrefabAndPickup.md`

No Turn, Skill, Combat, LAN, Boss, or formal Level rollback is required because those systems were not changed.
