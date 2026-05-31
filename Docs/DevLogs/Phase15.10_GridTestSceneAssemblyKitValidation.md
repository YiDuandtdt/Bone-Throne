# Phase 15.10 - GridTest Scene Assembly Kit Validation

## Goal and Boundary

Phase 15.10 validates the reusable production foundation created from Phase 15.1 through Phase 15.9.2 in the existing regression scene:

- `Assets/_BoneThrone/Scenes/GridTest.unity`

This phase is validation / cleanup / minimal-fix planning only. It is not formal level development.

`GridTest.unity` remains the regression baseline. It is not `Level_01`, `Level_02`, `Level_03`, or a formal production level.

This validation record was created as a docs-only pass. No scene, prefab, code, SkillData, KayKit source, material, package, or project setting was modified.

## Explicit Prohibitions

- Do not create `Level_01`, `Level_02`, or `Level_03`.
- Do not convert `GridTest.unity` into a formal level.
- Do not create or validate a final Boss fight.
- Do not implement BossDoor, BossKey, or SupplyPoint complete flow.
- Do not do LAN / Networking work.
- Do not modify KayKit source assets.
- Do not modify SkillData.
- Do not broadly rework Turn, Combat, Skill, or Potion systems.
- Do not change the single-player free-order PlayerTurn rule.
- Do not return single-player to Fighter -> Ranger -> Mage -> Barbarian fixed-order.

## Static Check Summary

The current project scan and Phase 15 DevLogs identify the following systems and assets as the Phase 15.10 GridTest validation surface:

- HealthPotion pickup from Phase 15.1.
- Existing Potion use through `PotionSystem`.
- Existing Key prefab and `KeyItem`.
- Existing Stairs prefab and `InteractableStairs`.
- Chest / Doorway visual-only interactable candidates from Phase 15.5.
- Environment prefab availability from Phase 15.4.
- Player prefabs: Fighter, Ranger, Mage, Barbarian.
- Enemy prefabs: Skeleton_Minion, Skeleton_Warrior, Skeleton_Rogue, Skeleton_Mage, Skeleton_Necromancer.
- Boss placeholder: Skeleton_Golem_Boss.
- Weapon / equipment visual attachments from Phase 15.7.
- Per-unit Animator Controller binding from Phase 15.9.2.
- Runtime animation integration from Phase 15.9.
- Smooth movement and MoveSpeed presentation.
- Four-direction facing.
- Attack / Skill target facing.
- Post-move facing toward nearest alive opponent.
- Basic Attack range per unit from Phase 15.9.1.
- Skill range still controlled by SkillData.
- DefendStart / DefendHold and defending clear synchronization.
- Hit / Dead animation.
- PlayerTurn free-order rule.
- EnemyTurn sequential action order.

## Static Asset Inventory for Validation

### Interactables

- `Assets/_BoneThrone/Prefabs/Interactables/HealthPotion.prefab`
- `Assets/_BoneThrone/Prefabs/Interactables/Key.prefab`
- `Assets/_BoneThrone/Prefabs/Interactables/Stairs.prefab`
- `Assets/_BoneThrone/Prefabs/Interactables/Chest.prefab`
- `Assets/_BoneThrone/Prefabs/Interactables/Doorway.prefab`

Expected state:

- HealthPotion remains an active pickup prefab.
- Key and Stairs retain their existing scripts.
- Chest and Doorway remain visual-only candidates with no loot, door, lock, BossDoor, or SupplyPoint behavior.

### Environment Prefabs

Environment prefabs are available under:

- `Assets/_BoneThrone/Prefabs/Environment/Architecture/`
- `Assets/_BoneThrone/Prefabs/Environment/Decor/`
- `Assets/_BoneThrone/Prefabs/Environment/Floors/`
- `Assets/_BoneThrone/Prefabs/Environment/Furniture/`
- `Assets/_BoneThrone/Prefabs/Environment/Props/`
- `Assets/_BoneThrone/Prefabs/Environment/Walls/`

Current summary from Phase 15.4:

- Total Environment prefabs: 37.
- All use the `Env_` prefix.
- Environment prefabs are static environment assets, not Interactables, Characters, Weapons, or Animations.

