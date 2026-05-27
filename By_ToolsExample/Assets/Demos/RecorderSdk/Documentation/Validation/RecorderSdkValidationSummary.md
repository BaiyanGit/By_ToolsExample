# Recorder SDK 验证摘要

## 当前验证状态

- Editor 控制台文档懒加载、更新日志滚动、验证记录滚动已完成代码侧检查。
- unitypackage 打包路径已调整为 SDK 根目录、Configs、FFmpegApp 等必要项，不包含 `Videos` 实际录制输出。
- FFmpeg 控制组检查已覆盖 `-version`、`-devices`、`-filters`，用于识别 wasapi / alimiter 支持情况。
- `RecorderSdkSmokeTest` 已覆盖状态、事件、SessionHistory、配置迁移和音量增益命令检查。

## 仍需真实环境验证

- Unity Editor 内脚本编译。
- Windows MP4 / WebM 真实录制。
- 连续 Start / Stop 10 次以上。
- 长时间录制至少 30 分钟。
- Windows WASAPI / 无音频设备 / 音频设备拔插。
- RTMP / RTMPS 推流、Stop、失败、重连、音频开关。
- Linux x11grab、pulse、Merge 和配置加载。

## 环境限制

- 当前记录中 Unity BatchMode 曾因 License Client IPC 超时未进入脚本编译阶段。
- 该问题被归类为本机 Unity 授权环境限制，不作为 Recorder SDK 代码失败处理。

## 查看完整记录

- 运行时完整验证：`RecorderSdkRuntimeValidation.md`
- UnityPackage 完整验证：`RecorderSdkUnityPackageValidation.md`
