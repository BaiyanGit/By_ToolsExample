//=====================================================
// 文件名称: AssetBundleService.cs
// 创建作者: Codex
// 创建日期: 2026-06-09
// 描    述: 实现 AssetBundle Runtime 服务。
//=====================================================

namespace _3rdBy.ByFramework.Platform.ResourceSystem.AssetBundleSystem
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using UnityEngine;
    using UnityObject = UnityEngine.Object;

    /// <summary>
    /// AssetBundle Runtime 服务。
    /// </summary>
    public sealed class AssetBundleService : IAssetBundleService
    {
        private readonly object _syncRoot = new();
        private readonly Dictionary<string, LoadedBundleState> _loadedBundles;
        private readonly Dictionary<string, int> _assetReferenceCounts;

        /// <summary>
        /// 初始化 AssetBundle Runtime 服务。
        /// </summary>
        public AssetBundleService(
            IAssetBundleManifest manifest = null,
            IAssetBundleLocator locator = null)
        {
            Manifest = manifest ?? new AssetBundleManifest(AssetBundleManifestSnapshot.Empty);
            Locator = locator ?? new AssetBundleLocator(Manifest);
            DependencyGraph = new AssetBundleDependencyGraph(Manifest.GetAllBundleEntries());
            if (DependencyGraph.ContainsCycle())
            {
                throw new InvalidOperationException("[AssetBundle] Manifest 中存在循环依赖，无法初始化 Runtime 服务。");
            }

            _loadedBundles = new Dictionary<string, LoadedBundleState>(StringComparer.Ordinal);
            _assetReferenceCounts = new Dictionary<string, int>(StringComparer.Ordinal);
        }

        /// <summary>
        /// 获取 Manifest 访问对象。
        /// </summary>
        public IAssetBundleManifest Manifest { get; }

        /// <summary>
        /// 获取 Bundle 定位器。
        /// </summary>
        public IAssetBundleLocator Locator { get; }

        /// <summary>
        /// 获取依赖图。
        /// </summary>
        public AssetBundleDependencyGraph DependencyGraph { get; }

        /// <summary>
        /// 强制加载 Bundle。
        /// </summary>
        public AssetBundle LoadBundle(string bundleName)
        {
            if (TryLoadBundle(bundleName, out AssetBundle bundle))
            {
                return bundle;
            }

            throw new InvalidOperationException($"[AssetBundle] Bundle 加载失败：{bundleName}");
        }

        /// <summary>
        /// 安全加载 Bundle。
        /// </summary>
        public bool TryLoadBundle(string bundleName, out AssetBundle bundle)
        {
            if (string.IsNullOrWhiteSpace(bundleName))
            {
                throw new ArgumentException("[AssetBundle] BundleName 不能为空。", nameof(bundleName));
            }

            lock (_syncRoot)
            {
                return TryLoadBundleInternal(bundleName, out bundle);
            }
        }

        /// <summary>
        /// 卸载 Bundle。
        /// </summary>
        public void UnloadBundle(string bundleName, bool unloadAllLoadedObjects = false)
        {
            if (string.IsNullOrWhiteSpace(bundleName))
            {
                throw new ArgumentException("[AssetBundle] BundleName 不能为空。", nameof(bundleName));
            }

            lock (_syncRoot)
            {
                ReleaseBundleReference(bundleName, unloadAllLoadedObjects);
            }
        }

        /// <summary>
        /// 判断 Bundle 是否已加载。
        /// </summary>
        public bool IsBundleLoaded(string bundleName)
        {
            if (string.IsNullOrWhiteSpace(bundleName))
            {
                throw new ArgumentException("[AssetBundle] BundleName 不能为空。", nameof(bundleName));
            }

            lock (_syncRoot)
            {
                return _loadedBundles.ContainsKey(bundleName);
            }
        }

        /// <summary>
        /// 判断 Manifest 中是否包含指定 Bundle。
        /// </summary>
        public bool ContainsBundle(string bundleName)
        {
            return Manifest.TryGetBundleEntry(bundleName, out _);
        }

        /// <summary>
        /// 强制加载 Bundle 内资源。
        /// </summary>
        public TAsset LoadAsset<TAsset>(string bundleName, string assetName) where TAsset : UnityObject
        {
            if (TryLoadAsset(bundleName, assetName, out TAsset asset))
            {
                return asset;
            }

            throw new InvalidOperationException($"[AssetBundle] 资源加载失败：{bundleName}/{assetName}");
        }

        /// <summary>
        /// 安全加载 Bundle 内资源。
        /// </summary>
        public bool TryLoadAsset<TAsset>(string bundleName, string assetName, out TAsset asset) where TAsset : UnityObject
        {
            if (string.IsNullOrWhiteSpace(bundleName))
            {
                throw new ArgumentException("[AssetBundle] BundleName 不能为空。", nameof(bundleName));
            }

            if (string.IsNullOrWhiteSpace(assetName))
            {
                throw new ArgumentException("[AssetBundle] AssetName 不能为空。", nameof(assetName));
            }

            lock (_syncRoot)
            {
                if (!TryLoadBundleInternal(bundleName, out AssetBundle bundle))
                {
                    asset = null;
                    return false;
                }

                TAsset loadedAsset = bundle.LoadAsset<TAsset>(assetName);
                if (loadedAsset == null)
                {
                    asset = null;
                    ReleaseBundleReference(bundleName, false);
                    return false;
                }

                string token = CreateAssetReferenceToken(bundleName, assetName);
                if (_assetReferenceCounts.TryGetValue(token, out int count))
                {
                    _assetReferenceCounts[token] = count + 1;
                }
                else
                {
                    _assetReferenceCounts[token] = 1;
                }

                asset = loadedAsset;
                return true;
            }
        }

        /// <summary>
        /// 获取 Bundle 版本信息。
        /// </summary>
        public AssetBundleVersionInfo GetBundleInfo(string bundleName)
        {
            return Manifest.GetVersionInfo(bundleName);
        }

        /// <summary>
        /// 获取全部 Bundle 版本信息。
        /// </summary>
        public IReadOnlyList<AssetBundleVersionInfo> GetAllBundleInfos()
        {
            List<AssetBundleVersionInfo> infos = new();
            IReadOnlyList<AssetBundleManifestEntry> entries = Manifest.GetAllBundleEntries();
            for (int index = 0; index < entries.Count; index++)
            {
                infos.Add(Manifest.GetVersionInfo(entries[index].BundleName));
            }

            return new ReadOnlyCollection<AssetBundleVersionInfo>(infos);
        }

        /// <summary>
        /// 获取依赖集合。
        /// </summary>
        public IReadOnlyList<string> GetDependencies(string bundleName, bool includeTransitive = true)
        {
            return includeTransitive
                ? DependencyGraph.GetAllDependencies(bundleName)
                : DependencyGraph.GetDirectDependencies(bundleName);
        }

        /// <summary>
        /// 获取 Manifest 快照。
        /// </summary>
        public AssetBundleManifestSnapshot GetManifestSnapshot()
        {
            return Manifest.Snapshot;
        }

        /// <summary>
        /// 释放资源对应的 Bundle 引用。
        /// </summary>
        public void ReleaseAssetReference(string bundleName, string assetName, bool unloadAllLoadedObjects = false)
        {
            if (string.IsNullOrWhiteSpace(bundleName))
            {
                throw new ArgumentException("[AssetBundle] BundleName 不能为空。", nameof(bundleName));
            }

            if (string.IsNullOrWhiteSpace(assetName))
            {
                throw new ArgumentException("[AssetBundle] AssetName 不能为空。", nameof(assetName));
            }

            lock (_syncRoot)
            {
                string token = CreateAssetReferenceToken(bundleName, assetName);
                if (_assetReferenceCounts.TryGetValue(token, out int count) && count > 0)
                {
                    if (count == 1)
                    {
                        _assetReferenceCounts.Remove(token);
                    }
                    else
                    {
                        _assetReferenceCounts[token] = count - 1;
                    }

                    ReleaseBundleReference(bundleName, unloadAllLoadedObjects);
                }
            }
        }

        private bool TryLoadBundleInternal(string bundleName, out AssetBundle bundle)
        {
            if (_loadedBundles.TryGetValue(bundleName, out LoadedBundleState state))
            {
                state.RefCount++;
                bundle = state.Bundle;
                return true;
            }

            if (!Locator.TryGetBundleEntry(bundleName, out _))
            {
                bundle = null;
                return false;
            }

            IReadOnlyList<string> directDependencies = DependencyGraph.GetDirectDependencies(bundleName);
            for (int index = 0; index < directDependencies.Count; index++)
            {
                if (!TryLoadBundleInternal(directDependencies[index], out _))
                {
                    bundle = null;
                    return false;
                }
            }

            string bundlePath = Locator.GetBundlePath(bundleName);
            if (!File.Exists(bundlePath))
            {
                ReleaseDependenciesOnFailure(directDependencies);
                bundle = null;
                return false;
            }

            AssetBundle loadedBundle = AssetBundle.LoadFromFile(bundlePath);
            if (loadedBundle == null)
            {
                ReleaseDependenciesOnFailure(directDependencies);
                bundle = null;
                return false;
            }

            _loadedBundles[bundleName] = new LoadedBundleState(loadedBundle, bundlePath, 1);
            bundle = loadedBundle;
            return true;
        }

        private void ReleaseDependenciesOnFailure(IReadOnlyList<string> dependencyBundleNames)
        {
            for (int index = dependencyBundleNames.Count - 1; index >= 0; index--)
            {
                ReleaseBundleReference(dependencyBundleNames[index], false);
            }
        }

        private void ReleaseBundleReference(string bundleName, bool unloadAllLoadedObjects)
        {
            if (!_loadedBundles.TryGetValue(bundleName, out LoadedBundleState state))
            {
                return;
            }

            state.RefCount--;
            if (state.RefCount > 0)
            {
                return;
            }

            state.Bundle.Unload(unloadAllLoadedObjects);
            _loadedBundles.Remove(bundleName);

            IReadOnlyList<string> dependencies = DependencyGraph.GetDirectDependencies(bundleName);
            for (int index = dependencies.Count - 1; index >= 0; index--)
            {
                ReleaseBundleReference(dependencies[index], unloadAllLoadedObjects);
            }
        }

        private static string CreateAssetReferenceToken(string bundleName, string assetName)
        {
            return $"{bundleName}::{assetName}";
        }

        private sealed class LoadedBundleState
        {
            public LoadedBundleState(AssetBundle bundle, string bundlePath, int refCount)
            {
                Bundle = bundle;
                BundlePath = bundlePath;
                RefCount = refCount;
            }

            public AssetBundle Bundle { get; }

            public string BundlePath { get; }

            public int RefCount { get; set; }
        }
    }
}
