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

## Minimal Repair Note

After initial Play Mode validation, Idle animation played but runtime animation triggers did not visibly react. The likely root cause was that `UnitAnimationController` prefab component YAML had been serialized as a malformed one-line component block, so Unity could fail to deserialize the component on prefab roots. GridTest scene instances reference the project-owned unit prefabs and did not contain local `UnitAnimationController` overrides, so the fix was applied to the project prefabs rather than editing `GridTest.unity`.

Repair changes:

- Normalized `UnitAnimationController` prefab component YAML into standard Unity multi-line `MonoBehaviour` blocks on all player, enemy, and boss unit prefabs.
- Added `debugLogging` to `UnitAnimationController`, default `false`.
- Kept once-only warnings for missing Animator / missing parameters.
- Updated `UnitMover` to keep `MoveSpeed` at `1f` for a short presentation pulse before returning to `0f`, because movement is currently instant.
- Did not modify `GridTest.unity`.
- Did not modify Animator Controllers.
- Did not modify gameplay rules.

Repair validation:

```powershell
dotnet build Assembly-CSharp.csproj
```

Result:

- Passed with 0 warnings and 0 errors.

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

## Directed Repair 2 - Runtime Trigger Visibility

After the first repair, Idle animation still played in Play Mode but BasicAttack / UsePotion / Dead / Move / Hit / Skill / Defend did not visibly react. This confirmed the visual Animator, Avatar, and Idle clip were usable, but the runtime presentation trigger path was still not reliable enough.

Diagnosis:

- `GridTest.unity` scene units are prefab instances referencing the project-owned unit prefabs.
- The scene instances do not contain local `UnitAnimationController` removal overrides.
- The visual child Animators on project prefabs remain bound to the Phase 15.8 controllers.
- The three Animator Controllers contain the required parameters.
- Any State trigger transitions exist, but the hand-authored controller transition path still did not produce visible runtime reactions in Play Mode.
- `MoveSpeed` transition thresholds were `0` and were corrected to `0.1`.

Repair changes:

- `UnitAnimationController` now still sets the Animator parameters, but also direct-crossfades to the matching presentation state as a small runtime fallback.
- Added direct state hashes for `Idle`, `Move`, `BasicAttack`, `SkillCast`, `Hit`, `Defend`, `UsePotion`, and `Dead`.
- Added `playStatesDirectly`, default enabled on project unit prefabs.
- Added `directCrossFadeDuration`, default `0.03`.
- Added debug ContextMenu helpers for BasicAttack, UsePotion, and Dead.
- Kept `debugLogging` default `false` to avoid log spam.
- Corrected `MoveSpeed` thresholds to `0.1` in all three controllers.
- Added explicit serialized `debugLogging`, `playStatesDirectly`, and `directCrossFadeDuration` values to all player, enemy, and boss unit prefabs.

Files modified by this directed repair:

- `Assets/_BoneThrone/Scripts/Units/UnitAnimationController.cs`
- `Assets/_BoneThrone/Animation/Controllers/BT_Player_Medium.controller`
- `Assets/_BoneThrone/Animation/Controllers/BT_Skeleton_Medium.controller`
- `Assets/_BoneThrone/Animation/Controllers/BT_Boss_Large.controller`
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
- `Docs/DevLogs/Phase15.9_AnimationIntegration.md`

No `GridTest.unity` edit was required.

Validation:

```powershell
dotnet build Assembly-CSharp.csproj
```

Result:

- Passed with 0 warnings and 0 errors.

Boundaries preserved:

- No gameplay rule changes.
- No Combat damage rule changes.
- No Skill resolution changes.
- No Potion rule changes.
- No Turn rule changes.
- No SkillData changes.
- No scene changes.
- No KayKit source changes.
- No animation events.
- No runtime animation-driven damage, cooldown, MarkMoved, MarkActed, End Turn, or turn progression.

## Hard Diagnosis Pass - Animation Trigger Chain

