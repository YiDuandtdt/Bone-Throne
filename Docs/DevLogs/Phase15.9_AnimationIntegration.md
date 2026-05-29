# Phase 15.9 - Animation Integration with Movement / Combat / Skill / Potion

Date: 2026-05-29

## Goal

Phase 15.9 connects the Phase 15.8 Animator Controller parameters to runtime presentation events. Animation remains presentation-only and does not drive damage, cooldowns, action economy, death rules, turn flow, tile state, or gameplay authority.

## Modified Files

Scripts:

- `Assets/_BoneThrone/Scripts/Units/UnitAnimationController.cs`
- `Assets/_BoneThrone/Scripts/Movement/UnitMover.cs`
- `Assets/_BoneThrone/Scripts/Combat/CombatSystem.cs`
- `Assets/_BoneThrone/Scripts/Combat/DamageResolver.cs`
- `Assets/_BoneThrone/Scripts/Skills/SkillSystem.cs`
- `Assets/_BoneThrone/Scripts/Combat/DefendSystem.cs`
- `Assets/_BoneThrone/Scripts/Items/PotionSystem.cs`
- `Assets/_BoneThrone/Scripts/Units/Unit.cs`

Prefabs:

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

- `Docs/DevLogs/Phase15.9_AnimationIntegration.md`

## UnitAnimationController

Added `UnitAnimationController` as a presentation-only component intended to live on the root Unit object.

Public methods:

- `SetMoveSpeed(float speed)`
- `PlayBasicAttack()`
- `PlaySkill()`
- `PlayHit()`
- `PlayDefend()`
- `PlayUsePotion()`
- `SetDead(bool isDead)`

Animator parameters:

- `MoveSpeed`
- `BasicAttack`
- `Skill`
- `Hit`
- `Defend`
- `UsePotion`
- `IsDead`

The component automatically finds an Animator on itself or child objects, preferring child visual Animators. Missing Animator or missing parameters are no-op safe and do not block gameplay.

## Runtime Integration Points

- Movement:
  - `UnitMover.TryMove()` sets `MoveSpeed` to `1f` around successful movement placement and always returns it to `0f` before the method exits the movement success/failure path.
- Basic Attack:
  - `CombatSystem.TryBasicAttack()` triggers `PlayBasicAttack()` after the attack is validated and in range, before D20 resolution.
  - Hit/miss rules are unchanged.
- Skill:
  - `SkillSystem.TryUseSkill()` triggers `PlaySkill()` after the skill effect has executed and before cooldown/action bookkeeping continues.
  - Skill range, target, damage, cooldown, and SkillData behavior are unchanged.
- Hit:
  - `DamageResolver.ApplyDamage()` triggers `PlayHit()` after actual nonzero damage when the target survives.
  - Death presentation is handled by `Unit.MarkDeadAndReleaseTile()`.
- Defend:
  - `DefendSystem.TryDefend()` triggers `PlayDefend()` after `SetDefending()` and existing action consumption.
- Potion:
  - `PotionSystem.TryUsePotion()` triggers `PlayUsePotion()` after potion consumption, healing, and existing action consumption.
- Dead:
  - `Unit.MarkDeadAndReleaseTile()` triggers `SetDead(true)` before the tile is released.

## Prefab Component Binding

Added `UnitAnimationController` to the root Unit object on:

- `Fighter`
- `Ranger`
- `Mage`
- `Barbarian`
- `Skeleton_Minion`
- `Skeleton_Warrior`
- `Skeleton_Rogue`
- `Skeleton_Mage`
- `Skeleton_Necromancer`
- `Skeleton_Golem_Boss`

The Animator remains on the visual child. No visual child, Animator Controller, weapon socket, Unit stats, AI, collider, health bar, or selection setup was changed.

## Boundaries Confirmed

- Animation is presentation-only.
- No gameplay rules were changed.
- No CombatSystem damage rules were changed.
- No SkillSystem resolution rules were changed.
- No PotionSystem potion rules were changed.
- No TurnManager turn rules were changed.
- No SkillData was modified.
- No scene was modified.
- `GridTest.unity` was not modified.
- No KayKit source asset was modified.
- No Animator Controller asset or state machine was modified.
- No animation event was added.
- No runtime animation bridge calls into TurnManager, CombatSystem, SkillSystem, or PotionSystem.
- No LAN / Networking work was added.
- Single-player PlayerTurn remains free-order.

## Validation

Attempted:

```powershell
dotnet build Assembly-CSharp.csproj
```

Result:

- Failed because the generated `Assembly-CSharp.csproj` explicitly lists compile files and has not been regenerated to include the newly added `UnitAnimationController.cs`.
- This phase did not modify `.csproj` files because they are outside the allowed file scope.
- Unity should regenerate project files and compile the new script on editor refresh.

## Unity 6.3 Play Mode Test Steps

1. Open Unity and allow the editor to refresh scripts.
2. Open each modified character prefab and confirm the root Unit object has `UnitAnimationController`.
3. Confirm the visual child still has the Animator Controller from Phase 15.8.
4. Enter Play Mode in `GridTest.unity`.
5. Move a player unit and confirm movement does not leave `MoveSpeed` stuck.
6. Perform a basic attack and confirm the attacker triggers `BasicAttack`; hit and miss rules should remain unchanged.
7. Use a skill and confirm the caster triggers `Skill`; cooldown and damage should remain unchanged.
8. Use Defend and confirm `Defend` triggers without ending turn.
9. Use Potion and confirm `UsePotion` triggers after successful potion use without ending turn.
10. Damage a unit and confirm surviving targets trigger `Hit`.
11. Kill a unit and confirm `IsDead` is set while tile release and death logic remain unchanged.
12. End turns through EnemyTurn and confirm single-player free-order PlayerTurn remains intact.

## Known Risks

- `MoveSpeed` may not visibly play because current movement is instant.
- Missing Animator or missing parameter warnings may appear if a prefab visual binding is broken.
- Animation triggers fire after gameplay validation but gameplay is still resolved immediately.
- Weapon alignment from Phase 15.7 may need refinement now that animations play.
- Unity must refresh/regenerate project files before command-line build sees the new script.

## Rollback

Before commit:

```powershell
git restore -- Assets/_BoneThrone/Scripts/Units/Unit.cs Assets/_BoneThrone/Scripts/Movement/UnitMover.cs Assets/_BoneThrone/Scripts/Combat/CombatSystem.cs Assets/_BoneThrone/Scripts/Combat/DamageResolver.cs Assets/_BoneThrone/Scripts/Combat/DefendSystem.cs Assets/_BoneThrone/Scripts/Skills/SkillSystem.cs Assets/_BoneThrone/Scripts/Items/PotionSystem.cs Assets/_BoneThrone/Prefabs/Units/Players/*.prefab Assets/_BoneThrone/Prefabs/Units/Enemies/*.prefab Assets/_BoneThrone/Prefabs/Units/Boss/*.prefab
Remove-Item -LiteralPath Assets/_BoneThrone/Scripts/Units/UnitAnimationController.cs -Force
Remove-Item -LiteralPath Assets/_BoneThrone/Scripts/Units/UnitAnimationController.cs.meta -Force
Remove-Item -LiteralPath Docs/DevLogs/Phase15.9_AnimationIntegration.md -Force
```

If the DevLog has already been tracked:

```powershell
git restore -- Docs/DevLogs/Phase15.9_AnimationIntegration.md
```
