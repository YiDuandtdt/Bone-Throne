# Phase 15.12 - Formal Level Scene Plan

## 1. Goal and Boundary

Phase 15.12 defines the formal level scene plan for the future three-floor dungeon structure.

This phase is planning only:

- Do not create formal scenes.
- Do not create `Level_01`, `Level_02`, or `Level_03`.
- Do not modify `GridTest.unity`.
- Do not modify C# code.
- Do not modify prefabs.
- Do not modify SkillData.
- Do not modify KayKit source assets.
- Do not create ScriptableObject assets.
- Do not implement Boss, LAN, Victory, Defeat, Retry, BossDoor, BossKey, or SupplyPoint systems.

Phase 15.13 is the first phase that may begin formal scene setup, after this plan is approved.

`GridTest.unity` remains the regression baseline and must not be replaced by formal level scenes.

Unity-Skills note:

- Unity-Skills was requested for read-only assistance.
- The local UnitySkills REST server did not respond during this phase.
- Fallback method used: ordinary file scan, documentation scan, prefab / scene YAML inspection, and docs-only planning.

## 2. Formal Three-Level Structure

The formal dungeon should be planned as three separate future scenes:

- `Level_01`
- `Level_02`
- `Level_03`

Design intent:

- `Level_01`: onboarding and first playable slice.
- `Level_02`: escalation and route complexity.
- `Level_03`: current-phase final floor planning with boss placeholder preparation only.

No scene loading, victory / defeat flow, Boss fight, or formal data asset instances are implemented in Phase 15.12.

### Level_01

Theme:

- Entrance corridor / first dungeon approach.
- Clear, readable rooms with short paths.
- Teach movement, selection, free-order PlayerTurn, basic attack, skill, potion, key, stairs, and simple room reveal.

Difficulty:

- Low.
- Favors melee and basic tactical positioning.

Suggested room count:

- 4 to 5 rooms.

Suggested progression:

1. Start Room.
2. Simple Combat Room.
3. HealthPotion side pickup or small optional side pocket.
4. Key Room.
5. Stair / Exit Room.

Critical path:

- Start -> Combat -> Key -> Stairs.

Optional rooms:

- At most one optional side room.
- Optional room should not block progression.

Key / stairs rule:

- One key should unlock progression to stairs.
- Stairs should remain a simple next-floor gate.
- Do not implement formal modal UI in this planning phase.

Enemy principle:

- Main enemies: `Skeleton_Minion`, `Skeleton_Warrior`.
- Optional light variation: one `Skeleton_Rogue` or one `Skeleton_Mage`.
- No Boss.

Environment prefab use:

- Use floors, walls, basic architecture, crates, barrels, rubble, rocks, torch mounted, simple decor.
- Avoid visually dense furniture in critical movement lanes.

Special interactables:

- HealthPotion can support attrition.
- Key and Stairs are progression interactables.
- Chest / Doorway may appear only as visual-only dressing if needed, but should not imply loot or lock systems.

### Level_02

Theme:

- Deeper dungeon halls / residence or courtyard-like interior using more furniture and decor.
- More turns, more route choice, and more mixed enemy pressure.

Difficulty:

- Medium.
- Introduces stronger positional pressure and ranged threats.

Suggested room count:

- 5 to 7 rooms.

Suggested progression:

1. Start Room.
2. Branch Combat Room.
3. Optional Reward / Dressing Room.
4. Key Room behind a harder encounter.
5. Elite / Hard Combat Room.
6. Stair / Exit Room.

Critical path:

- Start -> one required combat -> key route -> hard combat -> stairs.

Optional rooms:

- 1 to 2 optional rooms can provide potion placement, visual dressing, or future reward hook.
- Optional rooms should not require new systems.

Key / stairs rule:

- One key can still support floor progression.
- Avoid BossKey and BossDoor.

Enemy principle:

- Introduce `Skeleton_Rogue` and `Skeleton_Mage` as regular threats.
- Continue using `Skeleton_Minion` and `Skeleton_Warrior` for front-line pressure.
- Avoid `Skeleton_Necromancer` as a common enemy until Level_03.

Environment prefab use:

- Add furniture, shelves, banners, candles, tables, benches, bookcases, and more varied room dressing.
- Use rubble / crates / barrels to imply obstacles, but blocking semantics must be validated later.

