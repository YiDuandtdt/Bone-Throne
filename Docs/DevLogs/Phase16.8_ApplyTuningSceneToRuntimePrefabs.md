# Phase 16.8 - Apply Tuning Scene To Runtime Prefabs

## Purpose

Use the artist-tuned objects in `Assets/_BoneThrone/Scenes/VFX_Tuning.unity` as the source of truth for runtime hit presentation values.

## Runtime Timing

- Arrow users:
  - attacker animation plays first
  - arrow flight plays after the attack animation impact delay
  - damage is applied after arrow flight
  - target Hit animation plays through `DamageResolver`
  - attached hit VFX appears on the same impact frame
- Mage / dark spell users:
  - attacker animation plays first
  - damage is applied after the skill/basic impact delay
  - target Hit animation plays through `DamageResolver`
  - hit VFX appears on the same impact frame

## New Runtime Component

`MageHitPresentationConfig` stores target-attached effects for:

- Mage basic attack impact
- `mage_fireball`
- `mage_frost_bolt`
- `mage_arcane_burst`
- optional `mage_arcane_burst` area nova

## New Apply Menu

```text
BoneThrone/VFX/Tuning/Apply Current Tuning Scene To Runtime Prefabs
```

It reads the currently open `VFX_Tuning.unity` scene and applies:

- `Ranger_VFX_TuningRig` -> `Assets/_BoneThrone/Prefabs/Units/Players/Ranger.prefab`
- `Mage_VFX_TuningRig` -> `Assets/_BoneThrone/Prefabs/Units/Players/Mage.prefab`
- `Enemy_Reusable_NormalArrow_REAL_PREFAB` -> `Assets/_BoneThrone/Prefabs/Units/Enemies/Skeleton_Rogue.prefab`
- `Enemy_Reusable_BasicDarkSpell_REAL_PREFAB` -> `Assets/_BoneThrone/Prefabs/Units/Enemies/Skeleton_Mage.prefab`

## Usage

1. Save the adjusted tuning scene.
2. Wait for Unity to finish compiling after script changes.
3. Open `Assets/_BoneThrone/Scenes/VFX_Tuning.unity`.
4. Run `BoneThrone/VFX/Tuning/Apply Current Tuning Scene To Runtime Prefabs`.
5. Open the real battle test scene and test:
   - Ranger basic attack
   - Ranger skill 1 / 2 / 3
   - Mage basic attack
   - Mage skill 1 / 2 / 3
   - Skeleton Rogue basic attack
   - Skeleton Mage basic attack

## Notes

- Enemy side keeps only one reusable normal arrow and one reusable basic dark spell.
- If the Unity-generated `.csproj` has not refreshed yet, external `dotnet build` can temporarily report the new component as missing. Unity fixes this after asset refresh / script compilation.
