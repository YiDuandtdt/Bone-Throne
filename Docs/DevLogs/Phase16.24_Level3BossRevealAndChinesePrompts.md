# Phase 16.24 - Level 3 Boss Reveal And Chinese Prompts

Date: 2026-06-10

## Problem

Level 3 still exposed English text in bottom prompts, and the final boss room needed a first-enemy-turn boss reveal flow.

## Change

- Replaced the required boss-room progression failure reason with Chinese player-facing text.
- Added a defensive prompt fallback so pure-English gameplay reasons are converted to a generic Chinese message instead of being shown directly.
- Replaced the direct tile-click warning with Chinese text.
- Added `BossEncounterIntroController`.
- `EnemyTurnRunner` now plays the one-time boss reveal after the party ends its first player round inside an entered boss room.
- The reveal cuts through black to a camera above and diagonal from the boss, holds briefly, then starts returning the camera while enemy turn flow continues.
- Tuned the reveal pacing so the boss view holds for 2.65 seconds and the camera return takes 2.75 seconds.
- The boss BGM starts with the reveal.
- Battle HUD boss health can now expose when the reveal has played, independent of the old boss gate state.
- The boss explicitly skips its action on that first enemy turn; minions continue acting.
- Boss health bar names now use the existing Chinese unit-name mapping.

## Inspector Setup

No required manual setup. `EnemyTurnRunner` auto-adds `BossEncounterIntroController` if one is not assigned. Optional tuning fields on the controller include reveal offset, black fade, hold duration, and camera return duration.

## Play Mode Steps

1. Enter `Level_3_final`.
2. Move into the boss room.
3. End all player turns for the first player round in that room.
4. Confirm boss music starts, the boss health bar appears, and the screen cuts from black to the boss diagonal-overhead view.
5. Confirm the camera begins returning while the enemy turn proceeds.
6. Confirm the boss does not move or attack in that first enemy turn, while minions can act.
7. Confirm later enemy turns allow the boss to act normally.

## Verification

- `dotnet build .\Assembly-CSharp.csproj --no-restore` passed with 0 errors.
- `dotnet build .\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors.

## Risk

The reveal uses `Camera.main` when no camera is explicitly assigned. Scenes without a MainCamera-tagged gameplay camera will skip the camera reveal but still keep gameplay safe.

## Rollback

Revert `BossEncounterIntroController.cs`, remove the `EnemyTurnRunner` reveal hook, and restore the old `BattleHUDController.ShouldExposeBossFightRuntime()` gate if the boss reveal flow needs to be disabled.
