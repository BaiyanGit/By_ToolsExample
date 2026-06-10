//=====================================================
// 文件名称: InputContextSnapshot.cs
// 创 建 者: Codex
// 创建日期: 2026-06-09
// 描    述: 表达当前输入上下文不可变快照。
//=====================================================

namespace _3rdBy.ByFramework.Platform.InputSystem
{
    using System;
    using System.Collections.Generic;

    public sealed class InputContextSnapshot
    {
        public InputContextSnapshot(
            IReadOnlyList<InputContextHandle> activeContexts,
            IReadOnlyList<InputActionId> resolvableActions)
        {
            ActiveContexts = CopyHandles(activeContexts);
            ResolvableActions = CopyActions(resolvableActions);
        }

        public IReadOnlyList<InputContextHandle> ActiveContexts { get; }

        public IReadOnlyList<InputActionId> ResolvableActions { get; }

        private static IReadOnlyList<InputContextHandle> CopyHandles(IReadOnlyList<InputContextHandle> source)
        {
            if (source == null || source.Count == 0)
            {
                return Array.Empty<InputContextHandle>();
            }

            List<InputContextHandle> copied = new(source.Count);
            for (int index = 0; index < source.Count; index++)
            {
                copied.Add(source[index]);
            }

            return copied.AsReadOnly();
        }

        private static IReadOnlyList<InputActionId> CopyActions(IReadOnlyList<InputActionId> source)
        {
            if (source == null || source.Count == 0)
            {
                return Array.Empty<InputActionId>();
            }

            List<InputActionId> copied = new(source.Count);
            for (int index = 0; index < source.Count; index++)
            {
                copied.Add(source[index]);
            }

            return copied.AsReadOnly();
        }
    }
}
