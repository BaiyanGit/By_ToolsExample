# Recorder SDK 第 1 阶段验收说明

## 阶段目标

第 1 阶段只验收稳定性闭环，不进入 ConfigRegistry、BackendFactory、Profile 导入导出。

目标是让 SDK 调用方可以稳定判断：

- 当前处于什么 `RecorderState`。
- 操作失败时对应什么 `RecorderErrorCode`。
- 事件按什么顺序触发。
- 哪些场景会写入 `SessionHistory`。

## RecorderState 状态流转表

### 允许流转

| From | To | 说明 |
| --- | --- | --- |
| `Uninitialized` | `Ready` | 初始化完成。 |
| `Ready` | `Starting` | 开始录制请求通过基础检查。 |
| `Starting` | `Recording` | FFmpeg/音频采集启动成功。 |
| `Starting` | `Ready` | 预留回退路径。 |
| `Recording` | `Stopping` | 调用停止录制。 |
| `Stopping` | `Merging` | 采集停止成功，进入后台合并或推流收尾。 |
| `Stopping` | `Completed` | 预留无后台处理的完成路径。 |
| `Stopping` | `Ready` | 预留停止后回到就绪路径。 |
| `Merging` | `Completed` | 所有后台处理完成，且当前没有新的录制。 |
| `Merging` | `Ready` | 预留后台处理后回到就绪路径。 |
| `Merging` | `Starting` | 允许后台合并时立刻开始下一次录制。 |
| `Completed` | `Ready` | 完成后回到就绪。 |
| `Completed` | `Starting` | 完成后直接开始下一次。 |
| `Error` | `Ready` | 错误恢复后回到就绪。 |
| `Error` | `Starting` | 错误后直接重试开始。 |
| 任意状态 | `Error` | 任意主流程异常都可进入错误状态。 |
| 任意状态 | 当前状态 | 状态不变，不重复触发事件。 |

### 禁止流转

除上表之外的流转都属于非法流转，例如：

- `Ready -> Recording`
- `Ready -> Stopping`
- `Recording -> Starting`
- `Recording -> Completed`
- `Completed -> Recording`
- `Uninitialized -> Starting`

非法流转处理方式：

- `SetState` 会触发 `OnRecorderWarning`。
- `RecorderEventArgs.errorCode = RecorderErrorCode.StateTransitionInvalid`。
- 当前实现为了保留兼容性，记录 warning 后仍会完成状态赋值。

## 事件触发顺序

说明：

- `OnConfigChanged` 来自 `ReloadConfig()`，Start 前会触发。
- 状态变化统一先触发 `OnRecorderStateChanged`，随后触发旧兼容事件 `OnRecordingStateChanged`。
- 业务事件在状态事件之后触发。

### Start 成功

1. `OnConfigChanged`
2. 如尚未初始化，初始化成功后：`OnRecorderStateChanged(Ready)` -> `OnRecordingStateChanged(false)`
3. `OnRecorderStateChanged(Starting)` -> `OnRecordingStateChanged(true)`
4. FFmpeg/音频启动成功
5. `OnRecorderStateChanged(Recording)` -> `OnRecordingStateChanged(true)`
6. `OnRecordStarted(outputPath)`
7. `RecorderResult.success = true`
8. `RecorderResult.errorCode = None`
9. `SessionHistory` 不记录，因为会话仍在进行中

### Start 失败

配置校验失败：

1. `OnConfigChanged`
2. 如需要初始化，可能先触发 `Ready` 状态事件
3. `OnRecorderStateChanged(Error)` -> `OnRecordingStateChanged(false)`
4. `OnRecorderError`
5. `RecorderResult.success = false`
6. `RecorderResult.errorCode` 使用具体原因，例如 `ConfigNull`、`FFmpegPathMissing`、`StreamUrlEmpty`、`StreamUrlInvalid`、`DisplayNotFound`、`ConfigInvalid`
7. `SessionHistory` 不记录，因为尚未创建录制会话

启动 FFmpeg/音频失败：

1. `OnConfigChanged`
2. `OnRecorderStateChanged(Starting)` -> `OnRecordingStateChanged(true)`
3. 启动异常
4. `OnRecorderStateChanged(Error)` -> `OnRecordingStateChanged(false)`
5. `OnRecorderError`
6. `RecorderResult.success = false`
7. `RecorderResult.errorCode = FFmpegStartFailed`
8. `SessionHistory` 记录一条 `Error` 会话

### Stop 成功

1. `OnRecorderStateChanged(Stopping)` -> `OnRecordingStateChanged(true)`
2. FFmpeg/音频停止成功
3. `OnRecorderStateChanged(Merging)` -> `OnRecordingStateChanged(false)`
4. `OnMergeStarted`
5. `RecorderResult.success = true`
6. `RecorderResult.errorCode = None`
7. `SessionHistory` 暂不记录，等待 Merge 完成或失败后记录

### Stop 失败

未录制时 Stop：

1. `OnRecorderWarning`
2. `RecorderResult.success = false`
3. `RecorderResult.errorCode = NotRecording`
4. `SessionHistory` 不记录

正在 Starting/Stopping 时 Stop：

1. `OnRecorderWarning`
2. `RecorderResult.success = false`
3. `RecorderResult.errorCode = StateBusy`
4. `SessionHistory` 不记录

停止采集异常：

1. `OnRecorderStateChanged(Stopping)` -> `OnRecordingStateChanged(true)`
2. 停止异常
3. `OnRecorderStateChanged(Error)` -> `OnRecordingStateChanged(false)`
4. `OnRecorderError`
5. `RecorderResult.success = false`
6. `RecorderResult.errorCode = StopFailed`
7. `SessionHistory` 记录一条 `Error` 会话

