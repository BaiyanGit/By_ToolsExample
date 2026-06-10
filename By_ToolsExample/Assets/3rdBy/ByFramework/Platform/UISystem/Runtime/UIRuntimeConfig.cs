//=====================================================
// 文件名称: UIRuntimeConfig.cs
// 创建者: Codex
// 创建日期: 2026-06-09
// 描    述: 定义 UISystem Runtime 内部配置模型。
//=====================================================

namespace _3rdBy.ByFramework.Platform.UISystem
{
    using System;
    using System.Collections.Generic;

    [Serializable]
    internal sealed class UIRuntimeConfig
    {
        public string defaultUIRootId = "ui.root.default";
        public string defaultUILayerId = "ui.layer.main";
        public string defaultThemeId = "ui.theme.default";
        public bool enableRuntimeConfigUI = true;
        public string uiResourcePolicy = "Mapped";
        public string uiCachePolicy = "Retain";
        public List<string> registeredRoots = new();
        public List<string> availableLayers = new();
        public List<UIViewConfig> views = new();

        public string DefaultUIRootId => string.IsNullOrWhiteSpace(defaultUIRootId)
            ? "ui.root.default"
            : defaultUIRootId.Trim();

        public string DefaultUILayerId => string.IsNullOrWhiteSpace(defaultUILayerId)
            ? "ui.layer.main"
            : defaultUILayerId.Trim();

        public string DefaultThemeId => defaultThemeId ?? string.Empty;

        public bool EnableRuntimeConfigUI => enableRuntimeConfigUI;

        public string UIResourcePolicy => uiResourcePolicy ?? string.Empty;

        public string UICachePolicy => uiCachePolicy ?? string.Empty;

        public IReadOnlyList<string> RegisteredRoots
        {
            get
            {
                if (registeredRoots != null)
                {
                    return registeredRoots.AsReadOnly();
                }

                return Array.Empty<string>();
            }
        }

        public IReadOnlyList<string> AvailableLayers
        {
            get
            {
                if (availableLayers != null)
                {
                    return availableLayers.AsReadOnly();
                }

                return Array.Empty<string>();
            }
        }

        public IReadOnlyList<UIViewConfig> Views
        {
            get
            {
                if (views != null)
                {
                    return views.AsReadOnly();
                }

                return Array.Empty<UIViewConfig>();
            }
        }

        public static UIRuntimeConfig CreateDefault()
        {
            UIRuntimeConfig config = new();
            config.registeredRoots.Add(config.DefaultUIRootId);
            config.availableLayers.Add(config.DefaultUILayerId);
            config.views.Add(new UIViewConfig
            {
                uiKey = "ui.main_menu",
                resourceKey = "ui.main_menu",
                defaultRootId = config.DefaultUIRootId,
                defaultLayerId = config.DefaultUILayerId,
            });
            return config;
        }

        public UIRuntimeConfig Normalize()
        {
            UIRuntimeConfig normalized = new()
            {
                defaultUIRootId = DefaultUIRootId,
                defaultUILayerId = DefaultUILayerId,
                defaultThemeId = DefaultThemeId,
                enableRuntimeConfigUI = enableRuntimeConfigUI,
                uiResourcePolicy = UIResourcePolicy,
                uiCachePolicy = UICachePolicy,
                registeredRoots = new List<string>(),
                availableLayers = new List<string>(),
                views = new List<UIViewConfig>(),
            };

            IReadOnlyList<string> roots = RegisteredRoots;
            for (int index = 0; index < roots.Count; index++)
            {
                if (!string.IsNullOrWhiteSpace(roots[index]))
                {
                    normalized.registeredRoots.Add(roots[index].Trim());
                }
            }

            IReadOnlyList<string> layers = AvailableLayers;
            for (int index = 0; index < layers.Count; index++)
            {
                if (!string.IsNullOrWhiteSpace(layers[index]))
                {
                    normalized.availableLayers.Add(layers[index].Trim());
                }
            }

            IReadOnlyList<UIViewConfig> configuredViews = Views;
            for (int index = 0; index < configuredViews.Count; index++)
            {
                UIViewConfig view = configuredViews[index];
                if (view == null || string.IsNullOrWhiteSpace(view.uiKey))
                {
                    continue;
                }

                normalized.views.Add(view.Clone());
            }

            if (normalized.registeredRoots.Count == 0)
            {
                normalized.registeredRoots.Add(normalized.DefaultUIRootId);
            }

            if (normalized.availableLayers.Count == 0)
            {
                normalized.availableLayers.Add(normalized.DefaultUILayerId);
            }

            if (normalized.views.Count == 0)
            {
                return CreateDefault();
            }

            return normalized;
        }

        [Serializable]
        internal sealed class UIViewConfig
        {
            public string uiKey = string.Empty;
            public string resourceKey = string.Empty;
            public string defaultRootId = string.Empty;
            public string defaultLayerId = string.Empty;

            public UIViewConfig Clone()
            {
                return new UIViewConfig
                {
                    uiKey = uiKey ?? string.Empty,
                    resourceKey = resourceKey ?? string.Empty,
                    defaultRootId = defaultRootId ?? string.Empty,
                    defaultLayerId = defaultLayerId ?? string.Empty,
                };
            }
        }
    }
}
