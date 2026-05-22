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
        /// 获取当前配置中的 FFmpeg 路径，未配置时回退到 StreamingAssets/FFmpegTools/FFmpegApp。
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
                Directory.CreateDirectory(GetConfigDirectory());
                var    reference  = LoadOrCreateUseReference();
                string configPath = Path.Combine(GetConfigDirectory(), reference.fileName);
                if (!File.Exists(configPath))
                {
                    reference = CreateFallbackUseReference();
                    SaveUseReference(reference);
                    configPath = Path.Combine(GetConfigDirectory(), reference.fileName);
                }

                if (!File.Exists(configPath)) return false;
                var config = JsonUtility.FromJson<RecorderParamsConfig>(File.ReadAllText(configPath));
                if (config == null) return false;

                NormalizeLoadedConfig(config, configPath);
                recorderParamsConfig = config;
                return true;
            }
            catch (Exception exception)
            {
                Debug.LogError("读取当前录制配置失败: " + exception.Message);
                return false;
            }
        }

        /// <summary>
        /// 读取或创建当前平台的使用指针。
        /// </summary>
        private static RecordConfigReference LoadOrCreateUseReference()
        {
            string path = GetUseRecordConfigPath();
            if (File.Exists(path))
            {
                var reference = JsonUtility.FromJson<RecordConfigReference>(File.ReadAllText(path));
                if (reference != null && IsCurrentPlatform(reference.platform) && !string.IsNullOrWhiteSpace(reference.fileName))
                {
                    reference.fileName   = Path.GetFileName(reference.fileName);
                    reference.sourceType = GetSourceType(reference.fileName);
                    return reference;
                }
            }

            var fallback = CreateFallbackUseReference();
            SaveUseReference(fallback);
            return fallback;
        }

        /// <summary>
        /// 生成当前平台默认中配置的使用指针。
        /// </summary>
        private static RecordConfigReference CreateFallbackUseReference()
        {
            string platform = GetCurrentPlatformTag();
            return new RecordConfigReference
            {
                platform   = platform,
                sourceType = "Template",
                fileName   = $"Template_{platform}_Medium.json"
            };
        }

        /// <summary>
        /// 保存当前平台的使用指针。
        /// </summary>
        private static void SaveUseReference(RecordConfigReference reference)
        {
            if (reference == null) return;
            Directory.CreateDirectory(GetConfigDirectory());
            File.WriteAllText(GetUseRecordConfigPath(), JsonUtility.ToJson(reference, true));
        }

        /// <summary>
        /// 补全从 JSON 中读取到的旧配置字段。
        /// </summary>
        private static void NormalizeLoadedConfig(RecorderParamsConfig config, string path)
        {
            config.fileName  = Path.GetFileName(path);
            config.platform  = NormalizePlatform(config.platform);
            config.isDefault = IsTemplateFileName(config.fileName);
            if (string.IsNullOrWhiteSpace(config.configName)) config.configName                                        = Path.GetFileNameWithoutExtension(config.fileName);
            if (string.IsNullOrWhiteSpace(config.videoSaveDirectory) && config.useMode == 0) config.videoSaveDirectory = GetDefaultVideoSaveDirectory();
            if (!string.IsNullOrWhiteSpace(config.customFFmpegPath))
                config.customFFmpegPath = config.customFFmpegPath.Replace("/StreamingAssets/FFmpegApp/", "/StreamingAssets/FFmpegTools/FFmpegApp/");
        }

        /// <summary>
        /// 判断平台字段是否匹配当前运行平台。
        /// </summary>
        private static bool IsCurrentPlatform(string platform)
        {
            return string.IsNullOrWhiteSpace(platform) || string.Equals(NormalizePlatform(platform), GetCurrentPlatformTag(), StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// 统一平台命名，配置文件名使用 Win/Linux。
        /// </summary>
        private static string NormalizePlatform(string platform)
        {
            return string.Equals(platform, "Windows", StringComparison.OrdinalIgnoreCase) ? "Win" : platform;
        }

        /// <summary>
        /// 根据文件名前缀判断配置来源。
        /// </summary>
        private static string GetSourceType(string fileName)
        {
            return IsTemplateFileName(fileName) ? "Template" : "Creat";
        }

        /// <summary>
        /// 判断配置文件是否为模板文件。
        /// </summary>
        private static bool IsTemplateFileName(string fileName)
        {
            return !string.IsNullOrWhiteSpace(fileName) && fileName.StartsWith("Template_", StringComparison.OrdinalIgnoreCase);
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
            return Path.Combine(Application.streamingAssetsPath, "FFmpegTools", "Configs");
        }

        /// <summary>
        /// 获取当前平台使用指针文件路径。
        /// </summary>
        private static string GetUseRecordConfigPath()
        {
            string fileName = GetCurrentPlatformTag() == "Linux" ? "UseLinuxRecordConfig.json" : "UseWinRecordConfig.json";
            return Path.Combine(GetConfigDirectory(), fileName);
        }

        /// <summary>
        /// 获取默认视频保存目录。
        /// </summary>
        private static string GetDefaultVideoSaveDirectory()
        {
            return Path.Combine(Application.streamingAssetsPath, "FFmpegTools", "Videos");
        }

        /// <summary>
        /// 获取当前平台默认 FFmpeg 执行文件路径。
        /// </summary>
        private static string GetPlatformDefaultFFmpegPath()
        {
            return Application.platform switch
            {
                RuntimePlatform.WindowsPlayer or RuntimePlatform.WindowsEditor => Path.Combine(Application.streamingAssetsPath, "FFmpegTools", "FFmpegApp", "ffmpeg.exe"),
                RuntimePlatform.LinuxPlayer or RuntimePlatform.LinuxEditor     => Path.Combine(Application.streamingAssetsPath, "FFmpegTools", "FFmpegApp", "ffmpeg"),
                _                                                              => string.Empty
            };
        }
    }
}
