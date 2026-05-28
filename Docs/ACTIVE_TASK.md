# ACTIVE_TASK.md

## Current phase
Phase 15.1 - Health Potion Prefab and Pickup

## Phase 15.0 completion note
Phase 15.0 - Phase 15 Scope Rebaseline and ACTIVE_TASK Update is complete.

Phase 15 development order has been changed from the old Formal Level Scene Plan first sequence to a pre-level production foundation first sequence.

The Phase 15 plan now starts with Health Potion pickup, sequential enemy actions, asset inventory, prefabization, character prefab completion, weapon attachment, animation preparation, GridTest validation, and formal data assetization planning before formal Level scene planning and construction.

## Goal
Implement only the Phase 15.1 slice: Health Potion prefab and pickup flow.

Phase 15.1 should create a reusable Health Potion interactable/pickup path that can be validated in the existing project flow without rewriting stable Phase 14 combat, turn, skill, or scene systems.

## Phase 15.1 scope guardrails
- Phase 15.1 does not rewrite PotionSystem or the existing Potion action behavior.
- Phase 15.1 does not refactor the Turn system.
- Phase 15.1 does not refactor the Skill system.
- Phase 15.1 does not refactor the Combat system.
- Phase 15.1 does not convert `GridTest.unity` into a formal level.
- Phase 15.1 does not implement LAN.
- Phase 15.1 does not implement Boss.
- Phase 15.1 does not create a formal Level scene.
- Phase 15.1 does not roll single-player PlayerTurn back to fixed-order behavior.

## Allowed files
These are the possible Phase 15.1 file ranges only. They do not mean Phase 15.0 changed these files.

- `Assets/_BoneThrone/Scripts/Interactables/*`
- `Assets/_BoneThrone/Scripts/Items/*` or the existing equivalent item/pickup directory
- Minimal files in `Assets/_BoneThrone/Scripts/Units/*` directly related to potion count / inventory only if required
- Minimal files in `Assets/_BoneThrone/Scripts/UI/*` directly related to Potion button quantity display only if required
- `Assets/_BoneThrone/Prefabs/Interactables/HealthPotion.prefab`
- `Docs/DevLogs/Phase15.1_HealthPotionPrefabAndPickup.md`

## Forbidden changes
- Do not modify TurnManager / Turn system unless Phase 15.1 explicitly proves it is required.
- Do not modify SkillSystem or SkillData.
- Do not modify CombatSystem.
- Do not modify LAN / Networking.
- Do not create formal Level_01 / Level_02 / Level_03 scenes.
- Do not convert GridTest into a formal level.
- Do not change the single-player free-order PlayerTurn rule.
- Do not use Fighter -> Ranger -> Mage -> Barbarian fixed-order for single-player.
- Do not modify KayKit original assets.
- Do not modify `Skeleton_Golem` as a normal enemy.
- Do not rename or repurpose `Skeleton_Rogue`.
- Do not change Ranger gameplay identity away from Ranger, even if the visual source uses Rogue visual.

## Validation
Phase 15.1 validation should be defined before implementation.

Expected validation direction:

1. Confirm Unity compiles with no red Console errors.
2. Validate the Health Potion pickup flow in the approved test context.
3. Confirm existing UI Potion action still works as before unless Phase 15.1 explicitly changes only quantity/pickup integration.
4. Confirm single-player free-order PlayerTurn still allows selecting any alive not-ended player.
5. Confirm End Turn and EnemyTurn transitions still match the Phase 14.20 regression baseline.
6. Confirm `GridTest.unity` remains a regression baseline, not a formal level.
7. Confirm no LAN, Boss, formal Level scene, SkillData, or fixed-order single-player turn changes were introduced.