### Merge 成功

1. 后台处理完成后回到主线程
2. 如果当前没有新的录制且没有其它后台任务：`OnRecorderStateChanged(Completed)` -> `OnRecordingStateChanged(false)`
3. `SessionHistory` 记录一条 `Completed` 会话
4. `OnMergeCompleted`
5. `OnRecordStopped(outputPath)`

如果后台合并期间已经开始下一次录制：

- 不覆盖当前 `Starting/Recording/Stopping` 状态。
- 仍然记录 `SessionHistory`。
- 仍然触发 `OnMergeCompleted` 和 `OnRecordStopped`。

### Merge 失败

1. 后台处理失败后回到主线程
2. 如果当前没有新的录制且没有其它后台任务：`OnRecorderStateChanged(Error)` -> `OnRecordingStateChanged(false)`
3. `SessionHistory` 记录一条 `Error` 会话
4. `OnRecorderError`
5. `RecorderEventArgs.errorCode = MergeFailed`

如果后台合并期间已经开始下一次录制：

- 不覆盖当前 `Starting/Recording/Stopping` 状态。
- 仍然记录失败会话。
- 仍然触发 `OnRecorderError(MergeFailed)`。

### 非法状态调用 Start

当前实现中，Start 的显式拦截包括：

- `Starting`
- `Recording`
- `Stopping`
- FFmpeg 进程仍在运行
- 当前会话不为空

触发顺序：

1. `OnConfigChanged`
2. `OnRecorderError`
3. `RecorderResult.success = false`
4. `RecorderResult.errorCode = AlreadyRunning`
5. `SessionHistory` 不记录

`Merging -> Starting` 是合法流转，用于支持后台合并时开始下一次录制。

### 非法状态调用 Stop

当前实现中，Stop 的显式拦截包括：

- 没有正在运行的 FFmpeg 进程或当前会话为空：`NotRecording`
- `Starting` 或 `Stopping`：`StateBusy`

触发顺序：

1. `OnRecorderWarning`
2. `RecorderResult.success = false`
3. `RecorderResult.errorCode = NotRecording` 或 `StateBusy`
4. `SessionHistory` 不记录

## 最小验证方式

### Unity 内 SmokeTest

将 `RecorderSdkSmokeTest` 挂载到任意场景对象，勾选 `runOnStart` 或在 Inspector 右键执行 `Run Smoke Test`。

测试覆盖：

- 未录制时 Stop。
- FFmpeg 路径错误校验。
- 输出目录为空时回退默认 Videos 目录。
- 连续 Start 两次。
- 正常 Start -> Stop。
- 合并失败模拟。
- SessionHistory 是否记录。

依赖说明：

- 连续 Start、正常 Start -> Stop 需要本机配置了可用 FFmpeg。
- 合并失败模拟使用反射调用内部后台合并方法，只用于验收，不作为业务 API。

### 编译验证方式

`Assembly-CSharp.csproj` 是 Unity/Rider 生成的设计期工程，不适合作为真实命令行编译入口。

已定位现象：

- `Assembly-CSharp.csproj` 使用 `ToolsVersion=4.0`。
- 设置了 `NoStdLib=true`。
- `_TargetFrameworkDirectories` 和 `_FullFrameworkReferenceAssemblyPaths` 是 `non_empty_path_generated_by_unity.rider.package` 占位值。
- 工程依赖 Unity 的 Library、Package、Editor 编译管线和 Unity SourceGenerator。
- `dotnet build Assembly-CSharp.csproj --no-restore` 会出现“生成失败，0 个警告，0 个错误”，不能作为有效验收信号。

建议使用 Unity BatchMode 编译：

```powershell
& "C:\Program Files\Unity\Hub\Editor\6000.0.48f1\Editor\Unity.exe" -batchmode -quit -projectPath "E:\UnityProjectsE\Example\By_ToolsExample\By_ToolsExample" -logFile "Logs\UnityBatchmodeCompile.log"
```

如果需要跑 Unity Test Framework，可后续增加 EditMode/PlayMode 测试后使用：

```powershell
& "C:\Program Files\Unity\Hub\Editor\6000.0.48f1\Editor\Unity.exe" -batchmode -quit -projectPath "E:\UnityProjectsE\Example\By_ToolsExample\By_ToolsExample" -runTests -testPlatform editmode -testResults "Logs\RecorderEditModeTestResults.xml" -logFile "Logs\RecorderEditModeTests.log"
```

## 已完成

- `RecorderErrorCode` 已接入 `RecorderResult` 与 `RecorderEventArgs`。
- `RecorderStateTransition` 已集中定义状态流转。
- 事件顺序已按“状态事件优先，业务事件随后”整理。
- `SessionHistory` 已记录完成与失败会话。
- 后台合并期间开始下一次录制时，旧合并完成不会覆盖当前录制状态。
- 空 `videoSaveDirectory` 会回退默认 Videos 目录，不再误判模板配置不可用。

## 未验证

- 未在当前环境完成 Unity BatchMode 编译日志验收。
- 未在真实 Windows/Linux FFmpeg 环境完整跑通录制、停止、合并。
- Windows 推流系统音频仍依赖目标 FFmpeg 是否支持 WASAPI 输入。

## 下一阶段保留风险

- 事件流需要进一步稳定为可订阅的统一事件序列日志。
- `SessionHistory` 后续需要增加更完整的结束原因、错误码、结束时间和持续时长。
- 多个后台合并任务并行时，目前历史可记录，但 `CurrentSession` 仍只表达当前主会话/当前处理会话，不是完整任务队列。
