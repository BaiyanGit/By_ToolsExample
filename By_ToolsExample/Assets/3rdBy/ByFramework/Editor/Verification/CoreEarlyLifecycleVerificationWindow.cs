namespace _3rdBy.ByFramework.Editor.Verification
{
    using System.Collections.Generic;
    using System.Text;
    using _3rdBy.ByFramework.Core;
    using _3rdBy.ByFramework.FSM;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using ThreadDispatcher = _3rdBy.ByFramework.Extension.DispatcherThread;
    using FrameworkEventManager = _3rdBy.ByFramework.EventManager.Core.EventManager;

    public sealed class CoreEarlyLifecycleVerificationWindow : EditorWindow
    {
        private const string LogPrefix = "[ByFramework][Core Early 生命周期验证]";
        private const string FrameworkRootName = "[ByFramework]";

        private Vector2 _scrollPosition;
        private string _lastReport = "尚未生成验证报告。";

        [MenuItem("ByFramework/验证/Core Early 生命周期验证")]
        private static void Open()
        {
            GetWindow<CoreEarlyLifecycleVerificationWindow>("Core Early 生命周期验证");
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Core Early 生命周期验证", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "只读验证工具，用于检查 FrameworkEntry Phase2A 的 Core Early 生命周期链：ThreadDispatcher -> EventManager -> FSMManager。",
                MessageType.Info);

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                if (GUILayout.Button("检查 Core Early 对象"))
                {
                    CheckCoreEarlyObjects();
                }

                if (GUILayout.Button("检查重复实例"))
                {
                    CheckDuplicateInstances();
                }

                if (GUILayout.Button("检查生命周期静态状态"))
                {
                    CheckLifecycleStaticState();
                }

                if (GUILayout.Button("检查场景预放置实例"))
                {
                    CheckScenePreplacedInstances();
                }

                if (GUILayout.Button("打印验证报告"))
                {
                    PrintVerificationReport();
                }
            }

            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("最近一次报告", EditorStyles.boldLabel);

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            EditorGUILayout.TextArea(_lastReport, GUILayout.ExpandHeight(true));
            EditorGUILayout.EndScrollView();
        }

        private void CheckCoreEarlyObjects()
        {
            Snapshot snapshot = CreateSnapshot();
            var builder = CreateReportHeader("检查 Core Early 对象");

            AppendObjectCounts(builder, snapshot);
            AppendStaticAccessState(builder);
            AppendDuplicateSummary(builder, snapshot);

            LogReport(builder);
        }

        private void CheckDuplicateInstances()
        {
            Snapshot snapshot = CreateSnapshot();
            var builder = CreateReportHeader("检查重复实例");

            AppendObjectCounts(builder, snapshot);
            AppendDuplicateLine(builder, FrameworkRootName, snapshot.FrameworkRoots.Count, 1);
            AppendDuplicateLine(builder, nameof(FrameworkEntry), snapshot.FrameworkEntries.Count, 1);
            AppendDuplicateLine(builder, "ThreadDispatcher", snapshot.ThreadDispatchers.Count, 1);
            AppendDuplicateLine(builder, "EventManager", snapshot.EventManagers.Count, 1);
            AppendDuplicateLine(builder, nameof(FSMManager), snapshot.FsmManagers.Count, 1);

            LogReport(builder);
        }

        private void CheckLifecycleStaticState()
        {
            Snapshot snapshot = CreateSnapshot();
            var builder = CreateReportHeader("检查生命周期静态状态");

            builder.AppendLine($"Application.isPlaying: {Application.isPlaying}");
            builder.AppendLine($"FrameworkEntry.Instance 可访问状态: {FormatObjectState(FrameworkEntry.Instance)}");
            builder.AppendLine($"ThreadDispatcher.Current 可访问状态: {FormatObjectState(ThreadDispatcher.Current)}");
            builder.AppendLine($"EventManager.Instance 可访问状态: {FormatObjectState(FrameworkEventManager.Instance)}");
            builder.AppendLine($"FSMManager.Instance 可访问状态: {FormatObjectState(FSMManager.Instance)}");
            builder.AppendLine();
            AppendObjectCounts(builder, snapshot);

            LogReport(builder);
        }

        private void CheckScenePreplacedInstances()
        {
            Snapshot snapshot = CreateSnapshot();
            var builder = CreateReportHeader("检查场景预放置实例");

            builder.AppendLine("预放置实例指对象属于已加载场景资源，且 scene path 非空。");
            builder.AppendLine("DontDestroyOnLoad 下运行时创建的对象通常 scene path 为空，不计入预放置实例。");
            builder.AppendLine();
            AppendPreplacedLine(builder, FrameworkRootName, snapshot.FrameworkRoots);
            AppendPreplacedLine(builder, nameof(FrameworkEntry), snapshot.FrameworkEntries);
            AppendPreplacedLine(builder, "ThreadDispatcher", snapshot.ThreadDispatchers);
            AppendPreplacedLine(builder, "EventManager", snapshot.EventManagers);
            AppendPreplacedLine(builder, nameof(FSMManager), snapshot.FsmManagers);

            LogReport(builder);
        }

        private void PrintVerificationReport()
        {
            Snapshot snapshot = CreateSnapshot();
            var builder = CreateReportHeader("Core Early 生命周期验证报告");

            builder.AppendLine("预期初始化顺序: ThreadDispatcher -> EventManager -> FSMManager");
            builder.AppendLine("预期 Shutdown 顺序: FSMManager -> EventManager -> ThreadDispatcher");
            builder.AppendLine();
            AppendObjectCounts(builder, snapshot);
            AppendStaticAccessState(builder);
            AppendDuplicateSummary(builder, snapshot);
            builder.AppendLine();
            builder.AppendLine("人工验证结论:");
            builder.AppendLine("- 完成 PlayMode、Domain Reload、场景切换与 Shutdown 检查后，填写 Documentation/18_CoreEarlyLifecycleUnityVerification.md。");
            builder.AppendLine("- P3.5A 已完成本地 Unity 验证，本窗口继续用于后续回归验证。");

            LogReport(builder);
        }

        private static Snapshot CreateSnapshot()
        {
            return new Snapshot
            {
                FrameworkRoots = FindSceneGameObjectsByName(FrameworkRootName),
                FrameworkEntries = FindSceneComponents<FrameworkEntry>(),
                ThreadDispatchers = FindSceneComponents<ThreadDispatcher>(),
                EventManagers = FindSceneComponents<FrameworkEventManager>(),
                FsmManagers = FindSceneComponents<FSMManager>()
            };
        }

        private static StringBuilder CreateReportHeader(string title)
        {
            var builder = new StringBuilder();
            builder.AppendLine($"{LogPrefix} {title}");
            builder.AppendLine($"Unity 版本: {Application.unityVersion}");
            builder.AppendLine($"Domain Reload 状态: {GetDomainReloadState()}");
            builder.AppendLine($"当前场景: {GetActiveSceneDescription()}");
            builder.AppendLine($"已加载场景数量: {SceneManager.sceneCount}");
            builder.AppendLine($"Play Mode 状态: {Application.isPlaying}");
            builder.AppendLine();
            return builder;
        }

        private static void AppendObjectCounts(StringBuilder builder, Snapshot snapshot)
        {
            builder.AppendLine("对象数量:");
            builder.AppendLine($"- {FrameworkRootName}: {snapshot.FrameworkRoots.Count}");
            builder.AppendLine($"- {nameof(FrameworkEntry)}: {snapshot.FrameworkEntries.Count}");
            builder.AppendLine($"- ThreadDispatcher: {snapshot.ThreadDispatchers.Count}");
            builder.AppendLine($"- EventManager: {snapshot.EventManagers.Count}");
            builder.AppendLine($"- {nameof(FSMManager)}: {snapshot.FsmManagers.Count}");
            builder.AppendLine();
        }

        private static void AppendStaticAccessState(StringBuilder builder)
        {
            builder.AppendLine("静态访问状态:");
            builder.AppendLine($"- FrameworkEntry.Instance: {FormatObjectState(FrameworkEntry.Instance)}");
            builder.AppendLine($"- ThreadDispatcher.Current: {FormatObjectState(ThreadDispatcher.Current)}");
            builder.AppendLine($"- EventManager.Instance: {FormatObjectState(FrameworkEventManager.Instance)}");
            builder.AppendLine($"- FSMManager.Instance: {FormatObjectState(FSMManager.Instance)}");
            builder.AppendLine();
        }

        private static void AppendDuplicateSummary(StringBuilder builder, Snapshot snapshot)
        {
            builder.AppendLine("重复实例汇总:");
            AppendDuplicateLine(builder, FrameworkRootName, snapshot.FrameworkRoots.Count, 1);
            AppendDuplicateLine(builder, nameof(FrameworkEntry), snapshot.FrameworkEntries.Count, 1);
            AppendDuplicateLine(builder, "ThreadDispatcher", snapshot.ThreadDispatchers.Count, 1);
            AppendDuplicateLine(builder, "EventManager", snapshot.EventManagers.Count, 1);
            AppendDuplicateLine(builder, nameof(FSMManager), snapshot.FsmManagers.Count, 1);
        }

        private static void AppendDuplicateLine(StringBuilder builder, string label, int count, int expectedMaximum)
        {
            string status = count <= expectedMaximum ? "正常" : "重复";
            builder.AppendLine($"- {label}: {count} / 最大允许 {expectedMaximum} => {status}");
        }

        private static void AppendPreplacedLine<T>(StringBuilder builder, string label, IReadOnlyList<T> objects) where T : Component
        {
            int count = 0;
            var paths = new List<string>();

            foreach (T component in objects)
            {
                if (component == null || !IsPreplacedSceneObject(component.gameObject))
                {
                    continue;
                }

                count++;
                paths.Add(GetHierarchyPath(component.gameObject));
            }

            builder.AppendLine($"- {label}: {count}");
            foreach (string path in paths)
            {
                builder.AppendLine($"  - {path}");
            }
        }

        private static void AppendPreplacedLine(StringBuilder builder, string label, IReadOnlyList<GameObject> objects)
        {
            int count = 0;
            var paths = new List<string>();

            foreach (GameObject gameObject in objects)
            {
                if (gameObject == null || !IsPreplacedSceneObject(gameObject))
                {
                    continue;
                }

                count++;
                paths.Add(GetHierarchyPath(gameObject));
            }

            builder.AppendLine($"- {label}: {count}");
            foreach (string path in paths)
            {
                builder.AppendLine($"  - {path}");
            }
        }

        private void LogReport(StringBuilder builder)
        {
            _lastReport = builder.ToString();
            Debug.Log(_lastReport);
            Repaint();
        }

        private static List<T> FindSceneComponents<T>() where T : Component
        {
            var results = new List<T>();
            T[] objects = Resources.FindObjectsOfTypeAll<T>();

            foreach (T component in objects)
            {
                if (component == null || !IsSceneObject(component.gameObject))
                {
                    continue;
                }

                results.Add(component);
            }

            return results;
        }

        private static List<GameObject> FindSceneGameObjectsByName(string objectName)
        {
            var results = new List<GameObject>();
            GameObject[] objects = Resources.FindObjectsOfTypeAll<GameObject>();

            foreach (GameObject gameObject in objects)
            {
                if (gameObject == null || gameObject.name != objectName || !IsSceneObject(gameObject))
                {
                    continue;
                }

                results.Add(gameObject);
            }

            return results;
        }

        private static bool IsSceneObject(GameObject gameObject)
        {
            return gameObject != null && gameObject.scene.IsValid() && !EditorUtility.IsPersistent(gameObject);
        }

        private static bool IsPreplacedSceneObject(GameObject gameObject)
        {
            return IsSceneObject(gameObject) && !string.IsNullOrEmpty(gameObject.scene.path);
        }

        private static string FormatObjectState(Object target)
        {
            return target == null ? "空" : $"可访问 ({target.name})";
        }

        private static string GetActiveSceneDescription()
        {
            Scene scene = SceneManager.GetActiveScene();
            string path = string.IsNullOrEmpty(scene.path) ? "<未保存或运行时场景>" : scene.path;
            return $"{scene.name} ({path})";
        }

        private static string GetDomainReloadState()
        {
            if (!EditorSettings.enterPlayModeOptionsEnabled)
            {
                return "开启（默认 Enter Play Mode Options）";
            }

            bool disabled = (EditorSettings.enterPlayModeOptions & EnterPlayModeOptions.DisableDomainReload) != 0;
            return disabled ? "关闭" : "开启";
        }

        private static string GetHierarchyPath(GameObject gameObject)
        {
            var names = new Stack<string>();
            Transform current = gameObject.transform;

            while (current != null)
            {
                names.Push(current.name);
                current = current.parent;
            }

            string sceneName = gameObject.scene.IsValid() ? gameObject.scene.name : "<无效场景>";
            return $"{sceneName}/{string.Join("/", names)}";
        }

        private sealed class Snapshot
        {
            public List<GameObject> FrameworkRoots;
            public List<FrameworkEntry> FrameworkEntries;
            public List<ThreadDispatcher> ThreadDispatchers;
            public List<FrameworkEventManager> EventManagers;
            public List<FSMManager> FsmManagers;
        }
    }
}
