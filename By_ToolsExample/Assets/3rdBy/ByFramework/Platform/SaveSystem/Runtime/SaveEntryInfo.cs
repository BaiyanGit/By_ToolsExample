//=====================================================
// 文件名称: SaveEntryInfo.cs
// 创 建 者: Codex
// 创建日期: 2026-06-08
// 描    述: 定义 SaveSystem 的条目元数据模型。
//=====================================================

namespace _3rdBy.ByFramework.Platform.SaveSystem
{
    using System;

    /// <summary>
    /// SaveSystem 条目元数据模型。
    /// </summary>
    public sealed class SaveEntryInfo
    {
        /// <summary>
        /// 初始化条目元数据模型。
        /// </summary>
        /// <param name="saveKey">逻辑保存键。</param>
        /// <param name="scope">存储域。</param>
        /// <param name="profileName">Profile 名称。</param>
        /// <param name="providerType">Provider 类型。</param>
        /// <param name="resolvedPath">解析后的物理路径。</param>
        /// <param name="dataType">数据类型名。</param>
        /// <param name="createdTimeUtc">创建时间。</param>
        /// <param name="modifiedTimeUtc">修改时间。</param>
        /// <param name="sizeInBytes">大小。</param>
        public SaveEntryInfo(
            string saveKey,
            SaveScope scope,
            string profileName,
            string providerType,
            string resolvedPath,
            string dataType,
            DateTime createdTimeUtc,
            DateTime modifiedTimeUtc,
            long sizeInBytes)
        {
            SaveKey = saveKey ?? string.Empty;
            Scope = scope;
            ProfileName = profileName ?? string.Empty;
            ProviderType = providerType ?? string.Empty;
            ResolvedPath = resolvedPath ?? string.Empty;
            DataType = dataType ?? string.Empty;
            CreatedTimeUtc = createdTimeUtc;
            ModifiedTimeUtc = modifiedTimeUtc;
            SizeInBytes = sizeInBytes;
        }

        /// <summary>
        /// 获取逻辑保存键。
        /// </summary>
        public string SaveKey { get; }

        /// <summary>
        /// 获取存储域。
        /// </summary>
        public SaveScope Scope { get; }

        /// <summary>
        /// 获取 Profile 名称。
        /// </summary>
        public string ProfileName { get; }

        /// <summary>
        /// 获取 Provider 类型。
        /// </summary>
        public string ProviderType { get; }

        /// <summary>
        /// 获取解析后的物理路径。
        /// </summary>
        public string ResolvedPath { get; }

        /// <summary>
        /// 获取数据类型名。
        /// </summary>
        public string DataType { get; }

        /// <summary>
        /// 获取创建时间（UTC）。
        /// </summary>
        public DateTime CreatedTimeUtc { get; }

        /// <summary>
        /// 获取修改时间（UTC）。
        /// </summary>
        public DateTime ModifiedTimeUtc { get; }

        /// <summary>
        /// 获取文件大小。
        /// </summary>
        public long SizeInBytes { get; }
    }
}
