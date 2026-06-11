# Phase 15.8 - Character Animator Controllers and Animation State Machines

Date: 2026-05-29

## Goal

Phase 15.8 creates project-owned baseline Animator Controllers for player characters, Skeleton enemies, and the Skeleton Golem boss placeholder. This phase only prepares reusable animation state machines and binds controllers to the existing visual child `Animator` components on project-owned character prefabs.

No runtime animation integration was added.

## Created Controller Assets

- `Assets/_BoneThrone/Animation/Controllers/BT_Player_Medium.controller`
- `Assets/_BoneThrone/Animation/Controllers/BT_Skeleton_Medium.controller`
- `Assets/_BoneThrone/Animation/Controllers/BT_Boss_Large.controller`

Unity `.meta` files were generated for the new `Animation` folder, `Controllers` folder, and the three controller assets.

## Shared Animator Parameters

All three controllers use the same parameters:

- `MoveSpeed` float
- `BasicAttack` trigger
- `Skill` trigger
- `Hit` trigger
- `Defend` trigger
- `UsePotion` trigger
- `IsDead` bool

## Shared States

All three controllers contain the same base states:

- `Idle`
- `Move`
- `BasicAttack`
- `SkillCast`
- `Hit`
- `Defend`
- `UsePotion`
- `Dead`

State machine behavior:

- `Idle` is the default state.
- `MoveSpeed > 0.1` transitions from `Idle` to `Move`.
- `MoveSpeed < 0.1` transitions from `Move` to `Idle`.
- `BasicAttack`, `Skill`, `Hit`, `Defend`, and `UsePotion` triggers enter their matching states from Any State and return to `Idle` after playback.
- `IsDead == true` enters `Dead`.
- `Dead` has no automatic return transition.

No animation events were added.

## Controller Clip Sources

### BT_Player_Medium

- Idle: `Assets/_BoneThrone/Art/Animations/General/Idle.anim`
- Move: `Assets/_BoneThrone/Art/Animations/Movement/Walking_A.anim`
- BasicAttack: `Assets/_BoneThrone/Art/Animations/Combat/1H_Melee/1H_Melee_Attack_Slice_Horizontal.anim`
- SkillCast: `Assets/_BoneThrone/Art/Animations/Combat/Spellcasting/Spellcast_Shoot.anim`
- Hit: `Assets/_BoneThrone/Art/Animations/Combat/General/Hit_A.anim`
- Defend: `Assets/_BoneThrone/Art/Animations/Combat/Blocking/Block.anim`
- UsePotion: `Assets/_BoneThrone/Art/Animations/General/Use_Item.anim`
- Dead: `Assets/_BoneThrone/Art/Animations/Combat/General/Death_A_Pose.anim`

### BT_Skeleton_Medium

- Idle: `Assets/_BoneThrone/Art/Animations/Animations/Rig_Medium/Special/Skeletons_Idle.anim`
- Move: `Assets/_BoneThrone/Art/Animations/Animations/Rig_Medium/Special/Skeletons_Walking.anim`
- BasicAttack: `Assets/_BoneThrone/Art/Animations/Combat/1H_Melee/1H_Melee_Attack_Slice_Horizontal.anim`
- SkillCast: `Assets/_BoneThrone/Art/Animations/Combat/Spellcasting/Spellcast_Shoot.anim`
- Hit: `Assets/_BoneThrone/Art/Animations/Combat/General/Hit_A.anim`
- Defend: `Assets/_BoneThrone/Art/Animations/Combat/Blocking/Block.anim`
- UsePotion: `Assets/_BoneThrone/Art/Animations/General/Use_Item.anim`
- Dead: `Assets/_BoneThrone/Art/Animations/Animations/Rig_Medium/Special/Skeletons_Death_Pose.anim`

### BT_Boss_Large

- Idle: `Assets/_BoneThrone/Art/Animations/Animations/Rig_Large/General/Idle_A.anim`
- Move: `Assets/_BoneThrone/Art/Animations/Animations/Rig_Large/Movement Basic/Walking_A.anim`
- BasicAttack: `Assets/_BoneThrone/Art/Animations/Animations/Rig_Large/Combat Melee/Melee_2H_Attack.anim`
- SkillCast: `Assets/_BoneThrone/Art/Animations/Animations/Rig_Large/Combat Melee/Melee_2H_Slam.anim`
- Hit: `Assets/_BoneThrone/Art/Animations/Animations/Rig_Large/General/Hit_A.anim`
- Defend: `Assets/_BoneThrone/Art/Animations/Animations/Rig_Large/Combat Melee/Melee_Block.anim`
- UsePotion: `Assets/_BoneThrone/Art/Animations/Animations/Rig_Large/General/Idle_B.anim`
- Dead: `Assets/_BoneThrone/Art/Animations/Animations/Rig_Large/General/Death_A_Pose.anim`

