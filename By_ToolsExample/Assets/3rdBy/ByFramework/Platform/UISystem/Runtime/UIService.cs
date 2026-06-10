//=====================================================
// 文件名称: UIService.cs
// 创建者: Codex
// 创建日期: 2026-06-09
// 描    述: 实现 UISystem Runtime 的最小 UI 状态管理服务。
//=====================================================

namespace _3rdBy.ByFramework.Platform.UISystem
{
    using System;
    using System.Collections.Generic;
    using _3rdBy.ByFramework.Platform.DisplaySystem;
    using _3rdBy.ByFramework.Platform.FrameworkConfig;
    using _3rdBy.ByFramework.Platform.InputSystem;
    using _3rdBy.ByFramework.Platform.LocalizationSystem;
    using _3rdBy.ByFramework.Platform.ResourceSystem;
    using _3rdBy.ByFramework.Platform.SaveSystem;

    public sealed class UIService : IUIService
    {
        private const string OpenOperationType = "Open";
        private const string CloseOperationType = "Close";
        private const string UserUIPreferenceSaveKey = "platform.ui.preference";
        private readonly object _syncRoot = new();
        private readonly ISaveService _saveService;
        private readonly IResourceService _resourceService;
        private readonly UIRuntimeConfig _config;
        private readonly Dictionary<string, UIRootId> _roots;
        private readonly Dictionary<string, UILayerId> _layers;
        private readonly Dictionary<string, RegisteredView> _registeredViews;
        private readonly IReadOnlyList<UIRootId> _registeredRoots;
        private readonly IReadOnlyList<UILayerId> _availableLayers;
        private UIContext _currentContext;

        public UIService(
            IFrameworkConfigService frameworkConfigService = null,
            ISaveService saveService = null,
            IResourceService resourceService = null,
            IDisplayService displayService = null,
            IInputService inputService = null,
            ILocalizationService localizationService = null)
        {
            _saveService = saveService;
            _resourceService = resourceService;
            _config = LoadRuntimeConfig(frameworkConfigService);
            _roots = BuildRootMap(_config);
            _layers = BuildLayerMap(_config);
            _registeredViews = BuildViewMap(_config, _roots, _layers);
            _registeredRoots = BuildRootList(_roots);
            _availableLayers = BuildLayerList(_layers);
            _currentContext = BuildInitialContext();
            _ = displayService;
            _ = inputService;
            _ = localizationService;
        }

        public event Action<UIContextChangedEvent> ContextChanged;

        public UIContext GetCurrentContext()
        {
            lock (_syncRoot)
            {
                return _currentContext;
            }
        }

        public IReadOnlyList<UIRootId> GetRegisteredRoots()
        {
            return _registeredRoots;
        }

        public IReadOnlyList<UILayerId> GetAvailableLayers()
        {
            return _availableLayers;
        }

        public IReadOnlyList<UIViewDescriptor> GetOpenViews()
        {
            lock (_syncRoot)
            {
                return _currentContext.OpenViews;
            }
        }

        public bool TryGetView(
            UIKey uiKey,
            out UIViewDescriptor view,
            out UIQueryStatus status)
        {
            if (uiKey.IsEmpty)
            {
                throw new ArgumentException("[UISystem] UIKey cannot be empty.", nameof(uiKey));
            }

            lock (_syncRoot)
            {
                for (int index = 0; index < _currentContext.OpenViews.Count; index++)
                {
                    UIViewDescriptor current = _currentContext.OpenViews[index];
                    if (current.UIKey == uiKey)
                    {
                        view = current;
                        status = UIQueryStatus.Success;
                        return true;
                    }
                }

                view = null;
                status = UIQueryStatus.ViewNotFound;
                return false;
            }
        }

