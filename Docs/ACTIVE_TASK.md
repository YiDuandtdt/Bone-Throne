# ACTIVE_TASK.md

## Current phase
Phase 0 - Codex Access and Repository Rule Freeze

## Goal
Set up the repository rules, documentation structure, Git hygiene, and Codex working boundaries before implementing gameplay code.

## Allowed files
- AGENTS.md
- README.md
- .gitignore
- Docs/ACTIVE_TASK.md
- Docs/DevLogs/
- Assets/_BoneThrone/ folder structure

## Forbidden changes
- Do not implement grid, movement, combat, AI, rooms, levels, skills, networking gameplay, UI systems, or character logic in this phase.
- Do not modify Library, Temp, Obj, Logs, UserSettings, or generated IDE files.
- Do not add large art/audio assets yet.

## Acceptance tests in Unity
1. Unity 6.3 LTS opens the project without compile errors.
2. Assets/_BoneThrone folder structure exists.
3. Docs folder contains the three design/workflow documents.
4. AGENTS.md exists at repository root.
5. Console has no red compile errors.
