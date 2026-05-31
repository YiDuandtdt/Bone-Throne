# Phase 14.19-B - Ended Player Foot Tile Grey Indicator

## Goal
Extend the existing player foot tile marker so alive player units that have ended their turn use a grey baseline tile, while alive player units that can still act keep the white baseline tile.

## Actual Modified Files
- `Assets/_BoneThrone/Scripts/Movement/MovementDebugHighlighter.cs`
- `Docs/DevLogs/Phase14.19B_EndedPlayerFootTileGreyIndicator.md`
- `Docs/ACTIVE_TASK.md`

## Rules
- Alive player with `UnitTurnState.HasEnded == false`: current tile baseline is white.
- Alive player with `UnitTurnState.HasEnded == true`: current tile baseline is grey.
- Enemy units are ignored.
- Dead player units are ignored, so their tile returns to the original map color.
- Units without a `CurrentTile` are ignored.

## Drawing Priority
The repaint order remains:
1. Restore original map tile color from `originalColors`.
2. Apply player foot baseline: white for not ended, grey for ended.
3. Apply selected highlight.
4. Apply move / attack / skill action highlight.

This lets selected, move, attack, and skill highlights temporarily cover white or grey. When highlights clear, `Repaint()` restores the correct player baseline.

## Clear and Repaint Behavior
`originalColors` still stores only the original map color. White and grey are never written into `originalColors`. `MovementDebugHighlighter` now stores a per-renderer player foot color so each player tile can repaint as white or grey after `Clear()`, `ClearActionHighlights()`, or `ClearSelected()`.

## End Turn and New PlayerTurn
End Turn marks the selected player unit as ended through the existing turn flow. The existing highlighter clear/refresh path, plus the existing 0.25 second auto refresh, will repaint that unit's foot tile grey. When EnemyTurn ends and a new PlayerTurn starts, `UnitTurnState.ResetForNewRound()` clears `HasEnded`; the next refresh repaints alive player foot tiles white.

## Unmodified Systems
No scene, prefab, material asset, KayKit asset, SkillData asset, combat system, skill formula, TurnManager free-order rule, End Turn rule, EnemyTurn logic, camera control, or ActiveUnitProvider behavior was changed.

## Unity Play Mode Test Steps
1. Open `Assets/_BoneThrone/Scenes/GridTest.unity`.
2. Enter Play Mode.
3. Confirm all alive player units start with white foot tiles.
4. Select a player and click End Turn.
5. Confirm that player's foot tile becomes grey after highlight clear/refresh and cannot be selected again this round.
6. Confirm other not-ended alive player units remain white.
7. Use move, attack, and skill targeting highlights over player foot tiles; clear targeting and confirm white/grey baselines return.
8. End all alive player units, let EnemyTurn complete, and confirm the next PlayerTurn resets all alive player foot tiles to white.
9. Kill or disable a player unit if available in test setup and confirm no white/grey marker remains on its old tile.

## Risk and Rollback
Risk is limited to tile debug coloring. If the grey marker conflicts with another highlight, revert `MovementDebugHighlighter.cs` to the previous single-color `playerFootRenderers` baseline. No gameplay state or scene binding is involved.
