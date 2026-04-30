namespace _3rdBy.ByTools.ScenePlaySelector.Editor
{
    using System;
    using System.Reflection;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.UIElements;

    /// <summary>
    /// Unity Editor Toolbar 辅助工具。
    /// 
    /// 这个类负责通过反射访问 Unity 编辑器顶部 Toolbar。
    /// 因为 Unity 的 Toolbar 并不是正式公开 API，所以这里统一封装反射逻辑，
    /// 避免其它脚本里重复写反射代码，也方便后续 Unity 版本升级时集中维护。
    /// </summary>
    public static class EditorToolbarUtil
    {
        [Header("Unity Toolbar 左侧区域名称")]
        private const string LeftZoneName = "ToolbarZoneLeftAlign";

        [Header("Unity Toolbar 播放控制区域名称，Play/Pause/Step 按钮所在区域")]
        private const string PlayModeZoneName = "ToolbarZonePlayMode";

        [Header("Unity Toolbar 右侧区域名称")]
        private const string RightZoneName = "ToolbarZoneRightAlign";

        [Header("UnityEditor.Toolbar 的反射类型缓存")]
        private static Type _toolbarType;

        [Header("UnityEditor.Toolbar 内部 m_Root 字段缓存")]
        private static FieldInfo _rootField;

        /// <summary>
        /// 尝试获取 Unity Toolbar 的根 VisualElement。
        /// </summary>
        /// <param name="root">成功时返回 Toolbar 根节点。</param>
        /// <returns>是否成功获取根节点。</returns>
        private static bool TryGetRoot(out VisualElement root)
        {
            root = null;

            _toolbarType ??= typeof(Editor).Assembly.GetType("UnityEditor.Toolbar");
            if (_toolbarType == null)
            {
                return false;
            }

            var toolbars = Resources.FindObjectsOfTypeAll(_toolbarType);
            if (toolbars == null || toolbars.Length == 0)
            {
                return false;
            }

            _rootField ??= _toolbarType.GetField("m_Root", BindingFlags.NonPublic | BindingFlags.Instance);
            if (_rootField == null)
            {
                return false;
            }

            root = _rootField.GetValue(toolbars[0]) as VisualElement;
            return root != null;
        }

        /// <summary>
        /// 尝试获取 Toolbar 左侧区域。
        /// </summary>
        public static bool TryGetLeftZone(out VisualElement leftZone)
        {
            return TryGetZone(LeftZoneName, out leftZone);
        }

        /// <summary>
        /// 尝试获取 Toolbar 播放控制区域。
        /// 这个区域包含 Play / Pause / Step 按钮。
        /// </summary>
        public static bool TryGetPlayModeZone(out VisualElement playModeZone)
        {
            return TryGetZone(PlayModeZoneName, out playModeZone);
        }

        /// <summary>
        /// 尝试获取 Toolbar 右侧区域。
        /// </summary>
        public static bool TryGetRightZone(out VisualElement rightZone)
        {
            return TryGetZone(RightZoneName, out rightZone);
        }

        /// <summary>
        /// 根据区域名称获取 Toolbar 中的指定区域。
        /// </summary>
        private static bool TryGetZone(string zoneName, out VisualElement zone)
        {
            zone = null;

            if (string.IsNullOrWhiteSpace(zoneName))
            {
                return false;
            }

            if (!TryGetRoot(out var root))
            {
                return false;
            }

            zone = root.Q(zoneName);
            return zone != null;
        }

        /// <summary>
        /// 获取 Toolbar 左侧区域。
        /// </summary>
        public static VisualElement GetLeftZone()
        {
            return TryGetLeftZone(out var leftZone) ? leftZone : null;
        }

        /// <summary>
        /// 获取 Toolbar 播放控制区域。
        /// </summary>
        public static VisualElement GetPlayModeZone()
        {
            return TryGetPlayModeZone(out var playModeZone) ? playModeZone : null;
        }

        /// <summary>
        /// 判断 Toolbar 中是否已经存在指定名称的元素。
        /// </summary>
        public static bool HasElement(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return false;
            }

            if (!TryGetRoot(out var root))
            {
                return false;
            }

            return root.Q(name) != null;
        }

        /// <summary>
        /// 默认添加到 Toolbar 左侧区域。
        /// </summary>
        public static bool AddOrReplace(string name, VisualElement element)
        {
            return AddOrReplaceToLeft(name, element);
        }

        /// <summary>
        /// 添加或替换元素到 Toolbar 左侧区域。
        /// </summary>
        public static bool AddOrReplaceToLeft(string name, VisualElement element)
        {
            return AddOrReplaceToZone(LeftZoneName, name, element);
        }

        /// <summary>
        /// 添加或替换元素到播放控制区域。
        /// 追加元素后会显示在 Step 按钮右侧。
        /// </summary>
        public static bool AddOrReplaceToPlayModeRight(string name, VisualElement element)
        {
            return AddOrReplaceToZone(PlayModeZoneName, name, element);
        }

        /// <summary>
        /// 添加或替换元素到 Toolbar 右侧区域。
        /// </summary>
        public static bool AddOrReplaceToRight(string name, VisualElement element)
        {
            return AddOrReplaceToZone(RightZoneName, name, element);
        }

        /// <summary>
        /// 将指定元素添加到指定 Toolbar 区域。
        /// 如果旧元素已经存在，会先从整个 Toolbar 中移除，避免重复挂载。
        /// </summary>
        private static bool AddOrReplaceToZone(string zoneName, string name, VisualElement element)
        {
            if (string.IsNullOrWhiteSpace(zoneName) || string.IsNullOrWhiteSpace(name) || element == null)
            {
                return false;
            }

            if (!TryGetRoot(out var root))
            {
                return false;
            }

            var zone = root.Q(zoneName);
            if (zone == null)
            {
                return false;
            }

            // 从整个 Toolbar 根节点中查找旧元素，避免旧版本残留在其它区域。
            var old = root.Q(name);
            old?.RemoveFromHierarchy();

            element.name = name;
            element.RemoveFromHierarchy();
            zone.Add(element);
            return true;
        }
    }
}
