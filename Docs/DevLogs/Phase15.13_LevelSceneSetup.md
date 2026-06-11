# Phase 15.13 - Level_01 / Level_02 / Level_03 Scene Setup

## Goal

Create the first narrow formal scene skeletons for the three planned dungeon floors:

- `Assets/_BoneThrone/Scenes/Level_01.unity`
- `Assets/_BoneThrone/Scenes/Level_02.unity`
- `Assets/_BoneThrone/Scenes/Level_03.unity`

This phase intentionally creates scene structure only. It does not build a full playable floor.

## Boundary

Changed files:

- `Assets/_BoneThrone/Scenes/Level_01.unity`
- `Assets/_BoneThrone/Scenes/Level_01.unity.meta`
- `Assets/_BoneThrone/Scenes/Level_02.unity`
- `Assets/_BoneThrone/Scenes/Level_02.unity.meta`
- `Assets/_BoneThrone/Scenes/Level_03.unity`
- `Assets/_BoneThrone/Scenes/Level_03.unity.meta`
- `Docs/DevLogs/Phase15.13_LevelSceneSetup.md`
- `Docs/ACTIVE_TASK.md`

No changes were made to:

- `Assets/_BoneThrone/Scenes/GridTest.unity`
- C# scripts
- prefabs
- SkillData
- KayKit source assets
- ScriptableObject assets
- materials
- animations
- ProjectSettings
- Packages

## Implementation Choice

The formal scenes were created from scratch as minimal Unity scene YAML skeletons instead of copying `GridTest.unity`.

Reason:

- `GridTest.unity` is the regression baseline and was dirty in the open Unity Editor session.
- Direct scene creation / save operations could risk touching or replacing the open regression scene.
- The new scenes should avoid copying GridTest-only tester objects, debug rigs, temporary overrides, and regression-only layout.

The new scenes contain only native empty `GameObject` / `Transform` hierarchy plus scene render / lightmap / navmesh settings. They do not instantiate prefabs or attach scripts in this phase, which keeps missing script / missing prefab / missing reference risk low.

## Scene Structure

Each scene has a single root:

- `Level_01_Root`
- `Level_02_Root`
- `Level_03_Root`

Shared top-level groups:

- `00_Scene_Context`
- `01_Systems_Managers`
- `02_Cameras_Lighting`
- `03_Grid_Blockout`
- `04_Rooms_*`
- `05_Units_Placement_Placeholders`
- `06_Interactables_*_Placeholders`
- `07_Dressing_*`
- `08_Debug_None_Do_Not_Copy_GridTest_Testers`
- `99_Deferred_*`

Shared systems / managers placeholder groups under `01_Systems_Managers`:

- `Grid_Tile_Systems_To_Wire`
- `Turn_Combat_Skill_Potion_Systems_To_Wire`
- `Selection_Input_Movement_Systems_To_Wire`
- `Room_Reveal_Progression_Placeholders_To_Wire_Later`
- `BattleHUD_UI_To_Wire`

These are intentionally placeholders. Actual manager components and references should be wired in a later approved scene implementation phase.

## Per-Level Room Skeleton

### Level_01

Planned as onboarding / first slice foundation:

- `Room_01_Start_Player_Spawn`
- `Room_02_Simple_Combat_Minion_Warrior`
- `Room_03_Optional_HealthPotion_Pocket`
- `Room_04_Key_Room`
- `Room_05_Stair_Exit_To_Level_02_Future`

### Level_02

Planned as medium escalation / branch foundation:

- `Room_01_Start_Player_Spawn`
- `Room_02_Branch_Combat_Warrior_Rogue`
- `Room_03_Optional_Dressing_Visual_Reward`
- `Room_04_Key_Room_Harder_Encounter`
- `Room_05_Elite_Combat_Rogue_Mage_Minion`
- `Room_06_Stair_Exit_To_Level_03_Future`

### Level_03

Planned as high escalation / final-floor placeholder foundation:

- `Room_01_Start_Player_Spawn`
- `Room_02_Mixed_Combat_Rogue_Mage_Warrior`
- `Room_03_Key_Or_Route_Split_Concept`
- `Room_04_Hard_Combat_Necromancer_Intro`
- `Room_05_Boss_Preparation_Planning_Only_No_SupplyPoint`
- `Room_06_Boss_Room_Placeholder_No_BossFight`

