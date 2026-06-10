//=====================================================
// 文件名称: DisplayRuntimeConfig.cs
// 创建者: Codex
// 创建日期: 2026-06-09
// 描    述: 定义 DisplaySystem Runtime 内部配置模型。
//=====================================================

namespace _3rdBy.ByFramework.Platform.DisplaySystem
{
    using System;
    using System.Collections.Generic;

    [Serializable]
    internal sealed class DisplayRuntimeConfig
    {
        public string defaultDisplayProfileId = "display.default.single_screen";
        public DisplayMode defaultDisplayMode = DisplayMode.SingleScreen;
        public bool enableVR;
        public string displayProfileSelector = string.Empty;
        public List<DisplayProfileConfig> profiles = new();
        public List<DisplayTargetConfig> targets = new();

        public string DefaultDisplayProfileId => string.IsNullOrWhiteSpace(defaultDisplayProfileId)
            ? "display.default.single_screen"
            : defaultDisplayProfileId.Trim();

        public DisplayMode DefaultDisplayMode => defaultDisplayMode;

        public bool EnableVR => enableVR;

        public string DisplayProfileSelector => displayProfileSelector ?? string.Empty;

        public IReadOnlyList<DisplayProfileConfig> Profiles
        {
            get
            {
                if (profiles != null)
                {
                    return profiles.AsReadOnly();
                }

                return Array.Empty<DisplayProfileConfig>();
            }
        }

        public IReadOnlyList<DisplayTargetConfig> Targets
        {
            get
            {
                if (targets != null)
                {
                    return targets.AsReadOnly();
                }

                return Array.Empty<DisplayTargetConfig>();
            }
        }

        public static DisplayRuntimeConfig CreateDefault()
        {
            DisplayRuntimeConfig config = new();
            config.targets.Add(new DisplayTargetConfig
            {
                targetId = "target.main",
                targetType = DisplayTargetTypes.MainView,
                isConnected = true,
            });
            config.profiles.Add(new DisplayProfileConfig
            {
                profileId = config.DefaultDisplayProfileId,
                mode = DisplayMode.SingleScreen,
                targetIds = new List<string> { "target.main" },
            });
            return config;
        }

        public DisplayRuntimeConfig Normalize()
        {
            DisplayRuntimeConfig normalized = new()
            {
                defaultDisplayProfileId = DefaultDisplayProfileId,
                defaultDisplayMode = defaultDisplayMode,
                enableVR = enableVR,
                displayProfileSelector = displayProfileSelector ?? string.Empty,
                profiles = new List<DisplayProfileConfig>(),
                targets = new List<DisplayTargetConfig>(),
            };

            IReadOnlyList<DisplayTargetConfig> sourceTargets = Targets;
            for (int index = 0; index < sourceTargets.Count; index++)
            {
                DisplayTargetConfig target = sourceTargets[index];
                if (target == null || string.IsNullOrWhiteSpace(target.targetId) || string.IsNullOrWhiteSpace(target.targetType))
                {
                    continue;
                }

                normalized.targets.Add(target.Clone());
            }

            IReadOnlyList<DisplayProfileConfig> sourceProfiles = Profiles;
            for (int index = 0; index < sourceProfiles.Count; index++)
            {
                DisplayProfileConfig profile = sourceProfiles[index];
                if (profile == null || string.IsNullOrWhiteSpace(profile.profileId))
                {
                    continue;
                }

                normalized.profiles.Add(profile.Clone());
            }

            if (normalized.targets.Count == 0 || normalized.profiles.Count == 0)
            {
                return CreateDefault();
            }

            return normalized;
        }

        [Serializable]
        internal sealed class DisplayProfileConfig
        {
            public string profileId = string.Empty;
            public DisplayMode mode = DisplayMode.SingleScreen;
            public List<string> targetIds = new();

            public DisplayProfileConfig Clone()
            {
                return new DisplayProfileConfig
                {
                    profileId = profileId ?? string.Empty,
                    mode = mode,
                    targetIds = targetIds == null ? new List<string>() : new List<string>(targetIds),
                };
            }
        }

        [Serializable]
        internal sealed class DisplayTargetConfig
        {
            public string targetId = string.Empty;
            public string targetType = DisplayTargetTypes.MainView;
            public bool isConnected = true;

            public DisplayTargetConfig Clone()
            {
                return new DisplayTargetConfig
                {
                    targetId = targetId ?? string.Empty,
                    targetType = targetType ?? string.Empty,
                    isConnected = isConnected,
                };
            }
        }
    }
}
