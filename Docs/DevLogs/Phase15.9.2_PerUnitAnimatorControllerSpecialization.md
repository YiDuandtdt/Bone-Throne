# Phase 15.9.2 - Per-Unit Animator Controller Specialization Pass

## Goal

Create independent project-owned Animator Controllers for each player unit, enemy unit, and the boss placeholder so each role can use animation clips closer to its gameplay identity, visual source, weapon, and rig.

This pass only changes Animator Controller assets and project character prefab visual child Animator controller references.

## Created Controllers

Created under `Assets/_BoneThrone/Animation/Controllers/Units/`:

- `BT_Fighter.controller`
- `BT_Ranger.controller`
- `BT_Mage.controller`
- `BT_Barbarian.controller`
- `BT_Skeleton_Minion.controller`
- `BT_Skeleton_Warrior.controller`
- `BT_Skeleton_Rogue.controller`
- `BT_Skeleton_Mage.controller`
- `BT_Skeleton_Necromancer.controller`
- `BT_Skeleton_Golem_Boss.controller`

Generated `.meta` files for the `Units` folder and all 10 controller assets.

The existing base controllers remain in place as fallback / template assets:

- `BT_Player_Medium.controller`
- `BT_Skeleton_Medium.controller`
- `BT_Boss_Large.controller`

## Shared Parameters

All new controllers keep the Phase 15.9 runtime parameter contract:

- `MoveSpeed`
- `BasicAttack`
- `Skill`
- `Hit`
- `Defend`
- `UsePotion`
- `IsDead`
- `IsDefending`

No runtime parameter names were changed.

## Shared States

All new controllers use:

- `Idle`
- `Move`
- `BasicAttack`
- `SkillCast`
- `Hit`
- `DefendStart`
- `DefendHold`
- `UsePotion`
- `Dead`

`DefendStart` is the state entered by the existing `Defend` trigger. `DefendHold` is controlled by `IsDefending`.

No animation events were added.

## Controller Clip Mapping

| Controller | Idle | Move | BasicAttack | SkillCast | Hit | DefendStart | DefendHold | UsePotion | Dead |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| `BT_Fighter` | `Combat/General/Idle_Combat` | `Movement/Walking_A` | `1H_Melee_Attack_Slice_Horizontal` | `Blocking/Block_Attack` | `Hit_A` | `Blocking/Block` | `Blocking/Blocking` | `Use_Item` | `Death_A_Pose` |
| `BT_Ranger` | `Rig_Medium/Combat Ranged/Ranged_Bow_Idle` | `Rig_Medium/Movement Advanced/Running_HoldingBow` | `Ranged_Bow_Release` | `Ranged_Bow_Draw` | `Hit_B` | `Blocking/Block` | `Blocking/Blocking` | `Use_Item` | `Death_A_Pose` |
| `BT_Mage` | `General/Idle_B` | `Movement/Walking_A` | `Ranged_Magic_Shoot` | `Spellcast_Long` | `Hit_A` | `Ranged_Magic_Spellcasting` | `Blocking/Blocking` | `Use_Item` | `Death_B_Pose` |
| `BT_Barbarian` | `Combat/General/Idle_Combat` | `Movement/Walking_A` | `2H_Melee_Attack_Chop` | `2H_Melee_Attack_Spin` | `Hit_A` | `Blocking/Block` | `Blocking/Blocking` | `Use_Item` | `Death_A_Pose` |
| `BT_Skeleton_Minion` | `Skeletons_Idle` | `Skeletons_Walking` | `1H_Melee_Attack_Slice_Horizontal` | `1H_Melee_Attack_Chop` | `Hit_A` | `Rig_Medium/Melee_Block` | `Rig_Medium/Melee_Blocking` | `Use_Item` | `Skeletons_Death_Pose` |
| `BT_Skeleton_Warrior` | `Skeletons_Idle` | `Skeletons_Walking` | `1H_Melee_Attack_Chop` | `Blocking/Block_Attack` | `Hit_A` | `Rig_Medium/Melee_Block` | `Rig_Medium/Melee_Blocking` | `Use_Item` | `Skeletons_Death_Pose` |
| `BT_Skeleton_Rogue` | `Skeletons_Idle` | `Skeletons_Walking` | `Dualwield_Melee_Attack_Stab` | `1H_Melee_Attack_Stab` | `Hit_B` | `Rig_Medium/Melee_Block` | `Rig_Medium/Melee_Blocking` | `Use_Item` | `Skeletons_Death_Pose` |
| `BT_Skeleton_Mage` | `Skeletons_Idle` | `Skeletons_Walking` | `Ranged_Magic_Shoot` | `Spellcast_Long` | `Hit_A` | `Rig_Medium/Melee_Block` | `Rig_Medium/Melee_Blocking` | `Use_Item` | `Skeletons_Death_Pose` |
| `BT_Skeleton_Necromancer` | `Skeletons_Idle` | `Skeletons_Walking` | `Ranged_Magic_Shoot` | `Spellcast_Summon` | `Hit_A` | `Rig_Medium/Melee_Block` | `Rig_Medium/Melee_Blocking` | `Use_Item` | `Skeletons_Death_Pose` |
| `BT_Skeleton_Golem_Boss` | `Rig_Large/Idle_A` | `Rig_Large/Walking_A` | `Rig_Large/Melee_2H_Attack` | `Rig_Large/Melee_2H_Slam` | `Rig_Large/Hit_A` | `Rig_Large/Melee_Block` | `Rig_Large/Melee_Blocking` | `Rig_Large/Idle_B` | `Rig_Large/Death_A_Pose` |

