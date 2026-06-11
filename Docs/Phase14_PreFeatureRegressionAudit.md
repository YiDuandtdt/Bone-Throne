# Phase 14.7 Pre-Feature Regression Audit

## 1. Purpose and scope

This document is a manual pre-feature regression audit template for the current Bone Throne Unity 6.3 LTS project.

Phase 14.7 only records regression audit results. It does not implement fixes, write gameplay code, modify scenes, modify prefabs, modify ScriptableObject assets, modify KayKit original resources, or change `Assets`, `Packages`, or `ProjectSettings`.

Use this document to guide local Unity 6.3 LTS testing in `GridTest.unity` and to record whether current systems are ready for later stabilization proposals or feature work.

## 2. Source of truth

Use these references:

- `AGENTS.md`
- `Docs/Phase14_ProjectStateAudit.md`
- `Docs/Phase14_StabilizationPlan.md`
- `Docs/Phase14_InspectorBindingChecklist.md`
- `Docs/Phase14_RegressionChecklist.md`
- `Docs/Phase14_MinimalStabilizationImplementationProposal.md`
- `Docs/骸骨王座_系统设计文档_Unity6.3LTS_v2.2_CurrentProjectState.md`
- `Docs/骸骨王座_Codex完整即用Vibecoding开发文档_Unity6.3LTS_v1.2_CurrentProjectState.md`
- `Docs/DevLogs/`

If any older Markdown displays encoding corruption in the current terminal, do not copy corrupted text. Reconstruct the fact from the Phase 14.1 audit result, v2.2 current-state design document, v1.2 Vibecoding document, Phase 14.4 stabilization plan, Phase 14.5 checklists, Phase 14.6 proposal, and DevLogs.

## 3. Audit rules and non-fix boundary

Rules:

- Do not write code.
- Do not modify scene / prefab / ScriptableObject asset / C#.
- Do not modify `Assets`, `Packages`, or `ProjectSettings`.
- Do not modify KayKit original resources.
- Do not fix findings in this phase.
- Only record observed results.
- Do not mark any observed failure as fixed.
- Every test result may be recorded as `Pass`, `Fail`, `Blocked`, or `Not Tested`.
- `Fix attempted: No` must remain true for every finding in this phase.

Safety boundaries:

- UI must not directly subtract HP.
- UI must not directly change cooldown.
- UI must not directly call `MarkActed`.
- UI must not directly call `MarkMoved`.
- Highlight preview must not call `TryBasicAttack`.
- Highlight preview must not call `TryUseSkill`.
- Do not casually modify `DamageResolver`.
- Do not casually modify `SkillEffectExecutor` skill formulas.
- Do not implement CombatLog by parsing strings.
- `Skeleton_Rogue` is the ordinary enemy.
- `Skeleton_Golem` is reserved for future Boss / heavy Boss only.
- Player Ranger uses Adventurers Rogue visual.
- Do not modify KayKit original resources.

## 4. Environment snapshot

Fill this before testing:

| Field | Value |
| --- | --- |
| Tester | User local Unity tester |
| Date | 2026-05-28 |
| Branch | phase/14-documentation-and-stabilization |
| Commit hash | fd2dc58 |
| Unity version | Unity 6.3 LTS |
| OS | Windows |
| Scene | `Assets/_BoneThrone/Scenes/GridTest.unity` |

## 5. Git status before audit

Record command output before testing:

```powershell
git status --short
```

Output:

```text
No output. Working tree was clean before recording this audit result.
```

Record changed file names before testing:

```powershell
git diff --name-only
```

Output:

```text
No output. No changed file names before recording this audit result.
```

## 6. Scene under test

- Only test `Assets/_BoneThrone/Scenes/GridTest.unity`.
- `Assets/_BoneThrone/Scenes/MainMenu.unity` remains a placeholder.
- Do not treat this audit as formal `Level_01`, `Level_02`, or `Level_03` acceptance.
- Do not modify `GridTest.unity` in this phase.

## 7. Console status before Play Mode

Record Console state before entering Play Mode:

| Field | Value |
| --- | --- |
| Errors | No blocking errors reported by user. |
| Warnings | No blocking warnings reported by user. |
| Logs | Not separately captured in this audit entry. |
| Whether Play Mode is blocked | No. User reported all tests passed. |
| Notes | User reported all GridTest regression tests passed. |

