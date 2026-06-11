# Phase 16.19 DevLog - Fixed Aspect Mobile Camera

## Date
2026-06-10

## Scope
Implemented a narrow runtime mobile display and camera-input pass for the project-wide 16:9 gameplay frame.

Changed scripts only:

- `Assets/_BoneThrone/Scripts/Camera/FixedAspectViewportController.cs`
- `Assets/_BoneThrone/Scripts/Camera/GridTestCameraController.cs`

Not changed:

- No scenes.
- No prefabs.
- No Player Settings assets.
- No battle rules.
- No UI button flow.
- No networking.

## Implementation
Added `FixedAspectViewportController`.

Runtime behavior:

- Auto-creates once when the first scene loads.
- Forces active game cameras without render textures into a centered 16:9 viewport.
- Adds runtime black bars outside the 16:9 viewport.
- Converts ordinary `Screen Space - Overlay` canvases to `Screen Space - Camera` using `Camera.main`, so battle/menu UI stays inside the same 16:9 gameplay frame.
- Skips `BlackCoverCanvas`, so scene transition black cover can still cover the whole physical screen.
- Re-applies on scene load, screen-size changes, camera changes, and periodic canvas scans for late-created UI.

Updated `GridTestCameraController`.

Runtime behavior:

- PC mouse-wheel zoom stays intact.
- Mobile two-finger pinch zoom uses the same zoom path and existing min/max bounds.
- Two-finger touches over UI are ignored for camera zoom.
- Existing middle-mouse pan and right-mouse rotation behavior is unchanged.

## Inspector Setup
No required Inspector setup.

Optional camera tuning on any existing `GridTestCameraController`:

- `Pinch Zoom Enabled`: keep enabled for mobile.
- `Pinch Zoom Sensitivity`: default `0.02`; increase for faster pinch zoom or lower for finer control.

## Manual Play Mode Steps
1. Open a battle scene such as `GridTest.unity`, `Level_1.unity`, or `Level_2.unity`.
2. Enter Play Mode at 1920x1080. Expected: full-screen 16:9 view with no bars.
3. Switch Game View to a wider landscape ratio such as 2400x1080. Expected: the 16:9 game view stays centered and black bars appear on the left and right.
4. Switch Game View to a narrower ratio such as 1440x1080. Expected: the 16:9 game view stays centered and black bars appear on the top and bottom.
5. Confirm HUD buttons and panels remain inside the 16:9 viewport instead of stretching into the black bars.
6. On Android landscape, tap/select/move/attack/skill as before. Expected: ordinary gameplay inputs behave the same.
7. On Android landscape, pinch with two fingers in the battle view. Expected: the camera zooms in/out with the same limits as mouse-wheel zoom.
8. Pinch over HUD buttons. Expected: camera zoom does not trigger from that UI gesture.
9. Confirm scene transitions still fade through full-screen black.

## Expected Gameplay Result
The game presents the same 16:9 composition on PC and phone.

On wide phones, players see the desktop 16:9 gameplay frame centered on screen with black side bars. The phone does not reveal extra horizontal map space, and the UI does not stretch into the device-only area.

## Risks
- Runtime conversion from `Screen Space - Overlay` to `Screen Space - Camera` may expose sorting or raycast issues on a canvas that expected overlay behavior.
- Very late-created full-screen UI may appear in overlay mode for up to the next periodic canvas scan.
- Pinch gestures can still be affected by Unity legacy touch-to-mouse simulation in other input scripts; validate that a two-finger pinch does not also trigger a gameplay tap on target devices.
- The fixed viewport intentionally reduces visible area on non-16:9 screens.

## Rollback
Revert or delete these files:

```powershell
git checkout -- Assets/_BoneThrone/Scripts/Camera/GridTestCameraController.cs
Remove-Item -LiteralPath Assets/_BoneThrone/Scripts/Camera/FixedAspectViewportController.cs
Remove-Item -LiteralPath Assets/_BoneThrone/Scripts/Camera/FixedAspectViewportController.cs.meta
Remove-Item -LiteralPath Docs/DevLogs/Phase16.19_FixedAspectMobileCamera.md
```
