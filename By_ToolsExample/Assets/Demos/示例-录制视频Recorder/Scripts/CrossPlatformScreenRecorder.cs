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
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Debug = UnityEngine.Debug;

/// <summary>
/// 跨平台桌面录屏核心控制器。
/// Windows：视频由 ffmpeg 录制，系统声音由原生 WASAPI Loopback 插件录制。
/// Linux：保留 pulse 录音参数接口。
/// </summary>
public class CrossPlatformScreenRecorder : MonoBehaviour
{
    #region 屏幕相关

    [Header("是否激活全部 Unity 显示器")] public bool activateAllUnityDisplays = true;

    [Header("显示器列表缓存")] private readonly List<RecorderDisplayInfo> _displays = new();

    #endregion

    #region 录制音频相关

    [Header("音频采集模式")] public RecorderAudioMode audioMode = RecorderAudioMode.SystemAudio;

    [Header("音频编码器")] public string audioCodec = "aac";

    [Header("音频码率（如 128k / 192k）")] public string audioBitrate = "192k";

    [Header("音频采样率（仅合并时输出编码参考）")] public int audioSampleRate = 48000;

    [Header("音频声道数（仅合并时输出编码参考）")] public int audioChannels = 2;

    [Header("Linux PulseAudio 音频源名称（留空则自动探测默认输出设备的 monitor 源）")]
    public string linuxSystemAudioSourceName = "";

    #endregion

    #region 视频质量相关

    [Header("录制帧率")] [Range(10, 60)] public int captureFrameRate = 30;

    [Header("输出缩放比例，1 为原始分辨率，0.5 为半分辨率")] [Range(0.25f, 1f)]
    public float outputScale = 1f;


    [Header("x264 CRF，数值越小越清晰，文件越大")] [Range(16, 35)]
    public int videoCrf = 23;

    [Header("像素格式，兼容性更好时保持 yuv420p")] public string pixelFormat = "yuv420p";

    #endregion

    #region 录制参数相关

    [Header("输出文件前缀")] public string outputFilePrefix = "recording";

    [Header("视频编码器")] public string videoCodec = "libx264";

    [Header("视频编码预设")] public string videoPreset = "ultrafast";

    [Header("ffmpeg 进程运行器")] private FFmpegProcessRunner _processRunner;

    [Header("ffmpeg 可执行文件路径")] private string _ffmpegExecutablePath;

    [Header("停止录制等待 ffmpeg 退出超时（毫秒）")] public int stopVideoTimeoutMs = 15000;

    [Header("等待临时文件释放超时（毫秒）")] public int waitTempFileReadyTimeoutMs = 8000;

    [Header("后台合并音视频等待超时（毫秒），<=0 表示不限时")] public int mergeTimeoutMs;

    [Header("是否在后台合并完成后删除临时文件")] public bool deleteTempFilesAfterMerge = true;

    #endregion

    [Header("录屏输出目录，为空则使用 StreamingAssets")]
    public string outputDirectory = "";

    [Header("是否已初始化")] private bool _isInitialized;

    /// <summary>
    /// 是否正在异步启动录制。
    /// </summary>
    private bool _isStarting;

    /// <summary>
    /// 是否正在异步停止录制。
    /// </summary>
    private bool _isStopping;

    /// <summary>
    /// 当前录制会话。
    /// </summary>
    private CaptureSession _currentSession;

    /// <summary>
    /// 当前后台合并任务数量。
    /// </summary>
    private int _activeMergeJobs;

    /// <summary>
    /// 主线程回调队列。
    /// 后台线程完成后，将需要触发 Unity 事件或更新 UI 的逻辑投递回来。
    /// </summary>
    private readonly ConcurrentQueue<Action> _mainThreadActions = new();

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
    /// 当前是否正在录制、启动中或停止中。
    /// </summary>
    public bool IsRecording => _isStarting || _isStopping || _processRunner is { IsRunning: true };

    /// <summary>
    /// 当前是否还有后台合并任务。
    /// </summary>
    public bool IsMerging => _activeMergeJobs > 0;

    /// <summary>
    /// 当前输出文件路径。
    /// </summary>
    public string CurrentOutputFilePath => _currentSession != null ? _currentSession.finalOutputPath : string.Empty;

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
    public void Initialize()
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

        if (!TryLoadFFmpegPath(out _ffmpegExecutablePath))
        {
            return;
        }