Special interactables:

- HealthPotion can appear in optional or post-combat locations.
- Chest and Doorway remain visual-only candidates.
- Do not implement chest loot, door lock / unlock, SupplyPoint, BossDoor, or BossKey.

### Level_03

Theme:

- Treasure vault / deeper throne approach.
- Current planning endpoint for the demo's three-floor structure.

Difficulty:

- High for current non-Boss content.
- Enemy composition should pressure players to use skills, potion, defend, target priority, and movement.

Suggested room count:

- 6 to 8 rooms.

Suggested progression:

1. Start Room.
2. Combat Room with mixed enemies.
3. Key Room or route split.
4. Hard Combat Room.
5. Boss Preparation Room planning only.
6. Boss Room placeholder planning only.

Critical path:

- Start -> mixed combat -> key / gate concept -> hard combat -> boss preparation -> boss placeholder destination.

Optional rooms:

- Optional side route can provide visual treasure dressing or future reward hook.
- Do not implement formal loot.

Key / stairs rule:

- Level_03 may plan a future BossDoor / BossKey concept, but must not implement it in Phase 15.12.
- Stairs after Level_03 should be treated as future victory / end-flow planning, not implemented.

Enemy principle:

- Introduce `Skeleton_Necromancer`.
- Mix `Skeleton_Rogue`, `Skeleton_Mage`, `Skeleton_Warrior`, and support minions.
- `Skeleton_Golem_Boss` may be planned as a placeholder location only.
- Do not implement Boss fight, Boss AI, Boss skills, or Boss victory flow.

Environment prefab use:

- Use strongest visual dressing: banners, candles, shelves, columns, pillars, rubble, treasure-looking static dressing if available.
- Maintain tactical readability.

Special interactables:

- HealthPotion can support pre-Boss attrition.
- SupplyPoint can be noted as future preparation, but not implemented.
- Chest / Doorway remain visual-only or future candidates.

## 3. Room Type Plan

### Start Room

Purpose:

- Spawn players safely.
- Establish floor theme.

Rules:

- No immediate enemy pressure at spawn.
- Clear path to first combat room.
- Should contain no progression-critical interactable except tutorial / visual staging if later approved.

### Combat Room

Purpose:

- Standard encounter space.

Rules:

- Uses existing Grid / Tile, movement, turn, combat, skill, potion, defend, and EnemyTurn systems.
- Should support clear sight lines and reachable enemy layouts.

### Key Room

Purpose:

- Contains key progression object or future key encounter.

Rules:

- Can require clearing enemies before safe pickup.
- Uses existing Key concept only.
- Does not introduce multiple key IDs or BossKey.

### Elite / Hard Combat Room

Purpose:

- Higher pressure encounter without Boss systems.

Rules:

- Use enemy combinations and layout difficulty, not new AI rules.
- Suitable for Level_02 and Level_03.

### Treasure / Reward Room

Purpose:

- Visual or future reward space.

Rules:

- Planning only.
- Chest can be visual-only.
- No loot system implementation.

### Stair / Exit Room

Purpose:

- Floor transition endpoint.

Rules:

- Uses existing Stairs concept.
- Future scene loading is deferred.
- Formal modal confirmation remains deferred.

### Boss Preparation Room

Purpose:

- Pre-Boss staging for Level_03.

Rules:

- Planning only.
- Can reserve space for future SupplyPoint.
- No SupplyPoint implementation.

### Boss Room Placeholder

Purpose:

- Reserve formal space and flow for a later Boss phase.

Rules:

- Planning only.
- `Skeleton_Golem_Boss` may be used as a placeholder reference in docs.
- Do not implement Boss fight, Boss AI, Boss skills, BossDoor, BossKey, victory, or defeat.

## 4. Suggested Per-Level Structure

