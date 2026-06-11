# Phase 16.2 - Mouse Click, Cursor, And Invalid Feedback

## What changed

- Added `BTMouseFeedbackController` as a runtime-only UI helper.
- It plays `BTSFX_Mouse_Click` at end of frame when a left mouse click did not already trigger `BTSFX_Button_Click` or `BTSFX_Invalid_Action`.
- It switches between the normal and interactive cursor textures from `Assets/_BoneThrone/Art/2D/通用/鼠标` in Editor Play Mode.
- Locked level-select buttons now remain clickable, play `BTSFX_Invalid_Action`, and shake their locked overlay slightly.
- Locked skill slots now remain clickable for invalid feedback and play `BTSFX_Invalid_Action` instead of `BTSFX_Button_Click`.
- Click-rejected key, health potion, and supply point interactions now play `BTSFX_Invalid_Action` from their mouse-click entry points.

## Inspector setup

- No scene or prefab edits were made.
- Editor Play Mode auto-loads the cursor textures by asset path.
- For build validation, either add a scene/prefab `BTMouseFeedbackController` with `normalCursorTexture` and `interactiveCursorTexture` assigned, or place matching textures under `Resources/BoneThroneCursor`.

## Play Mode checks

1. Open the start menu and hover normal/interactive UI controls.
2. Click empty screen space and confirm `BTSFX_Mouse_Click` plays once.
3. Click a normal button and confirm only `BTSFX_Button_Click` plays.
4. Click a locked level and confirm `BTSFX_Invalid_Action` plays and the lock overlay shakes.
5. In battle, select a unit with a locked skill slot and click that slot.
6. Confirm the locked skill plays `BTSFX_Invalid_Action`, keeps disabled visuals, and still shows the existing prompt.
7. Hover clickable world interactables such as units, keys, boss keys, boss doors, stairs, potions, and supply points.
8. Click a key/potion/supply point while its click rule is invalid and confirm `BTSFX_Invalid_Action` plays.

## Expected result

- Ordinary non-button left clicks use `BTSFX_Mouse_Click`.
- Button clicks keep `BTSFX_Button_Click`.
- Locked-but-clickable UI uses `BTSFX_Invalid_Action`.
- Clickable world pickups/supply points use `BTSFX_Invalid_Action` when their click attempt is rejected.
- Locked level overlays provide a small visual shake without scene wiring.

## Risks

- Runtime-created cursor controller can only auto-load non-Resources cursor textures in Editor Play Mode via `AssetDatabase`.
- If a locked overlay is not under or over the button as expected, disabling its raycast target should let the level button receive clicks, but manually authored UI hierarchy should still be checked.
- Existing scenes/prefabs were not edited, so build cursor texture assignment needs a manual prefab/scene or Resources step.

## Rollback

- Remove `Assets/_BoneThrone/Scripts/UI/BTMouseFeedbackController.cs`.
- Revert the local changes to `BTAudioService.cs`, `StartMenuController.cs`, and `SkillBarView.cs`.
- Delete this DevLog file.
