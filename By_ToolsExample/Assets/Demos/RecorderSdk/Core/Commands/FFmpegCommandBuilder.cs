//=====================================================
// 文件名称: FFmpegCommandBuilder
// 创 建 者: wangbaiyan
// 创建日期: 2026-05-23
// 描    述: 负责按平台、输出方式和配置参数构建 FFmpeg 命令，不负责启动进程和修改录制状态。
//=====================================================

using System.Collections.Generic;
using Demos.示例_录制视频Recorder.Scripts.UISettings;
using UnityEngine;

namespace Demos.示例_录制视频Recorder.Scripts.Core
{
    /// <summary>
    /// FFmpeg 命令构建器。
    /// </summary>
    public static class FFmpegCommandBuilder
    {
        /// <summary>
        /// 构建当前平台的本地录制命令。
        /// </summary>
        public static FFmpegCommand BuildLocalCommand(string executablePath, RecorderParamsConfig config, RecorderDisplayInfo target, string outputPath, string resolvedLinuxAudioSource = "")
        {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            return BuildWindowsLocalCommand(executablePath, config, target, outputPath);
#elif UNITY_STANDALONE_LINUX || UNITY_EDITOR_LINUX
            return BuildLinuxLocalCommand(executablePath, config, target, outputPath, resolvedLinuxAudioSource);
#else
            return EmptyCommand(executablePath);
#endif
        }

        /// <summary>
        /// 构建当前平台的视频推流命令。
        /// </summary>
        public static FFmpegCommand BuildStreamCommand(string executablePath, RecorderParamsConfig config, RecorderDisplayInfo target, string streamUrl, string resolvedLinuxAudioSource = "")
        {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            return BuildWindowsStreamCommand(executablePath, config, target, streamUrl);
#elif UNITY_STANDALONE_LINUX || UNITY_EDITOR_LINUX
            return BuildLinuxStreamCommand(executablePath, config, target, streamUrl, resolvedLinuxAudioSource);
#else
            return EmptyCommand(executablePath);
#endif
        }

        /// <summary>
        /// 构建 Windows 本地录制命令。
        /// </summary>
        public static FFmpegCommand BuildWindowsLocalCommand(string executablePath, RecorderParamsConfig config, RecorderDisplayInfo target, string outputPath)
        {
            if (config == null) return EmptyCommand(executablePath);
            string scaleArgs = BuildScaleArgs(config, target);
            string arguments = config.outputAsWebm
                                   ? $"-f gdigrab -framerate {config.captureFrameRate} -offset_x {target.offsetX} -offset_y {target.offsetY} -video_size {target.width}x{target.height} -i desktop -y -c:v {config.webmVideoCodec} -b:v {config.webmVideoBitrate} -deadline {config.webmDeadline} -cpu-used {config.webmCpuUsed} -pix_fmt yuv420p {scaleArgs}\"{outputPath}\""
                                   : $"-f gdigrab -framerate {config.captureFrameRate} -offset_x {target.offsetX} -offset_y {target.offsetY} -video_size {target.width}x{target.height} -i desktop -y -c:v {config.videoCodec} -preset {config.videoPreset} -crf {config.videoCrf} -pix_fmt {config.pixelFormat} {scaleArgs}\"{outputPath}\"";
            return CreateCommand(executablePath, arguments);
        }

        /// <summary>
        /// 构建 Windows RTMP/RTMPS 推流命令。
        /// </summary>
        public static FFmpegCommand BuildWindowsStreamCommand(string executablePath, RecorderParamsConfig config, RecorderDisplayInfo target, string streamUrl)
        {
            if (config == null) return EmptyCommand(executablePath);
            int streamFrameRate = GetSafeStreamFrameRate(config);
            bool useSystemAudio = config.streamIncludeAudio && config.audioMode == 1;
            string audioInputArgs = config.streamIncludeAudio && config.audioMode == 1
                                        ? "-thread_queue_size 512 -f wasapi -i default -map 0:v:0 -map 1:a:0 "
                                        : "-map 0:v:0 -an ";
            string audioEncodeArgs = useSystemAudio
                                         ? $"-c:a aac -b:a {config.audioBitrate} -ar {config.audioSampleRate} -ac {config.audioChannels} {BuildAudioFilterArgs(config, true)}"
                                         : string.Empty;
            string arguments = $"-thread_queue_size 512 -rtbufsize 256M -f gdigrab -framerate {streamFrameRate} -offset_x {target.offsetX} -offset_y {target.offsetY} -video_size {target.width}x{target.height} -i desktop " +
                               audioInputArgs +
                               BuildStreamVideoArgs(config, target, streamFrameRate) +
                               audioEncodeArgs +
                               $"-f flv \"{streamUrl}\"";
            return CreateCommand(executablePath, arguments);
        }

