# Phase 15 Plan

## Purpose
This document defines the updated Phase 15 order after Phase 15.0 - Phase 15 Scope Rebaseline and ACTIVE_TASK Update.

Phase 15 now follows a pre-level production foundation first sequence. Formal Level scene planning and construction are intentionally deferred until reusable gameplay, asset, prefab, animation, validation, and data-planning foundations are clearer.

## Stable Baseline
- Current regression baseline: `Assets/_BoneThrone/Scenes/GridTest.unity`.
- `GridTest.unity` is a regression baseline, not a formal level.
- Phase 14.20 Full Combat Regression passed.
- Phase 14.21 Final Handover and Closure completed.
- Single-player PlayerTurn remains free-order: the player may select any alive player unit that has not ended.
- Phase 15.2 sequential enemy actions must not change the single-player free-order PlayerTurn rule.

## Phase 15 Order
- Phase 15.0 - Phase 15 Scope Rebaseline and ACTIVE_TASK Update
- Phase 15.1 - Health Potion Prefab and Pickup
- Phase 15.2 - Sequential Enemy Turn Actions
- Phase 15.3 - Asset Inventory and Prefabization Plan
- Phase 15.4 - Environment Prefabization Pass
- Phase 15.5 - Interactable Prefab Completion Pass
- Phase 15.6 - Character Prefab Completion
- Phase 15.7 - Weapon / Equipment Attachment Pass
- Phase 15.8 - Character Animator Controllers and Animation State Machines
- Phase 15.9 - Animation Integration with Movement / Combat / Skill / Potion
- Phase 15.10 - GridTest Scene Assembly Kit Validation
- Phase 15.11 - Formal Data Assetization Plan
- Phase 15.12 - Formal Level Scene Plan
- Phase 15.13 - Level_01 / Level_02 / Level_03 Scene Setup
- Phase 15.14 - Level_01 Playable Slice
- Phase 15.15 - Level_02 / Level_03 Progression Structure
- Phase 15.16 - Boss Door / Boss Key / Supply Point Preparation
- Phase 15.17 - Victory / Defeat / Retry Flow Plan
- Phase 15.18 - Phase 15 Full Regression and Handover

## Phase 15 Rules
- Phase 15 first-half goal is pre-level production foundation, not direct construction of the formal three-floor dungeon.
- Formal `Level_01`, `Level_02`, and `Level_03` work begins at Phase 15.13.
- Phase 15.10 uses the existing `GridTest.unity` for validation. Do not create a new Scene Assembly Kit scene.
- Phase 15.3 prefab types must be decided after Codex scans the existing assets. Do not predefine a fixed prefab type list before that asset inventory.
- Phase 15.2 is enemy-by-enemy sequential action behavior only. It must not change single-player PlayerTurn from free-order selection to fixed role order.
- Do not use Fighter -> Ranger -> Mage -> Barbarian fixed-order for single-player.
- The multiplayer long-term order reservation remains separate from single-player behavior.
- `Skeleton_Golem` must not be used as a normal enemy prefab.
- `Skeleton_Rogue` is the normal skeleton Rogue enemy naming.
- Ranger gameplay identity remains Ranger, even if the current visual resource uses Rogue visual.
- Do not modify KayKit original resources.
- Project-owned prefabs or prefab variants may be created only under the `_BoneThrone` project directories when a phase explicitly allows prefab work.

## Level Planning Boundary
Phase 15.12 is the first formal level planning phase.

Phase 15.13 is the first formal scene setup phase for:

- `Level_01`
- `Level_02`
- `Level_03`

Before those phases, work should focus on reusable foundations that reduce later scene churn and protect the Phase 14 regression baseline.

## Regression Boundary
`GridTest.unity` remains the integration regression scene through the pre-level foundation phases. It should be used to validate small gameplay and assembly-kit slices when a phase explicitly approves scene/prefab validation.

It must not be renamed, converted into a production level, or treated as the final Level_01 / Level_02 / Level_03 scene.
