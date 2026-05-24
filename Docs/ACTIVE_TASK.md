# ACTIVE_TASK.md

## Current phase
Phase 12.6 - Gameplay Prefab Wrapping / Visual Replacement

## Branch
phase/12.6-gameplay-prefab-wrapping

## Goal
Create project-owned gameplay prefabs that wrap the imported KayKit visual assets into the existing Bone Throne gameplay component system.

This phase is a visual replacement / prefab packaging phase. It must not rewrite movement, combat, turn logic, skills, AI, rooms, level progression, or networking.

## Background
Phase 0 - Phase 12.5 have been completed and merged into dev.

The project already has:
- Grid / Tile system
- Unit placement and occupancy
- Movement range, path preview, and A* movement
- Turn system
- D20 basic combat
- Enemy AI
- Room progression and room shadows
- Key, stairs, level transition, and upgrade flow
- Skill framework
- Four representative hero skills
- KayKit art assets imported and render-compatible

The current missing piece is project-owned gameplay prefabs that combine existing gameplay scripts with replaceable visual child objects.

## Required outcomes
1. Create or plan project-owned player gameplay prefabs:
   - Fighter
   - Ranger
   - Mage
   - Barbarian

2. Create or plan project-owned enemy gameplay prefabs:
   - Skeleton_Minion
   - Skeleton_Warrior
   - Skeleton_Mage
   - Skeleton_Golem or current available equivalent

3. Create or plan visual replacement prefabs for:
   - Key
   - Stairs

4. Keep existing gameplay scripts attached and functional:
   - Unit
   - UnitStats / runtime state if already used
   - KeyItem
   - InteractableStairs
   - Any existing tester or scene references required for regression tests

5. All visual references must be Inspector-configurable.
   Do not hard-code KayKit asset paths.

6. Missing UI, sound, VFX, icons, portraits, and formal animations can remain missing.
   If a placeholder is required for testing, use a simple placeholder and keep it replaceable.

## Allowed files
Codex must first scan the repository and propose the exact file list before editing.

Likely allowed areas:
- Assets/_BoneThrone/Prefabs/Units/
- Assets/_BoneThrone/Prefabs/Interactables/
- Assets/_BoneThrone/Art/ or existing KayKit asset folders only as references
- Assets/_BoneThrone/Scripts/Units/ only if a tiny visual-anchor helper is absolutely necessary
- Assets/_BoneThrone/Scripts/Interactables/ only if a tiny visual-anchor helper is absolutely necessary
- Docs/DevLogs/Phase13_Gameplay_Prefab_Wrapping.md

## Forbidden changes
- Do not rewrite Unit, TurnManager, CombatSystem, SkillSystem, Enemy AI, Room system, or LevelManager.
- Do not implement new combat, new skills, new AI, or networking.
- Do not create complex Animator Controllers or full animation state machines.
- Do not implement formal UI, sound, VFX, icons, portraits, victory screen, or defeat screen in this phase.
- Do not hard-code Resources.Load paths or KayKit asset paths.
- Do not modify Library, Temp, Obj, Logs, UserSettings, or generated IDE files.
- Do not modify scenes unless explicitly approved after the plan.

## Acceptance tests in Unity
1. Unity 6.3 LTS opens without compile errors.
2. Player gameplay prefabs can be placed in the test scene or inspected as prefabs without missing scripts.
3. Enemy gameplay prefabs can be inspected or placed without missing scripts.
4. Key and stairs prefabs keep their existing gameplay scripts and visual child objects.
5. Existing movement regression still works.
6. Existing D20 basic attack regression still works.
7. Existing representative skill regression still works.
8. Existing key / stairs / level transition / upgrade regression still works.
9. Missing visual/audio/VFX references do not cause runtime crashes.
10. Console has no red compile errors.

## Notes
This phase is primarily about prefab wrapping and visual replacement. Keep all visuals replaceable through serialized fields or child object references. Preserve the existing gameplay logic.