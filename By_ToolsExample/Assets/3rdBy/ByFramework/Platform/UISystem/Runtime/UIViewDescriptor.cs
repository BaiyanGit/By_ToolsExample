//=====================================================
// 文件名称: UIViewDescriptor.cs
// 创建者: Codex
// 创建日期: 2026-06-09
// 描    述: 表达只读视图快照。
//=====================================================

namespace _3rdBy.ByFramework.Platform.UISystem
{
    using System;

    public sealed class UIViewDescriptor
    {
        public UIViewDescriptor(
            UIKey uiKey,
            UIRootId rootId,
            UILayerId layerId,
            bool isVisible)
        {
            if (uiKey.IsEmpty)
            {
                throw new ArgumentException("[UISystem] UIKey cannot be empty.", nameof(uiKey));
            }

            if (rootId.IsEmpty)
            {
                throw new ArgumentException("[UISystem] RootId cannot be empty.", nameof(rootId));
            }

            if (layerId.IsEmpty)
            {
                throw new ArgumentException("[UISystem] LayerId cannot be empty.", nameof(layerId));
            }

            UIKey = uiKey;
            RootId = rootId;
            LayerId = layerId;
            IsVisible = isVisible;
        }

        public UIKey UIKey { get; }

        public UIRootId RootId { get; }

        public UILayerId LayerId { get; }

        public bool IsVisible { get; }
    }
}