### Character Prefabs

Player prefabs:

- `Assets/_BoneThrone/Prefabs/Units/Players/Fighter.prefab`
- `Assets/_BoneThrone/Prefabs/Units/Players/Ranger.prefab`
- `Assets/_BoneThrone/Prefabs/Units/Players/Mage.prefab`
- `Assets/_BoneThrone/Prefabs/Units/Players/Barbarian.prefab`

Enemy prefabs:

- `Assets/_BoneThrone/Prefabs/Units/Enemies/Skeleton_Minion.prefab`
- `Assets/_BoneThrone/Prefabs/Units/Enemies/Skeleton_Warrior.prefab`
- `Assets/_BoneThrone/Prefabs/Units/Enemies/Skeleton_Rogue.prefab`
- `Assets/_BoneThrone/Prefabs/Units/Enemies/Skeleton_Mage.prefab`
- `Assets/_BoneThrone/Prefabs/Units/Enemies/Skeleton_Necromancer.prefab`

Boss placeholder:

- `Assets/_BoneThrone/Prefabs/Units/Boss/Skeleton_Golem_Boss.prefab`

Expected state:

- Ranger gameplay identity remains Ranger, even if using Rogue visual.
- Skeleton_Rogue is the ordinary Rogue enemy.
- Skeleton_Golem_Boss remains a Boss / heavy boss placeholder only.
- Skeleton_Golem is not used as a normal enemy prefab.

### Animator Controllers

Base fallback / template controllers:

- `Assets/_BoneThrone/Animation/Controllers/BT_Player_Medium.controller`
- `Assets/_BoneThrone/Animation/Controllers/BT_Skeleton_Medium.controller`
- `Assets/_BoneThrone/Animation/Controllers/BT_Boss_Large.controller`

Per-unit controllers:

- `Assets/_BoneThrone/Animation/Controllers/Units/BT_Fighter.controller`
- `Assets/_BoneThrone/Animation/Controllers/Units/BT_Ranger.controller`
- `Assets/_BoneThrone/Animation/Controllers/Units/BT_Mage.controller`
- `Assets/_BoneThrone/Animation/Controllers/Units/BT_Barbarian.controller`
- `Assets/_BoneThrone/Animation/Controllers/Units/BT_Skeleton_Minion.controller`
- `Assets/_BoneThrone/Animation/Controllers/Units/BT_Skeleton_Warrior.controller`
- `Assets/_BoneThrone/Animation/Controllers/Units/BT_Skeleton_Rogue.controller`
- `Assets/_BoneThrone/Animation/Controllers/Units/BT_Skeleton_Mage.controller`
- `Assets/_BoneThrone/Animation/Controllers/Units/BT_Skeleton_Necromancer.controller`
- `Assets/_BoneThrone/Animation/Controllers/Units/BT_Skeleton_Golem_Boss.controller`

Expected shared parameter contract:

- `MoveSpeed`
- `BasicAttack`
- `Skill`
- `Hit`
- `Defend`
- `UsePotion`
- `IsDead`
- `IsDefending`

Expected shared states:

- `Idle`
- `Move`
- `BasicAttack`
- `SkillCast`
- `Hit`
- `DefendStart`
- `DefendHold`
- `UsePotion`
- `Dead`

## Unity Play Mode Manual Test Checklist

Use this section as the Phase 15.10 validation record. Fill the `Result` field with `Pass`, `Fail`, or `Not Tested`.

If a test fails, record the symptom, likely cause, recommended minimal fix range, and whether the issue should be deferred.

### A. Basic Scene Check

| Test | Expected Result | Result | Failure Notes / Minimal Fix / Deferred |
| --- | --- | --- | --- |
| Open `GridTest.unity`. | Scene opens without Console red errors. | Not Tested |  |
| Enter Play Mode. | No missing prefab error. | Not Tested |  |
| Enter Play Mode. | No missing script error. | Not Tested |  |
| Enter Play Mode. | No missing Animator Controller error. | Not Tested |  |
| Exercise basic actions. | No missing Animator parameter warnings. | Not Tested |  |

