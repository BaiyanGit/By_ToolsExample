//=====================================================
// 文件名称: DisplaySystemRuntimeTests.cs
// 创建者: Codex
// 创建日期: 2026-06-09
// 描    述: 覆盖 DisplaySystem Runtime 的冻结一致性行为测试。
//=====================================================

#if UNITY_INCLUDE_TESTS
namespace _3rdBy.ByFramework.Platform.DisplaySystem
{
    using System;
    using System.Collections.Generic;
    using _3rdBy.ByFramework.Platform.FrameworkConfig;
    using _3rdBy.ByFramework.Platform.SaveSystem;
    using NUnit.Framework;

    public sealed class DisplaySystemRuntimeTests
    {
        [Test]
        public void DisplayContext_RemainsImmutableAfterProfileApply()
        {
            DisplayService service = new(CreateConfigService(CreateDualProfileConfig()), null);

            DisplayContext previous = service.GetCurrentDisplayContext();
            DisplayApplyResult result = service.ApplyProfile(new DisplayProfileId("profile.debug"), false);
            DisplayContext current = service.GetCurrentDisplayContext();

            Assert.That(result.Success, Is.True);
            Assert.That(previous.CurrentProfileId, Is.EqualTo(new DisplayProfileId("profile.default")));
            Assert.That(previous.ActiveTargets.Count, Is.EqualTo(1));
            Assert.That(previous.ActiveTargets[0].TargetId, Is.EqualTo(new DisplayTargetId("target.main")));
            Assert.That(current.CurrentProfileId, Is.EqualTo(new DisplayProfileId("profile.debug")));
            Assert.That(current.ActiveTargets.Count, Is.EqualTo(2));
            Assert.That(current.Version, Is.EqualTo(previous.Version + 1));
        }

        [Test]
        public void ApplyProfile_PersistenceFailure_KeepsOldContextAndDoesNotPublishEvent()
        {
            StubSaveService saveService = new() { SaveShouldSucceed = false };
            DisplayService service = new(CreateConfigService(CreateDualProfileConfig()), saveService);
            DisplayContext before = service.GetCurrentDisplayContext();
            int eventCount = 0;
            service.DisplayContextChanged += _ => eventCount++;

            DisplayApplyResult result = service.ApplyProfile(new DisplayProfileId("profile.debug"), true);
            DisplayContext after = service.GetCurrentDisplayContext();

            Assert.That(result.Success, Is.False);
            Assert.That(result.Status, Is.EqualTo(DisplayApplyStatus.PersistenceFailure));
            Assert.That(after.CurrentProfileId, Is.EqualTo(before.CurrentProfileId));
            Assert.That(after.CurrentMode, Is.EqualTo(before.CurrentMode));
            Assert.That(after.Version, Is.EqualTo(before.Version));
            Assert.That(eventCount, Is.EqualTo(0));
        }

        [Test]
        public void DisplayContextChanged_PublishesOnlyAfterSuccessfulCommit()
        {
            DisplayService service = new(CreateConfigService(CreateDualProfileConfig()), null);
            DisplayContext observedContext = null;
            DisplayContextChangedEvent observedEvent = default;
            service.DisplayContextChanged += changedEvent =>
            {
                observedEvent = changedEvent;
                observedContext = service.GetCurrentDisplayContext();
            };

            DisplayApplyResult result = service.ApplyProfile(new DisplayProfileId("profile.debug"), false);

            Assert.That(result.Success, Is.True);
            Assert.That(observedContext, Is.Not.Null);
            Assert.That(observedContext.CurrentProfileId, Is.EqualTo(new DisplayProfileId("profile.debug")));
            Assert.That(observedEvent.PreviousProfileId, Is.EqualTo(new DisplayProfileId("profile.default")));
            Assert.That(observedEvent.CurrentProfileId, Is.EqualTo(new DisplayProfileId("profile.debug")));
            Assert.That(observedEvent.CurrentMode, Is.EqualTo(DisplayMode.SeparateWindows));
        }

