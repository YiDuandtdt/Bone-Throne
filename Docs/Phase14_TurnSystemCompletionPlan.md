# Phase 14.14 - Turn System Completion Plan

## 1. Purpose and scope

This document defines the Phase 14 turn system completion plan.

Phase 14.14 is planning only:

- No gameplay code is written.
- No C# files are modified.
- No assets, prefabs, scenes, Packages, or ProjectSettings are modified.
- Defend and Potion are not implemented in this phase.
- Turn System Completion is not implemented in this phase.

The purpose is to define the minimum future path for completing the local single-player turn loop while preserving the current GridTest baseline.

## 2. Source of truth

- `AGENTS.md`
- `Docs/ACTIVE_TASK.md`
- `Docs/DevLogs/Phase14.13E_CombatActionEconomyAudit.md`
- `Docs/DevLogs/Phase14.13A_SkillAssetsAndPrefabSlots.md`
- `Docs/DevLogs/Phase14.13B_UISkillSlotWiring.md`
- `Docs/DevLogs/Phase14.13C_SkillEffectBranches.md`
- `Docs/DevLogs/Phase14.13D_FormalSkillDataMigration.md`
- `Docs/DevLogs/Phase14.11_ActiveEnemyProvider.md`
- `Docs/骸骨王座_系统设计文档_Unity6.3LTS_v2.2_CurrentProjectState.md`
- `Docs/骸骨王座_Codex完整即用Vibecoding开发文档_Unity6.3LTS_v1.2_CurrentProjectState.md`

## 3. Current real state

Current `TurnManager` state:

- `TurnManager` has `PlayerTurn` and `EnemyTurn`.
- The fixed order is Fighter -> Ranger -> Mage -> Barbarian -> Enemy.
- `StartPlayerRound()` resets player unit turn state and enters `PlayerTurn`.
- `AdvanceTurn()` advances role / phase.
- `EndCurrentActorTurn()` currently calls `AdvanceTurn()`.
- `EnemyTurn` currently does not automatically execute full AI.

Current `UnitTurnState` state:

- `UnitTurnState` records `hasMoved`.
- `UnitTurnState` records `hasActed`.
- `UnitTurnState.MarkMoved()` records movement consumption.
- `UnitTurnState.MarkActed()` records action consumption.
- `UnitTurnState.ResetForNewRound()` clears moved / acted flags.

Current action consumption:

- Basic Attack succeeds through `CombatSystem.TryBasicAttack(...)` and then calls `MarkActed`.
- Skill succeeds through `SkillSystem.TryUseSkill(...)`, calls `StartCooldown`, then calls `MarkActed`.
- Move succeeds through `PlayerMovementController.TryMoveSelectedUnitTo(...)` and then calls `MarkMoved`.

Current cooldown state:

- `SkillRuntime.TickCooldowns()` exists.
- `SkillSystem.TickCooldownsForUnit(...)` exists.
- Cooldown tick is not connected to formal turn flow.

Current placeholder state:

- Defend is currently a placeholder.
- Potion is currently a placeholder.

Current enemy turn state:

- Enemy AI can execute a single enemy decision through `EnemyAIController`.
- `EnemyTurn` does not currently orchestrate all active alive enemies.
- Enemy AI is not yet a complete formal enemy turn runner.

## 4. Current action economy problems

Current problems:

- Actor turn lifecycle is missing.
- Cooldown tick is not connected to the formal turn system.
- `EnemyTurn` is only a phase, not a complete enemy turn flow.
- `ActionPermissionService` is currently player-oriented.
- Defend is still a placeholder.
- Potion is still a placeholder.
- Enemy AI and the player-only action gate can conflict.

## 5. Recommended turn rule

The recommended local action economy is:

- Each actor can move once per round.
- Each actor can take one action per round.
- Move does not consume action.
- Action includes:
  - Basic Attack
  - Skill
  - Defend
  - Potion
- Do not introduce initiative.
- Do not introduce action points.
- Do not introduce network synchronization in this phase family.

