# Phase 14.4 Stabilization Plan

## 1. Purpose and scope

This phase only defines the stabilization plan for the current Bone Throne Unity 6.3 LTS project.

It does not implement fixes. It does not write gameplay code. It does not modify scenes, prefabs, ScriptableObject assets, KayKit original resources, `Assets`, `Packages`, or `ProjectSettings`.

The purpose is to decide what must be verified, documented, or proposed before future feature work such as Boss, formal levels, data assetization, or LAN multiplayer.

## 2. Source of truth

Use these references before any future stabilization work:

- `AGENTS.md`
- `Docs/Phase14_ProjectStateAudit.md`
- `Docs/骸骨王座_系统设计文档_Unity6.3LTS_v2.2_CurrentProjectState.md`
- `Docs/骸骨王座_Codex完整即用Vibecoding开发文档_Unity6.3LTS_v1.2_CurrentProjectState.md`
- `Docs/DevLogs/`

If any old Markdown displays terminal encoding corruption, do not copy corrupted text. Reconstruct the fact from the Phase 14.1 audit result, v2.2 current-state design document, v1.2 Vibecoding document, and DevLogs.

## 3. Current stabilization priorities

Highest-priority stabilization areas:

- Inspector manual binding dependencies.
- UI action targeting.
- Fireball splash.
- CombatLog.
- Enemy Floating HP Bar.
- Enemy AI / turn gate.
- Room activation.
- Key / Stairs / LevelUp.
- `GridTest.unity` regression.

Current scene rule:

- `Assets/_BoneThrone/Scenes/GridTest.unity` is the current only real integrated validation scene.
- `Assets/_BoneThrone/Scenes/MainMenu.unity` remains a placeholder.
- There are no formal `Level_01`, `Level_02`, or `Level_03` scenes yet.

## 4. Manual Inspector binding risks

These bindings are currently high-risk because several systems still depend on manually assigned scene references or prefab-instance overrides.

| Binding | Risk | Impact | Validation |
| --- | --- | --- | --- |
| `SkillEffectExecutor.knownUnits` | Missing or stale enemy/player references. | Fireball splash may only damage the primary target and skip adjacent valid targets. | In `GridTest.unity`, use Mage Fireball on a target with adjacent enemies and confirm splash rows appear in CombatLog. |
| `BattleHUDController.enemyUnits` | HUD does not know current enemies. | Enemy display or downstream action setup can miss targets. | Enter Play Mode and verify UI targeting can see all ordinary enemies currently present. |
| `UIActionModeController.enemyUnits` | Target preview traverses an incomplete enemy list. | Red/yellow target highlights can be missing even when attacks or skills would be legal. | Enter Basic Attack and Skill Slot 0 targeting, confirm all valid enemy tiles highlight. |
| `BattleHUDController.playerUnits` | HUD hero panels have missing or wrong actors. | HP, level, moved, acted, and alive/dead display can be incomplete. | Confirm four hero panels map to Fighter, Ranger, Mage, Barbarian. |
| `TurnManager.playerUnits` | Turn ordering and player turn state references can be incomplete. | Role order, free selection, moved/acted reset, or future turn gates may behave inconsistently. | Confirm all four player units can be selected and their moved/acted state updates. |
| `LevelProgressionService.playerUnits` | LevelUp cannot reach the full party. | Some living players may not gain level, MaxHP, or HP refill. | Trigger placeholder level progression and verify all living players refresh level and HP. |
| `RoomEnemyActivator.assignedEnemies` | Room clear and activation list can be incomplete. | Enemies may remain inactive, missing from clear checks, or excluded from combat. | Enter the room and confirm all assigned enemies activate and participate. |
| `RoomEnemyActivator.spawnTiles` | Enemy placement targets can be missing or mismatched. | Activated enemies may fail placement or overlap incorrectly. | Enter the room and confirm enemies are placed on intended tiles without occupancy conflicts. |
| `LevelProgressionService.requiredClearedRooms` | Stairs requirements can be wrong. | Player may transition early or remain blocked after clearing the room. | Try stairs before and after clearing the required room. |
| `GridManager.initialTiles` | Grid lookup can be incomplete. | Movement range, A* pathfinding, highlighting, occupancy, and placement can fail. | Enter Play Mode and test movement range/pathing across expected `GridTest` tiles. |
| `KeyItem.progressionService` | Key pickup cannot update shared progression state. | Stairs may never unlock after pickup. | Pick up Key and confirm progression prompt/state changes. |
| `InteractableStairs.progressionService` | Stairs cannot read key/room conditions or trigger progression. | Stairs interaction may do nothing or show wrong prompt. | Click stairs before/after meeting conditions and confirm expected prompt/progression. |
| `InteractableStairs.selectionManager` | Stairs cannot validate current selected unit. | Stairs interaction can ignore selected actor context. | Select a living player and perform the stairs two-click interaction. |

## 5. UI / highlight / action safety risks

The current UI must remain an intent layer, not a gameplay authority layer.

