//=====================================================
// 文件名称: InputActionResolveStatus.cs
// 创 建 者: Codex
// 创建日期: 2026-06-09
// 描    述: 定义输入动作解析结果。
//=====================================================

namespace _3rdBy.ByFramework.Platform.InputSystem
{
    public enum InputActionResolveStatus
    {
        Success = 0,
        ActionNotFound = 1,
        ContextBlocked = 2,
        ContextNotFound = 3,
        DeviceUnavailable = 4,
        ProviderFailure = 5,
    }
}
