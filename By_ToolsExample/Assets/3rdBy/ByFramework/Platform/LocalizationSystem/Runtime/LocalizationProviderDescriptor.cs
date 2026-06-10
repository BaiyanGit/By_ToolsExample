//=====================================================
// 文件名称: LocalizationProviderDescriptor.cs
// 创 建 者: Codex
// 创建日期: 2026-06-09
// 描    述: 定义 LocalizationSystem Provider 的只读诊断描述信息。
//=====================================================

namespace _3rdBy.ByFramework.Platform.LocalizationSystem
{
    public sealed class LocalizationProviderDescriptor
    {
        public LocalizationProviderDescriptor(string providerId, string providerType, int order, bool isWritable)
        {
            ProviderId = providerId ?? string.Empty;
            ProviderType = providerType ?? string.Empty;
            Order = order;
            IsWritable = isWritable;
        }

        public string ProviderId { get; }

        public string ProviderType { get; }

        public int Order { get; }

        public bool IsWritable { get; }
    }
}
