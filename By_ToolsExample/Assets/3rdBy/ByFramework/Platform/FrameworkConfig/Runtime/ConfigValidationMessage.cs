//=====================================================
// 文件名称: ConfigValidationMessage.cs
// 创 建 者: Codex
// 创建日期: 2026-06-08
// 描    述: 表示 FrameworkConfig 在加载、合并或校验阶段产生的验证消息。
//=====================================================

namespace _3rdBy.ByFramework.Platform.FrameworkConfig
{
    using System;

    /// <summary>
    /// 配置验证消息。
    /// </summary>
    public sealed class ConfigValidationMessage
    {
        /// <summary>
        /// 初始化配置验证消息。
        /// </summary>
        /// <param name="severity">消息级别，建议使用 Info、Warning、Error、Fatal。</param>
        /// <param name="message">消息内容。</param>
        /// <param name="source">来源描述。</param>
        /// <param name="path">关联配置路径。</param>
        public ConfigValidationMessage(string severity, string message, string source, string path = "")
        {
            Severity = string.IsNullOrWhiteSpace(severity)
                ? throw new ArgumentException("Validation message severity cannot be null or empty.", nameof(severity))
                : severity;
            Message = string.IsNullOrWhiteSpace(message)
                ? throw new ArgumentException("Validation message content cannot be null or empty.", nameof(message))
                : message;
            Source = source ?? string.Empty;
            Path = path ?? string.Empty;
        }

        /// <summary>
        /// 获取消息级别。
        /// </summary>
        public string Severity { get; }

        /// <summary>
        /// 获取消息内容。
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// 获取来源描述。
        /// </summary>
        public string Source { get; }

        /// <summary>
        /// 获取关联配置路径。
        /// </summary>
        public string Path { get; }

        /// <summary>
        /// 获取当前消息是否属于阻塞性错误。
        /// </summary>
        public bool IsBlocking => Severity == "Error" || Severity == "Fatal";
    }
}