This keeps the current tactics loop simple and compatible with the existing `UnitTurnState` model.

## 6. Recommended cooldown tick timing

Recommendation:

- Tick the current actor's cooldown at actor turn start.
- `TryUseSkill` still immediately calls `StartCooldown` after a skill succeeds.
- The skill cooldown ticks again at the next turn start for that same actor.

Reasons:

- It matches the intuitive rule that cooldown decreases when that actor becomes active again.
- It avoids reducing cooldown immediately at actor turn end after a skill was just used.
- It avoids repeatedly ticking player cooldowns during a multi-enemy `EnemyTurn`.
- It fits the current fixed actor order.
- It keeps cooldown timing local to the actor lifecycle.

## 7. Player actor lifecycle

Recommended future player lifecycle:

1. `BeginActorTurn(role)`.
2. Find the corresponding player actor.
3. Tick the current actor cooldown.
4. Reset / prepare moved and acted state for the current actor.
5. Refresh UI / TurnBanner.
6. Player may move once.
7. Player may take one action.
8. `EndActorTurn`.
9. Enter the next role.

Initial implementation should prefer explicit turn ending over aggressive auto-advance. This keeps debugging and GridTest validation easier.

## 8. Enemy turn lifecycle

Recommended future enemy lifecycle:

1. Enter `EnemyTurn`.
2. Collect active alive enemies.
3. Each enemy gets move/action allowance.
4. `EnemyAIController` executes a single enemy decision.
5. `EnemyTurnRunner` orchestrates all enemy actions.
6. EnemyTurn completes.
7. Return to Fighter / `PlayerTurn`.

Boundary:

- Do not implement a complex behavior tree.
- Do not implement enemy skills in this turn completion phase.
- Keep enemy turn orchestration deterministic and small.

## 9. ActionPermissionService expansion plan

Recommended future expansion:

- Keep `CanMove`.
- Keep `CanAct`.
- Support `PlayerTurn + UnitFaction.Player`.
- Support `EnemyTurn + UnitFaction.Enemy`.
- Continue checking:
  - unit exists
  - unit is alive
  - unit has `UnitTurnState`
  - `hasMoved`
  - `hasActed`
- Player actors can keep current role restrictions.
- Enemy actors can be allowed through `EnemyTurn` / `RoleId.Enemy`.

Important UI rule:

- UI must not directly call `MarkMoved`.
- UI must not directly call `MarkActed`.
- Gameplay services remain responsible for consuming movement and action.

## 10. TurnManager minimal extensions

Future minimum `TurnManager` extensions:

- `BeginActorTurn`
- `EndActorTurn`
- `BeginEnemyTurn`
- `EndEnemyTurn`
- current actor role query
- current actor unit query
- cooldown tick hook
- enemy turn runner call point
- clear boundary between player round reset and actor reset

`TurnManager` should remain a lightweight coordinator, not a large tactical rules engine.

## 11. EnemyAIController / EnemyTurnRunner split

Recommended split:

- `EnemyAIController` only handles one enemy's single decision.
- `EnemyTurnRunner` orchestrates `EnemyTurn`.

`EnemyTurnRunner` future responsibilities:

- collect active alive enemies
- prepare each enemy allowance
- call `EnemyAIController` for each enemy
- complete enemy turn
- notify `TurnManager` when all enemies are done

Boundaries:

- no complex behavior tree
- no enemy skill implementation
- no broad enemy system rewrite

## 12. SkillRuntime cooldown tick integration

Recommended future integration:

- `TurnManager` decides tick timing.
- `TurnManager` calls `SkillSystem.TickCooldownsForUnit(actor)`.
- `SkillSystem` calls `SkillRuntime.TickCooldowns()`.
- `SkillSystem.TryUseSkill` semantics are not changed.

This keeps `SkillRuntime` focused on cooldown storage, `SkillSystem` focused on skill runtime operations, and `TurnManager` focused on lifecycle timing.

## 13. Defend future integration

Defend future rule:

