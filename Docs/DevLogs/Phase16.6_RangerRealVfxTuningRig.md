# Phase 16.6 - Ranger Real VFX Tuning Rig

## Purpose

Replace the failed Ranger gizmo / ghost preview approach with a scene-only tuning rig made from real GameObjects.

The old approach was removed because it could show editor handles, spheres, or wireframes without showing the actual arrow mesh and combat VFX clearly enough to tune.

## What Changed

- Removed editor-only preview fields from `RangerHitPresentationConfig`.
- Deleted the failed `RangerHitPresentationConfigEditor` ghost preview Inspector.
- Added `RangerVfxTuningRig`, a scene-only component that captures real scene transforms.
- Added `RangerVfxTuningRigEditor`, with menu item:

```text
BoneThrone/VFX/Ranger/Create Real Tuning Rig
```

## Tuning Workflow

1. Open a safe test scene, not a formal production level.
2. Run `BoneThrone/VFX/Ranger/Create Real Tuning Rig`.
3. In the generated `Ranger_VFX_TuningRig`, move/rotate/scale:
   - `Arrow_Preview_REAL_PREFAB_MOVE_ROTATE_SCALE`
   - `Arrow_Start_MOVE_THIS`
   - `Arrow_Impact_STICKS_HERE`
   - `Skill1_Precision_REAL_PREFAB_MOVE_ROTATE_SCALE`
   - `Skill2_Quick_REAL_PREFAB_MOVE_ROTATE_SCALE`
   - `Skill3_Piercing_REAL_PREFAB_MOVE_ROTATE_SCALE`
4. Select `Ranger_VFX_TuningRig`.
5. Click `Refresh Real Particle Preview` if particle effects need to be resimulated in Scene view.
6. Click `Apply Scene Transforms To Ranger Prefab`.
7. Test Ranger basic attack and three skills in Play Mode.

## Expected Result

- The generated scene rig uses real arrow, real VFX prefabs, and a real skeleton target body.
- Designers tune by dragging Unity transforms, not by typing offset numbers.
- Applying the rig writes only the Ranger runtime presentation values:
  - arrow start distance
  - arrow rotation offset
  - arrow world scale
  - embedded arrow local offset
  - three skill local offsets / rotations / scales

## Risk

- The generated rig is a temporary tuning object. Do not save it into formal level scenes.
- The apply button modifies `Assets/_BoneThrone/Prefabs/Units/Players/Ranger.prefab`.
- If particle prefabs are authored as short bursts, use `Refresh Real Particle Preview` after changing the preview time or transforms.

## Rollback

- Delete `Assets/_BoneThrone/Scripts/Units/RangerVfxTuningRig.cs`.
- Delete `Assets/_BoneThrone/Editor/RangerVfxTuningRigEditor.cs`.
- Revert `Assets/_BoneThrone/Prefabs/Units/Players/Ranger.prefab` to the previous saved Ranger presentation values.
