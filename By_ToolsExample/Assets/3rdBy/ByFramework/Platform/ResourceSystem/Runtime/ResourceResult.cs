//=====================================================
// 文件名称: ResourceResult.cs
// 创建作者: Codex
// 创建日期: 2026-06-08
// 描    述: 定义 ResourceSystem 资源加载结果。
//=====================================================

namespace _3rdBy.ByFramework.Platform.ResourceSystem
{
    using UnityObject = UnityEngine.Object;

    /// <summary>
    /// 资源加载结果基类。
    /// </summary>
    public class ResourceResult
    {
        /// <summary>
        /// 初始化资源加载结果。
        /// </summary>
        public ResourceResult(
            bool success,
            string resourceKey,
            string providerType,
            string resolvedAddress,
            ResourceLocation location,
            ResourceHandle handle = null,
            string errorCode = "",
            string errorMessage = "")
        {
            Success = success;
            ResourceKey = resourceKey ?? string.Empty;
            ProviderType = providerType ?? string.Empty;
            ResolvedAddress = resolvedAddress ?? string.Empty;
            Location = location;
            Handle = handle;
            ErrorCode = errorCode ?? string.Empty;
            ErrorMessage = errorMessage ?? string.Empty;
        }

        /// <summary>
        /// 获取是否成功。
        /// </summary>
        public bool Success { get; }

        /// <summary>
        /// 获取资源 Key。
        /// </summary>
        public string ResourceKey { get; }

        /// <summary>
        /// 获取 Provider 类型。
        /// </summary>
        public string ProviderType { get; }

        /// <summary>
        /// 获取解析后的地址。
        /// </summary>
        public string ResolvedAddress { get; }

        /// <summary>
        /// 获取资源位置。
        /// </summary>
        public ResourceLocation Location { get; }

        /// <summary>
        /// 获取资源句柄。
        /// </summary>
        public ResourceHandle Handle { get; }

        /// <summary>
        /// 获取错误码。
        /// </summary>
        public string ErrorCode { get; }

        /// <summary>
        /// 获取错误消息。
        /// </summary>
        public string ErrorMessage { get; }
    }

    /// <summary>
    /// 带资源对象的加载结果。
    /// </summary>
    public sealed class ResourceResult<TAsset> : ResourceResult where TAsset : UnityObject
    {
        /// <summary>
        /// 初始化带资源对象的加载结果。
        /// </summary>
        public ResourceResult(
            bool success,
            TAsset asset,
            string resourceKey,
            string providerType,
            string resolvedAddress,
            ResourceLocation location,
            ResourceHandle handle = null,
            string errorCode = "",
            string errorMessage = "")
            : base(success, resourceKey, providerType, resolvedAddress, location, handle, errorCode, errorMessage)
        {
            Asset = asset;
        }

        /// <summary>
        /// 获取资源对象。
        /// </summary>
        public TAsset Asset { get; }
    }
}
