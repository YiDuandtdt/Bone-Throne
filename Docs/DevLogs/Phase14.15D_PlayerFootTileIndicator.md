# Phase 14.15-D - Player Foot Tile Indicator

## Scope

Phase 14.15-D adds a lightweight white tile marker under alive player units so their current positions remain readable during the local single-player tactics loop.

This is implemented inside the existing debug tile highlighter. It does not modify scenes, prefabs, material assets, KayKit assets, combat, skills, turn rules, camera controls, or `ActiveUnitProvider`.

## Files changed

- `Assets/_BoneThrone/Scripts/Movement/MovementDebugHighlighter.cs`
- `Assets/_BoneThrone/Scripts/Movement/PlayerMovementController.cs`
- `Docs/ACTIVE_TASK.md`
- `Docs/DevLogs/Phase14.15D_PlayerFootTileIndicator.md`

No `BattleHUDController` or `TurnManager` changes were needed.

## Player foot tile rule

Alive player units now mark their current tile white by default.

The marker rules:

- player faction only
- active in hierarchy
- alive only
- requires `Unit.CurrentTile`
- enemy units are ignored
- dead players are ignored

The marker is a runtime tile color overlay. It does not alter material assets or prefab data.

## Highlighter layering

`MovementDebugHighlighter` now paints in layers:

1. restore remembered original map colors
2. apply player foot tile white markers
3. apply selected tile color
4. apply move / attack / skill action highlight color

This means selected, move, attack, and skill highlights can temporarily override white player foot tiles. When highlights clear, the player foot tile layer is repainted as white.

`originalColors` continues to store the map's original color, not the white marker color.

## Added highlighter API

`MovementDebugHighlighter` now includes:

- `RefreshPlayerFootTiles()`
- `ClearPlayerFootTiles()`
- `ReapplyPlayerFootTiles()`

It also exposes serialized fields:

- `showPlayerFootTiles`
- `playerFootTileColor`
- `autoRefreshPlayerFootTiles`
- `playerFootTileRefreshInterval`

Auto-refresh defaults to a lightweight interval so death or activation changes can remove stale player foot markers without requiring scene or combat-system edits.

## Movement refresh

`PlayerMovementController` calls `RefreshPlayerFootTiles()` after a successful player move and before repainting selection.

This ensures:

- the old tile is removed from the player foot marker set
- the old tile returns to its original or current highlight baseline
- the new current tile becomes white
- the selected tile can still draw selected color on top

Movement algorithms, pathfinding, and `UnitMover` were not changed.

## End Turn and cleanup behavior

End Turn from Phase 14.15-C already calls `MovementDebugHighlighter.Clear()`.

Because `Clear()` now repaints the foot tile layer after restoring original colors, player current tiles return to white instead of staying blue, green, red, or yellow.

No `BattleHUDController` change was required for this phase.

## Explicit non-changes

No changes were made to:

- `DamageResolver.cs`
- `SkillEffectExecutor.cs`
- `SkillSystem.cs`
- `SkillTargetingService.cs`
- `CombatSystem.cs`
- SkillData assets
- Player prefabs
- enemy prefabs
- scene files, including `GridTest.unity`
- KayKit original assets
- `Skeleton_Rogue`
- `Skeleton_Golem`
- Ranger visual / identity
- Defend
- Potion
- skill formulas
- turn-order rules
- camera controls
- `ActiveUnitProvider` behavior
- networking
- initiative
- AP
- behavior trees

## Unity Play Mode test steps

1. Open `Assets/_BoneThrone/Scenes/GridTest.unity`.
2. Confirm there are no compile errors.
3. Enter Play Mode.
4. Confirm each active alive player current tile appears white by default.
5. Confirm enemy current tiles do not become white.
6. Select a player and confirm selected color can override the white tile.
7. Enter Move targeting and confirm green move range can override white where applicable.
8. Cancel Move targeting and confirm player current tiles return to white.
9. Enter Basic Attack targeting and confirm red attack highlights still work.
10. Cancel Basic Attack targeting and confirm player current tiles return to white.
11. Enter Skill Slot 0 / 1 / 2 targeting and confirm yellow skill highlights still work.
12. Cancel Skill targeting and confirm player current tiles return to white.
13. Move a player and confirm the old tile is no longer white and the new tile is white.
14. End Turn and confirm selection/highlights clear while alive player current tiles remain white.
15. Kill or deactivate a player if possible and confirm its tile no longer keeps the player foot marker after refresh.
16. Confirm Phase 14.15-C free player turn order still works.
17. Confirm End Turn, EnemyTurn, Move, Basic Attack, Skill Slot 0 / 1 / 2, camera controls, ActiveUnitProvider, Room / Key / Stairs / LevelUp still work.
18. Confirm Console has no new red errors.

## Risks

- The indicator is a debug tile color overlay, not a dedicated art asset or prefab ring.
- Auto-refresh scans active `Unit` components at a short interval. This is acceptable for current GridTest scale but should be replaced by event-driven markers in a larger content phase.
- White foot markers depend on tile renderers supporting `_BaseColor` or `_Color`, matching the existing highlighter assumptions.
- If a tile's intended original color is also white, the marker may be visually subtle.

## Rollback

To roll back Phase 14.15-D:

1. Revert `Assets/_BoneThrone/Scripts/Movement/MovementDebugHighlighter.cs`.
2. Revert `Assets/_BoneThrone/Scripts/Movement/PlayerMovementController.cs`.
3. Revert `Docs/ACTIVE_TASK.md`.
4. Delete `Docs/DevLogs/Phase14.15D_PlayerFootTileIndicator.md`.

No scene, prefab, SkillData, material asset, KayKit, combat, skill, camera, ActiveUnitProvider, or turn-rule rollback is needed because those systems were not changed.
