# 骸骨王座 Codex 完整即用 Vibecoding 开发文档 Unity 6.3 LTS v1.2 Current Project State

## 0. Document purpose

This document replaces the old from-zero execution roadmap with a current-project continuation roadmap.

It is written for Codex tasks after Phase 14.3. It assumes Phase 0-13 already happened and must not be regenerated from scratch.

Use this document to:

- keep Codex aligned with the real Unity 6.3 LTS project state
- prevent accidental rewrites of existing systems
- keep documentation, stabilization, and future feature work separated
- preserve the long-term LAN / host-authoritative direction without claiming it is already implemented

## 1. Current source of truth

Every new Codex task must read these sources before planning:

- `AGENTS.md`
- `Docs/ACTIVE_TASK.md`
- `Docs/Phase14_ProjectStateAudit.md`
- `Docs/骸骨王座_系统设计文档_Unity6.3LTS_v2.2_CurrentProjectState.md`
- `Docs/DevLogs/`

If `Docs/Phase14_ProjectStateAudit.md` displays encoding corruption in the current terminal, do not copy corrupted text. Use the Phase 14.1 audit conclusions, the v2.2 current-state system design document, and DevLogs to reconstruct facts in clean UTF-8 Markdown.

Especially important DevLogs:

- `Docs/DevLogs/Phase12.6_Gameplay_Prefab_Wrapping.md`
- `Docs/DevLogs/Phase13_UI_Feedback.md`

These two files define the current prefab/visual rules and UI safety boundaries.

## 2. Long-term rules to preserve

The following rules remain long-term project direction. They must not be written as fully complete unless a later phase implements and validates them.

- Engine: Unity 6.3 LTS.
- Rendering: URP / Universal 3D.
- Battle UI: uGUI + TextMeshPro.
- LAN target stack: Netcode for GameObjects + Unity Transport.
- Future networking architecture: host-authoritative ActionCommand.
- LAN target: exactly four players.
- Multiplayer turn order: Fighter -> Ranger -> Mage -> Barbarian -> Enemy.
- Single-player must remain playable without NetworkManager.
- Formal three-floor dungeon remains a target.
- Boss, Victory, Defeat, MainMenu, Lobby, Ready, Role Slots remain targets.
- CharacterData, EnemyData, LevelData, and RoomData data-driven content remains a target.

## 3. Phase 0-13 are completed history

Do not treat Phase 0-13 as a to-do list.

Completed historical phases:

- Phase 0: repository rules, AGENTS, Docs, DevLogs baseline.
- Phase 1: package baseline including URP, TextMeshPro, Netcode for GameObjects, Unity Transport, Multiplayer Tools / Play Mode.
- Phase 2: Core skeleton with ActionCommand, GameStateSnapshot, IGameSession, LocalGameSession.
- Phase 3: grid coordinate and Tile / GridManager foundation.
- Phase 4: Unit, UnitStats, UnitRuntimeState, UnitData, UnitFaction.
- Phase 5: selection, BFS movement range, A* pathfinding, UnitMover, movement highlighter.
- Phase 6: turn phase, UnitTurnState, TurnOrderService, TurnManager, ActionPermissionService.
- Phase 7: D20 basic attack combat, DamageResolver, CombatLog, CombatSystem.
- Phase 8: initial enemy AI.
- Phase 9: room state, room trigger, room shadow, enemy activation.
- Phase 10: shared key, stairs, placeholder level progression, party level-up.
- Phase 11: SkillData, SkillRuntime, SkillTargetingService, SkillSystem.
- Phase 12: four Slot 0 representative skills and SkillEffectExecutor.
- Phase 12.5: KayKit art import and render compatibility.
- Phase 12.6: gameplay prefab wrapping for players, ordinary enemies, Key, and Stairs.
- Phase 13: BattleHUD, UI action targeting, CombatLog feedback, highlights, and enemy floating HP bar.

## 4. Do not regenerate existing systems

Future Codex prompts must not ask Codex to recreate these from scratch:

- Core skeleton
- Grid
- Unit
- Movement
- Turn
- Combat
- Enemy AI initial version
- Room / Key / Stairs / Level progression initial version
- Skill framework
- Four Slot 0 representative skills
- BattleHUD
- UI action targeting
- CombatLog
- Enemy Floating HP Bar

