//=====================================================
// 文件名称: FrameworkConfigService.cs
// 创 建 者: Codex
// 创建日期: 2026-06-08
// 描    述: 实现 FrameworkConfig Runtime 的加载、合并、校验、快照与变更发布能力。
//=====================================================

namespace _3rdBy.ByFramework.Platform.FrameworkConfig
{
    using System;
    using System.Collections.Generic;
    using _3rdBy.Plugins.LitJson;

    /// <summary>
    /// FrameworkConfig Runtime 服务实现。
    /// </summary>
    public sealed class FrameworkConfigService : IFrameworkConfigService
    {
        private readonly object _syncRoot = new();
        private readonly List<IConfigProvider> _providers;
        private readonly List<IConfigValidator> _validators;
        private readonly IConfigMerger _merger;
        private readonly ConfigReloadCapability _reloadCapability;
        private FrameworkConfigSnapshot _currentSnapshot;
        private int _version;

        /// <summary>
        /// 初始化 FrameworkConfig Runtime 服务。
        /// </summary>
        /// <param name="providers">配置来源提供器集合。</param>
        /// <param name="merger">配置合并器。</param>
        /// <param name="validators">配置校验器集合。</param>
        /// <param name="reloadCapability">当前对外暴露的重载能力状态。</param>
        public FrameworkConfigService(
            IReadOnlyList<IConfigProvider> providers = null,
            IConfigMerger merger = null,
            IReadOnlyList<IConfigValidator> validators = null,
            ConfigReloadCapability reloadCapability = ConfigReloadCapability.RestartRequired)
        {
            _providers = providers == null
                ? CreateDefaultProviders()
                : new List<IConfigProvider>(providers);
            _validators = validators == null
                ? CreateDefaultValidators()
                : new List<IConfigValidator>(validators);
            _merger = merger ?? new DefaultConfigMerger();
            _reloadCapability = reloadCapability;
        }

        /// <summary>
        /// 当配置快照发生变化时触发。
        /// </summary>
        public event Action<FrameworkConfigChangedEvent> ConfigChanged;

        /// <summary>
        /// 获取当前配置快照。
        /// </summary>
        public FrameworkConfigSnapshot CurrentSnapshot
        {
            get
            {
                lock (_syncRoot)
                {
                    _currentSnapshot ??= BuildSnapshot();
                    return _currentSnapshot;
                }
            }
        }

        /// <summary>
        /// 强制重新加载全部配置源，并返回最新快照。
        /// </summary>
        /// <returns>最新的只读配置快照。</returns>
        public FrameworkConfigSnapshot Reload()
        {
            FrameworkConfigChangedEvent changedEvent = null;
            FrameworkConfigSnapshot snapshot;

            lock (_syncRoot)
            {
                FrameworkConfigSnapshot previousSnapshot = _currentSnapshot;
                snapshot = BuildSnapshot();
                _currentSnapshot = snapshot;

                if (HasSnapshotChanged(previousSnapshot, snapshot))
                {
                    changedEvent = new FrameworkConfigChangedEvent(
                        previousSnapshot,
                        snapshot,
                        ResolveChangedSources(previousSnapshot, snapshot),
                        "[FrameworkConfig] Config snapshot updated.",
                        DateTime.UtcNow);
                }
            }

            if (changedEvent != null)
            {
                ConfigChanged?.Invoke(changedEvent);
            }

            return snapshot;
        }

        /// <summary>
        /// 强制获取指定模块配置。
        /// </summary>
        /// <typeparam name="TConfig">模块配置类型。</typeparam>
        /// <returns>已解析的模块配置对象。</returns>
        /// <exception cref="InvalidOperationException">当模块配置不存在或无法解析时抛出。</exception>
        public TConfig GetModuleConfig<TConfig>() where TConfig : class
        {
            return CurrentSnapshot.GetModuleConfig<TConfig>();
        }

        /// <summary>
        /// 安全获取指定模块配置。
        /// </summary>
        /// <typeparam name="TConfig">模块配置类型。</typeparam>
        /// <param name="config">输出的模块配置对象。</param>
        /// <returns>获取成功返回 true，否则返回 false。</returns>
        public bool TryGetModuleConfig<TConfig>(out TConfig config) where TConfig : class
        {
            return CurrentSnapshot.TryGetModuleConfig(out config);
        }