## Prefab Controller Binding

The controllers were bound only to the visual child `Animator` inherited from the KayKit character source prefab instances. They were not added to the root gameplay objects.

Player prefabs:

- `Fighter.prefab` -> `BT_Player_Medium.controller`
- `Ranger.prefab` -> `BT_Player_Medium.controller`
- `Mage.prefab` -> `BT_Player_Medium.controller`
- `Barbarian.prefab` -> `BT_Player_Medium.controller`

Skeleton enemy prefabs:

- `Skeleton_Minion.prefab` -> `BT_Skeleton_Medium.controller`
- `Skeleton_Warrior.prefab` -> `BT_Skeleton_Medium.controller`
- `Skeleton_Rogue.prefab` -> `BT_Skeleton_Medium.controller`
- `Skeleton_Mage.prefab` -> `BT_Skeleton_Medium.controller`
- `Skeleton_Necromancer.prefab` -> `BT_Skeleton_Medium.controller`

Boss placeholder:

- `Skeleton_Golem_Boss.prefab` -> `BT_Boss_Large.controller`

## Placeholder States

- Player `BasicAttack` uses a generic one-handed melee attack, so Ranger, Mage, and Barbarian will need role-specific overrides or controller refinement later.
- Skeleton `BasicAttack`, `SkillCast`, `Hit`, `Defend`, and `UsePotion` use medium-rig general combat clips where a dedicated skeleton variant was not selected.
- Boss `SkillCast` uses `Melee_2H_Slam` as a large-rig placeholder.
- Boss `UsePotion` uses `Idle_B` as a placeholder because the boss placeholder does not currently have potion gameplay.
- `Dead` states use pose clips so the state can hold after `IsDead == true`.

## Boundaries Confirmed

- No C# code was modified.
- No scene was modified.
- No material was modified.
- No SkillData was modified.
- No KayKit source animation, model, avatar, material, or controller was modified.
- No animation import settings were changed.
- No animation events were added.
- No Override Controller was created.
- No runtime animation bridge script was created.
- No Movement, Combat, Skill, Potion, Turn, LAN, Boss AI, or formal level runtime integration was added.
- `GridTest.unity` was not modified.

## Known Risks

- Avatar mismatch must be checked in Unity, especially for clips shared between character visual sources.
- Phase 15.7 weapon visual alignment may need hand/socket refinement once animation preview is reviewed.
- Placeholder clips may not match final class identity.
- `Dead` state hold behavior should be validated in Animator preview and later in Phase 15.9 runtime integration.
- Controller YAML should be opened by Unity once so Unity can reserialize if needed.

## Unity 6.3 Check Steps

1. Open each of the three controllers in the Animator window.
2. Confirm parameters exist: `MoveSpeed`, `BasicAttack`, `Skill`, `Hit`, `Defend`, `UsePotion`, `IsDead`.
3. Confirm states exist: `Idle`, `Move`, `BasicAttack`, `SkillCast`, `Hit`, `Defend`, `UsePotion`, `Dead`.
4. Confirm `Idle` is the default state.
5. Preview transitions manually in Animator where possible.
6. Open each modified character prefab in Prefab Mode.
7. Confirm the root gameplay object does not receive a new Animator.
8. Confirm the visual child Animator has the expected controller assigned.
9. Do not save scene changes.

## Rollback

Before commit, this phase can be reverted with:

```powershell
git restore -- Assets/_BoneThrone/Prefabs/Units/Players/*.prefab Assets/_BoneThrone/Prefabs/Units/Enemies/*.prefab Assets/_BoneThrone/Prefabs/Units/Boss/*.prefab
Remove-Item -LiteralPath Assets/_BoneThrone/Animation -Recurse -Force
Remove-Item -LiteralPath Docs/DevLogs/Phase15.8_CharacterAnimatorControllers.md -Force
```

If the DevLog has already been tracked, use:

```powershell
git restore -- Docs/DevLogs/Phase15.8_CharacterAnimatorControllers.md
```