        public UIOperationResult Open(UIKey uiKey, UIOpenOptions options = null)
        {
            if (uiKey.IsEmpty)
            {
                throw new ArgumentException("[UISystem] UIKey cannot be empty.", nameof(uiKey));
            }

            UIContextChangedEvent changedEvent = default;
            bool shouldPublish = false;

            lock (_syncRoot)
            {
                UIContext previousContext = _currentContext;
                if (ContainsOpenView(previousContext.OpenViews, uiKey))
                {
                    return CreateResult(
                        false,
                        uiKey,
                        OpenOperationType,
                        previousContext.Version,
                        previousContext.Version,
                        (int)UIOpenStatus.ViewAlreadyOpen);
                }

                if (!_registeredViews.TryGetValue(uiKey.Value, out RegisteredView registeredView)
                    || string.IsNullOrWhiteSpace(registeredView.ResourceKey))
                {
                    return CreateResult(
                        false,
                        uiKey,
                        OpenOperationType,
                        previousContext.Version,
                        previousContext.Version,
                        (int)UIOpenStatus.ResourceUnavailable);
                }

                UIRootId resolvedRoot = ResolveRoot(options, registeredView);
                if (resolvedRoot.IsEmpty || !_roots.ContainsKey(resolvedRoot.Value))
                {
                    return CreateResult(
                        false,
                        uiKey,
                        OpenOperationType,
                        previousContext.Version,
                        previousContext.Version,
                        (int)UIOpenStatus.RootNotFound);
                }

                UILayerId resolvedLayer = ResolveLayer(options, registeredView);
                if (resolvedLayer.IsEmpty || !_layers.ContainsKey(resolvedLayer.Value))
                {
                    return CreateResult(
                        false,
                        uiKey,
                        OpenOperationType,
                        previousContext.Version,
                        previousContext.Version,
                        (int)UIOpenStatus.LayerNotFound);
                }

                if (!IsResourceAvailable(registeredView.ResourceKey))
                {
                    return CreateResult(
                        false,
                        uiKey,
                        OpenOperationType,
                        previousContext.Version,
                        previousContext.Version,
                        (int)UIOpenStatus.ResourceUnavailable);
                }

                List<UIViewDescriptor> openViews = CopyViews(previousContext.OpenViews);
                openViews.Add(new UIViewDescriptor(uiKey, resolvedRoot, resolvedLayer, true));
                SortViews(openViews);
                UIContext nextContext = new(previousContext.RegisteredRoots, previousContext.AvailableLayers, openViews, previousContext.Version + 1);

                if (!TryPersistPreference(uiKey, resolvedRoot))
                {
                    return CreateResult(
                        false,
                        uiKey,
                        OpenOperationType,
                        previousContext.Version,
                        previousContext.Version,
                        (int)UIOpenStatus.ProviderFailure);
                }

                _currentContext = nextContext;
                changedEvent = new UIContextChangedEvent(previousContext.Version, nextContext.Version);
                shouldPublish = true;
            }

            if (shouldPublish)
            {
                ContextChanged?.Invoke(changedEvent);
            }

            return CreateResult(
                true,
                uiKey,
                OpenOperationType,
                changedEvent.PreviousVersion,
                changedEvent.CurrentVersion,
                (int)UIOpenStatus.Success);
        }

        public UIOperationResult Close(UIKey uiKey, UICloseReason reason = UICloseReason.Programmatic)
        {
            if (uiKey.IsEmpty)
            {
                throw new ArgumentException("[UISystem] UIKey cannot be empty.", nameof(uiKey));
            }

            UIContextChangedEvent changedEvent = default;
            bool shouldPublish = false;

            lock (_syncRoot)
            {
                UIContext previousContext = _currentContext;
                List<UIViewDescriptor> openViews = CopyViews(previousContext.OpenViews);
                int viewIndex = IndexOfView(openViews, uiKey);
                if (viewIndex < 0)
                {
                    int statusCode = _registeredViews.ContainsKey(uiKey.Value)
                        ? (int)UICloseStatus.ViewAlreadyClosed
                        : (int)UICloseStatus.ViewNotFound;
                    return CreateResult(
                        false,
                        uiKey,
                        CloseOperationType,
                        previousContext.Version,
                        previousContext.Version,
                        statusCode);
                }

                _ = reason;
                openViews.RemoveAt(viewIndex);
                UIContext nextContext = new(previousContext.RegisteredRoots, previousContext.AvailableLayers, openViews, previousContext.Version + 1);
                _currentContext = nextContext;
                changedEvent = new UIContextChangedEvent(previousContext.Version, nextContext.Version);
                shouldPublish = true;
            }

            if (shouldPublish)
            {
                ContextChanged?.Invoke(changedEvent);
            }

            return CreateResult(
                true,
                uiKey,
                CloseOperationType,
                changedEvent.PreviousVersion,
                changedEvent.CurrentVersion,
                (int)UICloseStatus.Success);
        }

