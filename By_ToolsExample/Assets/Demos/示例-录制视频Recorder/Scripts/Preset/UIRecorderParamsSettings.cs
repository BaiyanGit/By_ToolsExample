using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//=====================================================
// 文件名称: RecorderParamsSettings
// 创 建 者: wangbaiyan
// 创建日期: 2026-5-7
// 描    述: 录像参数设置脚本
//=====================================================


public class UIRecorderParamsSettings : MonoBehaviour
{
    [Header("配置文件")] public Dropdown drConfig;
    [Header("显示器")] public Dropdown drDisplay;
    [Header("运行平台")] public Dropdown drPlatform;
    [Header("视频格式")] public Dropdown drVideoFormat;
    [Header("视频文件前缀")] public InputField ifVideoPrefix;

    // Tips: 音频
    [Header("音频采集模式")] public Dropdown drAudioMode;
    [Header("音频编码器")] public Dropdown drAudioCoder;
    [Header("音频码率")] public Dropdown drAudioBitrate;
    [Header("音频采样率")] public Dropdown drAudioSampleRate;
    [Header("音频声道数")] public Dropdown drAudioChannel;

    // Tips: 视频
    [Header("视频录制帧率")] public Dropdown videoCaptureFrameRate;
    [Header("视频输出缩放比例")] public Dropdown videoOutputScale;
    [Header("视频画质档位")] public Dropdown videoCrf;
    [Header("视频像素格式")] public Dropdown videoPixelFormat;
    [Header("视频编码器")] public Dropdown videoCodec;
    [Header("视频编码预设")] public Dropdown videoPreset;
    [Header("视频码率")] public Dropdown webmVideoBitrate;
    [Header("视频实时编码模式")] public Dropdown webmVideoDeadlineMode;
    [Header("视频CPU使用等级")] public Dropdown webmVideoCpuUsed;
    [Header("视频停止录制等待ffmpeg退出超时")] public Dropdown stopVideoTimeoutMs;
    [Header("视频等待临时文件释放超时")] public Dropdown waitTempFileReadyTimeoutMs;
    [Header("视频后台合并音视频等待超时")] public Dropdown mergeTimeoutMs;
    [Header("视频是否在后台合并完成后删除临时文件")] public Dropdown deleteTempFilesAfterMerge;

    [Header("另存为")] public Button btnSaveAs;
    [Header("保存")] public Button btnSave;

    /// <summary>
    /// 视频文件前缀
    /// </summary>
    private string _videoPrefix;

    private void Start()
    {
        InitUI();
    }

    private void InitUI()
    {
        // Tips：显示器
        var displays = RecorderDisplayProvider.GetDisplays();
        if (displays == null || displays.Count == 0)
        {
            Debug.LogWarning("没有找到可用的显示器");
            return;
        }

        drDisplay.ClearOptions();

        var displayOptions = new List<string>();
        foreach (var display in displays)
        {
            displayOptions.Add(display.ToOptionText());
        }

        drDisplay.AddOptions(displayOptions);


        // Tips: 平台
        var platforms       = Application.platform.ToString();
        var platformOptions = new List<string> { platforms };
        drPlatform.ClearOptions();
        drPlatform.AddOptions(platformOptions);

        // Tips: 视频格式
        var options = new List<string> { "mp4", "webm" };
        drVideoFormat.ClearOptions();
        drVideoFormat.AddOptions(options);

        // Tips: 视频文件前缀
        // TODO: 应该读取配置，若配置没有，则使用默认值
        if (string.IsNullOrEmpty(ifVideoPrefix.text))
        {
            ifVideoPrefix.text = $"{Application.productName}_";
        }

        ifVideoPrefix.onValueChanged.AddListener(SetDefaultVideoPrefix);
    }

    private void SetDefaultVideoPrefix(string value)
    {
        _videoPrefix = value;
    }
}