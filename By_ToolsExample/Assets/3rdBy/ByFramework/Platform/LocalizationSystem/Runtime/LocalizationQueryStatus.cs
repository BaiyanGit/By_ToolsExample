//=====================================================
// 文件名称: LocalizationQueryStatus.cs
// 创 建 者: Codex
// 创建日期: 2026-06-09
// 描    述: 定义 LocalizationSystem 查询状态枚举。
//=====================================================

namespace _3rdBy.ByFramework.Platform.LocalizationSystem
{
    public enum LocalizationQueryStatus
    {
        Success = 0,
        MissingKey = 1,
        MissingLanguage = 2,
        ProviderFailure = 3,
    }
}
