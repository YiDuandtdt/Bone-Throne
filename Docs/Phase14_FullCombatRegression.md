# Phase 14 Full Combat Regression

## 1. Purpose and Scope
This document is the manual Unity Play Mode regression checklist for Phase 14 combat. Phase 14.20 is documentation-only: testers record results, screenshots, videos, and console evidence here or in a copied test run sheet.

Do not fix issues during this phase. Do not edit gameplay code, assets, prefabs, scenes, packages, or project settings while executing this regression.

## 2. Source of Truth
- `Docs/ACTIVE_TASK.md`
- `Docs/DevLogs/Phase14.15_TurnSystemCompletionImplementation.md`
- `Docs/DevLogs/Phase14.15B_EndTurnButtonUI.md`
- `Docs/DevLogs/Phase14.15C_FreePlayerTurnOrder.md`
- `Docs/DevLogs/Phase14.15D_PlayerFootTileIndicator.md`
- `Docs/DevLogs/Phase14.17_DefendPotionSkillAvailabilityImplementation.md`
- `Docs/DevLogs/Phase14.19_SkillRebuildImplementation.md`
- `Docs/DevLogs/Phase14.19A_SkillBalanceAndBugfix.md`
- `Docs/DevLogs/Phase14.19B_EndedPlayerFootTileGreyIndicator.md`
- `Docs/DevLogs/Phase14.19E_RestoreSkillBarUIFromGoodBaseline.md`
- `Docs/骸骨王座_系统设计文档_Unity6.3LTS_v2.2_CurrentProjectState.md`
- `Docs/骸骨王座_Codex完整即用Vibecoding开发文档_Unity6.3LTS_v1.2_CurrentProjectState.md`

## 3. Environment Snapshot
Fill before testing.

| Field | Value |
|---|---|
| Unity version | Unity 6.3 LTS |
| Scene under test | `Assets/_BoneThrone/Scenes/GridTest.unity` |
| Branch | `phase/14-documentation-and-stabilization` |
| Commit | `573c946` |
| Date | 2026-05-29 |
| Tester |  |
| Console status before test |  |

## 4. Status Definitions
| Status | Definition |
|---|---|
| Pass | Expected result is fully observed with no blocking console red errors. |
| Fail | Expected result is not observed, or a regression / red error appears. |
| Blocked | Tester cannot execute because setup, scene state, or prior failure prevents validation. |
| Not Tested | Test was intentionally skipped or not reached in this run. |

## 5. Git Status Before Test
Run and paste output before Play Mode testing:

```powershell
git status --short --untracked-files=all
git diff --name-only
git diff --check
```

## 6. Global No-Fix Rule
Phase 14.20 is regression-only. If a bug is found, record it in the issue log. Do not repair, tune, rebind, edit scene objects, change prefabs, change SkillData, or patch code during this phase. Fixes move to Phase 14.20-A Minimal Regression Fix.

## 7. UI Regression Checklist
| Test ID | Item | Expected | Actual | Status | Evidence / Notes |
|---|---|---|---|---|---|
| UI-01 | SkillBar good baseline | SkillBar matches Phase 14.19-B pre-regression good baseline. |  |  |  |
| UI-02 | All actions visible | Move / Basic Attack / Skill 0 / Skill 1 / Skill 2 / Defend / Potion / End Turn are visible. |  |  |  |
| UI-03 | Bounds | UI does not overflow screen top, bottom, left, or right. |  |  |  |
| UI-04 | Empty slot | Empty skill slot is grey and not clickable. |  |  |  |
| UI-05 | Locked slot | Locked skill slot is grey and not clickable. |  |  |  |
| UI-06 | Cooldown slot | Cooldown skill slot is grey/cooldown-colored and not clickable. |  |  |  |
| UI-07 | Ready slot | Ready skill slot is clickable. |  |  |  |
| UI-08 | Defend state | Defend enabled only when selected unit can act. |  |  |  |
| UI-09 | Potion state | Potion enabled only when selected unit can act, has potion, and is damaged. |  |  |  |
| UI-10 | End Turn state | End Turn enabled in PlayerTurn and disabled / ignored in EnemyTurn. |  |  |  |
| UI-11 | Console | No UI red errors appear. |  |  |  |

