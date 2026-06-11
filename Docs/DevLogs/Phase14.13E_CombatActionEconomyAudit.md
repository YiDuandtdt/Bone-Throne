# Phase 14.13-E - Combat Action Economy and Turn System Audit

## Purpose and scope

Phase 14.13-E is a read-only audit of the current combat action economy and turn system.

This phase does not implement fixes, does not write gameplay code, and does not modify assets, prefabs, scenes, Packages, or ProjectSettings.

Phase 14 remains open after this audit. The project still needs:

- Turn system completion.
- Defend action.
- Potion action.
- Skill rebuild design and implementation.
- Final Phase 14 regression.

## 1. Current TurnManager player / enemy turn control

`TurnManager` is currently a lightweight local turn coordinator.

Current behavior:

- `StartPlayerRound()` resets player unit turn states and starts `PlayerTurn`.
- `AdvanceTurn()` advances through `TurnOrderService`.
- Fixed order is:
  - Fighter
  - Ranger
  - Mage
  - Barbarian
  - Enemy
- If current role is Enemy, `currentPhase` becomes `EnemyTurn`.
- When the system advances from `EnemyTurn` back to `PlayerTurn`, player unit turn states are reset.
- `EndCurrentActorTurn()` currently calls `AdvanceTurn()`.

Current limitation:

- `EnemyTurn` is still a placeholder phase.
- Enemy AI is not automatically executed by `TurnManager`.
- There is no complete actor turn lifecycle yet.
- There is no formal actor turn start / actor turn end event flow.

## 2. moved / acted state

Movement and action consumption are currently tracked by `UnitTurnState`, not by `UnitRuntimeState`.

`UnitTurnState` stores:

- `hasMoved`
- `hasActed`

It provides:

- `ResetForNewRound()`
- `MarkMoved()`
- `MarkActed()`

`UnitRuntimeState` currently handles HP and death state only.

## 3. Basic Attack action consumption

`CombatSystem.TryBasicAttack(attacker, target)` consumes action on success.

Current flow:

1. Validate participants.
2. Validate turn gate if `TurnManager` and `ActionPermissionService` are bound.
3. Validate combat services.
4. Resolve D20 attack.
5. Apply damage on hit through `DamageResolver`.
6. Write structured `CombatLog` entries.
7. Call `MarkAttackerActed(attacker)`.
8. `MarkAttackerActed` calls `UnitTurnState.MarkActed()`.

Preview/highlight path:

- `CombatSystem.CanBasicAttack(...)`
- Does not consume action.

## 4. Skill action consumption

`SkillSystem.TryUseSkill(caster, target, slotIndex)` consumes action on success.

Current flow:

1. Validate caster and `SkillRuntime`.
2. Validate turn gate if `TurnManager` and `ActionPermissionService` are bound.
3. Validate skill target through `SkillTargetingService`.
4. Execute skill through `SkillEffectExecutor` or fallback guaranteed damage.
5. Start slot cooldown through `SkillRuntime.StartCooldown(slotIndex)`.
6. Call `MarkCasterActed(caster)`.
7. `MarkCasterActed` calls `UnitTurnState.MarkActed()`.
8. Write structured `CombatLog` entries from `SkillEffectResult`.

Preview/highlight path:

- `SkillSystem.CanUseSkillOnTarget(...)`
- Does not consume action.

## 5. Move movement consumption

`PlayerMovementController.TryMoveSelectedUnitTo(tile)` consumes movement on successful movement.

Current flow:

1. Validate selection and target tile.
2. Validate reachable position.
3. Validate move permission through `ActionPermissionService.CanMove(...)` if turn gating is configured.
4. Resolve path through `Pathfinder`.
5. Move through `UnitMover.TryMove(...)`.
6. If turn gating is configured, call `MarkSelectedUnitMoved(selectedUnit)`.
7. `MarkSelectedUnitMoved` calls `UnitTurnState.MarkMoved()`.

Move consumes movement, not action.

## 6. cooldown tick status

Cooldown support exists, but it is not connected to formal turn flow.

Existing APIs:

- `SkillRuntime.TickCooldowns()`
- `SkillSystem.TickCooldownsForUnit(unit)`
- `SkillSystem.TickCooldownsForUnits(units)`

Current limitation:

- `TurnManager` does not tick cooldowns.
- Actor turn start does not tick cooldowns.
- Actor turn end does not tick cooldowns.
- Round start / round end does not tick cooldowns.
- Existing tick usage is test/helper oriented rather than formal gameplay flow.

Conclusion:

