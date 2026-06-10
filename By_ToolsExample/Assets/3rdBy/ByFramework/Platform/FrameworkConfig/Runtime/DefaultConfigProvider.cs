//=====================================================
// 文件名称: DefaultConfigProvider.cs
// 创 建 者: Codex
// 创建日期: 2026-06-08
// 描    述: 提供 FrameworkConfig 的默认内置兼容配置层。
//=====================================================

namespace _3rdBy.ByFramework.Platform.FrameworkConfig
{
    using System;
    using _3rdBy.ByFramework.Core.Config;
    using _3rdBy.Plugins.LitJson;
    using UnityEngine;

    /// <summary>
    /// 默认内置配置提供器。
    /// </summary>
    public sealed class DefaultConfigProvider : IConfigProvider
    {
        /// <summary>
        /// 获取提供器对应的配置来源。
        /// </summary>
        public ConfigSource Source => ConfigSource.Default;

        /// <summary>
        /// 获取提供器优先级。
        /// </summary>
        public ConfigPriority Priority => ConfigPriority.Default;

        /// <summary>
        /// 获取提供器描述。
        /// </summary>
        public string Description => "DefaultConfigProvider";

        /// <summary>
        /// 尝试加载默认配置层。
        /// </summary>
        /// <param name="layer">输出的配置层。</param>
        /// <param name="validationMessage">加载警告。</param>
        /// <returns>始终返回 true。</returns>
        public bool TryLoad(out ConfigLayer layer, out ConfigValidationMessage validationMessage)
        {
            validationMessage = null;

            try
            {
                string json = BuildCompatibilityJson();
                layer = new ConfigLayer(Source, Priority, "DefaultCompatibility", json, DateTime.UtcNow);
                return true;
            }
            catch (Exception exception)
            {
                validationMessage = new ConfigValidationMessage(
                    "Error",
                    $"[FrameworkConfig] 构建默认兼容配置失败：{exception.Message}",
                    Description);
                layer = new ConfigLayer(Source, Priority, "DefaultFallback", "{}", DateTime.UtcNow);
                return true;
            }
        }

        private static string BuildCompatibilityJson()
        {
            _3rdBy.ByFramework.Core.Config.FrameworkConfig legacyConfig = FrameworkConfigProvider.Config;
            JsonData root = FrameworkConfigJsonUtility.CreateObject();
            JsonData framework = FrameworkConfigJsonUtility.CreateObject();
            framework["configVersion"] = "1.0.0";
            framework["compatibilitySource"] = "Core/Config/FrameworkConfig";
            root["framework"] = framework;

            JsonData modules = FrameworkConfigJsonUtility.CreateObject();
            modules["ModuleSettings"] = ParseUnityJson(JsonUtility.ToJson(legacyConfig.moduleSettings));
            modules["UISettings"] = ParseUnityJson(JsonUtility.ToJson(legacyConfig.uiSettings));
            modules["NetworkSettings"] = ParseUnityJson(JsonUtility.ToJson(legacyConfig.networkSettings));
            modules["DownloadSettings"] = ParseUnityJson(JsonUtility.ToJson(legacyConfig.downloadSettings));
            root["modules"] = modules;

            JsonData compatibility = FrameworkConfigJsonUtility.CreateObject();
            compatibility["legacyFrameworkConfig"] = ParseUnityJson(JsonUtility.ToJson(legacyConfig));
            root["compatibility"] = compatibility;
            return _3rdBy.Plugins.LitJson.JsonMapper.ToJson(root);
        }

        private static JsonData ParseUnityJson(string json)
        {
            return FrameworkConfigJsonUtility.Parse(json);
        }
    }
}
