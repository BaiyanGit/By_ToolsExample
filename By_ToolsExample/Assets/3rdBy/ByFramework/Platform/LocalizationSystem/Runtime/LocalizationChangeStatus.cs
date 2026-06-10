//=====================================================
// 文件名称: LocalizationChangeStatus.cs
// 创 建 者: Codex
// 创建日期: 2026-06-09
// 描    述: 定义 LocalizationSystem 语言切换状态枚举。
//=====================================================

namespace _3rdBy.ByFramework.Platform.LocalizationSystem
{
    public enum LocalizationChangeStatus
    {
        Success = 0,
        RuntimeSwitchDisabled = 1,
        LanguageNotAvailable = 2,
        ProviderFailure = 3,
        PersistenceFailure = 4,
    }
}
