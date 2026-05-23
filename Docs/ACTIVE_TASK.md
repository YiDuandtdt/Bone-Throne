# ACTIVE_TASK.md

## Current phase
Phase 6 - Turn System and Fixed Action Order

## Goal
Implement the minimal turn system for the Unity 6.3 LTS tactics demo.

This phase should allow the game to track player/enemy turn phases, track whether each Unit has moved and acted during the current round, restrict repeated movement within a turn, and prepare the fixed multiplayer order: Fighter -> Ranger -> Mage -> Barbarian -> Enemy Turn.

## Unity version
Unity 6000.3.10f1 / Unity 6.3 LTS series

## Allowed files
- Assets/_BoneThrone/Scripts/Core/**
- Assets/_BoneThrone/Scripts/Grid/**
- Assets/_BoneThrone/Scripts/Units/**
- Assets/_BoneThrone/Scripts/Movement/**
- Assets/_BoneThrone/Scripts/Turns/**
- Assets/_BoneThrone/Scripts/Tests/**
- Docs/ACTIVE_TASK.md
- Docs/DevLogs/Phase06_TurnSystem.md

## Forbidden changes
- Do not implement D20 combat, attack damage, skills, cooldowns, enemy AI behavior, room progression, fog/shadow rooms, stairs, keys, level switching, UI HUD, LAN multiplayer, lobby, NetworkManager, or Netcode synchronization in this phase.
- Do not implement actual attack resolution.
- Do not implement skill targeting or skill effects.
- Do not modify Packages, ProjectSettings, Library, Temp, Obj, Logs, UserSettings, or generated IDE files.
- Do not add large art/audio/model assets.
- Do not convert gameplay classes to NetworkBehaviour.
- Do not require NetworkManager.

## Required scope
Codex may propose and implement only a small set of turn-related scripts, preferably 4-6 files.

Expected files may include:
1. TurnPhase.cs
   - Defines simple turn phase enum values.
   - Example: None, PlayerTurn, EnemyTurn.

2. UnitTurnState.cs
   - Tracks whether a Unit has moved and acted this round.
   - Does not contain combat logic.

3. TurnOrderService.cs
   - Provides fixed role order for future multiplayer:
     Fighter -> Ranger -> Mage -> Barbarian -> Enemy Turn.
   - Does not use Netcode.
   - Does not check client ownership yet.

4. TurnManager.cs
   - Maintains current phase and active actor/role.
   - Starts player round.
   - Ends actor turn or advances to next actor.
   - Resets movement/action flags at the correct time.
   - Does not implement combat, skill, AI, room, or networking logic.

5. ActionPermissionService.cs
   - Answers whether a Unit is allowed to move or act based on turn state.
   - Does not execute the move or action.

6. TurnSystemTester.cs
   - Temporary Play Mode / ContextMenu test helper.
   - Verifies turn order, hasMoved, hasActed, reset behavior.
   - Clearly marked as temporary/test helper.

## Architecture rules
- Use namespace BoneThrone.Turns for turn scripts.
- Use BoneThrone.Core and BoneThrone.Units only when necessary.
- Do not reference Netcode.
- Do not inherit NetworkBehaviour.
- Keep turn logic independent from combat, skills, AI, rooms, levels, UI, and networking.
- Do not break Phase 5 movement.
- If integrating with Phase 5 PlayerMovementController, do it minimally and only to prevent repeated movement after a Unit has already moved.
- Singleplayer should remain playable without NetworkManager.
- Future multiplayer order should be represented as data/logic only, not actual network ownership.

## Acceptance tests in Unity
1. Unity 6.3 LTS opens the project without compile errors.
2. Console has no red compile errors.
3. A player turn can be started.
4. Player Units receive/reset turn state at the start of the round.
5. A Unit can be marked as moved.
6. A moved Unit cannot move again in the same turn/round.
7. A Unit can be marked as acted.
8. TurnManager can advance through the fixed role order.
9. EnemyTurn phase can be reached as a placeholder.
10. No D20, combat, skills, AI behavior, rooms, UI HUD, or networking is implemented.
11. Git status does not include Library, Temp, Obj, Logs, UserSettings, or generated IDE files.

## Expected Codex output for this phase
Codex should first perform a read-only scan and output:
1. Current repository status.
2. Proposed files, limited to 4-6 turn-related files.
3. Responsibility of each file.
4. How turn state relates to Unit and Phase 5 movement.
5. How hasMoved / hasActed are tracked without implementing combat.
6. How fixed order Fighter -> Ranger -> Mage -> Barbarian -> Enemy Turn is represented without networking.
7. Unity scene setup instructions for manual testing.
8. Risks and rollback method.

Codex must not write code until explicitly confirmed.