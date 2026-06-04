//=====================================================
// 文件名称: RecorderConfigMigrator
// 创 建 者: wangbaiyan
// 创建日期: 2026-05-23
// 描    述: 提供录制配置旧 JSON 字段读取、configId 规整和字段迁移辅助。
//=====================================================

using System;
using Demos.示例_录制视频Recorder.Scripts.UISettings;

namespace Demos.示例_录制视频Recorder.Scripts.Core
{
    /// <summary>
    /// 录制配置迁移辅助。
    /// </summary>
    public static class RecorderConfigMigrator
    {
        /// <summary>
        /// 迁移旧配置的显示名和录制屏幕名。
        /// </summary>
        public static void MigrateIdentity(RecorderParamsConfig config, string json, string fileName)
        {
            if (config == null) return;
            string legacyConfigName = ExtractJsonString(json, "configName");
            string legacyFileName = ExtractJsonString(json, "fileName");
            if (config.schemaVersion <= 1 && string.IsNullOrWhiteSpace(config.captureDisplayName) && !string.IsNullOrWhiteSpace(config.displayName))
            {
                config.captureDisplayName = config.displayName;
                config.displayName = string.Empty;
            }

            if (string.IsNullOrWhiteSpace(config.displayName)) config.displayName = !string.IsNullOrWhiteSpace(legacyConfigName) ? legacyConfigName : System.IO.Path.GetFileNameWithoutExtension(!string.IsNullOrWhiteSpace(legacyFileName) ? legacyFileName : fileName);
            config.configName = config.displayName;
            string identityFileName = !string.IsNullOrWhiteSpace(legacyFileName) ? legacyFileName : fileName;
            config.configId = string.IsNullOrWhiteSpace(config.configId) ? GetConfigIdFromFileName(identityFileName) : NormalizeConfigId(config.configId);
            EnsureAudioGainDefaults(config, json);
        }

        /// <summary>
        /// 迁移旧使用指针。
        /// </summary>
        public static void MigrateReference(RecordConfigReference reference, string json)
        {
            if (reference == null) return;
            string legacyFileName = ExtractJsonString(json, "fileName");
            if (string.IsNullOrWhiteSpace(reference.currentConfigId) && !string.IsNullOrWhiteSpace(legacyFileName)) reference.currentConfigId = GetConfigIdFromFileName(legacyFileName);
            if (string.IsNullOrWhiteSpace(reference.fileName) && !string.IsNullOrWhiteSpace(legacyFileName)) reference.fileName = legacyFileName;
            if (string.IsNullOrWhiteSpace(reference.fileName) && !string.IsNullOrWhiteSpace(reference.currentConfigId)) reference.fileName = BuildConfigFileName(reference.currentConfigId);
        }

        /// <summary>
        /// 根据配置 ID 生成文件名。
        /// </summary>
        public static string BuildConfigFileName(string configId)
        {
            return $"{NormalizeConfigId(configId)}.json";
        }

        /// <summary>
        /// 从文件名推导配置 ID。
        /// </summary>
        public static string GetConfigIdFromFileName(string fileName)
        {
            return NormalizeConfigId(System.IO.Path.GetFileNameWithoutExtension(fileName));
        }

        /// <summary>
        /// 将字符串规整成 configId 允许的格式。
        /// </summary>
        public static string NormalizeConfigId(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return "recorder_config";
            var chars = new System.Collections.Generic.List<char>();
            bool lastWasUnderscore = false;
            foreach (char c in value.Trim())
            {
                if (char.IsLetterOrDigit(c) || c == '-')
                {
                    chars.Add(c);
                    lastWasUnderscore = false;
                }
                else if (c == '_' || char.IsWhiteSpace(c))
                {
                    if (!lastWasUnderscore)
                    {
                        chars.Add('_');
                        lastWasUnderscore = true;
                    }
                }
            }

            string result = new string(chars.ToArray()).Trim('_');
            return string.IsNullOrWhiteSpace(result) ? "recorder_config" : result;
        }

        /// <summary>
        /// 从简单 JSON 文本中读取字符串字段。
        /// </summary>
        public static string ExtractJsonString(string json, string key)
        {
            if (string.IsNullOrWhiteSpace(json) || string.IsNullOrWhiteSpace(key)) return string.Empty;
            string pattern = $"\"{key}\"";
            int keyIndex = json.IndexOf(pattern, StringComparison.Ordinal);
            if (keyIndex < 0) return string.Empty;
            int colonIndex = json.IndexOf(':', keyIndex + pattern.Length);
            if (colonIndex < 0) return string.Empty;
            int startQuote = json.IndexOf('"', colonIndex + 1);
            if (startQuote < 0) return string.Empty;
            int endQuote = json.IndexOf('"', startQuote + 1);
            return endQuote < 0 ? string.Empty : json.Substring(startQuote + 1, endQuote - startQuote - 1);
        }

        /// <summary>
        /// 补齐旧配置缺失的录制音量增益字段默认值。
        /// </summary>
        private static void EnsureAudioGainDefaults(RecorderParamsConfig config, string json)
        {
            if (config == null) return;
            if (!HasJsonKey(json, "enableAudioGain")) config.enableAudioGain = false;
            if (!HasJsonKey(json, "audioGainDb")) config.audioGainDb = 0f;
            if (!HasJsonKey(json, "audioLimiterEnabled")) config.audioLimiterEnabled = true;
        }

        /// <summary>
        /// 判断 JSON 文本是否包含指定字段。
        /// </summary>
        private static bool HasJsonKey(string json, string key)
        {
            if (string.IsNullOrWhiteSpace(json) || string.IsNullOrWhiteSpace(key)) return false;
            return json.IndexOf($"\"{key}\"", StringComparison.Ordinal) >= 0;
        }
    }
}
