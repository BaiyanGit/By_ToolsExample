//=====================================================
// 文件名称: RecorderConfigValidator
// 创 建 者: wangbaiyan
// 创建日期: 2026-05-23
// 描    述: 统一校验录制配置中的 FFmpeg、输出目录、推流地址、显示器和关键编码参数。
//=====================================================

using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Demos.示例_录制视频Recorder.Scripts.UISettings;
using UnityEngine;

namespace Demos.示例_录制视频Recorder.Scripts.Core
{
    /// <summary>
    /// 录制配置校验器。
    /// </summary>
    public static class RecorderConfigValidator
    {
        private static readonly Dictionary<string, bool> _ffmpegWasapiSupportCache = new();
        private static readonly Dictionary<string, bool> _ffmpegFilterSupportCache = new();

        /// <summary>
        /// 校验开始录制前所需配置。
        /// </summary>
        public static RecorderValidationResult ValidateForStart(RecorderParamsConfig config, string ffmpegPath, IReadOnlyList<RecorderDisplayInfo> displays, string outputDirectory = "")
        {
            var result = new RecorderValidationResult();
            if (config == null)
            {
                result.AddError("当前录制配置为空，请先选择并使用一个录制配置。", RecorderErrorCode.ConfigNull);
                return result;
            }

            if (string.IsNullOrWhiteSpace(ffmpegPath) || !File.Exists(ffmpegPath)) result.AddError("FFmpeg 可执行文件不存在: " + ffmpegPath, RecorderErrorCode.FFmpegPathMissing);
            string realOutputDirectory = string.IsNullOrWhiteSpace(outputDirectory) ? RecorderPathService.GetDefaultVideoDirectory() : outputDirectory;
            if (config.useMode == 0 && string.IsNullOrWhiteSpace(realOutputDirectory)) result.AddError("本地存储模式需要配置视频文件保存路径。");
            if (config.useMode == 0 && !string.IsNullOrWhiteSpace(realOutputDirectory) && !Directory.Exists(realOutputDirectory)) result.AddWarning("视频保存目录不存在，开始录制时会尝试自动创建: " + realOutputDirectory);
            if (config.useMode == 1 && string.IsNullOrWhiteSpace(config.streamUrl)) result.AddError("当前为视频推流模式，但未配置视频推流地址。", RecorderErrorCode.StreamUrlEmpty);
            if (config.useMode == 1 && !IsRtmpUrl(config.streamUrl)) result.AddError("当前仅支持 RTMP/RTMPS 推流地址。", RecorderErrorCode.StreamUrlInvalid);
            if (config.captureFrameRate <= 0) result.AddError("录制帧率必须大于 0。");
            if (config.outputScale <= 0f || config.outputScale > 1f) result.AddError("输出缩放比例必须大于 0 且不超过 1。");
            if (config.audioGainDb < -20f)
            {
                config.audioGainDb = -20f;
                result.AddWarning("录制音量增益小于 -20dB，已自动修正为 -20dB。", RecorderErrorCode.AudioFilterInvalid);
            }

            if (config.audioGainDb > 20f)
            {
                config.audioGainDb = 20f;
                result.AddWarning("录制音量增益大于 20dB，已自动修正为 20dB。", RecorderErrorCode.AudioFilterInvalid);
            }

            bool hasAudioInput = config.audioMode == 1 && (config.useMode == 0 || config.streamIncludeAudio);
            if (config.enableAudioGain && !hasAudioInput)
            {
                result.AddWarning("已启用录制音量增强，但当前配置没有音频输入，本次不会拼接音频滤镜。", RecorderErrorCode.AudioFilterInvalid);
            }

            if (hasAudioInput && config.enableAudioGain && config.audioLimiterEnabled && !string.IsNullOrWhiteSpace(ffmpegPath) && File.Exists(ffmpegPath))
            {
                if (!FFmpegSupportsFilter(ffmpegPath, "alimiter"))
                {
                    config.audioLimiterEnabled = false;
                    result.AddWarning("当前 FFmpeg 不支持 alimiter 音频限幅器，已降级为只使用 volume 音量增强。", RecorderErrorCode.AudioFilterInvalid);
                }
            }

            if (displays == null || displays.Count == 0) result.AddError("当前没有可录制的显示器。", RecorderErrorCode.DisplayNotFound);
            else if (!HasDisplay(config, displays)) result.AddError($"当前录制配置的显示器不存在:{config.displayIndex} —— {config.captureDisplayName}", RecorderErrorCode.DisplayNotFound);
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            if (config.useMode == 1 && config.streamIncludeAudio && config.audioMode == 1 && !string.IsNullOrWhiteSpace(ffmpegPath) && File.Exists(ffmpegPath))
            {
                if (!FFmpegSupportsDevice(ffmpegPath, "wasapi"))
                {
                    result.AddError("当前 FFmpeg 不支持 wasapi 输入，Windows 推流包含系统音频无法启动。请更换支持 wasapi 的 FFmpeg，或关闭 streamIncludeAudio。", RecorderErrorCode.AudioStartFailed);
                }
            }
#endif
            return result;
        }

