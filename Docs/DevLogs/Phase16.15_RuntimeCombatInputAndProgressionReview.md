# Phase 16.15 Runtime Combat Input And Progression Review

Date: 2026-06-07

## Scope

- Fixed ordinary enemy behavior so a minion that moves into basic attack range can attack after the movement visual completes.
- Delayed Barbarian axe basic attack SFX by 0.8 seconds through `CombatSystem`.
- Improved click and camera input reliability without editing scene content.
- Reviewed current level-up/progression flow.

## Implementation Notes

- `EnemyTurnRunner` now waits for `UnitMover` movement completion after an ordinary enemy move, then asks `EnemyAIController` to attempt a follow-up basic attack only if the enemy is in range and still has an action.
- `UnitMover` exposes a read-only `IsMoving(Unit)` query so enemy turn sequencing does not depend only on event timing.
- `CombatSystem` now allows turn-gated basic attacks for the faction that matches the current phase, instead of assuming only player attacks during `PlayerTurn`.
- `CombatSystem` delays only `RoleId.Barbarian` + `AxeChop` basic attack SFX, leaving boss-specific axe presentation untouched.
- `PlayerMovementController` ignores world clicks while the pointer is over uGUI and falls back to finding a player unit on the clicked tile if the unit collider raycast misses.
- `GridTestCameraController` processes mouse-wheel zoom before UI blocking and uses a local forward fallback pivot when no explicit zoom pivot is assigned.

## Inspector Setup

- No required scene or prefab edits.
- Optional tuning: `CombatSystem.axeBasicAttackSfxDelay` defaults to `0.8`.
- Existing `GridTestCameraController.blockWhenPointerOverUI` can remain enabled; it now blocks drag/rotation over UI but not wheel zoom.

## Play Mode Verification

- Enemy move-then-attack: place a normal enemy outside basic attack range but close enough to move adjacent to a player, end player turn, and confirm it walks to range and attacks in the same enemy turn.
- Barbarian SFX: select the Barbarian/axe warrior, basic attack an enemy, and confirm the axe chop SFX lands about 0.8 seconds later than before.
- Character click: click player units from slightly awkward angles and while the HUD is visible; selection should not be stolen by UI clicks and should still work when the cursor lands on the occupied tile.
- Camera wheel: scroll over the battlefield and over HUD areas; perspective camera should zoom consistently even when no zoom pivot is assigned.
- Level-up: collect the shared key, satisfy required room clearance, enter next level, and confirm living player units gain one level, max HP increases, HP refills, and potions/current HP carry into the next scene snapshot.

## Risks

- Ordinary minions now have the intended move-plus-action cadence, so enemy turns are more dangerous than before.
- The clicked-tile unit fallback depends on each unit keeping `CurrentTile` accurate.
- The zoom fallback uses the camera forward direction; explicit `zoomPivot` remains preferable for heavily cinematic cameras.

## Rollback

- Revert `EnemyAIController`, `EnemyTurnRunner`, `UnitMover`, `CombatSystem`, `PlayerMovementController`, and `GridTestCameraController` changes from this dev log.
- Remove this dev log if the runtime behavior is rolled back.
