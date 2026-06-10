//=====================================================
// 文件名称: DisplayTargetDescriptor.cs
// 创建者: Codex
// 创建日期: 2026-06-09
// 描    述: 描述稳定显示目标信息。
//=====================================================

namespace _3rdBy.ByFramework.Platform.DisplaySystem
{
    using System;

    public sealed class DisplayTargetDescriptor
    {
        public DisplayTargetDescriptor(
            DisplayTargetId targetId,
            string targetType,
            bool isConnected)
        {
            if (targetId.IsEmpty)
            {
                throw new ArgumentException("[DisplaySystem] TargetId cannot be empty.", nameof(targetId));
            }

            if (string.IsNullOrWhiteSpace(targetType))
            {
                throw new ArgumentException("[DisplaySystem] TargetType cannot be empty.", nameof(targetType));
            }

            TargetId = targetId;
            TargetType = targetType.Trim();
            IsConnected = isConnected;
        }

        public DisplayTargetId TargetId { get; }

        public string TargetType { get; }

        public bool IsConnected { get; }
    }
}
