# Development and verification / 开发与验证

[English homepage](../README.md) · [中文主页](../README.zh-CN.md)

Commands below run from the repository root. / 以下命令在仓库根目录执行。

## English

### Building and testing

`scripts/build.ps1` uses .NET 8 SDK and local PEAK/BepInEx assemblies. Its default paths point to the original development machine; override `Dotnet`, `PeakManagedDir` and `BepInExCoreDir` for your installation. Game assemblies and SDK files are not included in the repository.

Run the standalone state-policy tests with:

```powershell
dotnet run --project tests/Tests.csproj -c Release
```

The plugin merges `movementInput` and `sprintIsPressed` in a Postfix on `CharacterInput.Sample(bool)`. The game's `SetMovementState` and `CalculateWorldMovementDir` continue handling sprint eligibility, stamina and physics. Patches only affect the local player's character. Private game methods are accessed through Harmony delegates cached at startup.

`RuntimeDriver` lives on a separate persistent GameObject, so frame callbacks do not depend on the BepInEx plugin host. Input resets, scene transitions and focus events clear the lock.

### In-game verification

Use a fresh BepInEx profile containing only this mod and test on clear ground in the offline airport before continuing a saved expedition.

1. Check the current process's diagnostic file for `START`, `FIRST_FRAME`, and `FIRST_SAMPLE` after entering a character.
2. Press W + Caps Lock and release: expect AUTO WALK and continued walking; backward input stops it. Then press Shift + W + Caps Lock and release: expect AUTO RUN and continued running.
3. Check repeated toggling, strafing, mouse steering, jumping, pause and switching away from the game. Returning after cancellation must leave the lock OFF.
4. Test climbing, empty-stamina cancellation for running, and scene transitions in a safe location.
5. Compare speed and stamina with manual running over the same route, then separately test host/client multiplayer.

With `[Diagnostics] Enabled = true`, local logs are written to `BepInEx/SprintLockDiagnostics/session-<PID>-<UTC>.log`. They include version, assembly MVIDs, state transitions and one sample per second of actual/merged input, position and stamina while locked. Logs are not uploaded.

Automated test success does not establish completion of every gameplay check above. See [verification.md](verification.md) for recorded results.

### Packaging

Run `scripts/package.ps1` after building to create `dist/PeakSprintLock-0.1.1.zip`. The script verifies manifest version, icon dimensions and SHA-256 of packaged files. Build outputs are excluded from Git. The published DLL matches the successful 0.1.1 gameplay test. See [release record](release-0.1.1.md).

## 简体中文

### 构建与测试

`scripts/build.ps1` 使用本机已有 .NET 8 SDK 和游戏/BepInEx 程序集；路径均可通过参数覆盖。不要求安装新的 SDK，不下载依赖包。

状态逻辑测试：`dotnet run --project tests/Tests.csproj -c Release`。

技术实现：在 `CharacterInput.Sample(bool)` 的 Postfix 合并 `movementInput` 和 `sprintIsPressed`；原版调用 `SetMovementState` 及 `CalculateWorldMovementDir`，继续处理冲刺、体力和物理。补丁只处理当前本地角色。游戏私有方法通过启动时缓存的 Harmony 委托访问。

`RuntimeDriver` 位于独立持久 GameObject，避免依赖插件宿主的帧回调。输入 reset、场景和焦点事件清理锁定。

### 实机验证

建议使用全新、只有该原型的 BepInEx 测试配置，在离线机场空旷地面测试，不要直接继续已有登山存档。

1. 检查当前游戏进程对应诊断文件出现 `START`、`FIRST_FRAME` 和进入角色后的 `FIRST_SAMPLE`。
2. 按 W＋Caps Lock 后松开：HUD 为 AUTO WALK，角色持续走路；按后退停止。再按 Shift＋W＋Caps Lock 后松开：HUD 为 AUTO RUN，角色持续奔跑。
3. 重复开启，测试再次切换、A/D、鼠标、跳跃、暂停和切出游戏；恢复后必须保持 OFF。
4. 在安全场地测试攀爬和体力耗尽取消，以及重新进入场景。
5. 与相同路线手动奔跑比较速度和体力；最后再测试房主与客户端。

设置 `[Diagnostics] Enabled = true` 后，诊断日志位于 `BepInEx/SprintLockDiagnostics/session-<PID>-<UTC>.log`。日志包含版本、程序集 MVID、状态转换，以及锁定期间每秒一次的真实输入/输出、位置和体力。日志只保存本地，不上传。

自动化测试通过不代表上述实机步骤通过。完整结果见 [verification.md](verification.md)。

### 打包

构建后运行 `scripts/package.ps1`，生成 `dist/PeakSprintLock-0.1.1.zip`，并验证版本、图标尺寸及包内文件的 SHA-256。构建产物不提交到 Git。已发布 DLL 与通过实机测试的 0.1.1 一致。见[发布记录](release-0.1.1.md)。