| Level | Suggested Rooms | Core Enemies | Progression Focus | Deferred Notes |
| --- | ---: | --- | --- | --- |
| `Level_01` | 4-5 | `Skeleton_Minion`, `Skeleton_Warrior`, optional one `Skeleton_Rogue` / `Skeleton_Mage` | Basic movement, combat, potion, key, stairs | No Boss, no complex doors |
| `Level_02` | 5-7 | `Skeleton_Warrior`, `Skeleton_Rogue`, `Skeleton_Mage`, support minions | Mixed threats, optional route, harder key path | Chest / Doorway visual-only |
| `Level_03` | 6-8 | `Skeleton_Mage`, `Skeleton_Rogue`, `Skeleton_Necromancer`, support enemies | Final floor escalation, boss placeholder planning | Boss fight deferred |

## 5. Grid / Tile / Room Planning Principles

- Formal levels remain based on the existing Grid / Tile system.
- Room progression should extend the current `RoomController`, `RoomTrigger`, `RoomShadowController`, and `RoomEnemyActivator` idea.
- Rooms should remain hidden / shadowed before entry when appropriate.
- Do not change tile occupancy logic.
- Do not change movement or pathfinding logic.
- Do not change turn or combat logic.
- Formal rooms should be easier to inspect than GridTest, with clearly named room roots and spawn markers.
- Avoid overly dense prop placement until collider / blocking semantics are intentionally refined.

## 6. Environment Prefab Usage Principles

Phase 15.4 produced 37 Environment prefabs under:

- `Architecture`
- `Decor`
- `Floors`
- `Furniture`
- `Props`
- `Walls`

### Blockout Candidates

Best for initial room layout:

- `Env_Floor_Tile_Large`
- `Env_Floor_Tile_Small`
- `Env_Floor_Tile_Broken_A`
- `Env_Wall_Straight`
- `Env_Wall_Corner`
- `Env_Wall_Endcap`
- `Env_Column`
- `Env_Pillar`

### Visual Dressing Candidates

Best after walkable layout is clear:

- `Env_Banner_*`
- `Env_Candle*`
- `Env_Plate*`
- `Env_Table_*`
- `Env_Chair`
- `Env_Stool*`
- `Env_Bench`
- `Env_Bookcase_*`
- `Env_Shelf*`
- `Env_Barrel_Small`
- `Env_Crate_Small`
- `Env_Box_Small`
- `Env_Bucket`
- `Env_Rubble_Half`
- `Env_Rocks_Small`
- `Env_Torch_Mounted`

### Blocking / Collider Risk

Potential tile blockers needing later refinement:

- walls
- column / pillar
- crate / barrel / box / bucket
- furniture
- rubble / rocks

Phase 15.12 does not refine colliders or blocking logic.

Do not modify KayKit source. Do not continue prefabization in this phase.

## 7. Interactable Usage Principles

Current Interactable prefabs:

- `HealthPotion`
- `Key`
- `Stairs`
- `Chest`
- `Doorway`

Usage plan:

- `HealthPotion` supports attrition and recovery pacing.
- `Key` supports floor progression requirements.
- `Stairs` supports future floor transition endpoints.
- `Chest` remains visual-only / future loot candidate.
- `Doorway` remains visual-only / future door candidate.

Explicit non-goals:

- Do not implement Chest loot.
- Do not implement Door lock / unlock.
- Do not implement BossDoor.
- Do not implement BossKey.
- Do not implement SupplyPoint.
- Do not implement formal modal confirmation UI.

## 8. Unit / Encounter Usage Principles

### Players

Formal levels should assume the existing party:

- `Fighter`
- `Ranger`
- `Mage`
- `Barbarian`

Rules:

- Single-player PlayerTurn remains free-order.
- Do not use Fighter -> Ranger -> Mage -> Barbarian fixed-order in single-player.
- Per-unit animator controllers and weapon visuals are ready for formal scene validation.

### Enemies

Available regular enemies:

- `Skeleton_Minion`
- `Skeleton_Warrior`
- `Skeleton_Rogue`
- `Skeleton_Mage`
- `Skeleton_Necromancer`

Boss placeholder:

- `Skeleton_Golem_Boss`

Enemy usage by floor:

- `Level_01`: Minion / Warrior first, optional light Rogue or Mage.
- `Level_02`: Warrior / Rogue / Mage mixed encounters.
- `Level_03`: Necromancer introduction and harder mixed groups.

Basic attack range differences should shape encounter rhythm:

- melee units pressure adjacency
- `Ranger` and caster enemies increase spacing pressure
- `Skeleton_Rogue` can threaten short range
- mage / necromancer enemies can threaten farther than melee

