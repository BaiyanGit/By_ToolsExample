using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 录屏清晰度下拉框控制器。
/// 挂到任意 UI 对象上，指定 Dropdown 和 CrossPlatformScreenRecorder 即可。
/// </summary>
public class RecorderQualityDropdownUI : MonoBehaviour
{
    [Header("录屏核心控制器")] public CrossPlatformScreenRecorder recorder;

    [Header("清晰度下拉框")] public Dropdown qualityDropdown;

    [Header("启动时自动填充选项")] public bool populateOptionsOnStart = true;

    [Header("启动时自动应用默认档位")] public bool applyDefaultPresetOnStart = true;

    [Header("默认档位索引")] public int defaultPresetIndex = 1;

    /// <summary>
    /// 清晰度配置。
    /// </summary>
    [Serializable]
    public class QualityPreset
    {
        [Header("显示名称")] public string displayName = "均衡";

        [Header("录制帧率")] public int captureFrameRate = 25;

        [Header("输出缩放比例")] [Range(0.25f, 1f)] public float outputScale = 0.75f;

        [Header("CRF，越小越清晰")] [Range(16, 35)] public int videoCrf = 23;

        [Header("编码预设")] public string videoPreset = "ultrafast";

        [Header("音频码率")] public string audioBitrate = "128k";
    }

    [Header("清晰度预设列表")] public List<QualityPreset> presets = new List<QualityPreset>()
    {
        new QualityPreset()
        {
            displayName      = "高质量",
            captureFrameRate = 30,
            outputScale      = 1f,
            videoCrf         = 20,
            videoPreset      = "veryfast",
            audioBitrate     = "192k"
        },
        new QualityPreset()
        {
            displayName      = "均衡",
            captureFrameRate = 25,
            outputScale      = 0.75f,
            videoCrf         = 23,
            videoPreset      = "ultrafast",
            audioBitrate     = "128k"
        },
        new QualityPreset()
        {
            displayName      = "长时间录制",
            captureFrameRate = 20,
            outputScale      = 0.5f,
            videoCrf         = 26,
            videoPreset      = "ultrafast",
            audioBitrate     = "128k"
        }
    };

    private void Start()
    {
        if (recorder == null)
        {
            Debug.LogError("RecorderQualityDropdownUI 未指定 CrossPlatformScreenRecorder。");
            return;
        }

        if (qualityDropdown == null)
        {
            Debug.LogError("RecorderQualityDropdownUI 未指定 Dropdown。");
            return;
        }

        BindDropdown();

        if (populateOptionsOnStart)
        {
            RefreshDropdownOptions();
        }

        if (presets.Count == 0)
        {
            Debug.LogWarning("RecorderQualityDropdownUI 当前没有可用的清晰度预设。");
            return;
        }

        defaultPresetIndex = Mathf.Clamp(defaultPresetIndex, 0, presets.Count - 1);

        if (applyDefaultPresetOnStart)
        {
            qualityDropdown.value = defaultPresetIndex;
            ApplyPreset(defaultPresetIndex);
        }
    }

    /// <summary>
    /// 绑定下拉框事件。
    /// </summary>
    private void BindDropdown()
    {
        qualityDropdown.onValueChanged.RemoveListener(OnQualityDropdownChanged);
        qualityDropdown.onValueChanged.AddListener(OnQualityDropdownChanged);
    }

    /// <summary>
    /// 刷新下拉框显示项。
    /// </summary>
    public void RefreshDropdownOptions()
    {
        if (qualityDropdown == null)
        {
            return;
        }

        qualityDropdown.ClearOptions();

        List<string> options = new List<string>();
        foreach (QualityPreset preset in presets)
        {
            options.Add(preset.displayName);
        }

        qualityDropdown.AddOptions(options);
    }

    /// <summary>
    /// 下拉框选项变化时应用预设。
    /// </summary>
    /// <param name="index">选中索引。</param>
    private void OnQualityDropdownChanged(int index)
    {
        ApplyPreset(index);
    }

    /// <summary>
    /// 应用指定索引的清晰度预设。
    /// </summary>
    /// <param name="index">预设索引。</param>
    public void ApplyPreset(int index)
    {
        if (recorder == null)
        {
            Debug.LogError("CrossPlatformScreenRecorder 为空，无法应用清晰度预设。");
            return;
        }

        if (index < 0 || index >= presets.Count)
        {
            Debug.LogWarning("清晰度预设索引越界: " + index);
            return;
        }

        QualityPreset preset = presets[index];

        recorder.captureFrameRate = Mathf.Clamp(preset.captureFrameRate, 10, 60);
        recorder.outputScale      = Mathf.Clamp(preset.outputScale, 0.25f, 1f);
        recorder.videoCrf         = Mathf.Clamp(preset.videoCrf, 16, 35);
        recorder.videoPreset      = string.IsNullOrWhiteSpace(preset.videoPreset) ? "ultrafast" : preset.videoPreset;
        recorder.audioBitrate     = string.IsNullOrWhiteSpace(preset.audioBitrate) ? "128k" : preset.audioBitrate;

        Debug.Log(
            $"已应用清晰度预设: {preset.displayName} | " +
            $"FPS={recorder.captureFrameRate}, " +
            $"Scale={recorder.outputScale}, " +
            $"CRF={recorder.videoCrf}, " +
            $"Preset={recorder.videoPreset}, " +
            $"AudioBitrate={recorder.audioBitrate}");
    }

    /// <summary>
    /// 供按钮直接调用：切到高质量。
    /// </summary>
    public void UseHighQuality()
    {
        ApplyPresetByName("高质量");
    }

    /// <summary>
    /// 供按钮直接调用：切到均衡。
    /// </summary>
    public void UseBalanced()
    {
        ApplyPresetByName("均衡");
    }

    /// <summary>
    /// 供按钮直接调用：切到长时间录制。
    /// </summary>
    public void UseLongRecording()
    {
        ApplyPresetByName("长时间录制");
    }

    /// <summary>
    /// 按名称应用预设。
    /// </summary>
    /// <param name="presetName">预设名称。</param>
    public void ApplyPresetByName(string presetName)
    {
        if (string.IsNullOrWhiteSpace(presetName))
        {
            return;
        }

        for (int i = 0; i < presets.Count; i++)
        {
            if (string.Equals(presets[i].displayName, presetName, StringComparison.OrdinalIgnoreCase))
            {
                if (qualityDropdown != null)
                {
                    qualityDropdown.value = i;
                }

                ApplyPreset(i);
                return;
            }
        }

        Debug.LogWarning("未找到清晰度预设: " + presetName);
    }

    private void OnDestroy()
    {
        if (qualityDropdown != null)
        {
            qualityDropdown.onValueChanged.RemoveListener(OnQualityDropdownChanged);
        }
    }
}