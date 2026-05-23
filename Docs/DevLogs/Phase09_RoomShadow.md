# Phase 09 DevLog - Room Progression and Semi-transparent Shadow

## Date
2026-05-23

## Branch
phase/09-room-shadow

## Unity version
Unity 6000.3.10f1 / Unity 6.3 LTS

## Goal
Implement the minimal room progression loop:
room states, room entry trigger, semi-transparent shadow reveal, assigned enemy activation, Tile occupancy on activation, and cleared-state detection.

## Files changed
- Assets/_BoneThrone/Scripts/Rooms/RoomState.cs
- Assets/_BoneThrone/Scripts/Rooms/RoomController.cs
- Assets/_BoneThrone/Scripts/Rooms/RoomTrigger.cs
- Assets/_BoneThrone/Scripts/Rooms/RoomShadowController.cs
- Assets/_BoneThrone/Scripts/Rooms/RoomEnemyActivator.cs
- Assets/_BoneThrone/Scripts/Tests/RoomSystemTester.cs
- Assets/_BoneThrone/Scripts/Movement/PlayerMovementController.cs

## Unity test result
- Compile: Pass
- Play Mode: Pass
- Scene: Assets/_BoneThrone/Scenes/GridTest.unity

## Passed tests
- [x] Room starts in Unentered state.
- [x] Shadow is visible before room entry.
- [x] Hidden enemies do not occupy Tiles before room entry.
- [x] Player entry can trigger room entry.
- [x] RoomSystemTester can manually trigger room entry.
- [x] Room shadow is hidden after entry.
- [x] Assigned enemies are activated after room entry.
- [x] Activated enemies are placed through Unit.TryPlaceOnTile.
- [x] Enemy spawn Tiles become occupied by the activated enemies.
- [x] Re-entering an already entered room is safely ignored.
- [x] Room detects Cleared after assigned enemies are dead.
- [x] Spawn Tile occupied case is handled safely with warning.
- [x] Player movement can click Tiles under RoomTrigger after trigger raycast handling.
- [x] No keys, stairs, level switching, skills, UI HUD, networking, or NetworkManager was implemented.

## Notes
RoomEnemyActivator releases enemy Tile occupancy before hiding enemies at start, preventing hidden enemies from blocking movement. On room entry, enemies are activated and placed onto assigned spawn Tiles through the existing Unit placement API.

PlayerMovementController may include a small compatibility fix so movement click raycasts ignore trigger colliders, allowing RoomTrigger volumes to coexist with clickable Tiles.

## Known issues
- RoomTrigger physics can depend on Collider/Rigidbody setup; RoomSystemTester remains the reliable manual validation path.
- Room shadows are simple SetActive objects, not complex fog of war.
- Enemy activation uses pre-bound enemies and spawn Tiles only; no procedural spawning or waves.
- No formal LevelManager, keys, stairs, rewards, or room sequence progression yet.

## Next phase notes
Phase 10 should introduce keys, stairs, level switching, and level-up progression while reusing the existing room, unit, movement, combat, and AI systems.