If Play Mode is blocked by Console errors, mark affected regression tests as `Blocked` and do not fix the issue in Phase 14.7.

## 8. Inspector binding audit

Status values: `Pass`, `Fail`, `Blocked`, `Not Tested`.

| Binding name | Scene object / component | Expected references | Actual references | Status: Pass / Fail / Blocked / Not Tested | Notes | Related regression tests | Related Phase 14.6 candidate |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `SkillEffectExecutor.knownUnits` | `SkillEffectExecutor` | Units needed for skill effects, especially Fireball splash targets. | Verified in `GridTest.unity`; no missing binding reported. | Pass | User reported Fireball splash and CombatLog checks passed. | REG-05, REG-06 | Candidate D |
| `BattleHUDController.enemyUnits` | `BattleHUDController` | Ordinary enemy scene instances. | Verified in `GridTest.unity`; no missing binding reported. | Pass | User reported Basic Attack, Skill Slot 0, and HP bar checks passed. | REG-03, REG-04, REG-07 | Candidate E |
| `UIActionModeController.enemyUnits` | `UIActionModeController` | Ordinary enemy scene instances used for red/yellow highlights. | Verified in `GridTest.unity`; no missing binding reported. | Pass | User reported red/yellow highlight checks passed. | REG-03, REG-04 | Candidate E |
| `BattleHUDController.playerUnits` | `BattleHUDController` | Fighter, Ranger, Mage, Barbarian scene instances. | Verified in `GridTest.unity`; no missing binding reported. | Pass | User reported player selection, HUD, CombatLog, and LevelUp checks passed. | REG-01, REG-02, REG-06, REG-15 | Candidate A |
| `TurnManager.playerUnits` | `TurnManager` | Player unit scene instances used by turn flow. | Verified in `GridTest.unity`; no missing binding reported. | Pass | User reported selection, movement, and Enemy AI turn checks passed. | REG-01, REG-02, REG-16 | Candidate B |
| `LevelProgressionService.playerUnits` | `LevelProgressionService` | Player unit scene instances used for LevelUp. | Verified in `GridTest.unity`; no missing binding reported. | Pass | User reported LevelUp passed. | REG-15 | Candidate H |
| `RoomEnemyActivator.assignedEnemies` | `RoomEnemyActivator` | Room enemy scene instances to activate and clear. | Verified in `GridTest.unity`; no missing binding reported. | Pass | User reported room trigger, activation, clear, and Enemy AI checks passed. | REG-09, REG-11, REG-12, REG-16 | Candidate G |
| `RoomEnemyActivator.spawnTiles` | `RoomEnemyActivator` | Spawn tiles aligned with assigned enemies. | Verified in `GridTest.unity`; no missing binding reported. | Pass | User reported room trigger and enemy activation checks passed. | REG-09, REG-11 | Candidate G |
| `LevelProgressionService.requiredClearedRooms` | `LevelProgressionService` | Rooms required before stairs progression. | Verified in `GridTest.unity`; no missing binding reported. | Pass | User reported room clear, key, stairs, and LevelUp checks passed. | REG-12, REG-13, REG-14, REG-15 | Candidate H |
| `GridManager.initialTiles` | `GridManager` | Current `GridTest.unity` tile objects. | Verified in `GridTest.unity`; no missing binding reported. | Pass | User reported selection, movement, and activation checks passed. | REG-01, REG-02, REG-11 | Candidate A |
| `KeyItem.progressionService` | `KeyItem` | Scene `LevelProgressionService`. | Verified in `GridTest.unity`; no missing binding reported. | Pass | User reported Key pickup and Stairs checks passed. | REG-13, REG-14 | Candidate H |
| `InteractableStairs.progressionService` | `InteractableStairs` | Scene `LevelProgressionService`. | Verified in `GridTest.unity`; no missing binding reported. | Pass | User reported Stairs and LevelUp checks passed. | REG-14, REG-15 | Candidate H |
| `InteractableStairs.selectionManager` | `InteractableStairs` | Scene `SelectionManager`. | Verified in `GridTest.unity`; no missing binding reported. | Pass | User reported Stairs hover / second click passed. | REG-14 | Candidate H |

