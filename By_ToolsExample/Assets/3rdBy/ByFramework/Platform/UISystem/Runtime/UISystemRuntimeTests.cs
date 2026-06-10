//=====================================================
// 文件名称: UISystemRuntimeTests.cs
// 创建者: Codex
// 创建日期: 2026-06-09
// 描    述: 覆盖 UISystem Runtime 的冻结一致性行为测试。
//=====================================================

#if UNITY_INCLUDE_TESTS
namespace _3rdBy.ByFramework.Platform.UISystem
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using _3rdBy.ByFramework.Platform.FrameworkConfig;
    using _3rdBy.ByFramework.Platform.ResourceSystem;
    using _3rdBy.ByFramework.Platform.SaveSystem;
    using NUnit.Framework;
    using UnityObject = UnityEngine.Object;

    public sealed class UISystemRuntimeTests
    {
        [Test]
        public void UIContext_RemainsImmutableAfterOpen()
        {
            UIService service = new(CreateConfigService(CreateRuntimeConfig()), null, new StubResourceService());

            UIContext previous = service.GetCurrentContext();
            UIOperationResult result = service.Open(new UIKey("ui.settings"));
            UIContext current = service.GetCurrentContext();

            Assert.That(result.Success, Is.True);
            Assert.That(previous.OpenViews.Count, Is.EqualTo(0));
            Assert.That(current.OpenViews.Count, Is.EqualTo(1));
            Assert.That(current.OpenViews[0].UIKey, Is.EqualTo(new UIKey("ui.settings")));
            Assert.That(current.Version, Is.EqualTo(previous.Version + 1));
        }

        [Test]
        public void Open_PersistenceFailure_KeepsOldContextAndDoesNotPublishEvent()
        {
            StubSaveService saveService = new() { SaveShouldSucceed = false };
            UIService service = new(CreateConfigService(CreateRuntimeConfig()), saveService, new StubResourceService());
            UIContext before = service.GetCurrentContext();
            int eventCount = 0;
            service.ContextChanged += _ => eventCount++;

            UIOperationResult result = service.Open(new UIKey("ui.settings"));
            UIContext after = service.GetCurrentContext();

            Assert.That(result.Success, Is.False);
            Assert.That(result.StatusCode, Is.EqualTo((int)UIOpenStatus.ProviderFailure));
            Assert.That(after.Version, Is.EqualTo(before.Version));
            Assert.That(after.OpenViews.Count, Is.EqualTo(before.OpenViews.Count));
            Assert.That(eventCount, Is.EqualTo(0));
        }

        [Test]
        public void ContextChanged_PublishesOnlyAfterSuccessfulOpenCommit()
        {
            UIService service = new(CreateConfigService(CreateRuntimeConfig()), null, new StubResourceService());
            UIContext observedContext = null;
            UIContextChangedEvent observedEvent = default;
            service.ContextChanged += changedEvent =>
            {
                observedEvent = changedEvent;
                observedContext = service.GetCurrentContext();
            };

            UIOperationResult result = service.Open(new UIKey("ui.settings"));

            Assert.That(result.Success, Is.True);
            Assert.That(observedContext, Is.Not.Null);
            Assert.That(observedContext.Version, Is.EqualTo(1));
            Assert.That(observedContext.OpenViews.Count, Is.EqualTo(1));
            Assert.That(observedEvent.PreviousVersion, Is.EqualTo(0));
            Assert.That(observedEvent.CurrentVersion, Is.EqualTo(1));
        }

        [Test]
        public void Close_SuccessfulCommit_PublishesContextChanged()
        {
            UIService service = new(CreateConfigService(CreateRuntimeConfig()), null, new StubResourceService());
            service.Open(new UIKey("ui.settings"));
            int eventCount = 0;
            service.ContextChanged += _ => eventCount++;

            UIOperationResult result = service.Close(new UIKey("ui.settings"));
            UIContext current = service.GetCurrentContext();

            Assert.That(result.Success, Is.True);
            Assert.That(result.StatusCode, Is.EqualTo((int)UICloseStatus.Success));
            Assert.That(current.OpenViews.Count, Is.EqualTo(0));
            Assert.That(current.Version, Is.EqualTo(2));
            Assert.That(eventCount, Is.EqualTo(1));
        }

        [Test]
        public void SavePreference_OverridesDefaultDuringStartup()
        {
            StubSaveService saveService = new();
            saveService.StoredPreference = new StubSaveService.StoredUIPreference
            {
                LastOpenedView = "ui.settings",
                RootPreference = "ui.root.secondary",
            };

            UIService service = new(CreateConfigService(CreateRuntimeConfig()), saveService, new StubResourceService());
            UIContext current = service.GetCurrentContext();

            Assert.That(current.OpenViews.Count, Is.EqualTo(1));
            Assert.That(current.OpenViews[0].UIKey, Is.EqualTo(new UIKey("ui.settings")));
            Assert.That(current.OpenViews[0].RootId, Is.EqualTo(new UIRootId("ui.root.secondary")));
        }

        private static StubFrameworkConfigService CreateConfigService(UIRuntimeConfig config)
        {
            return new StubFrameworkConfigService(config);
        }

        private static UIRuntimeConfig CreateRuntimeConfig()
        {
            UIRuntimeConfig config = UIRuntimeConfig.CreateDefault();
            config.defaultUIRootId = "ui.root.default";
            config.defaultUILayerId = "ui.layer.main";
            config.registeredRoots = new List<string>
            {
                "ui.root.default",
                "ui.root.secondary",
            };
            config.availableLayers = new List<string>
            {
                "ui.layer.main",
                "ui.layer.popup",
            };
            config.views = new List<UIRuntimeConfig.UIViewConfig>
            {
                new()
                {
                    uiKey = "ui.main_menu",
                    resourceKey = "ui.main_menu",
                    defaultRootId = "ui.root.default",
                    defaultLayerId = "ui.layer.main",
                },
                new()
                {
                    uiKey = "ui.settings",
                    resourceKey = "ui.settings",
                    defaultRootId = "ui.root.default",
                    defaultLayerId = "ui.layer.popup",
                },
            };
            return config;
        }

        private sealed class StubFrameworkConfigService : IFrameworkConfigService
        {
            private readonly UIRuntimeConfig _config;

            public StubFrameworkConfigService(UIRuntimeConfig config)
            {
                _config = config;
            }

            public FrameworkConfigSnapshot CurrentSnapshot => null;

            public event Action<FrameworkConfigChangedEvent> ConfigChanged
            {
                add { }
                remove { }
            }

            public FrameworkConfigSnapshot Reload()
            {
                return null;
            }

            public TConfig GetModuleConfig<TConfig>() where TConfig : class
            {
                if (TryGetModuleConfig(out TConfig config))
                {
                    return config;
                }

                throw new InvalidOperationException();
            }

            public bool TryGetModuleConfig<TConfig>(out TConfig config) where TConfig : class
            {
                config = _config as TConfig;
                return config != null;
            }
        }

        private sealed class StubSaveService : ISaveService
        {
            public bool SaveShouldSucceed { get; set; } = true;

            public StoredUIPreference StoredPreference { get; set; }

            public SaveResult Save<TData>(SaveRequest<TData> request)
            {
                TrySave(request, out SaveResult result);
                return result;
            }

            public bool TrySave<TData>(SaveRequest<TData> request, out SaveResult result)
            {
                if (!SaveShouldSucceed)
                {
                    result = new SaveResult(false, request.SaveKey, request.Scope, request.ProfileName, string.Empty, "Stub");
                    return false;
                }

                if (request.Data is object data)
                {
                    Type type = data.GetType();
                    StoredPreference = new StoredUIPreference
                    {
                        LastOpenedView = Convert.ToString(type.GetField("userLastOpenedView")?.GetValue(data))
                            ?? Convert.ToString(type.GetField("lastOpenedView")?.GetValue(data))
                            ?? string.Empty,
                        RootPreference = Convert.ToString(type.GetField("userUIRootPreference")?.GetValue(data)) ?? string.Empty,
                    };
                }

                result = new SaveResult(true, request.SaveKey, request.Scope, request.ProfileName, string.Empty, "Stub");
                return true;
            }

            public LoadResult<TData> Load<TData>(SaveRequest request)
            {
                TryLoad(request, out TData data, out LoadResult<TData> result);
                return result;
            }

            public bool TryLoad<TData>(SaveRequest request, out TData data, out LoadResult<TData> result)
            {
                if (StoredPreference == null)
                {
                    data = default;
                    result = new LoadResult<TData>(false, data, request.SaveKey, request.Scope, request.ProfileName, string.Empty, "Stub");
                    return false;
                }

                TData boxed = CreatePreferencePayload<TData>(StoredPreference);
                data = boxed;
                result = new LoadResult<TData>(true, boxed, request.SaveKey, request.Scope, request.ProfileName, string.Empty, "Stub");
                return true;
            }

            public SaveResult Delete(SaveRequest request)
            {
                StoredPreference = null;
                return new SaveResult(true, request.SaveKey, request.Scope, request.ProfileName, string.Empty, "Stub");
            }

            public bool Exists(SaveRequest request)
            {
                return StoredPreference != null;
            }

            public IReadOnlyList<SaveEntryInfo> GetEntries(SaveScope? scope = null, string profileName = null)
            {
                return Array.Empty<SaveEntryInfo>();
            }

            public SaveBackupInfo Backup(SaveRequest request)
            {
                throw new NotSupportedException();
            }

            public SaveResult Restore(SaveRequest request, string backupId)
            {
                throw new NotSupportedException();
            }

            private static TData CreatePreferencePayload<TData>(StoredUIPreference preference)
            {
                object instance = Activator.CreateInstance(typeof(TData), true);
                Type type = instance.GetType();
                type.GetField("userLastOpenedView")?.SetValue(instance, preference.LastOpenedView);
                type.GetField("userUIRootPreference")?.SetValue(instance, preference.RootPreference);
                type.GetField("lastOpenedView")?.SetValue(instance, preference.LastOpenedView);
                return (TData)instance;
            }

            public sealed class StoredUIPreference
            {
                public string LastOpenedView { get; set; }

                public string RootPreference { get; set; }
            }
        }

        private sealed class StubResourceService : IResourceService
        {
            private readonly HashSet<string> _resources = new(StringComparer.Ordinal)
            {
                "ui.main_menu",
                "ui.settings",
            };

            public IResourceLocator Locator => null;

            public ResourceManifestSnapshot ManifestSnapshot => null;

            public ResourceResult<TAsset> Load<TAsset>(ResourceRequest request) where TAsset : UnityObject
            {
                throw new NotSupportedException();
            }

            public bool TryLoad<TAsset>(ResourceRequest request, out ResourceResult<TAsset> result) where TAsset : UnityObject
            {
                result = null;
                return false;
            }

            public Task<ResourceResult<TAsset>> LoadAsync<TAsset>(ResourceRequest request, CancellationToken cancellationToken = default)
                where TAsset : UnityObject
            {
                throw new NotSupportedException();
            }

            public void Release(ResourceHandle handle)
            {
            }

            public bool Exists(string resourceKey)
            {
                return _resources.Contains(resourceKey ?? string.Empty);
            }

            public bool Exists(ResourceRequest request)
            {
                return request != null && Exists(request.ResourceKey);
            }

            public ResourceLocation GetLocation(string resourceKey)
            {
                throw new NotSupportedException();
            }

            public bool TryGetLocation(string resourceKey, out ResourceLocation location)
            {
                location = null;
                return false;
            }

            public IReadOnlyList<ResourceLocation> GetLocationsByGroup(string group)
            {
                return Array.Empty<ResourceLocation>();
            }

            public IReadOnlyList<string> GetAvailableGroups()
            {
                return Array.Empty<string>();
            }

            public IReadOnlyList<ResourceCacheEntry> GetRuntimeCacheEntries()
            {
                return Array.Empty<ResourceCacheEntry>();
            }

            public void ClearRuntimeCache()
            {
            }
        }
    }
}
#endif
