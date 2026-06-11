# Phase 14.5 Inspector Binding Checklist

## 1. Purpose and scope

This document records the Inspector binding checks needed for the current Bone Throne Unity 6.3 LTS project.

Phase 14.5 is documentation-only. It only records what to inspect and how to interpret likely binding failures. It does not implement fixes, write gameplay code, modify scenes, modify prefabs, modify ScriptableObject assets, modify KayKit original resources, or change `Assets`, `Packages`, or `ProjectSettings`.

If a missing or incorrect binding is found, record it as a finding. Do not fix it in Phase 14.5.

## 2. Source of truth

Use these references:

- `AGENTS.md`
- `Docs/Phase14_ProjectStateAudit.md`
- `Docs/Phase14_StabilizationPlan.md`
- `Docs/骸骨王座_系统设计文档_Unity6.3LTS_v2.2_CurrentProjectState.md`
- `Docs/骸骨王座_Codex完整即用Vibecoding开发文档_Unity6.3LTS_v1.2_CurrentProjectState.md`
- `Docs/DevLogs/`

If any older Markdown displays encoding corruption in the current terminal, do not copy corrupted text. Reconstruct the fact from the Phase 14.1 audit result, v2.2 current-state design document, v1.2 Vibecoding document, Phase 14.4 stabilization plan, and DevLogs.

## 3. GridTest scene rule

- `Assets/_BoneThrone/Scenes/GridTest.unity` is the current only real integrated validation scene.
- `Assets/_BoneThrone/Scenes/MainMenu.unity` remains a placeholder.
- Do not treat this checklist as formal three-floor scene acceptance.
- There are no formal `Level_01`, `Level_02`, or `Level_03` scenes yet.
- Do not modify `GridTest.unity` in this phase. This document only describes what should be inspected later in Unity.

## 4. Grid bindings

### `GridManager.initialTiles`

Check:

- The array/list exists on the scene `GridManager`.
- It contains the intended `GridTest.unity` tile objects.
- Entries are not null.
- Tile coordinates are unique and coherent.
- Walkable / occupied tile state can be inspected without changing scene data.

Why it matters:

- Movement range uses grid lookup.
- A* pathfinding uses grid lookup.
- Highlights depend on valid tile references.
- Unit placement and occupancy depend on registered tiles.

Likely failure symptoms:

- Movement range is empty or incomplete.
- Selected unit cannot move to expected tiles.
- A* path fails even when a route is visibly clear.
- Units overlap or fail to occupy/release tiles correctly.

Phase 14.5 action allowed:

- Record missing, null, duplicate, or suspicious entries.
- Do not repair the array in this phase.

## 5. Player / Turn / Level bindings

### `BattleHUDController.playerUnits`

Check:

- Four slots are assigned for Fighter, Ranger, Mage, and Barbarian.
- Slots are in the intended display order.
- Entries are living player unit instances from `GridTest.unity`, not prefab assets.
- Entries are not null unless intentionally documenting a missing binding.

Why it matters:

- HeroPanel displays HP, Level, moved, acted, and alive/dead state.
- HUD refresh can appear broken if player units are missing.

### `TurnManager.playerUnits`

Check:

- Player unit references are assigned.
- References match the same scene unit instances used by the HUD and progression service.
- If fixed role order is enabled in a future test, the order is consistent with Fighter -> Ranger -> Mage -> Barbarian.

Why it matters:

- Turn state, moved state, acted state, and future role gates depend on correct player references.
- Enemy AI / turn gate issues can be hard to interpret if player unit references are incomplete.

### `LevelProgressionService.playerUnits`

Check:

- All four player unit instances are assigned.
- Entries match the actual scene players.
- Dead/living state should be observed in Play Mode, not edited here.

Why it matters:

- Placeholder LevelUp refreshes living player Level, MaxHP, and HP through this list.
- Missing entries can make LevelUp appear partially broken.

Phase 14.5 action allowed:

- Record missing or mismatched player references.
- Do not bind or reorder references in this phase.

## 6. UI / HUD bindings

### `BattleHUDController.enemyUnits`

Check:

