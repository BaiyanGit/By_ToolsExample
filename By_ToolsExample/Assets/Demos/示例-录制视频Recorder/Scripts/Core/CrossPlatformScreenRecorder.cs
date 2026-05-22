//=====================================================
// 文件名称: CrossPlatformScreenRecorder
// 描    述:
//  1) Windows 下通过原生 WASAPI Loopback 插件录制系统声音
//  2) 屏幕视频由 ffmpeg 负责录制
//  3) 开始录制 / 停止录制 / 后台合并都尽量放到后台线程，减少主线程卡顿
//  4) 停止采集后立即允许下一次开始，音视频合并在后台继续进行
//=====================================================

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;


namespace Demos.示例_录制视频Recorder.Scripts.Core
{
    using Demos.示例_录制视频Recorder.Scripts;
    using System.IO;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using UISettings;
    using UnityEngine;
    using Debug = UnityEngine.Debug;
    using System.Diagnostics;

    /// <summary>
    /// 跨平台桌面录屏核心控制器。
    /// Windows：视频由 ffmpeg 录制，系统声音由原生 WASAPI Loopback 插件录制。
    /// Linux：保留 pulse 录音参数接口。
    /// </summary>
    public class CrossPlatformScreenRecorder : MonoBehaviour
    {
        public static CrossPlatformScreenRecorder ins;
        private static GameObject _container;

        [RuntimeInitializeOnLoadMethod]
        private static void RuntimeInitialize()
        {
            if (_container != null) return;
            _container = new GameObject($"[🔴 Rec] {nameof(CrossPlatformScreenRecorder)}");
            ins        = _container.AddComponent<CrossPlatformScreenRecorder>();
            DontDestroyOnLoad(_container);
        }

        #region 屏幕相关

        [Header("是否激活全部 Unity 显示器")] public bool activateAllUnityDisplays = true;
        [Header("录制配置管理器")] public RecordConfigProvider recordConfigProvider = new();
        [Header("显示器列表缓存")] private readonly List<RecorderDisplayInfo> _displays = new();

        #endregion

        #region 录制参数相关

        [Header("ffmpeg 进程运行器")] private FFmpegProcessRunner _processRunner;
        [Header("ffmpeg 可执行文件路径")] private string _ffmpegExecutablePath;

        #endregion

        [Header("是否已初始化")] private bool _isInitialized;


        #region 私有字段

        [Header("是否正在异步启动录制")] private bool _isStarting;
        [Header("是否正在异步停止录制")] private bool _isStopping;
        [Header("当前录制会话")] private CaptureSession _currentSession;
        [Header("当前后台合并任务数量")] private int _activeMergeJobs;

        [Header("主线程回调队列")] private readonly ConcurrentQueue<Action> _mainThreadActions = new(); // 后台线程完成后，将需要触发 Unity 事件或更新 UI 的逻辑投递回来。

        [Header("是否正在录制|启动中|停止中")] public bool   IsRecording           => _isStarting || _isStopping || _processRunner is { IsRunning: true };
        [Header("当前是否还有后台合并任务")]   public bool   IsMerging             => _activeMergeJobs > 0;
        [Header("当前输出文件路径")]       public string CurrentOutputFilePath => _currentSession != null ? _currentSession.finalOutputPath : string.Empty;

        #endregion

        /// <summary>
        /// 当显示器列表刷新完成时触发。
        /// </summary>
        public event Action<List<RecorderDisplayInfo>> OnDisplayListChanged;

        /// <summary>
        /// 当录制状态发生变化时触发。
        /// true：录制中 / 启动中 / 停止中
        /// false：当前没有正在录制
        /// </summary>
        public event Action<bool> OnRecordingStateChanged;

        /// <summary>
        /// 当录制开始时触发。
        /// 参数为最终输出文件路径。
        /// </summary>
        public event Action<string> OnRecordStarted;

        /// <summary>
        /// 当最终输出文件真正生成完成时触发。
        /// 注意：这里是在后台合并完成后才触发。
        /// </summary>
        public event Action<string> OnRecordStopped;


        /// <summary>
        /// 一次录制会话的临时路径与状态。
        /// </summary>
        [Serializable]
        private class CaptureSession
        {
            [Header("最终输出路径")] public string finalOutputPath;
            [Header("视频临时路径")] public string videoTempPath;
            [Header("音频临时路径")] public string audioTempPath;
            [Header("包含系统音频")] public bool containsSystemAudio;
            [Header("是否为视频推流")] public bool isStreaming;
            [Header("Linux 已解析音频源")] public string resolvedLinuxAudioSource;
        }


        /// <summary>
        /// 创建 ffmpeg 运行器。
        /// </summary>
        private void Awake()
        {
            _processRunner = new FFmpegProcessRunner();
        }

        /// <summary>
        /// 启动时自动初始化。
        /// </summary>
        private void Start()
        {
            Initialize();
        }

        /// <summary>
        /// 处理从后台线程投递回来的主线程回调。
        /// </summary>
        private void Update()
        {
            while (_mainThreadActions.TryDequeue(out var action))
            {
                try
                {
                    action?.Invoke();
                }
                catch (Exception e)
                {
                    Debug.LogError("执行主线程回调失败: " + e);
                }
            }
        }

        /// <summary>
        /// 初始化录屏器。
        /// </summary>
        private void Initialize()
        {
            if (_isInitialized)
            {
                return;
            }

            if (activateAllUnityDisplays)
            {
                for (int i = 0; i < Display.displays.Length; i++)
                {
                    Display.displays[i].Activate();
                }
            }

            ReloadConfig();
            if (!TryLoadFFmpegPath(out _ffmpegExecutablePath))
            {
                return;
            }

            RefreshDisplayList();
            _isInitialized = true;
        }

        /// <summary>
        /// 刷新显示器列表。
        /// </summary>
        private void RefreshDisplayList()
        {
            _displays.Clear();
            _displays.AddRange(RecorderDisplayProvider.GetDisplays());
        }

        /// <summary>
        /// 重新读取当前正在使用的录制配置。
        /// </summary>
        public void ReloadConfig()
        {
            recordConfigProvider ??= new RecordConfigProvider();
            recordConfigProvider.LoadCurrentRecordConfig();
        }

