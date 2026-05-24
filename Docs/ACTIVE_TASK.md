# ACTIVE_TASK.md

## Current phase
Phase 10 - Keys, Stairs, Level Switching and Level Up Progression

## Goal
Implement the minimum single-player progression loop for keys, stairs, level transition placeholders, and automatic party level-up when entering the next floor.

This phase must make the following behavior testable:
1. A key can be collected and recorded as a shared party/key state.
2. Stairs can be interacted with.
3. Stairs provide simple hover feedback or Outline.
4. Clicking stairs can show a confirmation panel or temporary confirmation test.
5. Entering the next floor checks progression conditions.
6. The project has a minimal level-switching / next-floor placeholder flow.
7. All player units level up when entering the next floor.

## Authoritative documents
- Docs/骸骨王座_系统设计文档_Unity6.3LTS_v2.1.docx
- Docs/骸骨王座_Codex完整即用Vibecoding开发文档_Unity6.3LTS_v1.1.docx
- Docs/骸骨王座_Codex从零开发使用Guide_Unity6.3LTS_v1.1.docx

## Current project state
Phase 9 has been completed and merged into dev.

Already completed:
- Grid and coordinate system
- Unit system and tile occupancy
- Selection, BFS range, and A* movement
- Turn system
- D20 basic attack combat
- Basic enemy AI
- Room progression and semi-transparent room shadow

## Recommended files to inspect
- Assets/_BoneThrone/Scripts/Core/
- Assets/_BoneThrone/Scripts/Grid/
- Assets/_BoneThrone/Scripts/Units/
- Assets/_BoneThrone/Scripts/Movement/
- Assets/_BoneThrone/Scripts/Turns/
- Assets/_BoneThrone/Scripts/Combat/
- Assets/_BoneThrone/Scripts/AI/
- Assets/_BoneThrone/Scripts/Rooms/
- Assets/_BoneThrone/Scripts/Levels/
- Assets/_BoneThrone/Scripts/Interactables/
- Assets/_BoneThrone/Scripts/Tests/

## Expected implementation files
Codex should propose the exact files after scanning the repository, but the expected scope is around:
- LevelManager.cs
- LevelProgressionService.cs
- InteractableStairs.cs
- KeyItem.cs
- ConfirmPanel.cs
- A small Phase 10 tester if needed

## Allowed changes
- Add or modify scripts directly required for key pickup, stairs interaction, level progression, and party level-up.
- Add small placeholder components needed to test this phase.
- Add a simple test script only if necessary.
- Add a DevLog after implementation and Unity validation.

## Forbidden changes
- Do not implement complex inventory.
- Do not implement equipment.
- Do not implement a full reward system.
- Do not implement full Boss door logic.
- Do not implement a large LevelManager rewrite.
- Do not implement networking.
- Do not modify NetworkManager or LAN lobby systems.
- Do not implement UI HUD redesign.
- Do not implement skill trees or new skill systems.
- Do not import all art assets in this phase.
- Do not modify Library, Temp, Obj, Logs, UserSettings, or generated IDE files.
- Do not make large scene changes unless explicitly approved.

## Deferred decision
Full art asset import is intentionally deferred.

Planned future branch:
phase/12.5-art-asset-import

Best timing:
After Phase 12 is completed and before Phase 13 begins.

Reason:
Phase 10 should stay focused on progression logic. Full art import should be isolated into its own branch and should not be mixed with gameplay code changes.

## Acceptance tests in Unity 6.3 LTS
1. Project opens without compile errors.
2. A test key object can be collected.
3. Key state is recorded and can be queried.
4. Stairs can detect player interaction.
5. Stairs hover feedback or simple Outline works.
6. Stairs interaction checks whether the required key/progression condition is satisfied.
7. If conditions are not satisfied, the player receives a clear temporary message or log.
8. If conditions are satisfied, the next-floor placeholder transition is triggered.
9. When the next floor is entered, all living player units level up.
10. Existing Phase 9 room/shadow behavior still works.
11. Existing movement, turn, D20 combat, and enemy AI are not broken.

## Codex instruction
Codex must first scan the repository and output a plan only.

Do not write code in the first response.
Do not modify files before the plan is reviewed.