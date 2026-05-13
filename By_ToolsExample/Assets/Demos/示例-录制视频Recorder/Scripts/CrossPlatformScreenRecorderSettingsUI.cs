using System;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// 通过 UGUI 控制 CrossPlatformScreenRecorder 参数。
/// 适合在 Inspector 中挂载 UI 元素，并将对应控件绑定到此组件。
/// </summary>
public class CrossPlatformScreenRecorderSettingsUI : MonoBehaviour
{
    [Header("录屏核心控制器")] public CrossPlatformScreenRecorder recorder;

    [Header("视频质量设置")] public InputField captureFrameRateInput;
    public InputField outputScaleInput;
    public InputField videoCrfInput;
    public InputField videoPresetInput;
    public Toggle outputAsWebMToggle;
    public InputField webmVideoCodecInput;
    public InputField webmAudioCodecInput;
    public InputField webmVideoBitrateInput;
    public InputField webmDeadlineInput;
    public Slider webmCpuUsedSlider;
    public Text webmCpuUsedValueText;

    [Header("音频设置")] public Dropdown audioModeDropdown;
    public InputField audioCodecInput;
    public InputField audioBitrateInput;
    public InputField audioSampleRateInput;
    public InputField audioChannelsInput;
    public InputField linuxSystemAudioSourceInput;

    [Header("路径设置")] public InputField outputDirectoryInput;
    public InputField customFFmpegPathInput;
    public Button browseFFmpegPathButton;
    public Text ffmpegHintText;
    public Button applyButton;
    public Button resetFFmpegPathButton;

    private void Start()
    {
        if (recorder == null)
        {
            Debug.LogError("CrossPlatformScreenRecorderSettingsUI: 未指定 recorder。请先在 Inspector 中绑定。");
            return;
        }

        InitializeUI();
        BindEvents();
        RefreshFFmpegHint();
    }

    private void InitializeUI()
    {
        if (captureFrameRateInput != null)
        {
            captureFrameRateInput.text = recorder.captureFrameRate.ToString();
        }

        if (outputScaleInput != null)
        {
            outputScaleInput.text = recorder.outputScale.ToString("0.##");
        }

        if (videoCrfInput != null)
        {
            videoCrfInput.text = recorder.videoCrf.ToString();
        }

        if (videoPresetInput != null)
        {
            videoPresetInput.text = recorder.videoPreset;
        }

        if (outputAsWebMToggle != null)
        {
            outputAsWebMToggle.isOn = recorder.outputAsWebM;
        }

        if (webmVideoCodecInput != null)
        {
            webmVideoCodecInput.text = recorder.webmVideoCodec;
        }

        if (webmAudioCodecInput != null)
        {
            webmAudioCodecInput.text = recorder.webmAudioCodec;
        }

        if (webmVideoBitrateInput != null)
        {
            webmVideoBitrateInput.text = recorder.webmVideoBitrate;
        }

        if (webmDeadlineInput != null)
        {
            webmDeadlineInput.text = recorder.webmDeadline;
        }

        if (webmCpuUsedSlider != null)
        {
            webmCpuUsedSlider.value = recorder.webmCpuUsed;
        }

        if (webmCpuUsedValueText != null)
        {
            webmCpuUsedValueText.text = recorder.webmCpuUsed.ToString();
        }

        if (audioModeDropdown != null)
        {
            audioModeDropdown.ClearOptions();
            audioModeDropdown.AddOptions(new System.Collections.Generic.List<string> { "静音录制", "系统音频" });
            audioModeDropdown.value = recorder.audioMode == RecorderAudioMode.SystemAudio ? 1 : 0;
        }

        if (audioCodecInput != null)
        {
            audioCodecInput.text = recorder.audioCodec;
        }

        if (audioBitrateInput != null)
        {
            audioBitrateInput.text = recorder.audioBitrate;
        }

        if (audioSampleRateInput != null)
        {
            audioSampleRateInput.text = recorder.audioSampleRate.ToString();
        }

        if (audioChannelsInput != null)
        {
            audioChannelsInput.text = recorder.audioChannels.ToString();
        }

        if (linuxSystemAudioSourceInput != null)
        {
            linuxSystemAudioSourceInput.text = recorder.linuxSystemAudioSourceName;
        }

        if (outputDirectoryInput != null)
        {
            outputDirectoryInput.text = recorder.outputDirectory;
        }

        if (customFFmpegPathInput != null)
        {
            customFFmpegPathInput.text = recorder.customFFmpegPath;
        }

        UpdateWebMFieldsState();
    }

