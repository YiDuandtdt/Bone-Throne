# Phase 14.19 - Skill Rebuild Implementation

## Scope

Phase 14.19 rebuilds the current 12 formal skill effects using only:

- Damage
- Stun
- Damage Amplify
- Knockback
- Bleed

The phase does not modify SkillData assets, player prefabs, enemy prefabs, scenes, UI scripts, `SkillSystem`, `SkillTargetingService`, `CombatSystem`, KayKit assets, Ranger identity, free player turn order, End Turn, player foot tile indicators, camera controls, or `ActiveUnitProvider` behavior.

## Files changed

- `Assets/_BoneThrone/Scripts/Skills/SkillEffectExecutor.cs`
- `Assets/_BoneThrone/Scripts/Skills/SkillKnockbackUtility.cs`
- `Assets/_BoneThrone/Scripts/Skills/SkillKnockbackUtility.cs.meta`
- `Assets/_BoneThrone/Scripts/Combat/DamageResolver.cs`
- `Assets/_BoneThrone/Scripts/Combat/CombatLog.cs`
- `Assets/_BoneThrone/Scripts/Combat/UnitStunState.cs`
- `Assets/_BoneThrone/Scripts/Combat/UnitStunState.cs.meta`
- `Assets/_BoneThrone/Scripts/Combat/UnitBleedState.cs`
- `Assets/_BoneThrone/Scripts/Combat/UnitBleedState.cs.meta`
- `Assets/_BoneThrone/Scripts/Combat/UnitDamageAmplifyState.cs`
- `Assets/_BoneThrone/Scripts/Combat/UnitDamageAmplifyState.cs.meta`
- `Assets/_BoneThrone/Scripts/Turns/TurnManager.cs`
- `Assets/_BoneThrone/Scripts/Turns/ActionPermissionService.cs`
- `Assets/_BoneThrone/Scripts/AI/EnemyTurnRunner.cs`
- `Docs/ACTIVE_TASK.md`
- `Docs/DevLogs/Phase14.19_SkillRebuildImplementation.md`

## New lightweight status files

Added:

- `UnitStunState`
- `UnitBleedState`
- `UnitDamageAmplifyState`

These are focused per-unit MonoBehaviours, not a general buff/status framework.

## Final 12 skill effects

| Skill id | Final effect |
| --- | --- |
| `fighter_shield_bash` | Damage 3, then attempt Knockback 1 tile away from caster. |
| `fighter_guard_strike` | Damage 2, apply Damage Amplify +1. |
| `fighter_crushing_challenge` | Damage 4, apply Stun. |
| `ranger_precision_shot` | Damage 5. |
| `ranger_quick_shot` | Damage 2, apply Bleed for 1 turn, 1 damage per tick. |
| `ranger_piercing_arrow` | Primary Damage 3; if enemy exists one tile behind target in caster -> target direction, secondary Damage 2. |
| `mage_fireball` | Primary Damage 3, adjacent active alive enemy splash Damage 1. |
| `mage_frost_bolt` | Damage 2, apply Stun. |
| `mage_arcane_burst` | Primary Damage 3, adjacent active alive enemy splash Damage 1, apply Damage Amplify +1 to primary target. |
| `barbarian_heavy_cleave` | Damage 5. |
| `barbarian_rage_strike` | Damage 3, apply Bleed for 1 turn, 1 damage per tick. |
| `barbarian_blood_fury_slash` | Damage 4, or 6 if caster HP is at or below half max HP; apply Bleed for 2 turns, 1 damage per tick. |

Unknown skill fallback is preserved.

## Stun

`UnitStunState` stores one pending stun.

`ActionPermissionService.CanAct(...)` consumes stun when a unit attempts its next action:

- logs stun consumed
- marks the unit acted
- returns false

Stunned units may still move. Stun affects both player and enemy units because both action paths use the action permission service.

## Bleed

`UnitBleedState` stores remaining turns and tick damage.

Rules:

- non-stacking
- reapply refreshes duration and damage
- tick damage is 1 for rebuilt skills
- tick damage uses `DamageResolver.ApplyDamage(...)`

Tick timing:

- Player bleed ticks once in `TurnManager.StartPlayerRound()` when a new PlayerTurn begins.
- Enemy bleed ticks once in `EnemyTurnRunner` immediately before that enemy attempts its action.

## Damage Amplify

`UnitDamageAmplifyState` stores one pending incoming damage bonus.

`DamageResolver.ApplyDamage(...)` order is:

1. base damage
2. consume Damage Amplify and add bonus
3. apply Defend reduction
4. apply HP damage and death behavior

Damage Amplify clears after one incoming damage event and logs amplified damage.

## Knockback

`SkillKnockbackUtility.TryKnockbackOneTile(...)` performs minimal one-tile knockback:

- direction is based on caster tile to target tile
- if diagonal, the larger axis is used
- destination must exist and `CanEnter()`
- blocked, occupied, or missing destination means no movement
- no collision damage
- no pathfinding changes
- no direct transform mutation
- movement uses `Unit.TryPlaceOnTile(...)`

CombatLog records knockback success or blocked reason.

## CombatLog entries

Added structured entry types and methods for:

- Stun applied
- Stun consumed / action skipped
- Bleed applied
- Bleed tick
- Damage Amplify applied
- Damage amplified
- Knockback
- Knockback blocked

No string parsing was added.

## Validation notes

`dotnet build Assembly-CSharp.csproj` was attempted, but the generated Unity project file did not yet include the newly added Phase 14.19 scripts. The external build therefore failed to resolve `UnitStunState`, `UnitBleedState`, `UnitDamageAmplifyState`, and `SkillKnockbackUtility`.

The generated `.csproj` was not modified because it is outside the allowed file list and Unity normally regenerates it after importing new scripts.

Unity Editor compilation and Play Mode validation must be run after Unity imports the new scripts.

## Unity Play Mode test steps

1. Open `Assets/_BoneThrone/Scenes/GridTest.unity`.
2. Wait for Unity to import new scripts and confirm no compile errors.
3. Enter Play Mode.
4. Test `fighter_shield_bash`: confirm Damage 3 and one-tile knockback or blocked log.
5. Test `fighter_guard_strike`: confirm Damage 2 and next incoming damage +1.
6. Test `fighter_crushing_challenge`: confirm Damage 4 and target skips next action but can still move.
7. Test `ranger_precision_shot`: confirm Damage 5.
8. Test `ranger_quick_shot`: confirm Damage 2 and Bleed 1 tick.
9. Test `ranger_piercing_arrow`: confirm primary Damage 3 and secondary Damage 2 only behind target.
10. Test `mage_fireball`: confirm primary Damage 3 and adjacent splash Damage 1.
11. Test `mage_frost_bolt`: confirm Damage 2 and Stun.
12. Test `mage_arcane_burst`: confirm primary Damage 3, adjacent splash Damage 1, and Damage Amplify +1.
13. Test `barbarian_heavy_cleave`: confirm Damage 5.
14. Test `barbarian_rage_strike`: confirm Damage 3 and Bleed 1 tick.
15. Test `barbarian_blood_fury_slash`: confirm Damage 4, or 6 at/below half HP, and Bleed duration 2.
16. Confirm Bleed ticks for players at new PlayerTurn start.
17. Confirm Bleed ticks for enemies before their EnemyTurn action.
18. Confirm Defend reduction still works after Damage Amplify.
19. Confirm Skill Slot 0 / 1 / 2 UI, Defend, Potion, End Turn, free player turn order, player foot tile indicator, camera controls, ActiveUnitProvider, Room / Key / Stairs / LevelUp still work.
20. Confirm Console has no new red errors.

## Risks

- `ActionPermissionService.CanAct(...)` now consumes Stun and marks acted, so any future preview path must not call it.
- Bleed damage goes through `DamageResolver`, so Damage Amplify and Defend can affect bleed unless later design changes that order.
- Knockback uses current tile occupancy and `Unit.TryPlaceOnTile`; invalid occupancy state can block movement.
- External build validation depends on Unity regenerating the generated project file.

## Rollback

To roll back Phase 14.19:

1. Revert `Assets/_BoneThrone/Scripts/Skills/SkillEffectExecutor.cs`.
2. Delete `Assets/_BoneThrone/Scripts/Skills/SkillKnockbackUtility.cs` and `.meta`.
3. Revert `Assets/_BoneThrone/Scripts/Combat/DamageResolver.cs`.
4. Revert `Assets/_BoneThrone/Scripts/Combat/CombatLog.cs`.
5. Delete `Assets/_BoneThrone/Scripts/Combat/UnitStunState.cs` and `.meta`.
6. Delete `Assets/_BoneThrone/Scripts/Combat/UnitBleedState.cs` and `.meta`.
7. Delete `Assets/_BoneThrone/Scripts/Combat/UnitDamageAmplifyState.cs` and `.meta`.
8. Revert `Assets/_BoneThrone/Scripts/Turns/TurnManager.cs`.
9. Revert `Assets/_BoneThrone/Scripts/Turns/ActionPermissionService.cs`.
10. Revert `Assets/_BoneThrone/Scripts/AI/EnemyTurnRunner.cs`.
11. Revert `Docs/ACTIVE_TASK.md`.
12. Delete `Docs/DevLogs/Phase14.19_SkillRebuildImplementation.md`.

No SkillData, prefab, scene, UI, SkillSystem, SkillTargetingService, CombatSystem, KayKit, camera, ActiveUnitProvider, Defend/Potion action economy, End Turn, free player order, or foot tile rollback is needed because those systems were not changed.