        /// <summary>
        /// 构建 Linux 本地录制命令。
        /// </summary>
        public static FFmpegCommand BuildLinuxLocalCommand(string executablePath, RecorderParamsConfig config, RecorderDisplayInfo target, string outputPath, string resolvedLinuxAudioSource)
        {
            if (config == null) return EmptyCommand(executablePath);
            string scaleArgs = BuildScaleArgs(config, target);
            bool useSystemAudio = config.audioMode == 1 && !string.IsNullOrWhiteSpace(resolvedLinuxAudioSource);
            string inputArgs = $"-f x11grab -framerate {config.captureFrameRate} -video_size {target.width}x{target.height} -i :0.0+{target.offsetX},{target.offsetY} ";
            string audioInputArgs = useSystemAudio ? $"-thread_queue_size 512 -f pulse -i \"{resolvedLinuxAudioSource}\" -map 0:v:0 -map 1:a:0 " : string.Empty;
            string videoArgs = config.outputAsWebm
                                   ? $"-y -c:v {config.webmVideoCodec} -b:v {config.webmVideoBitrate} -deadline {config.webmDeadline} -cpu-used {config.webmCpuUsed} -pix_fmt yuv420p {scaleArgs}"
                                   : $"-y -c:v {config.videoCodec} -preset {config.videoPreset} -crf {config.videoCrf} -pix_fmt {config.pixelFormat} {scaleArgs}";
            string audioArgs = useSystemAudio
                                   ? $"-c:a {(config.outputAsWebm ? config.webmAudioCodec : config.audioCodec)} -b:a {config.audioBitrate} -ar {config.audioSampleRate} -ac {config.audioChannels} {BuildAudioFilterArgs(config, true)}"
                                   : string.Empty;
            return CreateCommand(executablePath, inputArgs + audioInputArgs + videoArgs + audioArgs + $"\"{outputPath}\"");
        }

        /// <summary>
        /// 构建 Linux RTMP/RTMPS 推流命令。
        /// </summary>
        public static FFmpegCommand BuildLinuxStreamCommand(string executablePath, RecorderParamsConfig config, RecorderDisplayInfo target, string streamUrl, string resolvedLinuxAudioSource)
        {
            if (config == null) return EmptyCommand(executablePath);
            int streamFrameRate = GetSafeStreamFrameRate(config);
            bool useSystemAudio = config.audioMode == 1 && config.streamIncludeAudio && !string.IsNullOrWhiteSpace(resolvedLinuxAudioSource);
            string audioInputArgs = useSystemAudio
                                        ? $"-thread_queue_size 512 -f pulse -i \"{resolvedLinuxAudioSource}\" -map 0:v:0 -map 1:a:0 "
                                        : "-map 0:v:0 -an ";
            string audioEncodeArgs = useSystemAudio
                                         ? $"-c:a aac -b:a {config.audioBitrate} -ar {config.audioSampleRate} -ac {config.audioChannels} {BuildAudioFilterArgs(config, true)}"
                                         : string.Empty;
            string arguments = $"-f x11grab -thread_queue_size 512 -framerate {streamFrameRate} -video_size {target.width}x{target.height} -i :0.0+{target.offsetX},{target.offsetY} " +
                               audioInputArgs +
                               BuildStreamVideoArgs(config, target, streamFrameRate) +
                               audioEncodeArgs +
                               $"-f flv \"{streamUrl}\"";
            return CreateCommand(executablePath, arguments);
        }

        /// <summary>
        /// 构建音视频合并命令。
        /// </summary>
        public static FFmpegCommand BuildMergeCommand(string executablePath, RecorderParamsConfig config, string videoPath, string audioPath, string outputPath)
        {
            if (config == null) return EmptyCommand(executablePath);
            string targetAudioCodec = config.outputAsWebm ? config.webmAudioCodec : config.audioCodec;
            string arguments = $"-y -loglevel error -nostats -i \"{videoPath}\" -i \"{audioPath}\" -map 0:v:0 -map 1:a:0 -c:v copy -c:a {targetAudioCodec} -b:a {config.audioBitrate} -ar {config.audioSampleRate} -ac {config.audioChannels} {BuildAudioFilterArgs(config, true)}-shortest \"{outputPath}\"";
            return CreateCommand(executablePath, arguments);
        }

        /// <summary>
        /// 构建音频滤镜参数，当前用于音量增益和限幅器。
        /// </summary>
        public static string BuildAudioFilterArgs(RecorderParamsConfig config, bool hasAudio, string existingFilter = "")
        {
            string filter = BuildAudioFilter(config, hasAudio, existingFilter);
            return string.IsNullOrWhiteSpace(filter) ? string.Empty : $"-af \"{filter}\" ";
        }

        /// <summary>
        /// 构建音频滤镜表达式，不包含 -af 前缀；已有滤镜会合并成同一个 -af。
        /// </summary>
        public static string BuildAudioFilter(RecorderParamsConfig config, bool hasAudio, string existingFilter = "")
        {
            if (config == null || !hasAudio) return string.Empty;
            string filter = string.IsNullOrWhiteSpace(existingFilter) ? string.Empty : existingFilter.Trim();
            if (!config.enableAudioGain) return filter;

            float gain = Mathf.Clamp(config.audioGainDb, -20f, 20f);
            string gainText = gain.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture);
            string gainFilter = $"volume={gainText}dB";
            if (config.audioLimiterEnabled) gainFilter += ",alimiter=limit=0.95";
            filter = string.IsNullOrWhiteSpace(filter) ? gainFilter : $"{filter},{gainFilter}";
            return filter;
        }

