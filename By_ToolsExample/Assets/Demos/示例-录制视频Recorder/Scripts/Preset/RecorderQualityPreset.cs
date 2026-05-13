//=====================================================
// 文件名称: RecorderQualityPreset
// 创 建 者: 王柏雁
// 创建日期: 2026-5-6
// 描    述: 录制质量预设
//=====================================================

using UnityEngine;

public class RecorderQualityPreset : MonoBehaviour
{
    // Linux
    // 1.低
    // 2.中
    // 3.高

    // Windows
    // 1.低
    // 2.中
    // 3.高
}

public class RecorderQualityPresetData
{
    [Header("配置名称")] public string presetName;
    [Header("运行时平台")] public string platform;
    [Header("输出文件格式")] public bool outputAsWebM = true;
    [Header("输出文件前缀")] public string outputFilePrefix = "recording";

    #region 音频相关参数

    [Header("音频采集模式")] public int audioMode;
    [Header("音频编码器")] public string audioCodec = "aac";
    [Header("音频码率")] public string audioBitrate = "128k";
    [Header("音频采样率")] public int audioSampleRate = 48000;
    [Header("音频声道数")] public int audioChannels = 2;
    [Header("录制帧率")] public int captureFrameRate = 30;

    #endregion


    #region 视频相关参数

    [Header("视频输出缩放比例")] public float outputScale = 1f;
    [Header("视频画质档位")] public int videoCrf = 23;
    [Header("视频像素格式")] public string pixelFormat = "yuv420p";
    [Header("视频编码器")] public string videoCodec = "libvpx";
    [Header("视频编码预设")] public string videoPreset = "ultrafast";
    [Header("视频码率")] public string webmVideoBitrate = "3M";
    [Header("视频实时编码模式")] public string webmDeadline = "realtime";
    [Header("视频WebM CPU使用等级")] public int webmCpuUsed = 8;

    #endregion


    #region 录制参数相关

    [Header("ffmpeg 进程运行器")] private FFmpegProcessRunner _processRunner;
    [Header("ffmpeg 可执行文件路径")] private string _ffmpegExecutablePath;
    [Header("停止录制等待 ffmpeg 退出超时（毫秒）")] public int stopVideoTimeoutMs = 15000;
    [Header("等待临时文件释放超时（毫秒）")] public int waitTempFileReadyTimeoutMs = 8000;
    [Header("后台合并音视频等待超时（毫秒），<=0 表示不限时")] public int mergeTimeoutMs;
    [Header("是否在后台合并完成后删除临时文件")] public bool deleteTempFilesAfterMerge = true;

    #endregion
}