//=====================================================
// 文件名称: IResourceProvider.cs
// 创建作者: Codex
// 创建日期: 2026-06-08
// 描    述: 定义 ResourceSystem Provider 接口。
//=====================================================

namespace _3rdBy.ByFramework.Platform.ResourceSystem
{
    using System.Threading;
    using System.Threading.Tasks;
    using UnityObject = UnityEngine.Object;

    /// <summary>
    /// ResourceSystem Provider 接口。
    /// </summary>
    public interface IResourceProvider
    {
        /// <summary>
        /// 获取 Provider 类型标识。
        /// </summary>
        string ProviderType { get; }

        /// <summary>
        /// 判断当前 Provider 是否可处理指定位置。
        /// </summary>
        bool CanHandle(ResourceLocation location);

        /// <summary>
        /// 同步加载资源。
        /// </summary>
        ResourceResult<TAsset> Load<TAsset>(ResourceRequest request, ResourceLocation location) where TAsset : UnityObject;

        /// <summary>
        /// 异步加载资源。
        /// </summary>
        Task<ResourceResult<TAsset>> LoadAsync<TAsset>(
            ResourceRequest request,
            ResourceLocation location,
            CancellationToken cancellationToken = default) where TAsset : UnityObject;

        /// <summary>
        /// 判断资源是否存在。
        /// </summary>
        bool Exists(ResourceLocation location);

        /// <summary>
        /// 释放资源。
        /// </summary>
        void Release(ResourceHandle handle);
    }
}
