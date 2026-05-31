# Phase 15.11 - Formal Data Assetization Plan

## 1. Goal and Boundary

Phase 15.11 defines a formal data assetization plan before formal three-floor level planning and construction begins.

This phase is planning only:

- Do not create ScriptableObject assets.
- Do not migrate data.
- Do not modify C# code.
- Do not modify prefabs.
- Do not modify scenes.
- Do not modify SkillData.
- Do not modify KayKit source assets.
- Do not create formal Level scenes.
- Do not implement Boss, LAN, BossDoor, BossKey, SupplyPoint, or formal Level progression.

The goal is to identify current data sources, Inspector configuration, prefab-bound configuration, scene-bound configuration, and script default / hard-coded configuration, then define a safe order for future data assetization.

`Assets/_BoneThrone/Scenes/GridTest.unity` remains the regression baseline and must not be reworked just to introduce data assets.

## 2. Existing Data Assets

### SkillData

Current production SkillData path:

- `Assets/_BoneThrone/Data/Skills/`

Current SkillData assets include:

- Barbarian:
  - `barbarian_blood_fury_slash.asset`
  - `barbarian_heavy_cleave.asset`
  - `barbarian_rage_strike.asset`
- Fighter:
  - `fighter_crushing_challenge.asset`
  - `fighter_guard_strike.asset`
  - `fighter_shield_bash.asset`
- Mage:
  - `mage_arcane_burst.asset`
  - `mage_fireball.asset`
  - `mage_frost_bolt.asset`
- Ranger:
  - `ranger_piercing_arrow.asset`
  - `ranger_precision_shot.asset`
  - `ranger_quick_shot.asset`

Current test SkillData path:

- `Assets/_BoneThrone/Data/OnlyTest/Skills/`

`OnlyTest/Skills` contains matching test-oriented copies of the same class skill assets.

### Existing Data Types Without Formal Assets

`Assets/_BoneThrone/Scripts/Units/UnitData.cs` already defines a future `UnitData` ScriptableObject type with:

- unit id
- display name
- role id
- faction
- stats

Current scan found no formal `UnitData` asset instances under `Assets/_BoneThrone/Data/`.

### Missing Formal Data Directories

The project does not currently have formal directories for:

- Character data
- Enemy data
- Level data
- Room data
- Encounter data
- Interactable data
- Reward data
- Progression data
- AnimationProfile data

These directories should not be created in Phase 15.11 unless a later implementation prompt explicitly approves it.

## 3. Current Prefab Inspector Configuration Sources

The following data still primarily lives on project-owned prefab Inspector fields.

### Unit Identity and Stats

Source examples:

- `Assets/_BoneThrone/Prefabs/Units/Players/*.prefab`
- `Assets/_BoneThrone/Prefabs/Units/Enemies/*.prefab`
- `Assets/_BoneThrone/Prefabs/Units/Boss/*.prefab`

Current prefab-bound data:

- `Unit.unitId`
- `Unit.displayName`
- `Unit.roleId`
- `Unit.faction`
- `UnitStats.level`
- `UnitStats.maxLevel`
- `UnitStats.maxHpPerLevel`
- `UnitStats.maxHp`
- `UnitStats.moveRange`
- `UnitStats.basicAttackRange`
- `UnitStats.attackModifier`
- `UnitStats.defense`
- `UnitStats.baseDamage`

`basicAttackRange` is gameplay configuration and must remain independent of weapon visuals.

### Runtime State Components With Inspector Defaults

Current prefab / component sources:

- `UnitPotionState.initialPotionCount`
- `UnitPotionState.currentPotionCount`
- `UnitDefenseState.isDefending`
- `UnitDefenseState.flatReduction`
- `SkillRuntime.skillSlots`
- `SkillRuntime.currentCooldowns`
- `UnitTurnState` state defaults
- status components such as bleed, stun, and damage amplification where present

