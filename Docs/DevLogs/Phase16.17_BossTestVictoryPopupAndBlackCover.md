# Phase 16.17 - Boss Test Victory Popup And Black Cover

## What Changed

- Updated `Assets/_BoneThrone/Scripts/Core/GameOutcomeService.cs`
  - Added `ForceShowCurrentOutcomePopup()` so `boss_test` can explicitly show a result popup even when the normal victory popup toggle is off.
- Updated `Assets/_BoneThrone/Scripts/UI/BattleHUDController.cs`
  - Direct victory/defeat test buttons now force-show the matching popup after setting the outcome.
- Updated `Assets/_BoneThrone/Scripts/UI/GameResultPanelController.cs`
  - Returning to `StartMenu` now goes through a black scene cover instead of loading directly.
- Added `Assets/_BoneThrone/Scripts/UI/SceneBlackCoverService.cs`
  - Creates a persistent full-screen black overlay and keeps it visible through scene switching.

## Inspector Setup

- No manual Inspector rewiring is required for this fix.
- Existing victory/defeat popup prefabs already point at the imported art under:
  - `Assets/_BoneThrone/Art/2D/胜利与失败界面/胜利`
  - `Assets/_BoneThrone/Art/2D/胜利与失败界面/失败`

## Play Mode Steps

1. Open `Assets/_BoneThrone/Scenes/boss_test.unity`.
2. Enter Play Mode.
3. Click `直接胜利`.
4. Confirm the victory popup appears and uses the imported victory artwork.
5. Click the result button to return to the main menu.
6. Confirm the screen stays black during the transition and the boss scene no longer flashes.
7. Repeat with `直接失败`.

## Expected Result

- `直接胜利` now reliably opens the victory popup.
- Returning to the main menu from victory/defeat result UI no longer reveals the old battle scene for a frame.

## Risks

- The black cover is a persistent runtime overlay, so if a future flow reuses it incorrectly, it could remain visible longer than intended.

## Rollback

1. Revert:
   - `Assets/_BoneThrone/Scripts/Core/GameOutcomeService.cs`
   - `Assets/_BoneThrone/Scripts/UI/BattleHUDController.cs`
   - `Assets/_BoneThrone/Scripts/UI/GameResultPanelController.cs`
   - `Assets/_BoneThrone/Scripts/UI/SceneBlackCoverService.cs`
2. Remove this DevLog file.
