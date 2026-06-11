# Phase 14.5 Regression Checklist

## Purpose and scope

This document defines Play Mode regression checks for the current Bone Throne Unity 6.3 LTS project.

Phase 14.5 is documentation-only. It does not implement fixes, write gameplay code, modify scenes, modify prefabs, modify ScriptableObject assets, modify KayKit original resources, or change `Assets`, `Packages`, or `ProjectSettings`.

If any test fails, record the result in the checklist / known failure section. Do not fix it in Phase 14.5.

## Scene rule

Use `Assets/_BoneThrone/Scenes/GridTest.unity` for all current integrated regression tests.

`MainMenu.unity` is still a placeholder. This checklist is not formal three-floor scene acceptance.

## Global safety rules

Every test must preserve these rules:

- UI does not directly subtract HP.
- UI does not directly change cooldown.
- UI does not directly call `MarkActed`.
- UI does not directly call `MarkMoved`.
- Highlight preview does not call `TryBasicAttack`.
- Highlight preview does not call `TryUseSkill`.
- `DamageResolver` is not casually modified.
- `SkillEffectExecutor` skill formulas are not casually modified.
- `CombatLog` is not implemented by parsing strings.
- `Skeleton_Rogue` is the ordinary enemy.
- `Skeleton_Golem` is reserved for future Boss / heavy Boss only.
- Player Ranger uses Adventurers Rogue visual.
- KayKit original resources are not modified.

## Test field template

Each test uses these fields:

- Test ID
- System
- Scene
- Preconditions
- Inspector bindings to verify first
- Steps
- Expected result
- Failure symptoms
- Likely causes
- Screenshot required
- Console required
- Safety rules checked
- Phase 14.5 action allowed: document only / no fix

## REG-01 Select player unit

- Test ID: REG-01
- System: Selection / HUD
- Scene: `GridTest.unity`
- Preconditions: Unity opens with no red Console compile errors. `GridTest.unity` is loaded.
- Inspector bindings to verify first: `BattleHUDController.playerUnits`, `TurnManager.playerUnits`, `GridManager.initialTiles`
- Steps:
  1. Enter Play Mode.
  2. Click Fighter, Ranger, Mage, and Barbarian one at a time.
  3. Click the selected unit again to clear selection.
- Expected result: Living player units can be selected. Selected tile shows blue highlight. TurnBanner / HeroPanel reflects selected actor. Clicking the selected unit again clears selection and selected highlight.
- Failure symptoms: Selection does not change, selected blue highlight is missing, wrong actor appears in HUD, or selection does not clear.
- Likely causes: Missing `playerUnits`, incomplete `initialTiles`, selection raycast issue, or HUD binding mismatch.
- Screenshot required: Yes, if visual highlight or HUD actor is wrong.
- Console required: Yes, capture red errors or warnings.
- Safety rules checked: UI must not mutate HP, cooldown, acted, or moved state during selection.
- Phase 14.5 action allowed: document only / no fix.

## REG-02 Move mode

- Test ID: REG-02
- System: Movement / UI action targeting
- Scene: `GridTest.unity`
- Preconditions: A living player unit is selected and has not moved.
- Inspector bindings to verify first: `GridManager.initialTiles`, `BattleHUDController.playerUnits`, `TurnManager.playerUnits`
- Steps:
  1. Enter Play Mode.
  2. Select a living player unit.
  3. Click `Move`.
  4. Confirm green movement range appears.
  5. Click a valid green tile.
  6. Try clicking an invalid tile in a separate run.
- Expected result: Valid move goes through existing movement service, updates tile occupancy, marks moved through movement logic, and clears action highlight. Invalid tile does not move or consume unrelated state.
- Failure symptoms: Green range missing, unit moves without Move mode, invalid tile moves the unit, movement breaks selection, or HUD state does not refresh.
- Likely causes: `initialTiles` incomplete, movement controller binding issue, tile occupancy mismatch, or action mode state not clearing.
- Screenshot required: Yes, for missing or incorrect green range.
- Console required: Yes, for red errors or movement warnings.
- Safety rules checked: UI does not directly call `MarkMoved`; Move goes through existing movement service.
- Phase 14.5 action allowed: document only / no fix.

## REG-03 Basic Attack mode