## 8. Free Player Turn Order Checklist
| Test ID | Item | Expected | Actual | Status | Evidence / Notes |
|---|---|---|---|---|---|
| TURN-01 | PlayerTurn banner | PlayerTurn communicates free unit selection. |  |  |  |
| TURN-02 | No fixed opener | Fighter is not forced to start. |  |  |  |
| TURN-03 | Any unit first | Tester can select any alive not-ended player first. |  |  |  |
| TURN-04 | Active state | Selected player displays Active. |  |  |  |
| TURN-05 | Ended state | End Turn changes selected player to Ended. |  |  |  |
| TURN-06 | Ended selection lock | Ended player cannot be selected again this round. |  |  |  |
| TURN-07 | First three ended | PlayerTurn remains active after first 3 alive players end. |  |  |  |
| TURN-08 | All ended | Fourth alive player ending enters EnemyTurn. |  |  |  |
| TURN-09 | Enemy complete | EnemyTurn completes and returns to PlayerTurn. |  |  |  |
| TURN-10 | New round reset | New PlayerTurn sets all alive players to Not Started. |  |  |  |

## 9. Player Foot Tile Checklist
| Test ID | Item | Expected | Actual | Status | Evidence / Notes |
|---|---|---|---|---|---|
| FOOT-01 | Not-ended alive player | Foot tile is white. |  |  |  |
| FOOT-02 | Ended alive player | Foot tile is grey. |  |  |  |
| FOOT-03 | Immediate grey | End Turn changes tile directly to grey, with no white flash. |  |  |  |
| FOOT-04 | New PlayerTurn | All alive players return to white. |  |  |  |
| FOOT-05 | Selected overlay | Selected highlight can cover white/grey. |  |  |  |
| FOOT-06 | Move overlay | Move range can cover white/grey. |  |  |  |
| FOOT-07 | Attack overlay | Attack highlight can cover white/grey. |  |  |  |
| FOOT-08 | Skill overlay | Skill highlight can cover white/grey. |  |  |  |
| FOOT-09 | Clear restore | Clear restores correct white/grey baseline. |  |  |  |
| FOOT-10 | Enemy tile | Enemy tile is not white or grey. |  |  |  |
| FOOT-11 | Dead player | Dead player old tile does not keep white/grey. |  |  |  |

## 10. End Turn Checklist
| Test ID | Item | Expected | Actual | Status | Evidence / Notes |
|---|---|---|---|---|---|
| END-01 | No move/no action | Unit can End Turn. |  |  |  |
| END-02 | After move | Unit can End Turn. |  |  |  |
| END-03 | After attack | Unit can End Turn. |  |  |  |
| END-04 | After skill | Unit can End Turn. |  |  |  |
| END-05 | After Defend | Unit can End Turn. |  |  |  |
| END-06 | After Potion | Unit can End Turn. |  |  |  |
| END-07 | No MarkMoved/MarkActed | End Turn itself does not mark moved or acted. |  |  |  |
| END-08 | Selected only | End Turn ends only selected player unit. |  |  |  |
| END-09 | No direct runner call | End Turn does not directly call EnemyTurnRunner. |  |  |  |
| END-10 | All-ended transition | TurnManager enters EnemyTurn after all alive players ended. |  |  |  |

## 11. Defend Checklist
| Test ID | Item | Expected | Actual | Status | Evidence / Notes |
|---|---|---|---|---|---|
| DEF-01 | Availability | Selected alive unacted player can Defend. |  |  |  |
| DEF-02 | Consumes action | Defend marks acted. |  |  |  |
| DEF-03 | Blocks actions | After Defend, Attack / Skill / Potion / Defend unavailable. |  |  |  |
| DEF-04 | No auto end | Defend does not End Turn automatically. |  |  |  |
| DEF-05 | No movement cost | Defend does not MarkMoved. |  |  |  |
| DEF-06 | CombatLog | Defend log entry appears. |  |  |  |
| DEF-07 | Basic damage reduce | Basic Attack against defending target is reduced. |  |  |  |
| DEF-08 | Skill damage reduce | Skill damage against defending target is reduced. |  |  |  |
| DEF-09 | Amplify order | DamageAmplify applies before Defend reduction. |  |  |  |
| DEF-10 | One-hit clear | Defend state clears after one damage event. |  |  |  |