## 9. Regression test result table

Status values: `Pass`, `Fail`, `Blocked`, `Not Tested`.

| Test ID | Test name | Result: Pass / Fail / Blocked / Not Tested | Notes | Related Phase 14.6 candidate |
| --- | --- | --- | --- | --- |
| REG-01 | Select player unit | Pass | User reported test passed. | Candidate A |
| REG-02 | Move mode | Pass | User reported test passed. | Candidate A |
| REG-03 | Basic Attack mode | Pass | User reported test passed. | Candidate E |
| REG-04 | Skill Slot 0 mode | Pass | User reported test passed. | Candidate E |
| REG-05 | Mage Fireball splash | Pass | User reported test passed. | Candidate D |
| REG-06 | CombatLog structured entries | Pass | User reported test passed. | Candidate D |
| REG-07 | Enemy Floating HP Bar refresh | Pass | User reported test passed. | Candidate F |
| REG-08 | Enemy HP Bar death hide | Pass | User reported test passed. | Candidate F |
| REG-09 | Room trigger | Pass | User reported test passed. | Candidate G |
| REG-10 | Room shadow hide | Pass | User reported test passed. | Candidate G |
| REG-11 | Enemy activation | Pass | User reported test passed. | Candidate G |
| REG-12 | Room clear | Pass | User reported test passed. | Candidate G |
| REG-13 | Key pickup | Pass | User reported test passed. | Candidate H |
| REG-14 | Stairs hover / second click | Pass | User reported test passed. | Candidate H |
| REG-15 | LevelUp | Pass | User reported test passed. | Candidate H |
| REG-16 | Enemy AI turn | Pass | User reported test passed. | Candidate B |

## 10. Individual test result notes

Use these templates during manual testing. Do not fix failures.

### REG-01 Select player unit

| Field | Value |
| --- | --- |
| Test ID | REG-01 |
| System | Selection / HUD |
| Scene | `GridTest.unity` |
| Preconditions | Unity opens with no red Console compile errors. `GridTest.unity` is loaded. |
| Inspector bindings checked first | `BattleHUDController.playerUnits`, `TurnManager.playerUnits`, `GridManager.initialTiles` |
| Steps run | Executed by user in Unity 6.3 LTS using `GridTest.unity` and the Phase 14.5 regression checklist. |
| Expected result | Living player units can be selected. Selected tile shows blue highlight. TurnBanner / HeroPanel reflects selected actor. Clicking selected unit again clears selection and selected highlight. |
| Actual result | Passed; expected behavior observed according to user report. |
| Result | Pass |
| Screenshot / video evidence | Not provided. |
| Console errors / warnings | No blocking Console errors or warnings reported by user. |
| Likely cause | N/A - no failure observed. |
| Phase 14.6 candidate mapping | Candidate A |
| Follow-up recommendation | No stabilization implementation candidate is currently supported by observed failure evidence. |
| Fix attempted | No |

### REG-02 Move mode

| Field | Value |
| --- | --- |
| Test ID | REG-02 |
| System | Movement / UI action targeting |
| Scene | `GridTest.unity` |
| Preconditions | A living player unit is selected and has not moved. |
| Inspector bindings checked first | `GridManager.initialTiles`, `BattleHUDController.playerUnits`, `TurnManager.playerUnits` |
| Steps run | Executed by user in Unity 6.3 LTS using `GridTest.unity` and the Phase 14.5 regression checklist. |
| Expected result | Move mode shows green range. Valid move uses existing movement service, updates occupancy, marks moved through movement logic, and clears action highlight. Invalid tile does not move or consume unrelated state. |
| Actual result | Passed; expected behavior observed according to user report. |
| Result | Pass |
| Screenshot / video evidence | Not provided. |
| Console errors / warnings | No blocking Console errors or warnings reported by user. |
| Likely cause | N/A - no failure observed. |
| Phase 14.6 candidate mapping | Candidate A |
| Follow-up recommendation | No stabilization implementation candidate is currently supported by observed failure evidence. |
| Fix attempted | No |

### REG-03 Basic Attack mode

