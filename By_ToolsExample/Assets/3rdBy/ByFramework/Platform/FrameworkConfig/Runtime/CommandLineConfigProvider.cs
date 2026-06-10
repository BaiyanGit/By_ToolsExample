//=====================================================
// 文件名称: CommandLineConfigProvider.cs
// 创 建 者: Codex
// 创建日期: 2026-06-08
// 描    述: 提供命令行配置覆盖层读取能力。
//=====================================================

namespace _3rdBy.ByFramework.Platform.FrameworkConfig
{
    using System;
    using System.IO;

    /// <summary>
    /// 命令行配置提供器。
    /// </summary>
    public sealed class CommandLineConfigProvider : IConfigProvider
    {
        private const string JsonArgumentPrefix = "--framework-config-json=";
        private const string PathArgumentPrefix = "--framework-config-path=";
        private const string CompatArgumentPrefix = "--framework-config=";

        /// <summary>
        /// 获取提供器对应的配置来源。
        /// </summary>
        public ConfigSource Source => ConfigSource.CommandLine;

        /// <summary>
        /// 获取提供器优先级。
        /// </summary>
        public ConfigPriority Priority => ConfigPriority.CommandLine;

        /// <summary>
        /// 获取提供器描述。
        /// </summary>
        public string Description => "CommandLineConfigProvider";

        /// <summary>
        /// 尝试加载命令行配置层。
        /// </summary>
        /// <param name="layer">输出的配置层。</param>
        /// <param name="validationMessage">加载失败时输出的验证消息。</param>
        /// <returns>加载成功返回 true，否则返回 false。</returns>
        public bool TryLoad(out ConfigLayer layer, out ConfigValidationMessage validationMessage)
        {
            layer = null;
            validationMessage = null;

            string[] args = Environment.GetCommandLineArgs();
            string inlineJson = string.Empty;
            string filePath = string.Empty;

            for (int index = 0; index < args.Length; index++)
            {
                string argument = args[index];
                if (argument.StartsWith(JsonArgumentPrefix, StringComparison.OrdinalIgnoreCase))
                {
                    inlineJson = argument.Substring(JsonArgumentPrefix.Length);
                }
                else if (argument.StartsWith(PathArgumentPrefix, StringComparison.OrdinalIgnoreCase))
                {
                    filePath = argument.Substring(PathArgumentPrefix.Length);
                }
                else if (argument.StartsWith(CompatArgumentPrefix, StringComparison.OrdinalIgnoreCase))
                {
                    filePath = argument.Substring(CompatArgumentPrefix.Length);
                }
            }

            try
            {
                if (!string.IsNullOrWhiteSpace(inlineJson))
                {
                    string normalizedJson = FrameworkConfigJsonUtility.NormalizeJson(inlineJson);
                    layer = new ConfigLayer(Source, Priority, "CommandLine:inline", normalizedJson, DateTime.UtcNow);
                    return true;
                }

                if (string.IsNullOrWhiteSpace(filePath))
                {
                    return false;
                }

                if (!File.Exists(filePath))
                {
                    validationMessage = new ConfigValidationMessage(
                        "Warning",
                        "[FrameworkConfig] The config file specified by command line does not exist.",
                        Description,
                        filePath);
                    return false;
                }

                string rawJson = File.ReadAllText(filePath);
                string normalizedFileJson = FrameworkConfigJsonUtility.NormalizeJson(rawJson);
                layer = new ConfigLayer(Source, Priority, filePath, normalizedFileJson, DateTime.UtcNow);
                return true;
            }
            catch (Exception exception)
            {
                validationMessage = new ConfigValidationMessage(
                    "Warning",
                    $"[FrameworkConfig] 解析命令行配置失败：{exception.Message}",
                    Description);
                return false;
            }
        }
    }
}
