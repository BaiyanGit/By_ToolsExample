using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class RecorderParamsConfig
{
    [Header("配置名称")] public string configName;
    [Header("运行平台")] public string platform;
    [Header("是否默认配置")] public bool isDefault;
    [Header("文件名")] public string fileName;
    [Header("显示器")] public int displayIndex;
    [Header("输出文件前缀")] public string outputFilePrefix;
    [Header("输出WebM")] public bool outputAsWebm;
    [Header("FFmpeg可执行文件路径")] public string ffmpegExecutablePath;
    [Header("音频采集模式")] public int audioMode;
    [Header("音频编码器")] public string audioCodec;
    [Header("WebM音频编码器")] public string webmAudioCodec;
    [Header("音频码率")] public string audioBitrate;
    [Header("音频采样率")] public int audioSampleRate;
    [Header("音频声道数")] public int audioChannels;
    [Header("视频录制帧率")] public int captureFrameRate;
    [Header("视频输出缩放比例")] public float outputScale;
    [Header("视频画质档位")] public int videoCrf;
    [Header("视频像素格式")] public string pixelFormat;
    [Header("视频编码器")] public string videoCodec;
    [Header("视频编码预设")] public string videoPreset;
    [Header("WebM视频编码器")] public string webmVideoCodec;
    [Header("视频码率")] public string webmVideoBitrate;
    [Header("视频实时编码模式")] public string webmDeadline;
    [Header("视频CPU使用等级")] public int webmCpuUsed;
    [Header("停止录制等待ffmpeg退出超时")] public int stopVideoTimeoutMs;
    [Header("等待临时文件释放超时")] public int waitTempFileReadyTimeoutMs;
    [Header("后台合并音视频等待超时")] public int mergeTimeoutMs;
    [Header("后台合并完成后删除临时文件")] public bool deleteTempFilesAfterMerge;

    /// <summary>
    /// 功能：执行 Clone 相关逻辑。
    /// </summary>
    public RecorderParamsConfig Clone()
    {
        return JsonUtility.FromJson<RecorderParamsConfig>(JsonUtility.ToJson(this));
    }
}

[Serializable]
public class RecorderOptionDescriptionTable
{
    [Header("选项说明组")] public List<RecorderOptionDescriptionGroup> groups = new();
}

[Serializable]
public class RecorderOptionDescriptionGroup
{
    [Header("配置键")] public string key;
    [Header("参数名称")] public string settingName;
    [Header("选项说明")] public List<RecorderOptionDescriptionItem> options = new();
}

[Serializable]
public class RecorderOptionDescriptionItem
{
    [Header("选项")] public string option;
    [Header("说明")] public string description;
}