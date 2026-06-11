# Phase 16.13 - Outcome Popup Prefab And Death Audio

## Scope

- Converted the battle result popup controller to support prefab image replacement through Inspector fields.
- Changed popup buttons to return to the start scene or quit the game.
- Moved the two newly added death SFX into the existing `_BoneThrone/Audio` runtime audio tree and wired them into combat death handling.

## Implementation

- `GameResultPanelController`
  - Added draggable Sprite/Image fields for the popup panel, frame, return button, and quit button.
  - Preserved old prefab button references through `FormerlySerializedAs`.
  - Changed the old retry action into `StartMenu` scene loading.
  - Changed the close action into editor stop-play or player quit.
  - Refreshes button labels to `返回开始` and `退出游戏`.

- Outcome popup prefabs
  - Updated visible button text on victory/defeat/result canvas prefabs.
  - Runtime image fields can be assigned on the prefab without hard-coded art dependencies.

- Audio
  - Moved `怪物死亡.wav` to `Assets/_BoneThrone/Audio/Resources/BoneThroneAudio/SFX/Death/BTSFX_Monster_Death.wav`.
  - Moved `角色死亡.wav` to `Assets/_BoneThrone/Audio/Resources/BoneThroneAudio/SFX/Death/BTSFX_Player_Death.wav`.
  - Added `PlayerDeath` and `MonsterDeath` cue IDs.
  - Player deaths now play the player death SFX.
  - Enemy deaths now play the monster death SFX louder.
  - Boss-like deaths reuse the monster death SFX at a slower pitch and louder volume for a heavier feel.

## Inspector Setup

1. Open the victory/defeat popup prefab or the result canvas prefab.
2. Select the object with `GameResultPanelController`.
3. Optionally drag custom sprites into `Panel Background Sprite`, `Frame Sprite`, `Return Button Sprite`, and `Quit Button Sprite`.
4. If a specific Image target is needed, assign the matching Image fields; otherwise the controller auto-resolves the common panel/button images at runtime.

## Validation

- Ran `dotnet build Assembly-CSharp.csproj`: 0 errors, 0 warnings.
- Ran targeted `git diff --check` on the touched scripts and popup prefabs. Only Git's existing LF-to-CRLF notice appeared for `UI_GameResultCanvas.prefab`.
- Confirmed `StartMenu` is listed in `ProjectSettings/EditorBuildSettings.asset`.
- Confirmed no `*死亡.wav*` files remain at the `_BoneThrone` root.

## Unity Verification

1. Open `boss_test` and enter Play Mode.
2. Trigger demo defeat. The defeat popup should appear after the existing delay.
3. Click `返回开始`; Unity should load `StartMenu`.
4. Re-enter a battle, trigger defeat or victory, and click `退出游戏`; in Editor, Play Mode should stop.
5. Kill a normal monster and confirm the monster death SFX is louder.
6. Kill the Boss and confirm the death SFX is louder and lower/slower.
7. Let a player unit die and confirm the player death SFX plays.

## Risk And Rollback

- Risk: popup image targets are optional runtime-resolved fields, so custom art assignment should be checked once in prefab Inspector after Unity reimports scripts.
- Risk: death SFX feel may still need volume/pitch tuning after listening in Play Mode.
- Rollback: revert `GameResultPanelController`, the three outcome popup prefabs, `BTAudioCueId`, `BTAudioService`, `Unit`, `DamageResolver`, the new `SFX/Death` audio folder, and this DevLog.
