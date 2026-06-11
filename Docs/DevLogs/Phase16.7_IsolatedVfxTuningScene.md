# Phase 16.7 - Isolated VFX Tuning Scene

## Purpose

Move Ranger and Mage VFX tuning out of active combat scenes and into a dedicated temporary scene.

## New Menu Entries

```text
BoneThrone/VFX/Tuning/Open Dedicated VFX Tuning Scene
BoneThrone/VFX/Tuning/Clean Current Scene Generated Rigs
BoneThrone/VFX/Ranger/Create Real Tuning Rig
```

`BoneThrone/VFX/Ranger/Create Real Tuning Rig` now redirects to the dedicated scene workflow instead of spawning objects in the current combat scene.

## Dedicated Scene

The tool creates or opens:

```text
Assets/_BoneThrone/Scenes/VFX_Tuning.unity
```

Scene contents:

- `Ranger_VFX_TuningRig`
  - real Ranger arrow preview
  - real enemy target body
  - real Ranger skill 1 / 2 / 3 effect prefabs
- `Mage_VFX_TuningRig`
  - real Mage attacker reference
  - real enemy target body
  - real Mage basic attack impact preview
  - real Mage fireball / frost bolt / arcane burst previews
- `Enemy_Reusable_VFX_Only_Arrow_And_DarkSpell`
  - enemy ranger-like skeleton reference
  - enemy mage skeleton reference
  - one reusable normal arrow preview
  - one reusable basic dark spell preview

## Cleanup Behavior

On editor script reload, generated tuning roots are removed automatically from non-tuning scenes if their root names match:

- `BoneThrone_VFX_Tuning_Scene_Root`
- `Ranger_VFX_TuningRig`
- `Mage_VFX_TuningRig`

Manual cleanup is also available from:

```text
BoneThrone/VFX/Tuning/Clean Current Scene Generated Rigs
```

## Particle Warning Fix

Particle preview simulation now stops and clears systems before changing random seed values.

This avoids Unity warnings such as:

```text
Setting the random seed while system is still playing is not supported.
```

## VFX Selection Checklist

### Player Mage

- Basic attack impact: small blue / neutral magic hit on target.
- `mage_fireball`: fire impact on target, plus optional small area burst.
- `mage_frost_bolt`: frost / ice impact on target.
- `mage_arcane_burst`: arcane impact on target, plus optional area nova.

### Enemy Ranger-like Unit

- Keep only one normal arrow set.
- Reuse the same embedded-arrow workflow as player Ranger if needed.
- Do not keep multiple enemy arrow variants until one is chosen.

### Enemy Mage / Necromancer

- Keep only one basic dark spell.
- Recommended reuse candidate: soul / purple dark impact.
- Do not keep separate fire, frost, lightning, and arcane sets for first pass enemy casters.

## Verification

- `dotnet build Assembly-CSharp.csproj -nologo`
- `dotnet build Assembly-CSharp-Editor.csproj -nologo`

Both builds passed with 0 errors.

## Risk And Rollback

- The dedicated scene is a temporary tuning workspace, not a formal gameplay level.
- The cleanup only targets generated tuning root object names.
- Rollback by deleting `Assets/_BoneThrone/Scenes/VFX_Tuning.unity` and reverting `RangerVfxTuningRigEditor.cs`.
