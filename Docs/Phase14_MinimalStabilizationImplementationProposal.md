# Phase 14.6 Minimal Stabilization Implementation Proposal

## 1. Purpose and scope

This document proposes minimal future stabilization implementation candidates for the current Bone Throne Unity 6.3 LTS project.

Phase 14.6 is proposal-only. It does not implement fixes, write gameplay code, modify scenes, modify prefabs, modify ScriptableObject assets, modify KayKit original resources, or change `Assets`, `Packages`, or `ProjectSettings`.

Every candidate in this document is a future implementation candidate only. Each one requires:

- A later separate phase.
- Explicit user approval.
- The required Play Mode reproduction or Inspector check before implementation.

## 2. Source of truth

Use these references:

- `AGENTS.md`
- `Docs/Phase14_ProjectStateAudit.md`
- `Docs/Phase14_StabilizationPlan.md`
- `Docs/Phase14_InspectorBindingChecklist.md`
- `Docs/Phase14_RegressionChecklist.md`
- `Docs/骸骨王座_系统设计文档_Unity6.3LTS_v2.2_CurrentProjectState.md`
- `Docs/骸骨王座_Codex完整即用Vibecoding开发文档_Unity6.3LTS_v1.2_CurrentProjectState.md`
- `Docs/DevLogs/`

If any older Markdown displays encoding corruption in the current terminal, do not copy corrupted text. Reconstruct the fact from the Phase 14.1 audit result, v2.2 current-state design document, v1.2 Vibecoding document, Phase 14.4 stabilization plan, Phase 14.5 checklists, and DevLogs.

## 3. Candidate summary table

| Candidate ID | Name | Problem | Value | Risk | Requires Play Mode reproduction | Future allowed files | Future forbidden files | Recommendation |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| A | Inspector binding validation helper | Manual Inspector arrays can be missing or stale. | Earlier detection of binding errors. | A runtime/editor validator can require scene/prefab attachment if designed poorly. | Inspector check first; Play Mode only if runtime validation is proposed. | Future validation/test script only; Docs. | Scene/prefab edits unless separately approved. | High priority future candidate as a proposal; prefer no scene attachment. |
| B | Enemy AI turn gate issue | Enemy AI may be rejected by `TurnManager` / `ActionPermissionService` player-only gates. | Stabilizes EnemyTurn reliability. | Mistakes can break player action rules and future LAN order. | Yes, must reproduce in `GridTest.unity`. | Future AI/Turn files only after approval. | `DamageResolver`, combat formulas, scene/prefab assets. | High priority reproduction candidate; no fix until proven. |
| C | Skill cooldown tick flow | Cooldown tick is not fully connected to turn-end flow. | Improves turn-based skill rhythm. | Can affect `SkillRuntime`, `SkillSystem`, UI display, and turn lifecycle. | Yes, verify whether current gameplay is blocked. | Future Turn/Skill files only after approval. | Skill formulas, `DamageResolver`, `CombatLog` model. | Medium priority; likely defer until a clearer skill phase. |
| D | Fireball splash `knownUnits` dependency | Fireball splash relies on `SkillEffectExecutor.knownUnits`. | Reduces splash target misses. | Automatic collection can change skill reach and touches sensitive skill executor boundaries. | Yes, reproduce missing splash first. | Prefer Docs/checklist; future target-source boundary only if approved. | Skill formulas, `DamageResolver`, broad `SkillEffectExecutor` rewrite. | Defer automatic collection; checklist first. |
| E | UI target list / `enemyUnits` dependency | Red/yellow highlights depend on manually assigned enemy arrays. | More reliable target preview. | Active enemy provider can affect all UI targeting. | Yes, reproduce missing highlights first. | Future UI targeting/provider files only after approval. | Execution semantics of `TryBasicAttack` / `TryUseSkill`. | High/medium future candidate after reproduction. |
| F | Enemy Floating HP Bar robustness | Unit reference, HP refresh, death hide, raycast, and overrides can regress. | More reliable feedback without changing gameplay. | Prefab/scene override risk if not limited to script defense. | Yes, reproduce specific failure first. | Future HP bar UI script only after approval. | Enemy prefabs or HP bar prefab unless explicitly approved. | Medium priority future candidate. |
| G | Room activation validation | `assignedEnemies` / `spawnTiles` can be missing or mismatched. | Faster diagnosis of room activation issues. | Validator can require scene component attachment if designed poorly. | Play Mode recommended for room trigger/activation/clear. | Future validation/test script only after approval. | Room runtime rewrite, scene edits unless approved. | Medium priority as read-only validation proposal. |
| H | Key / Stairs / LevelUp placeholder boundary | Placeholder progression can be confused with formal three-floor loading. | Reduces design drift and false expectations. | Scene loading implementation would exceed stabilization scope. | Play Mode only to verify current placeholder behavior. | Prefer Docs; future prompt/presentation hints only if approved. | Formal Level_01/02/03 scene loading in this stabilization track. | Defer implementation; keep boundary documented. |