## 12. Potion Checklist
| Test ID | Item | Expected | Actual | Status | Evidence / Notes |
|---|---|---|---|---|---|
| POT-01 | Damaged use | Damaged player can use Potion. |  |  |  |
| POT-02 | Full HP | Full HP Potion is disabled or shows prompt. |  |  |  |
| POT-03 | Heal amount | Potion heals 4 HP or clamps to max HP. |  |  |  |
| POT-04 | Count | Potion count changes from 1 to 0. |  |  |  |
| POT-05 | Empty state | Potion becomes grey / unavailable after use. |  |  |  |
| POT-06 | Consumes action | Potion marks acted. |  |  |  |
| POT-07 | Blocks actions | After Potion, Attack / Skill / Defend unavailable. |  |  |  |
| POT-08 | No auto end | Potion does not End Turn automatically. |  |  |  |
| POT-09 | Logs | Potion / Heal log entries appear. |  |  |  |

## 13. Skill Availability Checklist
| Test ID | Item | Expected | Actual | Status | Evidence / Notes |
|---|---|---|---|---|---|
| AVAIL-01 | Empty | Empty skill cannot be clicked. |  |  |  |
| AVAIL-02 | Locked | Locked skill cannot be clicked. |  |  |  |
| AVAIL-03 | Cooldown | Cooldown skill cannot be clicked. |  |  |  |
| AVAIL-04 | Ready | Ready skill can be clicked. |  |  |  |
| AVAIL-05 | Acted player | Skills unavailable after acted. |  |  |  |
| AVAIL-06 | Ended player | Skills unavailable after ended. |  |  |  |
| AVAIL-07 | No targeting invalid | Locked / cooldown / empty do not enter targeting. |  |  |  |
| AVAIL-08 | Ready targeting | Ready skill enters targeting. |  |  |  |
| AVAIL-09 | Preview safety | Targeting preview does not call TryUseSkill. |  |  |  |

## 14. 12 Skill Regression Checklist
Use one row per execution. Add extra rows for blocked variants.

| Test ID | Role | Skill id | Preconditions | Steps | Expected result | Actual result | Status | CombatLog evidence | HP Bar evidence | Screenshot / video evidence | Notes |
|---|---|---|---|---|---|---|---|---|---|---|---|
| SK-FI-01 | Fighter | `fighter_shield_bash` | Fighter has action; target has open tile behind. | Use Shield Bash. | 3 damage + target knocked back 1 tile; tile and model move. |  |  |  |  |  |  |
| SK-FI-02 | Fighter | `fighter_shield_bash` | Behind tile blocked / invalid / occupied. | Use Shield Bash. | 3 damage; no knockback; KnockbackBlocked log. |  |  |  |  |  |  |
| SK-FI-03 | Fighter | `fighter_guard_strike` | Fighter has action; target alive. | Use Guard Strike, then deal next damage. | 5 damage + DamageAmplify +1; next damage +1 then clears. |  |  |  |  |  |  |
| SK-FI-04 | Fighter | `fighter_crushing_challenge` | Fighter has action; target alive. | Use Crushing Challenge; advance target opportunity. | 5 damage + Stun; stunned target skips move + action. |  |  |  |  |  |  |
| SK-RA-01 | Ranger | `ranger_precision_shot` | Ranger has action; target alive. | Use Precision Shot. | 5 damage. |  |  |  |  |  |  |
| SK-RA-02 | Ranger | `ranger_quick_shot` | Ranger has action; target alive. | Use Quick Shot; advance bleed ticks. | 3 damage + Bleed 2 stacks; ticks 2 then 1. |  |  |  |  |  |  |
| SK-RA-03 | Ranger | `ranger_piercing_arrow` | Enemy behind primary target in caster -> target direction. | Use Piercing Arrow. | Primary 6 damage; behind-target secondary 3 damage. |  |  |  |  |  |  |
| SK-RA-04 | Ranger | `ranger_piercing_arrow` | No valid behind target. | Use Piercing Arrow. | Primary 6 damage only. |  |  |  |  |  |  |
| SK-MA-01 | Mage | `mage_fireball` | Adjacent active alive enemy near target. | Use Fireball. | Primary 3 damage + adjacent splash 1; cooldown 1. |  |  |  |  |  |  |
| SK-MA-02 | Mage | `mage_frost_bolt` | Target within range 5. | Use Frost Bolt; inspect cooldown. | 2 damage + Stun; range 5; cooldown 2. |  |  |  |  |  |  |
| SK-MA-03 | Mage | `mage_arcane_burst` | Target within range 4; adjacent enemy near target. | Use Arcane Burst, then deal next damage to primary target. | Primary 5 + splash 2 + DamageAmplify +2; range 4. |  |  |  |  |  |  |
| SK-BA-01 | Barbarian | `barbarian_heavy_cleave` | Barbarian full HP. | Use Heavy Cleave. | 5 damage minimum. |  |  |  |  |  |  |
| SK-BA-02 | Barbarian | `barbarian_heavy_cleave` | Barbarian has lost HP. | Use Heavy Cleave. | 5 + floor(lostHP * 0.1) damage. |  |  |  |  |  |  |
| SK-BA-03 | Barbarian | `barbarian_rage_strike` | Barbarian above half HP. | Use Rage Strike. | 4 damage + Bleed 3. |  |  |  |  |  |  |
| SK-BA-04 | Barbarian | `barbarian_rage_strike` | Barbarian at/below half HP. | Use Rage Strike. | 8 damage + Bleed 6. |  |  |  |  |  |  |
| SK-BA-05 | Barbarian | `barbarian_blood_fury_slash` | Barbarian above half HP. | Use Blood Fury Slash. | 4 damage + Bleed 2. |  |  |  |  |  |  |
| SK-BA-06 | Barbarian | `barbarian_blood_fury_slash` | Barbarian at/below half HP. | Use Blood Fury Slash. | 6 damage + Bleed 2. |  |  |  |  |  |  |