Runtime state values should not be converted directly into static data assets. Future data assets may define starting values, but active runtime state must remain runtime-owned.

### Visual, Animation, and Equipment Bindings

Current project-owned Unit prefabs contain:

- visual child prefab references
- Animator components
- per-unit Animator Controller references
- weapon / equipment visual children and sockets
- enemy floating health bar child
- collider and selection target setup

These are candidates for a future `UnitVisualData` or `AnimationProfileData`, but should not be migrated before formal level needs are clear.

### Interactable Prefabs

Current interactable prefab-bound configuration:

- `HealthPotionPickup`
  - selection manager reference when placed
  - potion amount
  - pickup range
  - consume-on-collect flag
  - collected guard
- `KeyItem`
  - progression service reference when placed
  - consume-on-collect flag
  - collected guard
- `InteractableStairs`
  - progression service reference
  - selection manager reference
  - feedback renderers
  - normal / hover material references
  - second-click confirmation flag

Chest and Doorway remain visual-only candidates and should not drive data implementation yet.

## 4. Current Scene-Bound Configuration Sources

`GridTest.unity` still owns regression-scene wiring for many systems.

Scene-bound sources include:

- Turn / Combat / Skill / Potion / UI service references.
- `TurnManager.playerUnits`.
- `EnemyTurnRunner` references and fallback arrays.
- `LevelProgressionService`.
- `LevelManager`.
- `RoomController`.
- `RoomShadowController`.
- `RoomEnemyActivator`.
- `RoomEnemyActivator.assignedEnemies`.
- `RoomEnemyActivator.spawnTiles`.
- Key / Stairs / HealthPotion scene instances.
- UI references such as BattleHUD, skill bar, hero panel, prompt, turn banner, and combat feedback.
- Grid, Tile, tile occupancy, and placed unit references.

Phase 15.11 should document these scene-bound sources only. It should not migrate them.

## 5. Current Script Defaults and Hard-Coded Configuration

The following values or assumptions are currently script-owned defaults or hard-coded behavior:

- `PotionSystem.DefaultHealAmount = 4`
- `DefendSystem.DefaultReduction = 2`
- `UnitDefenseState.flatReduction = 2`
- `AttackRangeService.basicAttackRange = 1` fallback
- `HealthPotionPickup.potionAmount = 1`
- `HealthPotionPickup.pickupRange = 1.5`
- `EnemyTurnRunner.enemyActionDelay = 0.4`
- `UnitMover` movement speed / turn duration presentation settings
- `UnitAnimationController` parameter names:
  - `MoveSpeed`
  - `BasicAttack`
  - `Skill`
  - `Hit`
  - `Defend`
  - `UsePotion`
  - `IsDead`
  - `IsDefending`
- `LevelProgressionService` shared key, required rooms, placeholder level transition, and party level-up assumptions.
- `RoomEnemyActivator` assigned enemy and spawn tile arrays.
- `EnemyMovementPlanner` four-neighbor movement assumptions.
- Key and Stairs progression assumptions.
- Class skill effect routing through class-specific skill effect scripts.

These should be reviewed before formal data migration, but not all of them should become data assets immediately.

## 6. Data Suitable for Gradual Assetization

Good candidates for future phased assetization:

- Character base data.
- Enemy base data.
- Unit stats and basic attack range.
- Unit skill loadout references.
- Unit visual and animation binding data.
- Room structure metadata.
- Encounter composition and spawn rules.
- Level progression metadata.
- Interactable configuration.
- Reward configuration.
- Animation profile configuration, after animation presentation stabilizes further.

The safest first step is planning character / enemy data, because the relevant fields are already visible on prefabs and have passed GridTest regression.

## 7. Data That Should Not Be Assetized Yet

The following should remain deferred to avoid premature abstraction:

