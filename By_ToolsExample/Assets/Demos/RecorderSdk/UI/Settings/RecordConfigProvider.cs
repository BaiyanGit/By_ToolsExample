//=====================================================
// 文件名称: RecordConfigProvider
// 创 建 者: wangbaiyan
// 创建日期: 2026-05-19
// 描    述: 读取当前平台正在使用的录制配置引用，并加载对应配置文件。
//=====================================================

namespace Demos.示例_录制视频Recorder.Scripts
{
    using System;
    using System.IO;
    using Core;
    using UISettings;
    using UnityEngine;

    [Serializable]
    public class RecordConfigProvider
    {
        [Header("参数设置")] public RecorderParamsConfig recorderParamsConfig;

        /// <summary>
        /// 当前录制器实际使用的配置数据。
        /// </summary>
        public RecorderParamsConfig Config => recorderParamsConfig;

        /// <summary>
        /// 当前配置是否为本地存储模式。
        /// </summary>
        public bool IsLocalSaveMode => (recorderParamsConfig?.useMode ?? 0) == 0;

        /// <summary>
        /// 当前配置是否为视频推流模式。
        /// </summary>
        public bool IsStreamMode => (recorderParamsConfig?.useMode ?? 0) == 1;

        /// <summary>
        /// 获取当前配置中的视频推流地址。
        /// </summary>
        public string StreamUrl => recorderParamsConfig?.streamUrl?.Trim() ?? string.Empty;

        /// <summary>
        /// 获取当前配置的视频输出目录，本地存储未配置时回退到默认 Videos 目录。
        /// </summary>
        public string OutputDirectory
        {
            get
            {
                if (!IsLocalSaveMode) return string.Empty;
                return !string.IsNullOrWhiteSpace(recorderParamsConfig?.videoSaveDirectory)
                           ? recorderParamsConfig.videoSaveDirectory
                           : GetDefaultVideoSaveDirectory();
            }
        }

        /// <summary>
        /// 获取当前配置中的 FFmpeg 路径，未配置时回退到 StreamingAssets/RecorderSDK/FFmpeg。
        /// </summary>
        public string FFmpegPath
        {
            get
            {
                return !string.IsNullOrWhiteSpace(recorderParamsConfig?.customFFmpegPath)
                           ? recorderParamsConfig.customFFmpegPath.Trim()
                           : GetPlatformDefaultFFmpegPath();
            }
        }

        /// <summary>
        /// 读取当前平台的使用指针文件，并加载指针指向的真实配置；源文件缺失时回退到当前平台中配置模板。
        /// </summary>
        public bool LoadCurrentRecordConfig()
        {
            try
            {
                var registry = new RecorderConfigRegistry(GetConfigDirectory(), GetCurrentPlatformTag());
                var result = registry.GetCurrentConfig();
                if (!result.success || result.config == null)
                {
                    Debug.LogError("读取当前录制配置失败: " + result.message);
                    return false;
                }

                recorderParamsConfig = result.config;
                return true;
            }
            catch (Exception exception)
            {
                Debug.LogError("读取当前录制配置失败: " + exception.Message);
                return false;
            }
        }

        /// <summary>
        /// 获取当前平台标签。
        /// </summary>
        private static string GetCurrentPlatformTag()
        {
            return Application.platform == RuntimePlatform.LinuxEditor || Application.platform == RuntimePlatform.LinuxPlayer ? "Linux" : "Win";
        }

        /// <summary>
        /// 获取配置根目录。
        /// </summary>
        private static string GetConfigDirectory()
        {
            return Path.Combine(Application.streamingAssetsPath, "RecorderSDK", "Configs");
        }

        /// <summary>
        /// 获取默认视频保存目录。
        /// </summary>
        private static string GetDefaultVideoSaveDirectory()
        {
            return Path.Combine(Application.streamingAssetsPath, "RecorderSDK", "Videos");
        }


        /// <summary>
        /// 获取当前平台默认 FFmpeg 执行文件路径。
        /// </summary>
        private static string GetPlatformDefaultFFmpegPath()
        {
            return Application.platform switch
            {
                RuntimePlatform.WindowsPlayer or RuntimePlatform.WindowsEditor => Path.Combine(Application.streamingAssetsPath, "RecorderSDK", "FFmpeg", "ffmpeg.exe"),
                RuntimePlatform.LinuxPlayer or RuntimePlatform.LinuxEditor     => Path.Combine(Application.streamingAssetsPath, "RecorderSDK", "FFmpeg", "ffmpeg"),
                _                                                              => string.Empty
            };
        }
    }
}
