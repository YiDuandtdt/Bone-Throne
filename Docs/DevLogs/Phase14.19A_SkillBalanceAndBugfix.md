# Phase 14.19-A - Skill Balance and Bugfix

## Goal
Apply the Phase 14.19-A balance pass and fix knockback position sync while keeping the Phase 14.19 skill rebuild architecture intact.

## Actual Modified Files
- `Assets/_BoneThrone/Scripts/Skills/SkillEffectExecutor.cs`
- `Assets/_BoneThrone/Scripts/Skills/SkillKnockbackUtility.cs`
- `Assets/_BoneThrone/Scripts/Combat/UnitBleedState.cs`
- `Assets/_BoneThrone/Scripts/Combat/UnitStunState.cs`
- `Assets/_BoneThrone/Scripts/Combat/CombatLog.cs`
- `Assets/_BoneThrone/Scripts/Turns/ActionPermissionService.cs`
- `Assets/_BoneThrone/Scripts/AI/EnemyTurnRunner.cs`
- `Assets/_BoneThrone/Scripts/Movement/PlayerMovementController.cs`
- `Assets/_BoneThrone/Data/Skills/mage_fireball.asset`
- `Assets/_BoneThrone/Data/Skills/mage_frost_bolt.asset`
- `Assets/_BoneThrone/Data/Skills/mage_arcane_burst.asset`

## Knockback Fix
`SkillKnockbackUtility` now resolves `UnitMover` and sends a one-step path from the target tile to the destination tile. This keeps the unit tile reference, tile occupancy, and world transform synchronized through the existing movement API. Blocked, occupied, missing, or invalid destination tiles still only block knockback; the skill damage remains applied.

## Final Skill Values
- `fighter_shield_bash`: 3 damage, knockback 1 tile.
- `fighter_guard_strike`: 5 damage, DamageAmplify +1.
- `fighter_crushing_challenge`: 5 damage, Stun.
- `ranger_precision_shot`: 5 damage.
- `ranger_quick_shot`: 3 damage, Bleed 2 stacks.
- `ranger_piercing_arrow`: 6 primary damage, 3 behind-target secondary damage.
- `mage_fireball`: 3 primary damage, 1 adjacent splash damage, cooldown 1.
- `mage_frost_bolt`: 2 damage, Stun, range 5, cooldown 2.
- `mage_arcane_burst`: range 4, 5 primary damage, 2 adjacent splash damage, DamageAmplify +2.
- `barbarian_heavy_cleave`: 5 + floor(lost HP * 0.1) damage, minimum 5.
- `barbarian_rage_strike`: 4 damage and Bleed 3 stacks, doubled to 8 damage and 6 stacks at or below half HP.
- `barbarian_blood_fury_slash`: 4 damage, or 6 at or below half HP, Bleed 2 stacks.

## Mage SkillData Asset Changes
- `mage_fireball.asset`: `cooldownTurns` 2 -> 1.
- `mage_frost_bolt.asset`: `range` 3 -> 5, `cooldownTurns` 1 -> 2.
- `mage_arcane_burst.asset`: `range` 3 -> 4.

No skill id, display name, unlock level, target type, GUID, prefab, or scene binding was changed.

## Bleed Stacks Rule
`UnitBleedState` now stores stacks instead of fixed tick damage plus duration. Reapplying bleed refreshes to `max(currentStacks, newStacks)`. Each tick deals damage equal to the current stack count, then reduces stacks by 1. Player bleed still ticks once at new PlayerTurn start, and enemy bleed still ticks before that enemy acts.

## Stun Rule
Stun now skips both move and action for the next real opportunity. `CanMove` and `CanAct` remain pure query methods: they can return false for stunned units, but they do not consume stun, mark turn state, or write combat logs. Stun consumption stays in execution paths through `TryConsumeStunForAction`, which marks both moved and acted and logs the skip.

Enemy stun is consumed in `EnemyTurnRunner` before AI runs, so the enemy neither moves nor attacks. Player stun is consumed on real action attempts through Basic Attack, Skill, Defend, Potion, and on real movement tile clicks through `PlayerMovementController`; UI refresh and targeting preview do not consume stun.

## Damage Amplify
DamageResolver order remains:
1. Base damage.
2. DamageAmplify bonus.
3. Defend reduction.
4. HP apply.

DamageAmplify still triggers once and clears. Fighter applies +1, Mage Arcane Burst applies +2.

## Unmodified Systems
No prefab, scene, KayKit asset, OnlyTest skill asset, UI script, SkillSystem, SkillTargetingService, CombatSystem, player foot tile indicator, camera control, ActiveUnitProvider behavior, Ranger identity, Defend action economy, or Potion action economy was changed in this phase.

## Unity Play Mode Test Steps
1. Open `Assets/_BoneThrone/Scenes/GridTest.unity`.
2. Enter Play Mode and confirm player foot tiles remain white by default.
3. Use Fighter Shield Bash on an enemy with an open tile behind it; confirm damage applies and the enemy model moves to the destination tile.
4. Repeat Shield Bash with a blocked or occupied destination; confirm damage applies and knockback logs blocked without moving the model.
5. Verify each rebuilt skill value listed above in CombatLog and HP changes.
6. Apply Ranger Quick Shot and confirm bleed ticks 2 then 1 damage on later unit opportunities.
7. Apply Barbarian Rage Strike above and below half HP; confirm 4/3-stack and 8/6-stack variants.
8. Apply Mage Arcane Burst and confirm 5 primary, 2 splash, and DamageAmplify +2 on the primary target.
9. Stun an enemy and advance to EnemyTurn; confirm it skips both movement and attack.
10. Stun a player, then attempt Move, Basic Attack, Skill, Defend, or Potion; confirm the first real attempt consumes stun and blocks both move and action without preview consumption.
11. Confirm Phase 14.15 free player order, End Turn, Defend, Potion, cooldown, and Skill Slot 0/1/2 availability still work.

## Risk and Rollback
Risk is mainly in the stun timing and knockback path. If knockback causes unexpected movement behavior, revert `SkillKnockbackUtility.cs` to the previous tile-only placement path. If stun feels too punitive or does not clear at the desired point, revert `ActionPermissionService.cs`, `EnemyTurnRunner.cs`, and `PlayerMovementController.cs`. If balance values need another pass, revert only `SkillEffectExecutor.cs` and the three Mage SkillData assets.