Runtime animation presentation was still not visible after prefab, controller transition, and direct-crossfade repairs. This pass intentionally does not modify prefabs, Animator Controllers, scenes, SkillData, KayKit source, materials, TurnManager, or gameplay rules. It only adds opt-in diagnostics so the broken layer can be identified in Play Mode.

Files modified by this diagnosis pass:

- `Assets/_BoneThrone/Scripts/Units/UnitAnimationController.cs`
- `Assets/_BoneThrone/Scripts/Movement/UnitMover.cs`
- `Assets/_BoneThrone/Scripts/Combat/CombatSystem.cs`
- `Assets/_BoneThrone/Scripts/Combat/DamageResolver.cs`
- `Assets/_BoneThrone/Scripts/Combat/DefendSystem.cs`
- `Assets/_BoneThrone/Scripts/Skills/SkillSystem.cs`
- `Assets/_BoneThrone/Scripts/Items/PotionSystem.cs`
- `Assets/_BoneThrone/Scripts/Units/Unit.cs`
- `Docs/DevLogs/Phase15.9_AnimationIntegration.md`

`UnitAnimationController` diagnostics:

- `debugLogging` remains serialized and defaults to `false`.
- Awake / Start logging now reports the unit object, Animator object, runtime controller name, avatar name, layer count, layer 0 fullPathHash / shortNameHash, and required parameter availability.
- Public animation methods log method name, caller object, Animator presence, controller name, target parameter, target direct-crossfade state, and next-frame layer 0 state hashes when `debugLogging` is enabled.
- Missing Animator / missing parameter warnings remain no-op safe and do not affect gameplay.

ContextMenu test entries added:

- `BT Test/BasicAttack`
- `BT Test/Skill`
- `BT Test/Hit`
- `BT Test/Defend`
- `BT Test/UsePotion`
- `BT Test/Dead True`
- `BT Test/Dead False`
- `BT Test/MoveSpeed 1`
- `BT Test/MoveSpeed 0`

Gameplay trigger-point diagnostics:

- `UnitMover` logs MoveSpeed trigger attempts behind `AnimationDebug`.
- `CombatSystem` logs BasicAttack trigger attempts behind `AnimationDebug`.
- `SkillSystem` logs Skill trigger attempts behind `AnimationDebug`.
- `DamageResolver` logs Hit trigger attempts behind `AnimationDebug`.
- `DefendSystem` logs Defend trigger attempts behind `AnimationDebug`.
- `PotionSystem` logs UsePotion trigger attempts behind `AnimationDebug`.
- `Unit.MarkDeadAndReleaseTile` logs Dead trigger attempts behind `AnimationDebug`.

`AnimationDebug` defaults to `false` through a local static property in each file. To diagnose a specific system, temporarily change that file's property to return `true`, run the Play Mode test, then revert it.

Suggested diagnosis flow:

1. In Play Mode, select a runtime unit root and enable `UnitAnimationController.debugLogging`.
2. Use the `BT Test/*` ContextMenu entries on that component.
3. If ContextMenu animations do not move the Animator state, inspect the logged Animator object, controller, parameters, and state hashes.
4. If ContextMenu animations work, temporarily enable `AnimationDebug` in one gameplay system and run that action.
5. If gameplay logs show `controller=null`, the runtime unit root is missing `UnitAnimationController`.
6. If gameplay logs show a controller but `UnitAnimationController` logs no Animator, the component is not finding the visual child Animator.
7. If `UnitAnimationController` logs CrossFade but next-frame hashes do not change, the state name/hash or controller layer is the likely fault.

Validation:

```powershell
dotnet build Assembly-CSharp.csproj
```

Result:

- Passed with 0 warnings and 0 errors.

Boundaries preserved:

- No gameplay rule changes.
- No TurnManager changes.
- No SkillData changes.
- No scene or GridTest changes.
- No prefab changes in this diagnosis pass.
- No Animator Controller changes in this diagnosis pass.
- No KayKit source changes.
- No animation events.

## Runtime Root and Defend Hold Repair

Follow-up Play Mode diagnosis found the critical runtime issue:

