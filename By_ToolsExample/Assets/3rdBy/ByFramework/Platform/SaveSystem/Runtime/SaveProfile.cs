//=====================================================
// 文件名称: SaveProfile.cs
// 创 建 者: Codex
// 创建日期: 2026-06-08
// 描    述: 定义 SaveSystem 的保存策略快照。
//=====================================================

namespace _3rdBy.ByFramework.Platform.SaveSystem
{
    using System;

    /// <summary>
    /// 保存策略快照。
    /// </summary>
    public sealed class SaveProfile
    {
        /// <summary>
        /// 初始化保存策略快照。
        /// </summary>
        /// <param name="profileName">Profile 名称。</param>
        /// <param name="scope">存储域。</param>
        /// <param name="providerType">Provider 类型。</param>
        /// <param name="storageRoot">存储根目录。</param>
        /// <param name="format">存储格式。</param>
        /// <param name="enableBackup">是否启用备份。</param>
        /// <param name="enableMigration">是否启用迁移能力。</param>
        /// <param name="enableAsync">是否启用异步能力。</param>
        /// <param name="allowManualEdit">是否允许人工编辑。</param>
        public SaveProfile(
            string profileName,
            SaveScope scope,
            string providerType,
            string storageRoot,
            string format,
            bool enableBackup,
            bool enableMigration,
            bool enableAsync,
            bool allowManualEdit)
        {
            ProfileName = string.IsNullOrWhiteSpace(profileName)
                ? throw new ArgumentException("Profile name cannot be null or empty.", nameof(profileName))
                : profileName;
            ProviderType = string.IsNullOrWhiteSpace(providerType)
                ? throw new ArgumentException("Provider type cannot be null or empty.", nameof(providerType))
                : providerType;
            Format = string.IsNullOrWhiteSpace(format) ? "json" : format;
            StorageRoot = storageRoot ?? string.Empty;
            Scope = scope;
            EnableBackup = enableBackup;
            EnableMigration = enableMigration;
            EnableAsync = enableAsync;
            AllowManualEdit = allowManualEdit;
        }

        /// <summary>
        /// 获取 Profile 名称。
        /// </summary>
        public string ProfileName { get; }

        /// <summary>
        /// 获取存储域。
        /// </summary>
        public SaveScope Scope { get; }

        /// <summary>
        /// 获取 Provider 类型。
        /// </summary>
        public string ProviderType { get; }

        /// <summary>
        /// 获取存储根目录。
        /// </summary>
        public string StorageRoot { get; }

        /// <summary>
        /// 获取存储格式。
        /// </summary>
        public string Format { get; }

        /// <summary>
        /// 获取是否启用备份。
        /// </summary>
        public bool EnableBackup { get; }

        /// <summary>
        /// 获取是否启用迁移能力。
        /// </summary>
        public bool EnableMigration { get; }

        /// <summary>
        /// 获取是否启用异步能力。
        /// </summary>
        public bool EnableAsync { get; }

        /// <summary>
        /// 获取是否允许人工编辑。
        /// </summary>
        public bool AllowManualEdit { get; }
    }
}
