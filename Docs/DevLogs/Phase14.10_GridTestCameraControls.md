# Phase 14.10 - GridTest Camera Controls

Date: 2026-05-28

## Scope

Phase 14.10 adds a lightweight camera-control script for the current `GridTest.unity` integration scene.

This phase is limited to GridTest camera controls:

- Middle mouse drag pans the camera.
- Mouse wheel zooms the camera.
- The controller does not listen to left click.
- The controller does not listen to right click.
- The controller does not call gameplay services.

## Files changed

- Added `Assets/_BoneThrone/Scripts/Camera/GridTestCameraController.cs`.
- Added `Docs/DevLogs/Phase14.10_GridTestCameraControls.md`.

`Assets/_BoneThrone/Scenes/GridTest.unity` was not modified in this pass because the new script's Unity `.meta` GUID is not available under the current allowed file list. Attaching a script component into a scene YAML without a stable script GUID would risk a missing-script reference.

## Functionality

`GridTestCameraController` is a scene-local camera utility in namespace `BoneThrone.CameraControls`.

Responsibilities:

- Auto-use the same GameObject `Camera`.
- Fallback to `Camera.main` if needed.
- Process middle mouse drag for pan.
- Process mouse wheel for zoom.
- Block controls while the pointer is over UI by default.
- Preserve camera rotation.
- Keep pan movement from drifting camera height.

The script does not modify:

- Combat logic.
- Skill logic.
- Unit logic.
- Enemy logic.
- Room logic.
- Level logic.
- `DamageResolver`.
- `SkillEffectExecutor`.
- `CombatSystem.TryBasicAttack`.
- `SkillSystem.TryUseSkill`.
- `PlayerMovementController`.
- `UIActionModeController`.
- `BattleHUDController`.
- Prefabs.
- `SkillData` assets.
- KayKit original resources.
- `Skeleton_Rogue`.
- `Skeleton_Golem`.
- Ranger visual.

## Inspector configuration

Recommended conservative GridTest values:

| Field | Value |
| --- | --- |
| `controlsEnabled` | `true` |
| `blockWhenPointerOverUI` | `true` |
| `panSpeed` | `0.02` |
| `zoomSpeed` | `2` |
| `minOrthographicSize` | `3` |
| `maxOrthographicSize` | `14` |
| `perspectiveMinDistance` | `8` |
| `perspectiveMaxDistance` | `35` |
| `invertDrag` | `false` |
| `useForwardDollyForPerspective` | `true` |
| `zoomPivot` | `None` |

Current `GridTest.unity` uses a perspective Main Camera around `(-6, 18, -6)`, so the intended zoom path is forward dolly with distance clamps.

## Scene setup note

After Unity imports the new script and generates its `.meta`, attach `GridTestCameraController` to:

- `Assets/_BoneThrone/Scenes/GridTest.unity`
- Object: `Main Camera`

Do not modify gameplay objects, prefabs, ScriptableObject assets, or other scene bindings as part of this setup.

## Play Mode test steps

1. Open `Assets/_BoneThrone/Scenes/GridTest.unity`.
2. Confirm Unity imports `GridTestCameraController.cs` without compile errors.
3. Attach `GridTestCameraController` to `Main Camera` if it is not already attached.
4. Enter Play Mode.
5. Hold middle mouse and drag.
6. Confirm the camera pans and no unit is selected or moved.
7. Scroll the mouse wheel.
8. Confirm the camera zooms in and out within clamp limits.
9. Hover the pointer over BattleHUD UI and verify camera controls are blocked.
10. Select a player with left click and confirm selection still works.
11. Test Move mode and confirm tile targeting still works.
12. Test Basic Attack mode and confirm enemy targeting still works.
13. Test Skill Slot 0 and confirm skill targeting still works.
14. Confirm CombatLog, Enemy Floating HP Bar, Room trigger, Key, Stairs, LevelUp, and Enemy AI still behave as before.
15. Confirm Console has no new red errors.

## Known risks

- The script is not attached to `GridTest.unity` until Unity has a valid `.meta` GUID for the new script.
- Perspective dolly can feel too fast or too slow and may need Inspector tuning.
- If `zoomPivot` remains unset, distance clamps use world origin as the pivot.
- Future scrollable UI panels should continue using `blockWhenPointerOverUI = true`.

## Rollback

To roll back this phase:

- Remove `Assets/_BoneThrone/Scripts/Camera/GridTestCameraController.cs`.
- Remove the generated `.meta` file if Unity creates one for the script.
- If the component was attached later, remove `GridTestCameraController` from `GridTest.unity` Main Camera.
- Remove `Docs/DevLogs/Phase14.10_GridTestCameraControls.md`.

Do not use broad reset commands if other Phase 14 work is still in progress.
