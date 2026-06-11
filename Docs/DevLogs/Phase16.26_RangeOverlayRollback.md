# Phase 16.26 - Range Overlay Rollback

Date: 2026-06-10

## Problem

The raised runtime range overlay added during final polish created floating green quads that were smaller and brighter than the original tile highlights. It made the movement range look worse in areas where the original tile path was already readable.

## Change

- Removed the runtime overlay objects from `MovementDebugHighlighter`.
- Restored range display to the original tile-coloring behavior.
- Kept the enemy attack audio timing changes from the previous pass.

## Verification

- `dotnet build .\Assembly-CSharp.csproj --no-restore` passed with 0 errors.
- Searched `MovementDebugHighlighter.cs` for `RangeOverlay`, `RuntimeRangeOverlays`, and `rangeOverlay`; no matches remain.

## Rollback

No rollback is recommended for the overlay approach. If a future high-platform readability fix is needed, prefer adjusting the existing tile material/color path rather than adding floating runtime quads.
