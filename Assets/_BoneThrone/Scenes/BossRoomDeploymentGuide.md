# Boss Room Deployment Guide

This guide matches `Assets/_BoneThrone/Scenes/boss_test.unity` and is also the handoff rule for placing the real Boss room in Level 3.

## Closed Loop

1. Place the Boss directly in the Boss room scene, the same way normal enemies are placed.
2. Add that Boss to the Boss room `RoomEnemyActivator.assignedEnemies`.
3. Put the Boss spawn/current tile into `RoomEnemyActivator.spawnTiles`.
4. Put `BossKey` outside the Boss room, in a reachable area after the required non-Boss enemies are defeated.
5. Put `BossDoor` on the only valid Boss-room entrance.
6. Put the Boss room `RoomTrigger` behind the opened door, not covering side entrances.
7. The player clears non-Boss enemies, collects the Boss key, opens the Boss door, then enters the Boss room through the door.
8. Only after entering the Boss room does the Boss fight start: Boss health bar appears, Boss BGM starts, and the red health fill animates from 0 to full.

## Important Rules

- The Boss is not runtime-spawned. It should be visible in the scene from the start.
- Boss objects must have `Boss` or `Golem` in the GameObject name or display name.
- `RoomEnemyActivator` now keeps Boss-like assigned enemies active at start, so a deployed Boss will not be hidden like normal room minions.
- The Boss still does not act before the Boss fight starts. Enemy turns skip Boss-like units until the Boss room has been entered through an opened Boss door.
- The Boss health bar and Boss intent preview stay hidden until the Boss fight starts.
- `BossDoor` has no authored animation clip right now. Its fallback "open" behavior hides the `Visual` child and disables blockers.
- `BossDoor` restores nearby tiles to their original walkable state when opened. This prevents wall/side-entry tiles from becoming walkable by accident.
- A formal Boss room should be physically and grid-wise closed on the sides. Only the door passage should be walkable after the door opens.

## Optional BossGateProgressionState

`BossGateProgressionState` can be omitted in formal levels; `BossKey`, `BossDoor`, and Boss-room code can find or create one at runtime.

For a test scene or safer manual setup, add one scene object named `BossGateProgressionState` and wire:

- `requiredClearedRooms`: all non-Boss rooms that must be cleared before the Boss key can be collected.
- `levelProgressionService`: the scene progression service, if one exists.
- `bossNameContains`: `boss`.
- `golemNameContains`: `golem`.

## boss_test Current Setup

- `Room_1`: outer non-Boss fight room.
- `BossKey_AfterOuterRoomClear`: outside the Boss room at the reachable outer area.
- `BossDoor_ToBossRoom`: placed at the Boss-room entrance.
- `Room_0`: Boss room.
- `Skeleton_Golem_Boss`: scene-deployed Boss inside `Room_0`, assigned to `Room_0.RoomEnemyActivator`.
- `Room_0_BossTrigger`: Boss-room trigger. It only starts the Boss fight after the door is opened and the player enters from near the Boss door.
- Boss-room side and back edge tiles are intentionally set non-walkable so the test room can only be entered through the door route.

## Test Steps

1. Open `Assets/_BoneThrone/Scenes/boss_test.unity`.
2. Enter Play Mode.
3. Confirm `Skeleton_Golem_Boss` is visible inside the black Boss room from the start.
4. Kill all visible non-Boss enemies outside the Boss room.
5. Collect `BossKey_AfterOuterRoomClear`.
6. Move a player near `BossDoor_ToBossRoom`; the door should open and its visual should disappear.
7. Move through the door into the Boss room trigger.
8. Expected result: Boss BGM starts, Boss health UI appears, red fill grows from 0 to current Boss HP, and Boss intent previews begin during player turns.