        /// <summary>
        /// 获取当前录制配置。
        /// </summary>
        private RecorderParamsConfig GetCurrentConfig()
        {
            return recordConfigProvider?.Config;
        }

        /// <summary>
        /// 异步开始录制。
        /// 将“启动系统音频采集 + 启动 ffmpeg”的流程放入后台线程，减少点击开始时主线程顿一下的感觉。
        /// </summary>
        public async void StartRecording()
        {
            ReloadConfig();
            var config = GetCurrentConfig();
            if (config == null)
            {
                Debug.LogError("当前录制配置为空，请先选择并使用一个录制配置。");
                return;
            }

            if (config.useMode == 1 && string.IsNullOrWhiteSpace(config.streamUrl))
            {
                Debug.LogError("当前为视频推流模式，但未配置视频推流地址。");
                return;
            }

            if (!_isInitialized)
            {
                Initialize();
                if (!_isInitialized)
                {
                    return;
                }
            }

            if (_processRunner == null || _processRunner.IsRunning || _isStarting || _isStopping || _currentSession != null)
            {
                Debug.Log("录制已在进行中、正在启动、正在停止收尾，忽略重复开始。");
                return;
            }

            if (_displays.Count == 0)
            {
                Debug.LogError("当前没有可录制的显示器。");
                return;
            }

            bool isHaveDisplay = false;
            foreach (var display in _displays)
            {
                if (display.name == config.displayName && display.index == config.displayIndex)
                {
                    isHaveDisplay = true;
                    break;
                }
            }

            if (!isHaveDisplay)
            {
                Debug.LogError($"当前录制配置的显示器不存在:{config.displayIndex} —— {config.displayName}");
                return;
            }

            Debug.Log($"录制屏幕{config.displayIndex}");
            int safeIndex = Mathf.Clamp(config.displayIndex, 0, _displays.Count - 1);
            var target    = _displays[safeIndex];

            _isStarting = true;
            OnRecordingStateChanged?.Invoke(true);

            var session = PrepareOutputPaths();
            session.containsSystemAudio      = false;
            session.resolvedLinuxAudioSource = string.Empty;

            bool   success      = false;
            string errorMessage = string.Empty;

            Debug.Log("开始异步启动录制...");

            try
            {
                await Task.Run(() =>
                {
                    try
                    {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
                        if (config.audioMode == 1 && (!session.isStreaming || config.streamIncludeAudio))
                        {
                            if (session.isStreaming)
                            {
                                session.containsSystemAudio = true;
                            }
                            else if (!WindowsLoopbackAudioRecorder.StartRecording(session.audioTempPath, out string audioError))
                            {
                                throw new Exception("启动 Windows 系统声音录制失败: " + audioError);
                            }
                            else
                            {
                                session.containsSystemAudio = true;
                            }
                        }
#elif UNITY_STANDALONE_LINUX || UNITY_EDITOR_LINUX
                    if (config.audioMode == 1 && (!session.isStreaming || config.streamIncludeAudio))
                    {
                        if (!TryResolveLinuxSystemAudioSource(out string resolvedSource, out string resolveError))
                        {
                            throw new Exception("解析 Linux 系统音频源失败: " + resolveError);
                        }

                        session.resolvedLinuxAudioSource = resolvedSource;
                        session.containsSystemAudio = true;
                        Debug.Log("Linux 已解析系统音频源: " + resolvedSource);
                    }
#endif

                        string arguments = BuildFFmpegCaptureArguments(target, session.videoTempPath, session.resolvedLinuxAudioSource);
                        if (string.IsNullOrWhiteSpace(arguments))
                        {
                            throw new Exception("生成 ffmpeg 参数失败。");
                        }

                        bool started = _processRunner.Start(
                            _ffmpegExecutablePath,
                            arguments,
                            session.isStreaming ? ProcessPriorityClass.BelowNormal : ProcessPriorityClass.Normal,
                            onStdOut: msg => Debug.Log("[ffmpeg] " + msg),
                            onStdErr: msg => Debug.LogWarning("[ffmpeg] " + msg)
                        );
                        if (!started)
                        {
                            throw new Exception("ffmpeg 进程未能启动。");
                        }

                        success = true;
                    }
                    catch (Exception e)
                    {
                        errorMessage = e.ToString();
                    }
                });
            }
            finally
            {
                _isStarting = false;
            }

            if (!success)
            {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
                if (session.containsSystemAudio)
                {
                    WindowsLoopbackAudioRecorder.StopRecording(out _);
                }
#endif
                CleanupSessionFiles(session);
                Debug.LogError("启动录制失败: " + errorMessage);
                OnRecordingStateChanged?.Invoke(false);
                return;
            }

            _currentSession = session;

            Debug.Log($"开始录制显示器: {target.name}");
            Debug.Log(session.isStreaming ? $"视频推流地址: {session.finalOutputPath}" : $"视频临时文件: {session.videoTempPath}");
            if (session.containsSystemAudio)
            {
                Debug.Log($"音频临时文件: {session.audioTempPath}");
            }

#if UNITY_STANDALONE_LINUX || UNITY_EDITOR_LINUX
        if (!string.IsNullOrWhiteSpace(session.resolvedLinuxAudioSource))
        {
            Debug.Log($"Linux 本次录制使用音频源: {session.resolvedLinuxAudioSource}");
        }
#elif UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            if (session.isStreaming && config.audioMode == 1)
            {
                Debug.LogWarning("Windows 推流模式当前只推送画面，系统音频仍需接入实时 WASAPI/dshow 音频源后才能进入直播流。");
            }
#endif
            Debug.Log(session.isStreaming ? $"当前推流地址: {session.finalOutputPath}" : $"最终输出文件: {session.finalOutputPath}");
            Debug.Log($"当前质量设置: 帧率={config.captureFrameRate}, 缩放={config.outputScale}, CRF={config.videoCrf}, 预设={config.videoPreset}");

            OnRecordStarted?.Invoke(session.finalOutputPath);
        }

