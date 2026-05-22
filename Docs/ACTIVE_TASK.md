# ACTIVE_TASK.md

## Current phase
Phase 4 - Unit System and Data Model

## Goal
Implement the minimal unit system and data model for the Unity 6.3 LTS tactics demo.

This phase should allow player/enemy units to store basic stats, faction, runtime HP/death state, and occupy/release grid tiles.

## Unity version
Unity 6000.3.10f1 / Unity 6.3 LTS series

## Allowed files
- Assets/_BoneThrone/Scripts/Core/**
- Assets/_BoneThrone/Scripts/Grid/**
- Assets/_BoneThrone/Scripts/Units/**
- Assets/_BoneThrone/Scripts/Data/**
- Assets/_BoneThrone/Scripts/Tests/**
- Docs/ACTIVE_TASK.md
- Docs/DevLogs/Phase04_UnitSystem.md

## Forbidden changes
- Do not implement player input control, unit selection, BFS movement range, A* pathfinding, real movement animation, combat, D20 attacks, skills, enemy AI, room progression, UI HUD, LAN multiplayer, lobby, stairs, keys, or level switching in this phase.
- Do not implement turn management yet.
- Do not create full character balance tables or final ScriptableObject assets unless explicitly confirmed.
- Do not modify Packages, ProjectSettings, Library, Temp, Obj, Logs, UserSettings, or generated IDE files.
- Do not add large art/audio/model assets.
- Do not convert Unit or gameplay classes to NetworkBehaviour.
- Do not require NetworkManager.

## Required scope
Codex may propose and implement only a small set of unit-related scripts, preferably 4-6 files.

Expected files may include:
1. UnitFaction.cs
   - Defines Player, Enemy, Neutral, None.

2. UnitStats.cs
   - Serializable stats data: max HP, move range placeholder, attack modifier placeholder, defense placeholder, base damage placeholder.
   - No combat formula yet.

3. UnitRuntimeState.cs
   - Runtime HP, alive/dead state, hasMoved/hasActed placeholders if needed.
   - No turn system yet.

4. Unit.cs
   - MonoBehaviour attached to a unit object.
   - Stores unit id, display name, role id, faction, stats, runtime state.
   - Can be placed on a Tile through GridPosition/GridManager.
   - Can release occupied tile on death.
   - Does not move, attack, or act.

5. UnitData.cs
   - Optional ScriptableObject definition for future character/enemy data.
   - Only class definition, no actual asset creation unless confirmed.

6. UnitPlacementTester.cs
   - Optional temporary test helper to place a unit on a tile and test occupancy/death release.
   - Clearly marked as debug/test helper.

## Architecture rules
- Use namespace BoneThrone.Units for unit scripts.
- Use BoneThrone.Grid types for tile occupancy.
- Unit must not depend on Netcode, NetworkManager, LAN, Lobby, or NetworkBehaviour.
- Unit must not directly implement movement/pathfinding/combat/turn flow.
- Keep unit state simple and inspectable in Unity.
- Unit should be usable later by both singleplayer and host-authoritative multiplayer.

## Acceptance tests in Unity
1. Unity 6.3 LTS opens the project without compile errors.
2. Console has no red compile errors.
3. A test scene can contain several Tile objects and several Unit objects.
4. Four player units can be assigned to four different tiles.
5. Occupied tiles return CanEnter false.
6. A unit death/release test clears its tile occupancy.
7. No movement, BFS, A*, combat, turn system, skills, AI, UI, or networking is implemented.
8. Git status does not include Library, Temp, Obj, Logs, UserSettings, or generated IDE files.

## Expected Codex output for this phase
Codex should first perform a read-only scan and output:
1. Current repository status.
2. Proposed files, limited to 4-6 unit-related files.
3. Responsibility of each file.
4. How the unit system uses GridManager/Tile without implementing movement.
5. Unity scene setup instructions for manual testing.
6. Risks and rollback method.

Codex must not write code until explicitly confirmed.