- Cooldown tick is not fully integrated yet.
- A future turn system phase must define whether cooldown ticks at actor turn start, actor turn end, player round start, or another explicit point.

## 7. Defend status

Defend is currently a placeholder.

Existing pieces:

- `ActionCommandType.Defend` exists.
- Runtime BattleHUD creates a `Defend` button.
- `SkillBarView` displays `Defend\nPlaceholder`.

Missing pieces:

- No Defend button event.
- No Defend execution service.
- No defend state.
- No damage reduction integration.
- No structured CombatLog defend entry.
- No action consumption implementation for Defend.

Conclusion:

- Defend is not implemented yet.

## 8. Potion status

Potion is currently a placeholder.

Existing pieces:

- `ActionCommandType.UseItem` exists.
- Runtime BattleHUD creates a `Potion` button.
- `SkillBarView` displays `Potion\nPlaceholder`.

Missing pieces:

- No Potion button event.
- No Potion / item action service.
- No potion count or use tracking.
- No healing implementation.
- No structured CombatLog potion/heal entry.
- No action consumption implementation for Potion.

Conclusion:

- Potion is not implemented yet.

## 9. BattleHUD / SkillBar Defend and Potion display

`BattleHUDController` creates seven runtime action buttons:

- Move
- BasicAttack
- SkillSlot0
- SkillSlot1
- SkillSlot2
- Defend
- Potion

`SkillBarView.Refresh(...)` displays:

- `Defend\nPlaceholder`
- `Potion\nPlaceholder`

`SkillBarView.DisablePlaceholderButtons()` enables only the supported indices:

- Move
- BasicAttack
- SkillSlot0
- SkillSlot1
- SkillSlot2

Defend and Potion remain disabled placeholder UI entries.

## 10. ActionPermissionService coverage

`ActionPermissionService` currently provides:

- `CanMove(unit, turnManager)`
- `CanAct(unit, turnManager)`

It is enough for the current player-side minimum action economy:

- Move uses `CanMove`.
- Basic Attack uses `CanAct`.
- Skill uses `CanAct`.
- Defend could use `CanAct`.
- Potion could use `CanAct`.

Current limitation:

- It is player-only.
- It requires `TurnPhase.PlayerTurn`.
- It rejects non-player factions.
- It does not support complete `EnemyTurn` action permission.
- It does not distinguish action types beyond move vs action.

Conclusion:

- It is enough for a minimal player action economy.
- It is not enough for a complete unified enemy turn gate.

## 11. Enemy AI action gate status

Enemy AI currently does not use one complete shared action gate.

`EnemyAIController` current behavior:

- If enemy is in basic attack range, it calls `CombatSystem.TryBasicAttack(enemy, target)`.
- If enemy is out of range, it moves with `UnitMover.TryMove(...)`.
- After movement, it manually calls `UnitTurnState.MarkMoved()`.

Current issue:

- Enemy movement bypasses `ActionPermissionService.CanMove(...)`.
- Enemy attack enters `CombatSystem.TryBasicAttack(...)`.
- If `CombatSystem` is bound to `TurnManager` and `ActionPermissionService`, enemy attack can be rejected because the current gate only allows player units during `PlayerTurn`.

Conclusion:

- Enemy AI is still a manual/test-style action runner.
- A future turn completion phase must decide how enemy actions pass through or bypass action permission safely.

## 12. Future Phase 14 stage recommendations

Recommended order:

1. Phase 14.14 - Turn System Completion Plan
2. Phase 14.15 - Turn System Completion Implementation
3. Phase 14.16 - Defend Action Plan
4. Phase 14.17 - Defend Action Implementation
5. Phase 14.18 - Potion Action Plan
6. Phase 14.19 - Potion Action Implementation
7. Phase 14.20 - Skill Rebuild Design
8. Phase 14.21 - Skill Rebuild Implementation
9. Phase 14 Final Regression

## 13. Turn System Completion candidate scope

Future minimum files may include:

- `Assets/_BoneThrone/Scripts/Turns/TurnManager.cs`
- `Assets/_BoneThrone/Scripts/Turns/ActionPermissionService.cs`
- `Assets/_BoneThrone/Scripts/Turns/UnitTurnState.cs`
- `Assets/_BoneThrone/Scripts/Skills/SkillSystem.cs`, only for cooldown tick integration.
- `Assets/_BoneThrone/Scripts/AI/EnemyAIController.cs`
- A new lightweight enemy turn runner, if needed.
- `BattleHUDController.cs` / `TurnBannerView.cs`, only if turn UI needs new display or intent wiring.

Future goals:

- Define actor turn lifecycle.
- Define cooldown tick timing.
- Preserve one move + one action per turn.
- Execute enemy turn reliably.
- Return to player turn safely.

## 14. Defend Action candidate scope

Future minimum files may include:

- New `DefendSystem.cs`.
- `UnitTurnState.cs` or a new lightweight `UnitDefenseState.cs`.
- `DamageResolver.cs`, only if unified damage reduction requires it.
- `CombatLog.cs`, for structured defend / reduction events.
- `SkillBarView.cs`.
- `BattleHUDController.cs`.
- Possibly `UIActionModeController.cs`, depending on final UI flow.

Design boundary:

- Defend consumes action.
- Defend provides simple reduction.
- No taunt AI control.
- No complex buff/status framework.
- UI must not directly modify HP, cooldown, acted, or moved state.

## 15. Potion Action candidate scope

Future minimum files may include:

- New `PotionSystem.cs` or `ItemActionSystem.cs`.
- `UnitRuntimeState.cs`, if a safe heal API is added.
- `UnitStats.cs`, for max HP access.
- `CombatLog.cs`, for structured potion/heal events.
- `SkillBarView.cs`.
- `BattleHUDController.cs`.
- Possibly `UIActionModeController.cs`, depending on final UI flow.

Design boundary:

- Potion consumes action.
- Potion provides simple healing.
- No full inventory or backpack system.
- UI must not directly heal or mark acted.

## 16. Skill Rebuild candidate scope

Future skill rebuild should depend on:

- `SkillData`
- `SkillRuntime`
- `SkillSystem`
- `SkillTargetingService`
- `SkillEffectExecutor`
- `SkillEffectResult`
- `DamageResolver`
- `CombatLog`
- `ActiveUnitProvider`
- `UnitRuntimeState`
- `UnitTurnState`

Future skill rebuild must avoid:

- Changing `DamageResolver` casually.
- Changing `SkillSystem.TryUseSkill` execution semantics casually.
- Changing `CombatSystem.TryBasicAttack` execution semantics casually.
- UI directly modifying gameplay state.
- Highlighter calls to `TryUseSkill` or `TryBasicAttack`.
- String parsing for structured CombatLog behavior.

Skill rebuild may introduce lightweight state only if a future design phase proves it is needed.

Do not introduce a complex buff/status framework unless separately approved.

## 17. Do-not-touch rules

Do not touch unless a future phase explicitly approves:

- KayKit original resources.
- `Skeleton_Rogue` name / identity.
- `Skeleton_Golem` as a normal enemy.
- Ranger gameplay identity. Ranger is not renamed to Rogue.
- Player Ranger Rogue visual.
- UI direct HP changes.
- UI direct cooldown changes.
- UI direct `MarkActed` / `MarkMoved`.
- Highlight preview calling `TryBasicAttack`.
- Highlight preview calling `TryUseSkill`.
- CombatLog string parsing.
- `DamageResolver` base semantics.
- `SkillSystem.TryUseSkill` base semantics.
- `CombatSystem.TryBasicAttack` base semantics.
- Formal SkillData field values, unless a future Skill Rebuild phase explicitly approves.
- Broad scene or prefab rewrites.

## 18. Risks

Current risks:

- Enemy AI and player-only action gate can conflict.
- Cooldown tick timing is undefined in formal turn flow.
- Defend must integrate with damage resolution carefully or reduction will be inconsistent.
- Potion must avoid direct UI state mutation.
- Combining turn completion, defend, potion, and skill rebuild in one implementation phase would make regressions hard to isolate.
- Skill rebuild may require lightweight state, but overbuilding a full status framework would exceed the current milestone.

## 19. Rollback guidance

This audit phase only adds this DevLog.

Rollback for this phase:

- Delete `Docs/DevLogs/Phase14.13E_CombatActionEconomyAudit.md`.

Future implementation rollback guidance:

- Keep Turn / Defend / Potion / Skill Rebuild in separate phases.
- Revert only the files changed by the failing phase.
- Do not bundle unrelated fixes into the same implementation pass.
- Record observed failures first, then open a focused correction phase.

## Final conclusion

Phase 14 is not complete yet.

Completed in recent Phase 14 work:

- GridTest camera controls.
- ActiveUnitProvider.
- 12 formal player SkillData.
- Player prefab SkillRuntime Slot 0 / 1 / 2 wiring.
- UI Slot 0 / 1 / 2 targeting.
- Simplified skill effect branches.

Still open:

- Turn System Completion.
- Defend Action.
- Potion Action.
- Skill Rebuild Design.
- Skill Rebuild Implementation.
- Phase 14 Final Regression.
