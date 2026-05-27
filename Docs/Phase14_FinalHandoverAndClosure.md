# Phase 14 Documentation / Stabilization Preparation Handover and Scope Correction

## 1. Purpose and scope

This document is the handover record for the Phase 14 documentation / stabilization preparation subcycle of the Bone Throne / 骸骨王座 Unity 6.3 LTS project.

Phase 14.8 was originally written with overly broad closure language. Phase 14.9 corrects that scope: Phase 14 documentation / stabilization preparation is complete, but Phase 14 functional implementation remains open.

This document does not implement fixes, write gameplay code, modify scenes, modify prefabs, modify ScriptableObject assets, modify KayKit original resources, or change `Assets`, `Packages`, or `ProjectSettings`.

## 2. Source of truth

Use these references for any next phase:

- `AGENTS.md`
- `Docs/ACTIVE_TASK.md`
- `Docs/Phase14_ProjectStateAudit.md`
- `Docs/骸骨王座_系统设计文档_Unity6.3LTS_v2.2_CurrentProjectState.md`
- `Docs/骸骨王座_Codex完整即用Vibecoding开发文档_Unity6.3LTS_v1.2_CurrentProjectState.md`
- `Docs/Phase14_StabilizationPlan.md`
- `Docs/Phase14_InspectorBindingChecklist.md`
- `Docs/Phase14_RegressionChecklist.md`
- `Docs/Phase14_MinimalStabilizationImplementationProposal.md`
- `Docs/Phase14_PreFeatureRegressionAudit.md`
- `Docs/DevLogs/`

## 3. Phase 14 corrected conclusion

- Phase 14 documentation/stabilization preparation completed.
- Phase 14 functional backlog remains open.
- Phase 14 is not fully complete yet.
- The project must continue with Phase 14 functional implementation before Phase 15.
- Do not start Phase 15 yet.
- Phase 14.7 regression audit recorded all required regression tests as passed.
- There is currently no observed failure evidence supporting immediate stabilization code fixes.
- Phase 14.6 candidates remain future candidates only.
- The next step should remain inside Phase 14 until the functional backlog below is completed and revalidated.

## 4. Current project state

- Engine target: Unity 6.3 LTS.
- `Assets/_BoneThrone/Scenes/GridTest.unity` is the current only real integrated validation scene.
- `Assets/_BoneThrone/Scenes/MainMenu.unity` remains a placeholder.
- The current single-player test loop passed the Phase 14.7 regression audit.
- Boss, LAN, formal three-level scenes, Victory, and Defeat remain future / deferred.
- `SkillData` assets exist.
- `CharacterData`, `EnemyData`, `LevelData`, and `RoomData` are not fully assetized.
- The four player roles currently only implement Slot 0 representative skills.
- `ActionCommand`, `IGameSession`, and `GameStateSnapshot` remain architecture skeletons. They do not yet drive the actual gameplay flow.

## 5. Remaining Phase 14 functional backlog

These items were not completed by the Phase 14.1-14.8 documentation / stabilization preparation subcycle. They remain required before a real Phase 14 final closure:

1. GridTest camera controls:
   - Middle mouse drag.
   - Mouse wheel zoom.
2. Active enemy provider / scene and UI auto enemy collection:
   - Reduce manual `enemyUnits` / `knownUnits` dependency.
   - Do not break UI safety rules.
   - Do not call `TryBasicAttack` or `TryUseSkill` for highlight.
3. Skill SO cleanup:
   - Verify Slot 0 `SkillData` assets.
   - Do not change formulas unless separately approved.
4. Final regression after functional changes.
5. Real Phase 14 final handover after those functional items pass.

## 6. Phase 14.1-14.8 preparation summary

### Phase 14.1 - Current Project State Audit

Audited the real repository state and confirmed the project is not a from-zero plan anymore. The current project is a single-player integrated test flow centered on `GridTest.unity`, with implemented Grid, Unit, Movement, Turn, Combat, Enemy AI, Room / Key / Stairs / Level progression, Skill, UI feedback, and prefab wrapping foundations.

### Phase 14.2 - System Design Document Update

Created the current-state system design document v2.2. It separates current implemented state, test-only / placeholder state, deferred future state, Phase 14 safety rules, and do-not-touch rules.

### Phase 14.3 - Vibecoding Development Document Update

Created the current-state Vibecoding development document v1.2. It changes future Codex guidance from "from-zero development" to "continue from current real project state".

