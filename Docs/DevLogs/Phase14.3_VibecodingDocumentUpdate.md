# Phase 14.3 - Vibecoding Development Document Update

Date: 2026-05-27

## Scope

Phase 14.3 updated the Codex Vibecoding development guide so it no longer reads as a from-zero implementation plan. The new guide is a current-project continuation document based on Phase 14.1 audit findings, the Phase 14.2 current-state system design document, and existing DevLogs.

This phase was documentation-only.

## Files changed

- Added `Docs/骸骨王座_Codex完整即用Vibecoding开发文档_Unity6.3LTS_v1.2_CurrentProjectState.md`
- Added `Docs/DevLogs/Phase14.3_VibecodingDocumentUpdate.md`
- `Docs/ACTIVE_TASK.md` was inspected and did not require a content change for this phase.

## Source material

- `AGENTS.md`
- `Docs/ACTIVE_TASK.md`
- `Docs/Phase14_ProjectStateAudit.md`
- `Docs/骸骨王座_系统设计文档_Unity6.3LTS_v2.2_CurrentProjectState.md`
- `Docs/DevLogs/`
- Original Vibecoding direction from `Docs/骸骨王座_Codex完整即用Vibecoding开发文档_Unity6.3LTS_v1.1.docx` was retained only as long-term project direction where still valid.

## What was updated

The new Vibecoding v1.2 guide now defines the current source of truth:

- `Docs/Phase14_ProjectStateAudit.md`
- `Docs/骸骨王座_系统设计文档_Unity6.3LTS_v2.2_CurrentProjectState.md`
- `Docs/DevLogs/`

It states that Phase 0-13 are completed historical phases, not instructions to regenerate existing systems.

It explicitly warns future Codex runs not to recreate or replace:

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
- BattleHUD / UI action targeting / CombatLog / Enemy HP bar

## Current project state captured

The guide records the current real project state:

- `GridTest.unity` is the only real integration validation scene.
- `MainMenu.unity` remains a placeholder.
- There are no formal `Level_01`, `Level_02`, or `Level_03` scenes.
- Boss, Victory/Defeat, LAN lobby, Ready flow, and Role Slots are not implemented.
- Only `SkillData` ScriptableObject assets are currently present as gameplay data assets.
- `CharacterData`, `EnemyData`, `LevelData`, and `RoomData` are still deferred.
- The four player roles only have Slot 0 representative skills.
- `ActionCommand`, `IGameSession`, and `GameStateSnapshot` are architecture skeletons and are not wired into gameplay flow.
- Current UI action flow is `UI intent -> UIActionModeController -> gameplay service`.
- UI must not directly change HP, cooldown, acted state, or moved state.
- `CombatLog` is structured event output, not string parsing.
- Fireball splash uses structured `SkillEffectResult`.
- Fireball splash and UI target highlighting still depend on manual Inspector arrays such as `knownUnits` and `enemyUnits`.

## Inspector binding checklist added

The guide now includes a required Inspector binding checklist covering:

- `knownUnits`
- `enemyUnits`
- `playerUnits`
- `assignedEnemies`
- `spawnTiles`
- `requiredClearedRooms`
- `initialTiles`

It also records likely owning components for these bindings so future stabilization work can audit scenes without rewriting systems.

## Regression checklist added

The guide now includes a regression checklist covering:

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

## Safety and do-not-touch rules added

The guide now repeats Phase 14 safety boundaries:

- Do not modify `DamageResolver` casually.
- Do not casually modify `SkillEffectExecutor` skill formulas.
- Do not call `TryBasicAttack` or `TryUseSkill` for highlighting.
- Do not let UI directly modify HP, cooldown, acted state, or moved state.
- Do not implement structured `CombatLog` by parsing display strings.
- Do not modify KayKit original resources.
- `Skeleton_Rogue` is the ordinary enemy and must not be renamed.
- `Skeleton_Golem` is reserved for future Boss / heavy Boss use and must not be used as an ordinary enemy.
- Player Ranger uses Adventurers Rogue visual and must not be changed back to Ranger visual.

## Future prompts added

The guide now includes continuation prompts for:

- Phase 14.4 - Stabilization Plan Only
- Phase 14.5 - Inspector Binding and Regression Checklist
- Phase 14.6 - Minimal Stabilization Implementation Proposal Only
- Phase 14.7 - Pre-Feature Regression Audit

Phase 14.6 is explicitly proposal-only. It must not write code or files by default. Real implementation must happen in a separate phase after explicit user approval.

## Long-term goals preserved

The following original long-term directions remain valid but are not described as currently complete:

- Unity 6.3 LTS
- URP / Universal 3D
- uGUI + TextMeshPro
- Netcode for GameObjects + Unity Transport
- Host-authoritative `ActionCommand`
- LAN four-player target
- Fighter -> Ranger -> Mage -> Barbarian -> Enemy turn order
- Boss / Victory / Defeat
- MainMenu / Lobby / Ready / Role Slots
- Formal `Level_01`, `Level_02`, and `Level_03` scenes
- `CharacterData`, `EnemyData`, `LevelData`, and `RoomData`

## Validation

No Unity assets, scenes, prefabs, ScriptableObject assets, Packages, ProjectSettings, KayKit source assets, or C# scripts were intentionally modified in this phase.

Recommended verification commands:

```powershell
git status --short
git diff --name-only
git diff -- Docs
```

## Rollback

To roll back this phase only, remove the two added Phase 14.3 documentation files:

- `Docs/骸骨王座_Codex完整即用Vibecoding开发文档_Unity6.3LTS_v1.2_CurrentProjectState.md`
- `Docs/DevLogs/Phase14.3_VibecodingDocumentUpdate.md`

Do not use broad reset commands if other Phase 14 documentation work is still in progress.
