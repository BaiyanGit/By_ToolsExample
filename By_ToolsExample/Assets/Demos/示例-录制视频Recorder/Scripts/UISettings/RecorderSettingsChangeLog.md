# Recorder Settings 修改记录

## 记录规则

每次修改录制参数设置相关功能前，先在本文档新增“任务计划”。

每完成一个具体问题后，补充以下内容：

- 为什么这么做
- 做了什么
- 影响范围
- 验证结果

## 待执行任务计划模板

### 任务计划：YYYY-MM-DD HH:mm

- 问题：
- 计划：
- 风险点：

### 修改记录：YYYY-MM-DD HH:mm

- 为什么这么做：
- 做了什么：
- 影响范围：
- 验证结果：

### 任务计划：2026-05-20 14:40

- 问题：视频推流模式目前只保留了 useMode 选项，缺少推流地址配置、推流参数保存、录制核心推流输出逻辑和操作前校验。
- 计划：新增推流地址配置字段；UI 支持“视频推流”模式下显示推流地址并隐藏本地保存路径；保存、另存为、使用前校验推流地址；录制核心在推流模式下直接输出到推流地址，暂按通用 RTMP/FLV 推流实现。
- 风险点：如果实际要支持的协议不是 RTMP/FLV，需要再按目标平台协议补充 ffmpeg muxer 和参数；场景中若还没有推流地址输入框，需要把新增 public 字段绑定到 UI，或按自动节点名查找。

### 修改记录：2026-05-20 14:40

- 为什么这么做：推流是实时链路，不能复用本地录制结束后的音视频合并流程，所以需要独立的推流地址配置和 ffmpeg 直播输出参数。
- 做了什么：新增 streamUrl 配置字段；UI 增加推流地址输入框引用、自动查找和兜底创建；推流模式下隐藏本地保存路径并显示推流地址；保存、另存为、使用前校验 rtmp:// 或 rtmps:// 地址；CrossPlatformScreenRecorder 在推流模式下输出 FLV/RTMP，停止时跳过本地文件等待和合并。
- 影响范围：RecorderParamsConfig、RecordConfigProvider、UIRecorderParamsSettings、CrossPlatformScreenRecorder、默认 JSON 配置和选项说明 JSON。
- 验证结果：git diff --check 通过，仅有 CRLF 提示；dotnet build 仍为当前工程环境的“生成失败，0 个警告，0 个错误”，未给出可定位脚本错误。

### 任务计划：2026-05-20 14:52

- 问题：推流需要明确保证沿用当前屏幕选择、不阻塞主程序，并在 Win/Linux 以及国产硬件环境下尽量稳定流畅。
- 计划：确认推流参数使用当前选中显示器；为 ffmpeg 进程设置较低优先级，避免抢占主程序；补充推流专用编码参数，采用低延迟、固定 GOP、队列缓冲和 CPU 友好的默认值；增加推流码率/帧率/缩放的安全兜底。
- 风险点：不同国产 CPU/GPU/系统发行版的硬件编码器差异很大，默认先走 libx264 CPU 编码保证兼容；后续如果要硬编码，需要按实际设备提供 h264_qsv、h264_vaapi、h264_nvenc、厂商 SDK 或系统 FFmpeg 编译能力做可选策略。

### 修改记录：2026-05-20 14:52

- 为什么这么做：推流必须继续复用当前显示器选择，同时 ffmpeg 实时编码不能抢占 Unity 主程序资源；国产硬件环境差异较大，默认参数应优先保证跨平台兼容和稳定。
- 做了什么：推流仍使用当前配置的 displayIndex/displayName 选择目标屏幕；ffmpeg 进程支持设置优先级，推流时降为 BelowNormal；推流参数增加 thread_queue_size、rtbufsize、zerolatency、固定 GOP、maxrate、bufsize、threads 0、flush_packets；对推流帧率、码率、编码预设增加安全兜底，过慢预设自动回落到 veryfast。
- 影响范围：CrossPlatformScreenRecorder 推流参数生成与 FFmpegProcessRunner 进程启动。
- 验证结果：git diff --check 通过，仅有 CRLF 提示；dotnet build 仍为当前工程环境的“生成失败，0 个警告，0 个错误”。

### 任务计划：2026-05-20 17:08

- 问题：本地录制和视频推流共用过多 UI 参数，MP4/WebM 选项混杂，推流性能参数没有独立暴露，Windows/Linux 推流是否带系统音频也缺少配置。
- 计划：新增推流专用 JSON 字段；按使用方式隐藏本地/推流专属控件；按 MP4/WebM 动态过滤视频和音频编码器选项；推流模式显示码率、GOP、缓冲区、低延迟、重连、是否带系统音频等控件；同步更新选项说明 JSON 和 README。
- 风险点：Windows 实时系统音频依赖 ffmpeg 是否编译 WASAPI 输入，若目标机 FFmpeg 不支持，需要后续接入可选音频设备列表或虚拟声卡方案。

### 修改记录：2026-05-20 17:08

- 为什么这么做：本地录制和 RTMP 推流的参数目标不同，继续混用会让使用者看到无效选项，也容易把 MP4/WebM 编码器选错。
- 做了什么：新增 streamVideoBitrate、streamGop、streamBufferSize、streamLowLatency、streamAutoReconnect、streamReconnectCount、streamReconnectIntervalMs、streamIncludeAudio 字段；推流模式下显示推流专用控件并隐藏本地保存、格式、CRF、合并等本地专属控件；MP4/WebM 下动态过滤视频和音频编码器；推流命令改用推流专用码率、GOP、缓冲区和低延迟参数；Windows/Linux 可通过 streamIncludeAudio 控制是否推系统音频；更新 OptionDescriptions.json 与 README。
- 影响范围：RecorderParamsConfig、UIRecorderParamsSettings、UIRecorderParamsSettings.RuntimeUI、UIRecorderParamsSettings.ConfigFiles、UIRecorderParamsSettings.Descriptions、CrossPlatformScreenRecorder、默认/用户 JSON 配置、README。
- 验证结果：git diff --check 通过，仅有 CRLF 提示；dotnet build 仍为当前工程环境的“生成失败，0 个警告，0 个错误”。