- Test ID: REG-03
- System: UI action targeting / Combat
- Scene: `GridTest.unity`
- Preconditions: A living player unit is selected, has not acted, and at least one enemy is available.
- Inspector bindings to verify first: `UIActionModeController.enemyUnits`, `BattleHUDController.enemyUnits`, `BattleHUDController.playerUnits`
- Steps:
  1. Enter Play Mode.
  2. Select a living player unit.
  3. Click `Basic Attack`.
  4. Confirm valid enemies highlight red.
  5. Click a valid red enemy.
  6. Repeat with an invalid target or empty ground.
- Expected result: Red highlight uses read-only target validation. Real attack only runs after clicking a target and calls `CombatSystem.TryBasicAttack`. Invalid targets do not consume the action.
- Failure symptoms: Red highlight misses valid enemies, click attacks without targeting mode, invalid target consumes action, movement triggers during targeting, or CombatLog does not update.
- Likely causes: `enemyUnits` missing targets, target raycast issue, action mode not suspending movement input, or combat service binding issue.
- Screenshot required: Yes, for red highlight failures.
- Console required: Yes, capture combat rejection reasons.
- Safety rules checked: Highlight does not call `TryBasicAttack`; UI does not directly subtract HP or mark acted.
- Phase 14.5 action allowed: document only / no fix.

## REG-04 Skill Slot 0 mode

- Test ID: REG-04
- System: UI action targeting / Skill
- Scene: `GridTest.unity`
- Preconditions: A living player unit has Skill Slot 0 available and has not acted.
- Inspector bindings to verify first: `UIActionModeController.enemyUnits`, `BattleHUDController.enemyUnits`, `SkillEffectExecutor.knownUnits`
- Steps:
  1. Enter Play Mode.
  2. Select Fighter, Ranger, Mage, or Barbarian.
  3. Click `Skill Slot 0`.
  4. Confirm valid targets highlight yellow.
  5. Click a valid yellow enemy.
  6. Repeat with an invalid target or empty ground.
- Expected result: Yellow highlight uses read-only target validation. Real skill execution only runs after target click and calls `SkillSystem.TryUseSkill(selectedUnit, target, 0)`. Invalid targets do not consume action or cooldown.
- Failure symptoms: Yellow highlight misses valid enemies, invalid target consumes cooldown/action, UI changes cooldown directly, or skill result is missing from CombatLog.
- Likely causes: `enemyUnits` missing targets, `SkillRuntime` slot issue, skill system binding issue, cooldown state, or target validation rejection.
- Screenshot required: Yes, for yellow highlight failures.
- Console required: Yes, capture skill rejection reasons.
- Safety rules checked: Highlight does not call `TryUseSkill`; UI does not directly change cooldown or `MarkActed`.
- Phase 14.5 action allowed: document only / no fix.

## REG-05 Mage Fireball splash

- Test ID: REG-05
- System: Skill / Fireball splash
- Scene: `GridTest.unity`
- Preconditions: Mage is alive, has Skill Slot 0 ready, and at least one valid enemy is adjacent to the primary target.
- Inspector bindings to verify first: `SkillEffectExecutor.knownUnits`, `UIActionModeController.enemyUnits`, `BattleHUDController.enemyUnits`
- Steps:
  1. Enter Play Mode.
  2. Select Mage.
  3. Enter `Skill Slot 0` targeting.
  4. Click a valid target with adjacent enemies.
  5. Observe target HP, adjacent enemy HP, CombatLog, and HP bars.
- Expected result: Primary target takes Fireball damage. Adjacent valid enemies take splash damage. Splash damage produces structured `SkillEffectResult` entries and separate CombatLog rows.
- Failure symptoms: Primary target takes damage but adjacent enemy does not, splash is missing from CombatLog, HP bar does not update for splash target, or only summary text appears.
- Likely causes: `knownUnits` missing adjacent enemy, enemy not tiled/alive/active, spacing not adjacent by grid rules, or structured result not displayed.
- Screenshot required: Yes, for target layout and HP bars.
- Console required: Yes, capture skill logs or red errors.
- Safety rules checked: Do not modify `SkillEffectExecutor` formula; do not modify `DamageResolver`; CombatLog must not parse strings.
- Phase 14.5 action allowed: document only / no fix.

## REG-06 CombatLog structured entries