## 15. Status Effect Checklist
| Test ID | Item | Expected | Actual | Status | Evidence / Notes |
|---|---|---|---|---|---|
| FX-01 | Stun preview safety | UI refresh / preview does not consume Stun. |  |  |  |
| FX-02 | Stun skip | Stun skips move + action. |  |  |  |
| FX-03 | Player stun path | Player stun has a consumption path and cannot permanently lock the run. |  |  |  |
| FX-04 | Enemy stun | Stunned enemy skips move + attack. |  |  |  |
| FX-05 | Player bleed timing | Player bleed ticks at new PlayerTurn start. |  |  |  |
| FX-06 | Enemy bleed timing | Enemy bleed ticks before that enemy acts. |  |  |  |
| FX-07 | Bleed damage | Bleed damage equals current stacks. |  |  |  |
| FX-08 | Bleed decay | Bleed stacks decrease by 1 after tick. |  |  |  |
| FX-09 | Bleed clear | Bleed clears at 0 stacks. |  |  |  |
| FX-10 | Amplify +1 | Fighter DamageAmplify +1 applies to next damage and clears. |  |  |  |
| FX-11 | Amplify +2 | Mage DamageAmplify +2 applies to next damage and clears. |  |  |  |
| FX-12 | DamageResolver order | Damage order is base -> amplify -> defend -> HP. |  |  |  |
| FX-13 | Knockback success | Legal empty tile moves tile occupancy and model. |  |  |  |
| FX-14 | Knockback blocked | Blocked / occupied / invalid destination does not move target and still deals damage. |  |  |  |

## 16. EnemyTurn Checklist
| Test ID | Item | Expected | Actual | Status | Evidence / Notes |
|---|---|---|---|---|---|
| ENEMY-01 | Transition | All players ended enters EnemyTurn. |  |  |  |
| ENEMY-02 | Active alive enemies | Active alive enemies act. |  |  |  |
| ENEMY-03 | Dead enemies | Dead enemies do not act. |  |  |  |
| ENEMY-04 | Inactive enemies | Inactive enemies do not act. |  |  |  |
| ENEMY-05 | Stunned enemy | Stunned enemy skips move + attack. |  |  |  |
| ENEMY-06 | Bleeding enemy | Bleeding enemy ticks bleed before acting. |  |  |  |
| ENEMY-07 | Enemy gate | Enemy attack is not rejected by player-only gate. |  |  |  |
| ENEMY-08 | Return | EnemyTurn completion returns to PlayerTurn. |  |  |  |

## 17. CombatLog Checklist
| Test ID | Entry | Expected | Actual | Status | Evidence / Notes |
|---|---|---|---|---|---|
| LOG-01 | Attack / Skill / Death | Structured entries appear. |  |  |  |
| LOG-02 | Defend | Defend entry appears. |  |  |  |
| LOG-03 | DamageReduced | DamageReduced entry appears. |  |  |  |
| LOG-04 | Potion / Heal | Potion and Heal entries appear. |  |  |  |
| LOG-05 | StunApplied / StunConsumed | Stun entries appear. |  |  |  |
| LOG-06 | BleedApplied / BleedTick | Bleed entries appear. |  |  |  |
| LOG-07 | DamageAmplifyApplied / DamageAmplified | Amplify entries appear. |  |  |  |
| LOG-08 | Knockback / KnockbackBlocked | Knockback entries appear. |  |  |  |
| LOG-09 | No string parsing | Evidence comes from structured entries, not parsed strings. |  |  |  |