## 4. Candidate A: Inspector binding validation helper

Problem:

- Current gameplay depends on manual Inspector bindings such as `knownUnits`, `enemyUnits`, `playerUnits`, `assignedEnemies`, `spawnTiles`, `requiredClearedRooms`, and `initialTiles`.

Options:

- Continue with documentation + manual Inspector checklist only.
- Add a future read-only validator that reports missing/null/stale bindings.
- Add editor-only validation later, if a separate phase approves editor tooling.
- Add runtime validation later, if a separate phase approves a runtime diagnostic component.

Risk assessment:

- A validator is useful only if it stays read-only.
- If it requires scene or prefab attachment, it introduces scene/prefab modification risk.
- If it auto-fixes references, it violates the current safety direction and should be rejected.

Future recommendation:

- High priority future candidate.
- Prefer a no-scene-attachment approach first, such as a ContextMenu/test helper or editor utility proposal.
- Any implementation requires a later separate phase, explicit user approval, and an Inspector check based on Phase 14.5.

## 5. Candidate B: Enemy AI turn gate issue

Problem:

- Enemy AI calls the same `CombatSystem` and movement services as player-facing gameplay.
- Enemy action can be rejected if `TurnManager` / `ActionPermissionService` is configured as a player-only gate.

Risk assessment:

- This must not be fixed from theory.
- Mis-editing turn gates can break player movement, player attacks, action consumption, and future LAN turn order.
- Future LAN direction preserves Fighter -> Ranger -> Mage -> Barbarian -> Enemy, so turn-gate changes need extra care.

Required reproduction:

- In `GridTest.unity`, enter EnemyTurn.
- Confirm whether enemies select targets, move, and attack.
- Capture Console logs if actions are rejected.

Future recommendation:

- High priority reproduction candidate.
- Future implementation should be split into proposal and implementation.
- Any implementation requires a later separate phase, explicit user approval, and a successful Play Mode reproduction of the rejection.

## 6. Candidate C: Skill cooldown tick flow

Problem:

- Current cooldown tick is not fully connected to a complete turn-end flow.
- Skill Slot 0 exists for the four roles, but later skill-slot expansion depends on reliable cooldown timing.

Risk assessment:

- Cooldown changes can affect `SkillRuntime`, `SkillSystem`, HUD state, and turn-end semantics.
- Current project only has Slot 0 representative skills, so this may not block immediate stabilization.

Required reproduction:

- In `GridTest.unity`, use Skill Slot 0.
- Observe cooldown state across player/enemy phase transitions using the current test flow.
- Record whether cooldown behavior blocks normal regression tests.

Future recommendation:

- Medium priority future candidate.
- Likely defer until a clearer skill/turn lifecycle phase.
- Any implementation requires a later separate phase, explicit user approval, and Play Mode evidence that current behavior is a real blocker.