    private void BindEvents()
    {
        if (applyButton != null)
        {
            applyButton.onClick.RemoveListener(ApplySettings);
            applyButton.onClick.AddListener(ApplySettings);
        }

        if (resetFFmpegPathButton != null)
        {
            resetFFmpegPathButton.onClick.RemoveListener(ResetFFmpegPath);
            resetFFmpegPathButton.onClick.AddListener(ResetFFmpegPath);
        }

        if (browseFFmpegPathButton != null)
        {
            browseFFmpegPathButton.onClick.RemoveListener(BrowseFFmpegPath);
            browseFFmpegPathButton.onClick.AddListener(BrowseFFmpegPath);
        }

        if (outputAsWebMToggle != null)
        {
            outputAsWebMToggle.onValueChanged.RemoveListener(OnOutputAsWebMToggleChanged);
            outputAsWebMToggle.onValueChanged.AddListener(OnOutputAsWebMToggleChanged);
        }

        if (webmCpuUsedSlider != null)
        {
            webmCpuUsedSlider.onValueChanged.RemoveListener(OnWebmCpuUsedChanged);
            webmCpuUsedSlider.onValueChanged.AddListener(OnWebmCpuUsedChanged);
        }
    }

    private void ApplySettings()
    {
        if (recorder == null)
        {
            return;
        }

        if (captureFrameRateInput != null && int.TryParse(captureFrameRateInput.text, out int frameRate))
        {
            recorder.captureFrameRate = Mathf.Clamp(frameRate, 10, 60);
        }

        if (outputScaleInput != null && float.TryParse(outputScaleInput.text, out float scale))
        {
            recorder.outputScale = Mathf.Clamp(scale, 0.25f, 1f);
        }

        if (videoCrfInput != null && int.TryParse(videoCrfInput.text, out int crf))
        {
            recorder.videoCrf = Mathf.Clamp(crf, 16, 35);
        }

        if (videoPresetInput != null)
        {
            recorder.videoPreset = string.IsNullOrWhiteSpace(videoPresetInput.text) ? recorder.videoPreset : videoPresetInput.text.Trim();
        }

        if (outputAsWebMToggle != null)
        {
            recorder.outputAsWebM = outputAsWebMToggle.isOn;
        }

        if (webmVideoCodecInput != null)
        {
            recorder.webmVideoCodec = string.IsNullOrWhiteSpace(webmVideoCodecInput.text) ? recorder.webmVideoCodec : webmVideoCodecInput.text.Trim();
        }

        if (webmAudioCodecInput != null)
        {
            recorder.webmAudioCodec = string.IsNullOrWhiteSpace(webmAudioCodecInput.text) ? recorder.webmAudioCodec : webmAudioCodecInput.text.Trim();
        }

        if (webmVideoBitrateInput != null)
        {
            recorder.webmVideoBitrate = string.IsNullOrWhiteSpace(webmVideoBitrateInput.text) ? recorder.webmVideoBitrate : webmVideoBitrateInput.text.Trim();
        }

        if (webmDeadlineInput != null)
        {
            recorder.webmDeadline = string.IsNullOrWhiteSpace(webmDeadlineInput.text) ? recorder.webmDeadline : webmDeadlineInput.text.Trim();
        }

        if (webmCpuUsedSlider != null)
        {
            recorder.webmCpuUsed = Mathf.Clamp(Mathf.RoundToInt(webmCpuUsedSlider.value), 0, 8);
        }

        if (audioModeDropdown != null)
        {
            recorder.audioMode = audioModeDropdown.value == 1 ? RecorderAudioMode.SystemAudio : RecorderAudioMode.None;
        }

        if (audioCodecInput != null)
        {
            recorder.audioCodec = string.IsNullOrWhiteSpace(audioCodecInput.text) ? recorder.audioCodec : audioCodecInput.text.Trim();
        }

        if (audioBitrateInput != null)
        {
            recorder.audioBitrate = string.IsNullOrWhiteSpace(audioBitrateInput.text) ? recorder.audioBitrate : audioBitrateInput.text.Trim();
        }

        if (audioSampleRateInput != null && int.TryParse(audioSampleRateInput.text, out int sampleRate))
        {
            recorder.audioSampleRate = Mathf.Max(8000, sampleRate);
        }

        if (audioChannelsInput != null && int.TryParse(audioChannelsInput.text, out int channels))
        {
            recorder.audioChannels = Mathf.Clamp(channels, 1, 8);
        }

        if (linuxSystemAudioSourceInput != null)
        {
            recorder.linuxSystemAudioSourceName = linuxSystemAudioSourceInput.text.Trim();
        }

        if (outputDirectoryInput != null)
        {
            recorder.outputDirectory = outputDirectoryInput.text.Trim();
        }

        if (customFFmpegPathInput != null)
        {
            recorder.customFFmpegPath = customFFmpegPathInput.text.Trim();
        }

        RefreshFFmpegHint();
        Debug.Log("已应用录制设置。");
    }

