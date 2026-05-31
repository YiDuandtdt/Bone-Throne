# Phase 15.9.1 - Basic Attack Range Configuration Pass

## Goal

Make Basic Attack Range an explicit per-unit gameplay configuration instead of a single shared `AttackRangeService` value.

This pass affects Basic Attack legality only. It does not change SkillData range, skill resolution, animation, weapon visuals, turn flow, potion rules, or LAN / Networking.

## Modified Files

- `Assets/_BoneThrone/Scripts/Units/UnitStats.cs`
- `Assets/_BoneThrone/Scripts/Combat/AttackRangeService.cs`
- `Assets/_BoneThrone/Scripts/Combat/CombatSystem.cs`
- `Assets/_BoneThrone/Prefabs/Units/Players/Fighter.prefab`
- `Assets/_BoneThrone/Prefabs/Units/Players/Ranger.prefab`
- `Assets/_BoneThrone/Prefabs/Units/Players/Mage.prefab`
- `Assets/_BoneThrone/Prefabs/Units/Players/Barbarian.prefab`
- `Assets/_BoneThrone/Prefabs/Units/Enemies/Skeleton_Minion.prefab`
- `Assets/_BoneThrone/Prefabs/Units/Enemies/Skeleton_Warrior.prefab`
- `Assets/_BoneThrone/Prefabs/Units/Enemies/Skeleton_Rogue.prefab`
- `Assets/_BoneThrone/Prefabs/Units/Enemies/Skeleton_Mage.prefab`
- `Assets/_BoneThrone/Prefabs/Units/Enemies/Skeleton_Necromancer.prefab`
- `Assets/_BoneThrone/Prefabs/Units/Boss/Skeleton_Golem_Boss.prefab`
- `Docs/DevLogs/Phase15.9.1_BasicAttackRangeConfiguration.md`

## UnitStats

`UnitStats` now has:

- `basicAttackRange`, serialized, default `1`
- `BasicAttackRange`, read-only property clamped to at least `1`

Existing `maxHp`, `moveRange`, `attackModifier`, `defense`, and `baseDamage` behavior was not changed.

## AttackRangeService

`AttackRangeService` remains the central Basic Attack distance service.

The existing service-level `basicAttackRange` remains as a fallback when the attacker or attacker stats are missing.

New read path:

- `GetBasicAttackRange(Unit attacker)` returns `attacker.Stats.BasicAttackRange` when available.
- `IsInBasicAttackRange(attacker, target)` compares Manhattan distance against the attacker-specific range.
- The Manhattan distance algorithm was not changed.

## CombatSystem

`CombatSystem.CanBasicAttack()` and `CombatSystem.TryBasicAttack()` still use `AttackRangeService`.

Only rejection reason text was updated so `Range=` reports the attacker-specific Basic Attack Range instead of the service fallback.

D20 roll, hit check, damage, MarkActed, turn flow, and animation trigger behavior were not changed.

## Prefab Basic Attack Range Configuration

| Prefab | Basic Attack Range |
| --- | ---: |
| `Fighter.prefab` | 1 |
| `Ranger.prefab` | 4 |
| `Mage.prefab` | 3 |
| `Barbarian.prefab` | 1 |
| `Skeleton_Minion.prefab` | 1 |
| `Skeleton_Warrior.prefab` | 1 |
| `Skeleton_Rogue.prefab` | 2 |
| `Skeleton_Mage.prefab` | 3 |
| `Skeleton_Necromancer.prefab` | 3 |
| `Skeleton_Golem_Boss.prefab` | 2 |

These are gameplay configuration values. They are not inferred from weapon visual attachments.

## Boundaries Preserved

- No `SkillData` changes.
- No `SkillSystem` or `SkillTargetingService` changes.
- No animation controller changes.
- No weapon visual changes.
- No TurnManager changes.
- No PotionSystem changes.
- No KayKit source changes.
- No GridTest / scene changes.
- No formal level scene work.
- No LAN / Networking work.
- Single-player PlayerTurn free-order rule remains unchanged.

## Unity 6.3 Play Mode Test Steps

1. Open `Assets/_BoneThrone/Scenes/GridTest.unity`.
2. Enter Play Mode.
3. Select `Fighter` and confirm Basic Attack is valid at distance 1 and rejected beyond distance 1.
4. Select `Barbarian` and confirm Basic Attack is valid at distance 1 and rejected beyond distance 1.
5. Select `Ranger` and confirm Basic Attack can target enemies up to distance 4.
6. Select `Mage` and confirm Basic Attack can target enemies up to distance 3.
7. Let EnemyTurn run and confirm skeleton enemies attack or move based on their configured Basic Attack Range.
8. Confirm `Skeleton_Rogue` can use range 2, `Skeleton_Mage` / `Skeleton_Necromancer` can use range 3, and `Skeleton_Golem_Boss` can use range 2 when present.
9. Confirm Skill ranges remain controlled by `SkillData.Range`.
10. Confirm UI basic attack highlights follow `CombatSystem.CanBasicAttack()`.

## Risks

- Existing scene instances may not inherit prefab stat changes if they are unpacked or have overrides.
- Enemy AI can attack from farther away for ranged enemies, which is intended configuration behavior but should be regression-tested.
- Combat rejection logs now rely on attacker stats, so missing stats fall back to the service default.

## Rollback

```powershell
git restore -- Assets/_BoneThrone/Scripts/Units/UnitStats.cs Assets/_BoneThrone/Scripts/Combat/AttackRangeService.cs Assets/_BoneThrone/Scripts/Combat/CombatSystem.cs Assets/_BoneThrone/Prefabs/Units/Players Assets/_BoneThrone/Prefabs/Units/Enemies Assets/_BoneThrone/Prefabs/Units/Boss Docs/DevLogs/Phase15.9.1_BasicAttackRangeConfiguration.md
```
