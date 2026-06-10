//=====================================================
// 文件名称: IConfigProvider.cs
// 创 建 者: Codex
// 创建日期: 2026-06-08
// 描    述: 定义 FrameworkConfig 配置来源提供器接口。
//=====================================================

namespace _3rdBy.ByFramework.Platform.FrameworkConfig
{
    /// <summary>
    /// 配置来源提供器接口。
    /// </summary>
    public interface IConfigProvider
    {
        /// <summary>
        /// 获取提供器对应的配置来源。
        /// </summary>
        ConfigSource Source { get; }

        /// <summary>
        /// 获取提供器优先级。
        /// </summary>
        ConfigPriority Priority { get; }

        /// <summary>
        /// 获取提供器描述。
        /// </summary>
        string Description { get; }

        /// <summary>
        /// 尝试加载配置层。
        /// </summary>
        /// <param name="layer">输出的配置层。</param>
        /// <param name="validationMessage">加载失败或警告时输出的验证消息。</param>
        /// <returns>加载成功返回 true，否则返回 false。</returns>
        bool TryLoad(out ConfigLayer layer, out ConfigValidationMessage validationMessage);
    }
}
