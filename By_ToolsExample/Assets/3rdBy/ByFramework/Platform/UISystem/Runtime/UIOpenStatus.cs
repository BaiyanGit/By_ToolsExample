//=====================================================
// 文件名称: UIOpenStatus.cs
// 创建者: Codex
// 创建日期: 2026-06-09
// 描    述: 定义 UI 打开状态。
//=====================================================

namespace _3rdBy.ByFramework.Platform.UISystem
{
    public enum UIOpenStatus
    {
        Success = 0,
        ViewAlreadyOpen = 1,
        ResourceUnavailable = 2,
        RootNotFound = 3,
        LayerNotFound = 4,
        ProviderFailure = 5,
    }
}