- The project prefabs contained a `UnitAnimationController` YAML block, but that block had an invalid / missing fileID.
- The root GameObject `m_Component` list referenced a different component fileID, so Unity did not load `UnitAnimationController` as an actual runtime root component.
- This explains why Idle worked from the visual child Animator while gameplay-triggered presentation calls could not reach the Animator.

Prefab repair:

- Repaired the root `UnitAnimationController` component fileID on all project unit prefabs.
- Confirmed the root `m_Component` list now references the same fileID as the `UnitAnimationController` MonoBehaviour block.
- `debugLogging` remains default `false`.
- No `GridTest.unity` scene edit was required because the scene instances reference project-owned unit prefabs and do not show removed component overrides.

Repaired prefabs:

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

Defend presentation repair:

- Added `IsDefending` bool support to `UnitAnimationController`.
- Added `UnitAnimationController.SetDefending(bool isDefending)`.
- `DefendSystem.TryDefend()` now calls `PlayDefend()` and then `SetDefending(true)` after the existing gameplay defend state is successfully set.
- `UnitDefenseState.ClearDefending()` now calls `SetDefending(false)` so animation follows the existing gameplay clear point.
- This does not change defend duration, damage reduction, action cost, or turn rules.

Animator Controller repair:

- Added `IsDefending` bool parameter to:
  - `BT_Player_Medium.controller`
  - `BT_Skeleton_Medium.controller`
  - `BT_Boss_Large.controller`
- Added one `DefendHold` state per controller.
- `DefendHold` currently uses the existing Defend clip with speed `0` and cycle offset `0.85` as a placeholder hold pose.
- Defend action can transition to `DefendHold` when `IsDefending == true`.
- `DefendHold` returns to Idle when `IsDefending == false`.
- Dead remains a separate no-return state, and `UnitAnimationController` blocks BasicAttack / Skill / Hit / Defend / UsePotion / MoveSpeed presentation calls after `SetDead(true)`.

Existing gameplay clear point:

- `UnitDefenseState.TryConsumeReduction()` already calls `ClearDefending()` when defend is consumed.
- `ClearDefending()` is the animation sync point for `SetDefending(false)`.
- No new gameplay defend lifetime rule was added.

Validation:

```powershell
dotnet build Assembly-CSharp.csproj
```

Result:

- Passed with 0 warnings and 0 errors.

Play Mode test checklist:

1. Open `GridTest.unity`.
2. Enter Play Mode.
3. Select a player runtime root and confirm `UnitAnimationController` exists.
4. Use BasicAttack and confirm the attack presentation triggers.
5. Use Potion and confirm `UsePotion` triggers.
6. Damage a unit and confirm surviving targets can play `Hit`.
7. Kill a unit and confirm `Dead` holds and is not interrupted by later triggers.
8. Use Defend and confirm the unit enters a defend action, then holds the defend pose while gameplay `UnitDefenseState.IsDefending` remains true.
9. Attack that defending unit and confirm existing damage reduction consumes defend, then animation clears back toward Idle.

Boundaries preserved:

- No gameplay rule changes.
- No damage / skill / potion / turn resolution changes.
- No SkillData changes.
- No KayKit source changes.
- No animation events.
- No formal level scene work.
- No LAN / Networking work.

## Movement Presentation and Double-Trigger Repair

Play Mode follow-up found three presentation issues:

- Units did not rotate toward their movement direction.
- Movement visually snapped from the start tile to the target tile.
- Animation actions played twice because Animator triggers and direct CrossFade fallback could both run for the same action.

Animation duplicate repair:

- `UnitAnimationController.playStatesDirectly` now defaults to `false`.
- Runtime action playback now defaults to Animator parameters only:
  - `MoveSpeed` uses `SetFloat`.
  - BasicAttack / Skill / Hit / Defend / UsePotion use `SetTrigger`.
  - Dead uses `IsDead`.
  - Defend hold uses `IsDefending`.