        /// <summary>
        /// 检测 FFmpeg 是否支持指定输入设备，避免运行命令后才失败。
        /// </summary>
        private static bool FFmpegSupportsDevice(string ffmpegPath, string deviceName)
        {
            string cacheKey = $"{ffmpegPath}|{deviceName}";
            if (_ffmpegWasapiSupportCache.TryGetValue(cacheKey, out bool cached)) return cached;

            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = ffmpegPath,
                    Arguments = "-hide_banner -devices",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                using var process = Process.Start(startInfo);
                if (process == null)
                {
                    _ffmpegWasapiSupportCache[cacheKey] = false;
                    return false;
                }

                string output = process.StandardOutput.ReadToEnd() + process.StandardError.ReadToEnd();
                if (!process.WaitForExit(3000))
                {
                    process.Kill();
                    _ffmpegWasapiSupportCache[cacheKey] = false;
                    return false;
                }

                bool supported = output.IndexOf(deviceName, System.StringComparison.OrdinalIgnoreCase) >= 0;
                _ffmpegWasapiSupportCache[cacheKey] = supported;
                return supported;
            }
            catch
            {
                _ffmpegWasapiSupportCache[cacheKey] = false;
                return false;
            }
        }

        /// <summary>
        /// 检测 FFmpeg 是否支持指定音频滤镜，缺失时允许调用方降级处理。
        /// </summary>
        private static bool FFmpegSupportsFilter(string ffmpegPath, string filterName)
        {
            string cacheKey = $"{ffmpegPath}|{filterName}";
            if (_ffmpegFilterSupportCache.TryGetValue(cacheKey, out bool cached)) return cached;

            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = ffmpegPath,
                    Arguments = "-hide_banner -filters",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                using var process = Process.Start(startInfo);
                if (process == null)
                {
                    _ffmpegFilterSupportCache[cacheKey] = false;
                    return false;
                }

                string output = process.StandardOutput.ReadToEnd() + process.StandardError.ReadToEnd();
                if (!process.WaitForExit(3000))
                {
                    process.Kill();
                    _ffmpegFilterSupportCache[cacheKey] = false;
                    return false;
                }

                bool supported = output.IndexOf(filterName, System.StringComparison.OrdinalIgnoreCase) >= 0;
                _ffmpegFilterSupportCache[cacheKey] = supported;
                return supported;
            }
            catch
            {
                _ffmpegFilterSupportCache[cacheKey] = false;
                return false;
            }
        }

        /// <summary>
        /// 判断配置中的显示器是否存在于当前设备。
        /// </summary>
        private static bool HasDisplay(RecorderParamsConfig config, IReadOnlyList<RecorderDisplayInfo> displays)
        {
            foreach (var display in displays)
            {
                if ((string.IsNullOrWhiteSpace(config.captureDisplayName) || display.name == config.captureDisplayName) && display.index == config.displayIndex) return true;
            }

            return false;
        }

        /// <summary>
        /// 判断是否为 RTMP/RTMPS 地址。
        /// </summary>
        private static bool IsRtmpUrl(string url)
        {
            return !string.IsNullOrWhiteSpace(url) &&
                   (url.StartsWith("rtmp://", System.StringComparison.OrdinalIgnoreCase) ||
                    url.StartsWith("rtmps://", System.StringComparison.OrdinalIgnoreCase));
        }
    }
}
