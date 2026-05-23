# Phase 05 DevLog - Selection, BFS Range and A* Movement

## Date
2026-05-23

## Branch
phase/05-selection-movement

## Unity version
Unity 6000.3.10f1 / Unity 6.3 LTS

## Goal
Implement the minimal playable movement loop for the tactics demo:
select a player unit, calculate reachable tiles with BFS, find a four-direction A* path, move the unit, and update tile occupancy.

## Files changed
- Assets/_BoneThrone/Scripts/Movement/SelectionManager.cs
- Assets/_BoneThrone/Scripts/Movement/MovementRangeFinder.cs
- Assets/_BoneThrone/Scripts/Movement/Pathfinder.cs
- Assets/_BoneThrone/Scripts/Movement/UnitMover.cs
- Assets/_BoneThrone/Scripts/Movement/PlayerMovementController.cs
- Assets/_BoneThrone/Scripts/Movement/MovementDebugHighlighter.cs
- Assets/_BoneThrone/Scripts/Tests/GridTestBuilder.cs

## Unity test result
- Compile: Pass
- Play Mode: Pass
- Scene: Assets/_BoneThrone/Scenes/GridTest.unity

## Passed tests
- [x] Player Unit can be selected.
- [x] Only Player and alive units can be selected.
- [x] BFS reachable range is calculated from the selected Unit.
- [x] BFS uses four-direction movement only.
- [x] Obstacles and occupied tiles are excluded from reachable range.
- [x] Clicking a reachable empty Tile moves the Unit.
- [x] A* pathfinding uses four-direction movement only.
- [x] Unit movement updates Tile occupancy correctly.
- [x] Original Tile is released after movement.
- [x] Target Tile becomes occupied by the moved Unit.
- [x] Clicking unreachable, blocked, or occupied Tiles does not move the Unit.
- [x] No turn system, combat, skills, AI, rooms, UI HUD, networking, or NetworkManager was implemented.

## Notes
A temporary GridTestBuilder was added under Scripts/Tests to generate test grids for GridTest.unity. It is only a test helper and is not part of the formal level generation system.

## Known issues
- Movement currently uses instant transform placement rather than animation.
- MovementDebugHighlighter is debug-only and should not be treated as final visual design.
- GridTestBuilder uses reflection to help populate GridManager initial tiles for testing; future formal level generation should use a public API or LevelData.

## Next phase notes
Phase 6 should introduce the turn system and action state boundaries, including movement/action usage tracking, without adding combat or networking transport details yet.