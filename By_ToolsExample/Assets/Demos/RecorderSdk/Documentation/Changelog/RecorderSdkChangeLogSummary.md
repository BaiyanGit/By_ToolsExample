# Recorder SDK 更新日志摘要

## v0.3.0-beta - Stability Baseline

- 完成 `RecorderState` 状态机和 `RecorderErrorCode` 错误码体系。
- 完成 `RecorderResult`、`RecorderEventArgs`、统一事件流和 SessionHistory。
- 完成 `configId` 配置体系与 `RecorderConfigRegistry`。
- 完成 JSON `schemaVersion = 2` 配置结构升级和旧配置兼容迁移。
- 完成 FFmpeg 命令构建拆分到 `FFmpegCommandBuilder`。
- 完成音量增益配置：`enableAudioGain`、`audioGainDb`、`audioLimiterEnabled`。
- 完成基础演示场景、设置中心场景和 Editor UI Builder 工作流。
- 完成 Recorder SDK 控制台，集中管理 UI 工具、API 参数、环境检查、打包发布和文档查看。
- 打包发布工具不再包含 `Videos` 目录中的实际录制文件。

## 最近修复

- 修复控制台更新日志、验证记录长文本滚动问题。
- 修复文档中心和 API 参数页重复加载导致的卡顿风险。
- 修复 Settings 使用方式切换时左侧菜单和右侧页面不同步的问题。
- 修复 Unity 运行时动态 UI 字体 `Arial.ttf` 兼容问题。

完整日志请查看 `RecorderSettingsChangeLog.md`。