- Runtime state values such as current HP, current cooldowns, moved / acted / ended, active bleed, active stun, and active defense state.
- Stable small runtime service references that currently work in GridTest.
- Formal level structure before Phase 15.12 defines it.
- BossDoor / BossKey / SupplyPoint rules.
- LAN / host-authoritative command data.
- Animation polish override data that is still likely to change.
- Chest loot, door lock / unlock, and final reward logic.
- Tile occupancy and current tile runtime state.

## 8. Proposed Future Data Types

### 8.1 CharacterData

Responsibility:

- Define player character identity, base gameplay configuration, and references needed to instantiate or validate a player unit.

Suggested fields:

- id
- display name
- role / class
- prefab reference
- base stats
- move range
- basic attack range
- starting potion count
- skill loadout
- visual data reference
- animation profile reference

Likely sources:

- Current player prefabs.
- `Unit`, `UnitStats`, `UnitPotionState`, and `SkillRuntime`.
- Current SkillData references.

Target path:

- `Assets/_BoneThrone/Data/Characters/`

### 8.2 EnemyData

Responsibility:

- Define enemy identity, combat configuration, future encounter weighting, and visual profile reference.

Suggested fields:

- id
- enemy type
- prefab reference
- base stats
- basic attack range
- AI profile reference
- encounter weight
- visual data reference
- animation profile reference
- boss / elite flag

Likely sources:

- Current enemy and boss placeholder prefabs.
- `Unit`, `UnitStats`, enemy faction setup, and basic attack range.
- Phase 15.6 / 15.7 / 15.9.2 DevLogs.

Target path:

- `Assets/_BoneThrone/Data/Enemies/`

Important rule:

- `Skeleton_Golem_Boss` may be represented as boss / heavy boss data later.
- `Skeleton_Golem` must not become a normal enemy data entry.

### 8.3 UnitVisualData

Responsibility:

- Separate gameplay identity from visual source, equipment, sockets, and presentation offsets.

Suggested fields:

- visual prefab
- animator controller
- weapon visual references
- socket names
- health bar anchor
- selection marker offset
- scale defaults
- facing defaults

Likely sources:

- Current player, enemy, and boss prefabs.
- Phase 15.7 weapon / equipment attachment DevLog.
- Phase 15.9.2 per-unit Animator Controller bindings.

Target path:

- `Assets/_BoneThrone/Data/Visuals/`

### 8.4 LevelData

Responsibility:

- Describe a formal level after Phase 15.12 defines level structure.

Suggested fields:

- level id
- scene reference
- room list
- entry room
- exit room
- level index
- key requirement
- boss requirement
- progression target
- reward table reference

Likely sources:

- Future Phase 15.12 formal level plan.
- Current `LevelManager` and `LevelProgressionService` as reference only.

Target path:

- `Assets/_BoneThrone/Data/Levels/`

Do not create `LevelData` instances before formal level structure is known.

### 8.5 RoomData

Responsibility:

- Describe formal room metadata, encounter hooks, exits, and placement constraints after level planning.

Suggested fields:

- room id
- room type
- room bounds
- tile layout reference
- shadow / fog setting
- interactables
- encounter reference
- exits
- door references

Likely sources:

- Current `RoomController`, `RoomShadowController`, and `RoomEnemyActivator`.
- Future formal level room plan.

Target path:

- `Assets/_BoneThrone/Data/Rooms/`

### 8.6 EncounterData

Responsibility:

- Define enemy groups and spawn data for formal rooms.

Suggested fields:

- encounter id
- enemy groups
- spawn positions / spawn markers
- wave settings
- activation rule
- reward reference
- difficulty tag

Likely sources:

- `RoomEnemyActivator.assignedEnemies`
- `RoomEnemyActivator.spawnTiles`
- Future room / level layout.

Target path:

- `Assets/_BoneThrone/Data/Encounters/`

### 8.7 InteractableData

Responsibility:

- Define interactable gameplay-facing configuration after rules stabilize.

Suggested fields:

- interactable id
- prefab reference
- interaction type
- required key / condition
- pickup amount
- prompt text
- one-shot / repeatable flag
- target progression effect

Likely sources:

- `HealthPotionPickup`
- `KeyItem`
- `InteractableStairs`
- Phase 15.5 interactable prefab review

Target path:

- `Assets/_BoneThrone/Data/Interactables/`

### 8.8 RewardData

Responsibility:

- Define future reward grants after reward rules are approved.

Suggested fields:

- reward id
- gold / item / potion reward
- unlock reference
- probability / weight
- reward tier

Likely sources:

- Future chest, supply, and room reward rules.
- Current potion / key progression can inform early planning, but should not force reward data too soon.

Target path:

- `Assets/_BoneThrone/Data/Rewards/`

### 8.9 ProgressionData

Responsibility:

- Define floor ordering and progression conditions after formal level structure exists.

Suggested fields:

- floor index
- level order
- key progression
- stair target
- unlock condition
- victory condition
- defeat condition

Likely sources:

- `LevelProgressionService`
- `LevelManager`
- Future Phase 15.12 / 15.13 level structure

Target path:

- `Assets/_BoneThrone/Data/Progression/`

### 8.10 AnimationProfileData

Responsibility:

- Document or configure per-unit animation mappings after controller behavior stabilizes.

Suggested fields:

- idle clip
- move clip
- basic attack clip
- skill cast clip
- hit clip
- defend start clip
- defend hold clip
- use potion clip
- dead clip
- animator controller reference
- parameter name overrides if needed

Likely sources:

- Phase 15.9.2 per-unit Animator Controller mapping.
- Current Animator Controllers.
- `UnitAnimationController` parameter contract.

Target path:

- `Assets/_BoneThrone/Data/AnimationProfiles/`

This should remain a later polish-oriented data type unless formal production needs require it earlier.

## 9. Data Source Table

| Data Type | Current Source | Future Target Path | Required Before Phase 15.12? | Deferred |
| --- | --- | --- | --- | --- |
| `CharacterData` | Player prefabs, `Unit`, `UnitStats`, `SkillRuntime`, SkillData | `Assets/_BoneThrone/Data/Characters/` | No, planning only | No |
| `EnemyData` | Enemy / Boss prefabs, `Unit`, `UnitStats`, Phase 15.6 notes | `Assets/_BoneThrone/Data/Enemies/` | No, planning only | No |
| `UnitVisualData` | Unit prefabs, weapon sockets, Animator bindings, DevLogs | `Assets/_BoneThrone/Data/Visuals/` | No | Partial |
| `LevelData` | `LevelManager`, `LevelProgressionService`, future level plan | `Assets/_BoneThrone/Data/Levels/` | No | Yes, until Phase 15.12 / 15.13 |
| `RoomData` | `RoomController`, `RoomShadowController`, scene room setup | `Assets/_BoneThrone/Data/Rooms/` | No | Yes, until formal room plan |
| `EncounterData` | `RoomEnemyActivator`, assigned enemies, spawn tiles | `Assets/_BoneThrone/Data/Encounters/` | No | Yes, until formal room plan |
| `InteractableData` | `HealthPotionPickup`, `KeyItem`, `InteractableStairs`, prefabs | `Assets/_BoneThrone/Data/Interactables/` | No | Partial |
| `RewardData` | Potion / key behavior, future chest / supply rules | `Assets/_BoneThrone/Data/Rewards/` | No | Yes |
| `ProgressionData` | `LevelProgressionService`, `LevelManager`, scene references | `Assets/_BoneThrone/Data/Progression/` | No | Yes |
| `AnimationProfileData` | Animator Controllers, `UnitAnimationController`, DevLogs | `Assets/_BoneThrone/Data/AnimationProfiles/` | No | Yes, polish later |
| Existing `SkillData` | `Assets/_BoneThrone/Data/Skills/` and `OnlyTest/Skills` | Existing path remains valid | Already exists | No migration in Phase 15.11 |