Allowed future work should be scoped as extension, stabilization, documentation, or carefully approved refactor. It must preserve current single-player behavior.

## 5. Current real project state

Current implemented state:

- `GridTest.unity` is the current only real integrated validation scene.
- `MainMenu.unity` is still a placeholder.
- Current committed ScriptableObject gameplay data assets are SkillData only.
- The four player characters currently implement only Slot 0 representative skills.
- Current UI action flow is `UI intent -> UIActionModeController -> gameplay service`.
- `CombatLog` is structured event feedback and must not be rebuilt by parsing strings.
- Fireball splash uses structured `SkillEffectResult`.

Current architecture skeleton state:

- `ActionCommand`, `IGameSession`, and `GameStateSnapshot` exist.
- They are not connected to the actual gameplay flow yet.
- Current local UI and test helpers call gameplay services directly.

Current placeholder / deferred state:

- There are no formal `Level_01`, `Level_02`, or `Level_03` scenes.
- There is no Boss.
- There is no Victory / Defeat flow.
- There is no LAN lobby.
- There are no Ready / Role Slot systems.
- `CharacterData`, `EnemyData`, `LevelData`, and `RoomData` are not formally assetized.
- Skill Slot 1, Skill Slot 2, Defend, and Potion remain placeholder actions.
- Formal stairs modal confirmation is deferred.
- Formal VFX, SFX, animation controllers, icons, and portraits are deferred.

## 6. Current UI safety rules

UI is an intent layer, not a gameplay authority layer.

Current flow:

`UI intent -> UIActionModeController -> gameplay service`

Rules:

- UI must not directly change HP.
- UI must not directly change cooldown.
- UI must not directly call `MarkActed`.
- UI must not directly call `MarkMoved`.
- UI must not roll D20.
- UI must not decide death.
- UI must not mutate room, key, stairs, level, AI, or networking state directly.

Correct examples:

- Basic Attack button sends intent to UI action mode, then calls `CombatSystem.TryBasicAttack`.
- Skill Slot 0 button sends intent to UI action mode, then calls `SkillSystem.TryUseSkill`.
- Move button sends intent to UI action mode, then calls the existing movement controller path.

## 7. Current Inspector binding checklist

Before testing or planning scene/prefab work, inspect these bindings:

- `GridManager.initialTiles`
- `TurnManager.playerUnits`
- `LevelProgressionService.playerUnits`
- `LevelProgressionService.requiredClearedRooms`
- `RoomEnemyActivator.assignedEnemies`
- `RoomEnemyActivator.spawnTiles`
- `SkillEffectExecutor.knownUnits`
- `BattleHUDController.playerUnits`
- `BattleHUDController.enemyUnits`
- `UIActionModeController.enemyUnits`
- `KeyItem.progressionService`
- `InteractableStairs.progressionService`
- `InteractableStairs.selectionManager`

Required minimum binding terms to remember:

- `knownUnits`
- `enemyUnits`
- `playerUnits`
- `assignedEnemies`
- `spawnTiles`
- `requiredClearedRooms`
- `initialTiles`

Missing bindings can break Fireball splash, UI target highlights, room activation, level progression, HUD hero panels, or stairs prompts.

## 8. Regression checklist

Use `GridTest.unity` for current integrated regression unless a later phase creates a new approved validation scene.

Regression checklist:

- Move
- Basic Attack
- Skill Slot 0
- Fireball splash
- CombatLog
- Enemy Floating HP Bar
- Room trigger
- Room shadow
- Enemy activation
- Key pickup
- Stairs interaction
- LevelUp
- Enemy AI

Expected safety checks:

- UI clicks do not directly mutate gameplay state.
- Invalid targets do not consume actions.
- Highlight preview does not call execution APIs.
- CombatLog rows come from structured entries.
- Fireball splash writes structured SkillEffectResult entries.
- Enemy HP bars update without blocking raycasts.
- Skeleton_Rogue remains the normal rogue enemy.
- Skeleton_Golem is untouched.
- Ranger keeps Adventurers Rogue visual.

## 9. Do-not-touch rules

These rules must appear in future prompts when relevant:

