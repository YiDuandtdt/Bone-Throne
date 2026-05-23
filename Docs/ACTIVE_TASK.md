# ACTIVE_TASK.md

## Current phase
Phase 8 - Basic Enemy AI

## Goal
Implement the minimal enemy AI loop for the Unity 6.3 LTS tactics demo.

This phase should allow an enemy Unit to choose the nearest alive player Unit, decide whether to attack or move closer, use existing movement/pathfinding/combat systems, and safely skip when no valid action is possible.

## Unity version
Unity 6000.3.10f1 / Unity 6.3 LTS series

## Allowed files
- Assets/_BoneThrone/Scripts/Core/**
- Assets/_BoneThrone/Scripts/Grid/**
- Assets/_BoneThrone/Scripts/Units/**
- Assets/_BoneThrone/Scripts/Movement/**
- Assets/_BoneThrone/Scripts/Turns/**
- Assets/_BoneThrone/Scripts/Combat/**
- Assets/_BoneThrone/Scripts/AI/**
- Assets/_BoneThrone/Scripts/Tests/**
- Docs/ACTIVE_TASK.md
- Docs/DevLogs/Phase08_BasicEnemyAI.md

## Forbidden changes
- Do not implement rooms, fog/shadow rooms, enemy spawning, keys, stairs, level switching, skill effects, cooldowns, UI HUD, LAN multiplayer, lobby, NetworkManager, or Netcode synchronization in this phase.
- Do not implement complex behavior trees, utility AI, patrol, aggro memory, group tactics, or advanced tactical scoring.
- Do not implement new combat formulas beyond the existing Phase 7 basic attack.
- Do not implement new movement rules beyond the existing Phase 5 four-direction movement.
- Do not modify Packages, ProjectSettings, Library, Temp, Obj, Logs, UserSettings, or generated IDE files.
- Do not add large art/audio/model assets.
- Do not convert gameplay classes to NetworkBehaviour.
- Do not require NetworkManager.

## Required scope
Codex may propose and implement only a small set of AI-related scripts, preferably 4-6 files.

Expected files may include:

1. EnemyTargetSelector.cs
   - Finds the nearest alive Player Unit.
   - Uses GridPosition / CurrentTile distance.
   - Does not implement threat tables or advanced scoring.

2. EnemyAIController.cs
   - Represents one enemy Unit's simple AI decision.
   - If target is in basic attack range, attacks.
   - Otherwise moves closer.
   - If no valid target/path/action exists, safely skips.
   - Does not implement behavior trees.

3. EnemyActionRunner.cs
   - Temporary runner for executing one enemy action or all enemy actions.
   - May be used through ContextMenu for Play Mode testing.
   - Does not implement full formal enemy round UI.

4. EnemyMovementPlanner.cs
   - Uses existing Pathfinder / MovementRangeFinder / UnitMover to move closer to the selected target.
   - Four-direction only.
   - Avoids occupied and unwalkable Tiles through existing GridManager.CanEnter.

5. EnemyAITester.cs
   - Temporary Play Mode / ContextMenu test helper.
   - Clearly marked as temporary/test helper.

Optional only if necessary:
6. EnemyAIResult.cs
   - Small result struct/class for logging success, attack, move, or skipped.
   - No complex state machine.

## Architecture rules
- Use namespace BoneThrone.AI for AI scripts.
- Use BoneThrone.Grid, BoneThrone.Units, BoneThrone.Movement, BoneThrone.Combat, and BoneThrone.Turns only when necessary.
- Do not reference Netcode.
- Do not inherit NetworkBehaviour.
- Keep AI independent from rooms, levels, UI, and networking.
- Reuse Phase 5 movement systems instead of writing new movement logic.
- Reuse Phase 7 CombatSystem instead of writing a second attack resolver.
- EnemyTurn can remain a placeholder; testing may use ContextMenu.
- If using UnitTurnState, mark enemy moved/acted only after valid movement/attack.
- Singleplayer must remain playable without NetworkManager.

## Acceptance tests in Unity
1. Unity 6.3 LTS opens the project without compile errors.
2. Console has no red compile errors.
3. Enemy can find the nearest alive Player Unit.
4. Enemy attacks when target is within basic attack range.
5. Enemy attack uses existing CombatSystem and D20 logic.
6. Enemy moves closer when target is out of range.
7. Enemy movement uses existing four-direction Pathfinder / UnitMover behavior.
8. Enemy does not move onto occupied or unwalkable Tiles.
9. Enemy safely skips if no alive player target exists.
10. Enemy safely skips if no path or valid move exists.
11. Dead enemy does not act.
12. Enemy action does not implement rooms, spawning, skills, UI HUD, or networking.
13. Git status does not include Library, Temp, Obj, Logs, UserSettings, or generated IDE files.

## Expected Codex output for this phase
Codex should first perform a read-only scan and output:
1. Current repository status.
2. Proposed files, limited to 4-6 AI-related files.
3. Responsibility of each file.
4. How nearest target selection works.
5. How enemy decides attack vs move.
6. How existing Movement and Combat systems are reused.
7. How failure/skip cases are handled safely.
8. Unity scene setup instructions for manual testing.
9. Risks and rollback method.

Codex must not write code until explicitly confirmed.