        RefreshDisplayList();
        _isInitialized = true;
    }

    /// <summary>
    /// 获取当前显示器列表副本。
    /// </summary>
    public List<RecorderDisplayInfo> GetDisplays()
    {
        return new List<RecorderDisplayInfo>(_displays);
    }

    /// <summary>
    /// 刷新显示器列表。
    /// </summary>
    public void RefreshDisplayList()
    {
        _displays.Clear();
        _displays.AddRange(RecorderDisplayProvider.GetDisplays());

        OnDisplayListChanged?.Invoke(new List<RecorderDisplayInfo>(_displays));
    }

    /// <summary>
    /// 异步开始录制。
    /// 将“启动系统音频采集 + 启动 ffmpeg”的流程放入后台线程，减少点击开始时主线程顿一下的感觉。
    /// </summary>
    /// <param name="displayIndex">目标显示器索引。</param>
    public async void StartRecording(int displayIndex)
    {
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
            Debug.LogWarning("录制已在进行中、正在启动、或正在停止收尾，忽略重复开始。");
            return;
        }

        if (_displays.Count == 0)
        {
            Debug.LogError("当前没有可录制的显示器。");
            return;
        }

        int safeIndex = Mathf.Clamp(displayIndex, 0, _displays.Count - 1);
        var target    = _displays[safeIndex];

        _isStarting = true;
        OnRecordingStateChanged?.Invoke(true);

        var session = PrepareOutputPaths();
        session.containsSystemAudio = false;
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
                    if (audioMode == RecorderAudioMode.SystemAudio)
                    {
                        if (!WindowsLoopbackAudioRecorder.StartRecording(session.audioTempPath, out string audioError))
                        {
                            throw new Exception("启动 Windows 系统声音录制失败: " + audioError);
                        }

                        session.containsSystemAudio = true;
                    }
#elif UNITY_STANDALONE_LINUX || UNITY_EDITOR_LINUX
                    if (audioMode == RecorderAudioMode.SystemAudio)
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
        Debug.Log($"视频临时文件: {session.videoTempPath}");
        if (session.containsSystemAudio)
        {
            Debug.Log($"音频临时文件: {session.audioTempPath}");
        }

#if UNITY_STANDALONE_LINUX || UNITY_EDITOR_LINUX
        if (!string.IsNullOrWhiteSpace(session.resolvedLinuxAudioSource))
        {
            Debug.Log($"Linux 本次录制使用音频源: {session.resolvedLinuxAudioSource}");
        }
#endif
        Debug.Log($"最终输出文件: {session.finalOutputPath}");
        Debug.Log($"当前质量设置: 帧率={captureFrameRate}, 缩放={outputScale}, CRF={videoCrf}, 预设={videoPreset}");

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
        _processRunner.Stop(stopVideoTimeoutMs);
        Debug.Log("视频录制进程已停止。");

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        if (session.containsSystemAudio)
        {
            Debug.Log("停止 Windows 系统声音录制...");
            if (!WindowsLoopbackAudioRecorder.StopRecording(out string audioError))
            {
                throw new Exception("停止 Windows 系统声音录制失败: " + audioError);
            }

            Debug.Log("Windows 系统声音录制已停止。");
        }