        [Test]
        public void SavePreference_OverridesFrameworkDefaultDuringStartup()
        {
            StubSaveService saveService = new();
            saveService.StoredPreference = new StubSaveService.StoredDisplayPreference
            {
                ProfileId = "profile.debug",
                Mode = DisplayMode.SeparateWindows,
            };

            DisplayService service = new(CreateConfigService(CreateDualProfileConfig()), saveService);
            DisplayContext current = service.GetCurrentDisplayContext();

            Assert.That(current.CurrentProfileId, Is.EqualTo(new DisplayProfileId("profile.debug")));
            Assert.That(current.CurrentMode, Is.EqualTo(DisplayMode.SeparateWindows));
        }

        [Test]
        public void ApplyProfile_ModeUnsupported_KeepsOldContext()
        {
            DisplayRuntimeConfig config = CreateDualProfileConfig();
            config.enableVR = false;
            config.profiles.Add(new DisplayRuntimeConfig.DisplayProfileConfig
            {
                profileId = "profile.vr",
                mode = DisplayMode.VREnabled,
                targetIds = new List<string> { "target.vr" },
            });
            config.targets.Add(new DisplayRuntimeConfig.DisplayTargetConfig
            {
                targetId = "target.vr",
                targetType = DisplayTargetTypes.VR,
                isConnected = true,
            });

            DisplayService service = new(CreateConfigService(config), null);
            DisplayContext before = service.GetCurrentDisplayContext();

            DisplayApplyResult result = service.ApplyProfile(new DisplayProfileId("profile.vr"), false);
            DisplayContext after = service.GetCurrentDisplayContext();

            Assert.That(result.Success, Is.False);
            Assert.That(result.Status, Is.EqualTo(DisplayApplyStatus.ModeUnsupported));
            Assert.That(after.CurrentProfileId, Is.EqualTo(before.CurrentProfileId));
            Assert.That(after.Version, Is.EqualTo(before.Version));
        }

        private static StubFrameworkConfigService CreateConfigService(DisplayRuntimeConfig config)
        {
            return new StubFrameworkConfigService(config);
        }

        private static DisplayRuntimeConfig CreateDualProfileConfig()
        {
            DisplayRuntimeConfig config = DisplayRuntimeConfig.CreateDefault();
            config.defaultDisplayProfileId = "profile.default";
            config.defaultDisplayMode = DisplayMode.SingleScreen;
            config.targets = new List<DisplayRuntimeConfig.DisplayTargetConfig>
            {
                new()
                {
                    targetId = "target.main",
                    targetType = DisplayTargetTypes.MainView,
                    isConnected = true,
                },
                new()
                {
                    targetId = "target.debug",
                    targetType = DisplayTargetTypes.DebugView,
                    isConnected = true,
                },
            };
            config.profiles = new List<DisplayRuntimeConfig.DisplayProfileConfig>
            {
                new()
                {
                    profileId = "profile.default",
                    mode = DisplayMode.SingleScreen,
                    targetIds = new List<string> { "target.main" },
                },
                new()
                {
                    profileId = "profile.debug",
                    mode = DisplayMode.SeparateWindows,
                    targetIds = new List<string> { "target.main", "target.debug" },
                },
            };
            return config;
        }

        private sealed class StubFrameworkConfigService : IFrameworkConfigService
        {
            private readonly DisplayRuntimeConfig _config;

            public StubFrameworkConfigService(DisplayRuntimeConfig config)
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

            public StoredDisplayPreference StoredPreference { get; set; }

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
                    Type dataType = data.GetType();
                    string profileId = Convert.ToString(dataType.GetField("profileId")?.GetValue(data)) ?? string.Empty;
                    object boxedMode = dataType.GetField("mode")?.GetValue(data);
                    StoredPreference = new StoredDisplayPreference
                    {
                        ProfileId = profileId,
                        Mode = boxedMode is DisplayMode mode ? mode : DisplayMode.SingleScreen,
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

                TData boxed = CreateDisplayPreferencePayload<TData>(StoredPreference);
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

            private static TData CreateDisplayPreferencePayload<TData>(StoredDisplayPreference preference)
            {
                object instance = Activator.CreateInstance(typeof(TData), true);
                Type type = instance.GetType();
                type.GetField("profileId")?.SetValue(instance, preference.ProfileId);
                type.GetField("mode")?.SetValue(instance, preference.Mode);
                return (TData)instance;
            }

            public sealed class StoredDisplayPreference
            {
                public string ProfileId { get; set; }

                public DisplayMode Mode { get; set; }
            }
        }
    }
}
#endif