- Do not modify `DamageResolver`.
- Do not casually modify `SkillEffectExecutor` skill formulas.
- Do not call `TryBasicAttack` for highlighting.
- Do not call `TryUseSkill` for highlighting.
- Do not let UI directly modify HP, cooldown, acted state, or moved state.
- Do not implement CombatLog by parsing strings.
- Do not modify KayKit original resources.
- `Skeleton_Rogue` is the normal enemy and must not be renamed.
- `Skeleton_Golem` is reserved for future Boss / heavy Boss and must not be used as a normal enemy.
- Player Ranger uses Adventurers Rogue visual and must not be changed back to Adventurers Ranger.
- During documentation-only phases, do not modify `Assets`, `Packages`, or `ProjectSettings`.

## 10. Updated general Codex context prompt

Use this for future work:

```text
You are continuing the Unity 6.3 LTS project Bone Throne / 骸骨王座.

Before planning, read:
1. AGENTS.md
2. Docs/ACTIVE_TASK.md
3. Docs/Phase14_ProjectStateAudit.md
4. Docs/骸骨王座_系统设计文档_Unity6.3LTS_v2.2_CurrentProjectState.md
5. Docs/骸骨王座_Codex完整即用Vibecoding开发文档_Unity6.3LTS_v1.2_CurrentProjectState.md
6. Docs/DevLogs/, especially Phase12.6 and Phase13

Phase 0-13 are completed history. Do not regenerate existing Core, Grid, Unit, Movement, Turn, Combat, Enemy AI, Room/Key/Stairs/Level, Skill, or UI systems from scratch.

Preserve Unity 6.3 LTS, URP, uGUI + TextMeshPro, NGO + Unity Transport, host-authoritative ActionCommand direction, LAN four-player target, and Fighter -> Ranger -> Mage -> Barbarian -> Enemy order as long-term goals.

Do not write future features as already complete. Boss, Victory/Defeat, LAN lobby, formal three-floor scenes, and CharacterData/EnemyData/LevelData/RoomData remain deferred unless the current phase explicitly implements them.
```

## 11. Updated planning prompt template

```text
Current phase: [phase name]

Read AGENTS.md, ACTIVE_TASK.md, Phase14_ProjectStateAudit, the v2.2 CurrentProjectState system design document, this v1.2 Vibecoding document, and DevLogs before planning.

Output a plan only. Do not modify files yet.

Your plan must include:
1. Goal restatement.
2. Current implemented systems that this phase must preserve.
3. Files that may be changed.
4. Files that must not be changed.
5. Inspector bindings that may be affected.
6. Regression checklist entries to run in GridTest.
7. Risks and rollback.
8. Confirmation that you will not regenerate existing systems from scratch.
```

## 12. Updated implementation prompt template

Use only after the user explicitly confirms implementation.

```text
Confirmed. Implement only the approved scope.

Rules:
1. Do not modify files outside the approved list.
2. Do not modify scene/prefab/SO assets unless explicitly approved for this phase.
3. Do not modify DamageResolver or SkillEffectExecutor formulas unless explicitly approved.
4. Do not let UI directly mutate HP/cooldown/acted/moved state.
5. Do not call execution APIs only for highlights.
6. Keep single-player GridTest flow intact.
7. After changes, report actual files changed, Inspector setup, Play Mode steps, expected result, risks, and rollback.
```

## 13. Updated bugfix prompt template

```text
Current phase: [phase]
Unity version: Unity 6.3 LTS
Scene: GridTest.unity unless otherwise stated

Expected:
[what should happen]

Actual:
[what happens]

Console:
[full logs with file and line numbers]

Inspector / scene setup:
[bindings, prefab instance notes, selected objects]

Please:
1. Identify the top likely causes.
2. Ask me to verify Inspector bindings first when appropriate.
3. Propose the smallest safe fix.
4. Do not modify unrelated systems.
5. Do not touch do-not-touch systems unless the error is inside them and the user approves.
```

## 14. Phase 14.4 - Stabilization Plan Only

Purpose:

- Produce a stabilization plan only.
- No code changes.
- No scene/prefab/SO changes.
- Prefer Docs-only output.

Prompt:

