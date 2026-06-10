//=====================================================
// 文件名称: SaveScope.cs
// 创 建 者: Codex
// 创建日期: 2026-06-08
// 描    述: 定义 SaveSystem 的存储域枚举。
//=====================================================

namespace _3rdBy.ByFramework.Platform.SaveSystem
{
    /// <summary>
    /// 存储域枚举。
    /// </summary>
    public enum SaveScope
    {
        /// <summary>
        /// 全局存储域。
        /// </summary>
        Global = 0,

        /// <summary>
        /// 用户存储域。
        /// </summary>
        User = 1,

        /// <summary>
        /// 项目存储域。
        /// </summary>
        Project = 2,

        /// <summary>
        /// 运行时存储域。
        /// </summary>
        Runtime = 3,

        /// <summary>
        /// 缓存存储域。
        /// </summary>
        Cache = 4,

        /// <summary>
        /// 训练数据存储域。
        /// </summary>
        Training = 5,

        /// <summary>
        /// 设备数据存储域。
        /// </summary>
        Device = 6,

        /// <summary>
        /// 配置存储域。
        /// </summary>
        Config = 7,
    }
}
