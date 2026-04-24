# BonjourMC

跨平台 Minecraft Java Edition 启动器，使用 VB.NET + Avalonia UI 构建。

## 功能

- 下载和管理 Minecraft 版本（从 Mojang 官方 API）
- 128 线程并发下载 + SHA1 完整性校验 + 自动重试
- 启动 Minecraft 游戏进程
- 实时查看游戏日志输出
- BMCLAPI 镜像源支持（可选）

## 技术栈

| 组件 | 技术 |
|------|------|
| 语言 | VB.NET (.NET 8.0) |
| GUI | Avalonia UI 11.3.14 + FluentTheme |
| MVVM | ReactiveUI |
| JSON | Newtonsoft.Json |
| 测试 | NUnit |

## 项目结构

```
BonjourMC/
├── Launcher/          核心库（下载、启动、工具类）
│   ├── Core.vb                配置中心 + 入口
│   ├── Java/Client/
│   │   ├── Setup.vb           游戏启动器
│   │   └── Download/Mojang/
│   │       └── Minecraft.vb   下载引擎
│   └── Utility/
│       ├── Bridge/            适配器（网络、JSON、SHA1）
│       ├── Interface/         接口定义
│       └── Model/             数据模型
├── GUI/               Avalonia 桌面界面
│   ├── Views/                 页面视图（AXAML）
│   ├── ViewModels/            页面逻辑
│   ├── Services/              GUI↔Launcher 桥接
│   └── Models/                UI 数据模型
├── TUI/               命令行界面
└── Test/              单元测试
```

## 界面

GUI 包含 4 个页面：

- **Home** — 选择版本，一键启动游戏
- **Versions** — 浏览/下载/删除 Minecraft 版本
- **Settings** — 玩家名、内存分配、Java 路径、游戏目录
- **Logs** — 实时游戏输出，终止游戏进程

## 构建

```bash
# 还原依赖
dotnet restore

# 构建
dotnet build

# 运行 GUI
dotnet run --project GUI

# 运行 TUI
dotnet run --project TUI

# 运行测试
dotnet test
```

## 环境要求

- .NET 8.0 SDK
- macOS / Windows / Linux（跨平台支持）

## 测试

项目支持三端测试：Mac（本机）、Windows（远程）、Linux（远程）。

```bash
# Mac 测试
dotnet run --project GUI

# Windows 测试（远程）
# 1. 编译
sshpass -p 2328 ssh admin@192.168.1.113 "C:\Users\admin\AppData\Local\Microsoft\dotnet\dotnet.exe build C:\Users\admin\Projects\BonjourMC\GUI\GUI.vbproj -c Debug"
# 2. 复制到共享目录
sshpass -p 2328 ssh admin@192.168.1.113 "xcopy C:\Users\admin\Projects\BonjourMC\GUI\bin\Debug\net8.0\* C:\Users\Public\BonjourMC\ /Y /E"
# 3. 运行（用户 23287，session 3）
sshpass -p 2328 ssh admin@192.168.1.113 'schtasks /create /tn "BonjourMC" /tr "C:\Users\Public\BonjourMC\GUI.exe" /sc once /st 00:00 /ru 23287 /it /f && schtasks /run /tn "BonjourMC" && schtasks /delete /tn "BonjourMC" /f'
# 4. 读取日志
sshpass -p 2328 ssh admin@192.168.1.113 "type C:\Users\Public\BonjourMC\bonjourmc_debug.log"

# Linux 测试（远程，需要代理）
sshpass -p 2328 ssh bemly@10.211.55.3 "export ALL_PROXY=http://10.211.55.1:7890 && dotnet run --project GUI"
```

## 命名规范

- 函数/变量：`lowercase_with_underscores`
- 属性：`get_*`（只读）/ `set_*`（只写）/ `prop_*`（读写）
- 类/结构：`PascalCase`，单个单词，多单词通过文件夹自动命名空间
- 接口：`PascalCase`，放在 `Utility.Interface` 下
- 常量：`lowercase_with_underscores`
- 命名空间/文件夹：`PascalCase`

## 许可证

暂未指定。
