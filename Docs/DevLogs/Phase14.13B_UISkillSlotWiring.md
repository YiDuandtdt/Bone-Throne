# Phase 14.13-B UI Skill Slot Wiring

## Purpose and scope

Phase 14.13-B wires Skill Slot 1 and Skill Slot 2 into the existing BattleHUD / SkillBar / UI action targeting flow.

This phase only connects UI slot buttons to existing `SkillSystem` slot-index execution. It does not add role-specific Slot 1 / Slot 2 effects and does not change skill formulas.

## Files changed

- `Assets/_BoneThrone/Scripts/UI/SkillBarView.cs`
- `Assets/_BoneThrone/Scripts/UI/BattleHUDController.cs`
- `Assets/_BoneThrone/Scripts/UI/UIActionModeController.cs`
- `Docs/DevLogs/Phase14.13B_UISkillSlotWiring.md`

## UI slot wiring

### SkillBarView

- Added `slot1Button` and `slot2Button` support.
- Added `SkillSlot1Clicked` and `SkillSlot2Clicked` events.
- Kept existing `SkillSlot0Clicked` event.
- Runtime layout `SkillSlot1` and `SkillSlot2` buttons now bind to their click handlers when present.
- Generalized slot refresh into a slot-index based method.
- Slot 0 behavior remains the same path with slot index `0`.
- Slot 1 and Slot 2 now show current `SkillRuntime` state:
  - `Empty`
  - `Locked`
  - `Cooldown X`
  - `Ready`
- Defend and Potion remain placeholder buttons and are not enabled.

### BattleHUDController

- Subscribes to `SkillSlot1Clicked` and `SkillSlot2Clicked`.
- Keeps `HandleSkillSlot0Clicked()` and forwards it to the common slot handler.
- Added common `HandleSkillSlotClicked(int slotIndex)` path.
- Calls `UIActionModeController.HandleSkillSlotButtonClicked(slotIndex)`.
- Does not directly modify HP, cooldown, acted, moved, combat, or skill state.

### UIActionModeController

- Added public `HandleSkillSlotButtonClicked(int slotIndex)`.
- Kept `HandleSkillSlot0ButtonClicked()` as a compatibility wrapper around slot index `0`.
- Reused existing `pendingSkillSlotIndex`, `EnterSkillTargeting(slotIndex)`, and `ShowSkillTargets(slotIndex)`.
- Target preview still calls only `SkillSystem.CanUseSkillOnTarget(caster, enemy, slotIndex, out reason)`.
- Target preview still does not call `TryUseSkill`.
- Target preview still does not call `TryBasicAttack`.
- Target click execution still calls `SkillSystem.TryUseSkill(caster, target, pendingSkillSlotIndex)`.
- Right-click cancel targeting semantics are unchanged.

## Explicit non-changes

- No `SkillData` assets were modified.
- No Player prefabs were modified.
- No scene files were modified.
- `SkillEffectExecutor` was not modified.
- `FighterSkillEffects`, `RangerSkillEffects`, `MageSkillEffects`, and `BarbarianSkillEffects` were not modified.
- `SkillSystem` was not modified.
- `SkillTargetingService` was not modified.
- `DamageResolver` was not modified.
- `CombatSystem` was not modified.
- No enemy prefabs were modified.
- No KayKit original assets were modified.
- Phase 14.10 camera controls were not modified.
- Phase 14.11 `ActiveUnitProvider` behavior was not modified.

## Slot 1 / Slot 2 current effect state

Slot 1 and Slot 2 can now enter targeting and execute through `SkillSystem` when their `SkillRuntime` slot contains a valid unlocked `SkillData`.

Until Phase 14.13-C adds role-specific effect branches, Slot 1 and Slot 2 skills may use the existing `SkillEffectExecutor` generic fallback guaranteed-damage behavior. This keeps structured `SkillEffectResult` and CombatLog feedback without changing formulas in this phase.

## Play Mode test steps

1. Open `Assets/_BoneThrone/Scenes/GridTest.unity`.
2. Confirm there are no compile errors.
3. Enter Play Mode.
4. Select each player role.
5. Confirm Slot 0 still displays and behaves as before.
6. Confirm Slot 1 and Slot 2 display `Empty`, `Locked`, `Cooldown X`, or `Ready` according to `SkillRuntime` and unit level.
7. If Slot 1 is unlocked, click Slot 1 and confirm skill targeting begins.
8. Confirm Slot 1 target highlight uses valid active enemy targets.
9. Click a valid enemy and confirm execution goes through `SkillSystem`.
10. If Slot 2 is unlocked, click Slot 2 and confirm skill targeting begins.
11. Confirm Slot 2 target highlight uses valid active enemy targets.
12. Click a valid enemy and confirm execution goes through `SkillSystem`.
13. Confirm locked skills show unavailable behavior and do not execute.
14. Confirm target preview does not execute skills before target click.
15. Confirm CombatLog receives structured skill damage and cooldown feedback.
16. Confirm ActiveUnitProvider target discovery still works.
17. Confirm right-click still cancels targeting.
18. Confirm Phase 14.10 camera controls still work:
    - middle mouse drag
    - mouse wheel zoom
    - right mouse yaw / pitch rotation
19. Confirm Console has no new red errors.

## Risks

- Slot 1 / Slot 2 effects are intentionally generic until Phase 14.13-C.
- If a selected unit level is lower than a skill's `unlockLevel`, the button may display `Locked` and execution should be rejected by the existing validation path.
- If `SkillRuntime` slot references are missing, the UI displays `Empty` and targeting should not start.
- This phase keeps right-click cancel targeting unchanged; camera right-drag behavior should still be verified manually after UI targeting tests.

## Rollback

To roll back this phase:

1. Revert `Assets/_BoneThrone/Scripts/UI/SkillBarView.cs`.
2. Revert `Assets/_BoneThrone/Scripts/UI/BattleHUDController.cs`.
3. Revert `Assets/_BoneThrone/Scripts/UI/UIActionModeController.cs`.
4. Delete `Docs/DevLogs/Phase14.13B_UISkillSlotWiring.md`.

No asset, prefab, scene, combat, skill formula, KayKit, camera, or ActiveUnitProvider rollback is needed because those files were not modified.

## Recommended next phase

Proceed to Phase 14.13-C - SkillEffectExecutor Role-Specific Slot 1/2 Effects, only after Play Mode confirms Slot 1 and Slot 2 UI selection and execution route through the existing `SkillSystem` path.
