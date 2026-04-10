namespace _3rdBy.ByTools.TableConvertJson.Editor
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using ConvertHelper;
    using Excel.Generator;
    using LitJson;
    using OfficeOpenXml;
    using UnityEditor;
    using UnityEngine;
    using Xml.Generator;

    /// <summary>
    /// 文件转换器
    /// 将Excel和XML文件转换JSON
    /// 将Json文件转换Excel
    /// 不支持转Xml文件
    /// </summary>
    public class ExcelXmlJsonToolEditorWindow : EditorWindow
    {
        #region 窗口属性

        [Header("窗口大小")] private static readonly Vector2 windowSize = new(800, 600);

        [MenuItem("ByTools/🧩 Json转换工具")]
        private static void ShowEditor()
        {
            var window = GetWindow<ExcelXmlJsonToolEditorWindow>();
            window.minSize           = windowSize;
            window.maxSize           = windowSize;
            window.titleContent.text = "Json工具";
        }

        #endregion

        private enum PanelTab
        {
            [InspectorName("Xml、Excel文件转Json")] ToJson,
            [InspectorName("Json转Excel")] ToExcel
        }

        private PanelTab _currentTab = PanelTab.ToJson;

        #region 列表数据持久化

        [Serializable]
        private class DictWrapper
        {
            [Header("文件名")] public List<string> keys = new();
            [Header("文件路径")] public List<string> values = new();
        }

        private const string SessionKeyExcelXml = "SessionKeyExcelXml";
        private const string SessionKeyJsonExcel = "SessionKeyJsonExcel";

        [Header("文件列表")] private Dictionary<string, string> _fileListDict = new();
        [Header("Json文件保存路径")] private SaveJsonPathType _saveFilePathType;
        [Header("选择文件路径")] private string _selectedFilePath;
        [Header("生成Excel路径")] private string _outputFolder;
        [Header("Excel、Xml滚动列表位置")] private Vector2 _excelXmlScrollPos = Vector2.zero;
        [Header("Json滚动列表位置")] private Vector2 _jsonScrollPos = Vector2.zero;
        [Header("文件列表背景样式")] private GUIStyle _listFileBgStyle;
        [Header("帮助提示背景样式")] private GUIStyle _helpBoxStyle;
        [Header("Excel描述")] private string _excelDescStr;
        [Header("是否显示帮助")] private bool _showHelp;

        /// <summary>
        /// 更新持久化数据
        /// </summary>
        private void UpdateSessionData()
        {
            string sessionKey = _currentTab switch
            {
                PanelTab.ToJson  => SessionKeyExcelXml,
                PanelTab.ToExcel => SessionKeyJsonExcel,
                _                => throw new ArgumentOutOfRangeException()
            };

            // 列表无数据，则删除Session数据
            if (_fileListDict.Count == 0)
            {
                SessionState.EraseString(sessionKey);
                return;
            }

            // 列表有数据，则更新Session数据
            var wrapper = new DictWrapper();
            foreach (var kv in _fileListDict)
            {
                wrapper.keys.Add(kv.Key);
                wrapper.values.Add(kv.Value);
            }

            var json = JsonUtility.ToJson(wrapper);
            SessionState.SetString(sessionKey, json);
        }

        /// <summary>
        /// 加载持久化数据
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private void LoadSessionData()
        {
            string fileList = _currentTab switch
            {
                PanelTab.ToJson  => SessionState.GetString(SessionKeyExcelXml, ""),
                PanelTab.ToExcel => SessionState.GetString(SessionKeyJsonExcel, ""),
                _                => throw new ArgumentOutOfRangeException()
            };

            if (string.IsNullOrEmpty(fileList))
            {
                _fileListDict = new Dictionary<string, string>();
                return;
            }

            _fileListDict.Clear();
            var wrapper = JsonUtility.FromJson<DictWrapper>(fileList);
            if (wrapper != null)
            {
                for (int i = 0; i < wrapper.keys.Count; i++)
                {
                    _fileListDict.TryAdd(wrapper.keys[i], wrapper.values[i]);
                }
            }
        }

        #endregion

        /// <summary>
        /// 绘制选项卡
        /// </summary>
        private void OnTabOptionDraw()
        {
            // 使用水平布局放置两个按钮
            EditorGUILayout.BeginHorizontal();

            // 保存原始 GUI 颜色，以便重置
            var originalColor = GUI.backgroundColor;

            // 按钮背景色设为绿色
            if (_currentTab == PanelTab.ToJson)
                GUI.backgroundColor = Color.green;
            if (GUILayout.Button("Xml、Excel转Json", GUILayout.Height(30)))
            {
                if (_currentTab != PanelTab.ToJson)
                {
                    UpdateSessionData();
                    _currentTab = PanelTab.ToJson;
                    LoadSessionData();
                    _excelDescStr = string.Empty;
                }
            }

            // 重置颜色
            GUI.backgroundColor = originalColor;

            // 按钮背景色设为绿色
            if (_currentTab == PanelTab.ToExcel)
                GUI.backgroundColor = Color.green;
            if (GUILayout.Button("Json转Excel", GUILayout.Height(30)))
            {
                if (_currentTab != PanelTab.ToExcel)
                {
                    UpdateSessionData();
                    _currentTab = PanelTab.ToExcel;
                    LoadSessionData();
                    _excelDescStr = string.Empty;
                }
            }

            GUI.backgroundColor = originalColor;

            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// 绘制拖拽区域
        /// </summary>
        private void OnDragDropAreaDraw()
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
            GUI.Box(rect, "拖拽需要转换的文件到此处（支持跨路径）", label);
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
                        switch (_currentTab)
                        {
                            case PanelTab.ToJson:
                                if (ex is not (".xls" or ".xlsx" or ".xlsm" or ".xml"))
                                {
                                    EditorUtility.DisplayDialog("提示", "只支持 Excel和XML 文件", "确定");
                                    continue;
                                }

                                break;
                            case PanelTab.ToExcel:
                                if (ex is not ".json")
                                {
                                    EditorUtility.DisplayDialog("提示", "只支持 json 文件", "确定");
                                    continue;
                                }

                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }

                        _fileListDict.TryAdd(Path.GetFileName(path), path);
                    }
                }

                evt.Use();
            }
        }


        /// <summary>
        /// 绘制Excel描述
        /// </summary>
        private void DescHelpDraw()
        {
            _helpBoxStyle ??= new GUIStyle(EditorStyles.helpBox)
            {
                richText = true
            };

            if (string.IsNullOrEmpty(_excelDescStr))
            {
                var descFileName = _currentTab switch
                {
                    PanelTab.ToJson  => "Excel表规则",
                    PanelTab.ToExcel => "Json表规则",
                    _                => throw new ArgumentOutOfRangeException()
                };
                var dataPath = TableHelper.FindExcelDataClassTemplatePath(descFileName);
                if (File.Exists(dataPath))
                {
                    _excelDescStr = File.ReadAllText(dataPath);
                }
                else
                {
                    Debug.LogError("Excel描述文件不存在");
                }
            }

            EditorGUILayout.LabelField($"{_excelDescStr}", _helpBoxStyle);

            // Excel、Xml 示例路径
            if (_currentTab == PanelTab.ToJson)
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("查看Excel示例路径"))
                {
                    var path = TableHelper.FindExcelDataClassTemplatePath("ExampleTable", ".xlsx");
                    var file = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
                    Selection.activeObject = file; // 选中文件
                }

                if (GUILayout.Button("打开Excel示例文件"))
                {
                    var path = TableHelper.FindExcelDataClassTemplatePath("ExampleTable", ".xlsx");
                    EditorUtility.OpenWithDefaultApp(path); // 直接打开文件
                }

                GUILayout.EndHorizontal();
            }
        }

        /// <summary>
        /// 工具栏目录
        /// </summary>
        private void OnShortcutKeysDraw()
        {
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("输出路径：", EditorStyles.boldLabel, GUILayout.Width(70));
            switch (_currentTab)
            {
                case PanelTab.ToJson:
                    _saveFilePathType = (SaveJsonPathType)EditorGUILayout.EnumPopup(_saveFilePathType);
                    break;
                case PanelTab.ToExcel:
                    _outputFolder = string.IsNullOrEmpty(_outputFolder) ? CheckDirectoryHelper() : _outputFolder;
                    EditorGUILayout.TextField(_outputFolder);
                    if (GUILayout.Button("设置路径", GUILayout.Width(70)))
                    {
                        var folder = EditorUtility.OpenFolderPanel("输出文件夹", _outputFolder, "");
                        _outputFolder = folder;
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (GUILayout.Button("添加文件📍"))
            {
                var path   = string.IsNullOrEmpty(_selectedFilePath) ? CheckDirectoryHelper() : _selectedFilePath;
                var folder = Path.GetDirectoryName(path);

                string[] filters = _currentTab switch
                {
                    PanelTab.ToJson  => new[] { "文件格式", "xlsm,xlsx,xls,xml" },
                    PanelTab.ToExcel => new[] { "文件格式", "json" },
                    _                => throw new ArgumentOutOfRangeException()
                };
                _selectedFilePath = EditorUtility.OpenFilePanelWithFilters("选择文件", folder, filters);

                // Debug.Log(_filePath);
                if (!string.IsNullOrEmpty(_selectedFilePath))
                {
                    _fileListDict.TryAdd(Path.GetFileName(_selectedFilePath), _selectedFilePath);
                }
            }

            if (_fileListDict.Count > 1)
            {
                if (GUILayout.Button("清除列表🚮"))
                {
                    _fileListDict.Clear();
                    UpdateSessionData();
                }

                if (GUILayout.Button("一键转换📄"))
                {
                    var sw = new System.Diagnostics.Stopwatch();
                    sw.Start();
                    foreach (var file in _fileListDict)
                    {
                        if (string.IsNullOrEmpty(file.Value)) continue;

                        // TODO: 这里需要判断处理的是否是Excel文件或Json文件
                        switch (_currentTab)
                        {
                            case PanelTab.ToJson:
                                // 根据文件扩展名,选择不同的转换方式
                                var ex = Path.GetExtension(file.Value).ToLower();
                                switch (ex)
                                {
                                    case ".xls" or ".xlsx" or ".xlsm":
                                        new ExcelExportToClass().Generate(file.Value);
                                        new ExcelExportToAsset().Generate(file.Value, _saveFilePathType);
                                        break;
                                    case ".xml":
                                        new XmlExportToAsset().XmlGenerateToJson(file.Value, _saveFilePathType, out var json, out var fileName);
                                        new XmlExportToClass().GenerateClassesFromJson(json, fileName);
                                        break;
                                }

                                break;
                            case PanelTab.ToExcel:
                                if (File.Exists(file.Value))
                                {
                                    // 通过路径获取文件名，不含扩展
                                    var fileName = Path.GetFileNameWithoutExtension(file.Value);
                                    ConvertJsonToExcel(file.Value, _outputFolder, fileName);
                                }

                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }

                    sw.Stop();
                    Debug.Log($"文件转换完成, 耗时：{sw.ElapsedMilliseconds}\n{string.Join("\n", _fileListDict.Keys)}");
                }
            }

            // 绘制帮助按钮
            if (GUILayout.Button("帮助❔"))
            {
                _showHelp = !_showHelp;
            }

            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// 绘制列表内容
        /// </summary>
        private void OnFileListDraw()
        {
            _fileListDict ??= new Dictionary<string, string>(); // key:文件名，value:文件路径
            if (_fileListDict.Count > 0)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(20);
                GUILayout.Label("序列", GUILayout.Width(50));
                GUILayout.Space(20);
                GUILayout.Label("文件", GUILayout.Width(450));

                GUILayout.Label("路径", GUILayout.Width(60));
                GUILayout.Label("转换", GUILayout.Width(60));
                GUILayout.Label("移除", GUILayout.Width(60));
                GUILayout.EndHorizontal();
            }

            // TODO:不同变量
            switch (_currentTab)
            {
                case PanelTab.ToJson:
                    _excelXmlScrollPos = GUILayout.BeginScrollView(_excelXmlScrollPos, false, false, GUIStyle.none, GUI.skin.verticalScrollbar);
                    break;
                case PanelTab.ToExcel:
                    _jsonScrollPos = GUILayout.BeginScrollView(_jsonScrollPos, false, false, GUIStyle.none, GUI.skin.verticalScrollbar);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            var index = 0;
            foreach (var file in _fileListDict)
            {
                _listFileBgStyle ??= new GUIStyle(EditorStyles.helpBox)
                {
                    padding = new RectOffset(6, 6, 6, 6)
                };
                GUILayout.BeginHorizontal(_listFileBgStyle);
                GUILayout.Space(20);
                GUILayout.Label(index.ToString(), GUILayout.Width(50));
                GUILayout.Label(file.Key, GUILayout.Width(450));

                if (GUILayout.Button("打开📂", GUILayout.Width(60)))
                {
                    EditorUtility.OpenWithDefaultApp(file.Value);
                }

                if (GUILayout.Button("转换🥏", GUILayout.Width(60)))
                {
                    if (string.IsNullOrEmpty(file.Value)) continue;
                    if (File.Exists(file.Value))
                    {
                        // TODO: 这里需要判断处理的是否是Excel文件或Json文件
                        switch (_currentTab)
                        {
                            case PanelTab.ToJson:
                                // 根据文件扩展名,选择不同的转换方式
                                var ex = Path.GetExtension(file.Value).ToLower();
                                switch (ex)
                                {
                                    case ".xls" or ".xlsx" or ".xlsm":
                                        new ExcelExportToClass().Generate(file.Value);
                                        new ExcelExportToAsset().Generate(file.Value, _saveFilePathType);
                                        break;
                                    case ".xml":
                                        new XmlExportToAsset().XmlGenerateToJson(file.Value, _saveFilePathType, out var json, out var fileName);
                                        new XmlExportToClass().GenerateClassesFromJson(json, fileName);
                                        break;
                                }

                                break;
                            case PanelTab.ToExcel:
                                // 通过路径获取文件名，不含扩展
                                var jsonFileName = Path.GetFileNameWithoutExtension(file.Value);
                                ConvertJsonToExcel(file.Value, _outputFolder, jsonFileName);
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }
                }

                if (GUILayout.Button("移除❌", GUILayout.Width(60)))
                {
                    if (_fileListDict.ContainsKey(file.Key))
                    {
                        _fileListDict.Remove(file.Key);

                        // Tips:此时字典已经改变，进行重新循环
                        GUILayout.EndHorizontal();
                        break;
                    }
                }

                GUILayout.EndHorizontal();
                index++;
            }

            GUILayout.EndScrollView();
        }


        private void OnEnable()
        {
            LoadSessionData();
        }

        private void OnDisable()
        {
            UpdateSessionData();
        }

        private void OnGUI()
        {
            OnTabOptionDraw();
            OnShortcutKeysDraw();
            if (_showHelp)
            {
                DescHelpDraw();
            }
            else
            {
                OnDragDropAreaDraw();
                OnFileListDraw();
            }
        }


        #region Json转Excel

        /// <summary>
        /// Json转换Excel内容构造器
        /// </summary>
        /// <param name="jsonPath"></param>
        private void ConvertJsonToExcel(string jsonPath, string savePath, string saveName)
        {
            if (string.IsNullOrEmpty(jsonPath) || string.IsNullOrEmpty(_outputFolder))
            {
                Debug.LogError("路径为空");
                return;
            }

            // 构造Excel保存路径
            var fullSavePath = $"{savePath}/{saveName}.xlsx";

            string json = File.ReadAllText(jsonPath);
            var    root = JsonMapper.ToObject(json);
            if (!HasKeyConvertExcel(root, "dataList"))
            {
                Debug.LogError("JSON缺少 dataList");
                return;
            }

            var dataList = root["dataList"];
            if (dataList.Count == 0)
            {
                Debug.LogError("dataList为空");
                return;
            }

            // ===== 获取字段（用第一行）
            var first = dataList[0];
            var keys  = GetKeysConvertExcel(first);

            // ===== 类型推断
            var types = new List<string>();
            foreach (var key in keys)
            {
                types.Add(DetectTypeConvertExcel(first[key]));
            }

            using (var package = new ExcelPackage()) // ⚠️ 不使用 LicenseContext
            {
                var sheet = package.Workbook.Worksheets.Add("Sheet1");

                const int rowStart    = 4; // 起始行
                const int columnStart = 2; // 起始列
                // ===== 第4行：字段名
                for (int i = 0; i < keys.Count; i++)
                {
                    sheet.Cells[rowStart, i + columnStart].Value = keys[i];
                }

                // ===== 第5行：注释（空）
                for (int i = 0; i < keys.Count; i++)
                {
                    sheet.Cells[rowStart + 1, i + columnStart].Value = "#未注释";
                }

                // ===== 第6行：类型
                for (int i = 0; i < types.Count; i++)
                {
                    sheet.Cells[rowStart + 2, i + columnStart].Value = types[i];
                }

                // ===== 第7行往下：数据
                for (int i = 0; i < dataList.Count; i++)
                {
                    var row = dataList[i];

                    for (int j = 0; j < keys.Count; j++)
                    {
                        string key = keys[j];

                        if (!HasKeyConvertExcel(row, key))
                        {
                            sheet.Cells[rowStart + 3 + i, j + columnStart].Value = "";
                            continue;
                        }

                        var val = row[key];
                        sheet.Cells[rowStart + 3 + i, j + columnStart].Value = ConvertToExcelString(val);
                    }
                }

                File.WriteAllBytes(fullSavePath, package.GetAsByteArray());
            }

            Debug.Log($"Json转Excel完成: {fullSavePath}");
            AssetDatabase.Refresh();
        }

        #endregion

        #region 辅助

        // 检查目录
        private static string CheckDirectoryHelper()
        {
            var dataPath   = Application.dataPath;
            var folderPath = GetUpperDirectoryHelper(dataPath, 2);
            if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);
            return folderPath;
        }

        // 获取上级目录
        private static string GetUpperDirectoryHelper(string path, int level, string lastFold = "")
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

        /// <summary>
        /// 判断JsonData是否有指定键
        /// </summary>
        /// <param name="data"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        private static bool HasKeyConvertExcel(JsonData data, string key)
        {
            return data is { IsObject: true } && ((IDictionary)data).Contains(key);
        }

        /// <summary>
        /// 获取JsonData的键值列表
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private static List<string> GetKeysConvertExcel(JsonData data)
        {
            var list = new List<string>();
            var dict = (IDictionary)data;

            foreach (var k in dict.Keys)
            {
                list.Add(k.ToString());
            }

            return list;
        }

        /// <summary>
        /// 推断JsonData的类型
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private static string DetectTypeConvertExcel(JsonData data)
        {
            if (data == null) return "string";

            // 单值类型
            if (data.IsInt || data.IsLong) return "int";
            if (data.IsDouble) return "float";
            if (data.IsBoolean) return "bool";
            if (data.IsString) return "string";

            // 数组类型
            if (data.IsArray)
            {
                // 获取数组最大深度
                int depth = GetArrayDepthConvertExcel(data);

                // 推断最内层类型（取第一个元素递归）
                string baseType = InferBaseTypeConvertExcel(data);

                // 构造多维类型
                for (int i = 0; i < depth; i++)
                {
                    baseType += "[]";
                }

                return baseType;
            }

            return "string";
        }

        /// <summary>
        /// 推断数组最内层类型
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private static string InferBaseTypeConvertExcel(JsonData data)
        {
            while (true)
            {
                if (!data.IsArray)
                {
                    if (data.IsInt || data.IsLong) return "int";
                    if (data.IsDouble) return "float";
                    return data.IsBoolean ? "bool" : "string";
                }

                // 空数组默认 int
                if (data.Count == 0) return "int";
                // 取第一个元素递归判断
                data = data[0];
            }
        }

        /// <summary>
        /// 转Excel字符串（核心）
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private static string ConvertToExcelString(JsonData data)
        {
            if (data == null) return "";

            if (data.IsArray)
            {
                int depth = GetArrayDepthConvertExcel(data);
                return ConvertArrayConvertExcel(data, depth);
            }

            return data.IsBoolean ? data.ToString().ToLower() : data.ToString();
        }

        /// <summary>
        /// 递归获取数组深度
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private static int GetArrayDepthConvertExcel(JsonData data)
        {
            int depth   = 0;
            var current = data;

            while (current is { IsArray: true, Count: > 0 })
            {
                depth++;
                current = current[0];
            }

            return depth;
        }

        /// <summary>
        /// bool类型内容小写
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private static string ConvertBooleanConvertExcel(JsonData data)
        {
            return data.IsBoolean ? data.ToString().ToLower() : data.ToString();
        }

        /// <summary>
        /// 多维数组还原数组类型转字符串
        /// </summary>
        /// <param name="data"></param>
        /// <param name="depth"></param>
        /// <returns></returns>
        private static string ConvertArrayConvertExcel(JsonData data, int depth)
        {
            switch (depth)
            {
                case 1: // 一维
                {
                    var list = new List<string>();
                    foreach (JsonData item in data)
                        list.Add(ConvertBooleanConvertExcel(item));

                    return string.Join(",", list);
                }
                case 2: // 二维
                {
                    var list = new List<string>();
                    foreach (JsonData item in data)
                        list.Add(ConvertArrayConvertExcel(item, 1)); // 内层是1维
                    return string.Join("|", list);
                }
                case 3: // 三维
                {
                    var list = new List<string>();
                    foreach (JsonData item in data)
                        list.Add(ConvertArrayConvertExcel(item, 2)); // 内层是2维

                    return string.Join(";", list);
                }
            }

            // 超过3维（兜底）
            var fallback = new List<string>();
            foreach (JsonData item in data)
                fallback.Add(item.ToString());

            return string.Join(",", fallback);
        }

        #endregion
    }
}