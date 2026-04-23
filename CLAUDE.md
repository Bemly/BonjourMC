# CLAUDE.md — BonjourMC 项目指南

## 项目概述

BonjourMC 是一个 Minecraft Java Edition 启动器，使用 VB.NET (.NET 8.0) 编写，Avalonia UI 11.3.14 作为 GUI 框架。目标平台为 macOS，支持跨平台。

## 构建和运行

```bash
dotnet restore          # 还原依赖
dotnet build            # 构建全部项目
dotnet run --project GUI    # 运行 GUI
dotnet run --project TUI    # 运行 TUI
dotnet test             # 运行测试
```

## 架构

4 个项目通过 `BonjourMC.sln` 关联：

- **Launcher** — 核心类库，所有业务逻辑（下载、启动、工具类）
- **GUI** — Avalonia 桌面应用，引用 Launcher
- **TUI** — 命令行入口，引用 Launcher
- **Test** — NUnit 测试，引用 GUI + Launcher + TUI

### 设计模式

- **MVVM** — GUI 使用 ReactiveUI，ViewLocator 自动解析 ViewModel→View
- **Bridge/Adapter** — `Utility/Bridge/` 下的适配器类（Download、Json、Crypto），通过 `Config.api.*` 切换实现
- **Singleton** — 适配器使用 CLR 初始化的线程安全单例
- **Producer-Consumer** — 下载引擎使用 `ConcurrentQueue` + 128 个 Task 并发下载
- **Fluent Builder** — `Setup` 和 `Minecraft` 类使用方法链式调用

### 关键路径

- `Launcher/Core.vb` — `Config` 类（URL、路径、线程数、重试次数）和 `Entry.Point()` 入口
- `Launcher/Java/Client/Setup.vb` — 游戏启动器，参数化 fluent API + 事件
- `Launcher/Java/Client/Download/Mojang/Minecraft.vb` — 下载引擎，实现 `Progress` 接口
- `GUI/Services/LauncherService.vb` — GUI↔Launcher 门面，所有事件通过 `Dispatcher.UIThread.Post()` 调度
- `GUI/ViewModels/MainWindowViewModel.vb` — 导航逻辑，创建所有页面 ViewModel

## 命名规范

- 函数/变量：`lowercase_with_underscores`
- 属性：`get_*`（只读）/ `set_*`（只写）/ `prop_*`（读写）
- 类/结构：`PascalCase`，单词数限制为 1，多单词通过文件夹→命名空间实现
- 接口：`PascalCase`，放在 `Utility.Interface` 命名空间下
- 常量：`lowercase_with_underscores`
- 命名空间/文件夹：`PascalCase`
- 文件夹结构即命名空间结构

## 编码约定

- `Option Explicit On` + `Option Strict On`（所有 .vb 文件开头）
- 编译绑定已启用（`AvaloniaUseCompiledBindingsByDefault=true`），所有 AXAML 必须设置 `x:DataType`
- ViewLocator 命名约定：`GUI.ViewModels.XxxViewModel` → `GUI.Views.XxxView`
- 所有页面 ViewModel 放在 `GUI.ViewModels` 命名空间下（扁平结构，不嵌套）
- GUI 中访问 Launcher 类需要 `Imports Launcher`
- **每个新功能必须加入 `Debug.WriteLine` 调试日志**，格式为 `[ClassName] message`，用于运行时排查问题
- **每次修改完 GUI 代码后直接启动应用**：`dotnet run --project GUI`，让用户立即看到效果

## Avalonia 注意事项

- VB.NET 中 `Await` 不能在 `Catch` 块内使用，需用 `has_error` 标志位重构
- `ReactiveCommand(Of Unit, Unit)` 需要 `Imports System.Reactive`
- `RaisePropertyChanged()` 接受字符串参数（属性名），不是 `PropertyChangedEventArgs`
- `VersionEntry` 等绑定到 AXAML 的类必须用属性（Property），不能用字段（Field）
- `For Each` 循环变量在 `Option Strict On` 下需要显式类型声明

### 窗口透明

- **Mac**：`TransparencyLevelHint="Transparent"` + `ExtendClientAreaToDecorationsHint="True"` 都可用，阴影效果正常
- **Windows**：透明效果不工作（Avalonia 已知限制），`TransparencyLevelHint` 和 `ExtendClientAreaToDecorationsHint` 都无法实现透明。PCL-CE 用 WPF + DWM 原生 API 实现透明，与 Avalonia 机制不同
- 标题栏拖拽：`WindowDecorations="None"` 时，需要手动处理 `PointerPressed` 实现拖拽。注意按钮点击会冒泡到标题栏，需向上遍历视觉树检测是否有 Command 属性，避免按钮被拖拽拦截

