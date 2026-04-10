using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using LitJson;
using OfficeOpenXml;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Json 转 Excel 批量工具窗口
/// 功能：
/// 1. 支持拖拽多个 .json / .txt 文件
/// 2. 支持拖拽文件夹，自动扫描内部所有 json/txt
/// 3. 支持批量导出 Excel
/// 4. 支持预览将生成哪些 Sheet
/// 5. 支持任意结构 Json：
///    - 对象拍平成列
///    - 对象数组拆为子表
///    - 普通数组/混合数组/多维数组保存为 Json 字符串
/// </summary>
public class JsonToExcelBatchWindow : EditorWindow
{
    /// <summary>
    /// 数组处理模式
    /// </summary>
    private enum ArrayHandlingMode
    {
        /// <summary>
        /// 自动拆对象数组为子表，其他数组保存为 Json 字符串
        /// </summary>
        SplitObjectArrayToChildSheet = 0,

        /// <summary>
        /// 所有数组都保存为 Json 字符串
        /// </summary>
        StoreAllArraysAsJsonString = 1
    }

    /// <summary>
    /// 预览信息
    /// </summary>
    [Serializable]
    private class SheetPreviewInfo
    {
        public string SheetName;
        public int RowCount;
        public int ColumnCount;
    }

    /// <summary>
    /// 已添加的 Json 文件绝对路径列表
    /// </summary>
    private readonly List<string> _jsonFilePaths = new List<string>();

    /// <summary>
    /// Excel 输出目录
    /// </summary>
    private string _outputFolder = string.Empty;

    /// <summary>
    /// 批量导出时附加的后缀，可为空
    /// 例如：_cfg
    /// </summary>
    private string _fileNameSuffix = string.Empty;

    /// <summary>
    /// 数组处理模式
    /// </summary>
    private ArrayHandlingMode _arrayHandlingMode = ArrayHandlingMode.SplitObjectArrayToChildSheet;

    /// <summary>
    /// 当前预览选中的文件索引
    /// </summary>
    private int _selectedPreviewIndex = -1;

    /// <summary>
    /// 主滚动
    /// </summary>
    private Vector2 _mainScroll;

    /// <summary>
    /// 文件列表滚动
    /// </summary>
    private Vector2 _fileListScroll;

    /// <summary>
    /// 预览滚动
    /// </summary>
    private Vector2 _previewScroll;

    /// <summary>
    /// 预览缓存
    /// </summary>
    private readonly List<SheetPreviewInfo> _previewInfos = new List<SheetPreviewInfo>();

    /// <summary>
    /// 预览错误信息
    /// </summary>
    private string _previewError = string.Empty;

    /// <summary>
    /// 上次预览的文件路径
    /// </summary>
    private string _lastPreviewFilePath = string.Empty;

    /// <summary>
    /// 上次预览的数组模式
    /// </summary>
    private ArrayHandlingMode _lastPreviewMode = ArrayHandlingMode.SplitObjectArrayToChildSheet;

    [MenuItem("Tools/Json To Excel Batch")]
    public static void OpenWindow()
    {
        JsonToExcelBatchWindow window = GetWindow<JsonToExcelBatchWindow>("Json To Excel");
        window.minSize = new Vector2(820, 560);
        window.Show();
    }

    private void OnEnable()
    {
        if (string.IsNullOrEmpty(_outputFolder))
        {
            _outputFolder = Application.dataPath;
        }
    }

    private void OnGUI()
    {
        _mainScroll = EditorGUILayout.BeginScrollView(_mainScroll);

        GUILayout.Space(8);
        EditorGUILayout.LabelField("Json 转 Excel 批量工具", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "支持：多文件拖拽、文件夹拖拽、批量导出、Sheet 预览、任意结构 Json。\n" +
            "推荐：将脚本放到 Assets/Editor 下，并确保项目已接入 LitJson 与 EPPlus。",
            MessageType.Info);

        GUILayout.Space(6);

        DrawInputToolbar();
        GUILayout.Space(8);

        DrawDropArea();
        GUILayout.Space(10);

        DrawFileListSection();
        GUILayout.Space(10);

        DrawOptionsSection();
        GUILayout.Space(10);

        DrawPreviewSection();
        GUILayout.Space(10);

        DrawExportSection();

        EditorGUILayout.EndScrollView();
    }

    /// <summary>
    /// 绘制输入工具栏
    /// </summary>
    private void DrawInputToolbar()
    {
        EditorGUILayout.LabelField("1. 添加 Json 文件", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("添加文件", GUILayout.Height(28)))
        {
            AddSingleJsonFileByPanel();
        }

        if (GUILayout.Button("添加文件夹", GUILayout.Height(28)))
        {
            AddFolderByPanel();
        }

        if (GUILayout.Button("使用当前选中资源", GUILayout.Height(28)))
        {
            AddCurrentSelectedAsset();
        }

        if (GUILayout.Button("全部清空", GUILayout.Height(28), GUILayout.Width(100)))
        {
            ClearAllFiles();
        }

        EditorGUILayout.EndHorizontal();
    }

    /// <summary>
    /// 绘制拖拽区域
    /// </summary>
    private void DrawDropArea()
    {
        Rect dropRect = GUILayoutUtility.GetRect(0, 90, GUILayout.ExpandWidth(true));
        GUI.Box(dropRect, "将 .json / .txt 文件或文件夹拖到这里", EditorStyles.helpBox);
        HandleDragAndDrop(dropRect);
    }