## 10. Target Path Recommendations

Future paths:

- `Assets/_BoneThrone/Data/Characters/`
- `Assets/_BoneThrone/Data/Enemies/`
- `Assets/_BoneThrone/Data/Visuals/`
- `Assets/_BoneThrone/Data/Levels/`
- `Assets/_BoneThrone/Data/Rooms/`
- `Assets/_BoneThrone/Data/Encounters/`
- `Assets/_BoneThrone/Data/Interactables/`
- `Assets/_BoneThrone/Data/Rewards/`
- `Assets/_BoneThrone/Data/Progression/`
- `Assets/_BoneThrone/Data/AnimationProfiles/`

Do not create these directories in Phase 15.11 unless a later prompt explicitly changes the scope.

## 11. Recommended Data Assetization Order

### Batch 1 - CharacterData / EnemyData planning only

Plan the fields and source-of-truth rules for player and enemy identity / stats / skill loadouts.

Do not create assets yet.

### Batch 2 - UnitVisualData / AnimationProfileData planning only

Plan separation between gameplay identity and visuals.

Do not create assets yet.

### Batch 3 - RoomData / EncounterData after Phase 15.12 formal level plan

Wait until Phase 15.12 defines room count, room roles, layout needs, and encounter goals.

### Batch 4 - LevelData / ProgressionData after level structure is known

Wait until the formal floor structure is stable.

### Batch 5 - InteractableData / RewardData after interactable rules are stable

Wait until chest, door, supply, key, stairs, and reward rules are better defined.

## 12. Work Reserved for Phase 15.12 / 15.13+

Do not implement these in Phase 15.11:

- `LevelData` instances
- `RoomData` instances
- `EncounterData` instances
- spawn marker ids
- formal Level scene references
- formal floor progression data

These should follow the formal level plan and scene setup phases.

## 13. Deferred Content

The following are explicitly deferred:

- BossDoor / BossKey
- SupplyPoint
- Chest loot
- Door lock / unlock
- LAN / host-authoritative command data
- Boss AI / Boss skill data
- animation polish override data
- formal victory / defeat / retry data

## 14. Risks

- Premature assetization can make a small stable project harder to reason about.
- Prefab and data double-source conflicts can occur if both own the same stats.
- SkillData and CharacterData responsibilities can overlap around skill loadouts and unlock rules.
- Scene references may be lost if scene-bound service wiring is migrated too early.
- GridTest regression behavior could be disrupted by unnecessary data plumbing.
- LevelData may become obsolete if created before formal level planning.
- AnimationProfileData and Animator Controller responsibilities may become confused if animation mapping is abstracted before polish needs are clear.

## 15. Execution Principles

- Phase 15.11 is planning only.
- Phase 15.12 should first define the formal level scene plan.
- Phase 15.13+ should decide whether to create `LevelData`, `RoomData`, and `EncounterData` based on actual level construction needs.
- Do not refactor stable GridTest behavior just to introduce data assets.
- Preserve prefabs, scene wiring, and runtime behavior that already passed regression.
- Data assetization must serve formal level production, not abstraction for its own sake.

## 16. Explicit Prohibitions

- Do not create ScriptableObject assets.
- Do not modify SkillData.
- Do not modify prefabs.
- Do not modify scenes.
- Do not modify C# code.
- Do not modify KayKit source.
- Do not create formal Level scenes.
- Do not implement Boss, LAN, or Level progression.
- Do not change the single-player free-order PlayerTurn rule.
- Do not use Fighter -> Ranger -> Mage -> Barbarian fixed-order for single-player.

## 17. Phase 15.11 Output

This document is the Phase 15.11 formal data assetization plan.

It intentionally produces no runtime asset, no ScriptableObject instance, no code migration, and no scene / prefab changes.
