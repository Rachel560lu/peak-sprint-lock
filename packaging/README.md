# PEAK Sprint Lock

**Hands-free walking and running, with manual steering.**

Press **Forward + Caps Lock** to keep walking, or **Sprint + Forward + Caps Lock** to keep running. Release the keys and keep moving. Press Caps Lock again or move backward to cancel.

## Controls / 操作

| Default keys / 默认按键 | Result / 效果 |
| --- | --- |
| W + Caps Lock | Lock walking / 走路锁定：AUTO WALK |
| Shift + W + Caps Lock | Lock running / 奔跑锁定：AUTO RUN |
| Caps Lock again, or S | Cancel / 取消锁定 |
| Mouse and A/D | Steer and strafe / 转向和横移 |

The mode is captured when activated. Releasing Shift keeps a running lock; pressing Shift during a walking lock does not change its mode. Cancel and activate again to change modes. Movement and held-sprint actions follow your game's key bindings.

模式取决于开启瞬间是否按住冲刺键。松开按键后保持该模式；如需切换，先取消，再用对应组合键重新开启。

## Install with a mod manager / 管理器安装

1. Select **PEAK** in r2modman or a compatible Thunderstore manager and create a profile.
2. Install **BepInExPack_PEAK** by **BepInEx** from the online mod list.
3. For a ZIP shared directly by the author, use **Import local mod** in the profile/settings controls and select the ZIP. Its location varies by manager version. Once published on Thunderstore, install PeakSprintLock from the online list instead.
4. Launch using **Start modded**.

选择 PEAK 并新建配置，先安装 BepInExPack_PEAK，再通过管理器的“Import local mod / 导入本地 Mod”选择收到的 ZIP，最后点击“Start modded / 启动模组游戏”。普通玩家无需安装 .NET SDK、编译代码或使用开发启动脚本。

## Manual install / 手动安装

First install BepInExPack_PEAK following its instructions. With the game closed, copy this package's `plugins/PeakSprintLock` folder into the active installation/profile's `BepInEx/plugins/` folder. The resulting DLL path must be:

```text
BepInEx/plugins/PeakSprintLock/PeakSprintLock.dll
```

首次需先安装 BepInExPack_PEAK。退出游戏，把包内 `plugins/PeakSprintLock` 文件夹复制到所用配置的 `BepInEx/plugins/`。不要把整个发布 ZIP 当作完整的 BepInEx 安装包。

## Behavior / 行为

- Keeps the game's normal movement, stamina consumption and collision handling.
- Jumping keeps the lock; entering climbing, ropes or vines cancels it.
- Backward input, crouch input, pause, blocking menus/wheels, losing focus, incapacitation, death and scene changes cancel the lock.
- Running lock cancels when regular stamina is empty by default. Walking can remain locked with empty stamina.
- Cancelled movement never automatically resumes.
- No automatic obstacle avoidance, cliff braking, pathfinding or teammate following.

保留原版体力消耗、移动和碰撞规则。跳跃保持锁定；攀爬、后退、蹲下、暂停、阻挡输入的菜单/轮盘、切出游戏、昏迷、死亡或切换场景会取消。体力耗尽默认只取消奔跑锁定。取消后需手动重新开启。没有自动避障或悬崖刹车功能。

## Configuration / 设置

Launch once to generate `BepInEx/config/dev.midor.peaksprintlock.cfg`. Exit the game before editing, then relaunch.

| Section / Key | Default | Meaning |
| --- | --- | --- |
| General / Enabled | true | Enable movement lock |
| Controls / ToggleKey | CapsLock | Unity Input System key name; for example F6 |
| Controls / RequireForward | true | Require forward input when activating |
| Display / ShowHud | true | Show the status label |
| Safety / CancelWhenOutOfStamina | true | Cancel running lock at empty regular stamina |
| Diagnostics / Enabled | false | Detailed local diagnostic samples |

Caps Lock may also change your system's capitalization state. Change ToggleKey if it conflicts with another mod. 快捷键冲突时可改为 F6 等按键；诊断默认关闭，无需复制作者的测试配置。

## Compatibility and support / 兼容与反馈

Version **0.1.1** was built and locally gameplay-tested on **Windows, PEAK 2.4.b (3e62ee214), BepInEx runtime 5.4.23.3**. Walking and running lock were confirmed by the tester. Host/client multiplayer, controller activation, and compatibility with other movement/input mods have not been independently validated. Game updates may require an updated mod.

已在上述版本通过本机走路/奔跑锁定实机测试。尚未独立验证联机房主/客户端、手柄开启和其他移动 Mod 兼容性。

If it does not load, check the active profile, dependency installation and modded launch. Avoid duplicate copies of PeakSprintLock. Send the author your game/mod versions, reproduction steps and the relevant `BepInEx/LogOutput.log` error. Local session logs are under `BepInEx/SprintLockDiagnostics/`; detailed movement samples require Diagnostics to be enabled.

## Update or uninstall / 更新或卸载

Close the game first. Update through the manager or replace the single plugin DLL. Uninstall through the manager, or remove `BepInEx/plugins/PeakSprintLock`. Your config can be retained for a later reinstall.