        /// <summary>
        /// 异步停止录制。
        /// 停止采集后立即允许下一次开始，音视频合并转到后台执行。
        /// </summary>
        public async void StopRecording()
        {
            if (_processRunner == null || !_processRunner.IsRunning || _currentSession == null)
            {
                Debug.LogWarning("当前没有正在进行的录制。");
                return;
            }

            if (_isStopping || _isStarting)
            {
                Debug.LogWarning("当前正在启动或停止录制，请勿重复点击。");
                return;
            }

            _isStopping = true;

            var session = _currentSession;
            _currentSession = null;

            Debug.Log("开始停止录制：先停止视频，再停止系统声音。音视频合并将转入后台，不阻塞下一次开始录制。");

            bool   captureStopSuccess = false;
            string captureStopError   = string.Empty;

            try
            {
                await Task.Run(() =>
                {
                    try
                    {
                        StopCaptureInternal(session);
                        captureStopSuccess = true;
                    }
                    catch (Exception e)
                    {
                        captureStopError = e.ToString();
                    }
                });
            }
            finally
            {
                _isStopping = false;
                OnRecordingStateChanged?.Invoke(false);
            }

            if (!captureStopSuccess)
            {
                Debug.LogError("停止采集失败: " + captureStopError);
                return;
            }

            Debug.Log("采集已停止。若有音频，将在后台进行合并。现在可以立即开始下一次录制。");

            Interlocked.Increment(ref _activeMergeJobs);
            _ = Task.Run(() => MergeSessionInBackground(session));
        }

        /// <summary>
        /// 后台线程中停止视频录制与系统音频录制，并等待临时文件可读取。
        /// </summary>
        /// <param name="session">当前录制会话。</param>
        private void StopCaptureInternal(CaptureSession session)
        {
            if (session == null)
            {
                throw new Exception("停止录制时，Session 为空。");
            }

            Debug.Log("停止视频录制进程...");
            var config = GetCurrentConfig();
            _processRunner.Stop(config != null && config.stopVideoTimeoutMs > 0 ? config.stopVideoTimeoutMs : 15000);
            Debug.Log("视频录制进程已停止。");

            if (session.isStreaming)
            {
                return;
            }

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            if (session.containsSystemAudio)
            {
                if (!WindowsLoopbackAudioRecorder.StopRecording(out string audioError))
                {
                    throw new Exception("停止 Windows 系统声音录制失败: " + audioError);
                }

                Debug.Log("Windows 系统声音录制已停止。");
            }
#endif

            int waitTempFileReadyTimeoutMs = config != null && config.waitTempFileReadyTimeoutMs > 0 ? config.waitTempFileReadyTimeoutMs : 8000;
            if (!WaitForFileReady(session.videoTempPath, waitTempFileReadyTimeoutMs))
            {
                throw new Exception("等待临时视频文件释放超时: " + session.videoTempPath);
            }

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            if (session.containsSystemAudio)
            {
                if (!WaitForFileReady(session.audioTempPath, waitTempFileReadyTimeoutMs))
                {
                    throw new Exception("等待临时音频文件释放超时: " + session.audioTempPath);
                }
            }
#endif
        }

        /// <summary>
        /// 在后台线程中处理最终输出文件。
        /// 有音频则合并，无音频则直接把临时视频移动成最终文件。
        /// </summary>
        /// <param name="session">已停止采集的会话。</param>
        private void MergeSessionInBackground(CaptureSession session)
        {
            try
            {
                if (session.isStreaming)
                {
                    EnqueueMainThread(() =>
                    {
                        Debug.Log("视频推流已停止: " + session.finalOutputPath);
                        OnRecordStopped?.Invoke(session.finalOutputPath);
                    });
                    return;
                }

                bool hasVideo = File.Exists(session.videoTempPath) && new FileInfo(session.videoTempPath).Length > 0;
                bool hasAudio = session.containsSystemAudio &&
                                File.Exists(session.audioTempPath) &&
                                new FileInfo(session.audioTempPath).Length > 44;

                if (!hasVideo)
                {
                    throw new Exception("临时视频文件不存在或大小为 0。");
                }

                if (File.Exists(session.finalOutputPath))
                {
                    File.Delete(session.finalOutputPath);
                }

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
                if (hasAudio)
                {
                    MergeVideoAndAudio(session.videoTempPath, session.audioTempPath, session.finalOutputPath);
                }
                else
                {
                    File.Move(session.videoTempPath, session.finalOutputPath);
                }
#else
            File.Move(session.videoTempPath, session.finalOutputPath);
#endif

                var config = GetCurrentConfig();
                if (config == null || config.deleteTempFilesAfterMerge)
                {
                    CleanupSessionFiles(session);
                }

                EnqueueMainThread(() =>
                {
                    Debug.Log("后台合并完成。输出文件: " + session.finalOutputPath);
                    OnRecordStopped?.Invoke(session.finalOutputPath);
                });
            }
            catch (Exception e)
            {
                EnqueueMainThread(() => { Debug.LogError("后台处理录制结果失败: " + e); });
            }
            finally
            {
                Interlocked.Decrement(ref _activeMergeJobs);
            }
        }

        /// <summary>
        /// 获取当前平台下的默认 ffmpeg 可执行文件路径。
        /// </summary>
        /// <returns>默认 ffmpeg 路径。</returns>
        private static string GetPlatformDefaultFFmpegPath()
        {
            Application.Quit();
            return Application.platform switch
            {
                RuntimePlatform.WindowsPlayer or RuntimePlatform.WindowsEditor => $"{Application.streamingAssetsPath}/FFmpegTools/FFmpegApp/ffmpeg.exe",
                RuntimePlatform.LinuxPlayer or RuntimePlatform.LinuxEditor     => $"{Application.streamingAssetsPath}/FFmpegTools/FFmpegApp/ffmpeg",
                _                                                              => string.Empty
            };
        }

        /// <summary>
        /// 读取 ffmpeg 可执行文件路径，并在 Linux 下自动检查/补充执行权限。
        /// </summary>
        /// <param name="executablePath">输出的 ffmpeg 路径。</param>
        /// <returns>是否可用。</returns>
        private bool TryLoadFFmpegPath(out string executablePath)
        {
            executablePath = string.Empty;

            try
            {
                string ffmpegPath = recordConfigProvider != null ? recordConfigProvider.FFmpegPath : string.Empty;
                if (!string.IsNullOrWhiteSpace(ffmpegPath))
                {
                    executablePath = ffmpegPath.Trim();
                    if (!File.Exists(executablePath))
                    {
                        Debug.LogError("自定义 ffmpeg 路径不存在: " + executablePath);
                        return false;
                    }
                }
                else
                {
                    executablePath = GetPlatformDefaultFFmpegPath();
                    if (string.IsNullOrWhiteSpace(executablePath) || !File.Exists(executablePath))
                    {
                        Debug.LogError("ffmpeg 路径不存在: " + executablePath);
                        return false;
                    }
                }

#if (UNITY_STANDALONE_LINUX || UNITY_EDITOR_LINUX) && !UNITY_EDITOR_WIN
            if (!EnsureLinuxExecutablePermission(executablePath, out string permissionError))
            {
                Debug.LogError("ffmpeg 执行权限检查失败: " + permissionError);
                return false;
            }
#endif

                return true;
            }
            catch (Exception e)
            {
                Debug.LogError("读取 ffmpeg 配置失败: " + e.Message);
                return false;
            }
        }