| Field | Value |
| --- | --- |
| Test ID | REG-03 |
| System | UI action targeting / Combat |
| Scene | `GridTest.unity` |
| Preconditions | A living player unit is selected, has not acted, and at least one enemy is available. |
| Inspector bindings checked first | `UIActionModeController.enemyUnits`, `BattleHUDController.enemyUnits`, `BattleHUDController.playerUnits` |
| Steps run | Executed by user in Unity 6.3 LTS using `GridTest.unity` and the Phase 14.5 regression checklist. |
| Expected result | Red highlight uses read-only target validation. Real attack only runs after clicking a target and calls `CombatSystem.TryBasicAttack`. Invalid targets do not consume action. |
| Actual result | Passed; expected behavior observed according to user report. |
| Result | Pass |
| Screenshot / video evidence | Not provided. |
| Console errors / warnings | No blocking Console errors or warnings reported by user. |
| Likely cause | N/A - no failure observed. |
| Phase 14.6 candidate mapping | Candidate E |
| Follow-up recommendation | No stabilization implementation candidate is currently supported by observed failure evidence. |
| Fix attempted | No |

### REG-04 Skill Slot 0 mode

| Field | Value |
| --- | --- |
| Test ID | REG-04 |
| System | UI action targeting / Skill |
| Scene | `GridTest.unity` |
| Preconditions | A living player unit has Skill Slot 0 available and has not acted. |
| Inspector bindings checked first | `UIActionModeController.enemyUnits`, `BattleHUDController.enemyUnits`, `SkillEffectExecutor.knownUnits` |
| Steps run | Executed by user in Unity 6.3 LTS using `GridTest.unity` and the Phase 14.5 regression checklist. |
| Expected result | Yellow highlight uses read-only target validation. Real skill execution only runs after target click and calls `SkillSystem.TryUseSkill(selectedUnit, target, 0)`. Invalid targets do not consume action or cooldown. |
| Actual result | Passed; expected behavior observed according to user report. |
| Result | Pass |
| Screenshot / video evidence | Not provided. |
| Console errors / warnings | No blocking Console errors or warnings reported by user. |
| Likely cause | N/A - no failure observed. |
| Phase 14.6 candidate mapping | Candidate E |
| Follow-up recommendation | No stabilization implementation candidate is currently supported by observed failure evidence. |
| Fix attempted | No |

### REG-05 Mage Fireball splash

| Field | Value |
| --- | --- |
| Test ID | REG-05 |
| System | Skill / Fireball splash |
| Scene | `GridTest.unity` |
| Preconditions | Mage is alive, has Skill Slot 0 ready, and at least one valid enemy is adjacent to the primary target. |
| Inspector bindings checked first | `SkillEffectExecutor.knownUnits`, `UIActionModeController.enemyUnits`, `BattleHUDController.enemyUnits` |
| Steps run | Executed by user in Unity 6.3 LTS using `GridTest.unity` and the Phase 14.5 regression checklist. |
| Expected result | Primary target takes Fireball damage. Adjacent valid enemies take splash damage. Splash damage produces structured `SkillEffectResult` entries and separate CombatLog rows. |
| Actual result | Passed; expected behavior observed according to user report. |
| Result | Pass |
| Screenshot / video evidence | Not provided. |
| Console errors / warnings | No blocking Console errors or warnings reported by user. |
| Likely cause | N/A - no failure observed. |
| Phase 14.6 candidate mapping | Candidate D |
| Follow-up recommendation | No stabilization implementation candidate is currently supported by observed failure evidence. |
| Fix attempted | No |

### REG-06 CombatLog structured entries

| Field | Value |
| --- | --- |
| Test ID | REG-06 |
| System | CombatLog / UI feedback |
| Scene | `GridTest.unity` |
| Preconditions | Basic Attack and Skill Slot 0 can be executed. |
| Inspector bindings checked first | `BattleHUDController.playerUnits`, `BattleHUDController.enemyUnits`, `SkillEffectExecutor.knownUnits` |
| Steps run | Executed by user in Unity 6.3 LTS using `GridTest.unity` and the Phase 14.5 regression checklist. |
| Expected result | CombatLog shows structured rows for D20, hit/miss, damage, death, skill use, skill effect, cooldown, and Fireball splash damage entries. |
| Actual result | Passed; expected behavior observed according to user report. |
| Result | Pass |
| Screenshot / video evidence | Not provided. |
| Console errors / warnings | No blocking Console errors or warnings reported by user. |
| Likely cause | N/A - no failure observed. |
| Phase 14.6 candidate mapping | Candidate D |
| Follow-up recommendation | No stabilization implementation candidate is currently supported by observed failure evidence. |
| Fix attempted | No |

