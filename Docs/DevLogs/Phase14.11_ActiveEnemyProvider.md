# Phase 14.11 - Active Enemy Provider and Auto Enemy Collection

Date: 2026-05-28

## Scope

Phase 14.11 adds a lightweight read-only active unit provider for the current GridTest single-player integration flow.

Goal:

- Reduce manual `enemyUnits` / `knownUnits` dependency.
- Preserve existing manual arrays as fallback.
- Keep UI as an intent / preview layer.
- Keep combat and skill execution semantics unchanged.

## Files changed

- Added `Assets/_BoneThrone/Scripts/Units/ActiveUnitProvider.cs`.
- Added `Assets/_BoneThrone/Scripts/Units/ActiveUnitProvider.cs.meta`.
- Modified `Assets/_BoneThrone/Scripts/UI/BattleHUDController.cs`.
- Modified `Assets/_BoneThrone/Scripts/UI/UIActionModeController.cs`.
- Modified `Assets/_BoneThrone/Scripts/Skills/SkillEffectExecutor.cs`.
- Added `Docs/DevLogs/Phase14.11_ActiveEnemyProvider.md`.

`Assets/_BoneThrone/Scenes/GridTest.unity` was not modified. `BattleHUDController` now resolves an existing scene provider or adds a read-only provider component to its own GameObject at runtime if none is bound.

## Provider responsibilities

`ActiveUnitProvider` lives in namespace `BoneThrone.Units`.

It only:

- Reads current scene `Unit` components.
- Returns active alive enemies.
- Returns active alive units.
- Fills caller-provided lists.

It does not:

- Modify Unit state.
- Place units.
- Activate enemies.
- Drive turns.
- Drive AI.
- Apply damage.
- Execute skills.
- Change skill formulas.
- Special-case names such as `Skeleton_Rogue` or `Skeleton_Golem`.

## Provider API

- `Unit[] GetActiveAliveEnemies()`
- `Unit[] GetActiveAliveUnits()`
- `void FillActiveAliveEnemies(List<Unit> results)`
- `void FillActiveAliveUnits(List<Unit> results)`

Collection uses:

- `Object.FindObjectsByType<Unit>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)`

Filter rules:

- `unit != null`
- `unit.gameObject.activeInHierarchy`
- `unit.IsAlive`
- Enemy collection additionally requires `unit.Faction == UnitFaction.Enemy`

## Systems connected to provider

### BattleHUDController

- Added serialized `ActiveUnitProvider activeUnitProvider`.
- Keeps existing serialized `enemyUnits`.
- Resolves provider in `Awake`.
- Passes provider into `UIActionModeController.Configure(...)`.
- If no provider is bound and none exists in scene, adds `ActiveUnitProvider` to the BattleHUD GameObject at runtime.
- Does not directly change HP, cooldown, acted, moved, combat, skill, room, or level state.

### UIActionModeController

- Added serialized `ActiveUnitProvider activeUnitProvider`.
- Basic Attack and Skill target highlights now call `GetEnemyTargetsForPreview()`.
- If provider exists and returns non-empty active alive enemies, previews use provider results.
- If provider is missing or returns empty, previews fall back to serialized `enemyUnits`.
- Basic Attack preview still uses `CombatSystem.CanBasicAttack(...)`.
- Skill preview still uses `SkillSystem.CanUseSkillOnTarget(...)`.
- Highlight preview still does not call `TryBasicAttack`.
- Highlight preview still does not call `TryUseSkill`.
- Click execution logic was not changed.
- Right-click cancel targeting semantics were not changed.

### SkillEffectExecutor

- Added serialized `ActiveUnitProvider activeUnitProvider`.
- Mage Fireball branch now passes provider active alive units when available.
- If provider is missing or returns empty, it falls back to serialized `knownUnits`.
- `MageSkillEffects` was not modified.
- Skill damage formulas were not modified.
- `DamageResolver` was not modified.
- `SkillEffectResult` structured output was not modified.

## Systems not modified

No changes were made to:

- `CombatSystem`
- `SkillSystem`
- `DamageResolver`
- `MageSkillEffects`
- `CombatSystem.TryBasicAttack`
- `SkillSystem.TryUseSkill`
- Unit / Enemy / Room / Level runtime logic
- Player prefabs
- Enemy prefabs
- `SkillData` assets
- KayKit original resources
- `Skeleton_Rogue`
- `Skeleton_Golem`
- Ranger visual
- Phase 14.10 camera controls

## Inspector / scene binding notes

Manual binding remains supported:

- `BattleHUDController.activeUnitProvider`
- `UIActionModeController.activeUnitProvider`
- `SkillEffectExecutor.activeUnitProvider`

Current implementation does not require a `GridTest.unity` scene diff:

- If a provider is already present in scene, consumers can resolve it.
- If BattleHUD has no provider reference and no scene provider exists, BattleHUD adds a read-only provider component at runtime.
- Existing `enemyUnits` and `knownUnits` arrays remain fallback data.

Future optional scene setup:

- Add one `ActiveUnitProvider` scene object.
- Bind it to BattleHUD, UIActionModeController, and SkillEffectExecutor.
- Keep existing manual arrays as fallback.
- Do not modify player/enemy prefabs or `SkillData`.

## Play Mode test steps

1. Open `Assets/_BoneThrone/Scenes/GridTest.unity`.
2. Confirm there are no compile errors.
3. Enter Play Mode.
4. Enter Basic Attack mode.
5. Confirm red highlights include active alive enemies.
6. Enter Skill Slot 0 mode.
7. Confirm yellow highlights include active alive enemies.
8. Kill an enemy.
9. Confirm the dead enemy is not treated as an active preview target.
10. Use Mage Fireball.
11. Confirm primary damage remains unchanged.
12. Confirm adjacent valid enemy splash still deals the existing splash damage.
13. Confirm CombatLog structured entries still appear.
14. Trigger room enemy activation.
15. Confirm newly active enemies become available for preview.
16. Confirm Enemy Floating HP Bar still refreshes and hides on death.
17. Confirm Room / Key / Stairs / LevelUp still work.
18. Confirm Enemy AI turn still works.
19. Confirm Phase 14.10 camera controls still work:
    - middle mouse pan
    - mouse wheel zoom
    - right mouse yaw / pitch rotation
20. Confirm Console has no new red errors.

## Risks

- Provider collection allocates arrays/lists and is acceptable for current GridTest scale, but is not a final large-scale content system.
- Provider can expose active enemies that stale manual arrays missed, so highlight coverage may change by becoming more complete.
- If all enemies are dead, provider returns empty and the fallback array is still consulted; preview filtering still checks alive/active state.
- Runtime provider auto-add avoids scene YAML risk, but a future explicit scene provider object may be clearer for Inspector debugging.

## Rollback

To roll back this phase:

- Remove `Assets/_BoneThrone/Scripts/Units/ActiveUnitProvider.cs`.
- Remove `Assets/_BoneThrone/Scripts/Units/ActiveUnitProvider.cs.meta`.
- Revert provider field and runtime resolution in `BattleHUDController`.
- Revert provider field and preview target source in `UIActionModeController`.
- Revert provider field and known-unit source in `SkillEffectExecutor`.
- Remove `Docs/DevLogs/Phase14.11_ActiveEnemyProvider.md`.

Existing `enemyUnits` and `knownUnits` arrays remain available as fallback and should restore the previous behavior after rollback.