        private UIContext BuildInitialContext()
        {
            List<UIViewDescriptor> openViews = new();
            if (TryRestorePreference(out UIPreferenceData preference)
                && preference != null
                && UIKey.TryParse(preference.lastOpenedView, out UIKey lastOpenedView)
                && _registeredViews.TryGetValue(lastOpenedView.Value, out RegisteredView registeredView)
                && IsResourceAvailable(registeredView.ResourceKey))
            {
                UIRootId rootId = ResolvePersistedRootOrDefault(preference.userUIRootPreference, registeredView);
                openViews.Add(new UIViewDescriptor(lastOpenedView, rootId, registeredView.DefaultLayerId, true));
            }

            return new UIContext(_registeredRoots, _availableLayers, openViews, 0);
        }

        private UIRootId ResolveRoot(UIOpenOptions options, RegisteredView view)
        {
            if (options != null && !options.RootId.IsEmpty)
            {
                return options.RootId;
            }

            return view.DefaultRootId;
        }

        private UILayerId ResolveLayer(UIOpenOptions options, RegisteredView view)
        {
            if (options != null && !options.LayerId.IsEmpty)
            {
                return options.LayerId;
            }

            return view.DefaultLayerId;
        }

        private UIRootId ResolvePersistedRootOrDefault(string persistedRootValue, RegisteredView view)
        {
            return UIRootId.TryParse(persistedRootValue, out UIRootId persistedRoot)
                   && _roots.ContainsKey(persistedRoot.Value)
                ? persistedRoot
                : view.DefaultRootId;
        }

        private bool IsResourceAvailable(string resourceKey)
        {
            if (string.IsNullOrWhiteSpace(resourceKey))
            {
                return false;
            }

            return _resourceService == null || _resourceService.Exists(resourceKey);
        }

        private bool TryRestorePreference(out UIPreferenceData preference)
        {
            preference = null;
            if (_saveService == null)
            {
                return false;
            }

            SaveRequest request = new(UserUIPreferenceSaveKey, SaveScope.User);
            if (!_saveService.TryLoad(request, out UIPreferenceData data, out LoadResult<UIPreferenceData> result)
                || !result.Success
                || data == null)
            {
                return false;
            }

            preference = data;
            return true;
        }

        private bool TryPersistPreference(UIKey uiKey, UIRootId rootId)
        {
            if (_saveService == null)
            {
                return true;
            }

            SaveRequest<UIPreferenceData> request = new(
                UserUIPreferenceSaveKey,
                SaveScope.User,
                new UIPreferenceData
                {
                    userThemePreference = _config.DefaultThemeId,
                    userLastOpenedView = uiKey.Value,
                    userUIRootPreference = rootId.Value,
                    lastOpenedView = uiKey.Value,
                });

            return _saveService.TrySave(request, out SaveResult result) && result.Success;
        }

        private static UIRuntimeConfig LoadRuntimeConfig(IFrameworkConfigService frameworkConfigService)
        {
            if (frameworkConfigService != null
                && frameworkConfigService.TryGetModuleConfig(out UIRuntimeConfig config)
                && config != null)
            {
                return config.Normalize();
            }

            return UIRuntimeConfig.CreateDefault();
        }

        private static Dictionary<string, UIRootId> BuildRootMap(UIRuntimeConfig config)
        {
            Dictionary<string, UIRootId> roots = new(StringComparer.Ordinal);
            IReadOnlyList<string> configuredRoots = config.RegisteredRoots;
            for (int index = 0; index < configuredRoots.Count; index++)
            {
                if (!UIRootId.TryParse(configuredRoots[index], out UIRootId rootId))
                {
                    continue;
                }

                roots[rootId.Value] = rootId;
            }

            if (roots.Count == 0)
            {
                UIRootId defaultRoot = new(config.DefaultUIRootId);
                roots[defaultRoot.Value] = defaultRoot;
            }

            return roots;
        }

        private static Dictionary<string, UILayerId> BuildLayerMap(UIRuntimeConfig config)
        {
            Dictionary<string, UILayerId> layers = new(StringComparer.Ordinal);
            IReadOnlyList<string> configuredLayers = config.AvailableLayers;
            for (int index = 0; index < configuredLayers.Count; index++)
            {
                if (!UILayerId.TryParse(configuredLayers[index], out UILayerId layerId))
                {
                    continue;
                }

                layers[layerId.Value] = layerId;
            }

            if (layers.Count == 0)
            {
                UILayerId defaultLayer = new(config.DefaultUILayerId);
                layers[defaultLayer.Value] = defaultLayer;
            }

            return layers;
        }

