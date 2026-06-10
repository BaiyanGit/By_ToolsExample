//=====================================================
// 文件名称: IConfigMerger.cs
// 创 建 者: Codex
// 创建日期: 2026-06-08
// 描    述: 定义 FrameworkConfig 配置层合并接口。
//=====================================================

namespace _3rdBy.ByFramework.Platform.FrameworkConfig
{
    using System.Collections.Generic;

    /// <summary>
    /// 配置层合并接口。
    /// </summary>
    public interface IConfigMerger
    {
        /// <summary>
        /// 按优先级合并全部配置层，并返回最终 JSON 文本。
        /// </summary>
        /// <param name="layers">待合并的配置层集合。</param>
        /// <returns>最终合并后的 JSON 文本。</returns>
        string Merge(IReadOnlyList<ConfigLayer> layers);
    }
}