- Test ID: REG-06
- System: CombatLog / UI feedback
- Scene: `GridTest.unity`
- Preconditions: Basic Attack and Skill Slot 0 can be executed.
- Inspector bindings to verify first: `BattleHUDController.playerUnits`, `BattleHUDController.enemyUnits`, `SkillEffectExecutor.knownUnits`
- Steps:
  1. Enter Play Mode.
  2. Execute Basic Attack.
  3. Execute Skill Slot 0.
  4. Execute Mage Fireball splash if possible.
  5. Observe CombatFeedback UI.
- Expected result: CombatLog shows structured rows for D20, hit/miss, damage, death, skill use, skill effect, cooldown, and Fireball splash damage entries.
- Failure symptoms: CombatLog panel is empty, skill entries missing, splash row missing, death row missing, or display appears derived from parsed summary only.
- Likely causes: CombatLog binding missing, skill system not connected to CombatLog, `SkillEffectResult` not represented, or UI feedback filtering issue.
- Screenshot required: Yes, for CombatLog UI.
- Console required: Yes, capture related logs.
- Safety rules checked: CombatLog is structured event output and does not parse strings.
- Phase 14.5 action allowed: document only / no fix.

## REG-07 Enemy Floating HP Bar refresh

- Test ID: REG-07
- System: Enemy Floating HP Bar
- Scene: `GridTest.unity`
- Preconditions: Ordinary enemies are visible and alive.
- Inspector bindings to verify first: Enemy HP bar Unit reference, `BattleHUDController.enemyUnits`, `UIActionModeController.enemyUnits`
- Steps:
  1. Enter Play Mode.
  2. Confirm each ordinary enemy shows an HP bar.
  3. Damage an enemy with Basic Attack.
  4. Damage an enemy with Skill Slot 0.
  5. Damage adjacent targets with Fireball splash if possible.
- Expected result: HP bar fill changes after HP changes. Fill corresponds to current HP / max HP. Bars remain readable and do not block targeting.
- Failure symptoms: HP bar fill does not change, wrong enemy bar changes, bar disappears while enemy is alive, or click targeting is blocked by the bar.
- Likely causes: Stale Unit reference, prefab/scene override issue, fill image setup issue, or raycast target still enabled.
- Screenshot required: Yes, before/after damage.
- Console required: Yes, if errors occur.
- Safety rules checked: UI reads HP only; it does not directly modify HP.
- Phase 14.5 action allowed: document only / no fix.

## REG-08 Enemy HP Bar death hide

- Test ID: REG-08
- System: Enemy Floating HP Bar / Death
- Scene: `GridTest.unity`
- Preconditions: At least one ordinary enemy can be killed by Basic Attack or Skill Slot 0.
- Inspector bindings to verify first: Enemy HP bar Unit reference, `UIActionModeController.enemyUnits`, `SkillEffectExecutor.knownUnits`
- Steps:
  1. Enter Play Mode.
  2. Damage an enemy until it dies.
  3. Observe enemy object, tile occupancy, CombatLog death entry, and HP bar.
- Expected result: Dead enemy releases tile, CombatLog records death, and the HP bar hides.
- Failure symptoms: HP bar remains visible on dead enemy, death is not logged, tile remains occupied incorrectly, or dead enemy can still be targeted.
- Likely causes: HP bar Unit reference stale, death state not observed by HP bar, scene override issue, or target list still includes dead enemy for preview.
- Screenshot required: Yes, for dead enemy and bar state.
- Console required: Yes, capture death/combat logs.
- Safety rules checked: Death is handled by gameplay systems, not UI.
- Phase 14.5 action allowed: document only / no fix.

## REG-09 Room trigger

- Test ID: REG-09
- System: Room
- Scene: `GridTest.unity`
- Preconditions: A player can move to the room trigger area.
- Inspector bindings to verify first: `RoomEnemyActivator.assignedEnemies`, `RoomEnemyActivator.spawnTiles`, `LevelProgressionService.requiredClearedRooms`
- Steps:
  1. Enter Play Mode.
  2. Move a player into the room trigger area.
  3. Observe room state changes and feedback.
- Expected result: Room enters the expected state and starts the current room flow.
- Failure symptoms: Room state does not change, trigger is ignored, enemies do not activate, or room feedback is inconsistent.
- Likely causes: Trigger setup, missing assigned enemies, missing spawn tiles, or room controller binding mismatch.
- Screenshot required: Yes, for room state/scene view if possible.
- Console required: Yes, capture room logs/errors.
- Safety rules checked: Do not implement complex fog of war or rewrite room system.
- Phase 14.5 action allowed: document only / no fix.

## REG-10 Room shadow hide

