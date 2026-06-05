//=====================================================
// 文件名称: RecorderPathService
// 创 建 者: wangbaiyan
// 创建日期: 2026-05-23
// 描    述: 统一生成录制输出路径、临时路径和默认 StreamingAssets 工具目录。
//=====================================================

namespace Demos.RecorderSdk.Core.Config
{
    using System;
    using System.IO;
    using Runtime;
    using UI.Settings;
    using UnityEngine;

    /// <summary>
    /// 录制路径服务。
    /// </summary>
    public static class RecorderPathService
    {
        /// <summary>
        /// 获取默认 FFmpeg 路径。
        /// </summary>
        public static string GetPlatformDefaultFFmpegPath()
        {
            return Application.platform switch
            {
                RuntimePlatform.WindowsPlayer or RuntimePlatform.WindowsEditor => Path.Combine(Application.streamingAssetsPath, "RecorderSDK", "FFmpeg", "ffmpeg.exe"),
                RuntimePlatform.LinuxPlayer or RuntimePlatform.LinuxEditor     => Path.Combine(Application.streamingAssetsPath, "RecorderSDK", "FFmpeg", "ffmpeg"),
                _                                                              => string.Empty
            };
        }

        /// <summary>
        /// 获取默认视频保存目录。
        /// </summary>
        public static string GetDefaultVideoDirectory()
        {
            return Path.Combine(Application.streamingAssetsPath, "RecorderSDK", "Videos");
        }

        /// <summary>
        /// 鑾峰彇 RecorderSDK 褰曞埗涓存椂鐩綍銆?
        /// </summary>
        public static string GetDefaultTempDirectory()
        {
            return Path.Combine(Application.temporaryCachePath, "RecorderSDK");
        }

        /// <summary>
        /// 根据当前配置创建录制会话路径。
        /// </summary>
        public static RecorderSession CreateSession(RecorderParamsConfig config, string outputDirectory, RecorderDisplayInfo target)
        {
            bool isStreaming = (config?.useMode ?? 0) == 1;
            string sessionId = Guid.NewGuid().ToString("N");
            if (isStreaming)
            {
                string streamUrl = config?.streamUrl?.Trim() ?? string.Empty;
                return new RecorderSession
                {
                    sessionId = sessionId,
                    startTime = DateTime.Now,
                    outputPath = streamUrl,
                    videoTempPath = streamUrl,
                    audioTempPath = string.Empty,
                    isStreaming = true,
                    resolvedLinuxAudioSource = string.Empty,
                    configId = config?.configId,
                    configName = config?.displayName,
                    displayName = target.name
                };
            }

            string realOutputDirectory = string.IsNullOrWhiteSpace(outputDirectory) ? GetDefaultVideoDirectory() : outputDirectory;
            Directory.CreateDirectory(realOutputDirectory);
            string realTempDirectory = Path.Combine(GetDefaultTempDirectory(), sessionId);
            Directory.CreateDirectory(realTempDirectory);
            string outputFilePrefix = config == null || string.IsNullOrWhiteSpace(config.outputFilePrefix) ? "recording" : config.outputFilePrefix;
            string stamp = DateTime.Now.ToString("yyyyMMdd_HHmmss_fff");
            string extension = config == null || config.outputAsWebm ? ".webm" : ".mp4";

            return new RecorderSession
            {
                sessionId = sessionId,
                startTime = DateTime.Now,
                outputPath = Path.Combine(realOutputDirectory, $"{outputFilePrefix}_{stamp}{extension}"),
                videoTempPath = Path.Combine(realTempDirectory, $"{outputFilePrefix}_{stamp}_video_tmp{extension}"),
                audioTempPath = Path.Combine(realTempDirectory, $"{outputFilePrefix}_{stamp}_audio_tmp.wav"),
                isStreaming = false,
                resolvedLinuxAudioSource = string.Empty,
                configId = config?.configId,
                configName = config?.displayName,
                displayName = target.name
            };
        }
    }
}
