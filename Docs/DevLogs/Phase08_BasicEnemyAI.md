# Phase 08 DevLog - Basic Enemy AI

## Date
2026-05-23

## Branch
phase/08-basic-enemy-ai

## Unity version
Unity 6000.3.10f1 / Unity 6.3 LTS

## Goal
Implement the minimal enemy AI loop:
select the nearest alive player unit, attack if in range, move closer if out of range, and safely skip when no valid action is possible.

## Files changed
- Assets/_BoneThrone/Scripts/AI/EnemyAIResult.cs
- Assets/_BoneThrone/Scripts/AI/EnemyTargetSelector.cs
- Assets/_BoneThrone/Scripts/AI/EnemyMovementPlanner.cs
- Assets/_BoneThrone/Scripts/AI/EnemyAIController.cs
- Assets/_BoneThrone/Scripts/Tests/EnemyAITester.cs

## Unity test result
- Compile: Pass
- Play Mode: Pass
- Scene: Assets/_BoneThrone/Scenes/GridTest.unity

## Passed tests
- [x] Enemy can find the nearest alive Player Unit.
- [x] Enemy target selection uses Manhattan distance.
- [x] Distance tie can be resolved deterministically.
- [x] Enemy attacks when target is within basic attack range.
- [x] Enemy attack reuses Phase 7 CombatSystem and D20 logic.
- [x] Enemy moves closer when target is out of range.
- [x] Enemy movement reuses Phase 5 Pathfinder and UnitMover.
- [x] Enemy does not move onto occupied Tiles.
- [x] Enemy does not move onto unwalkable Tiles.
- [x] Enemy safely skips if no alive player target exists.
- [x] Enemy safely skips if no path or valid move exists.
- [x] Dead enemy does not act.
- [x] Multiple enemies can run through EnemyAITester Run All.
- [x] No rooms, spawning, skills, UI HUD, networking, or NetworkManager was implemented.

## Notes
EnemyAITester is a temporary ContextMenu helper for Play Mode validation. Run Selected Enemy Action runs only the explicitly assigned Selected Enemy. Run All Enemy Actions iterates through the Enemies array.

Phase 8 does not implement a formal EnemyTurn scheduler yet. It only provides reusable simple AI behavior that can be connected to the turn system later.

## Known issues
- EnemyTurn is still not a formal automated round.
- CombatSystem should keep TurnManager and ActionPermissionService unbound during Phase 8 enemy AI tests, otherwise Phase 6 player-only permission checks can reject enemy attacks.
- No behavior tree, utility scoring, patrol, aggro memory, or group tactics are implemented.
- No room spawning or enemy spawn system is implemented.

## Next phase notes
Phase 9 should introduce room progression and semi-transparent room shadow logic, while reusing the existing Unit, Movement, Combat, and AI systems.