### B. PlayerTurn

| Test | Expected Result | Result | Failure Notes / Minimal Fix / Deferred |
| --- | --- | --- | --- |
| Select any alive not-ended player unit. | Any alive not-ended player can be selected freely. | Not Tested |  |
| End Turn on selected unit. | Only the selected unit becomes ended. | Not Tested |  |
| End all alive player units. | Turn changes to EnemyTurn. | Not Tested |  |
| EnemyTurn completes. | New PlayerTurn starts and resets moved / acted / ended. | Not Tested |  |
| Repeat player selection after reset. | Single-player remains free-order; no Fighter -> Ranger -> Mage -> Barbarian fixed-order regression. | Not Tested |  |

### C. Movement / Facing

| Test | Expected Result | Result | Failure Notes / Minimal Fix / Deferred |
| --- | --- | --- | --- |
| Move a player unit to a valid tile. | Movement is smooth, not instant snapping. | Not Tested |  |
| Move along a multi-tile path. | Unit turns only North / South / East / West during path movement. | Not Tested |  |
| Finish movement near an alive opponent. | Unit faces the nearest alive opponent after movement. | Not Tested |  |
| Finish movement with no alive opponent. | Unit keeps final movement direction. | Not Tested |  |
| Inspect Animator after movement. | `MoveSpeed` returns to `0`. | Not Tested |  |

### D. Basic Attack

| Test | Expected Result | Result | Failure Notes / Minimal Fix / Deferred |
| --- | --- | --- | --- |
| Fighter Basic Attack at distance 1. | Attack is valid. | Not Tested |  |
| Fighter Basic Attack beyond distance 1. | Attack is rejected. | Not Tested |  |
| Barbarian Basic Attack at distance 1. | Attack is valid. | Not Tested |  |
| Barbarian Basic Attack beyond distance 1. | Attack is rejected. | Not Tested |  |
| Ranger Basic Attack up to distance 4. | Attack is valid within range 4 and rejected beyond. | Not Tested |  |
| Mage Basic Attack up to distance 3. | Attack is valid within range 3 and rejected beyond. | Not Tested |  |
| Skeleton_Rogue Basic Attack at distance 2. | Enemy can attack according to range 2 when AI chooses attack. | Not Tested |  |
| Skeleton_Mage / Skeleton_Necromancer Basic Attack at distance 3. | Enemy can attack according to range 3 when AI chooses attack. | Not Tested |  |
| UI Basic Attack highlight. | Highlight matches actual Basic Attack legality. | Not Tested |  |
| Basic Attack presentation. | Attacker faces target before attack. | Not Tested |  |
| Basic Attack animation. | Animation plays once only. | Not Tested |  |
| Damage / D20 / action economy. | Existing D20, damage, and action consumption rules remain unchanged. | Not Tested |  |

### E. Skills

| Test | Expected Result | Result | Failure Notes / Minimal Fix / Deferred |
| --- | --- | --- | --- |
| Target a skill. | Skill range still comes from SkillData. | Not Tested |  |
| Use a targeted skill. | Caster faces target before Skill animation. | Not Tested |  |
| Use a skill. | Skill animation plays once only. | Not Tested |  |
| Check cooldown / action. | Cooldown and action consumption rules remain unchanged. | Not Tested |  |

### F. Potion

| Test | Expected Result | Result | Failure Notes / Minimal Fix / Deferred |
| --- | --- | --- | --- |
| Select player and click HealthPotion in range. | Pickup adds potion count only and does not heal immediately. | Not Tested |  |
| Click the same HealthPotion again. | Pickup does not trigger twice. | Not Tested |  |
| Use Potion on damaged unit. | Potion heals, count decreases by 1, and action is consumed. | Not Tested |  |
| Use Potion. | Potion does not automatically End Turn. | Not Tested |  |
| Potion animation. | UsePotion animation plays once only. | Not Tested |  |

### G. Defend