## Prefab Controller Binding

- `Fighter.prefab` -> `BT_Fighter.controller`
- `Ranger.prefab` -> `BT_Ranger.controller`
- `Mage.prefab` -> `BT_Mage.controller`
- `Barbarian.prefab` -> `BT_Barbarian.controller`
- `Skeleton_Minion.prefab` -> `BT_Skeleton_Minion.controller`
- `Skeleton_Warrior.prefab` -> `BT_Skeleton_Warrior.controller`
- `Skeleton_Rogue.prefab` -> `BT_Skeleton_Rogue.controller`
- `Skeleton_Mage.prefab` -> `BT_Skeleton_Mage.controller`
- `Skeleton_Necromancer.prefab` -> `BT_Skeleton_Necromancer.controller`
- `Skeleton_Golem_Boss.prefab` -> `BT_Skeleton_Golem_Boss.controller`

Only visual child Animator controller references were changed.

## Placeholder Notes

- Fighter `SkillCast` uses `Block_Attack` as a shield-flavored placeholder.
- Mage `DefendStart` uses a magic spellcasting pose as a temporary defensive presentation.
- Skeleton caster `UsePotion` uses `Use_Item` even though enemies do not currently use potion gameplay.
- Boss `UsePotion` uses `Rig_Large/Idle_B` because the boss placeholder does not currently have potion gameplay.
- Skeleton mage and necromancer use medium magic / spell clips, not dedicated skeleton spell clips.

## Boundaries Preserved

- No C# code was modified.
- No scene / `GridTest.unity` was modified.
- No `SkillData` was modified.
- No KayKit source animation, model, avatar, prefab, or material was modified.
- No `.anim` file or animation import setting was modified.
- No animation event was added.
- No gameplay rule was changed.
- No CombatSystem, SkillSystem, TurnManager, PotionSystem, or UnitAnimationController changes were made.
- No formal level scene or LAN work was done.
- Ranger gameplay identity remains `Ranger`.
- `Skeleton_Golem_Boss` remains a boss / heavy boss placeholder only.

## Unity 6.3 Play Mode Test Steps

1. Open each new controller under `Assets/_BoneThrone/Animation/Controllers/Units/`.
2. Confirm all parameters exist and match Phase 15.9 names.
3. Confirm states exist: `Idle`, `Move`, `BasicAttack`, `SkillCast`, `Hit`, `DefendStart`, `DefendHold`, `UsePotion`, `Dead`.
4. Open each project character prefab in Prefab Mode.
5. Confirm the visual child Animator points to the expected per-unit controller.
6. Enter `GridTest.unity` Play Mode without saving scene changes.
7. Test movement, BasicAttack, Skill, Defend, UsePotion, Hit, and Dead on player units.
8. Let EnemyTurn run and confirm enemies use their per-unit controller presentations.
9. Confirm Dead holds and DefendHold exits when `IsDefending` clears.
10. Confirm Console has no missing parameter or avatar mismatch errors.

## Known Risks

- Some clips are still placeholders where a perfect class-specific clip does not exist.
- Ranger uses Rogue visual but Ranger controller and gameplay identity remain Ranger.
- Skeleton caster spell clips are shared medium rig magic clips and may need later visual refinement.
- Boss large rig clips must remain isolated to `BT_Skeleton_Golem_Boss`.
- Direct CrossFade debug fallback in `UnitAnimationController` still targets the old `Defend` state name if enabled; normal runtime uses the `Defend` trigger and is unaffected.
- Unity may reserialize controller YAML after opening the assets.

## Rollback

```powershell
git restore -- Assets/_BoneThrone/Prefabs/Units/Players Assets/_BoneThrone/Prefabs/Units/Enemies Assets/_BoneThrone/Prefabs/Units/Boss
Remove-Item -LiteralPath Assets/_BoneThrone/Animation/Controllers/Units -Recurse -Force
Remove-Item -LiteralPath Assets/_BoneThrone/Animation/Controllers/Units.meta -Force
Remove-Item -LiteralPath Docs/DevLogs/Phase15.9.2_PerUnitAnimatorControllerSpecialization.md -Force
```
