# Prototype verification — 2026-09-08

## 0.1.1 distribution package

- User subsequently confirmed both walking and running lock passed gameplay testing.
- Session PID 14548 records ON Auto walk with sprint=False after physical keys are released, ON Auto run with sprint=True after release, toggle-off and pause cancellation. Character root transform in these samples is constant, so position logs alone do not prove locomotion; movement confirmation is user-observed.
- Packaged the exact tested DLL (SHA-256 unchanged). No code changes for distribution.
- `dist/PeakSprintLock-0.1.1.zip` contains exactly manifest.json, bilingual player README.md, CHANGELOG.md, original 256x256 icon.png, and plugins/PeakSprintLock/PeakSprintLock.dll.
- Verified manifest/DLL version agreement, icon dimensions, complete ZIP allowlist and each ZIP entry's SHA-256 against the source file.
- Declared BepInEx-BepInExPack_PEAK-5.4.75301: its official package page identifies runtime 5.4.23.3, matching local testing.
- The ZIP has not been imported into a fresh mod-manager profile or uploaded to Thunderstore. Installation/package validation is structural, not a claim of a completed manager install test.

## 0.1.1 walking/running mode update

- User confirmed 0.1.0 running lock worked during actual gameplay.
- 0.1.1 captures the held sprint action when activating: forward alone locks walking; forward plus sprint locks running. Releasing keys preserves the selected mode.
- Build: zero warnings/errors; 58 policy/input-merge assertions pass, including separate modes, mode reset, key release and cancellation for running.
- Deployed DLL SHA-256: 67B233477EDC4B5DD67C5C94694BC3B2E88C8B6F490480230C7C7CDB334A9C53.
- Started isolated PID 14548 at 23:39 +08. BepInEx reports exactly one plugin, version 0.1.1. Matching session records START and FIRST_FRAME without plugin FAULT. Plugin MVID: 5d27345c-eff5-42bb-bbad-f7c1fdc2e524.
- User gameplay validation of walking versus running remains pending. In particular, check AUTO WALK remains walking with the game's sprint-toggle previously enabled, and empty-stamina walking remains available.

The sections below retain the original 0.1.0 verification history.

## Verified

- Local target: PEAK 2.4.b, build 3e62ee214.
- Release DLL builds against installed game and BepInEx assemblies: zero warnings, zero errors.
- 37 standalone policy/input-merge assertions pass (`scripts/build.ps1`).
- Inspected installed assembly IL: `CharacterMovement.Update` calls `CharacterInput.Sample(CanDoInput())`, then `SetMovementState`, then `CalculateWorldMovementDir`.
- `Sample` starts with `ResetInput`, reads rebound movement, clamps magnitude to 1 and reads sprint. Its Boolean parameter gates movement input.
- `CanDoInput` checks blocking GUI windows and active wheels. `isClimbingAnything` includes climbing, ropes and vines. `OutOfRegularStamina` uses the game's stamina rule.
- `SetMovementState` retains native sprint eligibility, afflictions and stamina consumption.
- Separate test profile contains the copied existing BepInEx core plus only this plugin. Existing item-insight profile and game junction are unchanged.

## Runtime startup verified — 2026-09-08 23:33 +08

- Isolated launcher started PID 47096. BepInEx reports exactly `1 plugin to load`: PEAK Sprint Lock 0.1.0.
- This PID's session trace records START, scene Pretitle and FIRST_FRAME. Private-method delegate creation and Harmony installation completed without plugin FAULT.
- Game MVID: c88cc526-8d4e-40fa-afdb-e24e4c233bd0. Plugin MVID: 352a6618-6924-410f-bd5c-d6f834eea3aa.
- BepInEx reported `Unable to start Unity log writer`; its file logger and the plugin's separate diagnostic file both work. This does not establish full Unity log capture.
- Old PID 48656 is a retained process record with zero threads and handles. Launcher now ignores that terminated record instead of blocking startup.

## Not yet verified

- Actual local-character input sampling and real gameplay.
- Real keyboard toggling, physical movement, HUD rendering, cancellation timing, speed/stamina equivalence.
- Host/client multiplayer and coexistence with other input mods.

At preparation time PEAK PID 48656 was running. The user was asked to exit normally before a test launch. No running game was terminated and no runtime result is claimed.

The offline tests exercise policy values, not the Unity state readers. Passing a case named `Climbing` does not prove game-world climbing detection works; that requires the in-game checklist in README.

## Evidence procedure

Run `scripts/start-test.ps1` after normal exit, then `scripts/read-test.ps1`. Require the current PID's START, FIRST_FRAME and FIRST_SAMPLE; old logs do not establish a successful launch. During active lock, Diagnostics records raw and merged inputs, position and stamina once per second. Confirm actual movement with the player as well as the logs.