## 7. Candidate D: Fireball splash knownUnits dependency

Problem:

- Mage Fireball splash currently depends on `SkillEffectExecutor.knownUnits`.
- Missing or stale `knownUnits` can make the primary hit work while splash targets are skipped.

Options:

- Keep manual binding and enforce the Phase 14.5 checklist.
- Add a future read-only warning when `knownUnits` is empty or incomplete.
- Defer automatic unit collection until a broader targeting/data phase.

Risk assessment:

- Automatic collection can change the set of affected targets.
- `SkillEffectExecutor` is formula-sensitive.
- Skill formulas must not be changed casually.

Required reproduction:

- In `GridTest.unity`, set up or use an existing Fireball test where an enemy is adjacent to the primary target.
- Confirm whether splash damage and structured `SkillEffectResult` CombatLog rows appear.

Future recommendation:

- Checklist first.
- Defer automatic collection.
- Any implementation requires a later separate phase, explicit user approval, and Play Mode reproduction of missing splash behavior.

## 8. Candidate E: UI target list / enemyUnits dependency

Problem:

- Basic Attack red highlight and Skill Slot 0 yellow highlight depend on `enemyUnits`.
- Missing `enemyUnits` entries can make preview wrong even when gameplay services are correct.

Options:

- Keep manual binding and enforce Phase 14.5 checklist.
- Add a future read-only active enemy provider for UI targeting.
- Add future warnings if `enemyUnits` is empty while enemies exist.

Risk assessment:

- UI targeting must remain an intent/preview layer.
- Highlight must not call `TryBasicAttack`.
- Highlight must not call `TryUseSkill`.
- Real execution must still go through `CombatSystem.TryBasicAttack` and `SkillSystem.TryUseSkill` only after player target selection.

Required reproduction:

- In `GridTest.unity`, enter Basic Attack mode and Skill Slot 0 mode.
- Confirm red/yellow highlights include all valid ordinary enemies.

Future recommendation:

- High/medium future candidate.
- More attractive than changing skill formulas because it can be isolated to UI targeting if approved.
- Any implementation requires a later separate phase, explicit user approval, and Play Mode reproduction of target preview failures.

## 9. Candidate F: Enemy Floating HP Bar robustness

Problem:

- Enemy Floating HP Bar can fail through stale Unit reference, HP refresh, death hiding, raycast settings, or prefab/scene overrides.

Risk assessment:

- Script-only defensive checks may be low-risk.
- Prefab or scene override changes are higher-risk and should not be default.
- HP bar must read gameplay state only; it must never modify HP or death state.

Required reproduction:

- In `GridTest.unity`, damage each ordinary enemy type.
- Confirm fill refresh.
- Kill an enemy and confirm death hide.
- Confirm enemy targeting is not blocked by HP bar graphics.

Future recommendation:

- Medium priority future candidate.
- Prefer script-only defensive diagnostics if a failure is reproduced.
- Any implementation requires a later separate phase, explicit user approval, and Play Mode reproduction of the exact HP bar failure.

## 10. Candidate G: Room activation assignedEnemies / spawnTiles validation

Problem:

- Room activation depends on manually assigned `assignedEnemies` and `spawnTiles`.
- Count/order mismatch can make room entry, placement, and room clear fail.

Options:

- Continue with Phase 14.5 checklist.
- Add a future read-only validation helper to report nulls, count mismatch, invalid spawn tiles, or stale enemies.

Risk assessment:

- A helper is useful if it remains read-only.
- Do not default to changing Room runtime behavior.
- Do not default to scene component attachment.

Required reproduction:

- In `GridTest.unity`, trigger room entry.
- Confirm assigned enemies activate, place on spawn tiles, and room clear works after enemies die.

Future recommendation:

- Medium priority future candidate.
- Prefer read-only validation proposal before any Room runtime changes.
- Any implementation requires a later separate phase, explicit user approval, and Play Mode / Inspector evidence of room binding failures.