- All ordinary scene enemy unit instances are assigned.
- Entries point to `Skeleton_Minion`, `Skeleton_Warrior`, `Skeleton_Mage`, and `Skeleton_Rogue` instances when those are present in `GridTest.unity`.
- Entries do not include `Skeleton_Golem`.
- Null entries are recorded as checklist findings.

Why it matters:

- HUD and action targeting can miss enemies if this list is stale.

### `UIActionModeController.enemyUnits`

Check:

- The action mode controller has the same ordinary enemy scene instances expected for targeting.
- Basic Attack red highlight and Skill Slot 0 yellow highlight depend on this list.
- Dead or inactive enemies should be skipped at runtime, but missing live enemies should be recorded.

Why it matters:

- Red highlight can miss valid Basic Attack targets.
- Yellow highlight can miss valid Skill Slot 0 targets.
- The underlying gameplay service can still be correct even if preview is wrong.

Phase 14.5 action allowed:

- Record missing enemy references.
- Do not fix `enemyUnits` in this phase.

## 7. Combat / Skill bindings

### `SkillEffectExecutor.knownUnits`

Check:

- The list includes units needed by skill effects that scan nearby units.
- Mage Fireball splash depends on this list.
- The list should include adjacent valid enemy targets for the Fireball regression setup.
- Null, inactive, duplicate, or stale entries should be recorded.

Why it matters:

- Fireball primary target can work while splash silently misses adjacent targets.
- Missing `knownUnits` is a binding problem, not necessarily a formula problem.

Phase 14.5 action allowed:

- Record whether `knownUnits` appears complete for current `GridTest.unity` enemies and players.
- Do not modify `SkillEffectExecutor`.
- Do not modify skill formulas.
- Do not modify `DamageResolver`.

## 8. Room activation bindings

### `RoomEnemyActivator.assignedEnemies`

Check:

- The room activator contains the enemies that should activate when the room is entered.
- Entries are scene instances, not prefab assets.
- The list is consistent with room clear expectations.

Why it matters:

- Missing assigned enemies can prevent activation.
- Room clear can be wrong if the activator does not know all enemies.

### `RoomEnemyActivator.spawnTiles`

Check:

- Spawn tiles are assigned.
- Spawn tile count and order match `assignedEnemies` expectations.
- Spawn tiles are valid tiles in `GridManager.initialTiles`.
- Spawn tiles are not obviously occupied before activation unless that is intentional for the test.

Why it matters:

- Enemies can fail placement or appear in invalid positions.
- Room entry can look broken even if activation code runs.

Phase 14.5 action allowed:

- Record missing, mismatched, or suspicious enemy/tile pairs.
- Do not edit room objects, enemies, or tile assignments in this phase.

## 9. Key / Stairs / LevelUp bindings

### `LevelProgressionService.requiredClearedRooms`

Check:

- Required room references are assigned.
- The list reflects the current `GridTest.unity` placeholder progression requirements.
- It is not treated as formal three-floor level data.

Why it matters:

- Stairs can unlock too early or stay locked after room clear.

### `KeyItem.progressionService`

Check:

- The Key scene instance references the scene `LevelProgressionService`.
- The reference is not a prefab asset reference.

Why it matters:

- Key pickup cannot update shared progression state if this is missing.

### `InteractableStairs.progressionService`

Check:

- The Stairs scene instance references the scene `LevelProgressionService`.

Why it matters:

- Stairs cannot read key/room conditions or trigger placeholder progression if this is missing.

### `InteractableStairs.selectionManager`

Check:

- The Stairs scene instance references the scene `SelectionManager`.

Why it matters:

- Stairs interaction can fail to validate the selected unit context.

Phase 14.5 action allowed:

- Record missing progression or selection references.
- Do not bind or modify Key / Stairs / LevelProgressionService in this phase.

## 10. Enemy Floating HP Bar checklist

Check on ordinary enemies only:

- `Skeleton_Minion`
- `Skeleton_Warrior`
- `Skeleton_Mage`
- `Skeleton_Rogue`

Required checks:

- Unit reference: HP bar should read the correct root `Unit`, either by explicit reference or runtime parent fallback.
- HP refresh: fill should update after Basic Attack, Skill Slot 0, and Fireball splash damage.
- Death hiding: HP bar should hide when the enemy dies.
- Raycast disabled: HP bar graphics should not block targeting or selection raycasts.
- Prefab / scene override risk: scene instances should not override prefab HP bar fields into stale or null references.

