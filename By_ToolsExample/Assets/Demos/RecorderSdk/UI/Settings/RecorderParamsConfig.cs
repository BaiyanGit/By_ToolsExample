namespace Demos.示例_录制视频Recorder.Scripts.UISettings
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    [Serializable]
    public class RecorderParamsConfig
    {
        [Header("配置版本")] public int schemaVersion = 0;
        [Header("配置ID")] public string configId;
        [Header("显示名称")] public string displayName;
        [Header("配置描述")] public string description;
        [Header("运行平台")] public string platform;
        [Header("使用方式")] public int useMode; // 0: 存储本地，1: 视频推流
        [Header("视频文件保存路径")] public string videoSaveDirectory;
        [Header("输出文件前缀")] public string outputFilePrefix;
        [Header("输出WebM")] public bool outputAsWebm;
        [Header("显示器")] public int displayIndex;
        [Header("显示器名称")] public string captureDisplayName;
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
        [Header("音频采集模式")] public int audioMode; // 0: 静音, 1: 系统声音
        [Header("音频编码器")] public string audioCodec;
        [Header("WebM音频编码器")] public string webmAudioCodec;
        [Header("音频码率")] public string audioBitrate;
        [Header("音频采样率")] public int audioSampleRate;
        [Header("音频声道数")] public int audioChannels;
        [Header("启用录制音量增强")] public bool enableAudioGain;
        [Header("录制音量增益dB")] public float audioGainDb;
        [Header("启用音频限幅器")] public bool audioLimiterEnabled = true;
        [Header("视频推流地址")] public string streamUrl;
        [Header("推流视频码率")] public string streamVideoBitrate;
        [Header("推流GOP帧间隔")] public int streamGop;
        [Header("推流缓冲区大小")] public string streamBufferSize;
        [Header("推流低延迟模式")] public bool streamLowLatency;
        [Header("推流自动重连")] public bool streamAutoReconnect;
        [Header("推流重连次数")] public int streamReconnectCount;
        [Header("推流重连间隔毫秒")] public int streamReconnectIntervalMs;
        [Header("推流是否包含系统音频")] public bool streamIncludeAudio;
        [Header("自定义FFmpeg可执行文件路径")] public string customFFmpegPath;
        [Header("停止录制等待ffmpeg退出超时")] public int stopVideoTimeoutMs;
        [Header("等待临时文件释放超时")] public int waitTempFileReadyTimeoutMs;
        [Header("后台合并音视频等待超时")] public int mergeTimeoutMs;
        [Header("后台合并完成后删除临时文件")] public bool deleteTempFilesAfterMerge;
        [NonSerialized] public string configName;
        [NonSerialized] public bool isDefault;
        [NonSerialized] public string fileName;

        /// <summary>
        /// 执行 Clone 相关逻辑。
        /// </summary>
        public RecorderParamsConfig Clone()
        {
            var clone = JsonUtility.FromJson<RecorderParamsConfig>(JsonUtility.ToJson(this));
            clone.configName = configName;
            clone.isDefault = isDefault;
            clone.fileName = fileName;
            return clone;
        }
    }

    [Serializable]
    public class RecordConfigReference
    {
        [Header("配置版本")] public int schemaVersion = 0;
        [Header("运行平台")] public string platform;
        [Header("当前配置ID")] public string currentConfigId;
        [NonSerialized] public string sourceType;
        [NonSerialized] public string fileName;
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
}
