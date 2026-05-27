# Recorder SDK Project Structure

本文档说明 Recorder SDK 当前交付目录的职责边界。正式包路径以 `Assets/示例-录制视频Recorder/RecorderSdk/` 为准。

## Core

`Core` 保存录制 SDK 的核心运行时代码，包括录制状态、错误码、配置数据、命令构建、事件、会话记录和录制流程入口。业务项目接入录制功能时主要依赖这里的公开 API。

## UI

`UI` 保存设置中心相关的运行时绑定脚本、UI 资源和布局模板。运行时脚本只负责绑定场景中已有 UI、刷新状态和读写配置，不负责在 Play 时动态创建整套界面。

## Demo

`Demo` 保存示例场景和示例脚本。当前推荐场景包括：

- `录制器_基础演示`：最小 SDK 调用示例。
- `录制器_设置中心`：完整配置管理界面。

## Editor

`Editor` 保存 Unity 编辑器工具，包括 `RecorderSdkHubWindow` 统一控制台、基础演示 UI Builder、设置中心 UI Builder、布局导出、布局应用和指南窗口菜单管理。推荐入口是 `ByTools/🔴Recorder SDK控制台`；快捷菜单统一挂在 `ByTools/🔴Recorder SDK/` 下，并按 `UI创建`、`UI布局导出`、`UI布局应用`、`API`、`验证工具`、`打包工具` 等功能分组。

## Plugins

`Plugins` 保存平台相关原生插件。Windows 平台的 WASAPI 回环录音插件应放在：

`Assets/示例-录制视频Recorder/RecorderSdk/Plugins/Windows/x86_64/WASAPILoopbackRecorder.dll`

## Documentation

`Documentation` 保存 README、变更记录、发布说明、验收文档、运行验证文档和项目结构说明。后续功能变更需要同步更新对应文档。

## StreamingAssets/FFmpegTools

`Assets/StreamingAssets/FFmpegTools/` 保存运行时配置、FFmpeg 执行文件目录和默认视频输出目录：

- `Configs`：Recorder 配置 JSON。
- `FFmpegApp`：FFmpeg 可执行文件放置目录。
- `Videos`：默认本地录制视频输出目录。
