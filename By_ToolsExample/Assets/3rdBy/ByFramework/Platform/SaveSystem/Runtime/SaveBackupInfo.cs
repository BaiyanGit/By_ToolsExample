//=====================================================
// 文件名称: SaveBackupInfo.cs
// 创 建 者: Codex
// 创建日期: 2026-06-08
// 描    述: 定义 SaveSystem 的备份元数据模型。
//=====================================================

namespace _3rdBy.ByFramework.Platform.SaveSystem
{
    using System;

    /// <summary>
    /// SaveSystem 备份元数据模型。
    /// </summary>
    public sealed class SaveBackupInfo
    {
        /// <summary>
        /// 初始化备份元数据模型。
        /// </summary>
        /// <param name="backupId">备份标识。</param>
        /// <param name="sourceSaveKey">源保存键。</param>
        /// <param name="scope">存储域。</param>
        /// <param name="profileName">Profile 名称。</param>
        /// <param name="createdTimeUtc">备份创建时间。</param>
        /// <param name="backupPath">备份路径。</param>
        /// <param name="reason">备份原因。</param>
        public SaveBackupInfo(
            string backupId,
            string sourceSaveKey,
            SaveScope scope,
            string profileName,
            DateTime createdTimeUtc,
            string backupPath,
            string reason)
        {
            BackupId = backupId ?? string.Empty;
            SourceSaveKey = sourceSaveKey ?? string.Empty;
            Scope = scope;
            ProfileName = profileName ?? string.Empty;
            CreatedTimeUtc = createdTimeUtc;
            BackupPath = backupPath ?? string.Empty;
            Reason = reason ?? string.Empty;
        }

        /// <summary>
        /// 获取备份标识。
        /// </summary>
        public string BackupId { get; }

        /// <summary>
        /// 获取源保存键。
        /// </summary>
        public string SourceSaveKey { get; }

        /// <summary>
        /// 获取存储域。
        /// </summary>
        public SaveScope Scope { get; }

        /// <summary>
        /// 获取 Profile 名称。
        /// </summary>
        public string ProfileName { get; }

        /// <summary>
        /// 获取备份创建时间（UTC）。
        /// </summary>
        public DateTime CreatedTimeUtc { get; }

        /// <summary>
        /// 获取备份路径。
        /// </summary>
        public string BackupPath { get; }

        /// <summary>
        /// 获取备份原因。
        /// </summary>
        public string Reason { get; }
    }
}
