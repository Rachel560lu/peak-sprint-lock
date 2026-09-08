# Teammate trail prototype — 0.2.0 (unreleased)

[English homepage](../README.md) · [中文主页](../README.zh-CN.md)

The source build now includes a display-only teammate trail. Thunderstore remains at 0.1.1; that download does not contain this feature. No game was launched for this prototype's verification.

## Controls / 操作

- Look toward a teammate and press **F6** to begin recording their subsequent positions. Press F6 again to clear the target and trail. Stop before choosing a different teammate.
- 看向队友后按 **F6**，开始记录其之后经过的位置；再按 F6 清除。切换目标需要先清除，再看向新目标开启。
- Selects the teammate closest to the camera's forward direction within approximately 20 degrees and 60 world units. Selection does not currently check line of sight; a teammate behind terrain can be selected. The HUD identifies the selected name and distance.
- 选择约 20 度视角、60 游戏距离单位内最接近准星方向的队友。目前不检查地形遮挡，因此可能选中墙后的队友；HUD 显示目标名字和距离。
- `[Trail] Enabled` enables the feature; `[Trail] ToggleKey` changes F6. Choose a key that does not conflict with other mods or the movement lock. The existing `ShowHud` option also controls the trail label.

## Behavior / 行为

History retains up to 45 seconds and 600 points. Normal recording is capped near 10 Hz, suppresses movement under 0.2 world units, and adds a stationary heartbeat every 0.5 seconds. Height is preserved for climbing/jumping. Green lines fade with age; line opacity interpolates between segment endpoints. The path is the observed character-root trajectory with a small vertical drawing offset, not ground-projected footsteps or a validated safe route.

轨迹保留最近 45 秒，最多 600 点。普通采样约每秒 10 次，小于 0.2 单位的位移会被过滤，停留时每 0.5 秒补点。保留攀爬和跳跃高度，旧线逐渐淡出。线条来自角色根节点的位置，并略微上移，不代表脚踩位置或已经验证的安全路线。

An observed jump over 8 world units, sampling gap over 1 second, or invalid coordinate starts a separate segment. The renderer draws at most 32 recent non-singleton segments. These thresholds are heuristics and need multiplayer tuning; smoothly interpolated teleports or repeated stale network positions may not be detected.

相邻观测位置跳变超过 8 单位、采样中断超过 1 秒或坐标无效时断线，最多绘制最近 32 个有效线段。这些阈值需要联机调整；平滑插值的瞬移、网络位置持续不更新等情况不一定能识别。

Target loss, target incapacitation/death, local character replacement, scene changes, blocked local input, pause, focus loss or disabling the feature clear the trail. No automatic resumption. Climbing itself is not a trail cancellation rule, but native input blocking still takes precedence. The module never injects movement, rotates the camera, sends network messages, or saves routes. It observes the remote position already available locally and cannot recover positions from before activation.

目标离开、失去行动能力或死亡、本地角色更换、场景变化、输入受阻、暂停、失焦或禁用功能时清除，且不自动恢复。攀爬本身不触发清除，但游戏输入阻断仍优先。此模块不控制移动、镜头，不发送网络消息或保存路线，也不能恢复开启前的轨迹。

## Verification / 验证

Release compilation against the installed PEAK assemblies and engine-independent policy tests are the current validation scope. Tests cover sampling, height preservation, jitter, discontinuities, expiry, capacity, renderer segmentation, opacity, target geometry and the existing walk/run lock policies.

尚未实机验证：队友选择、游戏原生输入阻断与攀爬的关系、材质/遮挡/线条高度、远端位置同步、场景生命周期和与其他 Mod 共存。代码测试通过不代表这些项目已通过。

Build with `scripts/build.ps1`. The development DLL is `bin/Release/netstandard2.1/PeakSprintLock.dll`, version 0.2.0. Published packaging metadata remains 0.1.1; do not package the prototype as the validated 0.1.1 release.

Validation result: Release build succeeded with 0 warnings and 0 errors; 86 checks passed (58 existing movement-lock checks and 28 trail checks). No in-game testing performed. / 验证结果：Release 编译零警告、零错误；86 项检查通过（原有移动锁定 58 项、轨迹 28 项），未启动游戏。