## 11. Candidate H: Key / Stairs / LevelUp placeholder boundary

Problem:

- Current Key / Stairs / LevelUp flow is placeholder progression.
- It is not true three-floor scene loading.
- Future work can accidentally treat the placeholder as formal Level_01 / Level_02 / Level_03 content.

Options:

- Keep documentation boundary only.
- Add future clearer prompts or debug text if users confuse placeholder progression.
- Defer formal scene loading to a future level/content phase.

Risk assessment:

- Directly implementing formal three-floor scene loading would exceed stabilization scope.
- Stairs modal, formal level scenes, Boss, Victory, and Defeat remain future features.

Required reproduction:

- In `GridTest.unity`, verify Key pickup, Stairs second-click placeholder progression, and LevelUp refresh.

Future recommendation:

- Deferred candidate.
- Prefer documentation boundary.
- Reject direct upgrade to formal three-floor scene loading in this stabilization proposal.

## 12. Priority ranking

### High priority future candidates

- A: Inspector binding validation helper.
- B: Enemy AI turn gate issue.
- E: UI target list / `enemyUnits` dependency.

### Medium priority future candidates

- F: Enemy Floating HP Bar robustness.
- G: Room activation `assignedEnemies` / `spawnTiles` validation.

### Deferred candidates

- C: Skill cooldown tick flow.
- D: Fireball splash automatic collection beyond checklist.
- H: Key / Stairs / LevelUp beyond placeholder boundary.

### Rejected changes

- Phase 14.6 direct implementation.
- Direct scene/prefab/SO/C# changes.
- Formula edits.
- Formal three-floor scene loading inside stabilization.

## 13. Play Mode reproduction requirements

Must reproduce in `GridTest.unity` before any future implementation:

- B: Enemy AI action rejection by turn gate.
- C: Cooldown behavior blocking current gameplay.
- D: Fireball splash missing valid adjacent targets.
- E: Red/yellow UI highlights missing valid enemies.
- F: Enemy HP bar refresh, death-hide, or raycast failure.
- G: Room enemy activation, placement, or clear failure.
- H: Key / Stairs / LevelUp placeholder behavior failing.

Candidate A can begin with Inspector-only validation. If it becomes a runtime validator, it should also be verified in `GridTest.unity`.

## 14. Future allowed files per candidate

These are maximum possible future scopes for separately approved phases. They do not allow Phase 14.6 changes.

- A: Future Docs; optionally a new read-only validation/test script under `Assets/_BoneThrone/Scripts/Tests/` or `Assets/_BoneThrone/Scripts/Validation/`.
- B: Future files under `Assets/_BoneThrone/Scripts/AI/` and `Assets/_BoneThrone/Scripts/Turns/`, only after reproduction.
- C: Future files under `Assets/_BoneThrone/Scripts/Turns/` and `Assets/_BoneThrone/Scripts/Skills/`, only after reproduction.
- D: Prefer Docs/checklist. If approved later, only target-source boundary work near skill execution, without formula changes.
- E: Future files under `Assets/_BoneThrone/Scripts/UI/`; optionally a read-only enemy provider if approved.
- F: Future `Assets/_BoneThrone/Scripts/UI/EnemyFloatingHealthBarView.cs` only, unless asset changes are separately approved.
- G: Future Docs or read-only validation/test script; Room runtime files only if a later phase explicitly approves.
- H: Future Docs first; lightweight prompt clarity only if separately approved. No formal scene loading in this track.

## 15. Future forbidden files per candidate

Forbidden by default for all candidates:

- `DamageResolver`.
- `SkillEffectExecutor` skill formulas.
- `CombatSystem.TryBasicAttack` execution semantics.
- `SkillSystem.TryUseSkill` execution semantics.
- `CombatLog` structured event model.
- KayKit original resources.
- `Skeleton_Rogue` name or ordinary enemy identity.
- Any change that uses `Skeleton_Golem` as a normal enemy.
- Player Ranger Adventurers Rogue visual.
- `GridTest.unity`, unless a future phase explicitly approves scene modification.
- Prefab assets, unless a future phase explicitly allows prefab modification.
- ScriptableObject assets, unless a future phase explicitly allows SO modification.

Additional candidate-specific forbidden changes:

- A: No auto-fixing Inspector values.
- B: No broad turn-system rewrite.
- C: No skill formula changes.
- D: No Fireball damage formula changes.
- E: No highlight path that calls `TryBasicAttack` or `TryUseSkill`.
- F: No HP mutation from HP bar UI.
- G: No default Room runtime rewrite.
- H: No formal Level_01 / Level_02 / Level_03 scene loading.

## 16. Recommended future implementation order

These are future phases only. They are not Phase 14.6 work.

1. 14.6-A Play Mode reproduction pass.
   - Run the Phase 14.5 regression checklist in `GridTest.unity`.
   - Record failures without fixing.

2. 14.6-B Inspector-only correction pass, if user approves scene/prefab binding edits.
   - Only correct missing Inspector bindings if explicitly approved.
   - This would modify scene/prefab assets and therefore must be separate.

3. 14.6-C Enemy AI gate minimal fix proposal / implementation split.
   - First produce a narrow fix proposal if Enemy AI rejection is reproduced.
   - Implement only in a later approved phase.

4. 14.6-D UI target provider proposal.
   - Propose a read-only enemy provider only if red/yellow highlight misses are reproduced.
   - Implement only in a later approved phase.

5. 14.6-E HP bar robustness proposal.
   - Propose script-only defensive checks only if refresh/death/raycast failures are reproduced.

6. 14.6-F Room validation helper proposal.
   - Propose read-only validation for assigned enemies and spawn tiles after room failures or Inspector mismatch are confirmed.

7. Later skill/level phases.
   - Cooldown tick flow and formal level loading should wait for clearer dedicated phases.

## 17. Rejected changes

Reject the following in Phase 14.6:

- Phase 14.6 directly writing code.
- Modifying scene / prefab / SO / C# / `Assets`.
- Modifying `Packages` or `ProjectSettings`.
- Calling `TryBasicAttack` for highlight.
- Calling `TryUseSkill` for highlight.
- Implementing CombatLog by parsing strings.
- Modifying `DamageResolver`.
- Modifying skill formulas.
- Using `Skeleton_Golem` as a normal enemy.
- Renaming `Skeleton_Rogue`.
- Changing Player Ranger back to Ranger visual.
- Modifying KayKit original resources.
- Directly upgrading placeholder progression into formal three-floor scene loading.

## 18. Do-not-touch list

Do not touch:

- `DamageResolver`.
- `SkillEffectExecutor` skill formulas.
- `CombatSystem.TryBasicAttack` execution semantics.
- `SkillSystem.TryUseSkill` execution semantics.
- `CombatLog` structured event model.
- KayKit original resources.
- `Skeleton_Rogue`.
- `Skeleton_Golem`.
- Player Ranger Adventurers Rogue visual.
- `GridTest.unity`, unless a future phase explicitly approves scene modification.
- Prefabs and ScriptableObject assets, unless a future phase explicitly approves those asset changes.
- `Assets`, `Packages`, and `ProjectSettings` during documentation-only phases.

## 19. Git diff validation

Phase 14.6 allows Docs-only changes.

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

## 20. Rollback

To roll back Phase 14.6 documentation only, remove:

- `Docs/Phase14_MinimalStabilizationImplementationProposal.md`
- `Docs/DevLogs/Phase14.6_MinimalStabilizationProposal.md`

If `Docs/ACTIVE_TASK.md` is modified in a future documentation update, manually restore only the Phase 14.6-related text. Do not use broad reset commands if other Phase 14 documentation work is still in progress.