        private FrameworkConfigSnapshot BuildSnapshot()
        {
            List<ConfigLayer> layers = new();
            List<ConfigValidationMessage> validationMessages = new();

            for (int index = 0; index < _providers.Count; index++)
            {
                IConfigProvider provider = _providers[index];
                if (provider == null)
                {
                    validationMessages.Add(new ConfigValidationMessage(
                        "Warning",
                        "[FrameworkConfig] Empty config provider detected and skipped.",
                        "FrameworkConfigService"));
                    continue;
                }

                try
                {
                    if (provider.TryLoad(out ConfigLayer layer, out ConfigValidationMessage validationMessage))
                    {
                        if (layer != null)
                        {
                            layers.Add(layer);
                        }
                    }

                    if (validationMessage != null)
                    {
                        validationMessages.Add(validationMessage);
                    }
                }
                catch (Exception exception)
                {
                    validationMessages.Add(new ConfigValidationMessage(
                        "Error",
                        $"[FrameworkConfig] 配置提供器执行失败：{exception.Message}",
                        provider.Description));
                }
            }

            layers.Sort(static (left, right) => left.Priority.CompareTo(right.Priority));

            string mergedJson;
            try
            {
                mergedJson = _merger.Merge(layers);
            }
            catch (Exception exception)
            {
                validationMessages.Add(new ConfigValidationMessage(
                    "Fatal",
                    $"[FrameworkConfig] 配置合并失败：{exception.Message}",
                    "FrameworkConfigService"));
                mergedJson = "{}";
            }

            FrameworkConfigSnapshot draftSnapshot = new(
                ++_version,
                mergedJson,
                layers,
                validationMessages,
                _reloadCapability,
                DateTime.UtcNow);

            List<ConfigValidationMessage> finalMessages = new(validationMessages);
            for (int index = 0; index < _validators.Count; index++)
            {
                IConfigValidator validator = _validators[index];
                if (validator == null)
                {
                    continue;
                }

                try
                {
                    validator.Validate(draftSnapshot, finalMessages);
                }
                catch (Exception exception)
                {
                    finalMessages.Add(new ConfigValidationMessage(
                        "Error",
                        $"[FrameworkConfig] 配置校验器执行失败：{exception.Message}",
                        validator.GetType().FullName));
                }
            }

            return new FrameworkConfigSnapshot(
                draftSnapshot.Version,
                draftSnapshot.MergedJson,
                draftSnapshot.Layers,
                finalMessages,
                draftSnapshot.ReloadCapability,
                draftSnapshot.CreatedTimeUtc);
        }

        private static IReadOnlyList<ConfigSource> ResolveChangedSources(
            FrameworkConfigSnapshot previousSnapshot,
            FrameworkConfigSnapshot currentSnapshot)
        {
            if (previousSnapshot == null)
            {
                List<ConfigSource> initialSources = new();
                for (int index = 0; index < currentSnapshot.Layers.Count; index++)
                {
                    initialSources.Add(currentSnapshot.Layers[index].Source);
                }

                return initialSources;
            }

            Dictionary<ConfigSource, string> previousLayers = new();
            for (int index = 0; index < previousSnapshot.Layers.Count; index++)
            {
                previousLayers[previousSnapshot.Layers[index].Source] = previousSnapshot.Layers[index].RawJson;
            }

            List<ConfigSource> changedSources = new();
            for (int index = 0; index < currentSnapshot.Layers.Count; index++)
            {
                ConfigLayer layer = currentSnapshot.Layers[index];
                if (!previousLayers.TryGetValue(layer.Source, out string previousJson)
                    || !string.Equals(previousJson, layer.RawJson, StringComparison.Ordinal))
                {
                    changedSources.Add(layer.Source);
                }
            }

            for (int index = 0; index < previousSnapshot.Layers.Count; index++)
            {
                ConfigSource source = previousSnapshot.Layers[index].Source;
                bool existsInCurrent = false;
                for (int currentIndex = 0; currentIndex < currentSnapshot.Layers.Count; currentIndex++)
                {
                    if (currentSnapshot.Layers[currentIndex].Source == source)
                    {
                        existsInCurrent = true;
                        break;
                    }
                }

                if (!existsInCurrent && !changedSources.Contains(source))
                {
                    changedSources.Add(source);
                }
            }

            return changedSources;
        }