### REG-07 Enemy Floating HP Bar refresh

| Field | Value |
| --- | --- |
| Test ID | REG-07 |
| System | Enemy Floating HP Bar |
| Scene | `GridTest.unity` |
| Preconditions | Ordinary enemies are visible and alive. |
| Inspector bindings checked first | Enemy HP bar Unit reference, `BattleHUDController.enemyUnits`, `UIActionModeController.enemyUnits` |
| Steps run | Executed by user in Unity 6.3 LTS using `GridTest.unity` and the Phase 14.5 regression checklist. |
| Expected result | HP bar fill changes after HP changes. Fill corresponds to current HP / max HP. Bars remain readable and do not block targeting. |
| Actual result | Passed; expected behavior observed according to user report. |
| Result | Pass |
| Screenshot / video evidence | Not provided. |
| Console errors / warnings | No blocking Console errors or warnings reported by user. |
| Likely cause | N/A - no failure observed. |
| Phase 14.6 candidate mapping | Candidate F |
| Follow-up recommendation | No stabilization implementation candidate is currently supported by observed failure evidence. |
| Fix attempted | No |

### REG-08 Enemy HP Bar death hide

| Field | Value |
| --- | --- |
| Test ID | REG-08 |
| System | Enemy Floating HP Bar / Death |
| Scene | `GridTest.unity` |
| Preconditions | At least one ordinary enemy can be killed by Basic Attack or Skill Slot 0. |
| Inspector bindings checked first | Enemy HP bar Unit reference, `UIActionModeController.enemyUnits`, `SkillEffectExecutor.knownUnits` |
| Steps run | Executed by user in Unity 6.3 LTS using `GridTest.unity` and the Phase 14.5 regression checklist. |
| Expected result | Dead enemy releases tile, CombatLog records death, and the HP bar hides. |
| Actual result | Passed; expected behavior observed according to user report. |
| Result | Pass |
| Screenshot / video evidence | Not provided. |
| Console errors / warnings | No blocking Console errors or warnings reported by user. |
| Likely cause | N/A - no failure observed. |
| Phase 14.6 candidate mapping | Candidate F |
| Follow-up recommendation | No stabilization implementation candidate is currently supported by observed failure evidence. |
| Fix attempted | No |

### REG-09 Room trigger

| Field | Value |
| --- | --- |
| Test ID | REG-09 |
| System | Room |
| Scene | `GridTest.unity` |
| Preconditions | A player can move to the room trigger area. |
| Inspector bindings checked first | `RoomEnemyActivator.assignedEnemies`, `RoomEnemyActivator.spawnTiles`, `LevelProgressionService.requiredClearedRooms` |
| Steps run | Executed by user in Unity 6.3 LTS using `GridTest.unity` and the Phase 14.5 regression checklist. |
| Expected result | Room enters the expected state and starts the current room flow. |
| Actual result | Passed; expected behavior observed according to user report. |
| Result | Pass |
| Screenshot / video evidence | Not provided. |
| Console errors / warnings | No blocking Console errors or warnings reported by user. |
| Likely cause | N/A - no failure observed. |
| Phase 14.6 candidate mapping | Candidate G |
| Follow-up recommendation | No stabilization implementation candidate is currently supported by observed failure evidence. |
| Fix attempted | No |

### REG-10 Room shadow hide

| Field | Value |
| --- | --- |
| Test ID | REG-10 |
| System | Room shadow |
| Scene | `GridTest.unity` |
| Preconditions | Room trigger can be activated. |
| Inspector bindings checked first | Room shadow object references, `RoomEnemyActivator.assignedEnemies` |
| Steps run | Executed by user in Unity 6.3 LTS using `GridTest.unity` and the Phase 14.5 regression checklist. |
| Expected result | Room shadow hides/reveals according to the existing simple overlay behavior. |
| Actual result | Passed; expected behavior observed according to user report. |
| Result | Pass |
| Screenshot / video evidence | Not provided. |
| Console errors / warnings | No blocking Console errors or warnings reported by user. |
| Likely cause | N/A - no failure observed. |
| Phase 14.6 candidate mapping | Candidate G |
| Follow-up recommendation | No stabilization implementation candidate is currently supported by observed failure evidence. |
| Fix attempted | No |