```text
Current phase: Phase 14.4 - Stabilization Plan Only

Read AGENTS.md, ACTIVE_TASK.md, Phase14_ProjectStateAudit, v2.2 CurrentProjectState, v1.2 Vibecoding, and DevLogs.

Do not modify files unless I explicitly allow a Docs-only plan document.

Output:
1. Stabilization goals.
2. Current risks by system.
3. Manual Inspector binding risks.
4. Regression checklist.
5. Candidate future fixes, ranked by risk.
6. Files that must not be touched.
7. Recommended next phase.
```

## 15. Phase 14.5 - Inspector Binding and Regression Checklist

Purpose:

- Create documentation for Inspector bindings and regression tests.
- Docs-only by default.

Prompt:

```text
Current phase: Phase 14.5 - Inspector Binding and Regression Checklist

Read all Phase 14 docs and DevLogs first.

Create or update Docs-only checklist material. Do not modify Assets, scene, prefab, SO, Packages, ProjectSettings, or C# scripts.

The checklist must cover:
- knownUnits
- enemyUnits
- playerUnits
- assignedEnemies
- spawnTiles
- requiredClearedRooms
- initialTiles

The regression checklist must cover:
- Move
- Basic Attack
- Skill Slot 0
- Fireball splash
- CombatLog
- Enemy Floating HP Bar
- Room trigger
- Room shadow
- Enemy activation
- Key pickup
- Stairs interaction
- LevelUp
- Enemy AI
```

## 16. Phase 14.6 - Minimal Stabilization Implementation Proposal Only

Purpose:

- Proposal only.
- No code by default.
- True implementation requires a separate later phase and explicit user approval.

Prompt:

```text
Current phase: Phase 14.6 - Minimal Stabilization Implementation Proposal Only

Read all Phase 14 docs and DevLogs first.

Do not write code. Do not modify files.

Propose minimal stabilization implementation candidates only.

For each candidate, provide:
1. Problem addressed.
2. Why it matters.
3. Candidate files.
4. Systems preserved.
5. Inspector bindings affected.
6. Regression tests.
7. Risks.
8. Rollback.

Do not implement anything in this phase. Actual implementation must be a separate phase and requires explicit user approval.
```

## 17. Phase 14.7 - Pre-Feature Regression Audit

Purpose:

- Audit readiness before future feature work such as Boss, formal levels, data assets, or LAN.
- No code by default.

Prompt:

```text
Current phase: Phase 14.7 - Pre-Feature Regression Audit

Read AGENTS.md, ACTIVE_TASK.md, Phase14_ProjectStateAudit, v2.2 CurrentProjectState, v1.2 Vibecoding, and all DevLogs.

Do not modify files unless explicitly approved for Docs-only audit output.

Audit:
1. GridTest regression readiness.
2. Inspector bindings.
3. Current deferred features.
4. Do-not-touch compliance.
5. Risks before Boss / formal levels / data assets / LAN.
6. Recommended next implementation phase.
```

## 18. Future feature prompt rules

Future feature prompts must:

- Identify whether the task is documentation, stabilization, or feature implementation.
- State whether scene/prefab/SO changes are allowed.
- Preserve current single-player GridTest behavior.
- Use existing systems instead of regenerating them.
- Include Inspector setup and Play Mode validation.
- Include rollback.

Boss, LAN, formal levels, and data assetization must each be separate phases. Do not bundle them together.

## 19. Git diff and validation rules

For documentation-only phases:

```powershell
git status --short
git diff -- Docs
git diff --name-only
```

Expected:

- Only Docs files changed.
- No `Assets/`.
- No `Packages/`.
- No `ProjectSettings/`.
- No `.cs`.
- No `.unity`.
- No `.prefab`.
- No `.asset`.

For implementation phases:

- Confirm allowed files before editing.
- Report actual changed files.
- Run or describe Unity Play Mode validation.
- Never claim Unity validation passed unless it was actually run and confirmed.

## 20. Rollback rules

Documentation-only rollback:

```powershell
git checkout -- Docs/ACTIVE_TASK.md
git rm [new docs file]
```

Implementation rollback:

- Prefer `git checkout -- <specific-file>` only for files changed in the current task.
- Never use broad destructive reset commands unless the user explicitly requests them.
- Do not revert user changes unrelated to the current task.
