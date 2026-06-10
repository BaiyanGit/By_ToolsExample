//=====================================================
// 文件名称: UIOpenOptions.cs
// 创建者: Codex
// 创建日期: 2026-06-09
// 描    述: 表达最小 UI 打开策略。
//=====================================================

namespace _3rdBy.ByFramework.Platform.UISystem
{
    public sealed class UIOpenOptions
    {
        public UIOpenOptions(
            UIRootId rootId = default,
            UILayerId layerId = default,
            bool addToHistory = true,
            bool cacheOnClose = true)
        {
            RootId = rootId;
            LayerId = layerId;
            AddToHistory = addToHistory;
            CacheOnClose = cacheOnClose;
        }

        public UIRootId RootId { get; }

        public UILayerId LayerId { get; }

        public bool AddToHistory { get; }

        public bool CacheOnClose { get; }
    }
}
