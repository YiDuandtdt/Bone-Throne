# ACTIVE_TASK.md

## Current phase
Phase 13 - UI and Feedback

## Branch
phase/13-ui-feedback

## Goal
Implement the first battle UI and gameplay feedback layer for the existing single-player gameplay loop.

This phase should make the current playable systems easier to understand:
- current turn / current actor
- selected unit
- player HP / level / action state
- skill buttons and cooldown/readiness display
- D20 combat log / floating feedback
- basic key / stairs / condition prompt
- lightweight world-space health bars if feasible

This phase must not rewrite movement, combat, skills, AI, rooms, levels, prefab wrapping, or networking.

## Background
Phase 0 - Phase 12.6 have been completed and merged into dev.

The project already has:
- Grid and tile system
- Unit placement and occupancy
- Movement and pathfinding
- Turn system
- D20 basic combat
- Enemy AI
- Room progression and shadows
- Key, stairs, level transition, and upgrade flow
- Skill framework and representative skills
- Gameplay prefabs for players, enemies, key, and stairs

Important Phase 12.6 notes:
- Player Ranger keeps Ranger gameplay but intentionally uses Adventurers / Rogue visual.
- Skeleton_Rogue is the normal skeleton rogue enemy prefab.
- Skeleton_Golem is reserved for a future Boss / heavy Boss prefab and must not be used as a normal enemy prefab.

## Required outcomes
1. Add a first battle HUD layer using uGUI + TextMeshPro.
2. Show current turn / current actor / selected unit clearly.
3. Show four player status panels if the current data is available:
   - name
   - HP
   - level
   - moved / acted state
4. Add a skill/action bar prototype:
   - basic attack
   - representative skill slot 0
   - defend / potion placeholders only if already supported
   - disabled or placeholder state if not currently supported
5. Add or connect combat feedback:
   - D20 roll
   - hit / miss
   - damage
   - death
6. Add simple condition prompts:
   - key missing
   - stairs confirmation / transition hint
   - invalid action / not your turn if current systems expose it
7. Keep all UI references Inspector-configurable.
8. Missing icons, portraits, final art, sound, and VFX can remain placeholders.

## Allowed files
Codex must first scan the repository and propose the exact file list before editing.

Likely allowed areas:
- Assets/_BoneThrone/Scripts/UI/
- Assets/_BoneThrone/Prefabs/UI/
- Assets/_BoneThrone/Scenes/GridTest.unity only if explicitly approved after the plan
- Assets/_BoneThrone/Data/OnlyTest/ only if a tiny UI test data asset is absolutely necessary
- Docs/DevLogs/Phase13_UI_Feedback.md

## Forbidden changes
- Do not rewrite Unit, TurnManager, CombatSystem, SkillSystem, Enemy AI, Room system, or LevelManager.
- Do not modify gameplay formulas.
- Do not add networking.
- Do not add formal sound, VFX, animation state machines, or final portraits.
- Do not use UI Toolkit for battle HUD in this phase.
- Do not hard-code scene object paths.
- Do not modify KayKit original assets.
- Do not modify Skeleton_Golem as a normal enemy.
- Do not change Ranger away from Adventurers Rogue visual.
- Do not modify Library, Temp, Obj, Logs, UserSettings, or generated IDE files.

## Acceptance tests in Unity
1. Unity 6.3 LTS opens without compile errors.
2. Battle HUD appears in GridTest or approved test scene.
3. Current turn / selected unit display updates.
4. Player status panels show available HP / state data.
5. Skill/action bar shows available skill/action state without breaking existing input.
6. D20 combat feedback appears when a basic attack is performed.
7. Invalid or blocked interactions show a readable prompt if existing systems provide the event.
8. Existing movement regression still works.
9. Existing D20 basic attack regression still works.
10. Existing representative skill regression still works.
11. Existing key / stairs / level transition / upgrade regression still works.
12. Console has no red compile errors.