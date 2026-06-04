# Recorder SDK 验收摘要

当前摘要用于替代 Phase1 / Phase2 / Phase3 / Stability 的长篇阶段文档，作为控制台默认展示的验收入口。历史阶段文档已归档到 `Documentation/Archive/`。

## 状态机与错误码

- `RecorderState` 已作为录制生命周期的主状态来源，覆盖 `Uninitialized`、`Ready`、`Starting`、`Recording`、`Stopping`、`Merging`、`Completed`、`Error`。
- Start / Stop / Merge / Error 统一通过状态流转处理，非法状态调用返回 `StateTransitionInvalid`，并以 Warning 形式通知调用方。
- `RecorderResult` 统一返回 `success`、`message`、`outputPath`、`errorCode`、`errorCodeText`、`sessionId`、`exception`。
- `RecorderErrorCode` 已覆盖 FFmpeg、音频、状态、合并、配置、音频滤镜等常见错误来源。

## 事件流与 SessionHistory

- 新事件流统一驱动：`OnRecorderStateChanged`、`OnRecorderWarning`、`OnRecorderError`、`OnRecorderProgress`、`OnMergeStarted`、`OnMergeCompleted`。
- 旧事件 `OnRecordStarted`、`OnRecordStopped` 保留兼容，但由新事件流程驱动，避免两套事件源。
- 所有事件均携带 `sessionId`，错误和警告事件携带 `RecorderErrorCode`。
- `SessionHistory` 已记录成功、失败、警告、合并、推流结束等会话节点。
- 后台 Merge 完成时不会覆盖新录制状态，但会写入对应旧 session 的历史记录。

## 配置系统

- 配置身份统一为 `configId`，显示名称统一为 `displayName`。
- JSON 新结构使用 `schemaVersion = 2`，不再写入 `configName` / `fileName`。
- `UseWinRecordConfig.json` / `UseLinuxRecordConfig.json` 使用 `currentConfigId` 指向当前配置。
- `RecorderConfigRegistry` 负责扫描、索引、迁移、自动修复、保存、克隆、删除用户配置。
- 旧配置可迁移读取，保存时升级为新版结构。

## UI 工具

- Runtime 脚本不再负责动态创建完整 UI。
- 屏幕录制 UI 和设置中心 UI 均通过 Editor 工具生成真实场景对象。
- 支持导出和应用 UI Layout Profile，便于手动调整后复用布局。
- 推荐统一从 `ByTools/Recorder SDK控制台` 使用 UI 创建、布局导出和布局应用工具。

## unitypackage 验证

- 打包工具默认包含 SDK 根目录、Configs，并可选包含 FFmpeg、WASAPI DLL、Documentation、Demo 场景、Videos 空目录说明。
- 不再导出 `Videos` 目录中的实际录制文件。
- 正式包需要检查 `WASAPILoopbackRecorder.dll`、Documentation、Demo 场景、Configs 等必要项。
- `ffmpeg.exe` 可选择内置到 `Assets/StreamingAssets/RecorderSDK/FFmpeg/`，也可由用户在 Settings UI 中指定。

## 已知限制

- Unity License Client IPC 环境问题会阻塞 BatchMode 编译，属于本机授权环境问题。
- 仍需在真实 Unity Editor 中完成更多 Windows / Linux 实机验证。
- 仍需补充长时间录制、多次 Start/Stop、推流重连、音频设备异常等压力验证。
- BackendFactory、Profile 导入导出、云同步能力暂未进入本阶段。

