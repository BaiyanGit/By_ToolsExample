//=====================================================
// 文件名称: ResourceCachePolicy.cs
// 创建作者: Codex
// 创建日期: 2026-06-08
// 描    述: 定义 ResourceSystem Runtime 缓存策略。
//=====================================================

namespace _3rdBy.ByFramework.Platform.ResourceSystem
{
    /// <summary>
    /// 运行时缓存策略。
    /// </summary>
    public sealed class ResourceCachePolicy
    {
        /// <summary>
        /// 默认缓存策略。
        /// </summary>
        public static readonly ResourceCachePolicy Default = new(true, false);

        /// <summary>
        /// 禁用运行时缓存策略。
        /// </summary>
        public static readonly ResourceCachePolicy NoCache = new(false, false);

        /// <summary>
        /// 保留已加载资源的缓存策略。
        /// </summary>
        public static readonly ResourceCachePolicy KeepLoaded = new(true, true);

        /// <summary>
        /// 初始化缓存策略。
        /// </summary>
        public ResourceCachePolicy(bool enableRuntimeCache, bool retainLoadedAsset)
        {
            EnableRuntimeCache = enableRuntimeCache;
            RetainLoadedAsset = retainLoadedAsset;
        }

        /// <summary>
        /// 获取是否启用运行时缓存。
        /// </summary>
        public bool EnableRuntimeCache { get; }

        /// <summary>
        /// 获取句柄全部释放后是否仍保留缓存资源。
        /// </summary>
        public bool RetainLoadedAsset { get; }
    }
}
