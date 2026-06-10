//=====================================================
// 文件名称: SaveMigrationPreview.cs
// 创 建 者: Codex
// 创建日期: 2026-06-08
// 描    述: 定义 SaveSystem 的迁移预览结果模型。
//=====================================================

namespace _3rdBy.ByFramework.Platform.SaveSystem
{
    /// <summary>
    /// SaveSystem 迁移预览结果模型。
    /// </summary>
    public sealed class SaveMigrationPreview
    {
        /// <summary>
        /// 初始化迁移预览结果。
        /// </summary>
        /// <param name="requiresMigration">是否需要迁移。</param>
        /// <param name="currentVersion">当前版本。</param>
        /// <param name="targetVersion">目标版本。</param>
        /// <param name="summary">摘要信息。</param>
        public SaveMigrationPreview(
            bool requiresMigration,
            string currentVersion,
            string targetVersion,
            string summary)
        {
            RequiresMigration = requiresMigration;
            CurrentVersion = currentVersion ?? string.Empty;
            TargetVersion = targetVersion ?? string.Empty;
            Summary = summary ?? string.Empty;
        }

        /// <summary>
        /// 获取是否需要迁移。
        /// </summary>
        public bool RequiresMigration { get; }

        /// <summary>
        /// 获取当前版本。
        /// </summary>
        public string CurrentVersion { get; }

        /// <summary>
        /// 获取目标版本。
        /// </summary>
        public string TargetVersion { get; }

        /// <summary>
        /// 获取摘要信息。
        /// </summary>
        public string Summary { get; }
    }
}
