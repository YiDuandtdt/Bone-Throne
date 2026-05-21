# ACTIVE_TASK.md

## Current phase
Phase 1 - Unity 6.3 Project and Package Baseline

## Goal
Verify that the Unity project uses the correct Unity 6.3 LTS baseline and has the required packages/settings for later singleplayer and LAN multiplayer development.

This phase only checks and prepares the project baseline. It must not implement gameplay code.

## Unity version
Unity 6000.3.10f1 / Unity 6.3 LTS series

## Allowed files
- Packages/manifest.json
- Packages/packages-lock.json
- ProjectSettings/* only when necessary
- Docs/Unity63PackageBaseline.md
- Docs/ACTIVE_TASK.md
- Docs/DevLogs/Phase01_PackageBaseline.md
- Assets/TextMesh Pro/** only for TMP Essential Resources import
- AGENTS.md only if the Unity version or package baseline rule is outdated

## Forbidden changes
- Do not implement grid, movement, combat, AI, rooms, levels, skills, UI, or networking gameplay in this phase.
- Do not add gameplay scripts.
- Do not create Unit, Tile, GridManager, TurnManager, CombatSystem, SkillSystem, RoomController, or Network gameplay classes in this phase.
- Do not modify scenes unless explicitly confirmed.
- Do not add large art, audio, model, VFX, or animation assets.
- Do not modify Library, Temp, Obj, Logs, UserSettings, or generated IDE files.

## Required package baseline
The project should confirm or install the following:

1. URP / Universal Render Pipeline
   - The project should use Universal 3D / URP.
   - URP asset should be assigned in Graphics / Quality settings.

2. TextMeshPro
   - TMP Essentials should be imported.
   - Used later for battle HUD, D20 logs, skill text, health text, and prompts.

3. Netcode for GameObjects
   - Required for later LAN multiplayer phase.
   - Do not implement multiplayer gameplay in this phase.

4. Unity Transport
   - Required transport layer for Netcode for GameObjects.
   - Do not implement Host/Client logic in this phase.

5. Multiplayer Tools
   - Recommended for later debugging and profiling.

6. Multiplayer Play Mode
   - Recommended for early four-client simulation.
   - Final LAN validation will still need builds or multiple devices.

## Acceptance tests in Unity
1. Unity 6.3 LTS opens the project without compile errors.
2. Console has no red compile errors after package import.
3. URP asset is assigned correctly.
4. TextMeshPro Essentials are imported.
5. Netcode for GameObjects is installed or clearly documented as missing.
6. Unity Transport is installed or clearly documented as missing.
7. Multiplayer Tools and Multiplayer Play Mode are installed or clearly documented as optional.
8. No gameplay scripts or gameplay systems are added in this phase.
9. Git status does not include Library, Temp, Obj, Logs, UserSettings, or generated IDE files.

## Expected Codex output for this phase
Codex should first perform a read-only scan and output:
1. Current package status.
2. Missing packages.
3. Files that may need modification.
4. Risks related to Unity 6.3 package versions.
5. Local Unity manual test steps.
6. Confirmation that no gameplay code is implemented.

Codex must not write code until explicitly confirmed.