## 远程调试

**每次修改完 GUI 代码后，必须在三个环境（Mac、Windows、Linux）都启动测试。**

### Mac（本机）
```bash
dotnet run --project GUI
```

### Windows
测试机：`192.168.1.113`，用户 `admin`，密码 `2328`

```bash
# 复制文件到 Windows
sshpass -p 2328 scp <本地文件> admin@192.168.1.113:"C:/Users/admin/Projects/BonjourMC/<路径>"

# 编译
sshpass -p 2328 ssh admin@192.168.1.113 "C:\Users\admin\AppData\Local\Microsoft\dotnet\dotnet.exe build C:\Users\admin\Projects\BonjourMC\GUI\GUI.vbproj -c Debug"

# 在用户桌面会话中启动 GUI（必须用 schtasks，SSH 直接运行的窗口看不到）
sshpass -p 2328 ssh admin@192.168.1.113 'schtasks /create /tn "BonjourMC" /tr "C:\Users\admin\Projects\BonjourMC\GUI\bin\Debug\net8.0\GUI.exe" /sc once /st 00:00 /ru admin /it /f && schtasks /run /tn "BonjourMC" && schtasks /delete /tn "BonjourMC" /f'

# 杀掉 GUI 进程
sshpass -p 2328 ssh admin@192.168.1.113 "taskkill /F /IM GUI.exe"

# 读取调试日志
sshpass -p 2328 ssh admin@192.168.1.113 "type C:\Users\admin\Projects\BonjourMC\GUI\bin\Debug\net8.0\bonjourmc_debug.log"
```

**注意**：通过 SSH 直接运行 GUI 程序，窗口不会显示在用户桌面上。必须使用 `schtasks /it` 参数在用户的交互式会话中启动。

### Linux
测试机：`10.211.55.3`，用户 `bemly`，密码 `2328`（NixOS）

```bash
# SSH 连接
sshpass -p 2328 ssh bemly@10.211.55.3

# 代理设置（如需要）
export ALL_PROXY=http://10.211.55.1:7890
export HTTPS_PROXY=http://10.211.55.1:7890
export HTTP_PROXY=http://10.211.55.1:7890

# dotnet 路径
export PATH=$HOME/.dotnet:$PATH

# 编译和运行
dotnet build GUI/GUI.vbproj -c Debug
dotnet run --project GUI
```

**注意**：NixOS 无法直接运行标准 dotnet 二进制文件，需要通过 nix-shell 或特殊配置。

## 依赖

| 包 | 版本 | 用途 |
|---|---|---|
| Avalonia | 11.3.14 | GUI 框架 |
| Avalonia.ReactiveUI | 11.3.8 | MVVM 支持 |
| Newtonsoft.Json | 13.0.4 | JSON 解析 |
| Microsoft.OpenApi | 3.5.2 | OpenAPI（预留） |
| NUnit | 4.5.1 | 测试框架 |

## 文件清单

### Launcher 核心文件
- `Launcher/Core.vb` — 配置 + 入口
- `Launcher/Java/Client/Setup.vb` — 游戏启动
- `Launcher/Java/Client/Download/Mojang/Minecraft.vb` — 下载引擎
- `Launcher/Utility/Bridge/Download.vb` — HTTP 下载适配器
- `Launcher/Utility/Bridge/Json.vb` — JSON 适配器
- `Launcher/Utility/Bridge/Crypto.vb` — SHA1 校验
- `Launcher/Utility/Interface/Progress.vb` — 进度接口
- `Launcher/Utility/Model/Version.vb` — 语义化版本
- `Launcher/Utility/Model/Mojang/Minecraft.vb` — Libraries/Assets 模型
- `Launcher/Utility/Model/Mojang/VersionEntry.vb` — 版本清单条目

### GUI 文件
- `GUI/App.axaml` — 应用定义，FluentTheme
- `GUI/ViewLocator.vb` — ViewModel→View 自动解析
- `GUI/Services/LauncherService.vb` — GUI↔Launcher 门面
- `GUI/ViewModels/*.vb` — 页面 ViewModel（Home/Versions/Settings/Log）
- `GUI/Views/*.axaml` — 页面视图

## 当前状态

- GUI 4 个页面已完成（Home/Versions/Settings/Logs）
- 下载引擎已支持进度事件和异步
- Setup 已参数化，支持 fluent API
- 认证（MSA）尚未实现，当前为离线模式
- Bedrock/China 版本尚未实现