### Phase 14.4 - Stabilization Plan

Created a stabilization plan covering Inspector binding risks, UI action targeting, Fireball splash, structured CombatLog, Enemy Floating HP Bar, Enemy AI / turn gate, Room activation, Key / Stairs / LevelUp, and `GridTest.unity` regression.

### Phase 14.5 - Inspector Binding and Regression Checklist

Created Inspector binding and Play Mode regression checklists. The checklists define what to verify before any future implementation or feature work.

### Phase 14.6 - Minimal Stabilization Implementation Proposal Only

Created a proposal-only document for future stabilization candidates. No candidate was implemented in Phase 14.6.

### Phase 14.7 - Pre-Feature Regression Audit

Recorded the user-reported `GridTest.unity` regression audit result: all required tests passed, no blocking Console issue was reported, and no stabilization candidate is currently supported by observed failure evidence.

### Phase 14.8 - Final Handover and Closure

Created a handover document, but its original wording incorrectly implied that all of Phase 14 was complete. Phase 14.9 supersedes that interpretation and reopens the functional backlog listed above.

### Phase 14.9 - Scope Correction and Functional Backlog Reopen

Corrects the Phase 14.8 closure scope. The documentation / stabilization preparation subcycle is complete, but Phase 14 functional work remains open and must continue before Phase 15.

## 7. Documents created / updated in Phase 14

Phase 14 documentation outputs:

- `Docs/Phase14_ProjectStateAudit.md`
- `Docs/骸骨王座_系统设计文档_Unity6.3LTS_v2.2_CurrentProjectState.md`
- `Docs/骸骨王座_Codex完整即用Vibecoding开发文档_Unity6.3LTS_v1.2_CurrentProjectState.md`
- `Docs/Phase14_StabilizationPlan.md`
- `Docs/Phase14_InspectorBindingChecklist.md`
- `Docs/Phase14_RegressionChecklist.md`
- `Docs/Phase14_MinimalStabilizationImplementationProposal.md`
- `Docs/Phase14_PreFeatureRegressionAudit.md`
- `Docs/Phase14_FinalHandoverAndClosure.md`

Phase 14 DevLogs:

- `Docs/DevLogs/Phase14.2_SystemDesignDocumentUpdate.md`
- `Docs/DevLogs/Phase14.3_VibecodingDocumentUpdate.md`
- `Docs/DevLogs/Phase14.4_StabilizationPlan.md`
- `Docs/DevLogs/Phase14.5_InspectorRegressionChecklist.md`
- `Docs/DevLogs/Phase14.6_MinimalStabilizationProposal.md`
- `Docs/DevLogs/Phase14.7_PreFeatureRegressionAudit.md`
- `Docs/DevLogs/Phase14.8_FinalHandoverAndClosure.md`
- `Docs/DevLogs/Phase14.9_ScopeCorrection.md`

Phase 14 also updated `Docs/ACTIVE_TASK.md` across the documentation phases.

## 8. Phase 14.7 regression audit result

Test scene:

- `Assets/_BoneThrone/Scenes/GridTest.unity`

Regression results:

1. Select player unit: Pass
2. Move mode: Pass
3. Basic Attack mode: Pass
4. Skill Slot 0 mode: Pass
5. Mage Fireball splash: Pass
6. CombatLog structured entries: Pass
7. Enemy Floating HP Bar refresh: Pass
8. Enemy HP Bar death hide: Pass
9. Room trigger: Pass
10. Room shadow hide: Pass
11. Enemy activation: Pass
12. Room clear: Pass
13. Key pickup: Pass
14. Stairs hover / second click: Pass
15. LevelUp: Pass
16. Enemy AI turn: Pass

Additional audit result:

- Inspector binding audit: Pass.
- Console: no blocking errors reported by the user.
- Fix attempted: No.
- Supported by observed evidence: None.

## 9. Current safe baseline

The current safe baseline is:

- `GridTest.unity` single-player integrated gameplay test flow.
- Player selection, movement, Basic Attack, Skill Slot 0, Mage Fireball splash, structured CombatLog, Enemy Floating HP Bar, Room trigger / shadow / activation / clear, Key pickup, Stairs placeholder progression, LevelUp, and Enemy AI turn all reported as passing.
- Existing UI action flow remains `UI intent -> UIActionModeController -> gameplay service`.
- Existing CombatLog remains structured event output.
- Existing Fireball splash uses structured `SkillEffectResult`.
- Existing ordinary enemy set keeps `Skeleton_Rogue` as the normal enemy.
- Existing Player Ranger visual remains Adventurers Rogue.