### REG-11 Enemy activation

| Field | Value |
| --- | --- |
| Test ID | REG-11 |
| System | Room enemy activation |
| Scene | `GridTest.unity` |
| Preconditions | Room trigger can be activated and assigned enemies exist. |
| Inspector bindings checked first | `RoomEnemyActivator.assignedEnemies`, `RoomEnemyActivator.spawnTiles`, `GridManager.initialTiles` |
| Steps run | Executed by user in Unity 6.3 LTS using `GridTest.unity` and the Phase 14.5 regression checklist. |
| Expected result | Assigned enemies activate and are placed on intended spawn tiles without occupancy conflicts. |
| Actual result | Passed; expected behavior observed according to user report. |
| Result | Pass |
| Screenshot / video evidence | Not provided. |
| Console errors / warnings | No blocking Console errors or warnings reported by user. |
| Likely cause | N/A - no failure observed. |
| Phase 14.6 candidate mapping | Candidate G |
| Follow-up recommendation | No stabilization implementation candidate is currently supported by observed failure evidence. |
| Fix attempted | No |

### REG-12 Room clear

| Field | Value |
| --- | --- |
| Test ID | REG-12 |
| System | Room clear / progression |
| Scene | `GridTest.unity` |
| Preconditions | Room enemies can be activated and defeated. |
| Inspector bindings checked first | `RoomEnemyActivator.assignedEnemies`, `LevelProgressionService.requiredClearedRooms` |
| Steps run | Executed by user in Unity 6.3 LTS using `GridTest.unity` and the Phase 14.5 regression checklist. |
| Expected result | Room clear occurs only after required assigned enemies are defeated or otherwise cleared. |
| Actual result | Passed; expected behavior observed according to user report. |
| Result | Pass |
| Screenshot / video evidence | Not provided. |
| Console errors / warnings | No blocking Console errors or warnings reported by user. |
| Likely cause | N/A - no failure observed. |
| Phase 14.6 candidate mapping | Candidate G |
| Follow-up recommendation | No stabilization implementation candidate is currently supported by observed failure evidence. |
| Fix attempted | No |

### REG-13 Key pickup

| Field | Value |
| --- | --- |
| Test ID | REG-13 |
| System | Key / progression |
| Scene | `GridTest.unity` |
| Preconditions | Key exists and a player can interact with it. |
| Inspector bindings checked first | `KeyItem.progressionService`, `LevelProgressionService.requiredClearedRooms` |
| Steps run | Executed by user in Unity 6.3 LTS using `GridTest.unity` and the Phase 14.5 regression checklist. |
| Expected result | Key pickup updates shared progression state and can deactivate or mark the key as collected according to current placeholder flow. |
| Actual result | Passed; expected behavior observed according to user report. |
| Result | Pass |
| Screenshot / video evidence | Not provided. |
| Console errors / warnings | No blocking Console errors or warnings reported by user. |
| Likely cause | N/A - no failure observed. |
| Phase 14.6 candidate mapping | Candidate H |
| Follow-up recommendation | No stabilization implementation candidate is currently supported by observed failure evidence. |
| Fix attempted | No |

### REG-14 Stairs hover / second click

| Field | Value |
| --- | --- |
| Test ID | REG-14 |
| System | Stairs / placeholder progression |
| Scene | `GridTest.unity` |
| Preconditions | Stairs exists, a living player can be selected, and key/room conditions can be tested. |
| Inspector bindings checked first | `InteractableStairs.progressionService`, `InteractableStairs.selectionManager`, `LevelProgressionService.requiredClearedRooms` |
| Steps run | Executed by user in Unity 6.3 LTS using `GridTest.unity` and the Phase 14.5 regression checklist. |
| Expected result | Stairs shows hover/prompt feedback, rejects progression before requirements, and performs current placeholder progression after second click when requirements are met. |
| Actual result | Passed; expected behavior observed according to user report. |
| Result | Pass |
| Screenshot / video evidence | Not provided. |
| Console errors / warnings | No blocking Console errors or warnings reported by user. |
| Likely cause | N/A - no failure observed. |
| Phase 14.6 candidate mapping | Candidate H |
| Follow-up recommendation | No stabilization implementation candidate is currently supported by observed failure evidence. |
| Fix attempted | No |

