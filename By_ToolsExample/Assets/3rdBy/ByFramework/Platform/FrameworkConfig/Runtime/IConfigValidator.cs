//=====================================================
// 文件名称: IConfigValidator.cs
// 创 建 者: Codex
// 创建日期: 2026-06-08
// 描    述: 定义 FrameworkConfig 快照校验接口。
//=====================================================

namespace _3rdBy.ByFramework.Platform.FrameworkConfig
{
    using System.Collections.Generic;

    /// <summary>
    /// 配置校验接口。
    /// </summary>
    public interface IConfigValidator
    {
        /// <summary>
        /// 校验指定快照，并将消息写入结果集合。
        /// </summary>
        /// <param name="snapshot">待校验的快照。</param>
        /// <param name="messages">输出消息集合。</param>
        void Validate(FrameworkConfigSnapshot snapshot, List<ConfigValidationMessage> messages);
    }
}