- Test ID: REG-10
- System: Room shadow
- Scene: `GridTest.unity`
- Preconditions: Room trigger can be activated.
- Inspector bindings to verify first: Room shadow object references, `RoomEnemyActivator.assignedEnemies`
- Steps:
  1. Enter Play Mode.
  2. Enter the room.
  3. Observe room shadow visibility.
- Expected result: Room shadow hides/reveals according to the existing simple overlay behavior.
- Failure symptoms: Shadow never hides, hides before entry, remains blocking view, or behaves like an unintended fog system.
- Likely causes: Shadow root binding issue, room state not advancing, or room trigger failure.
- Screenshot required: Yes.
- Console required: Only if errors/warnings appear.
- Safety rules checked: Do not add complex fog of war.
- Phase 14.5 action allowed: document only / no fix.

## REG-11 Enemy activation

- Test ID: REG-11
- System: Room enemy activation
- Scene: `GridTest.unity`
- Preconditions: Room trigger can be activated and assigned enemies exist.
- Inspector bindings to verify first: `RoomEnemyActivator.assignedEnemies`, `RoomEnemyActivator.spawnTiles`, `GridManager.initialTiles`
- Steps:
  1. Enter Play Mode.
  2. Enter the room.
  3. Observe assigned enemy active state and placement.
- Expected result: Assigned enemies activate and are placed on intended spawn tiles without occupancy conflicts.
- Failure symptoms: Enemy remains inactive, appears at wrong tile, overlaps another unit, lacks HP bar, or cannot be targeted.
- Likely causes: Missing assigned enemy, missing spawn tile, order mismatch, invalid tile, or occupancy conflict.
- Screenshot required: Yes, for enemy placement.
- Console required: Yes, capture activation logs/errors.
- Safety rules checked: Do not rename or replace ordinary enemy prefabs.
- Phase 14.5 action allowed: document only / no fix.

## REG-12 Room clear

- Test ID: REG-12
- System: Room clear / progression
- Scene: `GridTest.unity`
- Preconditions: Room enemies can be activated and defeated.
- Inspector bindings to verify first: `RoomEnemyActivator.assignedEnemies`, `LevelProgressionService.requiredClearedRooms`
- Steps:
  1. Enter Play Mode.
  2. Activate room enemies.
  3. Defeat assigned enemies.
  4. Observe room clear state and progression condition.
- Expected result: Room clear occurs only after required assigned enemies are defeated or otherwise cleared.
- Failure symptoms: Room clears too early, never clears, Stairs condition remains wrong, or inactive/stale enemies affect clear state.
- Likely causes: `assignedEnemies` incomplete/stale, required room list mismatch, dead/active state mismatch, or room controller state issue.
- Screenshot required: Yes, for room state/progression prompt.
- Console required: Yes, capture room/progression logs.
- Safety rules checked: Do not implement true three-level scene loading in this phase.
- Phase 14.5 action allowed: document only / no fix.

## REG-13 Key pickup

- Test ID: REG-13
- System: Key / progression
- Scene: `GridTest.unity`
- Preconditions: Key exists and a player can interact with it.
- Inspector bindings to verify first: `KeyItem.progressionService`, `LevelProgressionService.requiredClearedRooms`
- Steps:
  1. Enter Play Mode.
  2. Select/move a player to interact with the Key according to current scene setup.
  3. Observe prompt/progression state.
- Expected result: Key pickup updates shared progression state and can deactivate or mark the key as collected according to current placeholder flow.
- Failure symptoms: Key click has no effect, Stairs never recognizes key, prompt does not update, or Console errors appear.
- Likely causes: Missing `KeyItem.progressionService`, interaction/raycast issue, or progression state mismatch.
- Screenshot required: Yes, for prompt/key state.
- Console required: Yes, capture key/progression logs.
- Safety rules checked: UI does not directly mutate key/progression state.
- Phase 14.5 action allowed: document only / no fix.

## REG-14 Stairs hover / second click

- Test ID: REG-14
- System: Stairs / placeholder progression
- Scene: `GridTest.unity`
- Preconditions: Stairs exists, a living player can be selected, and key/room conditions can be tested.
- Inspector bindings to verify first: `InteractableStairs.progressionService`, `InteractableStairs.selectionManager`, `LevelProgressionService.requiredClearedRooms`
- Steps:
  1. Enter Play Mode.
  2. Select a living player.
  3. Hover over Stairs if hover feedback is configured.
  4. Click Stairs before requirements are met.
  5. Meet key/room requirements.
  6. Click Stairs once, then click again for second-click confirmation.
