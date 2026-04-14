# CrossPlatformScreenRecorder

## 项目简介
`CrossPlatformScreenRecorder` 是一套基于 Unity 与 FFmpeg 的跨平台桌面录屏方案，支持在 **Windows** 和 **Linux（优麒麟 / X11）** 环境下，按显示器选择目标屏幕进行录制。

该方案适用于多屏场景，可直接录制某一块物理显示器的最终输出内容，因此能够录到：

- Unity 场景内容
- `Screen Space - Overlay` UI
- 该显示器上的其它桌面显示内容

---

## 功能特点

- 支持 **Windows** 与 **Linux**
- 支持 **多显示器枚举**
- 支持 **下拉框选择目标显示器**
- 支持 **开始录制 / 停止录制**
- 通过 `Config.txt` 配置 FFmpeg 路径
- 使用 FFmpeg 外部进程录制，不依赖 Unity 内部渲染管线
- 录制目标为 **物理显示器区域**，而不是单纯的 Camera 画面

---

## 脚本结构

本方案拆分为以下 4 个脚本：

### 1. `RecorderDisplayInfo.cs`
显示器信息数据结构，用于保存：

- 显示器索引
- 显示器名称
- 分辨率
- 偏移坐标
- 是否主显示器

---

### 2. `RecorderDisplayProvider.cs`
显示器信息提供器，负责按平台获取系统显示器列表：

- Windows：通过 `EnumDisplayMonitors`
- Linux：通过 `xrandr --current`

---

### 3. `FFmpegProcessRunner.cs`
FFmpeg 进程管理器，负责：

- 启动 FFmpeg
- 接收输出日志
- 发送 `q` 指令正常结束录制
- 超时后强制结束进程

---

### 4. `CrossPlatformScreenRecorder.cs`
录屏主控制器，负责：

- 读取 FFmpeg 路径配置
- 激活 Unity 多显示器
- 刷新显示器列表
- 绑定 UI 按钮
- 构建 FFmpeg 参数
- 控制开始录制与停止录制

---

## 运行环境

### Unity
建议使用支持多显示器输出的 Unity 版本。

### 平台支持
- Windows
- Linux（基于 X11，优麒麟测试通过）

> 注意：Linux 当前实现依赖 `xrandr` 与 `x11grab`，因此要求系统运行在 **X11** 环境下。

---

## 依赖说明

本方案依赖外部 FFmpeg 可执行文件。

你需要在：

```txt
StreamingAssets/Config.txt