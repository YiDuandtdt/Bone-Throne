# Phase 15.6 - Character Prefab Completion

## Summary

Phase 15.6 reviewed the existing player and enemy Unit prefabs, then completed the character prefab set with:

- `Skeleton_Necromancer` as an additional Enemy prefab.
- `Skeleton_Golem_Boss` as a Boss / heavy boss placeholder.

The new prefabs follow the existing project-owned Unit prefab structure:

- Root project prefab under `Assets/_BoneThrone/Prefabs/Units/`
- `Unit`
- `UnitTurnState`
- Kinematic `Rigidbody`
- Convex `MeshCollider`
- KayKit visual source prefab as child visual
- Enemy floating health bar prefab child for enemy / boss units

No C# code, scenes, materials, SkillData, KayKit source assets, Turn, Combat, Skill, Potion, LAN, weapon/equipment, animation controller, Boss AI, or formal level content was modified.

## Modified / Added Files

Added:

- `Assets/_BoneThrone/Prefabs/Units/Enemies/Skeleton_Necromancer.prefab`
- `Assets/_BoneThrone/Prefabs/Units/Enemies/Skeleton_Necromancer.prefab.meta`
- `Assets/_BoneThrone/Prefabs/Units/Boss.meta`
- `Assets/_BoneThrone/Prefabs/Units/Boss/Skeleton_Golem_Boss.prefab`
- `Assets/_BoneThrone/Prefabs/Units/Boss/Skeleton_Golem_Boss.prefab.meta`
- `Docs/DevLogs/Phase15.6_CharacterPrefabCompletion.md`

Reviewed only:

- `Assets/_BoneThrone/Prefabs/Units/Players/Fighter.prefab`
- `Assets/_BoneThrone/Prefabs/Units/Players/Ranger.prefab`
- `Assets/_BoneThrone/Prefabs/Units/Players/Mage.prefab`
- `Assets/_BoneThrone/Prefabs/Units/Players/Barbarian.prefab`
- `Assets/_BoneThrone/Prefabs/Units/Enemies/Skeleton_Minion.prefab`
- `Assets/_BoneThrone/Prefabs/Units/Enemies/Skeleton_Warrior.prefab`
- `Assets/_BoneThrone/Prefabs/Units/Enemies/Skeleton_Rogue.prefab`
- `Assets/_BoneThrone/Prefabs/Units/Enemies/Skeleton_Mage.prefab`

## Player Prefab Status

| Prefab | Source Visual | UnitId | RoleId | Faction | Status |
| --- | --- | ---: | ---: | ---: | --- |
| `Fighter.prefab` | `Assets/_BoneThrone/Art/KayKit - Adventurers (for Unity)/Prefabs/Characters/Knight.prefab` | 1 | Fighter | Player | Reviewed; no change |
| `Ranger.prefab` | `Assets/_BoneThrone/Art/KayKit - Adventurers (for Unity)/Prefabs/Characters/Rogue.prefab` | 2 | Ranger | Player | Reviewed; no change |
| `Mage.prefab` | `Assets/_BoneThrone/Art/KayKit - Adventurers (for Unity)/Prefabs/Characters/Mage.prefab` | 3 | Mage | Player | Reviewed; no change |
| `Barbarian.prefab` | `Assets/_BoneThrone/Art/KayKit - Adventurers (for Unity)/Prefabs/Characters/Barbarian.prefab` | 4 | Barbarian | Player | Reviewed; no change |

Notes:

- Player gameplay identities remain `Fighter`, `Ranger`, `Mage`, and `Barbarian`.
- Ranger gameplay identity remains `Ranger` even though its visual source is `Rogue`.
- Existing player prefabs already contain `Unit`, `UnitTurnState`, `SkillRuntime`, kinematic `Rigidbody`, and convex `MeshCollider`.
- Weapon / equipment attachment is deferred to Phase 15.7.
- Animator Controller and animation state machines are deferred to Phase 15.8.

## Enemy Prefab Status

| Prefab | Source Visual | UnitId | RoleId | Faction | Status |
| --- | --- | ---: | ---: | ---: | --- |
| `Skeleton_Minion.prefab` | `Assets/_BoneThrone/Art/KayKit - Skeletons (for Unity)/Prefabs/Characters/Skeleton_Minion.prefab` | 1001 | Enemy | Enemy | Reviewed; no change |
| `Skeleton_Warrior.prefab` | `Assets/_BoneThrone/Art/KayKit - Skeletons (for Unity)/Prefabs/Characters/Skeleton_Warrior.prefab` | 1002 | Enemy | Enemy | Reviewed; no change |
| `Skeleton_Mage.prefab` | `Assets/_BoneThrone/Art/KayKit - Skeletons (for Unity)/Prefabs/Characters/Skeleton_Mage.prefab` | 1003 | Enemy | Enemy | Reviewed; no change |
| `Skeleton_Rogue.prefab` | `Assets/_BoneThrone/Art/KayKit - Skeletons (for Unity)/Prefabs/Characters/Skeleton_Rogue.prefab` | 1004 | Enemy | Enemy | Reviewed; no change |
| `Skeleton_Necromancer.prefab` | `Assets/_BoneThrone/Art/KayKit - Skeletons (for Unity)/Prefabs/Characters/Skeleton_Necromancer.prefab` | 1005 | Enemy | Enemy | Added |