#endif

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

            if (deleteTempFilesAfterMerge)
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
    /// 读取 ffmpeg 可执行文件路径，并在 Linux 下自动检查/补充执行权限。
    /// </summary>
    /// <param name="executablePath">输出的 ffmpeg 路径。</param>
    /// <returns>是否可用。</returns>
    private bool TryLoadFFmpegPath(out string executablePath)
    {
        executablePath = string.Empty;

        try
        {
            executablePath = Application.platform switch
            {
                RuntimePlatform.WindowsPlayer or RuntimePlatform.WindowsEditor => $"{Application.streamingAssetsPath}/FFmpegApp/ffmpeg.exe",
                RuntimePlatform.LinuxPlayer or RuntimePlatform.LinuxEditor     => $"{Application.streamingAssetsPath}/FFmpegApp/ffmpeg",
                _                                                              => throw new NotSupportedException("不支持的平台: " + Application.platform)
            };

            if (!File.Exists(executablePath))
            {
                Debug.LogError("ffmpeg 路径不存在: " + executablePath);
                return false;
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
        errorMessage = string.Empty;

        if (!string.IsNullOrWhiteSpace(linuxSystemAudioSourceName))
        {
            resolvedSourceName = linuxSystemAudioSourceName.Trim();
            Debug.Log("Linux 使用手动指定的音频源: " + resolvedSourceName);
            return true;
        }

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
        sources = new List<string>();
        errorMessage = string.Empty;

        try
        {
            using var process = new Process();
            process.StartInfo.FileName = "pactl";
            process.StartInfo.Arguments = "list short sources";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.CreateNoWindow = true;

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
    private bool TryRunProcess(string fileName, string arguments, out string stdOut, out string stdErr, out int exitCode)
    {
        stdOut = string.Empty;
        stdErr = string.Empty;
        exitCode = -1;

        try
        {
            using var process = new Process();
            process.StartInfo.FileName = fileName;
            process.StartInfo.Arguments = arguments;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.CreateNoWindow = true;

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
    /// 准备本次录制输出路径。
    /// 使用毫秒级时间戳，避免连续快速录制时重名。
    /// </summary>
    /// <returns>新建的录制会话。</returns>
    private CaptureSession PrepareOutputPaths()
    {
        if (Application.platform == RuntimePlatform.LinuxPlayer)
        {
            outputDirectory = "";
        }

        string realOutputDirectory = string.IsNullOrWhiteSpace(outputDirectory)
                                         ? Application.streamingAssetsPath
                                         : outputDirectory;

        Directory.CreateDirectory(realOutputDirectory);

        string outputDir = Path.Combine(realOutputDirectory, "RecorderVideo");
        Directory.CreateDirectory(outputDir);

        string stamp = DateTime.Now.ToString("yyyyMMdd_HHmmss_fff");

        return new CaptureSession
        {
            finalOutputPath = Path.Combine(outputDir, $"{outputFilePrefix}_{stamp}.mp4"),
            videoTempPath   = Path.Combine(outputDir, $"{outputFilePrefix}_{stamp}_video_tmp.mp4"),
            audioTempPath   = Path.Combine(outputDir, $"{outputFilePrefix}_{stamp}_audio_tmp.wav"),
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
        int scaledWidth  = MakeEven(Mathf.RoundToInt(target.width * Mathf.Clamp(outputScale, 0.25f, 1f)));
        int scaledHeight = MakeEven(Mathf.RoundToInt(target.height * Mathf.Clamp(outputScale, 0.25f, 1f)));

        string scaleArgs = (scaledWidth != target.width || scaledHeight != target.height)
                               ? $"-vf scale={scaledWidth}:{scaledHeight} "
                               : string.Empty;

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        return $"-f gdigrab " +
               $"-framerate {captureFrameRate} " +
               $"-offset_x {target.offsetX} " +
               $"-offset_y {target.offsetY} " +
               $"-video_size {target.width}x{target.height} " +
               $"-i desktop " +
               $"-y " +
               $"-c:v {videoCodec} " +
               $"-preset {videoPreset} " +
               $"-crf {videoCrf} " +
               $"-pix_fmt {pixelFormat} " +
               $"{scaleArgs}" +
               $"\"{outputPath}\"";

#elif UNITY_STANDALONE_LINUX || UNITY_EDITOR_LINUX
        string linuxSourceToUse = !string.IsNullOrWhiteSpace(resolvedLinuxAudioSource)
            ? resolvedLinuxAudioSource
            : linuxSystemAudioSourceName;

        bool useSystemAudio = audioMode == RecorderAudioMode.SystemAudio &&
                              !string.IsNullOrWhiteSpace(linuxSourceToUse);

        if (useSystemAudio)
        {
            return $"-f x11grab " +
                   $"-framerate {captureFrameRate} " +
                   $"-video_size {target.width}x{target.height} " +
                   $"-i :0.0+{target.offsetX},{target.offsetY} " +
                   $"-thread_queue_size 512 " +
                   $"-f pulse " +
                   $"-i \"{linuxSourceToUse}\" " +
                   $"-map 0:v:0 " +
                   $"-map 1:a:0 " +
                   $"-y " +
                   $"-c:v {videoCodec} " +
                   $"-preset {videoPreset} " +
                   $"-crf {videoCrf} " +
                   $"-pix_fmt {pixelFormat} " +
                   $"{scaleArgs}" +
                   $"-c:a {audioCodec} " +
                   $"-b:a {audioBitrate} " +
                   $"-ar {audioSampleRate} " +
                   $"-ac {audioChannels} " +
                   $"\"{outputPath}\"";
        }

        return $"-f x11grab " +
               $"-framerate {captureFrameRate} " +
               $"-video_size {target.width}x{target.height} " +
               $"-i :0.0+{target.offsetX},{target.offsetY} " +
               $"-y " +
               $"-c:v {videoCodec} " +
               $"-preset {videoPreset} " +
               $"-crf {videoCrf} " +
               $"-pix_fmt {pixelFormat} " +
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
        string tempOutput = outputPath + ".merging.mp4";

        if (File.Exists(tempOutput))
        {
            File.Delete(tempOutput);
        }

        string arguments =
            $"-y " +
            $"-loglevel error " +
            $"-nostats " +
            $"-i \"{videoPath}\" " +
            $"-i \"{audioPath}\" " +
            $"-map 0:v:0 " +
            $"-map 1:a:0 " +
            $"-c:v copy " +
            $"-c:a {audioCodec} " +
            $"-b:a {audioBitrate} " +
            $"-ar {audioSampleRate} " +
            $"-ac {audioChannels} " +
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
            if (mergeTimeoutMs > 0)
            {
                exited = process.WaitForExit(mergeTimeoutMs);
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

                throw new Exception($"ffmpeg 合并音视频超时，超过 {mergeTimeoutMs} ms。");
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
                    "ffmpeg 合并音视频失败，ExitCode=" + process.ExitCode +
                    (string.IsNullOrWhiteSpace(errorText) ? "" : ("\n" + errorText))
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
    private void CleanupSessionFiles(CaptureSession session)
    {
        if (session == null)
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

            string tempOutput = session.finalOutputPath + ".merging.mp4";
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