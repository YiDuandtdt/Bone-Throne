# Phase 16.20 - Android Build Toolchain Fix

Date: 2026-06-10

## Goal

Fix the Android build failure caused by a missing JDK while keeping Windows/PC builds unaffected.

## Changes

- Installed Android SDK components into the user-local SDK folder:
  - SDK root: `C:\Users\Admin\AppData\Local\Android\Sdk`
  - Android platform: `android-36`
  - Build Tools: `36.0.0`
  - NDK: `27.2.12479018`
  - CMake: `3.22.1`
- Reused an existing local JDK 17:
  - JDK root: `D:\Program Files\Processing\app\resources\jdk`
- Added `AndroidExternalToolsAutoConfigurator`.
  - Runs only when the active Unity build target is Android.
  - Sets Unity Android External Tools paths only if the current paths are invalid.
  - Verifies required SDK, Build Tools, NDK, and CMake components before configuring paths.
  - Does not run for Windows/Standalone targets.
- Replaced the temporary Android input-handling build guard with a legacy cleanup-only script.
  - The guard switched `activeInputHandler` from `Both` to `Input Manager` during Android build preprocessing.
  - That caused the Editor and Android Player to compile different `UnityEngine.Rendering.DebugActionDesc` serialized layouts.
  - The replacement does not implement build preprocess/postprocess callbacks and does not modify `ProjectSettings`.
  - Android builds now keep the project setting at `Both` so URP/Core RP serialization stays consistent.

## Validation

- `dotnet build .\Assembly-CSharp-Editor.csproj --no-restore`
  - Result: passed, 0 errors.
- `dotnet build .\Assembly-CSharp.csproj --no-restore`
  - Result: passed, 0 errors.
- `sdkmanager --list_installed`
  - Confirmed `cmake;3.22.1` is installed.

## Unity Setup

After Unity recompiles scripts, Android External Tools should be configured automatically while Android is the active build target.

If needed, run:

`BoneThrone > Android > Configure External Tools`

Expected External Tools paths:

- JDK: `D:\Program Files\Processing\app\resources\jdk`
- SDK: `C:\Users\Admin\AppData\Local\Android\Sdk`
- NDK: `C:\Users\Admin\AppData\Local\Android\Sdk\ndk\27.2.12479018`

Expected CMake path used by Unity Android builds:

- CMake: `C:\Users\Admin\AppData\Local\Android\Sdk\cmake\3.22.1`

## Notes

The URP additional punctual light shadow messages are warnings, not the JDK build failure. This change intentionally does not modify scene lights, `PC_RPAsset`, or Windows build settings.

The `DebugActionDesc` class-layout build failure was caused by changing Active Input Handling during Android build preprocessing. Do not switch the project from `Both` to `Input Manager` inside a build callback.

## Rollback

- Delete `Assets/_BoneThrone/Editor/AndroidExternalToolsAutoConfigurator.cs`.
- In Unity Preferences, clear or reset the Android External Tools paths if custom Android tools are no longer wanted.
- Optional: remove `C:\Users\Admin\AppData\Local\Android\Sdk\cmake\3.22.1` if the local SDK should be returned to its pre-Android-build state.
