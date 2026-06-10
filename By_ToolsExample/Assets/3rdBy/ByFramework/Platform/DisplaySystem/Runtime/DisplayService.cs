//=====================================================
// 文件名称: DisplayService.cs
// 创建者: Codex
// 创建日期: 2026-06-09
// 描    述: 实现 DisplaySystem Runtime 的最小显示状态管理服务。
//=====================================================

namespace _3rdBy.ByFramework.Platform.DisplaySystem
{
    using System;
    using System.Collections.Generic;
    using _3rdBy.ByFramework.Platform.FrameworkConfig;
    using _3rdBy.ByFramework.Platform.SaveSystem;

    public sealed class DisplayService : IDisplayService
    {
        private const string UserDisplayPreferenceSaveKey = "platform.display.preference";
        private readonly object _syncRoot = new();
        private readonly ISaveService _saveService;
        private readonly DisplayRuntimeConfig _config;
        private readonly Dictionary<string, DisplayProfileSnapshot> _profiles;
        private readonly Dictionary<string, DisplayTargetDescriptor> _targets;
        private readonly IReadOnlyList<DisplayProfileId> _availableProfiles;
        private readonly IReadOnlyList<DisplayTargetDescriptor> _availableTargets;
        private DisplayContext _currentContext;

        public DisplayService(
            IFrameworkConfigService frameworkConfigService = null,
            ISaveService saveService = null)
        {
            _saveService = saveService;
            _config = LoadRuntimeConfig(frameworkConfigService);
            _targets = BuildTargetMap(_config);
            _profiles = BuildProfileMap(_config, _targets);
            _availableProfiles = BuildProfileList(_profiles);
            _availableTargets = BuildTargetList(_targets);
            _currentContext = BuildInitialContext();
        }

        public event Action<DisplayContextChangedEvent> DisplayContextChanged;

        public DisplayContext GetCurrentDisplayContext()
        {
            lock (_syncRoot)
            {
                return _currentContext;
            }
        }

        public IReadOnlyList<DisplayProfileId> GetAvailableProfiles()
        {
            return _availableProfiles;
        }

        public bool TryGetProfile(
            DisplayProfileId profileId,
            out DisplayProfileSnapshot profile,
            out DisplayQueryStatus status)
        {
            if (profileId.IsEmpty)
            {
                throw new ArgumentException("[DisplaySystem] ProfileId cannot be empty.", nameof(profileId));
            }

            lock (_syncRoot)
            {
                if (_profiles.TryGetValue(profileId.Value, out profile))
                {
                    status = DisplayQueryStatus.Success;
                    return true;
                }

                status = DisplayQueryStatus.ProfileNotFound;
                return false;
            }
        }

        public IReadOnlyList<DisplayTargetDescriptor> GetAvailableTargets()
        {
            return _availableTargets;
        }

        public bool TryGetTarget(
            DisplayTargetId targetId,
            out DisplayTargetDescriptor target,
            out DisplayQueryStatus status)
        {
            if (targetId.IsEmpty)
            {
                throw new ArgumentException("[DisplaySystem] TargetId cannot be empty.", nameof(targetId));
            }

            lock (_syncRoot)
            {
                if (_targets.TryGetValue(targetId.Value, out target))
                {
                    status = DisplayQueryStatus.Success;
                    return true;
                }

                status = DisplayQueryStatus.TargetNotFound;
                return false;
            }
        }

        public DisplayMode GetCurrentMode()
        {
            lock (_syncRoot)
            {
                return _currentContext.CurrentMode;
            }
        }

