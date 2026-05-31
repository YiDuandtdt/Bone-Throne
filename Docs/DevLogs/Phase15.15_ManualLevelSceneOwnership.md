# Phase 15.15 - Manual Level Scene Ownership and Codex Scope Correction

## Summary

Phase 15.15 records a scope correction for formal level scene ownership.

Phase 15.14 produced a first-pass `Level_01` playable-slice wiring attempt. The user has confirmed that this result is not suitable as the formal usable level baseline.

From this phase onward, formal level scene content is owned manually by the user, not Codex.

## Decision

Codex must no longer directly:

- create formal level scenes
- modify formal level scenes
- wire level scene managers
- build room / tile / encounter / interactable scene content
- make `Level_01`, `Level_02`, or `Level_03` playable slices
- extend `Level_02` / `Level_03` progression scenes
- modify `GridTest` as a formal level
- automatically copy `GridTest` structure into formal level scenes

The original planned `Phase 15.15 - Level_02 / Level_03 Progression Structure` is paused and no longer the next task.

## Phase 15.14 Status

`Level_01` first-pass wiring exists as a historical Codex attempt.

It is not accepted as:

- the official playable Level_01 baseline
- a formal level scene template
- a pattern to extend into Level_02 or Level_03

The user may manually keep, alter, replace, or discard the scene contents.

## Codex Allowed Work Going Forward

Codex may still help with:

- documentation planning
- checklists
- review notes
- DevLogs
- narrow system fixes
- narrow prefab fixes when explicitly approved
- narrow code fixes when explicitly approved
- non-scene rule, data, UI, or system preparation
- advice for user-made scene setup without directly editing scenes

## Scene Boundary

Codex should not edit these formal scenes unless the user explicitly overrides this decision:

- `Assets/_BoneThrone/Scenes/Level_01.unity`
- `Assets/_BoneThrone/Scenes/Level_02.unity`
- `Assets/_BoneThrone/Scenes/Level_03.unity`

`Assets/_BoneThrone/Scenes/GridTest.unity` remains a regression baseline and must not be converted into a formal level.

## Next Phase

Next phase:

- `Phase 15.16 - Boss Door / Boss Key / Supply Point Preparation`

Phase 15.16 should be non-scene preparation only. It may prepare rules, data design, prefabs, code hooks, or checklists if explicitly approved, but it must not place BossDoor, BossKey, SupplyPoint, or related gameplay content into formal level scenes.

## Files Modified

- `Docs/ACTIVE_TASK.md`
- `Docs/DevLogs/Phase15.14_Level01PlayableSlice.md`
- `Docs/DevLogs/Phase15.15_ManualLevelSceneOwnership.md`

## Forbidden In This Phase

This phase intentionally did not modify:

- formal level scenes
- `GridTest.unity`
- C# code
- prefab assets
- SkillData
- KayKit source assets
- ProjectSettings
- Packages

## Rollback

To roll back this documentation-only correction:

```powershell
git restore -- Docs/ACTIVE_TASK.md Docs/DevLogs/Phase15.14_Level01PlayableSlice.md Docs/DevLogs/Phase15.15_ManualLevelSceneOwnership.md
```
