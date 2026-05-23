# Phase 06 DevLog - Turn System and Fixed Action Order

## Date
2026-05-23

## Branch
phase/06-turn-system

## Unity version
Unity 6000.3.10f1 / Unity 6.3 LTS

## Goal
Implement a minimal turn system for the tactics demo:
track player/enemy phases, track each Unit's moved/acted state, restrict repeated movement, and prepare the fixed future multiplayer order.

## Files changed
- Assets/_BoneThrone/Scripts/Turns/TurnPhase.cs
- Assets/_BoneThrone/Scripts/Turns/UnitTurnState.cs
- Assets/_BoneThrone/Scripts/Turns/TurnOrderService.cs
- Assets/_BoneThrone/Scripts/Turns/TurnManager.cs
- Assets/_BoneThrone/Scripts/Turns/ActionPermissionService.cs
- Assets/_BoneThrone/Scripts/Tests/TurnSystemTester.cs
- Assets/_BoneThrone/Scripts/Movement/PlayerMovementController.cs

## Unity test result
- Compile: Pass
- Play Mode: Pass
- Scene: Assets/_BoneThrone/Scenes/GridTest.unity

## Passed tests
- [x] Player round can be started.
- [x] Player UnitTurnState can be reset at the start of a round.
- [x] Unit can be marked as moved.
- [x] A moved Unit cannot move again in the same round.
- [x] Unit can be marked as acted.
- [x] Default singleplayer mode allows choosing any alive Player Unit that has not moved.
- [x] Default singleplayer mode does not require Unit.RoleId to match CurrentRole.
- [x] Optional fixed order mode can restrict movement to the current role.
- [x] Fixed order can advance through Fighter -> Ranger -> Mage -> Barbarian -> EnemyTurn.
- [x] EnemyTurn is only a placeholder and does not run AI.
- [x] No D20, combat, skills, AI, rooms, UI HUD, networking, or NetworkManager was implemented.

## Notes
Phase 6 keeps singleplayer free-selection behavior by default. Fixed role order is implemented as an optional enforcement mode for future multiplayer preparation.

## Known issues
- UnitTurnState must be manually added to Units that participate in turn restrictions.
- TurnManager.Player Units must be assigned for automatic reset behavior.
- EnemyTurn currently has no AI behavior, by design.
- No UI button exists yet for ending turns; testing uses TurnSystemTester ContextMenu.

## Next phase notes
Phase 7 should introduce D20 basic attack combat while preserving the turn/action boundaries created in Phase 6.