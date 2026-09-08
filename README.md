# PEAK Sprint Lock — 0.1.1

<p align="center">
  <img src="docs/images/mountain-run.png" width="320" alt="Abstract lime-green runner with speed lines in front of snow-capped mountains / 奔跑人物与雪山的抽象插图" />
</p>

[English](#english) | [简体中文](#简体中文)

## English

A walking and running lock mod built for **PEAK 2.4.b / 3e62ee214**. Both modes have passed local gameplay testing. Multiplayer and compatibility with other movement/input mods have not yet been verified.

![Walking and running lock controls](docs/images/movement-lock-guide.svg)

*Control illustration using the default key bindings; not a gameplay screenshot.*

### Download and distribution

Available on [Thunderstore / Rachel560lu / PeakSprintLock](https://thunderstore.io/c/peak/p/Rachel560lu/PeakSprintLock/). Source is stored in Rachel560lu's private GitHub repository; no open-source license is currently included.

`dist/PeakSprintLock-0.1.1.zip` is the standard Thunderstore-format package. Build outputs are excluded from this repository. Player-facing instructions are in [packaging/README.md](packaging/README.md). After building, run `scripts/package.ps1` to create the ZIP and verify the manifest version, icon dimensions and SHA-256 of every packaged file. The published DLL is the same version used in the successful 0.1.1 gameplay test.

### Controls

- Hold your game's forward key and press **Caps Lock** to lock walking. The HUD displays **AUTO WALK**.
- Hold your game's sprint key (Shift by default) and forward key, then press **Caps Lock** to lock running. The HUD displays **AUTO RUN**.
- The mode is captured at activation and persists after releasing the keys. Pressing or releasing Shift while locked does not switch modes; cancel and activate again to change modes.
- Press Caps Lock again or your game's backward key to cancel. Movement actions follow in-game key bindings.
- Use the mouse to steer and A/D to strafe. Jumping preserves the lock.
- Climbing (including ropes and vines), incapacitation, death, blocking menus/wheels, pause, loss of focus and scene changes cancel the lock. Empty stamina cancels running lock only; walking lock remains available.
- Crouch input also cancels. A cancelled lock never resumes automatically.
- No obstacle avoidance, cliff braking or automatic navigation is provided. Caps Lock may also change your system's capitalization state; the toggle key is configurable.

### Installation and configuration

In a compatible mod manager, select PEAK, install **PeakSprintLock** from Thunderstore with its **BepInExPack_PEAK** dependency, and launch using **Start modded**.

For manual installation into an existing BepInEx 5 PEAK profile, close the game and place `PeakSprintLock.dll` in `BepInEx/plugins/PeakSprintLock/`. Launch using that modded profile. Do not overwrite game assemblies. To uninstall, close the game and remove the plugin through your manager or delete its plugin folder.

The first launch generates `BepInEx/config/dev.midor.peaksprintlock.cfg`. Settings include `ToggleKey` (a Unity Input System Key name), `RequireForward`, `ShowHud` and `CancelWhenOutOfStamina`. Activation currently uses the keyboard; controller and other movement-mod compatibility are not guaranteed.

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

Automated test success does not establish completion of every gameplay check above. See [docs/verification.md](docs/verification.md) for recorded results.

## 简体中文

面向本机 PEAK **2.4.b / 3e62ee214** 编译的走路/奔跑锁定 Mod。用户已确认两种模式通过实机测试；联机及其他输入 Mod 兼容性尚未验证。

![走路锁定与奔跑锁定操作示意图](docs/images/movement-lock-guide.svg)

*默认按键的操作示意图，非游戏实机截图：W＋Caps Lock 锁定走路，Shift＋W＋Caps Lock 锁定奔跑。*

### 分发

`dist/PeakSprintLock-0.1.1.zip` 是标准 Thunderstore 格式安装包，可分享给朋友或上传到 Thunderstore。玩家说明在 `packaging/README.md`。运行 `scripts/package.ps1` 可重新打包并验证清单版本、图标尺寸和 ZIP 每个文件的 SHA-256。包中 DLL 与通过实机测试的 0.1.1 一致。

已发布到 [Thunderstore / Rachel560lu / PeakSprintLock](https://thunderstore.io/c/peak/p/Rachel560lu/PeakSprintLock/)。源码保存在 Rachel560lu 的私有 GitHub 仓库；当前不包含开源许可证。

### 操作

- 按住游戏的前进键，再按 **Caps Lock**：锁定走路，HUD 显示 **AUTO WALK**。
- 按住游戏的冲刺键（默认 Shift）＋前进键，再按 **Caps Lock**：锁定奔跑，HUD 显示 **AUTO RUN**。
- 模式取决于开启瞬间是否按住冲刺操作，松开按键后保持。锁定期间按下或松开 Shift 不切换模式；切换模式请先取消再重新开启。
- 再按 Caps Lock 或游戏的后退键取消。支持游戏内移动键重绑定。
- 鼠标控制转向；A/D 横移；跳跃保持锁定。
- 攀爬（含绳索和藤蔓）、昏迷、死亡、打开阻挡输入的菜单/轮盘、暂停、失去焦点、场景切换时取消。体力耗尽只取消奔跑锁定，走路锁定仍可使用。
- 蹲下输入也取消。取消后不会自动恢复锁定。
- 不提供避障、悬崖刹车或自动导航。Caps Lock 仍可能改变系统大小写状态，可在配置中改键。

### 安装原型

需要已正常工作的 BepInEx 5 PEAK 配置。在游戏退出后，将 `PeakSprintLock.dll` 放入该配置的 `BepInEx/plugins/PeakSprintLock/`，然后通过对应 Mod 配置启动游戏。

不要覆盖游戏程序集。卸载时退出游戏，再移除该插件目录。

首次加载生成 `BepInEx/config/dev.midor.peaksprintlock.cfg`。支持 `ToggleKey`（Unity Input System Key 名称）、`RequireForward`、`ShowHud`、`CancelWhenOutOfStamina`。原型为键盘开启；尚未承诺手柄或其他移动 Mod 兼容。

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

自动化测试通过不代表上述实机步骤通过。完整结果见 `docs/verification.md`。