## 10. Why immediate stabilization code fixes are not needed

Phase 14.6 candidates require observed failure evidence before they move toward implementation.

Phase 14.7 did not record any `Fail` or `Blocked` regression result. All required tests were reported as `Pass`.

Therefore:

- Do not perform preventive stabilization code changes now.
- Do not modify `DamageResolver`.
- Do not modify `SkillEffectExecutor` skill formulas.
- Do not modify UI targeting merely to replace manual arrays without evidence.
- Do not modify Enemy AI turn gates without an observed EnemyTurn failure.
- Do not modify Enemy Floating HP Bar without an observed HP bar failure.
- Do not modify Room runtime without an observed Room activation / clear failure.

If a future regression failure appears, return to the relevant Phase 14.6 candidate and open a separate proposal / implementation split with explicit user approval.

## 11. Deferred Phase 14.6 future candidates

These remain future candidates only. Each is not currently supported by observed failure evidence after the Phase 14.7 pass.

- Candidate A: Inspector binding validation helper. Status: Not currently supported by observed failure evidence after Phase 14.7 pass.
- Candidate B: Enemy AI turn gate issue. Status: Not currently supported by observed failure evidence after Phase 14.7 pass.
- Candidate C: Skill cooldown tick flow. Status: Not currently supported by observed failure evidence after Phase 14.7 pass.
- Candidate D: Fireball splash `knownUnits` dependency. Status: Not currently supported by observed failure evidence after Phase 14.7 pass.
- Candidate E: UI target list / `enemyUnits` dependency. Status: Not currently supported by observed failure evidence after Phase 14.7 pass.
- Candidate F: Enemy Floating HP Bar robustness. Status: Not currently supported by observed failure evidence after Phase 14.7 pass.
- Candidate G: Room activation validation. Status: Not currently supported by observed failure evidence after Phase 14.7 pass.
- Candidate H: Key / Stairs / LevelUp placeholder boundary. Status: Not currently supported by observed failure evidence after Phase 14.7 pass.

## 12. Current scene / prefab / SO / data state

- `GridTest.unity` is the current only real integrated validation scene.
- `MainMenu.unity` remains a placeholder.
- Formal `Level_01`, `Level_02`, and `Level_03` scenes are not complete.
- Current player and ordinary enemy gameplay prefabs exist and wrap KayKit visuals through project-specific prefabs.
- Current committed gameplay ScriptableObject data assets are `SkillData`.
- Character / enemy stats are still mainly represented through prefab / scene serialized values such as `UnitStats`.
- Manual Inspector bindings remain part of the current project shape, but Phase 14.7 did not report binding failures.
- Do not describe current content as formal three-floor completion.

## 13. Current UI / combat / skill / room / level state

Current UI state:

- UI action flow is `UI intent -> UIActionModeController -> gameplay service`.
- UI does not directly subtract HP.
- UI does not directly change cooldown.
- UI does not directly call `MarkActed`.
- UI does not directly call `MarkMoved`.
- Highlight preview must not call `TryBasicAttack` or `TryUseSkill`.

Current combat state:

- D20 basic combat exists.
- `DamageResolver` applies damage.
- `CombatLog` is structured event output and must not be rebuilt by parsing strings.

Current skill state:

- Four Slot 0 representative skills exist.
- Mage Fireball splash uses structured `SkillEffectResult`.
- Skill Slot 1 and Slot 2 remain deferred.

Current room / level state:

- Room trigger, room shadow, enemy activation, room clear, Key pickup, Stairs interaction, and LevelUp placeholder progression exist and passed Phase 14.7 regression.
- Stairs / Level progression remains placeholder progression, not formal scene loading.

## 14. Deferred future features

Deferred future work:

- Boss.
- LAN lobby / networked gameplay.
- Formal `Level_01`, `Level_02`, and `Level_03` scenes.
- Victory / Defeat UI.
- Full `CharacterData`, `EnemyData`, `LevelData`, and `RoomData` assetization.
- Skill Slot 1 / Slot 2.
- Defend / Potion.
- Formal stairs modal / real scene loading.

## 15. Do-not-touch rules

Do not touch these unless a future phase explicitly approves the exact scope:

- Do not modify KayKit original resources.
- `Skeleton_Rogue` is the ordinary enemy. Do not rename it.
- `Skeleton_Golem` is reserved for future Boss / heavy Boss only. Do not use it as a normal enemy.
- Player Ranger uses Adventurers Rogue visual. Do not change it back to Ranger visual.
- UI must not directly subtract HP.
- UI must not directly change cooldown.
- UI must not directly call `MarkActed`.
- UI must not directly call `MarkMoved`.
- Highlight preview must not call `TryBasicAttack`.
- Highlight preview must not call `TryUseSkill`.
- `CombatLog` is structured event output. Do not implement it by parsing strings.
- Do not casually modify `DamageResolver`.
- Do not casually modify `SkillEffectExecutor` skill formulas.
- `GridTest.unity` is the current only real integrated validation scene. Scene changes must be separately approved.

## 16. Recommended next Phase 14 continuation

Do not start Phase 15 yet. The recommended continuation stays inside Phase 14:

1. Phase 14.10 - GridTest Camera Controls.
2. Phase 14.11 - Active Enemy Provider / Scene and UI Auto Enemy Collection.
3. Phase 14.12 - Skill SO Cleanup.
4. Phase 14.13 - Final Regression After Functional Changes.
5. Phase 14.14 - Real Phase 14 Final Handover and Closure.

Phase 15 planning can be considered only after these Phase 14 functional items are completed, revalidated, and closed by a real final handover.

## 17. New conversation handover prompt

Copy this into a new conversation when continuing the project:

```text
You are continuing the Unity 6.3 LTS project Bone Throne / 骸骨王座.

Phase 14 documentation/stabilization preparation is complete, but Phase 14 functional backlog remains open.
Phase 14 is not fully complete yet.
Do not start Phase 15 yet.

Before planning, read:
1. AGENTS.md
2. Docs/ACTIVE_TASK.md
3. Docs/Phase14_FinalHandoverAndClosure.md
4. Docs/骸骨王座_系统设计文档_Unity6.3LTS_v2.2_CurrentProjectState.md
5. Docs/骸骨王座_Codex完整即用Vibecoding开发文档_Unity6.3LTS_v1.2_CurrentProjectState.md
6. Docs/Phase14_PreFeatureRegressionAudit.md
7. Docs/DevLogs/

Phase 14.7 regression audit reported all required GridTest.unity tests as passed.

Do not regenerate existing Phase 0-13 systems.
Do not treat Phase 14.6 candidates as current fix tasks.
Continue with Phase 14 functional implementation before Phase 15.

Remaining Phase 14 functional backlog:
1. GridTest camera controls: middle mouse drag and mouse wheel zoom.
2. Active enemy provider / scene and UI auto enemy collection, reducing manual enemyUnits / knownUnits dependency without breaking UI safety rules.
3. Skill SO cleanup: verify Slot 0 SkillData assets without formula changes unless separately approved.
4. Final regression after functional changes.
5. Real Phase 14 final handover after those functional items pass.

Preserve these rules:
- GridTest.unity is the current only real integrated validation scene.
- MainMenu.unity remains a placeholder.
- Boss, LAN, formal three-level scenes, Victory, and Defeat remain future / deferred.
- Skeleton_Rogue is the ordinary enemy.
- Skeleton_Golem is reserved for future Boss / heavy Boss only.
- Player Ranger uses Adventurers Rogue visual.
- Do not modify KayKit original resources.
- UI must not directly mutate HP, cooldown, acted, or moved state.
- Highlight preview must not call TryBasicAttack or TryUseSkill.
- CombatLog is structured event output and must not be implemented by parsing strings.
```

## 18. Git / branch checks

Recommended checks:

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

Suggested branch handling:

1. Review `git diff -- Docs`.
2. Commit Phase 14.9 scope correction documentation when reviewed.
3. Continue Phase 14 functional backlog on the appropriate branch.
4. Do not close or merge the branch as full Phase 14 completion until the functional backlog passes final regression.

## 19. Rollback

To roll back the Phase 14.9 scope correction only:

- Restore the previous text in `Docs/Phase14_FinalHandoverAndClosure.md`.
- Restore the previous text in `Docs/DevLogs/Phase14.8_FinalHandoverAndClosure.md`.
- Restore the previous Phase 14.9 edits in `Docs/ACTIVE_TASK.md`, if needed.
- Delete `Docs/DevLogs/Phase14.9_ScopeCorrection.md`.

Do not use broad reset commands if other Phase 14 documentation work is still in progress.
