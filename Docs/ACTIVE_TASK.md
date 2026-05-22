# ACTIVE_TASK.md

## Current phase
Phase 2 - Project Folders and Core Architecture Skeleton

## Goal
Create the minimal architecture skeleton for the Unity 6.3 LTS tactics demo.

This phase only creates foundational types, interfaces, enums, and folder-level organization. It must not implement gameplay behavior.

## Unity version
Unity 6000.3.10f1 / Unity 6.3 LTS series

## Allowed files
- Assets/_BoneThrone/Scripts/Core/**
- Assets/_BoneThrone/Scripts/Data/**
- Assets/_BoneThrone/Scripts/Grid/**
- Assets/_BoneThrone/Scripts/Units/**
- Assets/_BoneThrone/Scripts/Turns/**
- Assets/_BoneThrone/Scripts/Combat/**
- Assets/_BoneThrone/Scripts/Skills/**
- Assets/_BoneThrone/Scripts/Rooms/**
- Assets/_BoneThrone/Scripts/Levels/**
- Assets/_BoneThrone/Scripts/Interactables/**
- Assets/_BoneThrone/Scripts/UI/**
- Assets/_BoneThrone/Scripts/Networking/**
- Docs/ACTIVE_TASK.md
- Docs/DevLogs/Phase02_ArchitectureSkeleton.md

## Forbidden changes
- Do not implement grid generation, tile clicking, movement, pathfinding, combat, enemy AI, room progression, skills, UI behavior, or LAN gameplay in this phase.
- Do not create scene objects, prefabs, ScriptableObject assets, character data assets, enemy data assets, or level data assets yet.
- Do not modify Unity scenes unless explicitly confirmed.
- Do not modify Packages, ProjectSettings, Library, Temp, Obj, Logs, UserSettings, or generated IDE files.
- Do not add art, audio, model, animation, VFX, or large binary assets.

## Required skeleton scope
Codex may create only a small set of foundational code files, preferably 3-6 core files.

Suggested scope:
1. GameMode enum
   - Represents SinglePlayer, LANHost, LANClient, and future Online modes.

2. RoleId enum
   - Represents Fighter, Ranger, Mage, Barbarian, Enemy, None.

3. ActionCommand base structure
   - Abstract or base serializable command concept.
   - Only defines data fields and intent.
   - No actual movement, attack, skill, or network execution.

4. GameStateSnapshot placeholder
   - Defines the idea of current level, current actor, unit states, room states, and key state.
   - Placeholder only; no real gameplay state collection yet.

5. IGameSession interface
   - Defines how local or future network sessions submit commands.
   - No Netcode implementation in this phase.

6. LocalGameSession stub
   - Minimal local implementation that can receive a command and expose an event/callback.
   - No gameplay execution yet.

## Architecture rules
- Use namespace style under BoneThrone.*.
- Keep gameplay rules transport-agnostic.
- Do not make core gameplay depend directly on LAN/IP/Relay/Netcode.
- Do not convert gameplay classes to NetworkBehaviour in this phase.
- Singleplayer must remain possible without NetworkManager.
- Multiplayer will later use Host authority, but this phase only prepares interfaces.

## Acceptance tests in Unity
1. Unity 6.3 LTS opens the project without compile errors.
2. Console has no red compile errors.
3. New scripts compile successfully.
4. No scene or prefab setup is required.
5. No gameplay feature is implemented.
6. No NetworkManager, Lobby, Host, Client, GridManager, Unit, CombatSystem, TurnManager, SkillSystem, RoomController, or AI behavior is implemented in this phase.
7. Git status does not include Library, Temp, Obj, Logs, UserSettings, or generated IDE files.

## Expected Codex output for this phase
Codex should first perform a read-only scan and output:
1. Current repository status.
2. Proposed files, limited to 3-6 core files.
3. Responsibility of each file.
4. Why the proposed skeleton supports later singleplayer and LAN multiplayer.
5. What will not be implemented in this phase.
6. Local Unity manual test steps.
7. Risks and rollback method.

Codex must not write code until explicitly confirmed.