| Test | Expected Result | Result | Failure Notes / Minimal Fix / Deferred |
| --- | --- | --- | --- |
| Use Defend. | DefendStart plays once. | Not Tested |  |
| After DefendStart. | Unit enters DefendHold / Blocking. | Not Tested |  |
| New PlayerTurn starts. | DefendHold exits when existing gameplay defending clear runs. | Not Tested |  |
| Defending unit is hit and defend is consumed. | DefendHold exits through existing consume / clear path. | Not Tested |  |
| Defending unit dies. | Dead state overrides DefendHold. | Not Tested |  |

### H. Hit / Dead

| Test | Expected Result | Result | Failure Notes / Minimal Fix / Deferred |
| --- | --- | --- | --- |
| Unit takes nonzero damage and survives. | Hit animation plays. | Not Tested |  |
| Unit reaches death. | Dead state activates. | Not Tested |  |
| Dead unit receives later presentation triggers. | Dead remains held and is not interrupted by Hit / Attack / Defend / UsePotion. | Not Tested |  |
| Try to act with dead unit. | Dead unit cannot continue acting. | Not Tested |  |

### I. EnemyTurn

| Test | Expected Result | Result | Failure Notes / Minimal Fix / Deferred |
| --- | --- | --- | --- |
| End all alive player turns. | EnemyTurn starts. | Not Tested |  |
| Observe EnemyTurn. | Enemies act one by one in stable order. | Not Tested |  |
| Bleed enemy before its action. | Bleed ticks before that enemy action. | Not Tested |  |
| Stun enemy before its action. | Stun skips that enemy movement + action. | Not Tested |  |
| Enemy moves with smooth movement. | Smooth movement does not stall EnemyTurn. | Not Tested |  |
| EnemyTurn completes. | Turn returns to PlayerTurn. | Not Tested |  |

### J. Prefab / Art

| Test | Expected Result | Result | Failure Notes / Minimal Fix / Deferred |
| --- | --- | --- | --- |
| Inspect player prefabs in Play Mode. | Weapon / equipment visual attachments are present and not missing. | Not Tested |  |
| Inspect enemy prefabs in Play Mode. | Weapon / equipment visual attachments are present and not missing. | Not Tested |  |
| Inspect each character Animator. | Each character uses its per-unit Animator Controller. | Not Tested |  |
| Inspect Ranger. | Gameplay identity remains Ranger. | Not Tested |  |
| Inspect Skeleton_Rogue. | It remains Skeleton_Rogue, not Skeleton_Golem. | Not Tested |  |
| Inspect Skeleton_Golem_Boss if placed or tested manually. | It remains Boss placeholder only. | Not Tested |  |
| Inspect Chest / Doorway. | They remain visual-only candidates with no loot / lock / BossDoor behavior. | Not Tested |  |

## Failure Record Template

Use this template for each failed item:

```text
Test:
Result:
Symptom:
Likely cause:
Recommended minimal fix range:
Deferred: Yes / No
Notes:
```

Recommended minimal fix range examples:

- Missing prefab reference: project-owned prefab only, or scene instance only if GridTest has a broken override.
- Missing UnitAnimationController: project-owned unit prefab first; GridTest scene instance only if it is unpacked or has a removal override.
- Missing Animator parameter: relevant project-owned Animator Controller only.
- Basic Attack range mismatch: `UnitStats`, `AttackRangeService`, or specific project-owned Unit prefab range value only.
- Potion count mismatch: `UnitPotionState`, `HealthPotionPickup`, `PotionSystem`, or UI display binding only.
- EnemyTurn stall: `EnemyTurnRunner` only unless evidence points elsewhere.

## Deferred to Phase 15.11+

The following issues should not expand Phase 15.10 unless explicitly re-scoped:

- Formal data assetization plan -> Phase 15.11.
- Formal Level scene planning -> Phase 15.12.
- `Level_01`, `Level_02`, `Level_03` scene setup -> Phase 15.13.
- Boss fight, Boss AI, Boss skills, and boss battle flow.
- BossDoor, BossKey, and SupplyPoint complete flow -> Phase 15.16.
- Chest loot / reward / inventory system.
- Door open / close / lock / unlock system.
- Environment collider refinement and blocking / walkable semantics beyond minimal validation notes.
- Large animation controller redesign or new animation import settings.
- LAN / Networking.
- Internet lobby, matchmaking, account, or online systems.

