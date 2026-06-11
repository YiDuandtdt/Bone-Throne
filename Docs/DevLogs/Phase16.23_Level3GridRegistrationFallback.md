# Phase 16.23 - Level 3 Grid Registration Fallback

Date: 2026-06-10

## Problem

`Level_3_final` could show the battle HUD and enter move or skill targeting, but movement range, skill range, and target execution failed.

## Root Cause

`GridManager.initialTiles` in `Level_3_final.unity` was imported as null scene references (`fileID: 0`). The scene still contains Tile components, but the runtime grid dictionary had no registered positions. `MovementRangeFinder`, `Pathfinder`, and skill range targeting all depend on `GridManager`.

## Change

- `GridManager.RegisterInitialTiles()` now keeps the serialized tile list as the primary source.
- If the serialized list registers zero valid tiles, GridManager scans loaded scene Tile components once in `Awake` and registers them.
- Null entries in the serialized list are skipped during bulk registration to avoid warning spam from imported scenes with broken references.

## Inspector Setup

No manual setup required. Existing Level 1 and Level 2 GridManager objects continue using their serialized `Initial Tiles` lists. Imported scenes with broken or empty lists can leave `Auto Find Scene Tiles If Initial Tiles Missing` enabled.

## Verification

- `Level_3_final.unity` GridManager `Initial Tiles` has 952 entries, all null scene references.
- `Level_3_final.unity` contains 1751 Tile components with 1751 unique coordinates.
- `dotnet build .\Assembly-CSharp.csproj --no-restore` passed with 0 errors.
- `dotnet build .\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors.

## Play Mode Steps

1. Open `Assets/_BoneThrone/Scenes/Level_3_final.unity`.
2. Enter Play Mode.
3. Select a player unit, click Move, and confirm reachable tiles are highlighted.
4. Click a reachable tile and confirm the unit moves.
5. Select an attack or skill and confirm range/target highlights appear and valid targets can be used.

## Risk

If a future imported scene contains duplicate Tile coordinates, GridManager will keep rejecting duplicates with the existing warning path. The first discovered tile for a coordinate wins.

## Rollback

Revert `Assets/_BoneThrone/Scripts/Grid/GridManager.cs` to remove the scene Tile fallback. Then manually reconnect every Tile reference in each affected scene's GridManager `Initial Tiles` list.