        /// <summary>
        /// Linux 下自动解析系统音频源。
        /// 优先级：
        /// 1. 如果 Inspector 手动填写了 linuxSystemAudioSourceName，则优先使用该值
        /// 2. 尝试通过 pactl get-default-sink / pactl info 找到默认输出设备
        /// 3. 在 pactl list short sources 中寻找 “默认输出设备.monitor”
        /// 4. 如果没找到，回退到第一个 monitor 源
        /// </summary>
        /// <param name="resolvedSourceName">最终可用于 ffmpeg -f pulse -i 的音频源。</param>
        /// <param name="errorMessage">失败原因。</param>
        /// <returns>是否解析成功。</returns>
        private bool TryResolveLinuxSystemAudioSource(out string resolvedSourceName, out string errorMessage)
        {
            resolvedSourceName = string.Empty;
            errorMessage       = string.Empty;

            if (!TryGetLinuxPulseSources(out List<string> sources, out string sourcesError))
            {
                errorMessage = sourcesError;
                return false;
            }

            if (sources.Count == 0)
            {
                errorMessage = "未从 pactl 获取到任何 PulseAudio source。";
                return false;
            }

            if (TryGetLinuxDefaultSinkName(out string defaultSinkName))
            {
                string expectedMonitor = defaultSinkName + ".monitor";
                foreach (string source in sources)
                {
                    if (string.Equals(source, expectedMonitor, StringComparison.Ordinal))
                    {
                        resolvedSourceName = source;
                        Debug.Log("Linux 自动匹配到默认输出设备对应的 monitor 源: " + resolvedSourceName);
                        return true;
                    }
                }

                Debug.LogWarning("未找到默认输出设备对应的 monitor 源，默认输出设备: " + defaultSinkName);
            }

            foreach (string source in sources)
            {
                if (source.EndsWith(".monitor", StringComparison.OrdinalIgnoreCase))
                {
                    resolvedSourceName = source;
                    Debug.LogWarning("Linux 未定位到默认输出 monitor，已回退使用第一个 monitor 源: " + resolvedSourceName);
                    return true;
                }
            }

            errorMessage =
                "未找到可用于录制系统声音的 PulseAudio monitor 源。\n" +
                "请在终端执行：pactl list short sources\n" +
                "然后把类似 alsa_output.xxx.monitor 的完整名称填写到 linuxSystemAudioSourceName。";
            return false;
        }

        /// <summary>
        /// 获取 Linux 下所有 PulseAudio source 名称。
        /// 使用 pactl list short sources，解析第二列的 source 名。
        /// </summary>
        /// <param name="sources">解析结果。</param>
        /// <param name="errorMessage">失败原因。</param>
        /// <returns>是否成功。</returns>
        private bool TryGetLinuxPulseSources(out List<string> sources, out string errorMessage)
        {
            sources      = new List<string>();
            errorMessage = string.Empty;

            try
            {
                using var process = new Process();
                process.StartInfo.FileName               = "pactl";
                process.StartInfo.Arguments              = "list short sources";
                process.StartInfo.UseShellExecute        = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError  = true;
                process.StartInfo.CreateNoWindow         = true;

                process.Start();

                string stdOut = process.StandardOutput.ReadToEnd();
                string stdErr = process.StandardError.ReadToEnd();

                process.WaitForExit();

                if (!string.IsNullOrWhiteSpace(stdErr))
                {
                    Debug.LogWarning("[pactl] " + stdErr);
                }

                if (process.ExitCode != 0)
                {
                    errorMessage = "执行 pactl list short sources 失败，ExitCode=" + process.ExitCode;
                    return false;
                }

                string[] lines = stdOut.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string line in lines)
                {
                    string[] parts = line.Split('\t');
                    if (parts.Length >= 2)
                    {
                        string sourceName = parts[1].Trim();
                        if (!string.IsNullOrWhiteSpace(sourceName))
                        {
                            sources.Add(sourceName);
                        }
                    }
                }

                return true;
            }
            catch (Exception e)
            {
                errorMessage = "获取 Linux PulseAudio sources 失败: " + e.Message;
                return false;
            }
        }

