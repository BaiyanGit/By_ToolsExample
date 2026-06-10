//=====================================================
// 文件名称: AssetBundleDependencyGraph.cs
// 创建作者: Codex
// 创建日期: 2026-06-09
// 描    述: 定义 AssetBundle Bundle 依赖关系模型。
//=====================================================

namespace _3rdBy.ByFramework.Platform.ResourceSystem.AssetBundleSystem
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    /// <summary>
    /// AssetBundle Bundle 依赖关系模型。
    /// </summary>
    public sealed class AssetBundleDependencyGraph
    {
        private readonly IReadOnlyDictionary<string, IReadOnlyList<string>> _directDependencies;
        private readonly IReadOnlyDictionary<string, IReadOnlyList<string>> _dependents;

        /// <summary>
        /// 初始化依赖图。
        /// </summary>
        public AssetBundleDependencyGraph(IReadOnlyList<AssetBundleManifestEntry> entries)
        {
            Dictionary<string, List<string>> dependencies = new(StringComparer.Ordinal);
            Dictionary<string, List<string>> dependents = new(StringComparer.Ordinal);

            if (entries != null)
            {
                for (int index = 0; index < entries.Count; index++)
                {
                    AssetBundleManifestEntry entry = entries[index];
                    if (entry == null)
                    {
                        continue;
                    }

                    if (!dependencies.TryGetValue(entry.BundleName, out List<string> directList))
                    {
                        directList = new List<string>();
                        dependencies[entry.BundleName] = directList;
                    }

                    for (int dependencyIndex = 0; dependencyIndex < entry.DependencyBundleNames.Count; dependencyIndex++)
                    {
                        string dependencyName = entry.DependencyBundleNames[dependencyIndex];
                        if (string.IsNullOrWhiteSpace(dependencyName))
                        {
                            continue;
                        }

                        if (!directList.Contains(dependencyName))
                        {
                            directList.Add(dependencyName);
                        }

                        if (!dependents.TryGetValue(dependencyName, out List<string> dependentList))
                        {
                            dependentList = new List<string>();
                            dependents[dependencyName] = dependentList;
                        }

                        if (!dependentList.Contains(entry.BundleName))
                        {
                            dependentList.Add(entry.BundleName);
                        }
                    }
                }
            }

            _directDependencies = CreateReadonlyMap(dependencies);
            _dependents = CreateReadonlyMap(dependents);
        }

        /// <summary>
        /// 获取直接依赖。
        /// </summary>
        public IReadOnlyList<string> GetDirectDependencies(string bundleName)
        {
            if (string.IsNullOrWhiteSpace(bundleName))
            {
                return Array.Empty<string>();
            }

            return _directDependencies.TryGetValue(bundleName, out IReadOnlyList<string> dependencies)
                ? dependencies
                : Array.Empty<string>();
        }

        /// <summary>
        /// 获取全部依赖。
        /// </summary>
        public IReadOnlyList<string> GetAllDependencies(string bundleName)
        {
            if (string.IsNullOrWhiteSpace(bundleName))
            {
                return Array.Empty<string>();
            }

            HashSet<string> visited = new(StringComparer.Ordinal);
            List<string> ordered = new();
            CollectDependencies(bundleName, visited, ordered);
            return new ReadOnlyCollection<string>(ordered);
        }

        /// <summary>
        /// 获取依赖当前 Bundle 的上游集合。
        /// </summary>
        public IReadOnlyList<string> GetDependents(string bundleName)
        {
            if (string.IsNullOrWhiteSpace(bundleName))
            {
                return Array.Empty<string>();
            }

            return _dependents.TryGetValue(bundleName, out IReadOnlyList<string> dependents)
                ? dependents
                : Array.Empty<string>();
        }

        /// <summary>
        /// 判断依赖图是否存在循环。
        /// </summary>
        public bool ContainsCycle()
        {
            HashSet<string> visited = new(StringComparer.Ordinal);
            HashSet<string> stack = new(StringComparer.Ordinal);

            foreach (KeyValuePair<string, IReadOnlyList<string>> pair in _directDependencies)
            {
                if (ContainsCycle(pair.Key, visited, stack))
                {
                    return true;
                }
            }

            return false;
        }

        private static IReadOnlyDictionary<string, IReadOnlyList<string>> CreateReadonlyMap(Dictionary<string, List<string>> source)
        {
            Dictionary<string, IReadOnlyList<string>> map = new(StringComparer.Ordinal);
            foreach (KeyValuePair<string, List<string>> pair in source)
            {
                map[pair.Key] = new ReadOnlyCollection<string>(pair.Value);
            }

            return new ReadOnlyDictionary<string, IReadOnlyList<string>>(map);
        }

        private void CollectDependencies(string bundleName, HashSet<string> visited, List<string> ordered)
        {
            IReadOnlyList<string> directDependencies = GetDirectDependencies(bundleName);
            for (int index = 0; index < directDependencies.Count; index++)
            {
                string dependencyName = directDependencies[index];
                if (!visited.Add(dependencyName))
                {
                    continue;
                }

                CollectDependencies(dependencyName, visited, ordered);
                ordered.Add(dependencyName);
            }
        }

        private bool ContainsCycle(string bundleName, HashSet<string> visited, HashSet<string> stack)
        {
            if (stack.Contains(bundleName))
            {
                return true;
            }

            if (!visited.Add(bundleName))
            {
                return false;
            }

            stack.Add(bundleName);
            IReadOnlyList<string> dependencies = GetDirectDependencies(bundleName);
            for (int index = 0; index < dependencies.Count; index++)
            {
                if (ContainsCycle(dependencies[index], visited, stack))
                {
                    return true;
                }
            }

            stack.Remove(bundleName);
            return false;
        }
    }
}
