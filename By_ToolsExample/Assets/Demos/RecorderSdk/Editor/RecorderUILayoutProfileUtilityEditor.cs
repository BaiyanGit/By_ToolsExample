//=====================================================
// 文件名称: RecorderUILayoutProfileUtilityEditor
// 创 建 者: wangbaiyan
// 创建日期: 2026-05-25
// 描    述: Recorder SDK UI 布局配置导出与应用工具。
//=====================================================

#if UNITY_EDITOR
namespace Demos.示例_录制视频Recorder.Editor
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// Recorder UI 布局 Profile 工具。
    /// </summary>
    public static class RecorderUILayoutProfileUtilityEditor
    {
        public const string DEMO_PROFILE_FILE_NAME = "DemoUILayoutProfile.json";
        public const string SETTINGS_PROFILE_FILE_NAME = "SettingsUILayoutProfile.json";
        private const string PROFILE_RELATIVE_FOLDER = "UI/Settings/LayoutProfiles";

        [Serializable]
        private class LayoutProfile
        {
            public List<ElementProfile> elements = new();
        }

        [Serializable]
        private class ElementProfile
        {
            public string path;
            public Vector2 anchorMin;
            public Vector2 anchorMax;
            public Vector2 pivot;
            public Vector2 anchoredPosition;
            public Vector2 sizeDelta;
            public Vector3 localScale;
            public Color imageColor;
            public bool hasImage;
            public Color textColor;
            public int fontSize;
            public TextAnchor alignment;
            public bool hasText;
            public float spacing;
            public RectOffsetProfile padding;
            public bool hasHorizontalLayout;
            public bool hasVerticalLayout;
        }

        [Serializable]
        private class RectOffsetProfile
        {
            public int left;
            public int right;
            public int top;
            public int bottom;
        }

        /// <summary>
        /// 导出指定根对象下的布局配置。
        /// </summary>
        public static void ExportProfile(GameObject root, string fileName)
        {
            if (root == null)
            {
                Debug.LogWarning("Recorder UI 布局导出失败：未找到根对象。");
                return;
            }

            var profile = new LayoutProfile();
            var rects   = root.GetComponentsInChildren<RectTransform>(true);
            foreach (var rect in rects)
            {
                var element = new ElementProfile
                {
                    path             = GetRelativePath(root.transform, rect.transform),
                    anchorMin        = rect.anchorMin,
                    anchorMax        = rect.anchorMax,
                    pivot            = rect.pivot,
                    anchoredPosition = rect.anchoredPosition,
                    sizeDelta        = rect.sizeDelta,
                    localScale       = rect.localScale
                };

                var image = rect.GetComponent<Image>();
                if (image != null)
                {
                    element.hasImage   = true;
                    element.imageColor = image.color;
                }

                var text = rect.GetComponent<Text>();
                if (text != null)
                {
                    element.hasText   = true;
                    element.textColor = text.color;
                    element.fontSize  = text.fontSize;
                    element.alignment = text.alignment;
                }

                var horizontal = rect.GetComponent<HorizontalLayoutGroup>();
                if (horizontal != null)
                {
                    element.hasHorizontalLayout = true;
                    element.spacing             = horizontal.spacing;
                    element.padding             = ToProfile(horizontal.padding);
                }

                var vertical = rect.GetComponent<VerticalLayoutGroup>();
                if (vertical != null)
                {
                    element.hasVerticalLayout = true;
                    element.spacing           = vertical.spacing;
                    element.padding           = ToProfile(vertical.padding);
                }

                profile.elements.Add(element);
            }

            string profileFolder = GetProfileFolder();
            Directory.CreateDirectory(profileFolder);
            string path = Path.Combine(profileFolder, fileName).Replace("\\", "/");
            File.WriteAllText(path, JsonUtility.ToJson(profile, true));
            AssetDatabase.Refresh();
            Debug.Log("Recorder UI 布局已导出: " + path);
        }

        /// <summary>
        /// 如果 Profile 存在则应用到指定根对象。
        /// </summary>
        public static void ApplyProfileIfExists(GameObject root, string fileName)
        {
            string path = GetProfilePath(fileName);
            if (!File.Exists(path)) return;
            ApplyProfile(root, fileName);
        }

        /// <summary>
        /// 应用指定布局配置到根对象。
        /// </summary>
        public static void ApplyProfile(GameObject root, string fileName)
        {
            if (root == null)
            {
                Debug.LogWarning("Recorder UI 布局应用失败：未找到根对象。");
                return;
            }

            string path = GetProfilePath(fileName);
            if (!File.Exists(path))
            {
                Debug.LogWarning("Recorder UI 布局应用失败：Profile 不存在: " + path);
                return;
            }

            var profile = JsonUtility.FromJson<LayoutProfile>(File.ReadAllText(path));
            if (profile == null || profile.elements == null) return;

            foreach (var element in profile.elements)
            {
                var target = FindByRelativePath(root.transform, element.path);
                if (target == null) continue;
                Undo.RecordObject(target, "Apply Recorder UI Layout");
                target.anchorMin        = element.anchorMin;
                target.anchorMax        = element.anchorMax;
                target.pivot            = element.pivot;
                target.anchoredPosition = element.anchoredPosition;
                target.sizeDelta        = element.sizeDelta;
                target.localScale       = element.localScale;

                var image = target.GetComponent<Image>();
                if (element.hasImage && image != null)
                {
                    Undo.RecordObject(image, "Apply Recorder UI Layout");
                    image.color = element.imageColor;
                }

                var text = target.GetComponent<Text>();
                if (element.hasText && text != null)
                {
                    Undo.RecordObject(text, "Apply Recorder UI Layout");
                    text.color     = element.textColor;
                    text.fontSize  = element.fontSize;
                    text.alignment = element.alignment;
                }

                ApplyLayoutGroup(target.GetComponent<HorizontalLayoutGroup>(), element);
                ApplyLayoutGroup(target.GetComponent<VerticalLayoutGroup>(), element);
            }

            EditorUtility.SetDirty(root);
            Debug.Log("Recorder UI 布局已应用: " + path);
        }

        /// <summary>
        /// 查找屏幕录制 UI 根对象。
        /// </summary>
        public static GameObject FindDemoRoot()
        {
            return GameObject.Find("Recorder Demo Canvas");
        }

        /// <summary>
        /// 查找设置中心 UI 根对象。
        /// </summary>
        public static GameObject FindSettingsRoot()
        {
            return GameObject.Find("Recorder Settings Canvas");
        }

        private static string GetProfilePath(string fileName)
        {
            return Path.Combine(GetProfileFolder(), fileName).Replace("\\", "/");
        }

        private static string GetProfileFolder()
        {
            string[] guids = AssetDatabase.FindAssets("RecorderUILayoutProfileUtilityEditor t:MonoScript");
            if (guids != null && guids.Length > 0)
            {
                string scriptPath = AssetDatabase.GUIDToAssetPath(guids[0]).Replace("\\", "/");
                string marker = "/RecorderSdk/Editor/";
                int index = scriptPath.IndexOf(marker, StringComparison.Ordinal);
                if (index >= 0)
                {
                    string sdkRoot = scriptPath.Substring(0, index + "/RecorderSdk".Length);
                    return (sdkRoot + "/" + PROFILE_RELATIVE_FOLDER).Replace("\\", "/");
                }
            }

            return ("Assets/Demos/RecorderSdk/" + PROFILE_RELATIVE_FOLDER).Replace("\\", "/");
        }

        private static RectOffsetProfile ToProfile(RectOffset padding)
        {
            return new RectOffsetProfile { left = padding.left, right = padding.right, top = padding.top, bottom = padding.bottom };
        }

        private static void ApplyLayoutGroup(HorizontalOrVerticalLayoutGroup group, ElementProfile element)
        {
            if (group == null) return;
            bool shouldApply = group is HorizontalLayoutGroup ? element.hasHorizontalLayout : element.hasVerticalLayout;
            if (!shouldApply) return;
            Undo.RecordObject(group, "Apply Recorder UI Layout");
            group.spacing = element.spacing;
            if (element.padding != null)
            {
                group.padding.left   = element.padding.left;
                group.padding.right  = element.padding.right;
                group.padding.top    = element.padding.top;
                group.padding.bottom = element.padding.bottom;
            }
        }

        private static string GetRelativePath(Transform root, Transform target)
        {
            if (root == target) return string.Empty;
            var names = new Stack<string>();
            while (target != null && target != root)
            {
                names.Push(target.name);
                target = target.parent;
            }

            return string.Join("/", names);
        }

        private static RectTransform FindByRelativePath(Transform root, string path)
        {
            if (string.IsNullOrEmpty(path)) return root as RectTransform;
            var target = root.Find(path);
            return target as RectTransform;
        }
    }
}
#endif
