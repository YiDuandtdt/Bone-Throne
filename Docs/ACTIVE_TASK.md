# ACTIVE_TASK.md

## Current phase
Phase 3 - Grid and Coordinate System

## Goal
Implement the minimal square grid and coordinate system for the Unity 6.3 LTS tactics demo.

This phase should make it possible to represent grid positions, register tiles, detect walkability/occupancy, and click a tile to read its grid coordinate.

## Unity version
Unity 6000.3.10f1 / Unity 6.3 LTS series

## Allowed files
- Assets/_BoneThrone/Scripts/Core/**
- Assets/_BoneThrone/Scripts/Grid/**
- Assets/_BoneThrone/Scripts/Tests/**
- Docs/ACTIVE_TASK.md
- Docs/DevLogs/Phase03_GridCoordinateSystem.md

## Forbidden changes
- Do not implement player units, enemies, combat, turns, skills, room progression, AI, LAN multiplayer, lobby, UI HUD, inventory, stairs, keys, or level switching in this phase.
- Do not implement BFS movement range or A* pathfinding yet.
- Do not implement real character movement yet.
- Do not create complex procedural maps.
- Do not modify Packages, ProjectSettings, Library, Temp, Obj, Logs, UserSettings, or generated IDE files.
- Do not add large art/audio/model assets.
- Do not convert any gameplay class to NetworkBehaviour.
- Do not require NetworkManager.

## Required scope
Codex may propose and implement only a small set of grid-related scripts, preferably 3-6 files.

Expected files may include:
1. GridPosition.cs
   - Immutable or value-style grid coordinate struct.
   - Stores X and Y.
   - Supports equality and simple helper methods.

2. Tile.cs
   - MonoBehaviour attached to a tile object.
   - Stores GridPosition, walkable flag, occupancy flag or occupant id placeholder.
   - Exposes methods to initialize and query tile state.

3. GridManager.cs
   - Registers tiles.
   - Finds tile by GridPosition.
   - Checks if a position is inside the grid, walkable, and unoccupied.
   - Does not generate complex maps yet unless explicitly minimal.

4. GridInputTester.cs
   - Temporary test helper for clicking a tile and logging its coordinate.
   - Must be clearly marked as testing/debug helper.
   - Should live under Grid or Tests depending on Codex proposal.

Optional only if needed:
5. GridDebugHighlighter.cs
   - Minimal color/material feedback for clicked or hovered tile.
   - No complex range display.

## Architecture rules
- Use namespace BoneThrone.Grid or BoneThrone.Tests.
- Keep this phase independent from combat, turns, units, rooms, and networking.
- Do not reference Netcode.
- Do not inherit NetworkBehaviour.
- Do not call future gameplay systems.
- Grid should be usable later by both singleplayer and host-authoritative multiplayer.
- Keep implementation simple and inspectable in Unity.

## Acceptance tests in Unity
1. Unity 6.3 LTS opens the project without compile errors.
2. Console has no red compile errors.
3. A test scene can contain several tile objects with Tile components.
4. GridManager can register those tiles.
5. Clicking a tile logs or displays its grid coordinate.
6. Walkable and occupied checks return expected values.
7. No unit movement, BFS, A*, combat, turns, or networking is implemented.
8. Git status does not include Library, Temp, Obj, Logs, UserSettings, or generated IDE files.

## Expected Codex output for this phase
Codex should first perform a read-only scan and output:
1. Current repository status.
2. Proposed files, limited to 3-6 grid-related files.
3. Responsibility of each file.
4. How the grid system stays independent from units, combat, turns, and networking.
5. Unity scene setup instructions for manual testing.
6. Risks and rollback method.

Codex must not write code until explicitly confirmed.