- Defend is an action.
- Future `DefendSystem.TryDefend(unit)` checks `CanAct`.
- On success, it sets lightweight defend state.
- On success, it calls `MarkActed`.
- CombatLog should record a structured defend entry.
- Do not implement taunt.
- Do not implement Defend in Phase 14.14.

The actual reduction model must be designed in Phase 14.16 before implementation.

## 14. Potion future integration

Potion future rule:

- Potion is an action.
- Future `PotionSystem.TryUsePotion(unit)` checks `CanAct`.
- On success, it applies simple healing.
- On success, it calls `MarkActed`.
- CombatLog should record a structured potion / heal entry.
- Do not implement a backpack system.
- Do not implement Potion in Phase 14.14.

Potion count, if needed, should be handled with lightweight state in a later plan.

## 15. Future implementation allowed files

These files may be considered for future Phase 14.15 implementation only. They are not allowed to change in Phase 14.14:

- `TurnManager.cs`
- `ActionPermissionService.cs`
- `UnitTurnState.cs`
- `SkillSystem.cs`, only for cooldown tick hook.
- `EnemyAIController.cs`
- new `EnemyTurnRunner.cs`
- UI turn display / End Turn related scripts
- DevLog

## 16. Future forbidden files

Future implementation should not touch these unless a later phase explicitly approves:

- KayKit original resources.
- `Skeleton_Golem` as a normal enemy.
- `Skeleton_Rogue` name / identity.
- Ranger gameplay renamed to Rogue.
- Player Ranger Rogue visual.
- SkillData field values.
- `SkillEffectExecutor` skill formulas.
- `DamageResolver`, unless a future Defend Plan explicitly approves it.
- `CombatSystem.TryBasicAttack` semantics.
- `SkillSystem.TryUseSkill` semantics.
- UI directly changing HP / cooldown / acted / moved.
- Highlight preview calling `TryBasicAttack`.
- Highlight preview calling `TryUseSkill`.
- Broad scene rewrites.
- Broad prefab rewrites.

## 17. Recommended next phases

Recommended future sequence:

- Phase 14.15 - Turn System Completion Implementation
- Phase 14.16 - Defend Action Plan
- Phase 14.17 - Defend Action Implementation
- Phase 14.18 - Potion Action Plan
- Phase 14.19 - Potion Action Implementation
- Phase 14.20 - Skill Rebuild Design
- Phase 14.21 - Skill Rebuild Implementation
- Phase 14 Final Regression

## 18. Unity Play Mode regression checklist

Future regression after turn completion must cover:

- Fighter -> Ranger -> Mage -> Barbarian -> Enemy order.
- Each player actor can move only once.
- Each player actor can take only one action.
- Basic Attack consumes action.
- Skill consumes action and starts cooldown.
- Cooldown ticks at actor turn start.
- EnemyTurn automatically handles active alive enemies.
- Enemy move / attack is not incorrectly rejected by player-only gate.
- EnemyTurn completes and returns to PlayerTurn.
- UI Slot 0 / 1 / 2 still works.
- CombatLog structured entries still work.
- Enemy HP Bar refresh / death hide still works.
- ActiveUnitProvider target collection still works.
- Camera controls still work.
- Room / Key / Stairs / LevelUp still work.
- Console has no new red errors.

## 19. Risks and rollback

Risks:

- Cooldown tick timing changes skill pacing.
- Enemy action gate changes can accidentally break player UI actions.
- Automatic actor turn progression can conflict with the current free-selection test flow.
- EnemyTurn orchestration can become too complex if it absorbs AI behavior.
- Mixing Defend / Potion implementation into turn completion would make regressions harder to isolate.

Rollback guidance for future implementation:

- Implement Phase 14.15 separately.
- Do not implement Defend / Potion during turn completion.
- Keep changes to a small whitelist.
- If regression fails, revert only the current phase.
- Do not make opportunistic skill, asset, scene, or prefab changes during turn system implementation.

Phase 14.14 rollback:

- Delete `Docs/Phase14_TurnSystemCompletionPlan.md`.