        /// <summary>
        /// 获取 Linux 默认输出设备（Default Sink）名称。
        /// 优先用 pactl get-default-sink；如果失败，再回退到 pactl info 解析 Default Sink。
        /// </summary>
        /// <param name="defaultSinkName">默认 sink 名称。</param>
        /// <returns>是否成功获取。</returns>
        private bool TryGetLinuxDefaultSinkName(out string defaultSinkName)
        {
            defaultSinkName = string.Empty;

            if (TryRunProcess("pactl", "get-default-sink", out string stdOut1, out _, out int exitCode1) &&
                exitCode1 == 0)
            {
                string value = stdOut1.Trim();
                if (!string.IsNullOrWhiteSpace(value))
                {
                    defaultSinkName = value;
                    return true;
                }
            }

            if (TryRunProcess("pactl", "info", out string stdOut2, out _, out int exitCode2) &&
                exitCode2 == 0)
            {
                string[] lines = stdOut2.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string line in lines)
                {
                    const string prefix = "Default Sink:";
                    if (line.TrimStart().StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    {
                        string value = line.Substring(line.IndexOf(prefix, StringComparison.OrdinalIgnoreCase) + prefix.Length).Trim();
                        if (!string.IsNullOrWhiteSpace(value))
                        {
                            defaultSinkName = value;
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// 通用子进程运行工具。
        /// </summary>
        private static bool TryRunProcess(string fileName, string arguments, out string stdOut, out string stdErr, out int exitCode)
        {
            stdOut   = string.Empty;
            stdErr   = string.Empty;
            exitCode = -1;

            try
            {
                using var process = new Process();
                process.StartInfo.FileName               = fileName;
                process.StartInfo.Arguments              = arguments;
                process.StartInfo.UseShellExecute        = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError  = true;
                process.StartInfo.CreateNoWindow         = true;

                process.Start();
                stdOut = process.StandardOutput.ReadToEnd();
                stdErr = process.StandardError.ReadToEnd();
                process.WaitForExit();
                exitCode = process.ExitCode;

                return true;
            }
            catch (Exception e)
            {
                stdErr = e.Message;
                return false;
            }
        }

        /// <summary>
        /// Linux 下检查 ffmpeg 是否可执行；如果不可执行，则自动尝试 chmod +x。
        /// </summary>
        /// <param name="filePath">ffmpeg 本地路径。</param>
        /// <param name="errorMessage">失败原因。</param>
        /// <returns>最终是否可执行。</returns>
        private bool EnsureLinuxExecutablePermission(string filePath, out string errorMessage)
        {
            errorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(filePath))
            {
                errorMessage = "文件路径为空。";
                return false;
            }

            // 先检查是否已经可执行
            if (IsLinuxFileExecutable(filePath))
            {
                Debug.Log("ffmpeg 已具备执行权限: " + filePath);
                return true;
            }

            Debug.LogWarning("ffmpeg 当前没有执行权限，尝试自动执行 chmod +x: " + filePath);

            try
            {
                using var chmodProcess = new Process();
                chmodProcess.StartInfo.FileName               = "/bin/chmod";
                chmodProcess.StartInfo.Arguments              = $"+x \"{filePath}\"";
                chmodProcess.StartInfo.UseShellExecute        = false;
                chmodProcess.StartInfo.RedirectStandardOutput = true;
                chmodProcess.StartInfo.RedirectStandardError  = true;
                chmodProcess.StartInfo.CreateNoWindow         = true;

                chmodProcess.Start();

                string stdOut = chmodProcess.StandardOutput.ReadToEnd();
                string stdErr = chmodProcess.StandardError.ReadToEnd();

                chmodProcess.WaitForExit();

                if (!string.IsNullOrWhiteSpace(stdOut))
                {
                    Debug.Log("[chmod] " + stdOut);
                }

                if (!string.IsNullOrWhiteSpace(stdErr))
                {
                    Debug.LogWarning("[chmod] " + stdErr);
                }

                if (chmodProcess.ExitCode != 0)
                {
                    errorMessage = $"chmod +x 执行失败，ExitCode={chmodProcess.ExitCode}";
                    return false;
                }
            }
            catch (Exception e)
            {
                errorMessage = "执行 chmod +x 时出错: " + e.Message;
                return false;
            }

            // 再检查一次是否真的可执行
            if (!IsLinuxFileExecutable(filePath))
            {
                errorMessage =
                    "chmod +x 执行后，文件仍不可执行。可能原因：\n" +
                    "1. 所在目录或挂载点为 noexec\n" +
                    "2. 文件系统权限受限\n" +
                    "3. 不是有效的 Linux 可执行文件";
                return false;
            }

            Debug.Log("已成功为 ffmpeg 补充执行权限: " + filePath);
            return true;
        }

        /// <summary>
        /// Linux 下判断文件是否具备执行权限。
        /// 通过 /bin/test -x 来判断，兼容 Unity 常见运行环境。
        /// </summary>
        /// <param name="filePath">文件路径。</param>
        /// <returns>是否可执行。</returns>
        private bool IsLinuxFileExecutable(string filePath)
        {
            try
            {
                using var testProcess = new Process();
                testProcess.StartInfo.FileName        = "/bin/test";
                testProcess.StartInfo.Arguments       = $"-x \"{filePath}\"";
                testProcess.StartInfo.UseShellExecute = false;
                testProcess.StartInfo.CreateNoWindow  = true;

                testProcess.Start();
                testProcess.WaitForExit();

                return testProcess.ExitCode == 0;
            }
            catch (Exception e)
            {
                Debug.LogWarning("检查 Linux 文件执行权限失败: " + e.Message);
                return false;
            }
        }

        /// <summary>
        /// 获取当前输出视频文件扩展名。
        /// 当选择 WebM 时返回 .webm，否则返回 .mp4。
        /// </summary>
        /// <returns>视频扩展名。</returns>
        private string GetVideoFileExtension()
        {
            var config = GetCurrentConfig();
            return config == null || config.outputAsWebm ? ".webm" : ".mp4";
        }

        /// <summary>
        /// 判断当前配置是否为视频推流模式。
        /// </summary>
        private bool IsStreamMode()
        {
            return (GetCurrentConfig()?.useMode ?? 0) == 1;
        }

        /// <summary>
        /// 准备本次录制输出路径。
        /// 使用毫秒级时间戳，避免连续快速录制时重名。
        /// </summary>
        /// <returns>新建的录制会话。</returns>
        private CaptureSession PrepareOutputPaths()
        {
            var    config           = GetCurrentConfig();
            bool   isStreaming      = IsStreamMode();
            string streamUrl        = config?.streamUrl?.Trim() ?? string.Empty;
            if (isStreaming)
            {
                return new CaptureSession
                {
                    finalOutputPath          = streamUrl,
                    videoTempPath            = streamUrl,
                    audioTempPath            = string.Empty,
                    isStreaming              = true,
                    resolvedLinuxAudioSource = string.Empty
                };
            }

            string outputDirectory  = recordConfigProvider != null ? recordConfigProvider.OutputDirectory : string.Empty;
            string outputFilePrefix = config == null || string.IsNullOrWhiteSpace(config.outputFilePrefix) ? "recording" : config.outputFilePrefix;
            string realOutputDirectory = string.IsNullOrWhiteSpace(outputDirectory)
                                             ? Path.Combine(Application.streamingAssetsPath, "FFmpegTools", "Videos")
                                             : outputDirectory;

            Directory.CreateDirectory(realOutputDirectory);

            string stamp          = DateTime.Now.ToString("yyyyMMdd_HHmmss_fff");
            string videoExtension = GetVideoFileExtension();

            return new CaptureSession
            {
                finalOutputPath          = Path.Combine(realOutputDirectory, $"{outputFilePrefix}_{stamp}{videoExtension}"),
                videoTempPath            = Path.Combine(realOutputDirectory, $"{outputFilePrefix}_{stamp}_video_tmp{videoExtension}"),
                audioTempPath            = Path.Combine(realOutputDirectory, $"{outputFilePrefix}_{stamp}_audio_tmp.wav"),
                isStreaming              = false,
                resolvedLinuxAudioSource = string.Empty
            };
        }

        /// <summary>
        /// 构建 ffmpeg 录屏参数。
        /// 支持帧率、缩放、CRF、像素格式控制。
        /// </summary>
        /// <param name="target">目标显示器。</param>
        /// <param name="outputPath">输出视频临时文件路径。</param>
        /// <returns>ffmpeg 参数字符串。</returns>
        private string BuildFFmpegCaptureArguments(RecorderDisplayInfo target, string outputPath, string resolvedLinuxAudioSource = "")
        {
            var config = GetCurrentConfig();
            if (config == null) return string.Empty;
            int scaledWidth  = MakeEven(Mathf.RoundToInt(target.width * Mathf.Clamp(config.outputScale, 0.25f, 1f)));
            int scaledHeight = MakeEven(Mathf.RoundToInt(target.height * Mathf.Clamp(config.outputScale, 0.25f, 1f)));

            string scaleArgs = (scaledWidth != target.width || scaledHeight != target.height)
                                   ? $"-vf scale={scaledWidth}:{scaledHeight} "
                                   : string.Empty;
            int streamFrameRate = GetSafeStreamFrameRate(config);
            string streamBitrate = GetSafeStreamBitrate(config);
            string streamPreset = GetSafeStreamPreset(config);
            int streamGop = GetSafeStreamGop(config, streamFrameRate);
            string streamBufferSize = GetSafeStreamBufferSize(config, streamBitrate);
            string lowLatencyArgs = config.streamLowLatency ? "-tune zerolatency " : string.Empty;
            string streamCommonVideoArgs = $"-c:v libx264 " +
                                           $"-preset {streamPreset} " +
                                           lowLatencyArgs +
                                           $"-b:v {streamBitrate} " +
                                           $"-maxrate {streamBitrate} " +
                                           $"-bufsize {streamBufferSize} " +
                                           $"-g {streamGop} " +
                                           $"-keyint_min {streamFrameRate} " +
                                           $"-sc_threshold 0 " +
                                           $"-threads 0 " +
                                           $"-pix_fmt yuv420p " +
                                           $"{scaleArgs}" +
                                           $"-flush_packets 1 ";

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            if (IsStreamMode())
            {
                string audioInputArgs = config.streamIncludeAudio && config.audioMode == 1
                                            ? "-thread_queue_size 512 -f wasapi -i default -map 0:v:0 -map 1:a:0 "
                                            : "-map 0:v:0 -an ";
                string audioEncodeArgs = config.streamIncludeAudio && config.audioMode == 1
                                             ? $"-c:a aac -b:a {config.audioBitrate} -ar {config.audioSampleRate} -ac {config.audioChannels} "
                                             : string.Empty;
                return $"-thread_queue_size 512 " +
                       $"-rtbufsize 256M " +
                       $"-f gdigrab " +
                       $"-framerate {streamFrameRate} " +
                       $"-offset_x {target.offsetX} " +
                       $"-offset_y {target.offsetY} " +
                       $"-video_size {target.width}x{target.height} " +
                       $"-i desktop " +
                       audioInputArgs +
                       streamCommonVideoArgs +
                       audioEncodeArgs +
                       $"-f flv " +
                       $"\"{outputPath}\"";
            }

            if (config.outputAsWebm)
            {
                return $"-f gdigrab " +
                       $"-framerate {config.captureFrameRate} " +
                       $"-offset_x {target.offsetX} " +
                       $"-offset_y {target.offsetY} " +
                       $"-video_size {target.width}x{target.height} " +
                       $"-i desktop " +
                       $"-y " +
                       $"-c:v {config.webmVideoCodec} " +
                       $"-b:v {config.webmVideoBitrate} " +
                       $"-deadline {config.webmDeadline} " +
                       $"-cpu-used {config.webmCpuUsed} " +
                       $"-pix_fmt yuv420p " +
                       $"{scaleArgs}" +
                       $"\"{outputPath}\"";
            }

            return $"-f gdigrab " +
                   $"-framerate {config.captureFrameRate} " +
                   $"-offset_x {target.offsetX} " +
                   $"-offset_y {target.offsetY} " +
                   $"-video_size {target.width}x{target.height} " +
                   $"-i desktop " +
                   $"-y " +
                   $"-c:v {config.videoCodec} " +
                   $"-preset {config.videoPreset} " +
                   $"-crf {config.videoCrf} " +
                   $"-pix_fmt {config.pixelFormat} " +
                   $"{scaleArgs}" +
                   $"\"{outputPath}\"";

#elif UNITY_STANDALONE_LINUX || UNITY_EDITOR_LINUX
        string linuxSourceToUse = !string.IsNullOrWhiteSpace(resolvedLinuxAudioSource) ? resolvedLinuxAudioSource : string.Empty;

        bool useSystemAudio = config.audioMode == 1 &&
                              !string.IsNullOrWhiteSpace(linuxSourceToUse);

        if (IsStreamMode())
        {
            string audioInputArgs = useSystemAudio && config.streamIncludeAudio
                                        ? $"-thread_queue_size 512 -f pulse -i \"{linuxSourceToUse}\" -map 0:v:0 -map 1:a:0 "
                                        : "-map 0:v:0 -an ";
            string audioEncodeArgs = useSystemAudio && config.streamIncludeAudio
                                         ? $"-c:a aac -b:a {config.audioBitrate} -ar {config.audioSampleRate} -ac {config.audioChannels} "
                                         : string.Empty;

            return $"-f x11grab " +
                   $"-thread_queue_size 512 " +
                   $"-framerate {streamFrameRate} " +
                   $"-video_size {target.width}x{target.height} " +
                   $"-i :0.0+{target.offsetX},{target.offsetY} " +
                   audioInputArgs +
                   streamCommonVideoArgs +
                   audioEncodeArgs +
                   $"-f flv " +
                   $"\"{outputPath}\"";
        }

        if (useSystemAudio)
        {
            if (config.outputAsWebm)
            {
                return $"-f x11grab " +
                       $"-framerate {config.captureFrameRate} " +
                       $"-video_size {target.width}x{target.height} " +
                       $"-i :0.0+{target.offsetX},{target.offsetY} " +
                       $"-thread_queue_size 512 " +
                       $"-f pulse " +
                       $"-i \"{linuxSourceToUse}\" " +
                       $"-map 0:v:0 " +
                       $"-map 1:a:0 " +
                       $"-y " +
                       $"-c:v {config.webmVideoCodec} " +
                       $"-b:v {config.webmVideoBitrate} " +
                       $"-deadline {config.webmDeadline} " +
                       $"-cpu-used {config.webmCpuUsed} " +
                       $"-pix_fmt yuv420p " +
                       $"{scaleArgs}" +
                       $"-c:a {config.webmAudioCodec} " +
                       $"-b:a {config.audioBitrate} " +
                       $"-ar {config.audioSampleRate} " +
                       $"-ac {config.audioChannels} " +
                       $"\"{outputPath}\"";
            }

            return $"-f x11grab " +
                   $"-framerate {config.captureFrameRate} " +
                   $"-video_size {target.width}x{target.height} " +
                   $"-i :0.0+{target.offsetX},{target.offsetY} " +
                   $"-thread_queue_size 512 " +
                   $"-f pulse " +
                   $"-i \"{linuxSourceToUse}\" " +
                   $"-map 0:v:0 " +
                   $"-map 1:a:0 " +
                   $"-y " +
                   $"-c:v {config.videoCodec} " +
                   $"-preset {config.videoPreset} " +
                   $"-crf {config.videoCrf} " +
                   $"-pix_fmt {config.pixelFormat} " +
                   $"{scaleArgs}" +
                   $"-c:a {config.audioCodec} " +
                   $"-b:a {config.audioBitrate} " +
                   $"-ar {config.audioSampleRate} " +
                   $"-ac {config.audioChannels} " +
                   $"\"{outputPath}\"";
        }

        if (config.outputAsWebm)
        {
            return $"-f x11grab " +
                   $"-framerate {config.captureFrameRate} " +
                   $"-video_size {target.width}x{target.height} " +
                   $"-i :0.0+{target.offsetX},{target.offsetY} " +
                   $"-y " +
                   $"-c:v {config.webmVideoCodec} " +
                   $"-b:v {config.webmVideoBitrate} " +
                   $"-deadline {config.webmDeadline} " +
                   $"-cpu-used {config.webmCpuUsed} " +
                   $"-pix_fmt yuv420p " +
                   $"{scaleArgs}" +
                   $"\"{outputPath}\"";
        }

        return $"-f x11grab " +
               $"-framerate {config.captureFrameRate} " +
               $"-video_size {target.width}x{target.height} " +
               $"-i :0.0+{target.offsetX},{target.offsetY} " +
               $"-y " +
               $"-c:v {config.videoCodec} " +
               $"-preset {config.videoPreset} " +
               $"-crf {config.videoCrf} " +
               $"-pix_fmt {config.pixelFormat} " +
               $"{scaleArgs}" +
               $"\"{outputPath}\"";
#else
        return string.Empty;
#endif
        }

        /// <summary>
        /// 使用 ffmpeg 合并临时视频与临时音频。
        /// 合并输出先写到临时 mp4，再原子替换为最终文件。
        /// </summary>
        /// <param name="videoPath">临时视频文件。</param>
        /// <param name="audioPath">临时音频文件。</param>
        /// <param name="outputPath">最终输出文件。</param>
        private void MergeVideoAndAudio(string videoPath, string audioPath, string outputPath)
        {
            string  extension          = Path.GetExtension(outputPath);
            string  fileNameWithoutExt = Path.GetFileNameWithoutExtension(outputPath);
            string? dir                = Path.GetDirectoryName(outputPath);
            string  tempOutput         = Path.Combine(dir ?? string.Empty, $"{fileNameWithoutExt}.merging{extension}");

            if (File.Exists(tempOutput))
            {
                File.Delete(tempOutput);
            }

            var config = GetCurrentConfig();
            if (config == null) throw new Exception("合并音视频时当前录制配置为空。");
            string targetAudioCodec = config.outputAsWebm ? config.webmAudioCodec : config.audioCodec;

            string arguments =
                $"-y " +
                $"-loglevel error " +
                $"-nostats " +
                $"-i \"{videoPath}\" " +
                $"-i \"{audioPath}\" " +
                $"-map 0:v:0 " +
                $"-map 1:a:0 " +
                $"-c:v copy " +
                $"-c:a {targetAudioCodec} " +
                $"-b:a {config.audioBitrate} " +
                $"-ar {config.audioSampleRate} " +
                $"-ac {config.audioChannels} " +
                $"-shortest " +
                $"\"{tempOutput}\"";

            var errorBuilder = new StringBuilder();

            using (var process = new Process())
            {
                process.StartInfo.FileName               = _ffmpegExecutablePath;
                process.StartInfo.Arguments              = arguments;
                process.StartInfo.UseShellExecute        = false;
                process.StartInfo.RedirectStandardError  = true;
                process.StartInfo.RedirectStandardOutput = false;
                process.StartInfo.CreateNoWindow         = true;

                process.ErrorDataReceived += (_, e) =>
                {
                    if (!string.IsNullOrWhiteSpace(e.Data))
                    {
                        lock (errorBuilder)
                        {
                            errorBuilder.AppendLine(e.Data);
                        }
                    }
                };

                process.Start();
                process.BeginErrorReadLine();

                bool exited;
                if (config.mergeTimeoutMs > 0)
                {
                    exited = process.WaitForExit(config.mergeTimeoutMs);
                }
                else
                {
                    process.WaitForExit();
                    exited = true;
                }

                if (!exited)
                {
                    try
                    {
                        process.Kill();
                        process.WaitForExit();
                    }
                    catch
                    {
                        // ignored
                    }

                    throw new Exception($"ffmpeg 合并音视频超时，超过 {config.mergeTimeoutMs} ms。");
                }

                process.WaitForExit();

                if (process.ExitCode != 0)
                {
                    string errorText;
                    lock (errorBuilder)
                    {
                        errorText = errorBuilder.ToString();
                    }

                    throw new Exception(
                        "ffmpeg 合并音视频失败，ExitCode=" + process.ExitCode + (string.IsNullOrWhiteSpace(errorText) ? "" : "" + errorText)
                    );
                }
            }

            if (!File.Exists(tempOutput) || new FileInfo(tempOutput).Length == 0)
            {
                throw new Exception("ffmpeg 合并完成，但输出文件不存在或大小为 0。");
            }

            if (File.Exists(outputPath))
            {
                File.Delete(outputPath);
            }

            File.Move(tempOutput, outputPath);
        }

        /// <summary>
        /// 等待临时文件可读取。
        /// 用于避免进程刚退出但文件句柄仍未释放时立刻读取失败。
        /// </summary>
        /// <param name="path">文件路径。</param>
        /// <param name="timeoutMs">超时时间。</param>
        /// <returns>文件是否在超时前变为可读取。</returns>
        private bool WaitForFileReady(string path, int timeoutMs)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return false;
            }

            var stopwatch = Stopwatch.StartNew();

            while (stopwatch.ElapsedMilliseconds < timeoutMs)
            {
                try
                {
                    if (File.Exists(path))
                    {
                        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
                        return true;
                    }
                }
                catch
                {
                    // 文件还被占用，继续等待
                }

                Thread.Sleep(100);
            }

            return false;
        }

        /// <summary>
        /// 清理一个录制会话的临时文件。
        /// </summary>
        /// <param name="session">录制会话。</param>
        private static void CleanupSessionFiles(CaptureSession session)
        {
            if (session == null)
            {
                return;
            }

            if (session.isStreaming)
            {
                return;
            }

            try
            {
                if (!string.IsNullOrWhiteSpace(session.videoTempPath) && File.Exists(session.videoTempPath))
                {
                    File.Delete(session.videoTempPath);
                }

                if (!string.IsNullOrWhiteSpace(session.audioTempPath) && File.Exists(session.audioTempPath))
                {
                    File.Delete(session.audioTempPath);
                }

                string  extension          = Path.GetExtension(session.finalOutputPath);
                string  fileNameWithoutExt = Path.GetFileNameWithoutExtension(session.finalOutputPath);
                string? dir                = Path.GetDirectoryName(session.finalOutputPath);
                string  tempOutput         = Path.Combine(dir ?? string.Empty, $"{fileNameWithoutExt}.merging{extension}");
                if (File.Exists(tempOutput))
                {
                    File.Delete(tempOutput);
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning("清理临时文件失败: " + e.Message);
            }
        }

        /// <summary>
        /// 将需要在主线程执行的回调压入队列。
        /// </summary>
        /// <param name="action">回调。</param>
        private void EnqueueMainThread(Action action)
        {
            if (action != null)
            {
                _mainThreadActions.Enqueue(action);
            }
        }

        /// <summary>
        /// 把分辨率修正为偶数，避免 H.264 编码时出现奇数宽高问题。
        /// </summary>
        /// <param name="value">原始值。</param>
        /// <returns>偶数值。</returns>
        private static int MakeEven(int value)
        {
            if (value < 2)
            {
                value = 2;
            }

            return value % 2 == 0 ? value : value - 1;
        }

        /// <summary>
        /// 获取推流安全帧率，避免异常配置导致编码压力过高。
        /// </summary>
        private static int GetSafeStreamFrameRate(RecorderParamsConfig config)
        {
            int frameRate = config != null && config.captureFrameRate > 0 ? config.captureFrameRate : 25;
            return Mathf.Clamp(frameRate, 10, 60);
        }

        /// <summary>
        /// 获取推流安全码率，未配置时使用兼容性较好的 3M。
        /// </summary>
        private static string GetSafeStreamBitrate(RecorderParamsConfig config)
        {
            if (config == null) return "3M";
            return string.IsNullOrWhiteSpace(config.streamVideoBitrate) ? "3M" : config.streamVideoBitrate.Trim();
        }

        /// <summary>
        /// 获取推流编码预设，过慢的预设会自动回落，减少国产硬件上的主程序卡顿风险。
        /// </summary>
        private static string GetSafeStreamPreset(RecorderParamsConfig config)
        {
            string preset = config == null || string.IsNullOrWhiteSpace(config.videoPreset) ? "veryfast" : config.videoPreset.Trim();
            return preset == "ultrafast" || preset == "veryfast" || preset == "faster" ? preset : "veryfast";
        }

        /// <summary>
        /// 获取推流 GOP，未配置时按帧率的 2 倍兜底。
        /// </summary>
        private static int GetSafeStreamGop(RecorderParamsConfig config, int streamFrameRate)
        {
            int gop = config != null && config.streamGop > 0 ? config.streamGop : streamFrameRate * 2;
            return Mathf.Clamp(gop, streamFrameRate, streamFrameRate * 4);
        }

        /// <summary>
        /// 获取推流缓冲区大小。
        /// </summary>
        private static string GetSafeStreamBufferSize(RecorderParamsConfig config, string streamBitrate)
        {
            return config == null || string.IsNullOrWhiteSpace(config.streamBufferSize) ? GetDoubleBitrate(streamBitrate) : config.streamBufferSize.Trim();
        }

        /// <summary>
        /// 计算推流缓冲区码率，保持网络波动下的平滑输出。
        /// </summary>
        private static string GetDoubleBitrate(string bitrate)
        {
            if (string.IsNullOrWhiteSpace(bitrate)) return "6M";
            string value = bitrate.Trim();
            char suffix = value[value.Length - 1];
            string numberPart = char.IsLetter(suffix) ? value.Substring(0, value.Length - 1) : value;
            if (!float.TryParse(numberPart, out float number)) return "6M";
            return char.IsLetter(suffix) ? $"{number * 2:0.#}{suffix}" : $"{number * 2:0.#}";
        }

        /// <summary>
        /// 对象销毁时尝试停止系统音频录制，并释放 ffmpeg 进程资源。
        /// </summary>
        private void OnDestroy()
        {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            if (WindowsLoopbackAudioRecorder.IsRecording)
            {
                WindowsLoopbackAudioRecorder.StopRecording(out _);
            }
#endif
            _processRunner?.Dispose();
            _processRunner = null;
        }
    }
}
