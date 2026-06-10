//=====================================================
// 文件名称: SaveMigrationResult.cs
// 创 建 者: Codex
// 创建日期: 2026-06-08
// 描    述: 定义 SaveSystem 的迁移结果模型。
//=====================================================

namespace _3rdBy.ByFramework.Platform.SaveSystem
{
    /// <summary>
    /// SaveSystem 迁移结果模型。
    /// </summary>
    public sealed class SaveMigrationResult
    {
        /// <summary>
        /// 初始化迁移结果。
        /// </summary>
        /// <param name="success">是否成功。</param>
        /// <param name="fromVersion">源版本。</param>
        /// <param name="toVersion">目标版本。</param>
        /// <param name="message">结果消息。</param>
        public SaveMigrationResult(
            bool success,
            string fromVersion,
            string toVersion,
            string message)
        {
            Success = success;
            FromVersion = fromVersion ?? string.Empty;
            ToVersion = toVersion ?? string.Empty;
            Message = message ?? string.Empty;
        }

        /// <summary>
        /// 获取是否成功。
        /// </summary>
        public bool Success { get; }

        /// <summary>
        /// 获取源版本。
        /// </summary>
        public string FromVersion { get; }

        /// <summary>
        /// 获取目标版本。
        /// </summary>
        public string ToVersion { get; }

        /// <summary>
        /// 获取结果消息。
        /// </summary>
        public string Message { get; }
    }
}
