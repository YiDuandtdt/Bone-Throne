# ACTIVE_TASK.md

## Current phase
Phase 14.10-B - GridTest Camera Right Mouse Rotation

## Goal
Extend the existing GridTest camera controller with right mouse drag horizontal rotation.

The camera must support:
- holding right mouse button and dragging horizontally to rotate the camera left / right
- configurable rotation speed in Inspector
- optional rotation around zoomPivot or a stable world pivot
- no camera roll
- no gameplay state changes
- no interference with left-click selection, Move, Basic Attack, Skill Slot 0, HUD buttons, CombatLog, Enemy HP Bar, Room / Key / Stairs / LevelUp, or Enemy AI

This phase is limited to extending GridTest camera controls.

## Allowed files
- Assets/_BoneThrone/Scripts/Camera/GridTestCameraController.cs
- Assets/_BoneThrone/Scenes/GridTest.unity, only if needed to configure new Inspector fields
- Docs/DevLogs/Phase14.10B_GridTestCameraRotation.md
- Docs/ACTIVE_TASK.md

## Forbidden changes
- Do not modify combat logic.
- Do not modify skill logic.
- Do not modify DamageResolver.
- Do not modify SkillEffectExecutor.
- Do not modify CombatSystem.TryBasicAttack.
- Do not modify SkillSystem.TryUseSkill.
- Do not modify PlayerMovementController unless Codex first proves it is required.
- Do not modify UIActionModeController unless Codex first proves there is an existing right-click conflict and asks for approval.
- Do not modify unit prefabs.
- Do not modify enemy prefabs.
- Do not modify SkillData assets.
- Do not modify KayKit original assets.
- Do not modify Skeleton_Rogue.
- Do not use Skeleton_Golem as a normal enemy.
- Do not change Ranger visual back to Adventurers Ranger.
- Do not introduce Cinemachine.

## Required behavior
1. Right mouse rotation:
   - Hold right mouse button.
   - Drag horizontally.
   - Camera rotates left / right around a stable pivot.
   - Rotation should be yaw only.
   - Camera pitch should remain stable.
   - Camera roll must remain zero.

2. Inspector configuration:
   - rotation enabled toggle
   - rotation speed
   - optional invert rotation toggle
   - optional rotation pivot / zoom pivot reuse
   - optional min/max yaw only if needed

3. Compatibility:
   - Left-click selection still works.
   - Move mode still works.
   - Basic Attack targeting still works.
   - Skill Slot 0 targeting still works.
   - HUD buttons still work.
   - Existing middle mouse drag still works.
   - Existing mouse wheel zoom still works.
   - Console has no new red errors.

## Required first step
Before implementing, Codex must scan current input scripts and report whether right mouse button is already used by:
- UIActionModeController
- PlayerMovementController
- SelectionManager
- any input tester
- any camera script

If right mouse is already used, Codex must explain the conflict and propose a safe handling approach before editing code.

## Validation
Manual Unity Play Mode tests:
1. Open GridTest.unity.
2. Confirm no compile errors.
3. Enter Play Mode.
4. Hold right mouse and drag left / right: camera rotates horizontally.
5. Camera pitch remains stable.
6. Camera roll remains zero.
7. Middle mouse drag still works.
8. Mouse wheel zoom still works.
9. Left-click selection still works.
10. Move mode still works.
11. Basic Attack still works.
12. Skill Slot 0 still works.
13. HUD buttons still work.
14. Console has no new red errors.