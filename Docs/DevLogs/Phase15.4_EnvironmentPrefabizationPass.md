# Phase 15.4 - Environment Prefabization Pass

## Summary

Phase 15.4 created the first batch of project-owned Environment prefabs from actual KayKit Dungeon Remastered source prefabs.

The source prefabs remain under `Assets/_BoneThrone/Art/KayKit - Dungeon Remastered Pack (for Unity)/Prefabs/`. The project-owned copies were created under `Assets/_BoneThrone/Prefabs/Environment/`.

This phase only handled Environment / static prop assets.

## Created Prefabs

| Source Prefab | Target Prefab |
| --- | --- |
| `Assets/_BoneThrone/Art/KayKit - Dungeon Remastered Pack (for Unity)/Prefabs/floor_tile_large.prefab` | `Assets/_BoneThrone/Prefabs/Environment/Floors/Env_Floor_Tile_Large.prefab` |
| `Assets/_BoneThrone/Art/KayKit - Dungeon Remastered Pack (for Unity)/Prefabs/floor_tile_small.prefab` | `Assets/_BoneThrone/Prefabs/Environment/Floors/Env_Floor_Tile_Small.prefab` |
| `Assets/_BoneThrone/Art/KayKit - Dungeon Remastered Pack (for Unity)/Prefabs/floor_tile_small_broken_A.prefab` | `Assets/_BoneThrone/Prefabs/Environment/Floors/Env_Floor_Tile_Broken_A.prefab` |
| `Assets/_BoneThrone/Art/KayKit - Dungeon Remastered Pack (for Unity)/Prefabs/wall.prefab` | `Assets/_BoneThrone/Prefabs/Environment/Walls/Env_Wall_Straight.prefab` |
| `Assets/_BoneThrone/Art/KayKit - Dungeon Remastered Pack (for Unity)/Prefabs/wall_corner.prefab` | `Assets/_BoneThrone/Prefabs/Environment/Walls/Env_Wall_Corner.prefab` |
| `Assets/_BoneThrone/Art/KayKit - Dungeon Remastered Pack (for Unity)/Prefabs/wall_endcap.prefab` | `Assets/_BoneThrone/Prefabs/Environment/Walls/Env_Wall_Endcap.prefab` |
| `Assets/_BoneThrone/Art/KayKit - Dungeon Remastered Pack (for Unity)/Prefabs/column.prefab` | `Assets/_BoneThrone/Prefabs/Environment/Architecture/Env_Column.prefab` |
| `Assets/_BoneThrone/Art/KayKit - Dungeon Remastered Pack (for Unity)/Prefabs/pillar.prefab` | `Assets/_BoneThrone/Prefabs/Environment/Architecture/Env_Pillar.prefab` |
| `Assets/_BoneThrone/Art/KayKit - Dungeon Remastered Pack (for Unity)/Prefabs/crate_small.prefab` | `Assets/_BoneThrone/Prefabs/Environment/Props/Env_Crate_Small.prefab` |
| `Assets/_BoneThrone/Art/KayKit - Dungeon Remastered Pack (for Unity)/Prefabs/barrel_small.prefab` | `Assets/_BoneThrone/Prefabs/Environment/Props/Env_Barrel_Small.prefab` |
| `Assets/_BoneThrone/Art/KayKit - Dungeon Remastered Pack (for Unity)/Prefabs/rubble_half.prefab` | `Assets/_BoneThrone/Prefabs/Environment/Props/Env_Rubble_Half.prefab` |
| `Assets/_BoneThrone/Art/KayKit - Dungeon Remastered Pack (for Unity)/Prefabs/rocks_small.prefab` | `Assets/_BoneThrone/Prefabs/Environment/Props/Env_Rocks_Small.prefab` |
| `Assets/_BoneThrone/Art/KayKit - Dungeon Remastered Pack (for Unity)/Prefabs/torch_mounted.prefab` | `Assets/_BoneThrone/Prefabs/Environment/Props/Env_Torch_Mounted.prefab` |

## Created Directories

- `Assets/_BoneThrone/Prefabs/Environment/`
- `Assets/_BoneThrone/Prefabs/Environment/Floors/`
- `Assets/_BoneThrone/Prefabs/Environment/Walls/`
- `Assets/_BoneThrone/Prefabs/Environment/Architecture/`
- `Assets/_BoneThrone/Prefabs/Environment/Props/`

Unity `.meta` files were created for the new Environment directories and prefab files.

## Skipped Candidates

No requested first-batch source prefab was missing.

Deferred by scope:

- `wall_doorway`
- `stairs`
- `chest`
- `key` / `keyring`
- `bottle`
- `torch_lit` / candle assets with possible light or VFX expectations
- `table` / `chair` / `bench` / `shelf` / `bookcase`
- weapons / tools such as `sword_shield` or `pickaxe`
- characters / enemies / boss
- animations / Animator Controllers

## Collider and Blocking Notes

The created prefabs are project-owned copies of the source visual prefabs. No new colliders, gameplay scripts, interactable scripts, blocking scripts, lights, VFX, particles, or material overrides were added in this phase.

Text scan of the created prefab files did not find Collider components. Later level assembly phases should refine colliders and blocking semantics explicitly:

- Floor prefabs should remain non-blocking by default.
- Wall, column, pillar, crate, barrel, and rubble prefabs may need simple collider refinement later.
- Rocks and torch prefabs can remain visual-only unless a later phase proves otherwise.

## Non-Changes

This phase did not modify:

- KayKit original prefabs, materials, models, or textures
- C# gameplay code
- Scenes
- Materials
- SkillData
- Interactables
- Characters
- Weapons / Equipment
- Animations / Animator Controllers
- LAN / Networking
- Turn, Combat, Skill, or Potion systems
- Formal Level scenes
- `GridTest.unity`

## Unity Play Mode

Unity Play Mode was not run for Phase 15.4 because this phase only created project-owned prefab assets and did not change runtime gameplay, scenes, code, materials, or SkillData.

Recommended Unity 6.3 checks:

1. Open the Project window.
2. Inspect `Assets/_BoneThrone/Prefabs/Environment/`.
3. Open each prefab in Prefab Mode and confirm the visual renders.
4. Confirm root object names use the `Env_` naming scheme.
5. Confirm no gameplay scripts or interactable scripts were added.
6. Confirm no source KayKit prefab is dirty or modified.
7. Do not save any scene changes if prefabs are temporarily dragged into a scene for visual inspection.

## Risks

- The copied prefabs currently rely on source asset references from KayKit; deleting or moving KayKit source assets would break visuals.
- Collider and blocking semantics are intentionally deferred, so later scene assembly must not assume these prefabs already define final tile blocking.
- Some static props may later need to move into Interactable handling if gameplay behavior is added.
- Prefab scale and pivot consistency should be checked visually in Unity before formal level construction.

## Rollback

Rollback this phase by removing the new Environment prefab tree and this DevLog:

```powershell
Remove-Item -LiteralPath "Assets/_BoneThrone/Prefabs/Environment" -Recurse -Force
Remove-Item -LiteralPath "Assets/_BoneThrone/Prefabs/Environment.meta" -Force
Remove-Item -LiteralPath "Docs/DevLogs/Phase15.4_EnvironmentPrefabizationPass.md" -Force
```

Before rollback, run:

```powershell
git status --short
```

Only remove files created by Phase 15.4.