- Direct CrossFade is kept only as an opt-in debug fallback. It only runs when both `debugLogging` and `playStatesDirectly` are enabled.
- This prevents trigger + CrossFade from playing the same animation twice during normal Play Mode.
- Dead remains protected: after `SetDead(true)`, non-death presentation triggers are ignored and cannot interrupt Dead.

Movement presentation repair:

- `UnitMover.TryMove()` keeps the same public API and still returns `bool`.
- Existing movement legality, pathfinding result, tile occupancy, and final `Unit.CurrentTile` behavior are unchanged.
- Gameplay placement still resolves immediately through the existing `unit.TryPlaceOnTile(targetTile)` path.
- Visual transform movement now runs through an internal coroutine.
- The coroutine moves the unit root through the path tile positions, or at minimum from current world position to the final target tile.
- `MoveSpeed` is set to `1f` at visual movement start and `0f` after visual movement completes.
- Final transform position is snapped exactly to the target tile world position at the end of the coroutine.
- `moveSpeed` is serialized on `UnitMover` and defaults to `4f`.

Four-direction facing repair:

- Before each visual segment, `UnitMover` computes the segment delta and clamps facing to one of four world cardinal directions:
  - East / West from the X axis.
  - North / South from the Z axis.
- No diagonal / free rotation facing is used.
- `turnDuration` is serialized on `UnitMover` and defaults to `0.12f`.
- Rotation uses `Quaternion.Slerp` and affects only the unit transform presentation, not grid/path rules.

Coroutine and re-entry guard:

- `UnitMover` now tracks active visual movement routines per unit.
- A second movement request for a unit already moving visually is rejected to avoid overlapping visual movement coroutines.
- `OnDisable()` stops movement presentation coroutines and clears the tracking table.

Defend presentation check:

- Defend still uses the existing `Defend` trigger plus `IsDefending` bool.
- `IsDefending` continues to hold the `DefendHold` state until the existing gameplay clear point calls `UnitDefenseState.ClearDefending()`.
- No gameplay defend lifetime rule was added or changed.

Validation:

```powershell
dotnet build Assembly-CSharp.csproj
```

Result:

- Passed with 0 warnings and 0 errors.

Unity Play Mode test steps:

1. Open `GridTest.unity`.
2. Enter Play Mode.
3. Select a player unit and issue a multi-tile move.
4. Confirm the unit rotates only in four directions before / during each segment.
5. Confirm the unit visually moves instead of snapping.
6. Confirm `MoveSpeed` is active during movement and returns to `0` after the unit reaches the final tile.
7. Perform BasicAttack / Skill / Hit / UsePotion and confirm each action plays once.
8. Use Defend and confirm it enters Defend, then holds the defend pose until the existing gameplay defend clear point.
9. Kill a unit and confirm Dead remains held and is not interrupted by later presentation triggers.

Known risks:

- Gameplay placement is still immediate while the visual transform catches up, so a very fast follow-up action may begin while movement presentation is still finishing.
- The current four-direction facing assumes the grid's world axes align with Unity X/Z.
- `DefendHold` still uses a frozen pose from the existing Defend clip as a placeholder.

Rollback:

```powershell
git restore -- Assets/_BoneThrone/Scripts/Units/UnitAnimationController.cs Assets/_BoneThrone/Scripts/Movement/UnitMover.cs Docs/DevLogs/Phase15.9_AnimationIntegration.md
```

## Presentation Closeout Repair

Play Mode follow-up found three remaining presentation issues:

- Defend played the defend start motion twice before reaching the hold pose.
- BasicAttack and Skill did not rotate the acting unit toward the target.
- After movement, units kept the final movement direction even when an alive opponent was nearby.

Defend double-play repair:

- Root cause: `DefendHold` still used a start-like defend motion, so the controller visibly played the defend action once from the `Defend` trigger and then appeared to play it again when entering the hold state.
- `DefendHold` now uses blocking hold clips:
  - Player / Skeleton medium controllers use `Rig_Medium/Combat Melee/Melee_Blocking.anim`.
  - Boss large controller uses `Rig_Large/Combat Melee/Melee_Blocking.anim`.