## 18. HP Bar Checklist
| Test ID | Item | Expected | Actual | Status | Evidence / Notes |
|---|---|---|---|---|---|
| HP-01 | Damage | HP bar refreshes after damage. |  |  |  |
| HP-02 | Heal | HP bar refreshes after Potion heal. |  |  |  |
| HP-03 | Death | HP bar hides or displays death correctly. |  |  |  |
| HP-04 | Splash | Splash targets HP bars refresh. |  |  |  |
| HP-05 | Bleed | Bleed tick updates HP bar. |  |  |  |

## 19. Room / Key / Stairs / LevelUp Regression
| Test ID | Item | Expected | Actual | Status | Evidence / Notes |
|---|---|---|---|---|---|
| WORLD-01 | Room trigger | Room trigger activates expected room logic. |  |  |  |
| WORLD-02 | Room shadow | Room shadow hides when expected. |  |  |  |
| WORLD-03 | Enemy activation | Room enemies activate correctly. |  |  |  |
| WORLD-04 | Room clear | Room clear state is detected. |  |  |  |
| WORLD-05 | Key pickup | Key pickup works. |  |  |  |
| WORLD-06 | Stairs hover / second click | Stairs hover and second click work. |  |  |  |
| WORLD-07 | LevelUp | LevelUp triggers correctly. |  |  |  |
| WORLD-08 | Skill unlock refresh | LevelUp refreshes skill unlock state. |  |  |  |

## 20. Camera Controls Regression
| Test ID | Item | Expected | Actual | Status | Evidence / Notes |
|---|---|---|---|---|---|
| CAM-01 | Middle drag | Middle mouse drag pans camera. |  |  |  |
| CAM-02 | Wheel zoom | Mouse wheel zoom works. |  |  |  |
| CAM-03 | Right yaw/pitch | Right mouse yaw / pitch rotation works. |  |  |  |
| CAM-04 | UI hover block | UI hover blocks camera input where expected. |  |  |  |
| CAM-05 | Left click selection | Left click unit selection still works. |  |  |  |

## 21. ActiveUnitProvider Regression
| Test ID | Item | Expected | Actual | Status | Evidence / Notes |
|---|---|---|---|---|---|
| AUP-01 | Active alive enemies | Provider collects active alive enemies. |  |  |  |
| AUP-02 | Dead enemy preview | Dead enemies do not enter target preview. |  |  |  |
| AUP-03 | Inactive enemy preview | Inactive enemies do not enter target preview. |  |  |  |
| AUP-04 | Splash candidates | Splash candidates are correct. |  |  |  |
| AUP-05 | Secondary hit candidates | Piercing secondary hit candidates are correct. |  |  |  |

## 22. Issue Log Template
| Issue ID | Test ID | Severity | Symptom | Expected | Actual | Repro steps | Console message | Screenshot / video | Suspected system | Suggested follow-up phase | Fix attempted |
|---|---|---|---|---|---|---|---|---|---|---|---|
| ISSUE-001 |  |  |  |  |  |  |  |  |  | Phase 14.20-A Minimal Regression Fix | No |

## 23. Final Result Summary
| Field | Value |
|---|---|
| Overall status | Pass |
| Total tests | All required Phase 14.20 regression items |
| Passed | All required items passed |
| Failed | 0 reported |
| Blocked | 0 reported |
| Not tested | 0 required items reported untested |
| Console red errors | None reported |
| Fix attempted in Phase 14.20 | No |
| Can proceed to Phase 14.21? | Yes |
| Notes | User manually ran Unity Play Mode regression in `GridTest.unity` and reported all required regression items passed. |

## 24. Rollback / Follow-Up Rule
Do not roll back or fix during Phase 14.20. Record every failure. If all tests pass, proceed to Phase 14.21 Final Handover. If any test fails or blocks the run, open Phase 14.20-A Minimal Regression Fix with the smallest possible scoped repair plan.
