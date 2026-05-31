# Phase 14 Final Handover and Closure

## A. Document Purpose
This is the true final handover document for Phase 14 of 《Bone Throne / 骸骨王座》.

This document supersedes the earlier Phase 14.8 handover, which represented documentation and stabilization preparation only. Phase 14.21 records the actual functional closure after combat, UI, turn flow, skill, Defend, Potion, status effect, and full regression work.

## B. Final Phase Status
- Phase 14 completed.
- Phase 14.20 Full Combat Regression passed.
- User reported all required Unity Play Mode regression items passed.
- Current test scene: `Assets/_BoneThrone/Scenes/GridTest.unity`.
- Current safe baseline is after the Phase 14.20 regression pass.
- Phase 14.15-14.19 gameplay work is completed and validated.
- No immediate Phase 14.20-A bugfix is needed.
- Project is ready to move beyond Phase 14.

## C. Completed Phase 14 Timeline
| Phase | Summary |
|---|---|
| Phase 14.1 Project State Audit | Audited current project state and gaps. |
| Phase 14.2 System Design Current State Update | Updated system design documentation for current state. |
| Phase 14.3 Vibecoding Document Update | Updated the Codex/vibecoding development guide. |
| Phase 14.4 Stabilization Plan | Planned stabilization work before additional gameplay expansion. |
| Phase 14.5 Inspector / Regression Checklist | Added inspector and regression checklist coverage. |
| Phase 14.6 Minimal Stabilization Proposal | Proposed minimal stabilization work. |
| Phase 14.7 Pre-Feature Regression Audit | Audited regression state before new features. |
| Phase 14.8 Documentation Closure | Closed documentation/stabilization preparation, not final Phase 14 gameplay closure. |
| Phase 14.9 Scope Correction | Reopened scope for remaining functional combat work. |
| Phase 14.10 Camera Controls | Added middle drag and wheel zoom camera controls. |
| Phase 14.10-B Right Mouse Camera Rotation | Added right mouse yaw / pitch camera rotation. |
| Phase 14.11 ActiveUnitProvider | Added active unit/enemy provider support for targeting and enemy collection. |
| Phase 14.12 Skill SO Cleanup | Audited and cleaned skill ScriptableObject state. |
| Phase 14.13-A Skill Assets and Prefab Slots | Added formal skill assets and Player prefab slot wiring. |
| Phase 14.13-B UI Skill Slot Wiring | Wired UI skill slots 0/1/2. |
| Phase 14.13-C Skill Effect Branches | Added skill effect executor branches. |
| Phase 14.13-D Formal SkillData Migration | Migrated to formal SkillData assets. |
| Phase 14.13-E Combat Action Economy Audit | Audited movement/action economy. |
| Phase 14.14 Turn System Completion Plan | Planned turn lifecycle completion. |
| Phase 14.15 Free-order Turn System | Completed local turn system foundation. |
| Phase 14.15-B End Turn Button UI | Added End Turn UI entry point. |
| Phase 14.15-C Free Player Turn Order | Reworked single-player PlayerTurn to free unit order. |
| Phase 14.15-D Player Foot Tile Indicator | Added white foot tile marker for alive player units. |
| Phase 14.16 Defend / Potion / Skill Availability Plan | Planned Defend, Potion, and skill availability rules. |
| Phase 14.17 Defend / Potion / Skill Availability Implementation | Implemented Defend, Potion, and locked/cooldown skill button behavior. |
| Phase 14.18 Skill Rebuild Design | Designed current-architecture 12 skill rebuild. |
| Phase 14.19 Skill Rebuild Implementation | Implemented 12 rebuilt formal skill effects. |
| Phase 14.19-A Skill Balance and Bugfix | Applied final skill values, bleed stack rules, stun skip rules, and knockback model movement fix. |
| Phase 14.19-B Ended Player Foot Tile Grey Indicator | Added grey foot tile marker for ended alive players. |
| Phase 14.19-E Restore SkillBar UI from Good Baseline | Restored SkillBar UI from the last known good baseline. |
| Phase 14.20 Full Combat Regression | Full manual Unity Play Mode regression passed. |
| Phase 14.21 Final Handover and Closure | Final documentation-only closure. |

Phase 14.19-C and Phase 14.19-D were UI layout hotfix attempts. The final retained UI direction is Phase 14.19-E: restore the SkillBar UI from the last known good baseline.

## D. Current Gameplay State
- Single-player PlayerTurn uses free selection of any alive player unit that has not ended.
- End Turn ends the currently selected player unit.
- Ended player units cannot be selected again in the same player round.
- EnemyTurn starts after all alive player units have ended.
- EnemyTurn completes and returns to a new PlayerTurn.
- New PlayerTurn resets `moved`, `acted`, and `ended`.
- Alive not-ended player foot tiles are white.
- Alive ended player foot tiles are grey.
- SkillBar UI is restored to the good baseline from before the Phase 14.19-B/C/D layout attempts.
- Empty, Locked, and Cooldown skill slots are grey and not clickable.
- Ready skills are clickable.
- Defend and Potion consume action and do not automatically End Turn.
- Defend applies damage reduction through `DamageResolver`.
- Potion is self-use healing and decrements potion count.

