//=====================================================
// 文件名称: InputContextRegistration.cs
// 创 建 者: Codex
// 创建日期: 2026-06-09
// 描    述: 描述输入上下文注册参数。
//=====================================================

namespace _3rdBy.ByFramework.Platform.InputSystem
{
    using System;
    using System.Collections.Generic;

    public sealed class InputContextRegistration
    {
        public InputContextRegistration(
            string contextId,
            string ownerId,
            int priority,
            bool exclusive,
            bool passThrough,
            IReadOnlyList<InputActionId> allowedActions)
        {
            ContextId = string.IsNullOrWhiteSpace(contextId) ? string.Empty : contextId.Trim();
            OwnerId = string.IsNullOrWhiteSpace(ownerId) ? string.Empty : ownerId.Trim();
            Priority = priority;
            Exclusive = exclusive;
            PassThrough = passThrough;
            AllowedActions = CopyAllowedActions(allowedActions);
        }

        public string ContextId { get; }

        public string OwnerId { get; }

        public int Priority { get; }

        public bool Exclusive { get; }

        public bool PassThrough { get; }

        public IReadOnlyList<InputActionId> AllowedActions { get; }

        private static IReadOnlyList<InputActionId> CopyAllowedActions(IReadOnlyList<InputActionId> allowedActions)
        {
            if (allowedActions == null || allowedActions.Count == 0)
            {
                return Array.Empty<InputActionId>();
            }

            List<InputActionId> copied = new(allowedActions.Count);
            for (int index = 0; index < allowedActions.Count; index++)
            {
                if (!allowedActions[index].IsEmpty)
                {
                    copied.Add(allowedActions[index]);
                }
            }

            return copied.AsReadOnly();
        }
    }
}