- `DefendSystem.TryDefend()` now sets presentation `IsDefending` before firing the `Defend` trigger, so the start action can cleanly flow into the hold pose.
- `UnitAnimationController.playStatesDirectly` remains default `false`; direct CrossFade remains an opt-in debug fallback only.
- Dead presentation remains higher priority. When `IsDead` is true, normal action triggers are ignored and cannot interrupt Dead.

Attack / skill target-facing:

- `UnitAnimationController` now exposes `FaceTowards(Vector3 worldPosition)` and `FaceTowardsDirection(Vector3 worldDelta)`.
- Facing is clamped to four directions by dominant world axis:
  - Absolute X greater than or equal to absolute Z means East / West.
  - Otherwise Z means North / South.
- `CombatSystem.TryBasicAttack()` calls `FaceTowards(target.transform.position)` before `PlayBasicAttack()`.
- `SkillSystem.TryUseSkill()` calls `FaceTowards(target.transform.position)` before `PlaySkill()` when a unit target exists.
- These calls affect only root rotation presentation. They do not wait for rotation completion and do not change range, hit, damage, cooldown, action, or turn rules.

Movement end-facing:

- Movement still faces each path segment during visual movement.
- After visual movement completes and `MoveSpeed` is returned to `0f`, `UnitMover` looks for the nearest alive opponent:
  - Player units face the nearest alive Enemy.
  - Enemy units face the nearest alive Player.
  - If no opponent is found, the unit keeps its final movement direction.
- The lookup is presentation-only and does not affect enemy AI target selection, pathfinding, attack range, tile occupancy, or action economy.

Files changed in this closeout repair:

- `Assets/_BoneThrone/Scripts/Units/UnitAnimationController.cs`
- `Assets/_BoneThrone/Scripts/Movement/UnitMover.cs`
- `Assets/_BoneThrone/Scripts/Combat/CombatSystem.cs`
- `Assets/_BoneThrone/Scripts/Skills/SkillSystem.cs`
- `Assets/_BoneThrone/Scripts/Combat/DefendSystem.cs`
- `Assets/_BoneThrone/Animation/Controllers/BT_Player_Medium.controller`
- `Assets/_BoneThrone/Animation/Controllers/BT_Skeleton_Medium.controller`
- `Assets/_BoneThrone/Animation/Controllers/BT_Boss_Large.controller`
- `Docs/DevLogs/Phase15.9_AnimationIntegration.md`

Boundaries preserved:

- No gameplay rule changes.
- No damage / skill / potion / turn resolution changes.
- No SkillData changes.
- No scene / GridTest changes.
- No animation events.
- No LAN / Networking work.
- No formal level scene work.

Validation:

```powershell
dotnet build Assembly-CSharp.csproj
```

Result:

- Passed with 0 warnings and 0 errors.

Worktree note:

- `git status` currently shows `Assets/_BoneThrone/Art/Animations/Combat/Blocking/Block.anim` as modified.
- This file is outside the allowed scope for this closeout repair and was not intentionally edited as part of this pass.
- It should be reviewed separately before commit to avoid accidentally including an Art source animation change.

Unity Play Mode test steps:

1. Open `GridTest.unity`.
2. Enter Play Mode.
3. Move a player unit several tiles and confirm the unit turns only North / South / East / West during movement.
4. Confirm movement is smooth and `MoveSpeed` returns to `0f` at the final tile.
5. Confirm the unit turns toward the nearest alive enemy after movement finishes.
6. Use BasicAttack and confirm the attacker turns toward the target before the attack animation.
7. Use a targeted Skill and confirm the caster turns toward the target before the skill animation.
8. Use Defend and confirm the defend start motion plays once, then the unit holds the blocking pose.
9. Confirm the blocking pose clears only when the existing gameplay defending clear point runs.
10. Kill a unit and confirm Dead remains held and is not interrupted by BasicAttack / Skill / Hit / Defend / UsePotion.

Known risks:

