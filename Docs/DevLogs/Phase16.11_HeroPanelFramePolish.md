# Phase 16.11 - Hero Panel Frame Polish

## Scope

- Adjusted the four left-side hero status panels through `HeroPanelView`.
- Kept each panel root `RectTransform` unchanged so existing HUD placement and size remain stable.
- Added runtime-only visual children for a slightly inset content area, a thin transparent-white inner border, and a popup-colored outer frame.

## Implementation

- `HeroPanelView` now auto-builds frame visuals in `Awake` and reuses them afterward.
- Existing text and health bar children are moved under an inset runtime content root.
- The root panel background uses the current turn-popup panel color: `rgba(0.16, 0.10, 0.08, 0.94)`.
- The content area keeps the previous hero-panel dark style: `rgba(0.12, 0.09, 0.08, 0.92)`.
- The inner white border is subtle by default and becomes brighter when the bound unit is selected.

## Validation

- Ran `dotnet build Assembly-CSharp.csproj`: 0 errors, existing Inspector serialization warnings only.
- Ran `git diff --check -- Assets/_BoneThrone/Scripts/UI/HeroPanelView.cs`: no whitespace issues.

## Unity Verification

1. Open a battle scene or `boss_test`.
2. Enter Play Mode.
3. Confirm the four left hero blocks keep their old outer positions and sizes.
4. Confirm the dark content area is slightly inset with two thin frame layers.
5. Select each player unit and confirm that unit's inner white frame becomes brighter.

## Risk And Rollback

- Risk: because the frame is generated at runtime, prefab inspection will not show the generated child hierarchy until Play Mode.
- Rollback: revert `Assets/_BoneThrone/Scripts/UI/HeroPanelView.cs` and this DevLog.