### REG-15 LevelUp

| Field | Value |
| --- | --- |
| Test ID | REG-15 |
| System | Level progression / LevelUp |
| Scene | `GridTest.unity` |
| Preconditions | Stairs placeholder progression can be completed. |
| Inspector bindings checked first | `LevelProgressionService.playerUnits`, `LevelProgressionService.requiredClearedRooms`, `BattleHUDController.playerUnits` |
| Steps run | Executed by user in Unity 6.3 LTS using `GridTest.unity` and the Phase 14.5 regression checklist. |
| Expected result | Living player units level up according to current placeholder progression. MaxHP increases, HP refills, and HUD refreshes. |
| Actual result | Passed; expected behavior observed according to user report. |
| Result | Pass |
| Screenshot / video evidence | Not provided. |
| Console errors / warnings | No blocking Console errors or warnings reported by user. |
| Likely cause | N/A - no failure observed. |
| Phase 14.6 candidate mapping | Candidate H |
| Follow-up recommendation | No stabilization implementation candidate is currently supported by observed failure evidence. |
| Fix attempted | No |

### REG-16 Enemy AI turn

| Field | Value |
| --- | --- |
| Test ID | REG-16 |
| System | Enemy AI / Turn gate |
| Scene | `GridTest.unity` |
| Preconditions | At least one enemy is alive and active, and at least one player is alive. |
| Inspector bindings checked first | `TurnManager.playerUnits`, `BattleHUDController.playerUnits`, `RoomEnemyActivator.assignedEnemies` |
| Steps run | Executed by user in Unity 6.3 LTS using `GridTest.unity` and the Phase 14.5 regression checklist. |
| Expected result | Enemy AI can pick a living player target, move toward it when needed, and use the shared `CombatSystem.TryBasicAttack` when in range. |
| Actual result | Passed; expected behavior observed according to user report. |
| Result | Pass |
| Screenshot / video evidence | Not provided. |
| Console errors / warnings | No blocking Console errors or warnings reported by user. |
| Likely cause | N/A - no failure observed. |
| Phase 14.6 candidate mapping | Candidate B |
| Follow-up recommendation | No stabilization implementation candidate is currently supported by observed failure evidence. |
| Fix attempted | No |

## 11. Console errors / warnings log

| Time observed | Before / During / After Play Mode | Severity | Full message | Stack trace | Triggering test ID | Repro steps | Blocks testing | Candidate mapping |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| N/A | During Play Mode | Log | User reported all tests passed; no blocking Console issue reported. | N/A | All | Full GridTest regression pass. | No | None |

## 12. Screenshot / evidence log

| Evidence ID | Test ID | Screenshot / video path or description | What it shows | Related failure | Candidate mapping |
| --- | --- | --- | --- | --- | --- |
| N/A | All | No screenshot/video provided. | User reported all tests passed. | None | None |

## 13. Observed failure to likely cause mapping

Only map actual observed failures. Do not mark a candidate as supported if the failure was not reproduced.

| Observed failure | Likely cause | Phase 14.6 candidate |
| --- | --- | --- |
| Missing binding, empty array, or stale reference. | Manual Inspector dependency is missing, stale, or incomplete. | Candidate A |
| Enemy AI does not act or action permission rejects enemy action. | Enemy AI may be blocked by `TurnManager` / `ActionPermissionService`. | Candidate B |
| Cooldown does not tick or skill cooldown blocks current gameplay. | Cooldown tick flow is not fully connected to turn-end flow. | Candidate C |
| Fireball splash misses adjacent enemy. | `SkillEffectExecutor.knownUnits` may be incomplete or stale. | Candidate D |
| Red or yellow highlight misses valid enemy. | `enemyUnits` may be incomplete or stale. | Candidate E |
| Enemy HP Bar does not refresh, does not hide, or blocks clicks. | HP bar Unit reference, refresh, death-hide, raycast, or override issue. | Candidate F |
| Room does not activate, enemy does not place, or room clear is wrong. | `assignedEnemies` / `spawnTiles` / room clear bindings may be missing or mismatched. | Candidate G |
| Key / Stairs / LevelUp placeholder behavior is confusing or wrong. | Placeholder progression boundary or binding issue. | Candidate H |