- The nearest-opponent facing uses scene `Unit` lookup for presentation only. This is acceptable for GridTest scale but may be replaced with a cached unit registry later.
- Four-direction facing assumes the tactical grid aligns with Unity world X/Z axes.
- Attack / skill rotation starts immediately but does not block gameplay resolution.

Rollback:

```powershell
git restore -- Assets/_BoneThrone/Scripts/Units/UnitAnimationController.cs Assets/_BoneThrone/Scripts/Movement/UnitMover.cs Assets/_BoneThrone/Scripts/Combat/CombatSystem.cs Assets/_BoneThrone/Scripts/Skills/SkillSystem.cs Assets/_BoneThrone/Scripts/Combat/DefendSystem.cs Assets/_BoneThrone/Animation/Controllers/BT_Player_Medium.controller Assets/_BoneThrone/Animation/Controllers/BT_Skeleton_Medium.controller Assets/_BoneThrone/Animation/Controllers/BT_Boss_Large.controller Docs/DevLogs/Phase15.9_AnimationIntegration.md
```

## Defend New-Round Animation Sync Repair

Play Mode follow-up found that units entered `DefendHold` correctly, but stayed in the blocking pose when a new PlayerTurn round started.

Root cause:

- `UnitDefenseState.ClearDefending()` already clears gameplay defending and now also synchronizes `UnitAnimationController.SetDefending(false)`.
- Damage consumption already goes through `UnitDefenseState.TryConsumeReduction()`, which calls `ClearDefending()`.
- New player rounds reset `UnitTurnState` in `TurnManager.ResetPlayerUnitTurnStates()`, but did not call the existing defending clear path.
- As a result, a defending unit could keep `UnitDefenseState.IsDefending == true` across the new round, so Animator `IsDefending` also remained true and stayed in `DefendHold`.

Defending clear points after this repair:

- Defend success:
  - `DefendSystem.TryDefend()` calls `UnitDefenseState.SetDefending(defendReduction)`.
  - The same success path calls `UnitAnimationController.SetDefending(true)` and `PlayDefend()`.
- Damage consume:
  - `DamageResolver.ApplyDamage()` checks `UnitDefenseState.IsDefending`.
  - `UnitDefenseState.TryConsumeReduction()` calls `ClearDefending()`.
  - `ClearDefending()` calls `UnitAnimationController.SetDefending(false)` when present.
- New player round:
  - `TurnManager.StartPlayerRound()` calls `ResetPlayerUnitTurnStates()`.
  - `ResetPlayerUnitTurnStates()` now calls the existing `UnitDefenseState.ClearDefending()` for player units that are still defending.
  - This clears gameplay defending and synchronizes Animator `IsDefending=false` through the same method used by damage consumption.

Animator Controller check:

- No controller changes were required for this repair.
- Existing controllers already have `DefendHold / Blocking -> Idle` when `IsDefending == false`.
- Existing controllers already have Dead reachable by `IsDead == true`.
- No animation events were added.

Boundaries preserved:

- No damage reduction value changes.
- No action cost changes.
- No turn-order rule changes.
- No SkillData changes.
- No scene / GridTest changes.
- No KayKit source changes.
- Animation still follows gameplay state and does not drive gameplay.

Validation:

```powershell
dotnet build Assembly-CSharp.csproj
```

Result:

- Passed with 0 warnings and 0 errors.

Unity Play Mode test steps:

1. Open `GridTest.unity`.
2. Enter Play Mode.
3. Select a player unit and use Defend.
4. Confirm the unit enters `DefendHold / Blocking`.
5. End that unit's turn, then end all remaining alive player turns.
6. Let EnemyTurn finish and return to the next PlayerTurn.
7. Confirm the defending unit exits Blocking and returns toward Idle at the new PlayerTurn start.
8. Repeat with a defending unit being attacked and confirm damage consumption clears Blocking immediately through the existing consume path.
9. Kill a defending unit and confirm Dead remains higher priority than DefendHold.

Rollback:

```powershell
git restore -- Assets/_BoneThrone/Scripts/Turns/TurnManager.cs Docs/DevLogs/Phase15.9_AnimationIntegration.md
```
