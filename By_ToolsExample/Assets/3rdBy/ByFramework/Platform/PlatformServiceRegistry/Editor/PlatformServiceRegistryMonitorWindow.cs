//=====================================================
// 文件名称: PlatformServiceRegistryMonitorWindow.cs
// 创 建 者: Codex
// 创建日期: 2026-06-08
// 描    述: 提供 PlatformServiceRegistry 的 Editor 监视窗口。
//=====================================================

namespace _3rdBy.ByFramework.Platform.PlatformServiceRegistry.Editor
{
    using System;
    using System.Collections.Generic;
    using Registry = _3rdBy.ByFramework.Platform.PlatformServiceRegistry.PlatformServiceRegistry;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// PlatformServiceRegistry 监视窗口。
    /// </summary>
    public sealed class PlatformServiceRegistryMonitorWindow : EditorWindow
    {
        private const string LogPrefix = "[PlatformServiceRegistry]";

        [Header("服务表滚动位置。")]
        private Vector2 _scrollPosition;

        [Header("当前缓存的服务描述列表。")]
        private IReadOnlyList<ServiceDescriptor> _descriptors = Array.Empty<ServiceDescriptor>();

        [Header("最近一次验证结果文本。")]
        private string _validationMessage = "尚未执行验证。";

        [MenuItem("ByFramework/平台/PlatformServiceRegistry 监视器")]
        private static void Open()
        {
            PlatformServiceRegistryMonitorWindow window = GetWindow<PlatformServiceRegistryMonitorWindow>("PlatformServiceRegistry 监视器");
            window.minSize = new Vector2(960f, 420f);
            window.RefreshDescriptors();
        }

        private void OnEnable()
        {
            RefreshDescriptors();
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("PlatformServiceRegistry 监视器", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("用于查看当前已注册 Platform 服务、状态和标记接口实现情况。", MessageType.Info);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("刷新服务列表", GUILayout.Height(28f)))
                {
                    RefreshDescriptors();
                }

                if (GUILayout.Button("验证服务表", GUILayout.Height(28f)))
                {
                    ValidateDescriptors();
                }

                if (GUILayout.Button("清空服务表", GUILayout.Height(28f)))
                {
                    ClearRegistryWithConfirm();
                }
            }

            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField($"当前服务数量：{_descriptors.Count}", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(_validationMessage, MessageType.None);

            DrawTableHeader();

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            if (_descriptors.Count == 0)
            {
                EditorGUILayout.HelpBox("当前未注册任何 Platform 服务。", MessageType.Warning);
            }
            else
            {
                for (int i = 0; i < _descriptors.Count; i++)
                {
                    DrawDescriptorRow(_descriptors[i], i);
                }
            }

            EditorGUILayout.EndScrollView();
        }

        private void RefreshDescriptors()
        {
            _descriptors = Registry.GetAllDescriptors();
            Repaint();
        }

        private void ValidateDescriptors()
        {
            RefreshDescriptors();

            int failedCount = 0;
            int unmarkedCount = 0;

            for (int i = 0; i < _descriptors.Count; i++)
            {
                ServiceDescriptor descriptor = _descriptors[i];
                if (descriptor.IsPlatformService == false)
                {
                    unmarkedCount++;
                }

                if (descriptor.State == ServiceState.Failed)
                {
                    failedCount++;
                }
            }

            _validationMessage = $"验证完成：服务总数 {_descriptors.Count}，未实现 IPlatformService {unmarkedCount} 个，Failed 状态 {failedCount} 个。";

            if (unmarkedCount > 0 || failedCount > 0)
            {
                Debug.LogWarning($"{LogPrefix} 服务注册表验证完成，发现异常项。{_validationMessage}");
            }
            else
            {
                Debug.Log($"{LogPrefix} 服务注册表验证完成。{_validationMessage}");
            }
        }

        private void ClearRegistryWithConfirm()
        {
            bool confirmed = EditorUtility.DisplayDialog(
                "确认清空服务表",
                "该操作会清空当前 PlatformServiceRegistry 中的所有服务，仅用于验证和调试。是否继续？",
                "确认清空",
                "取消");

            if (confirmed == false)
            {
                return;
            }

            Registry.Clear();
            RefreshDescriptors();
            _validationMessage = "已清空当前服务表。";
            Debug.Log($"{LogPrefix} 已清空当前服务表。");
        }

        private static void DrawTableHeader()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
            {
                GUILayout.Label("服务接口", EditorStyles.boldLabel, GUILayout.Width(220f));
                GUILayout.Label("实现类型", EditorStyles.boldLabel, GUILayout.Width(240f));
                GUILayout.Label("状态", EditorStyles.boldLabel, GUILayout.Width(100f));
                GUILayout.Label("注册时间", EditorStyles.boldLabel, GUILayout.Width(160f));
                GUILayout.Label("来源", EditorStyles.boldLabel, GUILayout.Width(260f));
                GUILayout.Label("IPlatformService", EditorStyles.boldLabel, GUILayout.Width(120f));
            }
        }

        private static void DrawDescriptorRow(ServiceDescriptor descriptor, int index)
        {
            GUIStyle rowStyle = (index & 1) == 0 ? EditorStyles.helpBox : EditorStyles.textArea;

            using (new EditorGUILayout.HorizontalScope(rowStyle))
            {
                GUILayout.Label(GetTypeName(descriptor.ServiceType), GUILayout.Width(220f));
                GUILayout.Label(GetTypeName(descriptor.ImplementationType), GUILayout.Width(240f));
                GUILayout.Label(descriptor.State.ToString(), GUILayout.Width(100f));
                GUILayout.Label(descriptor.RegisterTime.ToString("yyyy-MM-dd HH:mm:ss"), GUILayout.Width(160f));
                GUILayout.Label(descriptor.Source, GUILayout.Width(260f));
                GUILayout.Label(descriptor.IsPlatformService ? "已实现" : "未实现", GUILayout.Width(120f));
            }
        }

        private static string GetTypeName(Type type)
        {
            return type == null ? "<空>" : type.FullName ?? type.Name;
        }
    }
}
