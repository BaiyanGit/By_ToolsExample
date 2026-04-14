//=====================================================
// 文件名称: CrossPlatformScreenRecorder
// 创 建 者: wangbaiyan
// 创建日期: 2026-4-13
// 描    述: 跨平台桌面录屏核心控制器，负责初始化、枚举显示器、构建 FFmpeg 参数并控制录制流程，不直接依赖 UI。
//=====================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
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

    [Header("Windows 系统音频设备名称（留空则自动检测，也可手填 alternative name）")]
    public string windowsSystemAudioDeviceName = "";

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

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
    [Serializable]
    private class WindowsAudioDeviceInfo
    {
        public string displayName;
        public string alternativeName;
    }
#endif

    public event Action<List<RecorderDisplayInfo>> OnDisplayListChanged;
    public event Action<bool> OnRecordingStateChanged;
    public event Action<string> OnRecordStarted;
    public event Action<string> OnRecordStopped;

    public bool IsRecording => processRunner != null && processRunner.IsRunning;
    public string CurrentOutputFilePath => currentOutputFilePath;

    private void Awake()
    {
        processRunner = new FFmpegProcessRunner();
    }

    private void Start()
    {
        Initialize();
    }

    public void Initialize()
    {
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

    public List<RecorderDisplayInfo> GetDisplays()
    {
        return new List<RecorderDisplayInfo>(displays);
    }

    public void RefreshDisplayList()
    {
        displays.Clear();
        displays.AddRange(RecorderDisplayProvider.GetDisplays());
        OnDisplayListChanged?.Invoke(new List<RecorderDisplayInfo>(displays));
    }

    public void StartRecording(int displayIndex)
    {
        if (!isInitialized)
        {
            Initialize();
            if (!isInitialized) return;
        }

        if (processRunner == null || processRunner.IsRunning) return;

        if (displays.Count == 0)
        {
            Debug.LogError("当前没有可录制的显示器。");
            return;
        }

        int safeIndex = Mathf.Clamp(displayIndex, 0, displays.Count - 1);
        var target = displays[safeIndex];

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        WindowsAudioDeviceInfo resolvedWindowsAudioDevice = null;

        if (audioMode == RecorderAudioMode.SystemAudio)
        {
            if (!TryResolveWindowsSystemAudioDevice(out resolvedWindowsAudioDevice, out string audioError))
            {
                Debug.LogError(audioError);
                Debug.LogError("本次录制已取消：当前选择的是“系统音频”，但没有找到可用的 Windows 系统音频设备。");
                return;
            }
        }
        else if (audioMode == RecorderAudioMode.UnityAudioReserved)
        {
            Debug.LogWarning("当前版本尚未实现 Unity 音频采集，将只录制视频。");
        }

        string arguments = BuildFFmpegArguments(target, resolvedWindowsAudioDevice);
#else
        string arguments = BuildFFmpegArguments(target);
#endif

        if (string.IsNullOrWhiteSpace(arguments))
        {
            Debug.LogError("生成 ffmpeg 参数失败。");
            return;
        }

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

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN

    private bool TryResolveWindowsSystemAudioDevice(out WindowsAudioDeviceInfo resolvedDevice, out string errorMessage)
    {
        resolvedDevice = null;
        errorMessage = string.Empty;

        List<WindowsAudioDeviceInfo> devices = GetWindowsDShowAudioDevices(out string rawOutput);
        LogWindowsAudioDevices(devices, rawOutput);

        if (devices.Count == 0)
        {
            errorMessage =
                "FFmpeg 没有枚举到任何 DirectShow 音频输入设备，无法录制系统音频。\n" +
                "请确认 ffmpeg 可正常执行，且系统中存在立体声混音或其他回环设备。";
            return false;
        }

        string manualName = windowsSystemAudioDeviceName == null ? string.Empty : windowsSystemAudioDeviceName.Trim();

        // 手动指定：允许填写 displayName，也允许直接填 alternative name
        if (!string.IsNullOrWhiteSpace(manualName))
        {
            foreach (var device in devices)
            {
                if (StringEqualsIgnoreCase(device.displayName, manualName) ||
                    StringEqualsIgnoreCase(device.alternativeName, manualName) ||
                    StringContainsIgnoreCase(device.displayName, manualName) ||
                    StringContainsIgnoreCase(device.alternativeName, manualName))
                {
                    resolvedDevice = device;
                    Debug.Log($"使用手动指定匹配到的 Windows 系统音频设备: display=[{device.displayName}] alt=[{device.alternativeName}]");
                    return true;
                }
            }

            // 如果用户直接填了完整 alternative name，但枚举比对仍未命中，也继续尝试
            resolvedDevice = new WindowsAudioDeviceInfo
            {
                displayName = manualName,
                alternativeName = manualName.StartsWith("@device_", StringComparison.OrdinalIgnoreCase) ? manualName : string.Empty
            };

            Debug.LogWarning($"手动填写的设备名未在枚举列表中命中，将直接尝试打开：{manualName}");
            return true;
        }

        // 自动优先选择更像系统回环的设备
        foreach (var device in devices)
        {
            if (IsLikelySystemAudioLoopbackDevice(device.displayName))
            {
                resolvedDevice = device;
                Debug.Log($"自动检测到 Windows 系统音频设备: display=[{device.displayName}] alt=[{device.alternativeName}]");
                return true;
            }
        }

        // 如果没命中关键词，但存在 alternative name，优先选第二个音频设备（很多机器上第一个是耳机/麦克风，第二个是 Stereo Mix）
        if (devices.Count >= 2)
        {
            resolvedDevice = devices[1];
            Debug.LogWarning($"未命中特征关键词，已回退选择第 2 个音频设备: display=[{resolvedDevice.displayName}] alt=[{resolvedDevice.alternativeName}]");
            return true;
        }

        // 最后兜底选第一个
        resolvedDevice = devices[0];
        Debug.LogWarning($"未命中特征关键词，已回退选择第 1 个音频设备: display=[{resolvedDevice.displayName}] alt=[{resolvedDevice.alternativeName}]");
        return true;
    }

    private List<WindowsAudioDeviceInfo> GetWindowsDShowAudioDevices(out string rawOutput)
    {
        rawOutput = string.Empty;
        List<WindowsAudioDeviceInfo> devices = new List<WindowsAudioDeviceInfo>();

        try
        {
            using (System.Diagnostics.Process process = new System.Diagnostics.Process())
            {
                process.StartInfo.FileName = ffmpegExecutablePath;
                process.StartInfo.Arguments = "-hide_banner -list_devices true -f dshow -i dummy";
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.CreateNoWindow = true;
                process.Start();

                string stdOut = process.StandardOutput.ReadToEnd();
                string stdErr = process.StandardError.ReadToEnd();

                if (!process.WaitForExit(5000))
                {
                    try { process.Kill(); } catch { }
                }

                rawOutput = (stdErr + Environment.NewLine + stdOut).Trim();
            }

            string[] lines = rawOutput.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            WindowsAudioDeviceInfo pendingDevice = null;

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].Trim();

                // 设备显示名行：只要包含 (audio) 就解析，不再依赖 “DirectShow audio devices” 标题
                if (line.Contains("(audio)"))
                {
                    string displayName = ExtractQuotedContent(line);
                    if (!string.IsNullOrWhiteSpace(displayName))
                    {
                        pendingDevice = new WindowsAudioDeviceInfo
                        {
                            displayName = displayName.Trim(),
                            alternativeName = string.Empty
                        };
                        devices.Add(pendingDevice);
                    }

                    continue;
                }

                // alternative name 紧跟在设备行后面
                if (pendingDevice != null && line.Contains("Alternative name"))
                {
                    string altName = ExtractQuotedContent(line);

                    if (!string.IsNullOrWhiteSpace(altName))
                    {
                        pendingDevice.alternativeName = altName.Trim();
                        continue;
                    }

                    // 少数输出格式里 alternative name 可能拆到下一行
                    if (i + 1 < lines.Length)
                    {
                        string nextLine = lines[i + 1].Trim();
                        string nextQuoted = ExtractQuotedContent(nextLine);
                        if (!string.IsNullOrWhiteSpace(nextQuoted))
                        {
                            pendingDevice.alternativeName = nextQuoted.Trim();
                            i++;
                            continue;
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            rawOutput = e.ToString();
            Debug.LogError("枚举 Windows DirectShow 音频设备失败: " + e.Message);
        }

        return devices;
    }

    private void LogWindowsAudioDevices(List<WindowsAudioDeviceInfo> devices, string rawOutput)
    {
        if (devices == null || devices.Count == 0)
        {
            Debug.LogWarning("FFmpeg 未识别到任何 Windows 音频输入设备。原始输出如下：\n" + rawOutput);
            return;
        }

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("Windows DirectShow 音频设备列表：");

        for (int i = 0; i < devices.Count; i++)
        {
            sb.AppendLine($"  [{i}] display = {devices[i].displayName}");
            sb.AppendLine($"      alt     = {devices[i].alternativeName}");
        }

        Debug.Log(sb.ToString());
    }

    private bool IsLikelySystemAudioLoopbackDevice(string deviceName)
    {
        if (string.IsNullOrWhiteSpace(deviceName))
        {
            return false;
        }

        string lower = deviceName.ToLowerInvariant();

        // 排除典型麦克风/耳机输入
        if (lower.Contains("microphone") ||
            lower.Contains("mic") ||
            lower.Contains("麦克风") ||
            lower.Contains("headset mic") ||
            lower.Contains("阵列") ||
            lower.Contains("array"))
        {
            return false;
        }

        // 正常名称
        if (lower.Contains("stereo mix") ||
            lower.Contains("立体声混音") ||
            lower.Contains("virtual-audio-capturer") ||
            lower.Contains("virtual audio capturer") ||
            lower.Contains("what u hear") ||
            lower.Contains("wave out") ||
            lower.Contains("loopback") ||
            lower.Contains("monitor"))
        {
            return true;
        }

        // 兼容你当前日志里的“立体声混音”乱码
        if (lower.Contains("绔嬩綋澹版贩闊"))
        {
            return true;
        }

        // Realtek 的 Stereo Mix 经常带这个关键字
        if (lower.Contains("realtek"))
        {
            return true;
        }

        return false;
    }

    private string GetBestDShowAudioInputName(WindowsAudioDeviceInfo device)
    {
        if (device == null)
        {
            return string.Empty;
        }

        if (!string.IsNullOrWhiteSpace(device.alternativeName))
        {
            return device.alternativeName;
        }

        return device.displayName ?? string.Empty;
    }

    private string ExtractQuotedContent(string line)
    {
        if (string.IsNullOrWhiteSpace(line))
        {
            return string.Empty;
        }

        int firstQuote = line.IndexOf('"');
        if (firstQuote < 0) return string.Empty;

        int secondQuote = line.IndexOf('"', firstQuote + 1);
        if (secondQuote <= firstQuote) return string.Empty;

        return line.Substring(firstQuote + 1, secondQuote - firstQuote - 1);
    }

    private bool StringEqualsIgnoreCase(string a, string b)
    {
        return !string.IsNullOrWhiteSpace(a) &&
               !string.IsNullOrWhiteSpace(b) &&
               string.Equals(a.Trim(), b.Trim(), StringComparison.OrdinalIgnoreCase);
    }

    private bool StringContainsIgnoreCase(string a, string b)
    {
        return !string.IsNullOrWhiteSpace(a) &&
               !string.IsNullOrWhiteSpace(b) &&
               a.IndexOf(b, StringComparison.OrdinalIgnoreCase) >= 0;
    }

    private string EscapeForDShowDevice(string deviceName)
    {
        return string.IsNullOrEmpty(deviceName) ? string.Empty : deviceName.Replace("\"", "\\\"");
    }

#endif

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
    private string BuildFFmpegArguments(RecorderDisplayInfo target, WindowsAudioDeviceInfo resolvedWindowsAudioDevice)
#else
    private string BuildFFmpegArguments(RecorderDisplayInfo target)
#endif
    {
        string realOutputDirectory = string.IsNullOrWhiteSpace(outputDirectory)
            ? Application.streamingAssetsPath
            : outputDirectory;

        Directory.CreateDirectory(realOutputDirectory);

        string outputDir = Path.Combine(realOutputDirectory, "RecorderVideo");
        if (!Directory.Exists(outputDir))
        {
            Directory.CreateDirectory(outputDir);
        }

        currentOutputFilePath = Path.Combine(outputDir, $"{outputFilePrefix}_{DateTime.Now:yyyyMMdd_HHmmss}.mp4");

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        bool useSystemAudio = audioMode == RecorderAudioMode.SystemAudio &&
                              resolvedWindowsAudioDevice != null;

        if (useSystemAudio)
        {
            string audioInputName = GetBestDShowAudioInputName(resolvedWindowsAudioDevice);

            if (string.IsNullOrWhiteSpace(audioInputName))
            {
                Debug.LogError("Windows 系统音频设备名为空，无法构建音频输入参数。");
                return string.Empty;
            }

            string escapedAudioInputName = EscapeForDShowDevice(audioInputName);

            Debug.Log($"Windows 系统音频录制设备已确定：display=[{resolvedWindowsAudioDevice.displayName}] alt=[{resolvedWindowsAudioDevice.alternativeName}]");
            Debug.Log($"最终传给 ffmpeg 的音频输入名：{audioInputName}");

            return $"-f gdigrab " +
                   $"-framerate 30 " +
                   $"-offset_x {target.offsetX} " +
                   $"-offset_y {target.offsetY} " +
                   $"-video_size {target.width}x{target.height} " +
                   $"-i desktop " +
                   $"-thread_queue_size 512 " +
                   $"-f dshow " +
                   $"-i audio=\"{escapedAudioInputName}\" " +
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
        bool useSystemAudio = audioMode == RecorderAudioMode.SystemAudio &&
                              !string.IsNullOrWhiteSpace(linuxSystemAudioSourceName);

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

    private void OnDestroy()
    {
        processRunner?.Dispose();
        processRunner = null;
    }
}