        private static bool HasSnapshotChanged(
            FrameworkConfigSnapshot previousSnapshot,
            FrameworkConfigSnapshot currentSnapshot)
        {
            if (previousSnapshot == null)
            {
                return true;
            }

            if (!string.Equals(previousSnapshot.MergedJson, currentSnapshot.MergedJson, StringComparison.Ordinal))
            {
                return true;
            }

            if (previousSnapshot.ReloadCapability != currentSnapshot.ReloadCapability)
            {
                return true;
            }

            if (previousSnapshot.ValidationMessages.Count != currentSnapshot.ValidationMessages.Count
                || previousSnapshot.Layers.Count != currentSnapshot.Layers.Count)
            {
                return true;
            }

            for (int index = 0; index < previousSnapshot.ValidationMessages.Count; index++)
            {
                ConfigValidationMessage previousMessage = previousSnapshot.ValidationMessages[index];
                ConfigValidationMessage currentMessage = currentSnapshot.ValidationMessages[index];
                if (!string.Equals(previousMessage.Severity, currentMessage.Severity, StringComparison.Ordinal)
                    || !string.Equals(previousMessage.Message, currentMessage.Message, StringComparison.Ordinal)
                    || !string.Equals(previousMessage.Source, currentMessage.Source, StringComparison.Ordinal)
                    || !string.Equals(previousMessage.Path, currentMessage.Path, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            for (int index = 0; index < previousSnapshot.Layers.Count; index++)
            {
                ConfigLayer previousLayer = previousSnapshot.Layers[index];
                ConfigLayer currentLayer = currentSnapshot.Layers[index];
                if (previousLayer.Source != currentLayer.Source
                    || previousLayer.Priority != currentLayer.Priority
                    || !string.Equals(previousLayer.Identifier, currentLayer.Identifier, StringComparison.Ordinal)
                    || !string.Equals(previousLayer.RawJson, currentLayer.RawJson, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        private static List<IConfigProvider> CreateDefaultProviders()
        {
            return new List<IConfigProvider>
            {
                new DefaultConfigProvider(),
                new StreamingAssetsConfigProvider(),
                new PersistentConfigProvider(),
                new CommandLineConfigProvider(),
            };
        }

        private static List<IConfigValidator> CreateDefaultValidators()
        {
            return new List<IConfigValidator>
            {
                new DefaultConfigValidator(),
            };
        }

        private sealed class DefaultConfigMerger : IConfigMerger
        {
            public string Merge(IReadOnlyList<ConfigLayer> layers)
            {
                JsonData mergedData = FrameworkConfigJsonUtility.CreateObject();
                for (int index = 0; index < layers.Count; index++)
                {
                    ConfigLayer layer = layers[index];
                    JsonData layerData = FrameworkConfigJsonUtility.Parse(layer.RawJson);
                    mergedData = FrameworkConfigJsonUtility.DeepMerge(mergedData, layerData);
                }

                return JsonMapper.ToJson(mergedData);
            }
        }

        private sealed class DefaultConfigValidator : IConfigValidator
        {
            public void Validate(FrameworkConfigSnapshot snapshot, List<ConfigValidationMessage> messages)
            {
                if (snapshot == null)
                {
                    throw new ArgumentNullException(nameof(snapshot));
                }

                if (messages == null)
                {
                    throw new ArgumentNullException(nameof(messages));
                }

                if (snapshot.Layers.Count == 0)
                {
                    messages.Add(new ConfigValidationMessage(
                        "Warning",
                        "[FrameworkConfig] No config layers were loaded. Empty snapshot will be used.",
                        "FrameworkConfigService"));
                }

                try
                {
                    JsonData root = FrameworkConfigJsonUtility.Parse(snapshot.MergedJson);
                    if (!root.ContainsKey("framework"))
                    {
                        messages.Add(new ConfigValidationMessage(
                            "Info",
                            "[FrameworkConfig] Current snapshot does not contain the framework node.",
                            "FrameworkConfigService",
                            "framework"));
                    }

                    if (!root.ContainsKey("modules"))
                    {
                        messages.Add(new ConfigValidationMessage(
                            "Info",
                            "[FrameworkConfig] Current snapshot does not contain the modules node.",
                            "FrameworkConfigService",
                            "modules"));
                    }
                }
                catch (Exception exception)
                {
                    messages.Add(new ConfigValidationMessage(
                        "Fatal",
                        $"[FrameworkConfig] 快照 JSON 非法：{exception.Message}",
                        "FrameworkConfigService"));
                }
            }
        }
    }
}