        public DisplayApplyResult ApplyProfile(
            DisplayProfileId profileId,
            bool persistPreference = true)
        {
            if (profileId.IsEmpty)
            {
                throw new ArgumentException("[DisplaySystem] ProfileId cannot be empty.", nameof(profileId));
            }

            DisplayContextChangedEvent changedEvent = default;
            bool shouldPublish = false;
            DisplayApplyResult result;

            lock (_syncRoot)
            {
                DisplayContext previousContext = _currentContext;
                if (!_profiles.TryGetValue(profileId.Value, out DisplayProfileSnapshot profile))
                {
                    return CreateApplyResult(false, DisplayApplyStatus.ProfileNotFound, profileId, previousContext, previousContext);
                }

                if (!IsModeSupported(profile.Mode))
                {
                    return CreateApplyResult(false, DisplayApplyStatus.ModeUnsupported, profileId, previousContext, previousContext);
                }

                if (!TryResolveActiveTargets(profile, out List<DisplayTargetDescriptor> activeTargets))
                {
                    return CreateApplyResult(false, DisplayApplyStatus.TargetUnavailable, profileId, previousContext, previousContext);
                }

                DisplayContext nextContext = new(
                    profile.ProfileId,
                    profile.Mode,
                    activeTargets,
                    previousContext.Version + 1);

                if (persistPreference && !TryPersistPreference(profile.ProfileId, profile.Mode))
                {
                    return CreateApplyResult(false, DisplayApplyStatus.PersistenceFailure, profileId, previousContext, previousContext);
                }

                _currentContext = nextContext;
                result = CreateApplyResult(true, DisplayApplyStatus.Success, profileId, previousContext, nextContext);

                if (previousContext.CurrentProfileId != nextContext.CurrentProfileId
                    || previousContext.CurrentMode != nextContext.CurrentMode)
                {
                    changedEvent = new DisplayContextChangedEvent(
                        previousContext.CurrentProfileId,
                        nextContext.CurrentProfileId,
                        previousContext.CurrentMode,
                        nextContext.CurrentMode);
                    shouldPublish = true;
                }
            }

            if (shouldPublish)
            {
                DisplayContextChanged?.Invoke(changedEvent);
            }

            return result;
        }

        private DisplayContext BuildInitialContext()
        {
            DisplayProfileSnapshot profile = ResolveInitialProfile();
            if (!TryResolveActiveTargets(profile, out List<DisplayTargetDescriptor> activeTargets))
            {
                profile = ResolveFallbackProfile();
                if (!TryResolveActiveTargets(profile, out activeTargets))
                {
                    activeTargets = new List<DisplayTargetDescriptor>();
                }
            }

            return new DisplayContext(profile.ProfileId, profile.Mode, activeTargets, 0);
        }

        private DisplayProfileSnapshot ResolveInitialProfile()
        {
            if (TryRestorePreference(out DisplayProfileId preferredProfileId, out _)
                && _profiles.TryGetValue(preferredProfileId.Value, out DisplayProfileSnapshot preferredProfile)
                && IsModeSupported(preferredProfile.Mode)
                && TryResolveActiveTargets(preferredProfile, out _))
            {
                return preferredProfile;
            }

            DisplayProfileId configuredId = new(_config.DefaultDisplayProfileId);
            if (_profiles.TryGetValue(configuredId.Value, out DisplayProfileSnapshot configuredProfile)
                && IsModeSupported(configuredProfile.Mode)
                && TryResolveActiveTargets(configuredProfile, out _))
            {
                return configuredProfile;
            }

            return ResolveFallbackProfile();
        }

        private DisplayProfileSnapshot ResolveFallbackProfile()
        {
            foreach (KeyValuePair<string, DisplayProfileSnapshot> pair in _profiles)
            {
                if (IsModeSupported(pair.Value.Mode) && TryResolveActiveTargets(pair.Value, out _))
                {
                    return pair.Value;
                }
            }

            return new DisplayProfileSnapshot(
                new DisplayProfileId(_config.DefaultDisplayProfileId),
                _config.DefaultDisplayMode,
                Array.Empty<DisplayTargetId>());
        }

        private bool TryResolveActiveTargets(DisplayProfileSnapshot profile, out List<DisplayTargetDescriptor> activeTargets)
        {
            activeTargets = new List<DisplayTargetDescriptor>(profile.TargetIds.Count);
            for (int index = 0; index < profile.TargetIds.Count; index++)
            {
                DisplayTargetId targetId = profile.TargetIds[index];
                if (!_targets.TryGetValue(targetId.Value, out DisplayTargetDescriptor descriptor) || !descriptor.IsConnected)
                {
                    activeTargets = null;
                    return false;
                }

                activeTargets.Add(descriptor);
            }

            return true;
        }

        private bool IsModeSupported(DisplayMode mode)
        {
            return mode != DisplayMode.VREnabled || _config.EnableVR;
        }

        private bool TryRestorePreference(out DisplayProfileId profileId, out DisplayMode mode)
        {
            profileId = default;
            mode = DisplayMode.SingleScreen;
            if (_saveService == null)
            {
                return false;
            }

            SaveRequest request = new(UserDisplayPreferenceSaveKey, SaveScope.User);
            if (!_saveService.TryLoad(request, out DisplayPreferenceData data, out LoadResult<DisplayPreferenceData> result)
                || !result.Success
                || data == null
                || !DisplayProfileId.TryParse(data.profileId, out profileId))
            {
                return false;
            }

            mode = data.mode;
            return true;
        }

