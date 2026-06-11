# Phase 16.3 - Ranger Arrow Impact Presentation

Date: 2026-06-06

## What Changed

- Added `RangerHitPresentationConfig` on `Assets/_BoneThrone/Prefabs/Units/Players/Ranger.prefab`.
- Ranger basic attacks now spawn a short impact arrow that flies from half a tile in front of the target into the body and stays embedded for 2 enemy turns.
- Ranger skills now also spawn an embedded arrow and attach target effects:
  - `ranger_precision_shot` -> `1.prefab`
  - `ranger_quick_shot` -> `2.prefab`
  - `ranger_piercing_arrow` -> `3.prefab`
- Added `RangerEmbeddedArrow` runtime helper for short flight + timed embedded cleanup.
- Added `TurnManager.EnemyTurnStarted` event so embedded arrows can count enemy turns without scene wiring.

## Inspector Setup

- Open `Assets/_BoneThrone/Prefabs/Units/Players/Ranger.prefab`.
- Select the root `Ranger` object.
- Find `Ranger Hit Presentation Config`.
- Tweak these fields as needed:
  - `Basic Attack Arrow Prefab`
  - `Skill Arrow Prefab`
  - `Arrow Start Distance From Target`
  - `Arrow Flight Duration`
  - `Arrow Rotation Offset Euler`
  - `Arrow World Scale`
  - `Embedded Arrow Local Offset`
  - `Embedded Arrow Lifetime Enemy Turns`
  - `Precision / Quick Shot / Piercing Arrow Local Offset`
  - `Precision / Quick Shot / Piercing Arrow Local Euler Angles`
  - `Precision / Quick Shot / Piercing Arrow Local Scale`

## Play Mode Validation

1. Enter a battle test scene with the Ranger and at least one enemy.
2. Use Ranger basic attack.
3. Confirm the arrow appears from half a tile in front of the target, flies into the enemy, and stays embedded.
4. End turns until 2 enemy turns pass.
5. Confirm the embedded arrow disappears on the second enemy turn.
6. Use `ranger_precision_shot`, `ranger_quick_shot`, and `ranger_piercing_arrow`.
7. Confirm each skill attaches `1`, `2`, and `3` respectively to the struck enemy.

## Expected Result

- Ranger ranged presentation is now visible and tied to actual hit timing.
- Embedded arrows persist briefly on the enemy body instead of disappearing immediately.
- Skill-specific target effects can be tuned directly from the Ranger prefab.

## Risks

- The default arrow orientation may still need a small Y rotation tweak depending on camera angle and chosen arrow mesh.
- If target body height differs a lot between enemies, `Embedded Arrow Local Offset` and skill effect offsets may need adjustment.
- These visuals are attached to the primary target only. `ranger_piercing_arrow` secondary damage does not attach a second effect.

## Rollback

- Remove `RangerHitPresentationConfig` from `Assets/_BoneThrone/Prefabs/Units/Players/Ranger.prefab`.
- Revert:
  - `Assets/_BoneThrone/Scripts/Combat/CombatSystem.cs`
  - `Assets/_BoneThrone/Scripts/Skills/SkillSystem.cs`
  - `Assets/_BoneThrone/Scripts/Turns/TurnManager.cs`
- Delete:
  - `Assets/_BoneThrone/Scripts/Units/RangerHitPresentationConfig.cs`
  - `Assets/_BoneThrone/Scripts/Units/RangerEmbeddedArrow.cs`