    private void BrowseFFmpegPath()
    {
#if UNITY_EDITOR
        string extension = Application.platform switch
        {
            RuntimePlatform.WindowsEditor => "exe",
            RuntimePlatform.LinuxEditor   => string.Empty,
            _                             => string.Empty
        };

        string path = EditorUtility.OpenFilePanel("选择 FFmpeg 可执行文件", string.Empty, extension);
        if (!string.IsNullOrWhiteSpace(path))
        {
            if (customFFmpegPathInput != null)
            {
                customFFmpegPathInput.text = path;
            }

            if (recorder != null)
            {
                recorder.customFFmpegPath = path;
            }

            RefreshFFmpegHint();
            Debug.Log("已选择自定义 FFmpeg 路径: " + path);
        }
#else
        Debug.LogWarning("浏览 FFmpeg 路径仅在编辑器中支持。运行时请直接输入路径。");
#endif
    }

    private void ResetFFmpegPath()
    {
        if (customFFmpegPathInput != null)
        {
            customFFmpegPathInput.text = string.Empty;
        }

        if (recorder != null)
        {
            recorder.customFFmpegPath = string.Empty;
        }

        RefreshFFmpegHint();
        Debug.Log("已重置自定义 FFmpeg 路径，恢复默认平台路径。");
    }

    private void RefreshFFmpegHint()
    {
        if (ffmpegHintText == null || recorder == null)
        {
            return;
        }

        string defaultPath = recorder.GetPlatformDefaultFFmpegPath();
        ffmpegHintText.text = $"默认 FFmpeg 路径: {defaultPath}\n留空则使用默认路径，或在下方输入自定义路径";
    }

    private void OnOutputAsWebMToggleChanged(bool isOn)
    {
        UpdateWebMFieldsState();
    }

    private void OnWebmCpuUsedChanged(float value)
    {
        if (webmCpuUsedValueText != null)
        {
            webmCpuUsedValueText.text = Mathf.RoundToInt(value).ToString();
        }
    }

    private void UpdateWebMFieldsState()
    {
        bool active = outputAsWebMToggle != null && outputAsWebMToggle.isOn;

        if (webmVideoCodecInput != null)
        {
            webmVideoCodecInput.interactable = active;
        }

        if (webmAudioCodecInput != null)
        {
            webmAudioCodecInput.interactable = active;
        }

        if (webmVideoBitrateInput != null)
        {
            webmVideoBitrateInput.interactable = active;
        }

        if (webmDeadlineInput != null)
        {
            webmDeadlineInput.interactable = active;
        }

        if (webmCpuUsedSlider != null)
        {
            webmCpuUsedSlider.interactable = active;
        }
    }

    private void OnDestroy()
    {
        if (applyButton != null)
        {
            applyButton.onClick.RemoveListener(ApplySettings);
        }

        if (resetFFmpegPathButton != null)
        {
            resetFFmpegPathButton.onClick.RemoveListener(ResetFFmpegPath);
        }

        if (outputAsWebMToggle != null)
        {
            outputAsWebMToggle.onValueChanged.RemoveListener(OnOutputAsWebMToggleChanged);
        }

        if (browseFFmpegPathButton != null)
        {
            browseFFmpegPathButton.onClick.RemoveListener(BrowseFFmpegPath);
        }

        if (webmCpuUsedSlider != null)
        {
            webmCpuUsedSlider.onValueChanged.RemoveListener(OnWebmCpuUsedChanged);
        }
    }
}