        private static Dictionary<string, RegisteredView> BuildViewMap(
            UIRuntimeConfig config,
            IReadOnlyDictionary<string, UIRootId> roots,
            IReadOnlyDictionary<string, UILayerId> layers)
        {
            Dictionary<string, RegisteredView> views = new(StringComparer.Ordinal);
            IReadOnlyList<UIRuntimeConfig.UIViewConfig> configuredViews = config.Views;
            for (int index = 0; index < configuredViews.Count; index++)
            {
                UIRuntimeConfig.UIViewConfig configuredView = configuredViews[index];
                if (!UIKey.TryParse(configuredView.uiKey, out UIKey uiKey))
                {
                    continue;
                }

                UIRootId rootId = UIRootId.TryParse(configuredView.defaultRootId, out UIRootId parsedRoot)
                                  && roots.ContainsKey(parsedRoot.Value)
                    ? parsedRoot
                    : new UIRootId(config.DefaultUIRootId);
                UILayerId layerId = UILayerId.TryParse(configuredView.defaultLayerId, out UILayerId parsedLayer)
                                    && layers.ContainsKey(parsedLayer.Value)
                    ? parsedLayer
                    : new UILayerId(config.DefaultUILayerId);

                views[uiKey.Value] = new RegisteredView(
                    uiKey,
                    configuredView.resourceKey,
                    rootId,
                    layerId);
            }

            return views;
        }

        private static IReadOnlyList<UIRootId> BuildRootList(IReadOnlyDictionary<string, UIRootId> roots)
        {
            List<UIRootId> values = new(roots.Values);
            values.Sort(static (left, right) => string.CompareOrdinal(left.Value, right.Value));
            return values.AsReadOnly();
        }

        private static IReadOnlyList<UILayerId> BuildLayerList(IReadOnlyDictionary<string, UILayerId> layers)
        {
            List<UILayerId> values = new(layers.Values);
            values.Sort(static (left, right) => string.CompareOrdinal(left.Value, right.Value));
            return values.AsReadOnly();
        }

        private static List<UIViewDescriptor> CopyViews(IReadOnlyList<UIViewDescriptor> source)
        {
            List<UIViewDescriptor> copied = new(source.Count);
            for (int index = 0; index < source.Count; index++)
            {
                copied.Add(source[index]);
            }

            return copied;
        }

        private static bool ContainsOpenView(IReadOnlyList<UIViewDescriptor> views, UIKey uiKey)
        {
            return IndexOfView(views, uiKey) >= 0;
        }

        private static int IndexOfView(IReadOnlyList<UIViewDescriptor> views, UIKey uiKey)
        {
            for (int index = 0; index < views.Count; index++)
            {
                if (views[index].UIKey == uiKey)
                {
                    return index;
                }
            }

            return -1;
        }

        private static void SortViews(List<UIViewDescriptor> views)
        {
            views.Sort(static (left, right) => string.CompareOrdinal(left.UIKey.Value, right.UIKey.Value));
        }

        private static UIOperationResult CreateResult(
            bool success,
            UIKey uiKey,
            string operationType,
            int previousVersion,
            int currentVersion,
            int statusCode)
        {
            return new UIOperationResult(
                success,
                uiKey,
                operationType,
                previousVersion,
                currentVersion,
                statusCode);
        }

        private sealed class RegisteredView
        {
            public RegisteredView(
                UIKey uiKey,
                string resourceKey,
                UIRootId defaultRootId,
                UILayerId defaultLayerId)
            {
                UIKey = uiKey;
                ResourceKey = resourceKey ?? string.Empty;
                DefaultRootId = defaultRootId;
                DefaultLayerId = defaultLayerId;
            }

            public UIKey UIKey { get; }

            public string ResourceKey { get; }

            public UIRootId DefaultRootId { get; }

            public UILayerId DefaultLayerId { get; }
        }

        [Serializable]
        private sealed class UIPreferenceData
        {
            public string userThemePreference = string.Empty;
            public string userLastOpenedView = string.Empty;
            public string userUIRootPreference = string.Empty;
            public string lastOpenedView = string.Empty;
        }
    }
}