`Skeleton_Golem` is not used as a normal enemy. `Skeleton_Golem_Boss` is not instantiated. The boss room remains a naming / planning placeholder only.

## Unity-Skills Validation

Unity-Skills was available and used.

Server status:

- `UnitySkills 2.0.0`
- Unity `6000.3.10f1`
- project `Bone Throne`
- mode `bypass`

Validation performed:

- Refreshed AssetDatabase with `asset_refresh`.
- Queried `asset_find` with `t:Scene Level_`; Unity recognized all three new scenes as `SceneAsset`.
- Queried `asset_get_info` for all three scenes; each had a unique GUID.
- Loaded each new scene additively with `scene_load`.
- Set each new scene active temporarily with `scene_set_active`.
- Read `scene_get_info` and `scene_get_hierarchy`.
- Switched active scene back to `GridTest`.
- Unloaded each new scene with `scene_unload`.
- Confirmed only `GridTest` remained loaded afterward.
- Confirmed Unity was not compiling or updating with `debug_check_compilation`.

Observed results:

- `Level_01`: loaded successfully, `isDirty: false`, one root object, 10 root children.
- `Level_02`: loaded successfully, `isDirty: false`, one root object, 10 root children.
- `Level_03`: loaded successfully, `isDirty: false`, one root object, 10 root children.
- Static scan found no `m_Script` entries.
- Static scan found no nonzero prefab instance references.
- `GridTest.unity` had no disk diff from this phase.

Note:

- The open Unity Editor reported `GridTest` as dirty before and after validation. This phase did not save it.

## Unity Inspector Setup

For this phase, no Inspector component setup is required.

When opening each scene in Unity, verify:

- The scene root exists.
- Top-level groups are visible and named clearly.
- `01_Systems_Managers` contains the expected placeholder groups.
- `04_Rooms_*` contains the expected per-floor room skeleton.
- No scripts, prefab instances, SkillData, or KayKit source objects were added.

## Play Mode Steps

Do not use these scenes for gameplay regression yet.

Manual check:

1. Open `Level_01.unity`, `Level_02.unity`, and `Level_03.unity` one at a time.
2. Confirm each scene opens without red Console errors.
3. Confirm the hierarchy is visible.
4. Do not enter Play Mode expecting gameplay; systems are placeholders only.
5. Run gameplay regression in `GridTest.unity`, not in the new formal scenes.

## Expected Result

- Three formal scenes exist and can be opened.
- The scenes have a stable hierarchy skeleton for future setup.
- There are no missing scripts because no scripts were attached.
- There are no missing prefab references because no prefabs were instantiated.
- `GridTest.unity` remains the regression baseline and was not modified on disk.
- Single-player PlayerTurn behavior is untouched.

## Risks

- The formal scenes are not playable yet.
- Cameras, lights, UI, grid tiles, managers, units, and interactables still need later approved wiring.
- The placeholder hierarchy may need adjustment after actual manager migration begins.
- `Level_03` contains boss-planning room names only; boss fight, BossDoor, BossKey, SupplyPoint, Victory, Defeat, and LAN remain deferred.
- Since `GridTest` was already dirty in the open Editor session, avoid saving it accidentally during unrelated manual testing.

## Rollback

To roll back Phase 15.13, delete only:

- `Assets/_BoneThrone/Scenes/Level_01.unity`
- `Assets/_BoneThrone/Scenes/Level_01.unity.meta`
- `Assets/_BoneThrone/Scenes/Level_02.unity`
- `Assets/_BoneThrone/Scenes/Level_02.unity.meta`
- `Assets/_BoneThrone/Scenes/Level_03.unity`
- `Assets/_BoneThrone/Scenes/Level_03.unity.meta`
- `Docs/DevLogs/Phase15.13_LevelSceneSetup.md`

Then restore `Docs/ACTIVE_TASK.md` to the Phase 15.13 state.

## Next Phase

Phase 15.14 should begin `Level_01 Playable Slice` planning / implementation.

Recommended first decisions:

1. Whether to wire `Level_01` managers manually or through a small editor-only setup workflow.
2. Which exact GridTest manager objects are safe to reproduce.
3. Which minimum player, enemy, tile, UI, HealthPotion, Key, and Stairs instances belong in the first playable slice.
4. How to validate `Level_01` without changing the `GridTest` regression baseline.
