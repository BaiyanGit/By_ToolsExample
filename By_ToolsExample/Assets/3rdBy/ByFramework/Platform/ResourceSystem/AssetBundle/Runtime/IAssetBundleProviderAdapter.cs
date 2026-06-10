//=====================================================
// 文件名称: IAssetBundleProviderAdapter.cs
// 创建作者: Codex
// 创建日期: 2026-06-09
// 描    述: 定义 AssetBundle 到 ResourceSystem 的 Provider 适配接口。
//=====================================================

namespace _3rdBy.ByFramework.Platform.ResourceSystem.AssetBundleSystem
{
    /// <summary>
    /// AssetBundle 到 ResourceSystem 的 Provider 适配接口。
    /// </summary>
    public interface IAssetBundleProviderAdapter : _3rdBy.ByFramework.Platform.ResourceSystem.IResourceProvider
    {
        /// <summary>
        /// 获取 AssetBundle 服务。
        /// </summary>
        IAssetBundleService AssetBundleService { get; }
    }
}