## E. Final 12 Skill List
- `fighter_shield_bash`: 3 damage + knockback 1.
- `fighter_guard_strike`: 5 damage + DamageAmplify +1.
- `fighter_crushing_challenge`: 5 damage + Stun.
- `ranger_precision_shot`: 5 damage.
- `ranger_quick_shot`: 3 damage + Bleed 2 stacks.
- `ranger_piercing_arrow`: 6 primary damage + 3 secondary damage.
- `mage_fireball`: 3 primary damage + 1 splash damage, cooldown 1.
- `mage_frost_bolt`: 2 damage + Stun, range 5, cooldown 2.
- `mage_arcane_burst`: range 4, 5 primary damage + 2 splash damage + DamageAmplify +2.
- `barbarian_heavy_cleave`: 5 + floor(lostHP * 0.1), minimum 5.
- `barbarian_rage_strike`: 4 damage + Bleed 3; at or below half HP becomes 8 damage + Bleed 6.
- `barbarian_blood_fury_slash`: 4 damage, at or below half HP 6 damage, Bleed 2.

## F. Final Status Effect Rules
- Stun skips move + action once.
- Stun is not consumed by UI refresh or targeting preview.
- Bleed uses stacks; tick damage equals current stacks, then stacks decrease by 1.
- Player bleed ticks at new PlayerTurn start.
- Enemy bleed ticks before that enemy acts.
- DamageAmplify applies once through `DamageResolver`, then clears.
- `DamageResolver` order is: base damage -> amplify -> defend -> HP.
- Knockback uses safe movement / tile placement so model position, tile occupancy, and unit tile reference move together.

## G. Final Validation Result
- Phase 14.20 regression: Pass.
- User reported all required regression items passed.
- Console red errors: None reported.
- No Phase 14.20-A Minimal Regression Fix is required.

## H. Do-Not-Touch Rules Still Active
- Do not modify KayKit original resources.
- Do not treat `Skeleton_Golem` as a normal enemy.
- Do not rename or repurpose `Skeleton_Rogue` into `Skeleton_Golem`.
- Ranger gameplay identity remains Ranger, even though the visual resource uses Rogue visual.
- Do not parse CombatLog strings for gameplay behavior.
- UI must not directly change HP, cooldowns, `MarkMoved`, or `MarkActed`.
- Highlighting and preview must not call `TryBasicAttack` or `TryUseSkill`.
- Do not directly change scenes, prefabs, or SkillData unless a future phase explicitly approves it.

## I. Remaining Deferred / Future Systems
- Formal `Level_01`, `Level_02`, and `Level_03` scenes.
- Boss encounter and `Skeleton_Golem` heavy boss behavior.
- Victory / Defeat screens.
- MainMenu mode selection.
- LAN lobby and Netcode for GameObjects gameplay sync.
- Formal `CharacterData`, `EnemyData`, `LevelData`, and `RoomData` assetization.
- Final VFX, SFX, and animation polish.
- Final UI art replacement.
- Full level progression beyond the `GridTest` placeholder.

## J. Recommended Next Phase Options
- Phase 15A - Formal Level Scene Plan.
- Phase 15B - Boss Encounter Plan.
- Phase 15C - Victory / Defeat / Retry Flow.
- Phase 15D - Formal Data Assetization.
- Phase 15E - MainMenu / Mode Flow.
- Phase 15F - LAN Architecture Revisit.

Recommended next step: start with Phase 15A - Formal Level Scene Plan, or Phase 15B - Boss Encounter Plan if the priority is a more complete combat demo loop.

## K. New Conversation Handover Prompt
Copy this prompt into a new conversation to continue cleanly:

```text
Continue Unity 6.3 LTS project 《Bone Throne / 骸骨王座》 at D:\Project\Unity\Bone Throne.

Phase 14 is functionally complete and closed. Phase 14.20 Full Combat Regression passed in Unity Play Mode on Assets/_BoneThrone/Scenes/GridTest.unity. Do not reopen Phase 14 unless a specific regression is reported.

Current baseline:
- Single-player PlayerTurn is free-order; players can select any alive not-ended player.
- End Turn ends selected player only.
- All alive players ended -> EnemyTurn -> new PlayerTurn.
- New PlayerTurn resets moved / acted / ended.
- Alive not-ended player foot tile is white; ended alive player foot tile is grey.
- SkillBar UI restored to good baseline.
- Skill Empty / Locked / Cooldown grey and not clickable; Ready clickable.
- Defend and Potion consume action and do not auto End Turn.
- Defend uses DamageResolver reduction.
- Potion heals self and decrements potion count.
- 12 formal skills rebuilt and validated.

Do not modify scene, prefab, SkillData, KayKit, or gameplay code unless the new phase explicitly allows it.

Recommended next phase: Phase 15A - Formal Level Scene Plan, or Phase 15B - Boss Encounter Plan.
```

## L. Git Closing Checks
Run before committing or handing off:

```powershell
git status --short --untracked-files=all
git diff --name-only
git diff --check
```

## M. Rollback Note
Phase 14.21 is documentation-only. To roll back this phase, restore:
- `Docs/Phase14_FullCombatRegression.md`
- `Docs/Phase14_FinalHandoverAndClosure.md`
- `Docs/DevLogs/Phase14.21_FinalHandoverAndClosure.md`
- `Docs/ACTIVE_TASK.md`

No gameplay rollback is required because no code, asset, prefab, or scene was changed in this phase.