        private bool TryPersistPreference(DisplayProfileId profileId, DisplayMode mode)
        {
            if (_saveService == null)
            {
                return true;
            }

            SaveRequest<DisplayPreferenceData> request = new(
                UserDisplayPreferenceSaveKey,
                SaveScope.User,
                new DisplayPreferenceData
                {
                    profileId = profileId.Value,
                    mode = mode,
                });

            return _saveService.TrySave(request, out SaveResult result) && result.Success;
        }

        private static DisplayRuntimeConfig LoadRuntimeConfig(IFrameworkConfigService frameworkConfigService)
        {
            if (frameworkConfigService != null
                && frameworkConfigService.TryGetModuleConfig(out DisplayRuntimeConfig config)
                && config != null)
            {
                return config.Normalize();
            }

            return DisplayRuntimeConfig.CreateDefault();
        }

        private static Dictionary<string, DisplayTargetDescriptor> BuildTargetMap(DisplayRuntimeConfig config)
        {
            Dictionary<string, DisplayTargetDescriptor> targets = new(StringComparer.Ordinal);
            IReadOnlyList<DisplayRuntimeConfig.DisplayTargetConfig> configuredTargets = config.Targets;
            for (int index = 0; index < configuredTargets.Count; index++)
            {
                DisplayRuntimeConfig.DisplayTargetConfig configuredTarget = configuredTargets[index];
                DisplayTargetId targetId = new(configuredTarget.targetId);
                targets[targetId.Value] = new DisplayTargetDescriptor(
                    targetId,
                    configuredTarget.targetType,
                    configuredTarget.isConnected);
            }

            return targets;
        }

        private static Dictionary<string, DisplayProfileSnapshot> BuildProfileMap(
            DisplayRuntimeConfig config,
            IReadOnlyDictionary<string, DisplayTargetDescriptor> targets)
        {
            Dictionary<string, DisplayProfileSnapshot> profiles = new(StringComparer.Ordinal);
            IReadOnlyList<DisplayRuntimeConfig.DisplayProfileConfig> configuredProfiles = config.Profiles;
            for (int index = 0; index < configuredProfiles.Count; index++)
            {
                DisplayRuntimeConfig.DisplayProfileConfig configuredProfile = configuredProfiles[index];
                DisplayProfileId profileId = new(configuredProfile.profileId);
                List<DisplayTargetId> targetIds = new();
                for (int targetIndex = 0; targetIndex < configuredProfile.targetIds.Count; targetIndex++)
                {
                    if (!DisplayTargetId.TryParse(configuredProfile.targetIds[targetIndex], out DisplayTargetId targetId))
                    {
                        continue;
                    }

                    if (!targets.ContainsKey(targetId.Value))
                    {
                        continue;
                    }

                    targetIds.Add(targetId);
                }

                profiles[profileId.Value] = new DisplayProfileSnapshot(
                    profileId,
                    configuredProfile.mode,
                    targetIds);
            }

            return profiles;
        }

        private static IReadOnlyList<DisplayProfileId> BuildProfileList(IReadOnlyDictionary<string, DisplayProfileSnapshot> profiles)
        {
            List<DisplayProfileId> profileIds = new(profiles.Count);
            foreach (KeyValuePair<string, DisplayProfileSnapshot> pair in profiles)
            {
                profileIds.Add(pair.Value.ProfileId);
            }

            profileIds.Sort(static (left, right) => string.CompareOrdinal(left.Value, right.Value));
            return profileIds.AsReadOnly();
        }

        private static IReadOnlyList<DisplayTargetDescriptor> BuildTargetList(IReadOnlyDictionary<string, DisplayTargetDescriptor> targets)
        {
            List<DisplayTargetDescriptor> descriptors = new(targets.Values);
            descriptors.Sort(static (left, right) => string.CompareOrdinal(left.TargetId.Value, right.TargetId.Value));
            return descriptors.AsReadOnly();
        }

        private static DisplayApplyResult CreateApplyResult(
            bool success,
            DisplayApplyStatus status,
            DisplayProfileId requestedProfileId,
            DisplayContext previousContext,
            DisplayContext currentContext)
        {
            return new DisplayApplyResult(
                success,
                status,
                requestedProfileId,
                previousContext.CurrentProfileId,
                currentContext.CurrentProfileId);
        }

        [Serializable]
        private sealed class DisplayPreferenceData
        {
            public string profileId = string.Empty;
            public DisplayMode mode = DisplayMode.SingleScreen;
        }
    }
}