Required rules:

- UI does not directly subtract HP.
- UI does not directly change cooldown.
- UI does not directly call `MarkActed`.
- UI does not directly call `MarkMoved`.
- Highlight preview must not call `CombatSystem.TryBasicAttack`.
- Highlight preview must not call `SkillSystem.TryUseSkill`.
- Move / Attack / Skill actions must go through existing gameplay services.

Current intended flow:

- Move: UI intent -> `UIActionModeController` -> existing movement controller / gameplay service.
- Basic Attack: UI intent -> `UIActionModeController` -> `CombatSystem.TryBasicAttack`.
- Skill Slot 0: UI intent -> `UIActionModeController` -> `SkillSystem.TryUseSkill`.

Stabilization risk:

- Future UI polish could accidentally move gameplay mutation into UI scripts.
- Future highlight work could accidentally use execution APIs for preview.
- Targeting arrays can make the UI appear broken even when the underlying gameplay service is correct.

## 6. Combat / Skill / CombatLog risks

Current safety rules:

- Do not casually modify `DamageResolver`.
- Do not casually modify `SkillEffectExecutor` skill formulas.
- `CombatLog` is structured event output. Do not rebuild it by parsing display strings.
- Fireball splash uses structured `SkillEffectResult`.
- Missing `knownUnits` can make Fireball splash fail even when the core skill formula is unchanged.

Specific risks:

- Formula drift: small changes to `DamageResolver` or `SkillEffectExecutor` can silently change combat balance.
- Logging regression: parsing strings would make Fireball splash and future multi-hit skills fragile.
- Data dependency regression: Fireball splash is structurally correct but can still fail if `knownUnits` does not include adjacent units.

Validation should confirm:

- Basic Attack still reports D20, hit/miss, damage, HP, and death through CombatLog.
- Skill Slot 0 still reports skill use, skill damage, cooldown, and death through CombatLog.
- Fireball splash creates separate structured damage rows for splash targets.

## 7. Enemy AI / turn gate risks

Current state:

- Enemy AI calls the same `CombatSystem` and movement services used by player-facing gameplay.
- `TurnManager` and `ActionPermissionService` can gate actions by phase, role, moved state, and acted state.

Risk:

- If `TurnManager` / `ActionPermissionService` is bound or configured as a player-only gate, enemy movement or enemy attacks can be rejected even when the AI decision is valid.

Required rule:

- This issue must be reproduced in Unity Play Mode before deciding whether any future code fix is needed.

Validation should confirm:

- Enemy phase starts.
- Enemy AI can select a living player target.
- Enemy AI can move toward the target when out of range.
- Enemy AI can use `CombatSystem.TryBasicAttack` when in range.
- Rejected enemy actions produce understandable logs or observable reasons before any implementation proposal is made.

## 8. Enemy Floating HP Bar risks

Current risk areas:

- `Unit` reference.
- HP refresh.
- Death hiding.
- Raycast disabled behavior.
- Prefab / scene override state.

Expected stable behavior:

- Each ordinary enemy prefab has an `EnemyFloatingHealthBar` child.
- The HP bar reads the correct root `Unit`.
- Runtime fallback can recover the parent `Unit` if manual reference is stale.
- Fill refreshes when HP changes.
- The bar hides when the enemy dies.
- UI graphics on the world-space bar do not block gameplay raycasts.

Validation should cover:

- `Skeleton_Minion`, `Skeleton_Warrior`, `Skeleton_Mage`, and `Skeleton_Rogue` show HP bars.
- Basic Attack and Skill Slot 0 update HP bar fill.
- Fireball splash updates affected splash target HP bars.
- Dead enemies hide their HP bars.
- Clicking enemies still selects or targets them correctly, without the HP bar blocking raycasts.
- Scene overrides do not disconnect prefab HP bar fields.

## 9. Room / Key / Stairs / LevelUp risks

Room risks:

- `assignedEnemies` may omit enemies required for activation or clear checks.
- `spawnTiles` may be missing or mismatched with assigned enemies.
- Room shadow is a simple active/inactive overlay, not a full fog-of-war system.
- Room clear depends on assigned enemy state and can be wrong if enemies are unassigned, inactive, or dead before activation.

Key / Stairs / LevelUp risks:

- Key pickup depends on `KeyItem.progressionService`.
- Stairs interaction depends on `InteractableStairs.progressionService` and `InteractableStairs.selectionManager`.
- Stairs currently use prompt / two-click placeholder progression, not a final modal.
- Level progression is placeholder root/index progression, not true three-floor scene loading.
- LevelUp must refresh living player Level, MaxHP, and HP.

Validation should confirm:

- Room trigger enters the room and updates room state.
- Room shadow hides or reveals at the expected time.
- Assigned enemies activate and place on spawn tiles.
- Room clear condition changes only after the assigned enemies are defeated or otherwise cleared.
- Key pickup updates shared key state.
- Stairs reject progression before required key/room conditions.
- Stairs accept progression after requirements and perform placeholder level advance.
- LevelUp refreshes living player level and HP.