- Expected result: Stairs shows hover/prompt feedback, rejects progression before requirements, and performs current placeholder progression after second click when requirements are met.
- Failure symptoms: Hover missing, first click immediately progresses without confirmation, second click ignored, wrong prompt, or no progression after requirements.
- Likely causes: Missing `progressionService`, missing `selectionManager`, missing feedback renderers/materials, required room mismatch, or placeholder progression state issue.
- Screenshot required: Yes, for prompt/hover state.
- Console required: Yes, capture stairs/progression logs.
- Safety rules checked: Current flow is placeholder progression, not true scene loading.
- Phase 14.5 action allowed: document only / no fix.

## REG-15 LevelUp

- Test ID: REG-15
- System: Level progression / LevelUp
- Scene: `GridTest.unity`
- Preconditions: Stairs placeholder progression can be completed.
- Inspector bindings to verify first: `LevelProgressionService.playerUnits`, `LevelProgressionService.requiredClearedRooms`, `BattleHUDController.playerUnits`
- Steps:
  1. Enter Play Mode.
  2. Meet Key and Room requirements.
  3. Trigger Stairs placeholder progression.
  4. Observe living player Level, MaxHP, CurrentHP, and HUD HeroPanel refresh.
- Expected result: Living player units level up according to current placeholder progression. MaxHP increases, HP refills, and HUD refreshes.
- Failure symptoms: Some players do not level up, HP does not refill, HeroPanel does not update, dead players update unexpectedly, or progression occurs without requirements.
- Likely causes: `LevelProgressionService.playerUnits` missing entries, `BattleHUDController.playerUnits` mismatch, `requiredClearedRooms` mismatch, or placeholder progression state issue.
- Screenshot required: Yes, before/after LevelUp.
- Console required: Yes, capture progression logs.
- Safety rules checked: Do not implement formal Level_01 / Level_02 / Level_03 scene loading in this phase.
- Phase 14.5 action allowed: document only / no fix.

## REG-16 Enemy AI turn

- Test ID: REG-16
- System: Enemy AI / Turn gate
- Scene: `GridTest.unity`
- Preconditions: At least one enemy is alive and active, and at least one player is alive.
- Inspector bindings to verify first: `TurnManager.playerUnits`, `BattleHUDController.playerUnits`, `RoomEnemyActivator.assignedEnemies`
- Steps:
  1. Enter Play Mode.
  2. Progress to EnemyTurn using the current test flow.
  3. Observe enemy target selection, movement, and attack behavior.
  4. Record any rejection logs or missing action.
- Expected result: Enemy AI can pick a living player target, move toward it when needed, and use the shared `CombatSystem.TryBasicAttack` when in range.
- Failure symptoms: Enemy turn starts but no enemy acts, enemy cannot move, enemy cannot attack, attack is rejected by action permission, or enemy attacks during the wrong phase.
- Likely causes: `TurnManager` / `ActionPermissionService` gate configured in a way that rejects enemy actions, room activation issue, enemy not tiled, target not alive/tiled, or AI service binding issue.
- Screenshot required: Yes, if enemy position/turn state is unclear.
- Console required: Yes, required for rejection reasons.
- Safety rules checked: Do not modify `CombatSystem.TryBasicAttack`; do not modify `DamageResolver`; reproduce before any future implementation proposal.
- Phase 14.5 action allowed: document only / no fix.

## Known failure recording section

Use this section during future manual testing. Do not fix findings in Phase 14.5.

| Test ID | Date | Result | Failure symptom | Screenshot path / note | Console note | Suspected cause | Follow-up phase needed |
| --- | --- | --- | --- | --- | --- | --- | --- |
|  |  |  |  |  |  |  |  |

## Git diff validation

Validation commands:

```powershell
git status --short
git diff --name-only
git diff -- Docs
```

Expected:

- Only `Docs/` files changed.
- No `Assets/`.
- No `Packages/`.
- No `ProjectSettings/`.
- No `.cs`.
- No `.unity`.
- No `.prefab`.
- No ScriptableObject `.asset`.

## Rollback

To roll back this checklist, remove or revert:

- `Docs/Phase14_RegressionChecklist.md`

Do not use broad reset commands if other Phase 14 documentation work is still in progress.
