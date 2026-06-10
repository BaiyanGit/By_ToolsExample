//=====================================================
// 文件名称: InputProfileSnapshot.cs
// 创 建 者: Codex
// 创建日期: 2026-06-09
// 描    述: 表达当前输入配置快照。
//=====================================================

namespace _3rdBy.ByFramework.Platform.InputSystem
{
    using System;
    using System.Collections.Generic;

    public sealed class InputProfileSnapshot
    {
        public InputProfileSnapshot(
            string profileId,
            int version,
            IReadOnlyList<InputBindingDescriptor> bindings)
        {
            ProfileId = string.IsNullOrWhiteSpace(profileId) ? string.Empty : profileId.Trim();
            Version = version;
            Bindings = CopyBindings(bindings);
        }

        public string ProfileId { get; }

        public int Version { get; }

        public IReadOnlyList<InputBindingDescriptor> Bindings { get; }

        private static IReadOnlyList<InputBindingDescriptor> CopyBindings(IReadOnlyList<InputBindingDescriptor> bindings)
        {
            if (bindings == null || bindings.Count == 0)
            {
                return Array.Empty<InputBindingDescriptor>();
            }

            List<InputBindingDescriptor> copied = new(bindings.Count);
            for (int index = 0; index < bindings.Count; index++)
            {
                if (bindings[index] != null)
                {
                    copied.Add(bindings[index]);
                }
            }

            return copied.AsReadOnly();
        }
    }
}
