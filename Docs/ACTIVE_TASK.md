# ACTIVE_TASK.md

## Current phase
Phase 14.19-A - Skill Rebuild Balance and Bugfix

## Goal
Apply the Phase 14.19-A skill balance pass, fix knockback transform sync, update bleed to stacks, and make stun skip both move and action without adding new systems.

Allowed effect families:

- Damage
- Stun
- Damage Amplify
- Knockback
- Bleed

## Allowed files
- `Assets/_BoneThrone/Scripts/Skills/SkillEffectExecutor.cs`
- `Assets/_BoneThrone/Scripts/Skills/SkillKnockbackUtility.cs`
- `Assets/_BoneThrone/Scripts/Skills/SkillKnockbackUtility.cs.meta`
- `Assets/_BoneThrone/Scripts/Combat/CombatLog.cs`
- `Assets/_BoneThrone/Scripts/Combat/UnitStunState.cs`
- `Assets/_BoneThrone/Scripts/Combat/UnitBleedState.cs`
- `Assets/_BoneThrone/Scripts/Turns/ActionPermissionService.cs`
- `Assets/_BoneThrone/Scripts/AI/EnemyTurnRunner.cs`
- `Assets/_BoneThrone/Scripts/Movement/PlayerMovementController.cs`
- `Assets/_BoneThrone/Data/Skills/mage_fireball.asset`
- `Assets/_BoneThrone/Data/Skills/mage_frost_bolt.asset`
- `Assets/_BoneThrone/Data/Skills/mage_arcane_burst.asset`
- `Docs/DevLogs/Phase14.19A_SkillBalanceAndBugfix.md`
- `Docs/ACTIVE_TASK.md`

## Forbidden changes
- Do not modify SkillData assets except the three listed formal Mage asset `range` / `cooldownTurns` fields.
- Do not modify Player prefabs.
- Do not modify enemy prefabs.
- Do not modify scene files, including `GridTest.unity`.
- Do not modify `SkillSystem.cs`.
- Do not modify `SkillTargetingService.cs`.
- Do not modify `CombatSystem.cs`.
- Do not modify UI scripts.
- Do not modify KayKit original assets.
- Do not modify `Skeleton_Rogue` or `Skeleton_Golem`.
- Do not modify Ranger visual or identity.
- Do not modify Defend / Potion action economy.
- Do not modify End Turn / free player turn order.
- Do not modify player foot tile indicator.
- Do not modify camera controls.
- Do not modify `ActiveUnitProvider` behavior.
- Do not introduce mana, skill trees, inventory, extra movement, slow, shield, lifesteal, summon, taunt AI, counterattack, guard ally, complex status framework, complex line targeting, target-tile casting, networking, initiative, AP, or behavior trees.

## Validation
Implementation phase.

Manual checks:

1. Confirm only allowed files changed.
2. Confirm no scene, prefab, OnlyTest SkillData, Packages, ProjectSettings, Library, Temp, Obj, Logs, or UserSettings changes.
3. Let Unity import new scripts and compile.
4. Open `Assets/_BoneThrone/Scenes/GridTest.unity`.
5. Enter Play Mode and validate knockback movement sync, bleed stack ticks, stun skip move/action, all 12 final skill values, and Phase 14.15 / 14.17 regressions.
