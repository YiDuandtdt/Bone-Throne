# Phase 14.4 - Stabilization Plan Only

Date: 2026-05-27

## Scope

Phase 14.4 created a documentation-only stabilization plan for the current Bone Throne Unity 6.3 LTS project.

This phase did not implement fixes.

## Files changed

- Added `Docs/Phase14_StabilizationPlan.md`
- Added `Docs/DevLogs/Phase14.4_StabilizationPlan.md`
- `Docs/ACTIVE_TASK.md` was already set to Phase 14.4 and did not require a content update.

## Source material

- `AGENTS.md`
- `Docs/ACTIVE_TASK.md`
- `Docs/Phase14_ProjectStateAudit.md`
- `Docs/骸骨王座_系统设计文档_Unity6.3LTS_v2.2_CurrentProjectState.md`
- `Docs/骸骨王座_Codex完整即用Vibecoding开发文档_Unity6.3LTS_v1.2_CurrentProjectState.md`
- `Docs/DevLogs/`

## Stabilization areas recorded

The plan covers:

- Inspector manual binding risks.
- UI action targeting safety.
- Fireball splash and `knownUnits`.
- Structured `CombatLog`.
- Enemy Floating HP Bar.
- Enemy AI / turn gate.
- Room activation.
- Key / Stairs / LevelUp placeholder progression.
- `GridTest.unity` regression as the current only integrated validation scene.

## Play Mode reproduction first

The plan marks these as requiring Unity Play Mode reproduction before future implementation proposals:

- Enemy AI rejected by `TurnManager` / `ActionPermissionService`.
- Fireball splash missing secondary targets.
- Basic Attack red highlight missing valid enemies.
- Skill Slot 0 yellow highlight missing valid enemies.
- CombatLog missing structured Fireball splash rows.
- Enemy Floating HP Bar HP refresh, death hiding, or raycast blocking failures.
- Room enemy activation or room clear failures.
- Key pickup, Stairs two-click interaction, and LevelUp refresh failures.

No Unity Play Mode validation was run in this phase.

## Inspector-only checklist first

The plan marks these as Inspector checklist candidates before code is considered:

- `SkillEffectExecutor.knownUnits`
- `BattleHUDController.enemyUnits`
- `UIActionModeController.enemyUnits`
- `BattleHUDController.playerUnits`
- `TurnManager.playerUnits`
- `LevelProgressionService.playerUnits`
- `RoomEnemyActivator.assignedEnemies`
- `RoomEnemyActivator.spawnTiles`
- `LevelProgressionService.requiredClearedRooms`
- `GridManager.initialTiles`
- `KeyItem.progressionService`
- `InteractableStairs.progressionService`
- `InteractableStairs.selectionManager`

## Future implementation candidates

The plan lists future implementation candidates only. They are not Phase 14.4 tasks.

Any code or asset implementation requires a later separate phase and explicit user approval. Phase 14.6 is still defined as `Minimal Stabilization Implementation Proposal Only`, so it must not implement code by default.

Candidate areas include:

- Runtime query replacement or supplement for manual `knownUnits` / `enemyUnits` arrays.
- Missing binding diagnostics.
- Formal EnemyTurn scheduler proposal.
- Cooldown turn-end flow proposal.
- Room / Level progression diagnostics.
- Enemy Floating HP Bar stale-reference diagnostics.
- Future formal three-level scene loading.

## Do-not-touch rules repeated

The plan repeats these boundaries:

- Do not modify `DamageResolver`.
- Do not casually modify `SkillEffectExecutor` skill formulas.
- Do not change `CombatSystem.TryBasicAttack` execution semantics.
- Do not change `SkillSystem.TryUseSkill` execution semantics.
- Do not replace structured CombatLog with string parsing.
- Do not modify KayKit original resources.
- Do not rename `Skeleton_Rogue`.
- Do not use `Skeleton_Golem` as a normal enemy.
- Do not change Player Ranger away from Adventurers Rogue visual.
- Do not modify `GridTest.unity` unless a future phase explicitly approves scene changes.

## Validation

Documentation-only validation:

- No Unity Play Mode was run.
- No fixes were implemented.
- No gameplay code was written.
- No `Assets`, `Packages`, or `ProjectSettings` changes were intentionally made.
- No scene, prefab, ScriptableObject asset, C# script, or KayKit original resource was intentionally modified.

Recommended verification commands:

```powershell
git status --short
git diff --name-only
git diff -- Docs
```

## Recommended next phase

Recommended next phase:

- Phase 14.5 - Inspector Binding and Regression Checklist

Phase 14.5 should turn the stabilization plan into concrete Inspector binding and regression checklists without implementing fixes.

## Rollback

To roll back this phase only, remove:

- `Docs/Phase14_StabilizationPlan.md`
- `Docs/DevLogs/Phase14.4_StabilizationPlan.md`

Do not use broad reset commands if other Phase 14 documentation work is still in progress.