## Closeout Rule

Phase 15.10 should close only after manual Unity 6.3 Play Mode validation has been completed and the checklist above is updated with results.

`Docs/ACTIVE_TASK.md` should only be moved to Phase 15.11 after Phase 15.10 is closed.

If any item fails:

1. Record the failure in this document.
2. Identify the smallest likely fix range.
3. Mark whether it is Phase 15.10 blocking or deferred.
4. Get explicit approval before modifying scene, prefab, code, SkillData, KayKit source, materials, or animation assets.

## Closeout - Unity Play Mode Validation Passed

Phase 15.10 has been manually validated in Unity 6.3 Play Mode and is closed.

Validation result summary:

| Section | Area | Result | Notes |
| --- | --- | --- | --- |
| A | Basic Scene Check | Pass | GridTest opened and entered Play Mode without blocking missing prefab, missing script, missing Animator Controller, or missing parameter issues. |
| B | PlayerTurn | Pass | Single-player free-order PlayerTurn remains intact; End Turn only ends the selected unit; all alive player units ending advances to EnemyTurn; new PlayerTurn resets moved / acted / ended. |
| C | Movement / Facing | Pass | Movement is smooth, four-direction facing works, units face nearest alive opponent after movement, and MoveSpeed returns to 0. |
| D | Basic Attack | Pass | Per-unit Basic Attack ranges validated for melee, Ranger, Mage, Skeleton_Rogue, Skeleton_Mage, and Skeleton_Necromancer; attack facing and one-shot animation behavior validated; D20 / damage / action rules unchanged. |
| E | Skills | Pass | Skill range still comes from SkillData; targeted skills face target; skill animation plays once; cooldown and action rules unchanged. |
| F | Potion | Pass | HealthPotion pickup adds count without healing; Potion use heals, consumes count and action, does not auto End Turn, and plays once. |
| G | Defend | Pass | DefendStart plays once, DefendHold / Blocking holds, new round and consumed defend clear the hold, and Dead remains higher priority. |
| H | Hit / Dead | Pass | Hit plays on surviving damaged units; Dead activates and holds; dead units cannot continue acting. |
| I | EnemyTurn | Pass | Enemies act sequentially; bleed / stun timing is preserved; smooth movement does not stall EnemyTurn; EnemyTurn returns to PlayerTurn. |
| J | Prefab / Art | Pass | Weapon visuals, per-unit Animator Controllers, Ranger identity, Skeleton_Rogue naming, Skeleton_Golem_Boss placeholder boundary, and Chest / Doorway visual-only status were validated. |

Known deferred items:

- Formal data assetization planning moves to Phase 15.11.
- Formal Level scene planning remains deferred to Phase 15.12.
- `Level_01`, `Level_02`, and `Level_03` setup remains deferred to Phase 15.13.
- Boss fight, Boss AI, BossDoor, BossKey, and SupplyPoint complete flow remain deferred to later dedicated phases.
- Chest loot / reward behavior and Door lock / unlock behavior remain deferred.
- Environment collider refinement, blocking / walkable semantics, and formal placement rules remain deferred.

Closeout boundary confirmation:

- `GridTest.unity` remains the regression baseline, not a formal level.
- No scene was modified.
- No prefab was modified.
- No C# code was modified.
- No SkillData was modified.
- No KayKit source asset was modified.
- No material, animation clip, Animator Controller, ProjectSettings, or Packages file was modified.
- No Boss, LAN, formal Level scene, BossDoor, BossKey, or SupplyPoint implementation was added.

Next phase:

- `Docs/ACTIVE_TASK.md` is updated to Phase 15.11 - Formal Data Assetization Plan.
- Phase 15.11 should remain planning-focused and must not directly create formal Level scenes or implement formal level flow.

## Rollback

This pass is docs-only. Rollback by deleting this validation record:

```powershell
Remove-Item -LiteralPath "Docs/DevLogs/Phase15.10_GridTestSceneAssemblyKitValidation.md" -Force
```

Before rollback, inspect the worktree:

```powershell
git status --short
```
