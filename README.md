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
- macOS（主要目标平台，支持跨平台）

## 命名规范

- 函数/变量：`lowercase_with_underscores`
- 属性：`get_*`（只读）/ `set_*`（只写）/ `prop_*`（读写）
- 类/结构：`PascalCase`，单个单词，多单词通过文件夹自动命名空间
- 接口：`PascalCase`，放在 `Utility.Interface` 下
- 常量：`lowercase_with_underscores`
- 命名空间/文件夹：`PascalCase`

## 许可证

暂未指定。
