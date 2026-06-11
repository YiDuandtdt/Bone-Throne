# Phase 16.27 - Key And Stairs Progression Reliability

## Goal

Fix the first-floor and cross-level progression experience where the player can defeat all enemies but cannot reliably pick up the floor key or understand how to enter the next level.

## Changes

- `KeyItem` now auto-resolves `LevelProgressionService`, `SelectionManager`, and `PromptView` at runtime.
- `KeyItem` now creates a runtime trigger sphere and kinematic Rigidbody when the prefab only has a non-trigger MeshCollider.
- `KeyItem` now polls nearby living player units briefly, so PC and Android can pick up the pass key by standing near it even if trigger callbacks do not fire.
- Clicking a key now falls back to the nearest living player in range instead of failing only because the currently selected unit is elsewhere.
- Visible player text now calls the ordinary floor key `通行钥匙` instead of `共用钥匙`.
- `InteractableStairs` now auto-resolves missing scene references and surfaces rejection / confirmation messages through `PromptView`.
- `LevelProgressionService` now ignores null entries in `requiredClearedRooms` instead of permanently blocking stairs progression.
- Enemy axe basic attack SFX extra delay changed from `1.0s` to `0.5s`; player Barbarian and Boss attack SFX timing are unchanged.

## Three-Level Check

- `Level_1` contains one `Key` prefab instance, one `Stairs` prefab instance, and transitions to `Level_2`.
- `Level_1` has one null required-room entry; runtime code now logs and ignores it.
- `Level_2` contains one `Key` prefab instance, one `Stairs` prefab instance, and transitions to `Level_3_final`.
- `Level_3_final` contains `Stairs`, skips the ordinary pass-key requirement, requires the configured boss room, and transitions to `EndMenu`.

## Validation

- `dotnet build .\Assembly-CSharp.csproj --no-restore` passed with existing CS0649 warnings and 0 errors.
- `dotnet build .\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 warnings and 0 errors.
- `rg` found no remaining visible `共用钥匙` text under runtime assets.

## Play Mode Steps

1. Open `Level_1`, defeat all enemies, then move any living player near the key.
2. Confirm the key is collected, pickup VFX/SFX play, and the bottom prompt says the party obtained the `通行钥匙`.
3. Click stairs once and confirm the bottom prompt asks for a second click.
4. Click stairs again and confirm the scene loads `Level_2`.
5. Repeat the same key and stairs flow in `Level_2`.
6. In `Level_3_final`, defeat the required boss room, then use stairs to reach `EndMenu`.

## Risks

- Key pickup is now intentionally more forgiving: any living player inside the small pass-key radius can collect it without requiring an exact click.
- Runtime trigger creation changes only Play Mode objects, not the prefab asset, but it depends on player units having normal transforms and living state.

## Rollback

Revert:

- `Assets/_BoneThrone/Scripts/Interactables/KeyItem.cs`
- `Assets/_BoneThrone/Scripts/Interactables/InteractableStairs.cs`
- `Assets/_BoneThrone/Scripts/Levels/LevelProgressionService.cs`
- `Assets/_BoneThrone/Scripts/UI/PromptView.cs`
- `Assets/_BoneThrone/Scripts/Combat/CombatLog.cs`
- `Assets/_BoneThrone/Scripts/Combat/CombatSystem.cs`
