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

    [Header("Linux PulseAudio 音频源名称（如 default 或 xxx.monitor）")]
    public string linuxSystemAudioSourceName = "default";

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

    [Header("ffmpeg 配置文件名称")] public string configFileName = "Config.txt";

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
    public bool IsRecording => _isStarting || _isStopping || (_processRunner != null && _processRunner.IsRunning);

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
#endif

                    string arguments = BuildFFmpegCaptureArguments(target, session.videoTempPath);
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

                    Debug.LogError("123123123");
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

        if (session.containsSystemAudio)
        {
            if (!WaitForFileReady(session.audioTempPath, waitTempFileReadyTimeoutMs))
            {
                throw new Exception("等待临时音频文件释放超时: " + session.audioTempPath);
            }
        }
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

            if (hasAudio)
            {
                MergeVideoAndAudio(session.videoTempPath, session.audioTempPath, session.finalOutputPath);
            }
            else
            {
                File.Move(session.videoTempPath, session.finalOutputPath);
            }

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
    /// 读取 ffmpeg 路径。
    /// 路径来自 StreamingAssets/Config.txt。
    /// </summary>
    /// <param name="executablePath">输出 ffmpeg 可执行文件路径。</param>
    /// <returns>是否读取成功。</returns>
    private bool TryLoadFFmpegPath(out string executablePath)
    {
        executablePath = string.Empty;

        try
        {
            string configPath = Path.Combine(Application.streamingAssetsPath, configFileName);

            if (!File.Exists(configPath))
            {
                Debug.LogError("未找到 Config.txt: " + configPath);
                return false;
            }

            executablePath = File.ReadAllText(configPath).Trim();

            if (string.IsNullOrEmpty(executablePath))
            {
                Debug.LogError("Config.txt 中未配置 ffmpeg 路径");
                return false;
            }

            if (!File.Exists(executablePath))
            {
                Debug.LogError("ffmpeg 路径不存在: " + executablePath);
                return false;
            }

            return true;
        }
        catch (Exception e)
        {
            Debug.LogError("读取 ffmpeg 配置失败: " + e.Message);
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
            audioTempPath   = Path.Combine(outputDir, $"{outputFilePrefix}_{stamp}_audio_tmp.wav")
        };
    }

    /// <summary>
    /// 构建 ffmpeg 录屏参数。
    /// 支持帧率、缩放、CRF、像素格式控制。
    /// </summary>
    /// <param name="target">目标显示器。</param>
    /// <param name="outputPath">输出视频临时文件路径。</param>
    /// <returns>ffmpeg 参数字符串。</returns>
    private string BuildFFmpegCaptureArguments(RecorderDisplayInfo target, string outputPath)
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
        bool useSystemAudio = audioMode == RecorderAudioMode.SystemAudio &&
                              !string.IsNullOrWhiteSpace(linuxSystemAudioSourceName);

        if (useSystemAudio)
        {
            return $"-f x11grab " +
                   $"-framerate {captureFrameRate} " +
                   $"-video_size {target.width}x{target.height} " +
                   $"-i :0.0+{target.offsetX},{target.offsetY} " +
                   $"-thread_queue_size 512 " +
                   $"-f pulse " +
                   $"-i \"{linuxSystemAudioSourceName}\" " +
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