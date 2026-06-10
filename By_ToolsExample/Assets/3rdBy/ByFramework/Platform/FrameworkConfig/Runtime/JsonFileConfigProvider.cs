//=====================================================
// 文件名称: JsonFileConfigProvider.cs
// 创 建 者: Codex
// 创建日期: 2026-06-08
// 描    述: 提供基于 JSON 文件的 FrameworkConfig Provider 公共加载逻辑。
//=====================================================

namespace _3rdBy.ByFramework.Platform.FrameworkConfig
{
    using System;
    using System.IO;

    public abstract class JsonFileConfigProvider : IConfigProvider
    {
        public abstract ConfigSource Source { get; }

        public abstract ConfigPriority Priority { get; }

        public abstract string Description { get; }

        protected abstract string ResolveFilePath();

        public bool TryLoad(out ConfigLayer layer, out ConfigValidationMessage validationMessage)
        {
            layer = null;
            validationMessage = null;

            string filePath = ResolveFilePath();
            if (string.IsNullOrWhiteSpace(filePath))
            {
                return false;
            }

            if (!File.Exists(filePath))
            {
                return false;
            }

            try
            {
                string rawJson = File.ReadAllText(filePath);
                string normalizedJson = FrameworkConfigJsonUtility.NormalizeJson(rawJson);
                layer = new ConfigLayer(Source, Priority, filePath, normalizedJson, DateTime.UtcNow);
                return true;
            }
            catch (Exception exception)
            {
                validationMessage = new ConfigValidationMessage(
                    "Warning",
                    $"[FrameworkConfig] 读取配置文件失败：{exception.Message}",
                    Description,
                    filePath);
                return false;
            }
        }
    }
}
