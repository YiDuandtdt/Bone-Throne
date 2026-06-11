# Phase 16.21 - Android HUD Touch And Slider Fix

Date: 2026-06-10

## Scope

Fixed three Android-facing runtime issues without editing formal level scenes:

- Battle HUD left hero panel backing now normalizes its loose backdrop images to left anchors and stable sizes at runtime.
- Mobile single-touch camera drag now requires the lower-left camera joystick region, leaving other single touches for battle movement/tap/drag input.
- The shared settings page prefab now contains a `FillLimit` child under each audio slider `Fill`, with each handle parented under that limit rect, so the transparent track overhang is not used as the drag range.

## Changed Files

- `Assets/_BoneThrone/Scripts/UI/BattleHUDController.cs`
- `Assets/_BoneThrone/Scripts/Camera/GridTestCameraController.cs`
- `Assets/_BoneThrone/Scripts/UI/SettingsPageView.cs`
- `Assets/_BoneThrone/Prefabs/UI/Common/UI_SettingsPage.prefab`

## Unity Inspector Setup

No manual Inspector wiring is required.

The new mobile HUD fields on `BattleHUDController` default to:

- `mobileHudSafeLayoutEnabled = true`
- `showMobileCameraJoystick = true`
- `mobileCameraJoystickViewportCenter = (0.17, 0.16)`
- `mobileCameraJoystickRadiusViewportHeight = 0.085`

`GridTestCameraController` uses the same joystick center and radius defaults for touch hit testing.

The settings prefab now has this slider hierarchy for both SFX and BGM:

- `Slider / Fill / FillLimit / Handle`

`Slider.fillRect` is intentionally left empty. The purple bar is refreshed by `Image.fillAmount`, while drag range is controlled by the `Handle` parent rect (`FillLimit`). This keeps manually edited `Fill` and `FillLimit` RectTransforms from being driven back by Unity's `Slider` visual update.

## Play Mode / Build Validation

1. Build or run Android.
2. Enter a battle scene that uses the main `BattleHUD`.
3. Confirm the left hero portrait backing stays aligned in landscape across aspect ratios.
4. Touch-drag outside the lower-left joystick area: the camera should not rotate.
5. Touch-drag inside the lower-left joystick area: the camera should rotate.
6. Open settings and drag BGM/SFX sliders: the handle should be constrained to the prefab `FillLimit` width, not the transparent visual overhang.

## Verification

- `dotnet build .\Assembly-CSharp.csproj --no-restore`: passed, 0 errors.
- `dotnet build .\Assembly-CSharp-Editor.csproj --no-restore`: passed, 0 errors.

## Risks

- The joystick is created at runtime, so prefab edit mode will not show it until Play Mode or Android runtime.
- Slider `FillLimit` is now part of `UI_SettingsPage.prefab`; the script only reuses that existing rect and does not create it at runtime or overwrite its layout.
- The HUD safe layout only normalizes the main battle HUD backdrop set, avoiding the single placeholder in the boss test HUD.

## Rollback

Revert the three changed script files listed above and delete this devlog.
