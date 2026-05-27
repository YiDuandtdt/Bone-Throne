# ACTIVE_TASK.md

## Current phase
Phase 14.10 - GridTest Camera Controls

## Goal
Implement lightweight camera controls for the current GridTest integration scene.

The camera must support:
- holding middle mouse button to drag / pan the camera
- using mouse wheel to zoom in and out
- configurable speed and zoom limits in Inspector
- no interference with left-click unit selection, tile clicking, UI buttons, attack targeting, or skill targeting

This phase is limited to GridTest camera control only.

## Allowed files
- Assets/_BoneThrone/Scripts/Camera/GridTestCameraController.cs
- Assets/_BoneThrone/Scenes/GridTest.unity, only if needed to attach/configure the camera controller
- Docs/DevLogs/Phase14.10_GridTestCameraControls.md
- Docs/ACTIVE_TASK.md

## Forbidden changes
- Do not modify combat logic.
- Do not modify skill logic.
- Do not modify DamageResolver.
- Do not modify SkillEffectExecutor.
- Do not modify CombatSystem.TryBasicAttack.
- Do not modify SkillSystem.TryUseSkill.
- Do not modify unit prefabs.
- Do not modify enemy prefabs.
- Do not modify SkillData assets.
- Do not modify KayKit original assets.
- Do not modify Skeleton_Rogue.
- Do not use Skeleton_Golem as a normal enemy.
- Do not change Ranger visual back to Adventurers Ranger.
- Do not introduce Cinemachine unless the current scene already depends on it and the user explicitly approves.

## Required behavior
1. Middle mouse drag:
   - Hold middle mouse button.
   - Move mouse.
   - Camera pans across the grid.
   - Drag must not select units or trigger tile actions.

2. Mouse wheel zoom:
   - Scroll up/down.
   - Camera zooms in/out.
   - Zoom must be clamped between configurable min and max values.

3. Inspector configuration:
   - pan speed
   - zoom speed
   - min zoom
   - max zoom
   - optional drag inversion toggle
   - optional enable/disable toggle

4. Compatibility:
   - Must work in GridTest.unity.
   - Must not break BattleHUD buttons.
   - Must not affect left click selection / move / attack / skill targeting.
   - Must not require changes to gameplay systems.

## Validation
Manual Unity Play Mode tests:
1. Open GridTest.unity.
2. Confirm no compile errors.
3. Enter Play Mode.
4. Hold middle mouse and drag: camera pans.
5. Scroll wheel: camera zooms in/out.
6. Try selecting a player with left click: still works.
7. Try Move mode: still works.
8. Try Basic Attack mode: still works.
9. Try Skill Slot 0 mode: still works.
10. Try clicking HUD buttons: still works.
11. Confirm Console has no new red errors.