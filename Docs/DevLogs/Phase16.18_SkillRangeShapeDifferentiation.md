# Phase 16.18 DevLog - Skill Range Shape Differentiation

## Date
2026-06-10

## Scope
Implemented a narrow C# behavior change for skill target range shapes.

Changed scripts only:

- `Assets/_BoneThrone/Scripts/Skills/SkillData.cs`
- `Assets/_BoneThrone/Scripts/Skills/SkillTargetingService.cs`
- `Assets/_BoneThrone/Scripts/Skills/SkillSystem.cs`
- `Assets/_BoneThrone/Scripts/UI/UIActionModeController.cs`

Not changed:

- No scenes.
- No prefabs.
- No SkillData `.asset` files.
- No damage formulas.
- No cooldown values.
- No UI layout.
- No networking.

## Implementation
Added `SkillRangeShape` to `SkillData`:

- `Automatic`
- `Manhattan`
- `Chebyshev`
- `EightWayLine`

Existing skill assets can stay on the default serialized value, `Automatic`.

Automatic range shape rules:

- Fighter skills: `Manhattan`.
- Ranger skills: `EightWayLine`.
- Mage skills: `Chebyshev`.
- Barbarian skills: `Chebyshev`.
- Unknown skills: fallback to `Manhattan`.

Gameplay validation now uses the resolved skill range shape in `SkillTargetingService.IsInRange`.

HUD skill yellow highlights now ask `SkillSystem.TryFillSkillRangeTiles`, which delegates to `SkillTargetingService`. This keeps target preview and real skill validation on the same rules.

## Expected Gameplay Result
Skill ranges are no longer all the same Manhattan/cross-style shape.

Examples:

- Fighter melee skills keep the existing four-direction close-combat feel.
- Barbarian range-1 skills can target all 8 surrounding tiles, matching wide axe swings such as Heavy Cleave.
- Ranger skills target along 8 straight rays, matching arrow lanes.
- Mage skills use a square/caster-centered spell reach, including diagonals.

## Inspector Setup
No required Inspector setup.

Optional follow-up:

- Open any `SkillData` asset.
- Leave `Range Shape` as `Automatic` for the default role-based behavior.
- Override `Range Shape` only when a specific skill needs a custom pattern.

## Manual Play Mode Steps
1. Open `Assets/_BoneThrone/Scenes/GridTest.unity`.
2. Enter Play Mode and confirm there are no red Console errors.
3. Select Fighter and click a skill button. Expected: skill range keeps the familiar Manhattan close-combat pattern.
4. Select Barbarian and click Heavy Cleave. Expected: yellow skill range includes the 8 surrounding tiles when range is 1.
5. Select Ranger and click a skill button. Expected: yellow skill range follows straight horizontal, vertical, and diagonal rays.
6. Select Mage and click Fireball or another mage skill. Expected: yellow skill range includes diagonal tiles within a square reach.
7. Click highlighted valid enemy targets for each role. Expected: accepted targets match the highlighted range.
8. Click non-highlighted or out-of-shape targets. Expected: skill is rejected and does not consume the action.
9. Re-run Move, Basic Attack, Skill Slot 0/1/2, Fireball splash, CombatLog, Enemy Floating HP Bar, Room, Key/Stairs, LevelUp, and Enemy AI regression checks.

## Risks
- Ranger skills are intentionally more directional than before; this may make some positions that were previously legal under Manhattan range invalid unless they lie on a straight or diagonal ray.
- Mage and Barbarian skills can reach diagonal tiles under `Automatic`, increasing tactical flexibility.
- The change does not add line-of-sight or obstacle blocking.
- The change does not change splash shape; Fireball and Arcane Burst splash still use the existing adjacent-Manhattan splash logic.

## Rollback
Revert these files:

```powershell
git checkout -- Assets/_BoneThrone/Scripts/Skills/SkillData.cs
git checkout -- Assets/_BoneThrone/Scripts/Skills/SkillTargetingService.cs
git checkout -- Assets/_BoneThrone/Scripts/Skills/SkillSystem.cs
git checkout -- Assets/_BoneThrone/Scripts/UI/UIActionModeController.cs
git checkout -- Docs/DevLogs/Phase16.18_SkillRangeShapeDifferentiation.md
```
