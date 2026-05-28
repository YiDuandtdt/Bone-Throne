# Phase 15.0 - Scope Rebaseline

## Summary
Phase 15.0 is a documentation-only scope rebaseline. It updates the active project direction from Formal Level Scene Plan first to pre-level production foundation first.

## Actual Modified Files
- `Docs/ACTIVE_TASK.md`
- `Docs/Phase15_Plan.md`
- `Docs/DevLogs/Phase15.0_ScopeRebaseline.md`

## What Changed
- Recorded the full updated Phase 15 sequence in `Docs/Phase15_Plan.md`.
- Updated `Docs/ACTIVE_TASK.md` to point to Phase 15.1 - Health Potion Prefab and Pickup.
- Documented that Phase 15.0 is complete and that Phase 15.1 is the next implementation slice.

## What Did Not Change
- No code changed.
- No assets changed.
- No prefabs changed.
- No scenes changed.
- No SkillData assets changed.
- No ProjectSettings or Packages changed.
- No KayKit original resources changed.
- No LAN, Boss, or formal Level scene work was done.

## Why Phase 15 Was Rebaselined
The previous recommended next step after Phase 14 was Formal Level Scene Plan first. That is no longer the safest order.

Formal scenes depend on reusable foundations that are not yet fully production-ready: Health Potion pickup flow, sequential enemy actions, asset inventory, prefabization, interactable completion, character prefab completion, weapon attachment, animation controllers, animation integration, GridTest assembly validation, and data assetization planning.

Doing formal Level scenes before these foundations would increase scene churn, Inspector binding risk, prefab rework, and regression cost. The new order keeps `GridTest.unity` as the stable regression baseline while Phase 15 builds the reusable production foundation first.

## Current Stable Baseline
- Current regression baseline remains `Assets/_BoneThrone/Scenes/GridTest.unity`.
- Phase 14.20 Full Combat Regression passed.
- Phase 14.21 Final Handover and Closure completed.
- Phase 14 remains functionally closed.

## PlayerTurn Rule
Single-player PlayerTurn remains free-order.

The player may select any alive player unit that has not ended. Phase 15 must not roll this back to Fighter -> Ranger -> Mage -> Barbarian fixed-order single-player behavior.

## Unity Validation
Unity Play Mode was not run for Phase 15.0 because this phase is docs-only.

## Next Phase
Next phase: Phase 15.1 - Health Potion Prefab and Pickup.

Phase 15.1 should implement only the Health Potion prefab and pickup flow. It should not rewrite PotionSystem, refactor Turn / Skill / Combat systems, touch LAN, create Boss content, create formal Level scenes, or change the single-player free-order PlayerTurn rule.

## Rollback
Phase 15.0 rollback is docs-only:

- Restore `Docs/ACTIVE_TASK.md` to the previous version.
- Delete or revert `Docs/Phase15_Plan.md`.
- Delete or revert `Docs/DevLogs/Phase15.0_ScopeRebaseline.md`.

No gameplay rollback is required because no code, asset, prefab, scene, SkillData, ProjectSettings, or Packages files were changed.
