//=====================================================
// 文件名称: SaveDataEnvelope.cs
// 创 建 者: Codex
// 创建日期: 2026-06-08
// 描    述: 定义 SaveSystem 的通用数据包裹模型。
//=====================================================

namespace _3rdBy.ByFramework.Platform.SaveSystem
{
    using System;

    /// <summary>
    /// SaveSystem 通用数据包裹模型。
    /// </summary>
    [Serializable]
    public sealed class SaveDataEnvelope
    {
        /// <summary>
        /// 获取或设置 Schema 版本。
        /// </summary>
        public string SchemaVersion { get; set; } = "1.0.0";

        /// <summary>
        /// 获取或设置数据版本。
        /// </summary>
        public string DataVersion { get; set; } = "1.0.0";

        /// <summary>
        /// 获取或设置创建时间（UTC）。
        /// </summary>
        public string CreatedTimeUtc { get; set; } = DateTime.UtcNow.ToString("O");

        /// <summary>
        /// 获取或设置修改时间（UTC）。
        /// </summary>
        public string ModifiedTimeUtc { get; set; } = DateTime.UtcNow.ToString("O");

        /// <summary>
        /// 获取或设置载荷数据类型名。
        /// </summary>
        public string PayloadType { get; set; } = string.Empty;

        /// <summary>
        /// 获取或设置业务数据载荷 JSON。
        /// </summary>
        public string Payload { get; set; } = "{}";
    }
}