Phase 14.5 action allowed:

- Record missing bars, stale Unit references, refresh issues, death-hide issues, or raycast concerns.
- Do not edit enemy prefabs, HP bar prefab, or scene instances in this phase.

## 11. Visual do-not-touch checks

These checks protect current art/prefab rules:

- `Skeleton_Rogue` is the ordinary enemy. Do not rename it.
- `Skeleton_Golem` is reserved for a future Boss / heavy Boss. Do not use it as an ordinary enemy.
- Player Ranger uses Adventurers Rogue visual. Do not change it back to Ranger visual.
- Do not modify KayKit original resources.

Phase 14.5 action allowed:

- Record any deviation as a finding.
- Do not repair visual or prefab issues in this phase.

## 12. Failure symptom to likely binding cause

| Failure symptom | Likely binding cause | Phase 14.5 action |
| --- | --- | --- |
| Fireball splash does not hit adjacent enemies. | `SkillEffectExecutor.knownUnits` is missing adjacent targets, has stale references, or has inactive/null entries. | Record finding only. |
| Red Basic Attack highlight misses an enemy. | `UIActionModeController.enemyUnits` or `BattleHUDController.enemyUnits` is missing that enemy. | Record finding only. |
| Yellow Skill Slot 0 highlight misses an enemy. | `UIActionModeController.enemyUnits` is missing that enemy, or the enemy is inactive/dead/untiled in Play Mode. | Record finding only. |
| HeroPanel does not refresh a player. | `BattleHUDController.playerUnits` is missing or mismatched. | Record finding only. |
| Turn state appears inconsistent for a player. | `TurnManager.playerUnits` may be incomplete or out of sync with scene players. | Record finding only. |
| LevelUp does not affect all living players. | `LevelProgressionService.playerUnits` is missing one or more players. | Record finding only. |
| Room enter does not activate enemies. | `RoomEnemyActivator.assignedEnemies` or `spawnTiles` is missing, mismatched, or points to invalid scene objects. | Record finding only. |
| Room clear never completes. | `assignedEnemies` omits active enemies or includes stale/non-room enemies. | Record finding only. |
| Key pickup has no effect. | `KeyItem.progressionService` is missing or points to the wrong service. | Record finding only. |
| Stairs does not respond. | `InteractableStairs.progressionService` or `selectionManager` is missing. | Record finding only. |
| Stairs progression condition is wrong. | `LevelProgressionService.requiredClearedRooms` is missing or mismatched. | Record finding only. |
| Movement/pathing fails across visible grid. | `GridManager.initialTiles` may be incomplete, duplicated, or include invalid tiles. | Record finding only. |
| HP bar does not refresh or hide. | `EnemyFloatingHealthBar` Unit reference is stale, missing, or scene override broke prefab defaults. | Record finding only. |

## 13. Inspector-only validation workflow

Workflow:

1. Open Unity 6.3 LTS.
2. Open `Assets/_BoneThrone/Scenes/GridTest.unity`.
3. Do not enter Play Mode yet.
4. Inspect each binding listed in this document.
5. Record missing, null, stale, duplicate, or suspicious references.
6. Do not modify Inspector values in Phase 14.5.
7. If a missing binding is found, add it to the known failure notes for the next approved phase.
8. Future fixes must happen in a separate phase with explicit user approval.

This phase only checks and records.

## 14. Phase 14.5 non-fix rule

Do not fix findings in Phase 14.5, including:

- Missing Inspector bindings.
- Missing Fireball splash targets.
- Missing UI target highlights.
- Enemy AI turn gate rejection.
- Enemy HP Bar reference, refresh, or death-hide issues.
- Room activation or clear issues.
- Key / Stairs / LevelUp binding issues.
- CombatLog display or structured-entry issues.

Phase 14.6 is also only `Minimal Stabilization Implementation Proposal Only`. Actual implementation requires a later separate phase and explicit user approval.

## 15. Git diff validation

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

To roll back this checklist, remove or revert:

- `Docs/Phase14_InspectorBindingChecklist.md`

Do not use broad reset commands if other Phase 14 documentation work is still in progress.