Notes:

- Existing enemy prefabs already contain `Unit`, `UnitTurnState`, kinematic `Rigidbody`, convex `MeshCollider`, KayKit visual child, and `EnemyFloatingHealthBar` child.
- `Skeleton_Necromancer.prefab` was created from the existing project enemy prefab structure and points to the actual KayKit `Skeleton_Necromancer` source visual.
- No EnemyAIController, EnemyTurnRunner, CombatSystem, SkillData, or AI behavior was modified.

## Boss / Heavy Boss Placeholder

Created:

- `Assets/_BoneThrone/Prefabs/Units/Boss/Skeleton_Golem_Boss.prefab`

Source visual:

- `Assets/_BoneThrone/Art/KayKit - Skeletons (for Unity)/Prefabs/Characters/Skeleton_Golem.prefab`

Role:

- Boss / heavy boss placeholder only.

Notes:

- `Skeleton_Golem_Boss` uses the existing project enemy prefab structure.
- It is placed under `Assets/_BoneThrone/Prefabs/Units/Boss/`, not under normal enemies.
- It is not introduced as a normal enemy prefab.
- It does not implement Boss AI, Boss door, Boss key, boss encounter logic, or formal level placement.

## Naming Rules

- Player gameplay roles remain `Fighter`, `Ranger`, `Mage`, and `Barbarian`.
- `Ranger` remains the gameplay identity, even when using Rogue visual.
- `Skeleton_Rogue` remains the normal skeleton Rogue enemy naming.
- `Skeleton_Golem` is not used as a normal enemy prefab.
- `Skeleton_Golem_Boss` is a Boss / heavy boss placeholder only.

## Non-Changes

This phase did not modify:

- KayKit original resources
- C# code
- Scenes
- Materials
- SkillData
- `GridTest.unity`
- TurnManager / Turn system
- CombatSystem
- SkillSystem / SkillData
- PotionSystem
- LAN / Networking
- Environment prefabs
- Interactable prefabs
- Weapons / Equipment
- Animations / Animator Controllers
- Boss AI
- Formal Level scenes

## Deferred Work

- Weapon, shield, bow, staff, axe, and equipment attachment -> Phase 15.7.
- Hand sockets and attachment placement -> Phase 15.7.
- Animator Controllers and animation state machines -> Phase 15.8.
- Animation integration with movement, combat, skills, and potion -> Phase 15.9.
- Boss AI, Boss Door, Boss Key, and SupplyPoint preparation -> later dedicated phases.
- Formal Level scene placement -> Phase 15.13 and later.

## Unity Play Mode

Unity Play Mode was not run because this phase only created and reviewed prefab assets without changing runtime code, scenes, SkillData, materials, or systems.

Recommended Unity 6.3 checks:

1. Open `Assets/_BoneThrone/Prefabs/Units/Players/`.
2. Confirm `Fighter`, `Ranger`, `Mage`, and `Barbarian` have correct `Unit` role and Player faction.
3. Open `Assets/_BoneThrone/Prefabs/Units/Enemies/`.
4. Confirm `Skeleton_Minion`, `Skeleton_Warrior`, `Skeleton_Rogue`, `Skeleton_Mage`, and `Skeleton_Necromancer` have Enemy faction, `UnitTurnState`, collider, and health bar child.
5. Open `Assets/_BoneThrone/Prefabs/Units/Boss/Skeleton_Golem_Boss.prefab`.
6. Confirm it is under Boss, not normal Enemies, and has no Boss AI or special logic.
7. Confirm no KayKit source prefab is dirty.
8. If prefabs are temporarily dragged into a scene for visual inspection, do not save the scene.

## Risks

- New `Skeleton_Necromancer` and `Skeleton_Golem_Boss` use the current enemy prefab structure but should be visually inspected in Unity for scale, pivot, collider fit, and health bar offset.
- `Skeleton_Golem_Boss` has placeholder combat stats only and must not be treated as a finished boss encounter.
- Weapon and equipment visuals are intentionally absent until Phase 15.7.
- Animator Controller and animation behavior are intentionally deferred until Phase 15.8.

## Rollback

Rollback Phase 15.6 additions by removing the new character prefab files and this DevLog:

```powershell
Remove-Item -LiteralPath "Assets/_BoneThrone/Prefabs/Units/Enemies/Skeleton_Necromancer.prefab" -Force
Remove-Item -LiteralPath "Assets/_BoneThrone/Prefabs/Units/Enemies/Skeleton_Necromancer.prefab.meta" -Force
Remove-Item -LiteralPath "Assets/_BoneThrone/Prefabs/Units/Boss" -Recurse -Force
Remove-Item -LiteralPath "Assets/_BoneThrone/Prefabs/Units/Boss.meta" -Force
Remove-Item -LiteralPath "Docs/DevLogs/Phase15.6_CharacterPrefabCompletion.md" -Force
```

Before rollback, run:

```powershell
git status --short
```

Only remove files created by Phase 15.6.