## 10. Play Mode reproduction requirements

These issues must be reproduced in Unity Play Mode before any future fix is proposed as implementation:

- Enemy AI action rejection caused by `TurnManager` / `ActionPermissionService` gates.
- Fireball splash missing secondary targets.
- Basic Attack red highlight missing valid enemies.
- Skill Slot 0 yellow highlight missing valid enemies.
- CombatLog missing Fireball splash structured rows.
- Enemy Floating HP Bar not refreshing HP.
- Enemy Floating HP Bar not hiding on death.
- Enemy Floating HP Bar blocking raycasts.
- Room enemy activation failing on enter.
- Room clear condition failing after enemies die.
- Key pickup not updating progression.
- Stairs two-click interaction failing.
- LevelUp not refreshing Level / MaxHP / HP.
- Any issue that appears only in `GridTest.unity` scene wiring and cannot be proven from documentation alone.

## 11. Inspector-only checklist candidates

These should be checked through Inspector binding documentation and manual scene review before considering code changes:

- `SkillEffectExecutor.knownUnits`.
- `BattleHUDController.enemyUnits`.
- `UIActionModeController.enemyUnits`.
- `BattleHUDController.playerUnits`.
- `TurnManager.playerUnits`.
- `LevelProgressionService.playerUnits`.
- `RoomEnemyActivator.assignedEnemies`.
- `RoomEnemyActivator.spawnTiles`.
- `LevelProgressionService.requiredClearedRooms`.
- `GridManager.initialTiles`.
- `KeyItem.progressionService`.
- `InteractableStairs.progressionService`.
- `InteractableStairs.selectionManager`.

Likely Inspector-only symptoms:

- Missing Fireball splash because `knownUnits` is incomplete.
- Missing red/yellow target highlights because `enemyUnits` is incomplete.
- Missing hero panel data because `playerUnits` is incomplete.
- Stairs not reacting because progression or selection references are unbound.
- Room activation missing enemies because `assignedEnemies` or `spawnTiles` are incomplete.
- Movement/pathing failures caused by incomplete `initialTiles`.

## 12. Future implementation candidates

These are not Phase 14.4 implementation tasks.

They require a later separate phase and explicit user approval. Phase 14.6 is currently defined as `Minimal Stabilization Implementation Proposal Only`, so even Phase 14.6 must not implement code by default.

Future candidates:

- Replace or supplement `knownUnits` / `enemyUnits` manual arrays with a safe runtime query service.
- Add validation diagnostics for missing Inspector bindings.
- Add a formal EnemyTurn scheduler if Play Mode proves current enemy flow is unstable.
- Clarify and connect cooldown ticking to a reliable turn-end flow.
- Add read-only target query helpers where missing, while preserving execution API semantics.
- Add room activation diagnostics for assigned enemy / spawn tile mismatch.
- Add safer LevelProgression diagnostics for key, room, stairs, and level-up state.
- Add stronger Enemy Floating HP Bar fallback diagnostics for stale Unit references.
- Eventually replace placeholder level progression with formal Level_01 / Level_02 / Level_03 scene flow.

## 13. Do-not-touch list

Do not modify these unless a future phase explicitly approves the exact change:

- `DamageResolver`.
- `SkillEffectExecutor` skill formulas.
- `CombatSystem.TryBasicAttack` execution semantics.
- `SkillSystem.TryUseSkill` execution semantics.
- `CombatLog` structured event model.
- KayKit original resources.
- `Skeleton_Rogue` ordinary enemy identity and name.
- `Skeleton_Golem`, reserved for future Boss / heavy Boss only.
- Player Ranger Adventurers Rogue visual.
- `GridTest.unity`, unless a future phase explicitly approves scene modification.
- `Assets`, `Packages`, and `ProjectSettings` during documentation-only phases.

## 14. Recommended Phase 14.5 / 14.6 / 14.7 order

Recommended next order:

1. Phase 14.5 - Inspector Binding and Regression Checklist.
2. Phase 14.6 - Minimal Stabilization Implementation Proposal Only.
3. Phase 14.7 - Pre-Feature Regression Audit.

Phase 14.5 should create the concrete Inspector binding checklist and regression checklist.

Phase 14.6 should only propose minimal implementation candidates. It must not write code or modify files by default.

Phase 14.7 should audit readiness before future feature work such as Boss, formal levels, data assets, or LAN.

## 15. Git diff validation

Phase 14.4 allows Docs-only changes.

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

## 16. Rollback

To roll back Phase 14.4 documentation only, remove or revert:

- `Docs/Phase14_StabilizationPlan.md`
- `Docs/DevLogs/Phase14.4_StabilizationPlan.md`

If `Docs/ACTIVE_TASK.md` is modified in a future documentation update, revert only the Phase 14.4 text changes manually. Do not use broad reset commands if other Phase 14 documentation changes are still in progress.
