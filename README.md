# 酒狐扩展（STS2_WineFox_Addons）

《杀戮尖塔 2》模组「酒狐」的扩展 MOD，基于 RitsuLib 框架编写。

当前版本：**0.1.0**

## 内容一览

### 卡牌

- **自给自足** —— 无色 · 稀有 · 能力牌，1 费（升级后 0 费）。打出后每回合开始时随机获得 1 个普通食物，可叠加。

### 遗物

- **美食家围裙** —— 酒狐专属 · 罕见。每回合开始时获得 1 层再生。

### 事件

- **深夜食堂** —— 三选一：饱餐一顿（按最大生命值百分比回复）、带一份走（获得 1 个随机食物）、离开。

### 食物（7 种，进入酒狐的食物池）

卷心菜、洋葱、稻米、番茄、炒饭、玉米、烤玉米。可以卖给商人换金币，其中稻米与玉米抵达篝火时分别变成炒饭与烤玉米。

### 菜肴（4 道，击败 Boss 必定掉落）

填馅南瓜、牧羊人派、蜜汁火腿、烤鸡。均为稀有，都能直接卖给商人换取金币，并且都会附带 1 层「滋养」。

### 其他

- 关键词 **滋养**：回合结束时按层数回复生命，受到未被格挡的伤害时免疫该伤害并减少 1 层。
- 设置项 **启用全角色食物掉落**：默认关闭；关闭时遵循酒狐本体的规则，队伍里必须有酒狐才会掉落食物。
- 界面：食物在药水弹窗里的第一个选项显示「食用」，而不是默认的「饮用」（非食物药水不受影响）。

## 依赖

| 依赖 | 版本要求 |
| --- | --- |
| 杀戮尖塔 2 | 0.107.1 |
| RitsuLib | ≥ 0.5.0 |
| 酒狐（STS2_WineFox） | ≥ 1.2.15 |

注意：扩展的 `mod_manifest.json` 里**没有**写酒狐的 `min_version`。酒狐使用 `1.2.15.1` 这类四段版本号，而游戏内置的版本解析只认三段，写进清单会导致解析失败、扩展被拒绝加载；因此版本检查改在运行时完成，且只输出日志、不阻止加载。

## 安装

1. 先安装 RitsuLib 与酒狐；
2. 把 `STS2_WineFox_Addons.dll`、`STS2_WineFox_Addons.json`、`STS2_WineFox_Addons.pck` 放进 `<游戏目录>/mods/STS2_WineFox_Addons/`。

## 从源码构建

需要 .NET 9 SDK 与 Godot 4.5.1（.NET 版）。先改 `STS2_WineFox_Addons.csproj` 里的本机路径：

```xml
<Sts2Dir>你的杀戮尖塔 2 目录</Sts2Dir>
<GodotExe>你的 Godot 可执行文件</GodotExe>
```

然后：

```shell
dotnet publish -c Release
```

构建会自动把 DLL 与清单复制到游戏的 `mods/` 目录，并用 Godot 的无界面模式导出 PCK。**部署前请先关闭游戏**，否则 DLL 被占用会构建失败。

## 目录结构

| 目录 | 内容 |
| --- | --- |
| `Cards/` | 卡牌 |
| `Potions/Food/`、`Potions/Dishes/` | 食物与菜肴 |
| `Powers/` | 能力 |
| `Relics/`、`Events/` | 遗物与事件 |
| `Keywords/` | 卡牌关键词注册（滋养） |
| `Settings/` | RitsuLib 模组设置，含联机同步 |
| `Patches/` | 补丁：食物掉落、Boss 菜肴掉落、联机同步、界面文案 |
| `Utils/` | 对酒狐内部接口的兼容封装 |
| `STS2_WineFox_Addons/` | Godot 资源：图标与本地化 |

## 说明

- 本仓库不包含《杀戮尖塔 2》的游戏本体资源；本地反编译产物目录 `Decompiled/` 已在 `.gitignore` 中排除。
- `STS2_WineFox_Addons/powers/next_turn_strength_power.png` 与 `next_turn_temporary_strength_power.png` 目前是在游戏本体的力量图标上叠加箭头改造而成的占位素材，正式发布前建议替换为自制图标。
- 本地化目前只有简体中文（`STS2_WineFox_Addons/localization/zhs/`）。

## 许可证

本项目采用 [MIT 许可证](LICENSE)。