    /// <summary>
    /// 绘制文件列表区域
    /// </summary>
    private void DrawFileListSection()
    {
        EditorGUILayout.LabelField("2. 当前待导出文件", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("文件数量", _jsonFilePaths.Count.ToString());

        Rect listRect = GUILayoutUtility.GetRect(0, 220, GUILayout.ExpandWidth(true));
        GUI.Box(listRect, GUIContent.none);

        Rect innerRect = new Rect(listRect.x + 4, listRect.y + 4, listRect.width - 8, listRect.height - 8);

        GUILayout.BeginArea(innerRect);
        _fileListScroll = EditorGUILayout.BeginScrollView(_fileListScroll);

        if (_jsonFilePaths.Count == 0)
        {
            EditorGUILayout.HelpBox("当前没有文件。你可以点击按钮添加，或者直接拖拽文件/文件夹进来。", MessageType.Warning);
        }
        else
        {
            for (int i = 0; i < _jsonFilePaths.Count; i++)
            {
                DrawSingleFileRow(i, _jsonFilePaths[i]);
            }
        }

        EditorGUILayout.EndScrollView();
        GUILayout.EndArea();
    }

    /// <summary>
    /// 绘制单个文件行
    /// </summary>
    private void DrawSingleFileRow(int index, string path)
    {
        bool     isSelected = index == _selectedPreviewIndex;
        GUIStyle rowStyle   = new GUIStyle(EditorStyles.helpBox);
        if (isSelected)
        {
            rowStyle.normal.background = Texture2D.grayTexture;
        }

        EditorGUILayout.BeginVertical(rowStyle);

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Toggle(isSelected, "", GUILayout.Width(18)))
        {
            if (_selectedPreviewIndex != index)
            {
                _selectedPreviewIndex = index;
                InvalidatePreviewCache();
            }
        }

        EditorGUILayout.LabelField((index + 1).ToString(), GUILayout.Width(28));
        EditorGUILayout.LabelField(Path.GetFileName(path), EditorStyles.boldLabel);

        if (GUILayout.Button("预览", GUILayout.Width(60)))
        {
            _selectedPreviewIndex = index;
            RefreshPreview();
        }

        if (GUILayout.Button("移除", GUILayout.Width(60)))
        {
            RemoveFileAt(index);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            return;
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.SelectableLabel(path, EditorStyles.textField, GUILayout.Height(18));

        EditorGUILayout.EndVertical();
    }

    /// <summary>
    /// 绘制选项区域
    /// </summary>
    private void DrawOptionsSection()
    {
        EditorGUILayout.LabelField("3. 导出设置", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.TextField("保存路径", _outputFolder);

        if (GUILayout.Button("选择路径", GUILayout.Width(90)))
        {
            SelectOutputFolder();
        }

        if (GUILayout.Button("打开目录", GUILayout.Width(90)))
        {
            OpenOutputFolder();
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        _fileNameSuffix = EditorGUILayout.TextField("文件名后缀", _fileNameSuffix);
        EditorGUILayout.EndHorizontal();

        EditorGUI.BeginChangeCheck();
        _arrayHandlingMode = (ArrayHandlingMode)EditorGUILayout.EnumPopup("数组处理模式", _arrayHandlingMode);
        if (EditorGUI.EndChangeCheck())
        {
            InvalidatePreviewCache();
        }

        EditorGUILayout.HelpBox(
            "数组处理模式说明：\n" +
            "1. SplitObjectArrayToChildSheet：对象数组拆子表，普通数组存 Json 字符串\n" +
            "2. StoreAllArraysAsJsonString：所有数组都直接存 Json 字符串",
            MessageType.None);
    }

    /// <summary>
    /// 绘制预览区域
    /// </summary>
    private void DrawPreviewSection()
    {
        EditorGUILayout.LabelField("4. 结构预览", EditorStyles.boldLabel);

        if (_selectedPreviewIndex < 0 || _selectedPreviewIndex >= _jsonFilePaths.Count)
        {
            EditorGUILayout.HelpBox("请选择一个文件进行预览。", MessageType.Info);
            return;
        }

        string previewPath = _jsonFilePaths[_selectedPreviewIndex];
        EditorGUILayout.LabelField("当前预览", Path.GetFileName(previewPath));

        if (NeedRefreshPreview(previewPath))
        {
            RefreshPreview();
        }

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("刷新预览", GUILayout.Width(100)))
        {
            RefreshPreview();
        }

        EditorGUILayout.LabelField("路径", previewPath);

        EditorGUILayout.EndHorizontal();

        Rect previewRect = GUILayoutUtility.GetRect(0, 180, GUILayout.ExpandWidth(true));
        GUI.Box(previewRect, GUIContent.none);

        Rect innerRect = new Rect(previewRect.x + 4, previewRect.y + 4, previewRect.width - 8, previewRect.height - 8);

        GUILayout.BeginArea(innerRect);
        _previewScroll = EditorGUILayout.BeginScrollView(_previewScroll);

        if (!string.IsNullOrEmpty(_previewError))
        {
            EditorGUILayout.HelpBox(_previewError, MessageType.Error);
        }
        else if (_previewInfos.Count == 0)
        {
            EditorGUILayout.HelpBox("没有可显示的 Sheet 预览。", MessageType.Info);
        }
        else
        {
            for (int i = 0; i < _previewInfos.Count; i++)
            {
                SheetPreviewInfo info = _previewInfos[i];
                EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                EditorGUILayout.LabelField("Sheet", info.SheetName, GUILayout.Width(280));
                EditorGUILayout.LabelField("Rows", info.RowCount.ToString(), GUILayout.Width(90));
                EditorGUILayout.LabelField("Columns", info.ColumnCount.ToString(), GUILayout.Width(110));
                EditorGUILayout.EndHorizontal();
            }
        }

        EditorGUILayout.EndScrollView();
        GUILayout.EndArea();
    }

    /// <summary>
    /// 绘制导出区域
    /// </summary>
    private void DrawExportSection()
    {
        EditorGUILayout.LabelField("5. 导出", EditorStyles.boldLabel);

        bool hasFiles  = _jsonFilePaths.Count > 0;
        bool hasOutput = !string.IsNullOrEmpty(_outputFolder);

        using (new EditorGUI.DisabledScope(!hasFiles || !hasOutput))
        {
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("导出当前预览文件", GUILayout.Height(34)))
            {
                ExportSelectedFile();
            }

            if (GUILayout.Button("批量导出全部文件", GUILayout.Height(34)))
            {
                ExportAllFiles();
            }

            EditorGUILayout.EndHorizontal();
        }

        if (!hasFiles || !hasOutput)
        {
            EditorGUILayout.HelpBox("请先添加文件，并设置保存路径。", MessageType.Warning);
        }
    }

    /// <summary>
    /// 通过面板添加单个 Json 文件
    /// </summary>
    private void AddSingleJsonFileByPanel()
    {
        string path = EditorUtility.OpenFilePanel("选择 Json 文件", Application.dataPath, "json");
        if (string.IsNullOrEmpty(path))
            return;

        AddPathSmart(path, true);
    }

    /// <summary>
    /// 通过面板添加文件夹
    /// </summary>
    private void AddFolderByPanel()
    {
        string folder = EditorUtility.OpenFolderPanel("选择 Json 文件夹", Application.dataPath, string.Empty);
        if (string.IsNullOrEmpty(folder))
            return;

        AddPathSmart(folder, true);
    }

    /// <summary>
    /// 添加当前选中资源
    /// </summary>
    private void AddCurrentSelectedAsset()
    {
        UnityEngine.Object obj = Selection.activeObject;
        if (obj == null)
        {
            EditorUtility.DisplayDialog("提示", "当前没有选中文件或文件夹。", "确定");
            return;
        }

        string assetPath = AssetDatabase.GetAssetPath(obj);
        if (string.IsNullOrEmpty(assetPath))
        {
            EditorUtility.DisplayDialog("提示", "当前选中对象不是有效资源。", "确定");
            return;
        }

        string absolutePath = ToAbsolutePath(assetPath);
        AddPathSmart(absolutePath, true);
    }

    /// <summary>
    /// 清空所有文件
    /// </summary>
    private void ClearAllFiles()
    {
        _jsonFilePaths.Clear();
        _selectedPreviewIndex = -1;
        InvalidatePreviewCache();
    }

    /// <summary>
    /// 移除指定索引的文件
    /// </summary>
    private void RemoveFileAt(int index)
    {
        if (index < 0 || index >= _jsonFilePaths.Count)
            return;

        _jsonFilePaths.RemoveAt(index);

        if (_jsonFilePaths.Count == 0)
        {
            _selectedPreviewIndex = -1;
        }
        else if (_selectedPreviewIndex >= _jsonFilePaths.Count)
        {
            _selectedPreviewIndex = _jsonFilePaths.Count - 1;
        }

        InvalidatePreviewCache();
        Repaint();
    }

    /// <summary>
    /// 智能添加路径：文件或文件夹
    /// </summary>
    private void AddPathSmart(string rawPath, bool showDialogIfEmpty)
    {
        string path = NormalizePath(rawPath);

        if (File.Exists(path))
        {
            if (IsSupportedJsonFile(path))
            {
                AddSingleFile(path);
                return;
            }

            if (showDialogIfEmpty)
            {
                EditorUtility.DisplayDialog("提示", "请选择 .json 或 .txt 文件，或者包含这些文件的文件夹。", "确定");
            }

            return;
        }

        if (Directory.Exists(path))
        {
            int addCount = AddAllJsonFilesFromFolder(path);
            if (addCount <= 0 && showDialogIfEmpty)
            {
                EditorUtility.DisplayDialog("提示", "该文件夹内没有找到 .json 或 .txt 文件。", "确定");
            }

            return;
        }

        if (showDialogIfEmpty)
        {
            EditorUtility.DisplayDialog("提示", "路径无效。", "确定");
        }
    }

    /// <summary>
    /// 添加单个文件
    /// </summary>
    private void AddSingleFile(string absolutePath)
    {
        string path = NormalizePath(absolutePath);

        if (!File.Exists(path) || !IsSupportedJsonFile(path))
            return;

        if (_jsonFilePaths.Contains(path))
            return;

        _jsonFilePaths.Add(path);

        if (_selectedPreviewIndex < 0)
        {
            _selectedPreviewIndex = 0;
        }

        InvalidatePreviewCache();
        Repaint();
    }

    /// <summary>
    /// 从文件夹递归添加所有 Json/Txt 文件
    /// </summary>
    private int AddAllJsonFilesFromFolder(string folderPath)
    {
        int before = _jsonFilePaths.Count;

        string[] files = Directory.GetFiles(folderPath, "*.*", SearchOption.AllDirectories);
        for (int i = 0; i < files.Length; i++)
        {
            string path = NormalizePath(files[i]);
            if (IsSupportedJsonFile(path) && !_jsonFilePaths.Contains(path))
            {
                _jsonFilePaths.Add(path);
            }
        }

        if (_selectedPreviewIndex < 0 && _jsonFilePaths.Count > 0)
        {
            _selectedPreviewIndex = 0;
        }

        InvalidatePreviewCache();
        Repaint();

        return _jsonFilePaths.Count - before;
    }

    /// <summary>
    /// 处理拖拽
    /// </summary>
    private void HandleDragAndDrop(Rect dropRect)
    {
        Event evt = Event.current;
        if (!dropRect.Contains(evt.mousePosition))
            return;

        if (evt.type != EventType.DragUpdated && evt.type != EventType.DragPerform)
            return;

        bool hasValid = false;

        if (DragAndDrop.paths != null)
        {
            for (int i = 0; i < DragAndDrop.paths.Length; i++)
            {
                string abs = ToAbsolutePath(DragAndDrop.paths[i]);
                if (File.Exists(abs) && IsSupportedJsonFile(abs))
                {
                    hasValid = true;
                    break;
                }

                if (Directory.Exists(abs))
                {
                    hasValid = true;
                    break;
                }
            }
        }

        if (!hasValid && DragAndDrop.objectReferences != null)
        {
            for (int i = 0; i < DragAndDrop.objectReferences.Length; i++)
            {
                UnityEngine.Object obj       = DragAndDrop.objectReferences[i];
                string             assetPath = AssetDatabase.GetAssetPath(obj);
                if (string.IsNullOrEmpty(assetPath))
                    continue;

                string abs = ToAbsolutePath(assetPath);
                if ((File.Exists(abs) && IsSupportedJsonFile(abs)) || Directory.Exists(abs))
                {
                    hasValid = true;
                    break;
                }
            }
        }

        DragAndDrop.visualMode = hasValid ? DragAndDropVisualMode.Copy : DragAndDropVisualMode.Rejected;

        if (evt.type == EventType.DragPerform && hasValid)
        {
            DragAndDrop.AcceptDrag();

            if (DragAndDrop.paths != null)
            {
                for (int i = 0; i < DragAndDrop.paths.Length; i++)
                {
                    AddPathSmart(ToAbsolutePath(DragAndDrop.paths[i]), false);
                }
            }

            if (DragAndDrop.objectReferences != null)
            {
                for (int i = 0; i < DragAndDrop.objectReferences.Length; i++)
                {
                    UnityEngine.Object obj       = DragAndDrop.objectReferences[i];
                    string             assetPath = AssetDatabase.GetAssetPath(obj);
                    if (string.IsNullOrEmpty(assetPath))
                        continue;

                    AddPathSmart(ToAbsolutePath(assetPath), false);
                }
            }

            GUI.FocusControl(null);
        }

        evt.Use();
    }

    /// <summary>
    /// 选择输出目录
    /// </summary>
    private void SelectOutputFolder()
    {
        string folder = EditorUtility.OpenFolderPanel("选择 Excel 保存路径", _outputFolder, string.Empty);
        if (string.IsNullOrEmpty(folder))
            return;

        _outputFolder = NormalizePath(folder);
        Repaint();
    }

    /// <summary>
    /// 打开输出目录
    /// </summary>
    private void OpenOutputFolder()
    {
        if (string.IsNullOrEmpty(_outputFolder))
            return;

        if (!Directory.Exists(_outputFolder))
        {
            Directory.CreateDirectory(_outputFolder);
        }

        EditorUtility.RevealInFinder(_outputFolder);
    }

    /// <summary>
    /// 导出当前选中预览文件
    /// </summary>
    private void ExportSelectedFile()
    {
        if (_selectedPreviewIndex < 0 || _selectedPreviewIndex >= _jsonFilePaths.Count)
        {
            EditorUtility.DisplayDialog("提示", "请先选择一个文件。", "确定");
            return;
        }

        string filePath = _jsonFilePaths[_selectedPreviewIndex];
        string saveName = BuildOutputFileName(filePath);

        try
        {
            ConvertJsonFileToExcel(filePath, _outputFolder, saveName, _arrayHandlingMode);
            EditorUtility.DisplayDialog("导出成功", "当前文件导出完成。", "确定");
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            EditorUtility.DisplayDialog("导出失败", e.Message, "确定");
        }
    }

    /// <summary>
    /// 批量导出全部文件
    /// </summary>
    private void ExportAllFiles()
    {
        if (_jsonFilePaths.Count == 0)
        {
            EditorUtility.DisplayDialog("提示", "没有可导出的文件。", "确定");
            return;
        }

        int          successCount = 0;
        int          failCount    = 0;
        List<string> failedFiles  = new List<string>();

        try
        {
            for (int i = 0; i < _jsonFilePaths.Count; i++)
            {
                string filePath = _jsonFilePaths[i];
                string saveName = BuildOutputFileName(filePath);

                EditorUtility.DisplayProgressBar(
                    "批量导出 Excel",
                    string.Format("正在处理：{0}\n{1}/{2}", Path.GetFileName(filePath), i + 1, _jsonFilePaths.Count),
                    (i + 1f) / _jsonFilePaths.Count);

                try
                {
                    ConvertJsonFileToExcel(filePath, _outputFolder, saveName, _arrayHandlingMode);
                    successCount++;
                }
                catch (Exception e)
                {
                    Debug.LogError(string.Format("导出失败：{0}\n{1}", filePath, e));
                    failCount++;
                    failedFiles.Add(Path.GetFileName(filePath));
                }
            }
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }

        string msg = string.Format("批量导出完成。\n成功：{0}\n失败：{1}", successCount, failCount);

        if (failedFiles.Count > 0)
        {
            msg += "\n失败文件：\n" + string.Join("\n", failedFiles.ToArray());
        }

        EditorUtility.DisplayDialog("导出结果", msg, "确定");
    }

    /// <summary>
    /// 构建输出文件名
    /// </summary>
    private string BuildOutputFileName(string filePath)
    {
        string fileName = Path.GetFileNameWithoutExtension(filePath);

        if (!string.IsNullOrEmpty(_fileNameSuffix))
        {
            fileName += _fileNameSuffix;
        }

        return fileName;
    }

    /// <summary>
    /// 是否需要刷新预览
    /// </summary>
    private bool NeedRefreshPreview(string currentPath)
    {
        return _lastPreviewFilePath != currentPath || _lastPreviewMode != _arrayHandlingMode;
    }

    /// <summary>
    /// 使预览缓存失效
    /// </summary>
    private void InvalidatePreviewCache()
    {
        _lastPreviewFilePath = string.Empty;
        _previewError        = string.Empty;
        _previewInfos.Clear();
    }

    /// <summary>
    /// 刷新预览
    /// </summary>
    private void RefreshPreview()
    {
        _previewInfos.Clear();
        _previewError = string.Empty;

        if (_selectedPreviewIndex < 0 || _selectedPreviewIndex >= _jsonFilePaths.Count)
            return;

        string filePath = _jsonFilePaths[_selectedPreviewIndex];
        _lastPreviewFilePath = filePath;
        _lastPreviewMode     = _arrayHandlingMode;

        try
        {
            Dictionary<string, List<Dictionary<string, string>>> tables;
            BuildTablesFromJsonFile(filePath, _arrayHandlingMode, out tables);

            foreach (KeyValuePair<string, List<Dictionary<string, string>>> kv in tables)
            {
                List<string> columns = GetAllColumns(kv.Value);

                SheetPreviewInfo info = new SheetPreviewInfo();
                info.SheetName   = kv.Key;
                info.RowCount    = kv.Value.Count;
                info.ColumnCount = columns.Count;

                _previewInfos.Add(info);
            }
        }
        catch (Exception e)
        {
            _previewError = "预览失败：\n" + e.Message;
        }
    }

    /// <summary>
    /// 将单个 Json 文件转为 Excel
    /// </summary>
    private static void ConvertJsonFileToExcel(
        string jsonPath,
        string savePath,
        string saveName,
        ArrayHandlingMode arrayMode)
    {
        if (string.IsNullOrEmpty(jsonPath) || string.IsNullOrEmpty(savePath))
        {
            throw new Exception("路径为空。");
        }

        if (!File.Exists(jsonPath))
        {
            throw new Exception("Json文件不存在：" + jsonPath);
        }

        if (!Directory.Exists(savePath))
        {
            Directory.CreateDirectory(savePath);
        }

        Dictionary<string, List<Dictionary<string, string>>> tables;
        BuildTablesFromJsonFile(jsonPath, arrayMode, out tables);

        string fullSavePath = NormalizePath(Path.Combine(savePath, saveName + ".xlsx"));

        using (ExcelPackage package = new ExcelPackage())
        {
            WriteAllTablesToExcel(package, tables);
            File.WriteAllBytes(fullSavePath, package.GetAsByteArray());
        }

        Debug.Log("Json转Excel完成: " + fullSavePath);
        AssetDatabase.Refresh();
    }

    /// <summary>
    /// 从 Json 文件构建表结构
    /// </summary>
    private static void BuildTablesFromJsonFile(
        string jsonPath,
        ArrayHandlingMode arrayMode,
        out Dictionary<string, List<Dictionary<string, string>>> tables)
    {
        tables = new Dictionary<string, List<Dictionary<string, string>>>();

        string   json = File.ReadAllText(jsonPath);
        JsonData root;

        try
        {
            root = JsonMapper.ToObject(json);
        }
        catch (Exception e)
        {
            throw new Exception("Json解析失败：" + e.Message);
        }

        if (root == null)
        {
            throw new Exception("Json解析结果为空。");
        }

        Dictionary<string, int> rowCounters = new Dictionary<string, int>();

        if (root.IsObject)
        {
            CollectSingleObjectTable(root, "Root", tables, rowCounters, null, arrayMode);
        }
        else if (root.IsArray && IsArrayOfObjects(root))
        {
            // 顶层对象数组始终按主表导出，这样更实用
            CollectObjectArrayTable(root, "Root", tables, rowCounters, null, arrayMode);
        }
        else
        {
            CollectPathValueTable(root, "Root", tables, arrayMode);
        }
    }

    /// <summary>
    /// 收集单个对象为一张表（单行）
    /// </summary>
    private static void CollectSingleObjectTable(
        JsonData obj,
        string sheetName,
        Dictionary<string, List<Dictionary<string, string>>> tables,
        Dictionary<string, int> rowCounters,
        string parentId,
        ArrayHandlingMode arrayMode)
    {
        EnsureTable(tables, sheetName);

        Dictionary<string, string> row   = new Dictionary<string, string>();
        string                     rowId = GetNextRowId(sheetName, rowCounters);

        row["_id"] = rowId;
        if (!string.IsNullOrEmpty(parentId))
        {
            row["_parentId"] = parentId;
        }

        List<string> keys = GetKeys(obj);
        for (int i = 0; i < keys.Count; i++)
        {
            string key = keys[i];
            FlattenNodeToRow(obj[key], key, row, sheetName, rowId, tables, rowCounters, arrayMode);
        }

        tables[sheetName].Add(row);
    }

    /// <summary>
    /// 收集对象数组为一张表（多行）
    /// </summary>
    private static void CollectObjectArrayTable(
        JsonData array,
        string sheetName,
        Dictionary<string, List<Dictionary<string, string>>> tables,
        Dictionary<string, int> rowCounters,
        string parentId,
        ArrayHandlingMode arrayMode)
    {
        EnsureTable(tables, sheetName);

        for (int i = 0; i < array.Count; i++)
        {
            JsonData                   item  = array[i];
            Dictionary<string, string> row   = new Dictionary<string, string>();
            string                     rowId = GetNextRowId(sheetName, rowCounters);

            row["_id"]    = rowId;
            row["_index"] = i.ToString();

            if (!string.IsNullOrEmpty(parentId))
            {
                row["_parentId"] = parentId;
            }

            if (item == null)
            {
                row["value"] = string.Empty;
                tables[sheetName].Add(row);
                continue;
            }

            if (item.IsObject)
            {
                List<string> keys = GetKeys(item);
                for (int j = 0; j < keys.Count; j++)
                {
                    string key = keys[j];
                    FlattenNodeToRow(item[key], key, row, sheetName, rowId, tables, rowCounters, arrayMode);
                }
            }
            else
            {
                row["value"] = ConvertNodeToCellString(item);
            }

            tables[sheetName].Add(row);
        }
    }

    /// <summary>
    /// 将任意节点展开到当前行
    /// </summary>
    private static void FlattenNodeToRow(
        JsonData node,
        string columnPath,
        Dictionary<string, string> row,
        string currentSheet,
        string currentRowId,
        Dictionary<string, List<Dictionary<string, string>>> tables,
        Dictionary<string, int> rowCounters,
        ArrayHandlingMode arrayMode)
    {
        if (node == null)
        {
            row[columnPath] = string.Empty;
            return;
        }

        if (IsSimpleValue(node))
        {
            row[columnPath] = ConvertPrimitiveValue(node);
            return;
        }

        if (node.IsObject)
        {
            List<string> keys = GetKeys(node);
            for (int i = 0; i < keys.Count; i++)
            {
                string key       = keys[i];
                string childPath = CombinePath(columnPath, key);
                FlattenNodeToRow(node[key], childPath, row, currentSheet, currentRowId, tables, rowCounters, arrayMode);
            }

            return;
        }

        if (node.IsArray)
        {
            if (node.Count == 0)
            {
                row[columnPath] = "[]";
                return;
            }

            if (arrayMode == ArrayHandlingMode.SplitObjectArrayToChildSheet && IsArrayOfObjects(node))
            {
                row[columnPath] = "[" + node.Count + " items]";
                string childSheet = currentSheet + "__" + columnPath;
                CollectObjectArrayTable(node, childSheet, tables, rowCounters, currentRowId, arrayMode);
                return;
            }

            row[columnPath] = JsonMapper.ToJson(node);
            return;
        }

        row[columnPath] = ConvertNodeToCellString(node);
    }

    /// <summary>
    /// 顶层不是对象/对象数组时，退化为 path-value 表
    /// </summary>
    private static void CollectPathValueTable(
        JsonData root,
        string sheetName,
        Dictionary<string, List<Dictionary<string, string>>> tables,
        ArrayHandlingMode arrayMode)
    {
        EnsureTable(tables, sheetName);
        CollectPathValueRows(root, "$", tables[sheetName], arrayMode);
    }

    /// <summary>
    /// 递归收集 path-value 行
    /// </summary>
    private static void CollectPathValueRows(
        JsonData node,
        string path,
        List<Dictionary<string, string>> rows,
        ArrayHandlingMode arrayMode)
    {
        if (node == null)
        {
            rows.Add(new Dictionary<string, string>
            {
                { "path", path },
                { "type", "string" },
                { "value", string.Empty }
            });
            return;
        }

        if (IsSimpleValue(node))
        {
            rows.Add(new Dictionary<string, string>
            {
                { "path", path },
                { "type", DetectPrimitiveType(node) },
                { "value", ConvertPrimitiveValue(node) }
            });
            return;
        }

        if (node.IsObject)
        {
            List<string> keys = GetKeys(node);
            if (keys.Count == 0)
            {
                rows.Add(new Dictionary<string, string>
                {
                    { "path", path },
                    { "type", "string" },
                    { "value", "{}" }
                });
                return;
            }

            for (int i = 0; i < keys.Count; i++)
            {
                string key = keys[i];
                CollectPathValueRows(node[key], path + "." + key, rows, arrayMode);
            }

            return;
        }

        if (node.IsArray)
        {
            if (arrayMode == ArrayHandlingMode.StoreAllArraysAsJsonString)
            {
                rows.Add(new Dictionary<string, string>
                {
                    { "path", path },
                    { "type", "string" },
                    { "value", JsonMapper.ToJson(node) }
                });
                return;
            }

            if (node.Count == 0)
            {
                rows.Add(new Dictionary<string, string>
                {
                    { "path", path },
                    { "type", "string" },
                    { "value", "[]" }
                });
                return;
            }

            for (int i = 0; i < node.Count; i++)
            {
                CollectPathValueRows(node[i], path + "[" + i + "]", rows, arrayMode);
            }

            return;
        }

        rows.Add(new Dictionary<string, string>
        {
            { "path", path },
            { "type", "string" },
            { "value", ConvertNodeToCellString(node) }
        });
    }

    /// <summary>
    /// 写出所有表到 Excel
    /// </summary>
    private static void WriteAllTablesToExcel(
        ExcelPackage package,
        Dictionary<string, List<Dictionary<string, string>>> tables)
    {
        HashSet<string> usedSheetNames = new HashSet<string>();

        foreach (KeyValuePair<string, List<Dictionary<string, string>>> kv in tables)
        {
            WriteSingleTableToExcel(package, kv.Key, kv.Value, usedSheetNames);
        }
    }

    /// <summary>
    /// 写单张表
    /// </summary>
    private static void WriteSingleTableToExcel(
        ExcelPackage package,
        string logicalSheetName,
        List<Dictionary<string, string>> rows,
        HashSet<string> usedSheetNames)
    {
        string         safeSheetName = MakeSafeSheetName(logicalSheetName, usedSheetNames);
        ExcelWorksheet sheet         = package.Workbook.Worksheets.Add(safeSheetName);

        const int rowStart    = 4;
        const int columnStart = 2;

        List<string> columns = GetAllColumns(rows);
        List<string> types   = InferColumnTypes(rows, columns);

        for (int i = 0; i < columns.Count; i++)
        {
            sheet.Cells[rowStart, i + columnStart].Value = columns[i];
        }

        for (int i = 0; i < columns.Count; i++)
        {
            sheet.Cells[rowStart + 1, i + columnStart].Value = "#未注释";
        }

        for (int i = 0; i < columns.Count; i++)
        {
            sheet.Cells[rowStart + 2, i + columnStart].Value = types[i];
        }

        for (int r = 0; r < rows.Count; r++)
        {
            Dictionary<string, string> row = rows[r];

            for (int c = 0; c < columns.Count; c++)
            {
                string column = columns[c];
                string value;
                row.TryGetValue(column, out value);
                sheet.Cells[rowStart + 3 + r, c + columnStart].Value = value ?? string.Empty;
            }
        }

        if (sheet.Dimension != null)
        {
            sheet.Cells[sheet.Dimension.Address].AutoFitColumns();
            sheet.View.FreezePanes(rowStart + 3, columnStart);
        }
    }

    /// <summary>
    /// 获取所有列
    /// </summary>
    private static List<string> GetAllColumns(List<Dictionary<string, string>> rows)
    {
        List<string>    list = new List<string>();
        HashSet<string> set  = new HashSet<string>();

        AddColumnIfNotExists("_id", list, set);
        AddColumnIfNotExists("_parentId", list, set);
        AddColumnIfNotExists("_index", list, set);

        for (int i = 0; i < rows.Count; i++)
        {
            Dictionary<string, string> row = rows[i];
            foreach (KeyValuePair<string, string> kv in row)
            {
                AddColumnIfNotExists(kv.Key, list, set);
            }
        }

        return list;
    }

    /// <summary>
    /// 推断列类型
    /// </summary>
    private static List<string> InferColumnTypes(List<Dictionary<string, string>> rows, List<string> columns)
    {
        List<string> result = new List<string>();

        for (int i = 0; i < columns.Count; i++)
        {
            string column = columns[i];

            if (column == "_index")
            {
                result.Add("int");
                continue;
            }

            if (column == "_id" || column == "_parentId")
            {
                result.Add("string");
                continue;
            }

            bool hasValue = false;
            bool allBool  = true;
            bool allInt   = true;
            bool allFloat = true;

            for (int r = 0; r < rows.Count; r++)
            {
                string value;
                if (!rows[r].TryGetValue(column, out value) || string.IsNullOrEmpty(value))
                    continue;

                hasValue = true;
                string trimmed = value.Trim();

                if (!(trimmed == "true" || trimmed == "false"))
                    allBool = false;

                int intVal;
                if (!int.TryParse(trimmed, NumberStyles.Integer, CultureInfo.InvariantCulture, out intVal))
                    allInt = false;

                double floatVal;
                if (!double.TryParse(trimmed, NumberStyles.Float, CultureInfo.InvariantCulture, out floatVal))
                    allFloat = false;

                if (!allBool && !allInt && !allFloat)
                    break;
            }

            if (!hasValue)
                result.Add("string");
            else if (allBool)
                result.Add("bool");
            else if (allInt)
                result.Add("int");
            else if (allFloat)
                result.Add("float");
            else
                result.Add("string");
        }

        return result;
    }

    /// <summary>
    /// 添加列
    /// </summary>
    private static void AddColumnIfNotExists(string column, List<string> list, HashSet<string> set)
    {
        if (set.Add(column))
        {
            list.Add(column);
        }
    }

    /// <summary>
    /// 判断是否为基础值
    /// </summary>
    private static bool IsSimpleValue(JsonData data)
    {
        if (data == null) return true;
        return data.IsString || data.IsBoolean || data.IsInt || data.IsLong || data.IsDouble;
    }

    /// <summary>
    /// 判断数组是否全部为对象
    /// </summary>
    private static bool IsArrayOfObjects(JsonData data)
    {
        if (data == null || !data.IsArray || data.Count == 0)
            return false;

        for (int i = 0; i < data.Count; i++)
        {
            JsonData item = data[i];
            if (item == null || !item.IsObject)
                return false;
        }

        return true;
    }

    /// <summary>
    /// 基础值转字符串
    /// </summary>
    private static string ConvertPrimitiveValue(JsonData data)
    {
        if (data == null)
            return string.Empty;

        if (data.IsBoolean)
            return data.ToString().ToLower();

        return data.ToString();
    }

    /// <summary>
    /// 任意节点转单元格字符串
    /// </summary>
    private static string ConvertNodeToCellString(JsonData data)
    {
        if (data == null)
            return string.Empty;

        if (IsSimpleValue(data))
            return ConvertPrimitiveValue(data);

        return JsonMapper.ToJson(data);
    }

    /// <summary>
    /// 基础类型判断
    /// </summary>
    private static string DetectPrimitiveType(JsonData data)
    {
        if (data == null) return "string";
        if (data.IsBoolean) return "bool";
        if (data.IsInt || data.IsLong) return "int";
        if (data.IsDouble) return "float";
        return "string";
    }

    /// <summary>
    /// 获取对象全部键
    /// </summary>
    private static List<string> GetKeys(JsonData data)
    {
        List<string> list = new List<string>();

        if (data == null || !data.IsObject)
            return list;

        IDictionary dict = (IDictionary)data;
        foreach (object key in dict.Keys)
        {
            list.Add(key.ToString());
        }

        return list;
    }

    /// <summary>
    /// 组合路径
    /// </summary>
    private static string CombinePath(string parent, string child)
    {
        if (string.IsNullOrEmpty(parent)) return child;
        if (string.IsNullOrEmpty(child)) return parent;
        return parent + "." + child;
    }

    /// <summary>
    /// 确保表存在
    /// </summary>
    private static void EnsureTable(
        Dictionary<string, List<Dictionary<string, string>>> tables,
        string sheetName)
    {
        if (!tables.ContainsKey(sheetName))
        {
            tables[sheetName] = new List<Dictionary<string, string>>();
        }
    }

    /// <summary>
    /// 获取表内下一条ID
    /// </summary>
    private static string GetNextRowId(string sheetName, Dictionary<string, int> rowCounters)
    {
        if (!rowCounters.ContainsKey(sheetName))
        {
            rowCounters[sheetName] = 0;
        }

        rowCounters[sheetName]++;
        return rowCounters[sheetName].ToString();
    }

    /// <summary>
    /// 生成合法 Sheet 名
    /// </summary>
    private static string MakeSafeSheetName(string rawName, HashSet<string> usedSheetNames)
    {
        if (string.IsNullOrWhiteSpace(rawName))
        {
            rawName = "Sheet";
        }

        string name = rawName
            .Replace("\\", "_")
            .Replace("/", "_")
            .Replace("?", "_")
            .Replace("*", "_")
            .Replace("[", "_")
            .Replace("]", "_")
            .Replace(":", "_");

        if (name.Length > 31)
        {
            name = name.Substring(0, 31);
        }

        string finalName = name;
        int    index     = 1;

        while (usedSheetNames.Contains(finalName))
        {
            string suffix     = "_" + index;
            int    maxBaseLen = 31 - suffix.Length;
            string baseName   = name.Length > maxBaseLen ? name.Substring(0, maxBaseLen) : name;
            finalName = baseName + suffix;
            index++;
        }

        usedSheetNames.Add(finalName);
        return finalName;
    }

    /// <summary>
    /// 判断是否为支持的 Json 文件
    /// </summary>
    private static bool IsSupportedJsonFile(string path)
    {
        if (string.IsNullOrEmpty(path))
            return false;

        string ext = Path.GetExtension(path).ToLowerInvariant();
        return ext == ".json" || ext == ".txt";
    }

    /// <summary>
    /// 将资源路径转绝对路径
    /// </summary>
    private static string ToAbsolutePath(string path)
    {
        if (string.IsNullOrEmpty(path))
            return string.Empty;

        if (Path.IsPathRooted(path))
            return NormalizePath(path);

        string projectRoot = Directory.GetParent(Application.dataPath) != null
                                 ? Directory.GetParent(Application.dataPath).FullName
                                 : Application.dataPath;

        return NormalizePath(Path.Combine(projectRoot, path));
    }

    /// <summary>
    /// 规范化路径分隔符
    /// </summary>
    private static string NormalizePath(string path)
    {
        return string.IsNullOrEmpty(path) ? string.Empty : path.Replace("\\", "/");
    }
}