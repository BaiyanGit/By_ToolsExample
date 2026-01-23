using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace _3rdBy.ByTools.GenerateAssets.Editor
{
    /// <summary>
    /// Assets资源脚本生成器
    /// </summary>
    public class AssetGeneratorEditorWindow : EditorWindow
    {
        private class ScriptEntry
        {
            public MonoScript script;
            public bool selected;
        }

        private readonly List<ScriptEntry> _scripts = new();
        private readonly List<string> _existFiles = new();
        private Vector2 _scroll;
        private static string _outputFolder = "";
        private const string Desc = "生成.Asset配置文件工具，支持批量生成 ScriptableObject 资产。(支持所有的Mono脚本)";
        private const string OutputFolderKey = "OutputFolderKey";

        [MenuItem("ByTools/🗂️ .asset生成工具")]
        public static void Open()
        {
            GetWindow<AssetGeneratorEditorWindow>("SO资产生成器");
        }

        private void OnEnable()
        {
            GetThisScriptPath();
        }

        // 更新输出目录
        private static void UpdateOutFolder(string path)
        {
            PlayerPrefs.SetString(OutputFolderKey, path);
            _outputFolder = path;
            PlayerPrefs.Save();
        }

        // 获取上级目录
        private static string GetUpperDirectory(string path, int level, string lastFold = "")
        {
            var currentPath = path;

            for (var i = 0; i < level; i++)
            {
                currentPath = Path.GetDirectoryName(currentPath);

                if (string.IsNullOrEmpty(currentPath))
                    return null;
            }

            lastFold    = string.IsNullOrEmpty(lastFold) ? lastFold : $"{lastFold}/";
            currentPath = $"{currentPath.Replace('\\', '/')}/{lastFold}";
            return currentPath;
        }

        // 获取当前脚本路径
        private void GetThisScriptPath()
        {
            var outputFolder = PlayerPrefs.GetString(OutputFolderKey, string.Empty);
            if (!string.IsNullOrEmpty(outputFolder))
            {
                _outputFolder = outputFolder;
                return;
            }

            var monoScript     = MonoScript.FromScriptableObject(this);
            var scriptFullPath = AssetDatabase.GetAssetPath(monoScript);
            var path           = GetUpperDirectory(scriptFullPath, 2, "AssetFiles");
            UpdateOutFolder(path);
        }

        private void OnGUI()
        {
            EditorGUILayout.HelpBox(Desc, MessageType.Info);

            EditorGUILayout.Space();
            DrawOutputPath();
            EditorGUILayout.Space();
            DrawScriptList();
            EditorGUILayout.Space();
            DrawDropArea();
            EditorGUILayout.Space();
            DrawGenerateButton();
        }

        // 绘制拖拽区域
        private void DrawDropArea()
        {
            var rect = GUILayoutUtility.GetRect(0, 70, GUILayout.ExpandWidth(true));

            var label = new GUIStyle(EditorStyles.helpBox)
            {
                normal =
                {
                    textColor = Color.white
                },
                alignment = TextAnchor.MiddleCenter,
                fontSize  = 12,
                fontStyle = FontStyle.Bold
            };
            GUI.Box(rect, "拖拽 ScriptableObject 脚本到这里（支持跨路径）", label);
            var evt = Event.current;
            if (!rect.Contains(evt.mousePosition)) return;

            if (evt.type is EventType.DragUpdated or EventType.DragPerform)
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                if (evt.type == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();
                    foreach (var obj in DragAndDrop.objectReferences)
                    {
                        if (obj is MonoScript ms)
                        {
                            var type = ms.GetClass();
                            if (type != null && type.IsSubclassOf(typeof(ScriptableObject)))
                            {
                                AddScript(ms);
                            }
                            else
                            {
                                if (type != null) Debug.Log($"❌生成.assets失败\n{type.Name}.cs 未继承 ScriptableObject");
                            }
                        }
                    }
                }

                evt.Use();
            }
        }

        // 添加脚本
        private void AddScript(MonoScript script)
        {
            if (_scripts.Exists(s => s.script == script)) return;
            _scripts.Add(new ScriptEntry { script = script, selected = true });
        }

        // 绘制脚本列表
        private void DrawScriptList()
        {
            EditorGUILayout.LabelField("脚本列表", EditorStyles.boldLabel);
            _scroll = EditorGUILayout.BeginScrollView(_scroll, GUILayout.Height(200));

            for (var i = _scripts.Count - 1; i >= 0; i--)
            {
                var entry = _scripts[i];
                EditorGUILayout.BeginHorizontal();
                entry.selected = EditorGUILayout.Toggle(entry.selected, GUILayout.Width(20));
                EditorGUILayout.LabelField($"{i}.", GUILayout.Width(10));
                EditorGUILayout.ObjectField(entry.script, typeof(MonoScript), false);
                if (GUILayout.Button("移除", GUILayout.Width(50)))
                {
                    _scripts.RemoveAt(i);
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();
        }

        // 绘制输出路径
        private void DrawOutputPath()
        {
            EditorGUILayout.LabelField("生成路径", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.TextField(_outputFolder);
            if (GUILayout.Button("选择", GUILayout.Width(60)))
            {
                var path = EditorUtility.OpenFolderPanel("🫳 选择生成路径", Application.dataPath, "");
                if (!string.IsNullOrEmpty(path))
                {
                    if (!path.StartsWith(Application.dataPath))
                    {
                        EditorUtility.DisplayDialog("❌ 错误", "必须选择 Assets 目录下的路径", "OK");
                    }
                    else
                    {
                        var selectedFolder = $"Assets{path.Substring(Application.dataPath.Length)}";
                        UpdateOutFolder(selectedFolder);
                    }
                }
            }

            if (GUILayout.Button("默认", GUILayout.Width(60)))
            {
                PlayerPrefs.DeleteKey(OutputFolderKey);
                GetThisScriptPath();
            }

            EditorGUILayout.EndHorizontal();
        }

        // 绘制生成按钮
        private void DrawGenerateButton()
        {
            using (new EditorGUI.DisabledScope(_scripts.Count == 0))
            {
                if (GUILayout.Button("生成选中的 Asset", GUILayout.Height(30)))
                {
                    GenerateAssets();
                }
            }
        }

        // 生成 Asset
        private void GenerateAssets()
        {
            var generaCount  = 0; // 生成文件数量
            var outputFolder = PlayerPrefs.GetString(OutputFolderKey, string.Empty);
            if (string.IsNullOrEmpty(outputFolder))
            {
                EditorUtility.DisplayDialog("❌ 错误", "请选择一个生成路径...", "好的");
                return;
            }

            // 创建文件目录
            if (!Directory.Exists(outputFolder))
            {
                Directory.CreateDirectory(outputFolder);
            }

            var existIndex = 0; // 重复文件数量
            _existFiles.Clear();
            foreach (var entry in _scripts)
            {
                if (!entry.selected) continue;
                var type = entry.script.GetClass();
                if (type == null) continue;

                var assetPath = Path.Combine(outputFolder, $"{type.Name}.asset");
                assetPath = assetPath.Replace("\\", "/");
                if (File.Exists(assetPath))
                {
                    existIndex++;
                    _existFiles.Add($"{existIndex}、 {type.Name}.assets");
                }
            }

            if (_existFiles.Count > 0)
            {
                var selectRes = EditorUtility.DisplayDialog("📄 文件已存在", $"{string.Join("\n", _existFiles)}", "全部覆盖", "取消");
                if (!selectRes) return; // 用户选择了取消
            }

            foreach (var entry in _scripts)
            {
                if (!entry.selected) continue;

                var type = entry.script.GetClass();
                if (type == null) continue;

                var asset     = CreateInstance(type);
                var assetPath = Path.Combine(outputFolder, $"{type.Name}.asset");
                assetPath = assetPath.Replace("\\", "/");

                if (File.Exists(assetPath))
                {
                    AssetDatabase.DeleteAsset(assetPath);
                }

                generaCount++;
                AssetDatabase.CreateAsset(asset, assetPath);
                Debug.Log($"[ <color=green>🗂️Assets</color> ] {generaCount}. Path: {assetPath}");
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // EditorUtility.DisplayDialog("完成", $"⚠️ 您已生成{generaCount}个Asset文件", "OK");
        }
    }
}