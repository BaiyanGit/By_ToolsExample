//=====================================================
// 文件名称: UICloseStatus.cs
// 创建者: Codex
// 创建日期: 2026-06-09
// 描    述: 定义 UI 关闭状态。
//=====================================================

namespace _3rdBy.ByFramework.Platform.UISystem
{
    public enum UICloseStatus
    {
        Success = 0,
        ViewNotFound = 1,
        ViewAlreadyClosed = 2,
        ProviderFailure = 3,
    }
}