        /// <summary>
        /// 创建命令对象。
        /// </summary>
        private static FFmpegCommand CreateCommand(string executablePath, string arguments)
        {
            return new FFmpegCommand
            {
                executablePath = executablePath,
                arguments = new List<string> { arguments },
                rawCommandLine = arguments
            };
        }

        /// <summary>
        /// 创建空命令。
        /// </summary>
        private static FFmpegCommand EmptyCommand(string executablePath)
        {
            return CreateCommand(executablePath, string.Empty);
        }

        /// <summary>
        /// 构建缩放参数。
        /// </summary>
        private static string BuildScaleArgs(RecorderParamsConfig config, RecorderDisplayInfo target)
        {
            int scaledWidth = MakeEven(Mathf.RoundToInt(target.width * Mathf.Clamp(config.outputScale, 0.25f, 1f)));
            int scaledHeight = MakeEven(Mathf.RoundToInt(target.height * Mathf.Clamp(config.outputScale, 0.25f, 1f)));
            return scaledWidth != target.width || scaledHeight != target.height ? $"-vf scale={scaledWidth}:{scaledHeight} " : string.Empty;
        }

        /// <summary>
        /// 构建推流视频编码公共参数。
        /// </summary>
        private static string BuildStreamVideoArgs(RecorderParamsConfig config, RecorderDisplayInfo target, int streamFrameRate)
        {
            string streamBitrate = GetSafeStreamBitrate(config);
            string streamPreset = GetSafeStreamPreset(config);
            int streamGop = GetSafeStreamGop(config, streamFrameRate);
            string streamBufferSize = GetSafeStreamBufferSize(config, streamBitrate);
            string lowLatencyArgs = config.streamLowLatency ? "-tune zerolatency " : string.Empty;
            return $"-c:v libx264 -preset {streamPreset} {lowLatencyArgs}-b:v {streamBitrate} -maxrate {streamBitrate} -bufsize {streamBufferSize} -g {streamGop} -keyint_min {streamFrameRate} -sc_threshold 0 -threads 0 -pix_fmt yuv420p {BuildScaleArgs(config, target)}-flush_packets 1 ";
        }

        /// <summary>
        /// 把分辨率修正为偶数。
        /// </summary>
        private static int MakeEven(int value)
        {
            if (value < 2) value = 2;
            return value % 2 == 0 ? value : value - 1;
        }

        /// <summary>
        /// 获取推流安全帧率。
        /// </summary>
        private static int GetSafeStreamFrameRate(RecorderParamsConfig config)
        {
            int frameRate = config != null && config.captureFrameRate > 0 ? config.captureFrameRate : 25;
            return Mathf.Clamp(frameRate, 10, 60);
        }

        /// <summary>
        /// 获取推流安全码率。
        /// </summary>
        private static string GetSafeStreamBitrate(RecorderParamsConfig config)
        {
            if (config == null) return "3M";
            return string.IsNullOrWhiteSpace(config.streamVideoBitrate) ? "3M" : config.streamVideoBitrate.Trim();
        }

        /// <summary>
        /// 获取推流编码预设。
        /// </summary>
        private static string GetSafeStreamPreset(RecorderParamsConfig config)
        {
            string preset = config == null || string.IsNullOrWhiteSpace(config.videoPreset) ? "veryfast" : config.videoPreset.Trim();
            return preset == "ultrafast" || preset == "veryfast" || preset == "faster" ? preset : "veryfast";
        }

        /// <summary>
        /// 获取推流 GOP。
        /// </summary>
        private static int GetSafeStreamGop(RecorderParamsConfig config, int streamFrameRate)
        {
            int gop = config != null && config.streamGop > 0 ? config.streamGop : streamFrameRate * 2;
            return Mathf.Clamp(gop, streamFrameRate, streamFrameRate * 4);
        }

        /// <summary>
        /// 获取推流缓冲区大小。
        /// </summary>
        private static string GetSafeStreamBufferSize(RecorderParamsConfig config, string streamBitrate)
        {
            return config == null || string.IsNullOrWhiteSpace(config.streamBufferSize) ? GetDoubleBitrate(streamBitrate) : config.streamBufferSize.Trim();
        }

        /// <summary>
        /// 计算推流缓冲区码率。
        /// </summary>
        private static string GetDoubleBitrate(string bitrate)
        {
            if (string.IsNullOrWhiteSpace(bitrate)) return "6M";
            string value = bitrate.Trim();
            char suffix = value[value.Length - 1];
            string numberPart = char.IsLetter(suffix) ? value.Substring(0, value.Length - 1) : value;
            if (!float.TryParse(numberPart, out float number)) return "6M";
            return char.IsLetter(suffix) ? $"{number * 2:0.#}{suffix}" : $"{number * 2:0.#}";
        }
    }
}