`Skeleton_Golem_Boss` is planning-only and must not be treated as a normal enemy.

## 9. Three-Level Progression Planning

### Level_01 to Level_02

Future direction:

- Clear required room.
- Collect shared key.
- Reach stairs.
- Transition to Level_02.

Phase 15.12 does not implement scene loading or modify LevelManager.

### Level_02 to Level_03

Future direction:

- Increase room count and route complexity.
- Key path should require at least one harder encounter.
- Stairs route should be clearer after key acquisition.

Phase 15.12 does not create LevelData or ProgressionData.

### After Level_03

Future direction:

- Boss placeholder route can lead to future Boss encounter.
- Victory / Defeat / Retry planning belongs to later phases.

Phase 15.12 does not implement Victory, Defeat, Retry, Boss fight, or scene loading.

## 10. Data Assetization Alignment

Phase 15.12 only plans future `LevelData`, `RoomData`, and `EncounterData` needs.

Do not create:

- `LevelData` instances
- `RoomData` instances
- `EncounterData` instances
- formal data directories
- formal scene references

After Phase 15.13 begins actual scene setup, data assetization can be revisited based on real scene needs.

Deferred data:

- Boss data
- BossDoor / BossKey data
- SupplyPoint data
- RewardData
- Victory / Defeat data
- LAN / network command data
- Animation polish override data

## 11. Phase 15.13 Input Requirements

If Phase 15.13 starts scene setup, it should define:

- scene names:
  - `Level_01`
  - `Level_02`
  - `Level_03`
- folder path:
  - `Assets/_BoneThrone/Scenes/`
- creation order:
  1. `Level_01`
  2. `Level_02`
  3. `Level_03`
- system objects to copy or reproduce from GridTest:
  - Grid / Tile management
  - Turn / Combat / Skill / Potion systems
  - UI / BattleHUD setup
  - Selection and movement systems
  - Room system roots, if ready
  - Level progression placeholder only if explicitly approved
- initial blockout prefabs:
  - floors
  - walls
  - architecture
  - limited props
  - HealthPotion / Key / Stairs as needed
  - player / enemy prefabs
- content not to copy blindly:
  - test-only helper scripts
  - ContextMenu-only testers
  - temporary debug objects
  - accidental GridTest-only overrides
  - placeholder room / level wiring that does not match formal plan

Initial acceptance criteria:

- scene opens without red Console errors
- no missing scripts
- no missing prefab references
- four players can spawn
- PlayerTurn free-order works
- at least one basic combat room works in `Level_01`
- EnemyTurn returns to PlayerTurn
- HealthPotion / Key / Stairs can be validated if placed
- GridTest remains unchanged and still passes regression

## 12. Deferred Content

The following must remain deferred:

- Boss fight
- Boss AI
- BossDoor / BossKey
- SupplyPoint
- Chest loot
- Door lock / unlock
- Victory / Defeat / Retry
- LAN / Networking
- formal data asset instances
- full progression save / load
- advanced VFX / audio polish
- formal modal stairs confirmation

## 13. Risks

- Creating formal scenes too early can produce large, noisy diffs.
- GridTest regression may be damaged if formal level work replaces or mutates it.
- Environment prefab blocking semantics are not fully defined yet.
- Room, Level, and Data responsibilities can blur if all are introduced at once.
- Boss, Door, Chest, or SupplyPoint implementation can easily expand scope.
- Scene references can be lost when copying manager objects.
- Formal scene behavior may drift from GridTest if validation criteria are not explicit.
- Over-dressing rooms can obscure tile readability.

## 14. Execution Principles

- Plan first, build second.
- Make `Level_01` the first playable slice.
- Expand `Level_02` and `Level_03` after `Level_01` is stable.
- Keep GridTest as regression baseline.
- Formal levels must not replace GridTest.
- Every new scene must have explicit acceptance criteria.
- Large scene changes must be split into staged commits.
- Prefer clear room roots, spawn markers, and naming over hidden scene wiring.
- Validate after each room or system slice.

## 15. Phase 15.12 Output

This document is the Phase 15.12 formal level scene plan.

It intentionally creates no scene, no prefab, no code, no SkillData, no ScriptableObject, and no KayKit source change.
