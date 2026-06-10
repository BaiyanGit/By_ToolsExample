//=====================================================
// 文件名称: InputContextChangeStatus.cs
// 创 建 者: Codex
// 创建日期: 2026-06-09
// 描    述: 定义输入上下文变更状态。
//=====================================================

namespace _3rdBy.ByFramework.Platform.InputSystem
{
    public enum InputContextChangeStatus
    {
        Success = 0,
        OwnerInvalid = 1,
        ConflictRejected = 2,
        HandleNotFound = 3,
        ProviderFailure = 4,
    }
}
