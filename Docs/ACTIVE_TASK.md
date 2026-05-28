# ACTIVE_TASK.md

## Current phase
Phase 14.20 - Full Combat Regression

## Goal
Create the full combat regression testing document for manual Unity Play Mode validation. This phase is documentation-only and does not modify gameplay code, assets, prefabs, or scenes.

## Allowed files
- `Docs/Phase14_FullCombatRegression.md`
- `Docs/DevLogs/Phase14.20_FullCombatRegression.md`
- `Docs/ACTIVE_TASK.md`

## Forbidden changes
- Do not modify any C# file.
- Do not modify SkillData assets.
- Do not modify Player prefabs.
- Do not modify enemy prefabs.
- Do not modify scene files, including `GridTest.unity`.
- Do not modify Packages or ProjectSettings.
- Do not modify UI scripts or scene layout.
- Do not modify KayKit original assets.
- Do not modify art or materials.
- Do not fix issues during this phase.

## Validation
Documentation-only regression planning phase.

Manual checks:

1. Confirm only docs changed.
2. Open `Docs/Phase14_FullCombatRegression.md`.
3. Run the listed git status commands before testing.
4. Open `Assets/_BoneThrone/Scenes/GridTest.unity`.
5. Execute the checklist in Unity Play Mode and record results.
6. If all pass, proceed to Phase 14.21 Final Handover.
7. If any fail, proceed to Phase 14.20-A Minimal Regression Fix.
