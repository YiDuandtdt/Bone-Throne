# Phase 16.28 - Level Transition VFX, Audio, and Skill Progression Fix

## Scope

Narrow runtime fix for the Level_1 -> Level_2 stairs transition freeze, battle BGM restart behavior, and direct level-select skill progression.

No formal scene or prefab was modified.

## Changes

- `BTInteractionVfxService`
  - Rechecks spawned VFX objects after coroutine waits before reading particle systems or destroying the object.
  - Prevents `MissingReferenceException` when a level-up or key VFX is destroyed by a scene load before its delayed cleanup runs.
- `BTAudioService`
  - Scene-loaded battle BGM now force-restarts when entering a scene, even if the previous scene used the same battle track.
  - End-menu scene BGM now stops unless victory/defeat one-shot music is already playing.
- `LevelProgressionService`
  - When no saved party snapshot exists, player units are raised to the current scene's default level based on `MenuProgressionState.GetLevelIndexForScene`.
  - This keeps direct Level_2 / Level_3 entry consistent with skills unlocked by unit level.
- `StartMenuController`
  - Clears in-memory party progression snapshots before starting a new game or loading a selected level from the menu.
  - Prevents previous test-run HP, potion, or level state from leaking into a direct level entry.

## Validation

- `dotnet build .\Assembly-CSharp.csproj --no-restore`
  - Passed with 0 errors.
  - Existing serialized-field CS0649 warnings remain.
- `dotnet build .\Assembly-CSharp-Editor.csproj --no-restore`
  - Passed with 0 warnings and 0 errors.

## Play Mode Checklist

1. Enter Level_1, collect the floor pass key, use stairs, and confirm Level_2 loads without `MissingReferenceException`.
2. Listen for battle BGM restarting after Level_2 loads.
3. Continue Level_2 -> Level_3_final and confirm the same no-freeze / BGM restart behavior.
4. From the start menu, directly enter Level_2 and Level_3_final after unlocking them.
5. Confirm player skills match the expected unit level for that level.

## Risk

- The scene default level fallback only runs when no saved party snapshot exists, so normal stairs progression should keep its HP/potion carry-over behavior.
- If a future menu flow needs persistent party resume, it should use a real save snapshot instead of the current in-memory progression cache.

## Rollback

Revert these files:

- `Assets/_BoneThrone/Scripts/Audio/BTInteractionVfxService.cs`
- `Assets/_BoneThrone/Scripts/Audio/BTAudioService.cs`
- `Assets/_BoneThrone/Scripts/Levels/LevelProgressionService.cs`
- `Assets/_BoneThrone/Scripts/UI/StartMenuController.cs`
- `Docs/DevLogs/Phase16.28_LevelTransitionVfxAudioAndSkillProgressionFix.md`
