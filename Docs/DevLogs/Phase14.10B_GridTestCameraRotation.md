# Phase 14.10-B - GridTest Camera Right Mouse Rotation

Date: 2026-05-28

## Scope

Phase 14.10-B extends the existing `GridTestCameraController` with right mouse camera rotation for the current `GridTest.unity` integration scene.

This phase is limited to GridTest camera controls. It does not introduce Cinemachine or a formal production camera system.

## Files changed

- Modified `Assets/_BoneThrone/Scripts/Camera/GridTestCameraController.cs`.
- Added `Docs/DevLogs/Phase14.10B_GridTestCameraRotation.md`.

`Assets/_BoneThrone/Scenes/GridTest.unity` was not modified. The new fields have code defaults, and no scene serialized-field update was required in this pass.

## Right mouse conflict check

Current right mouse usage:

- `UIActionModeController` uses `Input.GetMouseButtonDown(1)` to cancel active Move / Basic Attack / Skill targeting.
- `PlayerMovementController` does not use right mouse.
- `SelectionManager` does not read mouse input.
- `GridInputTester` uses left mouse only.
- Existing `GridTestCameraController` used middle mouse and mouse wheel before this phase.

Conflict handling:

- `UIActionModeController` was not modified.
- Right mouse down in the camera controller only records the drag start position.
- Camera rotation begins only after horizontal movement exceeds `rotationStartThresholdPixels`.
- This preserves the right-click cancel-targeting semantic for light right-clicks.
- The camera controller does not know about or reference targeting state.

## Implementation

`GridTestCameraController` now supports:

- `Input.GetMouseButtonDown(1)` to record right-drag start and previous mouse position.
- `Input.GetMouseButton(1)` to rotate only after the horizontal movement threshold is exceeded.
- `Input.GetMouseButtonUp(1)` to clear right-drag state.
- Horizontal mouse delta for yaw rotation.
- Vertical mouse delta for pitch rotation.
- Pitch clamped between `minPitch` and `maxPitch`.
- Pivot priority:
  1. `rotationPivot`
  2. `zoomPivot`, when `reuseZoomPivotForRotation` is true
  3. `camera.position + camera.forward * fallbackPivotDistance`, with `pivot.y = 0`
- Stable pitch control by maintaining `currentPitch` from the current camera rotation and clamping it.
- Roll forced to `0`.
- UI blocking through the existing `blockWhenPointerOverUI` rule.

The script still does not listen to left mouse and does not call gameplay services.

## Inspector defaults

New fields:

| Field | Default |
| --- | --- |
| `rotationEnabled` | `true` |
| `rotationSpeed` | `0.2` |
| `invertRotation` | `false` |
| `rotationStartThresholdPixels` | `3` |
| `rotationPivot` | `None` |
| `reuseZoomPivotForRotation` | `true` |
| `fallbackPivotDistance` | `18` |
| `verticalRotationEnabled` | `true` |
| `verticalRotationSpeed` | `0.15` |
| `invertVerticalRotation` | `false` |
| `minPitch` | `35` |
| `maxPitch` | `75` |

Existing fields remain unchanged:

- `controlsEnabled = true`
- `blockWhenPointerOverUI = true`
- `panSpeed = 0.02`
- `zoomSpeed = 2`
- `minOrthographicSize = 3`
- `maxOrthographicSize = 14`
- `perspectiveMinDistance = 8`
- `perspectiveMaxDistance = 35`
- `invertDrag = false`
- `useForwardDollyForPerspective = true`
- `zoomPivot = None`

## Systems not modified

No changes were made to:

- `UIActionModeController`
- `PlayerMovementController`
- `SelectionManager`
- `GridInputTester`
- `CombatSystem`
- `SkillSystem`
- `DamageResolver`
- `SkillEffectExecutor`
- Unit / Enemy / Room / Level logic
- Prefabs
- `SkillData` assets
- KayKit original resources
- `Skeleton_Rogue`
- `Skeleton_Golem`
- Ranger visual

## Play Mode test steps

1. Open `Assets/_BoneThrone/Scenes/GridTest.unity`.
2. Confirm there are no compile errors.
3. Confirm `GridTestCameraController` is attached to `Main Camera`.
4. Enter Play Mode.
5. Right-click lightly while in targeting mode.
6. Confirm targeting cancels and the camera does not noticeably rotate.
7. Hold right mouse and drag left / right.
8. Confirm the camera rotates horizontally by yaw.
9. Hold right mouse and drag up / down.
10. Confirm the camera pitches up / down within `minPitch = 35` and `maxPitch = 75`.
11. Confirm the camera does not flip, drill through the ground, or become an upward-looking view.
12. Confirm camera roll remains `0`.
13. Hover over BattleHUD UI and confirm right-drag rotation is blocked.
14. Confirm middle mouse drag still pans.
15. Confirm mouse wheel still zooms.
16. Confirm left-click selection still works.
17. Confirm Move mode still works.
18. Confirm Basic Attack targeting still works.
19. Confirm Skill Slot 0 targeting still works.
20. Confirm HUD buttons still work.
21. Confirm Console has no new red errors.

## Known risks

- Right mouse is shared with targeting cancel, so the drag threshold may need tuning if light right-clicks rotate the camera.
- The fallback pivot is based on camera forward distance and may feel different from a dedicated scene pivot.
- Pitch clamp defaults are intentionally conservative: `35` to `75`.
- If vertical rotation feels reversed, tune `invertVerticalRotation`.
- If `GridTestCameraController` is not attached to `Main Camera`, the feature will not run.
- If future UI adds right-drag behavior, keep `blockWhenPointerOverUI = true`.

## Rollback

To roll back this phase:

- Revert the right-mouse rotation additions in `Assets/_BoneThrone/Scripts/Camera/GridTestCameraController.cs`.
- If scene fields are later serialized, restore only the `GridTestCameraController` fields on `GridTest.unity` Main Camera.
- Remove `Docs/DevLogs/Phase14.10B_GridTestCameraRotation.md`.

Do not use broad reset commands if other Phase 14 work is still in progress.