## 14. Mapping to Phase 14.6 stabilization candidates

### Supported by observed evidence

Only add candidates here after an actual `Fail` or `Blocked` result with notes, Console output, or screenshot evidence.

| Candidate | Evidence | Related test ID | Recommended follow-up |
| --- | --- | --- | --- |
| None | No observed failures; all reported tests passed. | All | No follow-up fix proposal supported by evidence. |

### Not yet supported by evidence

List candidates that have not been supported by actual observed failures.

| Candidate | Reason not supported yet |
| --- | --- |
| Candidate A | Not supported by observed failure evidence; Inspector-related checks and dependent regression tests were reported as passed. |
| Candidate B | Not supported by observed failure evidence; Enemy AI turn was reported as passed. |
| Candidate C | Not supported by observed failure evidence; no cooldown-blocking failure was reported. |
| Candidate D | Not supported by observed failure evidence; Fireball splash and CombatLog structured entries were reported as passed. |
| Candidate E | Not supported by observed failure evidence; Basic Attack and Skill Slot 0 targeting were reported as passed. |
| Candidate F | Not supported by observed failure evidence; Enemy HP Bar refresh and death hide were reported as passed. |
| Candidate G | Not supported by observed failure evidence; Room trigger, shadow, activation, and clear were reported as passed. |
| Candidate H | Not supported by observed failure evidence; Key, Stairs, and LevelUp were reported as passed. |

### Blocked by missing test data

List candidates that cannot be evaluated because tests were blocked or not run.

| Candidate | Missing data / blocked reason | Needed next action |
| --- | --- | --- |
| None | No test data missing; user reported all required tests passed. | No immediate action. |

## 15. Issues that cannot be fixed in Phase 14.7

Record only. Do not fix:

- Inspector missing bindings.
- Console errors.
- UI highlight missing targets.
- Fireball splash missing targets.
- CombatLog missing structured rows.
- Enemy HP Bar issues.
- Enemy AI gate issues.
- Room activation / clear issues.
- Key / Stairs / LevelUp issues.
- `DamageResolver` issues.
- `SkillEffectExecutor` issues.
- `CombatSystem.TryBasicAttack` issues.
- `SkillSystem.TryUseSkill` issues.

## 16. Recommended next phase

Use these decision rules after the user fills the audit:

- If most tests are `Not Tested`: continue manual testing. Do not enter implementation.
- If most tests are `Blocked` by Console errors: create a separate diagnostic proposal phase. Do not fix inside Phase 14.7.
- If only Inspector bindings are missing: consider a future Inspector-only correction phase, but only if the user explicitly approves scene/prefab binding edits.
- If Enemy AI gate failure is actually reproduced: consider a separate Enemy AI gate minimal fix proposal.
- If UI target list failure is actually reproduced: consider a separate UI target provider proposal / implementation split.
- If Fireball splash failure is actually reproduced: consider a separate Fireball `knownUnits` dependency proposal. Do not modify skill formulas casually.
- If Enemy HP Bar failure is actually reproduced: consider a separate HP bar robustness proposal / implementation split.
- If Room activation / clear failure is actually reproduced: consider a separate room validation helper proposal.
- If Key / Stairs / LevelUp placeholder confusion is observed: keep it as placeholder boundary or propose clearer prompts in a future phase.
- Do not directly enter Boss, LAN, or formal three-floor content until this audit has clear results and stabilization risks are addressed or consciously accepted.

## 17. Git diff validation

Phase 14.7 allows Docs-only changes.

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

## 18. Rollback

To roll back Phase 14.7 documentation only, remove:

- `Docs/Phase14_PreFeatureRegressionAudit.md`
- `Docs/DevLogs/Phase14.7_PreFeatureRegressionAudit.md`

If `Docs/ACTIVE_TASK.md` is modified in a future documentation update, manually restore only the Phase 14.7-related text. Do not use broad reset commands if other Phase 14 documentation work is still in progress.

