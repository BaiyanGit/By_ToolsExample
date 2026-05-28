using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Demos.RecorderSdk.Editor
{
    internal static class RecorderSdkUiTransformDiagnosticsEditor
    {
        private const string SCAN_MENU_PATH = "ByTools/🔴 Recorder SDK/诊断/扫描异常 Transform 与 UI";
        private const string FIX_MENU_PATH = "ByTools/🔴 Recorder SDK/诊断/修复明显异常 Transform";
        private const float QUATERNION_MIN_LENGTH = 0.9999f;
        private const float QUATERNION_MAX_LENGTH = 1.0001f;
        private const float SCALE_EPSILON = 0.0001f;
        private const float HUGE_VALUE = 1000000f;
        private static readonly Quaternion reportedInvalidQuaternion = new(-0.333378f, -0.220343f, -0.079487f, 0.912606f);

        [MenuItem(SCAN_MENU_PATH)]
        private static void ScanCurrentScenes()
        {
            var issues = CollectIssues();
            LogReport("扫描异常 Transform 与 UI", issues);
        }

        [MenuItem(FIX_MENU_PATH)]
        private static void FixObviousIssues()
        {
            var beforeFix = CollectIssues();
            int fixedCount = 0;

            foreach (var go in GetLoadedSceneObjects())
            {
                var transform = go.transform;
                string path = GetHierarchyPath(transform);

                if (FixTransform(path, transform)) fixedCount++;
                if (FixNavigation(path, go)) fixedCount++;
            }

            if (fixedCount > 0)
            {
                foreach (var scene in GetLoadedScenes())
                {
                    EditorSceneManager.MarkSceneDirty(scene);
                }
            }

            Debug.LogWarning($"[Recorder SDK] 修复明显异常 Transform 完成。修复项：{fixedCount}，修复前问题数：{beforeFix.Count}。请重新执行扫描确认。");
        }

        private static List<Issue> CollectIssues()
        {
            var issues = new List<Issue>();

            foreach (var go in GetLoadedSceneObjects())
            {
                string path = GetHierarchyPath(go.transform);
                ScanTransform(go.transform, path, issues);
                ScanRectTransform(go.transform as RectTransform, path, issues);
                ScanSelectable(go, path, issues);
                ScanScrollAndRangeControls(go, path, issues);
                ScanLayout(go, path, issues);
            }

            return issues;
        }

        private static IEnumerable<GameObject> GetLoadedSceneObjects()
        {
            var objects = Resources.FindObjectsOfTypeAll<GameObject>();
            foreach (var go in objects)
            {
                if (go == null) continue;
                if (EditorUtility.IsPersistent(go)) continue;

                var scene = go.scene;
                if (!scene.IsValid() || !scene.isLoaded) continue;

                yield return go;
            }
        }

        private static IEnumerable<Scene> GetLoadedScenes()
        {
            int count = SceneManager.sceneCount;
            for (int i = 0; i < count; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                if (scene.IsValid() && scene.isLoaded)
                {
                    yield return scene;
                }
            }
        }

        private static void ScanTransform(Transform transform, string path, List<Issue> issues)
        {
            var localPosition = transform.localPosition;
            var localRotation = transform.localRotation;
            var localScale = transform.localScale;
            var lossyScale = transform.lossyScale;

            if (HasInvalidValue(localRotation))
            {
                Add(issues, path, transform, "localRotation", Format(localRotation), "重置为 Quaternion.identity。");
            }
            else
            {
                float length = QuaternionLength(localRotation);
                if (length < QUATERNION_MIN_LENGTH || length > QUATERNION_MAX_LENGTH)
                {
                    Add(issues, path, transform, "localRotation.length", $"{Format(localRotation)} length={length}", "归一化 localRotation；若长度无效则重置为 Quaternion.identity。");
                }

                if (IsCloseToReportedQuaternion(localRotation))
                {
                    Add(issues, path, transform, "localRotation", Format(localRotation), "该值接近 Console 报错 Quaternion，优先检查并归一化。");
                }
            }

            if (HasInvalidValue(localPosition))
            {
                Add(issues, path, transform, "localPosition", Format(localPosition), "重置为有限数值，例如 (0,0,0)。");
            }

            if (HasInvalidValue(localScale))
            {
                Add(issues, path, transform, "localScale", Format(localScale), "重置为有限数值，例如 (1,1,1)。");
            }
            else if (IsNearZero(localScale.x) || IsNearZero(localScale.y) || IsNearZero(localScale.z))
            {
                Add(issues, path, transform, "localScale", Format(localScale), "避免任意轴接近 0；UI 对象通常应使用 (1,1,1)。");
            }

            if (HasInvalidValue(lossyScale))
            {
                Add(issues, path, transform, "lossyScale", Format(lossyScale), "检查父级缩放链，避免 NaN / Infinity。");
            }
            else if (IsNearZero(lossyScale.x) || IsNearZero(lossyScale.y) || IsNearZero(lossyScale.z))
            {
                Add(issues, path, transform, "lossyScale", Format(lossyScale), "检查本对象或父级是否存在 0 缩放。");
            }
        }

        private static void ScanRectTransform(RectTransform rect, string path, List<Issue> issues)
        {
            if (rect == null) return;

            CheckVector(issues, path, rect, "anchoredPosition", rect.anchoredPosition, "重置为有限坐标。");
            CheckVector(issues, path, rect, "sizeDelta", rect.sizeDelta, "重置为有限尺寸。");
            CheckVector(issues, path, rect, "anchorMin", rect.anchorMin, "重置为合法锚点。");
            CheckVector(issues, path, rect, "anchorMax", rect.anchorMax, "重置为合法锚点。");
            CheckVector(issues, path, rect, "pivot", rect.pivot, "重置为合法 pivot。");
            CheckVector(issues, path, rect, "offsetMin", rect.offsetMin, "重置为有限 offset。");
            CheckVector(issues, path, rect, "offsetMax", rect.offsetMax, "重置为有限 offset。");
        }

        private static void CheckVector(List<Issue> issues, string path, Component component, string field, Vector2 value, string suggestion)
        {
            if (HasInvalidValue(value))
            {
                Add(issues, path, component, field, Format(value), suggestion);
                return;
            }

            if (Mathf.Abs(value.x) > HUGE_VALUE || Mathf.Abs(value.y) > HUGE_VALUE)
            {
                Add(issues, path, component, field, Format(value), "数值过大，建议恢复到合理 UI 范围。");
            }
        }

        private static void ScanSelectable(GameObject go, string path, List<Issue> issues)
        {
            var selectable = go.GetComponent<Selectable>();
            if (selectable == null) return;

            var navigation = selectable.navigation;
            if (navigation.mode == Navigation.Mode.Automatic)
            {
                Add(issues, path, selectable, "navigation.mode", navigation.mode.ToString(), "建议设为 Navigation.Mode.None。");
            }

            CheckNavigationTarget(issues, path, selectable, "selectOnUp", navigation.selectOnUp);
            CheckNavigationTarget(issues, path, selectable, "selectOnDown", navigation.selectOnDown);
            CheckNavigationTarget(issues, path, selectable, "selectOnLeft", navigation.selectOnLeft);
            CheckNavigationTarget(issues, path, selectable, "selectOnRight", navigation.selectOnRight);
        }

        private static void CheckNavigationTarget(List<Issue> issues, string path, Selectable owner, string field, Selectable target)
        {
            if (target == null) return;

            if (target == owner)
            {
                Add(issues, path, owner, "navigation." + field, GetHierarchyPath(target.transform), "清空自引用或设为 None。");
                return;
            }

            if (!target.gameObject.activeInHierarchy)
            {
                Add(issues, path, owner, "navigation." + field, GetHierarchyPath(target.transform), "目标对象 inactive，建议清空或重新绑定。");
            }
        }

        private static void ScanScrollAndRangeControls(GameObject go, string path, List<Issue> issues)
        {
            var scrollRect = go.GetComponent<ScrollRect>();
            if (scrollRect != null)
            {
                if (scrollRect.content == null) Add(issues, path, scrollRect, "content", "null", "ScrollRect 通常需要有效 Content。");
                if (scrollRect.viewport == null) Add(issues, path, scrollRect, "viewport", "null", "建议绑定 Viewport RectTransform。");
            }

            var slider = go.GetComponent<Slider>();
            if (slider != null)
            {
                if (!Enum.IsDefined(typeof(Slider.Direction), slider.direction)) Add(issues, path, slider, "direction", slider.direction.ToString(), "重置为合法 Slider.Direction。");
                if (slider.handleRect == null) Add(issues, path, slider, "handleRect", "null", "Slider 通常需要有效 Handle Rect。");
            }

            var scrollbar = go.GetComponent<Scrollbar>();
            if (scrollbar != null)
            {
                if (!Enum.IsDefined(typeof(Scrollbar.Direction), scrollbar.direction)) Add(issues, path, scrollbar, "direction", scrollbar.direction.ToString(), "重置为合法 Scrollbar.Direction。");
                if (scrollbar.handleRect == null) Add(issues, path, scrollbar, "handleRect", "null", "Scrollbar 需要有效 Handle Rect。");
            }
        }

        private static void ScanLayout(GameObject go, string path, List<Issue> issues)
        {
            var layoutGroup = go.GetComponent<LayoutGroup>();
            var fitter = go.GetComponent<ContentSizeFitter>();
            if (layoutGroup == null && fitter == null) return;

            var rect = go.transform as RectTransform;
            if (rect == null) return;

            var currentRect = rect.rect;
            if (Mathf.Abs(currentRect.width) <= SCALE_EPSILON || Mathf.Abs(currentRect.height) <= SCALE_EPSILON)
            {
                var component = layoutGroup != null ? (Component)layoutGroup : fitter;
                Add(issues, path, component, "rect.size", $"width={currentRect.width}, height={currentRect.height}", "参与布局计算的对象建议避免 0 宽高。");
            }
        }

        private static bool FixTransform(string path, Transform transform)
        {
            bool changed = false;
            var rotation = transform.localRotation;

            if (HasInvalidValue(rotation) || QuaternionLength(rotation) <= SCALE_EPSILON)
            {
                Debug.LogWarning($"[Recorder SDK] 修复非法 Quaternion：{path} {Format(rotation)} -> identity");
                Undo.RecordObject(transform, "Fix invalid transform");
                transform.localRotation = Quaternion.identity;
                changed = true;
            }
            else if (QuaternionLength(rotation) < QUATERNION_MIN_LENGTH || QuaternionLength(rotation) > QUATERNION_MAX_LENGTH)
            {
                var normalized = NormalizeQuaternion(rotation);
                Debug.LogWarning($"[Recorder SDK] 归一化 Quaternion：{path} {Format(rotation)} -> {Format(normalized)}");
                Undo.RecordObject(transform, "Fix invalid transform");
                transform.localRotation = normalized;
                changed = true;
            }

            if (HasInvalidValue(transform.localPosition))
            {
                Debug.LogWarning($"[Recorder SDK] 修复非法 localPosition：{path} {Format(transform.localPosition)} -> (0,0,0)");
                Undo.RecordObject(transform, "Fix invalid transform");
                transform.localPosition = Vector3.zero;
                changed = true;
            }

            if (HasInvalidValue(transform.localScale))
            {
                Debug.LogWarning($"[Recorder SDK] 修复非法 localScale：{path} {Format(transform.localScale)} -> (1,1,1)");
                Undo.RecordObject(transform, "Fix invalid transform");
                transform.localScale = Vector3.one;
                changed = true;
            }
            else if (IsZeroVector(transform.localScale) && !IsCanvasRoot(transform))
            {
                Debug.LogWarning($"[Recorder SDK] 修复 0 localScale：{path} {Format(transform.localScale)} -> (1,1,1)");
                Undo.RecordObject(transform, "Fix invalid transform");
                transform.localScale = Vector3.one;
                changed = true;
            }
            else if (IsZeroVector(transform.localScale) && IsCanvasRoot(transform))
            {
                Debug.LogWarning($"[Recorder SDK] 跳过 Canvas 根对象 0 localScale：{path}。请人工确认是否改为 (1,1,1)。");
            }

            return changed;
        }

        private static bool FixNavigation(string path, GameObject go)
        {
            var selectable = go.GetComponent<Selectable>();
            if (selectable == null) return false;

            var navigation = selectable.navigation;
            bool changed = false;
            if (navigation.mode == Navigation.Mode.Automatic)
            {
                navigation.mode = Navigation.Mode.None;
                changed = true;
            }

            if (navigation.selectOnUp == selectable) { navigation.selectOnUp = null; changed = true; }
            if (navigation.selectOnDown == selectable) { navigation.selectOnDown = null; changed = true; }
            if (navigation.selectOnLeft == selectable) { navigation.selectOnLeft = null; changed = true; }
            if (navigation.selectOnRight == selectable) { navigation.selectOnRight = null; changed = true; }

            if (!changed) return false;

            Debug.LogWarning($"[Recorder SDK] 修复 Selectable Navigation：{path}");
            Undo.RecordObject(selectable, "Fix selectable navigation");
            selectable.navigation = navigation;
            return true;
        }

        private static bool IsCanvasRoot(Transform transform)
        {
            return transform.GetComponent<Canvas>() != null && transform.parent == null;
        }

        private static void LogReport(string title, List<Issue> issues)
        {
            var builder = new StringBuilder(4096);
            builder.AppendLine("[Recorder SDK] " + title);
            builder.AppendLine("扫描范围：当前已加载场景中的所有 GameObject（含 inactive，排除资源文件）。");
            builder.AppendLine("发现问题数：" + issues.Count);

            if (issues.Count == 0)
            {
                builder.AppendLine("未发现明显异常。若 Assertion 仍出现，请结合 Console 堆栈检查正在运行的脚本或 Prefab 资源。");
                Debug.Log(builder.ToString());
                return;
            }

            for (int i = 0; i < issues.Count; i++)
            {
                var issue = issues[i];
                builder.AppendLine();
                builder.Append(i + 1).Append(". ").AppendLine(issue.path);
                builder.Append("   组件：").AppendLine(issue.componentType);
                builder.Append("   字段：").AppendLine(issue.field);
                builder.Append("   当前值：").AppendLine(issue.value);
                builder.Append("   建议：").AppendLine(issue.suggestion);
            }

            Debug.LogWarning(builder.ToString());
        }

        private static void Add(List<Issue> issues, string path, Component component, string field, string value, string suggestion)
        {
            issues.Add(new Issue
            {
                path = path,
                componentType = component != null ? component.GetType().FullName : "(missing component)",
                field = field,
                value = value,
                suggestion = suggestion
            });
        }

        private static bool HasInvalidValue(Vector2 value)
        {
            return IsInvalid(value.x) || IsInvalid(value.y);
        }

        private static bool HasInvalidValue(Vector3 value)
        {
            return IsInvalid(value.x) || IsInvalid(value.y) || IsInvalid(value.z);
        }

        private static bool HasInvalidValue(Quaternion value)
        {
            return IsInvalid(value.x) || IsInvalid(value.y) || IsInvalid(value.z) || IsInvalid(value.w);
        }

        private static bool IsInvalid(float value)
        {
            return float.IsNaN(value) || float.IsInfinity(value);
        }

        private static bool IsNearZero(float value)
        {
            return Mathf.Abs(value) <= SCALE_EPSILON;
        }

        private static bool IsZeroVector(Vector3 value)
        {
            return IsNearZero(value.x) && IsNearZero(value.y) && IsNearZero(value.z);
        }

        private static float QuaternionLength(Quaternion value)
        {
            return Mathf.Sqrt(value.x * value.x + value.y * value.y + value.z * value.z + value.w * value.w);
        }

        private static Quaternion NormalizeQuaternion(Quaternion value)
        {
            float length = QuaternionLength(value);
            if (length <= SCALE_EPSILON || float.IsNaN(length) || float.IsInfinity(length)) return Quaternion.identity;
            return new Quaternion(value.x / length, value.y / length, value.z / length, value.w / length);
        }

        private static bool IsCloseToReportedQuaternion(Quaternion value)
        {
            return Mathf.Abs(value.x - reportedInvalidQuaternion.x) < 0.0005f
                   && Mathf.Abs(value.y - reportedInvalidQuaternion.y) < 0.0005f
                   && Mathf.Abs(value.z - reportedInvalidQuaternion.z) < 0.0005f
                   && Mathf.Abs(value.w - reportedInvalidQuaternion.w) < 0.0005f;
        }

        private static string Format(Vector2 value)
        {
            return $"({value.x}, {value.y})";
        }

        private static string Format(Vector3 value)
        {
            return $"({value.x}, {value.y}, {value.z})";
        }

        private static string Format(Quaternion value)
        {
            return $"({value.x}, {value.y}, {value.z}, {value.w})";
        }

        private static string GetHierarchyPath(Transform transform)
        {
            if (transform == null) return "(missing)";

            var builder = new StringBuilder(transform.name);
            var current = transform.parent;
            while (current != null)
            {
                builder.Insert(0, current.name + "/");
                current = current.parent;
            }

            var scene = transform.gameObject.scene;
            if (scene.IsValid())
            {
                builder.Insert(0, scene.name + ":");
            }

            return builder.ToString();
        }

        private struct Issue
        {
            public string path;
            public string componentType;
            public string field;
            public string value;
            public string suggestion;
        }
    }
}
