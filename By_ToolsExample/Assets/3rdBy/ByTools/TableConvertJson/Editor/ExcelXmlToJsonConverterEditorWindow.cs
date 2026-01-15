namespace _3rdBy.ByTools.TableConvertJson.Editor
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using ConvertHelper;
    using Excel.Generator;
    using UnityEditor;
    using UnityEngine;
    using Xml.Generator;

    /// <summary>
    /// 将Excel和XML文件转换为JSON格式的编辑器窗口
    /// </summary>
    public class ExcelXmlToJsonConverterEditorWindow : EditorWindow
    {
        #region 脚本重新编译 / 刷新编译器时，列表数据不丢处理

        [Serializable]
        private class DictWrapper
        {
            public List<string> keys = new();
            public List<string> values = new();
        }

        private const string SessionKeyElementTablePaths = "ExcelXmlToJson_ElementTablePaths";

        private void SaveSession()
        {
            if (_elementTablePaths == null) return;

            var wrapper = new DictWrapper();
            foreach (var kv in _elementTablePaths)
            {
                wrapper.keys.Add(kv.Key);
                wrapper.values.Add(kv.Value);
            }

            var json = JsonUtility.ToJson(wrapper);
            SessionState.SetString(SessionKeyElementTablePaths, json);
        }

        private void RestoreSession()
        {
            var json = SessionState.GetString(SessionKeyElementTablePaths, "");
            if (string.IsNullOrEmpty(json))
            {
                _elementTablePaths = new Dictionary<string, string>();
                return;
            }

            var wrapper = JsonUtility.FromJson<DictWrapper>(json);
            _elementTablePaths = new Dictionary<string, string>();

            for (int i = 0; i < wrapper.keys.Count; i++)
            {
                _elementTablePaths[wrapper.keys[i]] = wrapper.values[i];
            }
        }

        private void OnDisable()
        {
            SaveSession();
        }

        #endregion

        [Header("窗口大小")] private static readonly Vector2 windowSize = new(800, 600);
        [Header("单个文件路径")] private string _filePath;
        [Header("生成路径")] private string[] _outputFilePathArray;
        [Header("生成路径选择")] private int _selectedOutputFilePath;
        [Header("处理的文件集合")] private Dictionary<string, string> _elementTablePaths; // key:文件名，value:文件路径
        [Header("处理的文件集合滚动列表位置")] private Vector2 _elementTableScrollPos = Vector2.zero;
        [Header("文件列表背景样式")] private GUIStyle _elementTableBgStyle;
        [Header("Excel描述")] private string _excelDescStr;

        [MenuItem("ByTools/🧩 表格转换Json工具")]
        private static void ShowEditor()
        {
            var window = GetWindow<ExcelXmlToJsonConverterEditorWindow>();
            window.minSize           = windowSize;
            window.maxSize           = windowSize;
            window.titleContent.text = "生成Json工具";
        }


        private void OnEnable()
        {
            RestoreSession();

            var dataPath = TableHelper.FindExcelDataClassTemplatePath("Excel表规则");
            if (File.Exists(dataPath))
            {
                _excelDescStr = File.ReadAllText(dataPath);
            }

            _outputFilePathArray = default(SaveJsonPathType).GetInspectorNames();
        }

        private void OnGUI()
        {
            DrawAddSelectedFile(); // 绘制文件添加
            DrawBuildToJson();     // 绘制转换按钮
            DragAndDropFiles();    // 绘制拖拽文件
            DrawElementTable();    // 绘制文件列表
            DrawExcelDesc();       // 绘制Excel描述
        }

        private void DrawExcelDesc()
        {
            var helpBoxStyle = new GUIStyle(EditorStyles.helpBox)
            {
                richText = true
            };
            EditorGUILayout.LabelField($"{_excelDescStr}", helpBoxStyle);
        }

        private void DragAndDropFiles()
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
            GUI.Box(rect, "拖拽需要转换Json的文件到此处（支持跨路径）", label);
            var evt = Event.current;
            if (!rect.Contains(evt.mousePosition)) return;
            if (evt.type is EventType.DragUpdated or EventType.DragPerform)
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                if (evt.type == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();
                    foreach (var path in DragAndDrop.paths)
                    {
                        string ex = Path.GetExtension(path).ToLower();
                        if (ex is not (".xls" or ".xlsx" or ".xlsm" or ".xml"))
                        {
                            EditorUtility.DisplayDialog("提示", "只支持 Excel和XML 文件", "确定");
                            continue;
                        }

                        _elementTablePaths.TryAdd(Path.GetFileName(path), path);
                    }
                }

                evt.Use();
            }
        }


        private void DrawElementTable()
        {
            _elementTablePaths ??= new Dictionary<string, string>(); // key:文件名，value:文件路径
            if (_elementTablePaths.Count > 0)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(20);
                GUILayout.Label("序列", GUILayout.Width(100));
                GUILayout.Label("文件", GUILayout.Width(200));
                GUILayout.Label("     路径", GUILayout.Width(60));
                GUILayout.Label("     操作", GUILayout.Width(60));
                GUILayout.Label("单个转换", GUILayout.Width(60));
                GUILayout.EndHorizontal();
            }

            _elementTableScrollPos = GUILayout.BeginScrollView(_elementTableScrollPos, false, false, GUIStyle.none, GUI.skin.verticalScrollbar);

            var index = 0;
            foreach (var file in _elementTablePaths)
            {
                _elementTableBgStyle ??= new GUIStyle(EditorStyles.helpBox)
                {
                    padding = new RectOffset(6, 6, 6, 6)
                };
                GUILayout.BeginHorizontal(_elementTableBgStyle);
                GUILayout.Space(20);
                GUILayout.Label(index.ToString(), GUILayout.Width(100));
                GUILayout.Label(file.Key, GUILayout.Width(200));

                if (GUILayout.Button("查看", GUILayout.Width(60)))
                {
                    EditorUtility.DisplayDialog("查看XMl文件", $"文件：\n{file.Key}\n\n路径：\n{file.Value}", "确定");
                }

                if (GUILayout.Button("移除", GUILayout.Width(60)))
                {
                    if (_elementTablePaths.ContainsKey(file.Key))
                    {
                        _elementTablePaths.Remove(file.Key);

                        // Tips:此时字典已经改变，进行重新循环
                        GUILayout.EndHorizontal();
                        break;
                    }
                }

                if (GUILayout.Button("转换", GUILayout.Width(60)))
                {
                    if (string.IsNullOrEmpty(file.Value)) continue;
                    if (File.Exists(file.Value))
                    {
                        // 根据文件扩展名,选择不同的转换方式
                        var ex = Path.GetExtension(file.Value).ToLower();
                        switch (ex)
                        {
                            case ".xls" or ".xlsx" or ".xlsm":
                                new ExcelExportToClass().Generate(file.Value);
                                new ExcelExportToAsset().Generate(file.Value, (SaveJsonPathType)_selectedOutputFilePath);
                                break;
                            case ".xml":
                                new XmlExportToAsset().XmlGenerateToJson(file.Value, (SaveJsonPathType)_selectedOutputFilePath, out var json, out var fileName);
                                new XmlExportToClass().GenerateClassesFromJson(json, fileName);
                                break;
                        }
                    }
                }

                GUILayout.EndHorizontal();
                index++;
            }

            GUILayout.EndScrollView();
        }

        private void DrawAddSelectedFile()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("生成路径：", GUILayout.Width(70));
            _selectedOutputFilePath = EditorGUILayout.Popup(_selectedOutputFilePath, _outputFilePathArray);

            if (GUILayout.Button("清除所有文件"))
            {
                _elementTablePaths.Clear();
            }

            if (GUILayout.Button("添加文件"))
            {
                var path   = string.IsNullOrEmpty(_filePath) ? CheckDirectory() : _filePath;
                var folder = Path.GetDirectoryName(path);
                _filePath = EditorUtility.OpenFilePanelWithFilters("选择文件", folder, new[] { "文件格式", "xlsm,xlsx,xls,xml", });
                // Debug.Log(_filePath);
                if (!string.IsNullOrEmpty(_filePath))
                {
                    _elementTablePaths.TryAdd(Path.GetFileName(_filePath), _filePath);
                }
            }

            GUILayout.EndHorizontal();
        }

        private void DrawBuildToJson()
        {
            if (_elementTablePaths.Count == 0) return;
            if (!GUILayout.Button("批量转换")) return;

            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            foreach (var file in _elementTablePaths)
            {
                if (string.IsNullOrEmpty(file.Value)) continue;
                if (File.Exists(file.Value))
                {
                    // 根据文件扩展名,选择不同的转换方式
                    var ex = Path.GetExtension(file.Value).ToLower();
                    switch (ex)
                    {
                        case ".xls" or ".xlsx" or ".xlsm":
                            new ExcelExportToClass().Generate(file.Value);
                            new ExcelExportToAsset().Generate(file.Value, (SaveJsonPathType)_selectedOutputFilePath);
                            break;
                        case ".xml":
                            new XmlExportToAsset().XmlGenerateToJson(file.Value, (SaveJsonPathType)_selectedOutputFilePath, out var json, out var fileName);
                            new XmlExportToClass().GenerateClassesFromJson(json, fileName);
                            break;
                    }
                }
            }

            sw.Stop();
            Debug.Log($"文件转换完成, 耗时：{sw.ElapsedMilliseconds}\n{string.Join("\n", _elementTablePaths.Keys)}");
        }

        #region 辅助

        private static string CheckDirectory()
        {
            var dataPath   = Application.dataPath;
            var folderPath = GetUpperDirectory(dataPath, 2);
            if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);
            return folderPath;
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

        #endregion
    }
}