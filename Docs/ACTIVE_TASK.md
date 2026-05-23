# ACTIVE_TASK.md

## Current phase
Phase 9 - Room Progression and Semi-transparent Shadow

## Goal
Implement the minimal room progression loop for the Unity 6.3 LTS tactics demo.

This phase should allow a room to start as hidden/unentered, show a semi-transparent shadow overlay before entry, trigger room entry when a player unit enters or reaches a trigger area, reveal the room by hiding the shadow, activate or spawn assigned enemies, track whether the room is cleared, and safely keep the logic independent from levels, keys, stairs, UI, and networking.

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
- Assets/_BoneThrone/Scripts/Rooms/**
- Assets/_BoneThrone/Scripts/Tests/**
- Docs/ACTIVE_TASK.md
- Docs/DevLogs/Phase09_RoomShadow.md

## Forbidden changes
- Do not implement keys, stairs, level switching, boss doors, full level progression, skills, cooldowns, UI HUD, LAN multiplayer, lobby, NetworkManager, or Netcode synchronization in this phase.
- Do not implement complex fog of war.
- Do not implement procedural dungeon generation.
- Do not implement complex enemy spawn waves.
- Do not implement rewards, loot, potions, or inventory.
- Do not modify Packages, ProjectSettings, Library, Temp, Obj, Logs, UserSettings, or generated IDE files.
- Do not add large art/audio/model assets.
- Do not convert gameplay classes to NetworkBehaviour.
- Do not require NetworkManager.

## Required scope
Codex may propose and implement only a small set of room-related scripts, preferably 4-6 files.

Expected files may include:

1. RoomState.cs
   - Defines simple room states.
   - Example: Unentered, Entered, CombatActive, Cleared.
   - No level switching or key logic.

2. RoomController.cs
   - Owns one room's state.
   - Handles EnterRoom(), RevealRoom(), ActivateEnemies(), CheckCleared().
   - Does not manage whole-level progression.

3. RoomTrigger.cs
   - Temporary trigger component for detecting player entry.
   - Can use Collider trigger or manual ContextMenu test.
   - Does not implement UI.

4. RoomShadowController.cs
   - Controls semi-transparent shadow object visibility.
   - Can simply SetActive(true/false) or switch renderer/material alpha.
   - No complex fog of war.

5. RoomEnemyActivator.cs
   - Activates pre-placed enemy GameObjects when room is entered.
   - Optional simple spawn from assigned prefabs/spawn points only if minimal.
   - No wave system or procedural spawning.

6. RoomSystemTester.cs
   - Temporary Play Mode / ContextMenu test helper.
   - Clearly marked as temporary/test helper.

## Architecture rules
- Use namespace BoneThrone.Rooms for room scripts.
- Use BoneThrone.Units only when necessary.
- Keep room logic independent from keys, stairs, levels, UI, and networking.
- Do not reference Netcode.
- Do not inherit NetworkBehaviour.
- Enemy activation should reuse existing Unit / AI / Combat systems indirectly by enabling assigned enemies.
- Room cleared condition may be based on all assigned enemies being dead.
- Singleplayer must remain playable without NetworkManager.
- This phase must not break Phase 5 movement, Phase 6 turns, Phase 7 combat, or Phase 8 enemy AI.

## Acceptance tests in Unity
1. Unity 6.3 LTS opens the project without compile errors.
2. Console has no red compile errors.
3. A room starts in Unentered state.
4. Room shadow is visible before entry.
5. Player entry triggers EnterRoom.
6. Room state changes to Entered or CombatActive.
7. Room shadow is hidden or disabled after entry.
8. Assigned enemies are activated when the room is entered.
9. Room can detect cleared state when assigned enemies are dead.
10. No keys, stairs, level switching, skills, UI HUD, networking, or NetworkManager is implemented.
11. Git status does not include Library, Temp, Obj, Logs, UserSettings, or generated IDE files.

## Expected Codex output for this phase
Codex should first perform a read-only scan and output:
1. Current repository status.
2. Proposed files, limited to 4-6 room-related files.
3. Responsibility of each file.
4. How room states work.
5. How shadow reveal works.
6. How enemy activation works without complex spawning.
7. How room cleared state is detected.
8. Unity scene setup instructions for manual testing.
9. Risks and rollback method.

Codex must not write code until explicitly confirmed.