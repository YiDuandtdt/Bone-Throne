# Phase 15.7 - Weapon / Equipment Attachment Pass

Date: 2026-05-29

## Goal

Phase 15.7 adds visual-only weapon and equipment attachments to the project-owned player, enemy, and boss placeholder prefabs. This pass does not change gameplay values, skills, combat rules, turn rules, scenes, materials, source art, or animation controllers.

## Modified Files

Character prefabs:

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

Documentation:

- `Docs/DevLogs/Phase15.7_WeaponEquipmentAttachmentPass.md`

## Source Assets Used

Adventurer equipment sources:

- `Assets/_BoneThrone/Art/KayKit - Adventurers (for Unity)/Prefabs/Accessories/sword_1handed.prefab`
- `Assets/_BoneThrone/Art/KayKit - Adventurers (for Unity)/Prefabs/Accessories/shield_square.prefab`
- `Assets/_BoneThrone/Art/KayKit - Adventurers (for Unity)/Prefabs/Accessories/bow.prefab`
- `Assets/_BoneThrone/Art/KayKit - Adventurers (for Unity)/Prefabs/Accessories/quiver.prefab`
- `Assets/_BoneThrone/Art/KayKit - Adventurers (for Unity)/Prefabs/Accessories/staff.prefab`
- `Assets/_BoneThrone/Art/KayKit - Adventurers (for Unity)/Prefabs/Accessories/axe_2handed.prefab`

Skeleton equipment sources:

- `Assets/_BoneThrone/Art/KayKit - Skeletons (for Unity)/Prefabs/Accessories/Skeleton_Blade.prefab`
- `Assets/_BoneThrone/Art/KayKit - Skeletons (for Unity)/Prefabs/Accessories/Skeleton_Axe.prefab`
- `Assets/_BoneThrone/Art/KayKit - Skeletons (for Unity)/Prefabs/Accessories/Skeleton_Shield_Small_A.prefab`
- `Assets/_BoneThrone/Art/KayKit - Skeletons (for Unity)/Prefabs/Accessories/Skeleton_Dagger.prefab`
- `Assets/_BoneThrone/Art/KayKit - Skeletons (for Unity)/Prefabs/Accessories/Skeleton_Staff.prefab`
- `Assets/_BoneThrone/Art/KayKit - Skeletons (for Unity)/Prefabs/Accessories/Skeleton_Golem_Axe_Large.prefab`

The source assets were referenced as visual children only. No KayKit source prefab, material, model, or asset was modified.

## Player Prefabs

- `Fighter.prefab`
  - Added `WeaponSocket_R` with `Sword_1Handed_Visual`.
  - Added `ShieldSocket_L` with `Shield_Square_Visual`.
- `Ranger.prefab`
  - Added `WeaponSocket_R` with `Bow_Visual`.
  - Added `BackSocket` with `Quiver_Visual`.
  - Ranger gameplay identity remains `Ranger`.
- `Mage.prefab`
  - Added `WeaponSocket_R` with `Staff_Visual`.
- `Barbarian.prefab`
  - Added `WeaponSocket_R` with `Axe_2Handed_Visual`.

## Enemy Prefabs

- `Skeleton_Minion.prefab`
  - Added `WeaponSocket_R` with `Skeleton_Blade_Visual`.
- `Skeleton_Warrior.prefab`
  - Added `WeaponSocket_R` with `Skeleton_Axe_Visual`.
  - Added `ShieldSocket_L` with `Skeleton_Shield_Small_A_Visual`.
- `Skeleton_Rogue.prefab`
  - Added `WeaponSocket_R` with `Skeleton_Dagger_Visual`.
  - `Skeleton_Rogue` remains the normal Rogue enemy naming.
- `Skeleton_Mage.prefab`
  - Added `WeaponSocket_R` with `Skeleton_Staff_Visual`.
- `Skeleton_Necromancer.prefab`
  - Added `WeaponSocket_R` with `Skeleton_Staff_Visual`.

## Boss Placeholder

- `Skeleton_Golem_Boss.prefab`
  - Added `WeaponSocket_R` with `Skeleton_Golem_Axe_Large_Visual`.
  - This remains a boss / heavy boss placeholder only.
  - No Boss AI, Boss skill, BossDoor, BossKey, or boss battle flow was created.

## Boundaries Confirmed

- No C# code was modified.
- No scene was modified.
- No material was modified.
- No SkillData was modified.
- No CombatSystem, SkillSystem, TurnManager, PotionSystem, EnemyAI, or LAN code was modified.
- No KayKit source prefab, model, material, or asset was modified.
- No Environment or Interactable prefab was modified.
- No Animator Controller, animation transition, or animation state machine was created or changed.
- No formal Level_01 / Level_02 / Level_03 scene was created.
- `GridTest.unity` was not modified.
- No gameplay stats, damage values, defense values, skills, action economy, or turn order rules were changed.

## Unity 6.3 Check Suggestions

1. Open each modified project-owned character prefab in Prefab Mode.
2. Confirm the new socket children exist:
   - `WeaponSocket_R`
   - `ShieldSocket_L` where applicable
   - `BackSocket` where applicable
3. Confirm the weapon / equipment visual prefab appears as a child of the socket.
4. Check rough visual alignment in the Scene view.
5. Confirm the prefab still has its existing Unit / faction / AI / collider / health bar components.
6. Open `GridTest.unity` without saving changes and confirm existing scene instances are not unintentionally changed.
7. Optional Play Mode smoke test:
   - Select player units.
   - End turns until enemies act.
   - Confirm combat still runs and the added equipment is visual-only.

## Known Risks

- Socket placement is a first-pass visual alignment and may need per-character refinement after animation work.
- Equipment orientation may need cleanup during Phase 15.8 / animation integration.
- Because this phase does not add hand-bone runtime attachment logic, some weapons may not follow ideal hand pose during future animations until sockets are refined.
- Boss placeholder equipment is visual-only and does not imply completed Boss AI or combat design.

## Rollback

To revert this phase before commit:

```powershell
git restore -- Assets/_BoneThrone/Prefabs/Units/Players/*.prefab Assets/_BoneThrone/Prefabs/Units/Enemies/*.prefab Assets/_BoneThrone/Prefabs/Units/Boss/*.prefab
Remove-Item -LiteralPath Docs/DevLogs/Phase15.7_WeaponEquipmentAttachmentPass.md -Force
```

If the DevLog has already been tracked, use:

```powershell
git restore -- Docs/DevLogs/Phase15.7_WeaponEquipmentAttachmentPass.md
```
