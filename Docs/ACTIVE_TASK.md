# ACTIVE_TASK.md

## Current phase
Phase 5 - Selection, BFS Range and A* Movement

## Goal
Implement the minimal playable movement loop for the Unity 6.3 LTS tactics demo.

This phase should allow selecting a player unit, showing reachable tiles with BFS, clicking a valid target tile, moving the unit along an A* path, and updating tile occupancy.

## Unity version
Unity 6000.3.10f1 / Unity 6.3 LTS series

## Allowed files
- Assets/_BoneThrone/Scripts/Core/**
- Assets/_BoneThrone/Scripts/Grid/**
- Assets/_BoneThrone/Scripts/Units/**
- Assets/_BoneThrone/Scripts/Movement/**
- Assets/_BoneThrone/Scripts/Tests/**
- Docs/ACTIVE_TASK.md
- Docs/DevLogs/Phase05_SelectionMovement.md

## Forbidden changes
- Do not implement turn management, D20 combat, skills, enemy AI, room progression, fog/shadow rooms, stairs, keys, level switching, UI HUD, LAN multiplayer, lobby, or NetworkManager in this phase.
- Do not implement attack actions or damage.
- Do not implement skill targeting or cooldowns.
- Do not modify Packages, ProjectSettings, Library, Temp, Obj, Logs, UserSettings, or generated IDE files.
- Do not add large art/audio/model assets.
- Do not convert gameplay classes to NetworkBehaviour.
- Do not require NetworkManager.

## Required scope
Codex may propose and implement only a small set of movement-related scripts, preferably 4-6 files.

Expected files may include:
1. SelectionManager.cs
   - Selects a Unit by clicking it.
   - Stores current selected Unit.
   - Does not implement UI.

2. MovementRangeFinder.cs
   - Uses BFS to calculate reachable GridPosition values within Unit move range.
   - Uses GridManager.CanEnter.
   - Does not move the unit.

3. Pathfinder.cs
   - Uses A* to find a path from current Unit position to target GridPosition.
   - Four-direction movement only.
   - Does not implement animation.

4. UnitMover.cs
   - Moves a Unit along a path.
   - Updates Tile occupancy.
   - Can initially use simple transform movement or instant stepping.

5. PlayerMovementController.cs
   - Temporary Play Mode controller for clicking selected unit and target tile.
   - Clearly marked as temporary/test controller.
   - Does not implement turn system or networking.

Optional only if needed:
6. MovementDebugHighlighter.cs
   - Highlights reachable tiles with simple material/color changes.
   - No complex UI or VFX.

## Architecture rules
- Use namespace BoneThrone.Movement for movement scripts.
- Use BoneThrone.Grid and BoneThrone.Units.
- Do not reference Netcode.
- Do not inherit NetworkBehaviour.
- Keep movement independent from combat, turns, skills, AI, rooms, and networking.
- Movement must update Tile occupancy correctly.
- Four-direction movement only.

## Acceptance tests in Unity
1. Unity 6.3 LTS opens the project without compile errors.
2. Console has no red compile errors.
3. A player Unit can be selected.
4. Reachable tiles are calculated by BFS.
5. Clicking a reachable tile moves the Unit to that tile.
6. Original tile is released and target tile becomes occupied.
7. Clicking an unreachable or occupied tile does not move the Unit.
8. Movement uses four-direction rules only.
9. No combat, turn system, skills, AI, UI, or networking is implemented.
10. Git status does not include Library, Temp, Obj, Logs, UserSettings, or generated IDE files.

## Expected Codex output for this phase
Codex should first perform a read-only scan and output:
1. Current repository status.
2. Proposed files, limited to 4-6 movement-related files.
3. Responsibility of each file.
4. How movement uses GridManager, Tile, and Unit.
5. How BFS and A* are bounded to this phase.
6. Unity scene setup instructions for manual testing.
7. Risks and rollback method.

Codex must not write code until explicitly confirmed.