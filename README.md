# PEAK Sprint Lock — 0.1.1

面向本机 PEAK **2.4.b / 3e62ee214** 编译的走路/奔跑锁定 Mod。用户已确认两种模式通过实机测试；联机及其他输入 Mod 兼容性尚未验证。

## 分发

`dist/PeakSprintLock-0.1.1.zip` 是标准 Thunderstore 格式安装包，可分享给朋友或上传到 Thunderstore。玩家说明在 `packaging/README.md`。运行 `scripts/package.ps1` 可重新打包并验证清单版本、图标尺寸和 ZIP 每个文件的 SHA-256。包中 DLL 与通过实机测试的 0.1.1 一致。

已发布到 [Thunderstore / Rachel560lu / PeakSprintLock](https://thunderstore.io/c/peak/p/Rachel560lu/PeakSprintLock/)。源码保存在 Rachel560lu 的私有 GitHub 仓库；当前不包含开源许可证。

## 操作

- 按住游戏的前进键，再按 **Caps Lock**：锁定走路，HUD 显示 **AUTO WALK**。
- 按住游戏的冲刺键（默认 Shift）＋前进键，再按 **Caps Lock**：锁定奔跑，HUD 显示 **AUTO RUN**。
- 模式取决于开启瞬间是否按住冲刺操作，松开按键后保持。锁定期间按下或松开 Shift 不切换模式；切换模式请先取消再重新开启。
- 再按 Caps Lock 或游戏的后退键取消。支持游戏内移动键重绑定。
- 鼠标控制转向；A/D 横移；跳跃保持锁定。
- 攀爬（含绳索和藤蔓）、昏迷、死亡、打开阻挡输入的菜单/轮盘、暂停、失去焦点、场景切换时取消。体力耗尽只取消奔跑锁定，走路锁定仍可使用。
- 蹲下输入也取消。取消后不会自动恢复锁定。
- 不提供避障、悬崖刹车或自动导航。Caps Lock 仍可能改变系统大小写状态，可在配置中改键。

## 安装原型

需要已正常工作的 BepInEx 5 PEAK 配置。在游戏退出后，将 `PeakSprintLock.dll` 放入该配置的 `BepInEx/plugins/PeakSprintLock/`，然后通过对应 Mod 配置启动游戏。

不要覆盖游戏程序集。卸载时退出游戏，再移除该插件目录。

首次加载生成 `BepInEx/config/dev.midor.peaksprintlock.cfg`。支持 `ToggleKey`（Unity Input System Key 名称）、`RequireForward`、`ShowHud`、`CancelWhenOutOfStamina`。原型为键盘开启；尚未承诺手柄或其他移动 Mod 兼容。

## 构建与测试

`scripts/build.ps1` 使用本机已有 .NET 8 SDK 和游戏/BepInEx 程序集；路径均可通过参数覆盖。不要求安装新的 SDK，不下载依赖包。

状态逻辑测试：`dotnet run --project tests/Tests.csproj -c Release`。

技术实现：在 `CharacterInput.Sample(bool)` 的 Postfix 合并 `movementInput` 和 `sprintIsPressed`；原版调用 `SetMovementState` 及 `CalculateWorldMovementDir`，继续处理冲刺、体力和物理。补丁只处理当前本地角色。游戏私有方法通过启动时缓存的 Harmony 委托访问。

`RuntimeDriver` 位于独立持久 GameObject，避免依赖插件宿主的帧回调。输入 reset、场景和焦点事件清理锁定。

## 实机验证

建议使用全新、只有该原型的 BepInEx 测试配置，在离线机场空旷地面测试，不要直接继续已有登山存档。

1. 检查当前游戏进程对应诊断文件出现 `START`、`FIRST_FRAME` 和进入角色后的 `FIRST_SAMPLE`。
2. 按 W＋Caps Lock 后松开：HUD 为 AUTO WALK，角色持续走路；按后退停止。再按 Shift＋W＋Caps Lock 后松开：HUD 为 AUTO RUN，角色持续奔跑。
3. 重复开启，测试再次切换、A/D、鼠标、跳跃、暂停和切出游戏；恢复后必须保持 OFF。
4. 在安全场地测试攀爬和体力耗尽取消，以及重新进入场景。
5. 与相同路线手动奔跑比较速度和体力；最后再测试房主与客户端。

设置 `[Diagnostics] Enabled = true` 后，诊断日志位于 `BepInEx/SprintLockDiagnostics/session-<PID>-<UTC>.log`。日志包含版本、程序集 MVID、状态转换，以及锁定期间每秒一次的真实输入/输出、位置和体力。日志只保存本地，不上传。

自动化测试通过不代表上述实机步骤通过。完整结果见 `docs/verification.md`。
