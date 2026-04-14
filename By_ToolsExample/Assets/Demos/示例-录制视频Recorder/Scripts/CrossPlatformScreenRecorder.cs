//=====================================================
// 文件名称: CrossPlatformScreenRecorder
// 创 建 者: wangbaiyan
// 创建日期: 2026-4-13
// 描    述: 跨平台桌面录屏核心控制器，负责初始化、枚举显示器、构建 FFmpeg 参数并控制录制流程，不直接依赖 UI。
//=====================================================

using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class CrossPlatformScreenRecorder : MonoBehaviour
{
    #region 屏幕相关

    [Header("是否激活全部 Unity 显示器")] public bool activateAllUnityDisplays = true;
    [Header("显示器列表缓存")] private readonly List<RecorderDisplayInfo> displays = new();

    #endregion


    #region 录制音频相关

    [Header("音频采集模式")] public RecorderAudioMode audioMode = RecorderAudioMode.SystemAudio;
    [Header("音频编码器")] public string audioCodec = "aac";
    [Header("音频采样率")] public int audioSampleRate = 48000;
    [Header("音频声道数")] public int audioChannels = 2;

    [Header("Windows 系统音频设备名称（如 Stereo Mix / virtual-audio-capture）")]
    public string windowsSystemAudioDeviceName = "立体声混音 (Realtek(R) Audio)";

    [Header("Linux PulseAudio 音频源名称（如 default 或 xxx.monitor）")]
    public string linuxSystemAudioSourceName = "default";

    #endregion

    #region 录制参数相关

    [Header("输出文件前缀")] public string outputFilePrefix = "recording";
    [Header("视频编码器")] public string videoCodec = "libx264";
    [Header("视频编码预设")] public string videoPreset = "ultrafast";
    [Header("ffmpeg 配置文件名称")] public string configFileName = "Config.txt";
    [Header("ffmpeg 进程运行器")] private FFmpegProcessRunner processRunner;
    [Header("ffmpeg 可执行文件路径")] private string ffmpegExecutablePath;

    #endregion

    [Header("当前输出文件路径")] private string currentOutputFilePath;

    [Header("录屏输出目录，为空则使用 StreamingAssets")]
    public string outputDirectory = "";

    [Header("是否已初始化")] private bool isInitialized;

    /// <summary>
    /// 当显示器列表刷新完成时触发。
    /// </summary>
    public event Action<List<RecorderDisplayInfo>> OnDisplayListChanged;

    /// <summary>
    /// 当录制状态发生变化时触发。
    /// </summary>
    public event Action<bool> OnRecordingStateChanged;

    /// <summary>
    /// 当录制开始时触发。
    /// </summary>
    public event Action<string> OnRecordStarted;

    /// <summary>
    /// 当录制停止时触发。
    /// </summary>
    public event Action<string> OnRecordStopped;

    /// <summary>
    /// 当前是否正在录制。
    /// </summary>
    public bool IsRecording => processRunner != null && processRunner.IsRunning;

    /// <summary>
    /// 当前输出文件路径。
    /// </summary>
    public string CurrentOutputFilePath => currentOutputFilePath;

    /// <summary>
    /// 初始化运行器。
    /// </summary>
    private void Awake()
    {
        processRunner = new FFmpegProcessRunner();
    }

    /// <summary>
    /// 启动时自动初始化。
    /// </summary>
    private void Start()
    {
        Initialize();
    }

    /// <summary>
    /// 初始化录屏器。
    /// </summary>
    public void Initialize()
    {
        // 防止重复初始化
        if (isInitialized) return;

        if (activateAllUnityDisplays)
        {
            for (int i = 0; i < Display.displays.Length; i++)
            {
                Display.displays[i].Activate();
            }
        }

        if (!TryLoadFFmpegPath(out ffmpegExecutablePath))
        {
            return;
        }

        RefreshDisplayList();
        isInitialized = true;
    }

    /// <summary>
    /// 获取当前显示器列表。
    /// </summary>
    /// <returns>显示器列表副本。</returns>
    public List<RecorderDisplayInfo> GetDisplays()
    {
        return new List<RecorderDisplayInfo>(displays);
    }

    /// <summary>
    /// 刷新显示器列表。
    /// </summary>
    public void RefreshDisplayList()
    {
        displays.Clear();
        displays.AddRange(RecorderDisplayProvider.GetDisplays());

        OnDisplayListChanged?.Invoke(new List<RecorderDisplayInfo>(displays));
    }

    /// <summary>
    /// 开始录制指定索引的显示器。
    /// </summary>
    /// <param name="displayIndex">目标显示器索引。</param>
    public void StartRecording(int displayIndex)
    {
        if (!isInitialized)
        {
            Initialize();
            if (!isInitialized) return;
        }

        // ffmpeg不存在或进程正在运行，则不允许再开始录制
        if (processRunner == null || processRunner.IsRunning) return;

        if (displays.Count == 0)
        {
            Debug.LogError("当前没有可录制的显示器。");
            return;
        }

        // 验证 Windows 系统声音设备配置是否合理
        ValidateWindowsAudioDevice();


        int    safeIndex = Mathf.Clamp(displayIndex, 0, displays.Count - 1);
        var    target    = displays[safeIndex];
        string arguments = BuildFFmpegArguments(target);

        Debug.Log($"启动 ffmpeg: {ffmpegExecutablePath} {arguments}");

        try
        {
            bool started = processRunner.Start(
                ffmpegExecutablePath,
                arguments,
                onStdOut: msg => Debug.Log("[ffmpeg] " + msg),
                onStdErr: msg => Debug.LogWarning("[ffmpeg] " + msg)
            );

            if (started)
            {
                Debug.Log($"开始录制显示器: {target.name}");
                Debug.Log($"输出文件: {currentOutputFilePath}");

                OnRecordingStateChanged?.Invoke(true);
                OnRecordStarted?.Invoke(currentOutputFilePath);
            }
        }
        catch (Exception e)
        {
            Debug.LogError("启动 ffmpeg 失败: " + e.Message);
        }
    }

    /// <summary>
    /// 停止当前录制。
    /// </summary>
    public void StopRecording()
    {
        if (processRunner == null || !processRunner.IsRunning)
        {
            return;
        }

        try
        {
            processRunner.Stop();
            Debug.Log("录制已停止");
            Debug.Log("输出文件: " + currentOutputFilePath);

            OnRecordingStateChanged?.Invoke(false);
            OnRecordStopped?.Invoke(currentOutputFilePath);
        }
        catch (Exception e)
        {
            Debug.LogError("停止录制出错: " + e.Message);
        }
    }

    /// <summary>
    /// 读取 ffmpeg 可执行文件路径。
    /// </summary>
    /// <param name="executablePath">输出的 ffmpeg 路径。</param>
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

    /*/// <summary>
    /// 构建 ffmpeg 录制参数。
    /// </summary>
    /// <param name="target">目标显示器信息。</param>
    /// <returns>ffmpeg 参数字符串。</returns>
    private string BuildFFmpegArguments(RecorderDisplayInfo target)
    {
        string realOutputDirectory = string.IsNullOrWhiteSpace(outputDirectory)
                                         ? Application.streamingAssetsPath
                                         : outputDirectory;

        Directory.CreateDirectory(realOutputDirectory);

        currentOutputFilePath = Path.Combine(realOutputDirectory,
            // $"{outputFilePrefix}_{target.name}_{DateTime.Now:yyyyMMdd_HHmmss}.mp4"
            $"{outputFilePrefix}_{DateTime.Now:yyyyMMdd_HHmmss}.mp4"
        );

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        return
            $"-f gdigrab " +
            $"-offset_x {target.offsetX} " +
            $"-offset_y {target.offsetY} " +
            $"-video_size {target.width}x{target.height} " +
            $"-i desktop " +
            $"-y " +
            $"-c:v {videoCodec} " +
            $"-preset {videoPreset} " +
            $"\"{currentOutputFilePath}\"";
#elif UNITY_STANDALONE_LINUX || UNITY_EDITOR_LINUX
        return
            $"-f x11grab " +
            $"-video_size {target.width}x{target.height} " +
            $"-i :0.0+{target.offsetX},{target.offsetY} " +
            $"-y " +
            $"-c:v {videoCodec} " +
            $"-preset {videoPreset} " +
            $"\"{currentOutputFilePath}\"";
#else
        return string.Empty;
#endif
    }*/

    /// <summary>
    /// 检查 Windows 系统声音设备配置是否合理。
    /// </summary>
    private void ValidateWindowsAudioDevice()
    {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        if (audioMode != RecorderAudioMode.SystemAudio)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(windowsSystemAudioDeviceName))
        {
            Debug.LogError("未配置 Windows 系统声音采集设备，将只录制视频。");
            return;
        }

        string lowerName = windowsSystemAudioDeviceName.ToLowerInvariant();
        if (lowerName.Contains("microphone") || lowerName.Contains("mic") || lowerName.Contains("麦克风"))
        {
            Debug.LogError($"当前配置的音频设备看起来像麦克风：{windowsSystemAudioDeviceName}。若要录系统声音，请改为 Stereo Mix、立体声混音或虚拟回环设备。");
        }
#endif
    }

    /*
    /// <summary>
    /// 构建 ffmpeg 录制参数。
    /// </summary>
    /// <param name="target">目标显示器信息。</param>
    /// <returns>ffmpeg 参数字符串。</returns>
    private string BuildFFmpegArguments(RecorderDisplayInfo target)
    {
        string realOutputDirectory = string.IsNullOrWhiteSpace(outputDirectory)
                                         ? Application.streamingAssetsPath
                                         : outputDirectory;

        Directory.CreateDirectory(realOutputDirectory);

        // currentOutputFilePath = Path.Combine(realOutputDirectory, $"{outputFilePrefix}_{target.name}_{DateTime.Now:yyyyMMdd_HHmmss}.mp4");
        currentOutputFilePath = Path.Combine(realOutputDirectory, $"ReocderVideo/{outputFilePrefix}_{DateTime.Now:yyyyMMdd_HHmmss}.mp4");

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        bool useSystemAudio = audioMode == RecorderAudioMode.SystemAudio && !string.IsNullOrWhiteSpace(windowsSystemAudioDeviceName);

        /*
        if (useSystemAudio)
        {
            return
                $"-f gdigrab " +
                $"-framerate 30 " +
                $"-offset_x {target.offsetX} " +
                $"-offset_y {target.offsetY} " +
                $"-video_size {target.width}x{target.height} " +
                $"-i desktop " +
                $"-thread_queue_size 512 " +
                $"-f dshow " +
                $"-i audio=\"{windowsSystemAudioDeviceName}\" " +
                $"-map 0:v:0 " +
                $"-map 1:a:0 " +
                $"-y " +
                $"-c:v {videoCodec} " +
                $"-preset {videoPreset} " +
                $"-c:a {audioCodec} " +
                $"-ar {audioSampleRate} " +
                $"-ac {audioChannels} " +
                $"\"{currentOutputFilePath}\"";
        }
        else
        {
            return
                $"-f gdigrab " +
                $"-framerate 30 " +
                $"-offset_x {target.offsetX} " +
                $"-offset_y {target.offsetY} " +
                $"-video_size {target.width}x{target.height} " +
                $"-i desktop " +
                $"-y " +
                $"-c:v {videoCodec} " +
                $"-preset {videoPreset} " +
                $"\"{currentOutputFilePath}\"";
        }#1#

        if (useSystemAudio)
        {
            return
                $"-f gdigrab " +
                $"-framerate 30 " +
                $"-offset_x {target.offsetX} " +
                $"-offset_y {target.offsetY} " +
                $"-video_size {target.width}x{target.height} " +
                $"-i desktop " +
                $"-thread_queue_size 512 " +
                $"-f dshow " +
                $"-i audio=\"{windowsSystemAudioDeviceName}\" " +
                $"-map 0:v:0 " +
                $"-map 1:a:0 " +
                $"-y " +
                $"-c:v {videoCodec} " +
                $"-preset {videoPreset} " +
                $"-c:a {audioCodec} " +
                $"-ar {audioSampleRate} " +
                $"-ac {audioChannels} " +
                $"\"{currentOutputFilePath}\"";
        }

        return
            $"-f gdigrab " +
            $"-framerate 30 " +
            $"-offset_x {target.offsetX} " +
            $"-offset_y {target.offsetY} " +
            $"-video_size {target.width}x{target.height} " +
            $"-i desktop " +
            $"-y " +
            $"-c:v {videoCodec} " +
            $"-preset {videoPreset} " +
            $"\"{currentOutputFilePath}\"";

#elif UNITY_STANDALONE_LINUX || UNITY_EDITOR_LINUX
    bool useSystemAudio = audioMode == RecorderAudioMode.SystemAudio
                          && !string.IsNullOrWhiteSpace(linuxSystemAudioSourceName );

    if (useSystemAudio)
    {
        return
            $"-f x11grab " +
            $"-framerate 30 " +
            $"-video_size {target.width}x{target.height} " +
            $"-i :0.0+{target.offsetX},{target.offsetY} " +
            $"-thread_queue_size 512 " +
            $"-f pulse " +
            $"-i \"{linuxSystemAudioSourceName }\" " +
            $"-map 0:v:0 " +
            $"-map 1:a:0 " +
            $"-y " +
            $"-c:v {videoCodec} " +
            $"-preset {videoPreset} " +
            $"-c:a {audioCodec} " +
            $"-ar {audioSampleRate} " +
            $"-ac {audioChannels} " +
            $"\"{currentOutputFilePath}\"";
    }
    else
    {
        return
            $"-f x11grab " +
            $"-framerate 30 " +
            $"-video_size {target.width}x{target.height} " +
            $"-i :0.0+{target.offsetX},{target.offsetY} " +
            $"-y " +
            $"-c:v {videoCodec} " +
            $"-preset {videoPreset} " +
            $"\"{currentOutputFilePath}\"";
    }
#else
    return string.Empty;
#endif
    }
    */
    
    [Header("音频")] public AudioCapture audioCapture;
    
    /// <summary>
    /// 获取用于录制系统声音的音频设备名称（自动查找）。
    /// </summary>
    private string GetSystemAudioDeviceName()
    {
        // 如果用户已经手动配置，优先使用
        if (!string.IsNullOrWhiteSpace(windowsSystemAudioDeviceName))
        {
            return windowsSystemAudioDeviceName;
        }

        // 自动查找系统声音设备
        try
        {
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            process.StartInfo.FileName              = ffmpegExecutablePath;
            process.StartInfo.Arguments             = "-list_devices true -f dshow -i dummy";
            process.StartInfo.UseShellExecute       = false;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.CreateNoWindow        = true;
            process.Start();

            string output = process.StandardError.ReadToEnd();
            process.WaitForExit();

            // 解析输出，寻找系统声音设备
            // 常见的设备名有 "立体声混音", "Stereo Mix", "virtual-audio-capturer" 等
            string[] lines = output.Split('\n');
            foreach (string line in lines)
            {
                // 匹配格式: "立体声混音 (Realtek(R) Audio)" (audio)
                if (line.Contains("(audio)") &&
                    (line.Contains("立体声混音") ||
                     line.Contains("Stereo Mix") ||
                     line.Contains("virtual-audio-capturer")))
                {
                    // 提取设备名称
                    int start = line.IndexOf('"') + 1;
                    int end   = line.IndexOf('"', start);
                    if (start > 0 && end > start)
                    {
                        string deviceName = line.Substring(start, end - start);
                        Debug.Log($"自动检测到系统声音设备: {deviceName}");
                        return deviceName;
                    }
                }
            }

            Debug.LogWarning("未自动检测到系统声音设备，请确保已启用'立体声混音'或安装虚拟音频设备。");
        }
        catch (Exception e)
        {
            Debug.LogError($"检测音频设备时出错: {e.Message}");
        }

        return string.Empty;
    }

    /// <summary>
    /// 构建 ffmpeg 录制参数。
    /// </summary>
    /// <param name="target">目标显示器信息。</param>
    /// <returns>ffmpeg 参数字符串。</returns>
    private string BuildFFmpegArguments(RecorderDisplayInfo target)
    {
        string realOutputDirectory = string.IsNullOrWhiteSpace(outputDirectory)
                                         ? Application.streamingAssetsPath
                                         : outputDirectory;
        Directory.CreateDirectory(realOutputDirectory);

        // 确保输出目录存在
        string outputDir = Path.Combine(realOutputDirectory, "ReocderVideo");
        if (!Directory.Exists(outputDir)) Directory.CreateDirectory(outputDir);
        currentOutputFilePath = Path.Combine(outputDir, $"{outputFilePrefix}_{DateTime.Now:yyyyMMdd_HHmmss}.mp4");

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        // 🎧 核心修改：尝试获取系统声音设备名称
        string systemAudioDevice = GetSystemAudioDeviceName();

        // 如果找到系统声音设备，则启用音频录制
        bool useSystemAudio = !string.IsNullOrWhiteSpace(systemAudioDevice) && audioMode == RecorderAudioMode.SystemAudio;

        if (useSystemAudio)
        {
            Debug.Log($"✅ 正在使用系统声音设备: {systemAudioDevice}");
            return $"-f gdigrab " +
                   $"-framerate 30 " +
                   $"-offset_x {target.offsetX} " +
                   $"-offset_y {target.offsetY} " +
                   $"-video_size {target.width}x{target.height} " +
                   $"-i desktop " +
                   $"-thread_queue_size 512 " +
                   $"-f dshow " +
                   $"-i audio=\"{systemAudioDevice}\" " +
                   $"-map 0:v:0 " +
                   $"-map 1:a:0 " +
                   $"-y " +
                   $"-c:v {videoCodec} " +
                   $"-preset {videoPreset} " +
                   $"-c:a {audioCodec} " +
                   $"-ar {audioSampleRate} " +
                   $"-ac {audioChannels} " +
                   $"\"{currentOutputFilePath}\"";
        }

        // ⚠️ 如果没有找到系统声音设备，则只录制视频，并给出警告
        Debug.LogWarning("未找到系统声音设备，将只录制视频。请检查系统中是否启用了'立体声混音'设备。");
        return $"-f gdigrab " +
               $"-framerate 30 " +
               $"-offset_x {target.offsetX} " +
               $"-offset_y {target.offsetY} " +
               $"-video_size {target.width}x{target.height} " +
               $"-i desktop " +
               $"-y " +
               $"-c:v {videoCodec} " +
               $"-preset {videoPreset} " +
               $"\"{currentOutputFilePath}\"";

#elif UNITY_STANDALONE_LINUX || UNITY_EDITOR_LINUX
    // Linux下的实现逻辑
    bool useSystemAudio = audioMode == RecorderAudioMode.SystemAudio && !string.IsNullOrWhiteSpace(linuxSystemAudioSourceName);

    if (useSystemAudio)
    {
        return $"-f x11grab " +
               $"-framerate 30 " +
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
               $"-c:a {audioCodec} " +
               $"-ar {audioSampleRate} " +
               $"-ac {audioChannels} " +
               $"\"{currentOutputFilePath}\"";
    }

    return $"-f x11grab " +
           $"-framerate 30 " +
           $"-video_size {target.width}x{target.height} " +
           $"-i :0.0+{target.offsetX},{target.offsetY} " +
           $"-y " +
           $"-c:v {videoCodec} " +
           $"-preset {videoPreset} " +
           $"\"{currentOutputFilePath}\"";
#else
    return string.Empty;
#endif
    }

    /// <summary>
    /// 销毁对象时释放进程资源。
    /// </summary>
    private void OnDestroy()
    {
        processRunner?.Dispose();
        processRunner = null;
    }
}