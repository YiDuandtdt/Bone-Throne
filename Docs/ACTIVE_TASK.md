# ACTIVE TASK

## Current Phase

Phase 15.15 - Manual Level Scene Ownership and Codex Scope Correction

## Status

Phase 15.15 records a project scope correction after the Phase 15.14 `Level_01` playable-slice wiring attempt.

Phase 15.14 produced a first-pass `Level_01.unity` wiring attempt, but the user has confirmed that version is not suitable as a formal usable level baseline.

From Phase 15.15 onward, formal level scene content is owned manually by the user.

Codex must not create, modify, wire, or auto-generate formal level scenes.

`GridTest.unity` remains the regression baseline and must not be converted into a formal level.

## Phase 15.14 Result Rebaseline

Phase 15.14 historical result:

- `Level_01.unity` first-pass scene wiring exists in the working tree/history.
- It is not accepted as the official usable level baseline.
- It should not be extended by Codex into a formal playable level.
- The user may manually keep, alter, replace, or discard its scene contents.

Codex may document or review user-made scene setup, but must not directly edit formal level scenes.

## Codex Scene Boundary

Codex no longer owns:

- creating formal level scenes
- modifying formal level scenes
- wiring level scene managers
- building room / tile / encounter / interactable scene content
- making `Level_01`, `Level_02`, or `Level_03` playable slices
- extending `Level_02` / `Level_03` progression scenes
- modifying `GridTest` as a formal level
- automatically copying `GridTest` structure into formal levels

Codex may still help with:

- documentation planning
- checklist creation
- review notes
- DevLogs
- narrow code fixes
- narrow prefab fixes when explicitly approved
- non-scene rules / data / UI / system preparation
- scene inspection advice for user-made levels, without direct scene modification

## References

Current references:

- `Docs/Phase15_FormalLevelScenePlan.md`
- `Docs/Phase15_FormalDataAssetizationPlan.md`
- `Docs/Phase15_AssetInventoryAndPrefabizationPlan.md`
- `Docs/Phase15_EnvironmentPrefabizationSummary.md`
- `Docs/DevLogs/Phase15.10_GridTestSceneAssemblyKitValidation.md`
- `Docs/DevLogs/Phase15.13_LevelSceneSetup.md`
- `Docs/DevLogs/Phase15.14_Level01PlayableSlice.md`
- `Docs/DevLogs/Phase15.15_ManualLevelSceneOwnership.md`
- `Docs/Phase15_Plan.md`

## Next Phase Candidate

Phase 15.16 - Boss Door / Boss Key / Supply Point Preparation

Phase 15.16 should be non-scene preparation only. It may plan or implement narrow code / prefab / data support if explicitly approved, but must not place BossDoor, BossKey, SupplyPoint, or related content into formal level scenes.

## Forbidden Scene Changes

- Do not create formal level scenes.
- Do not modify formal level scenes.
- Do not modify `GridTest.unity`.
- Do not modify `Level_01.unity`, `Level_02.unity`, or `Level_03.unity` unless the user explicitly overrides this boundary.
- Do not wire level scene managers.
- Do not build room / tile / encounter / interactable scene content.
- Do not create playable slices for formal levels.
- Do not auto-copy `GridTest` scene structure into formal levels.

## General Forbidden Changes Until Explicitly Approved

- Do not modify C# code unless a phase prompt explicitly approves a narrow fix.
- Do not modify SkillData.
- Do not modify KayKit source assets.
- Do not implement Boss fight.
- Do not place BossDoor / BossKey.
- Do not place SupplyPoint.
- Do not implement Victory / Defeat / Retry.
- Do not implement LAN / Networking.
- Do not create ScriptableObject assets unless a later data phase explicitly approves it.
- Do not change the single-player free-order PlayerTurn rule.
- Do not use Fighter -> Ranger -> Mage -> Barbarian fixed-order for single-player.
