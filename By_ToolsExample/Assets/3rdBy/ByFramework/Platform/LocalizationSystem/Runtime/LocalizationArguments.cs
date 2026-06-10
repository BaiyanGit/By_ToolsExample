//=====================================================
// 文件名称: LocalizationArguments.cs
// 创 建 者: Codex
// 创建日期: 2026-06-09
// 描    述: 定义 LocalizationSystem 的格式化参数快照。
//=====================================================

namespace _3rdBy.ByFramework.Platform.LocalizationSystem
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    /// <summary>
    /// 文本格式化参数快照。
    /// </summary>
    public readonly struct LocalizationArguments
    {
        private readonly IReadOnlyList<object> _positionalArguments;
        private readonly IReadOnlyDictionary<string, object> _namedArguments;

        public LocalizationArguments(
            IReadOnlyList<object> positionalArguments = null,
            IReadOnlyDictionary<string, object> namedArguments = null)
        {
            _positionalArguments = positionalArguments == null
                ? Array.Empty<object>()
                : new ReadOnlyCollection<object>(new List<object>(positionalArguments));
            _namedArguments = namedArguments == null
                ? new ReadOnlyDictionary<string, object>(new Dictionary<string, object>(StringComparer.Ordinal))
                : new ReadOnlyDictionary<string, object>(new Dictionary<string, object>(namedArguments, StringComparer.Ordinal));
        }

        public static LocalizationArguments Empty => new();

        public IReadOnlyList<object> PositionalArguments => _positionalArguments ?? Array.Empty<object>();

        public IReadOnlyDictionary<string, object> NamedArguments => _namedArguments
            ?? new ReadOnlyDictionary<string, object>(new Dictionary<string, object>(StringComparer.Ordinal));
    }
}
