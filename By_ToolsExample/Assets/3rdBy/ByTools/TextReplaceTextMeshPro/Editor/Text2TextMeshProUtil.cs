namespace _3rdBy.ByTools.TextReplaceTextMeshPro.Editor
{
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using TMPro;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.UI;

    public class Text2TextMeshProUtil : EditorWindow
    {
        private static List<string> _scriptsFolders;

        private static EditorWindow _myWindow;
        private static List<string> _directoryPrefabs;
        private static List<string> _allAssetPaths;
        private static TMP_FontAsset _tmpFont;
        private readonly string[] _replaceMode = { "批量替换", "单个替换" };

        private bool _isAutoFixLinkedScripts = true;
        private string _path;
        private int _replaceModeIndex;
        private Vector2 _scrollPos = Vector2.zero;
        private bool _showPrefabs = true;

        private string path
        {
            get => _path;
            set
            {
                if (value != _path)
                {
                    if (_directoryPrefabs == null)
                        _directoryPrefabs = new List<string>();
                    else
                        _directoryPrefabs.Clear();

                    if (_allAssetPaths == null)
                        _allAssetPaths = new List<string>();
                    else
                        _allAssetPaths.Clear();

                    _path = value;
                }
            }
        }

        private int replaceModeIndex
        {
            get => _replaceModeIndex;
            set
            {
                if (value != _replaceModeIndex)
                {
                    if (_directoryPrefabs == null)
                        _directoryPrefabs = new List<string>();
                    else
                        _directoryPrefabs.Clear();

                    if (_allAssetPaths == null)
                        _allAssetPaths = new List<string>();
                    else
                        _allAssetPaths.Clear();

                    path              = "";
                    _replaceModeIndex = value;
                }
            }
        }

        [MenuItem("ByTools/🧩 替换Text为TextMeshPro")]
        private static void Init()
        {
            _myWindow         = GetWindow(typeof(Text2TextMeshProUtil));
            _directoryPrefabs = new List<string>();
            _allAssetPaths    = new List<string>();
            _scriptsFolders   = new List<string> { "Assets/Scripts/UI" };
            _tmpFont          = TMP_Settings.defaultFontAsset;
            // myWindow.minSize = new Vector2(500, 500);
            // myWindow.maxSize = new Vector2(500, 500);
        }

        private void OnGUI()
        {
            _tmpFont                = (TMP_FontAsset)EditorGUILayout.ObjectField("字体文件", _tmpFont, typeof(TMP_FontAsset), false);
            _isAutoFixLinkedScripts = EditorGUILayout.BeginToggleGroup("修改关联脚本", _isAutoFixLinkedScripts);
            ShowScriptsFolders();
            EditorGUILayout.EndToggleGroup();
            replaceModeIndex = GUILayout.Toolbar(replaceModeIndex, _replaceMode);
            if (replaceModeIndex == 0)
            {
                _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos, GUILayout.Width(_myWindow.position.width),
                    GUILayout.Height(_myWindow.position.height - _scriptsFolders.Count * 20 - 90));
                GetPath();
                ShowAllPrefabs();
                EditorGUILayout.EndScrollView();
                EditorGUILayout.BeginHorizontal();
                LoadPrefab();
                Text2TextMeshPro();
                EditorGUILayout.EndHorizontal();
            }
            else if (replaceModeIndex == 1)
            {
                _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos, GUILayout.Width(_myWindow.position.width),
                    GUILayout.Height(_myWindow.position.height - _scriptsFolders.Count * 20 - 90));
                GetPath(false);
                ShowAllPrefabs();
                EditorGUILayout.EndScrollView();
                Text2TextMeshPro();
            }
        }

        private void GetPath(bool isFolder = true)
        {
            var e = Event.current;
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Path:", GUILayout.Width(40));
            path = GUILayout.TextField(path);
            EditorGUILayout.EndHorizontal();
            if (Event.current.type == EventType.DragExited || Event.current.type == EventType.DragUpdated)
            {
                if (GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition))
                {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Link;
                    if (Event.current.type == EventType.DragExited)
                    {
                        DragAndDrop.AcceptDrag();
                        if (!isFolder)
                        {
                            if (DragAndDrop.objectReferences != null && DragAndDrop.objectReferences.Length > 0)
                            {
                                var objectReferences = DragAndDrop.objectReferences;
                                for (var i = 0; i < objectReferences.Length; i++)
                                {
                                    var index = i;
                                    if (AssetDatabase.GetAssetPath(objectReferences[index]).EndsWith(".prefab"))
                                        if (!_directoryPrefabs.Contains(
                                                AssetDatabase.GetAssetPath(objectReferences[index])))
                                            _directoryPrefabs.Add(AssetDatabase.GetAssetPath(objectReferences[index]));
                                }
                            }
                        }
                        else
                        {
                            if (DragAndDrop.paths != null && DragAndDrop.paths.Length > 0)
                            {
                                if (File.Exists(DragAndDrop.paths[0]))
                                {
                                    EditorUtility.DisplayDialog("警告", "批量模式下请拖拽文件夹！", "确定");
                                    return;
                                }

                                path = DragAndDrop.paths[0];
                            }
                        }
                    }

                    e.Use();
                }
                else
                {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
                }
            }
        }

        private void ShowAllPrefabs()
        {
            if (_directoryPrefabs != null && _directoryPrefabs.Count > 0)
            {
                _showPrefabs = EditorGUILayout.Foldout(_showPrefabs, "显示预制体");
                if (_showPrefabs)
                {
                    for (var i = 0; i < _directoryPrefabs.Count; i++)
                    {
                        var index = i;
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.SelectableLabel($"预制体路径：{_directoryPrefabs[index]}");
                        if (GUILayout.Button("查看", GUILayout.Width(60)))
                        {
                            EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<Object>(_directoryPrefabs[index]));
                            Selection.activeGameObject =
                                AssetDatabase.LoadAssetAtPath<Object>(_directoryPrefabs[index]) as GameObject;
                        }

                        if (GUILayout.Button("删除", GUILayout.Width(60))) _directoryPrefabs.RemoveAt(index);

                        EditorGUILayout.EndHorizontal();
                    }
                }
                else
                {
                    if (GUILayout.Button("清空选择", GUILayout.Width(60)))
                    {
                        _directoryPrefabs.Clear();
                        _allAssetPaths.Clear();
                        path = "";
                    }
                }
            }
        }

        private void LoadPrefab()
        {
            if (GUILayout.Button("加载预制体"))
                if (!string.IsNullOrEmpty(path))
                {
                    var direction = new DirectoryInfo(path);
                    var files     = direction.GetFiles("*.prefab", SearchOption.AllDirectories);
                    for (var i = 0; i < files.Length; i++)
                    {
                        var startindex = files[i].FullName.IndexOf("Assets");
                        var unityPath  = files[i].FullName.Substring(startindex);
                        if (!_directoryPrefabs.Contains(unityPath)) _directoryPrefabs.Add(unityPath);
                    }
                }
        }

        private void Text2TextMeshPro()
        {
            if (_directoryPrefabs is { Count: > 0 })
                if (GUILayout.Button("一键替换"))
                {
                    if (_tmpFont == null)
                    {
                        EditorUtility.DisplayDialog("警告", "请先选择字体！", "确定");
                        return;
                    }

                    for (var i = 0; i < _directoryPrefabs.Count; i++)
                    {
                        var index = i;
                        Text2TextMeshPro(_directoryPrefabs[index]);
                        EditorUtility.DisplayProgressBar("替换进度", "当前进度", index / (float)_directoryPrefabs.Count);
                    }

                    EditorUtility.ClearProgressBar();
                }
        }

        private void Text2TextMeshPro(string path)
        {
            var root = PrefabUtility.LoadPrefabContents(path);
            if (root)
            {
                var list = root.GetComponentsInChildren<Text>(true);
                for (var i = 0; i < list.Length; i++)
                {
                    var text               = list[i];
                    var target             = text.transform;
                    var size               = text.rectTransform.sizeDelta;
                    var strContent         = text.text;
                    var color              = text.color;
                    var fontSize           = text.fontSize;
                    var fontStyle          = text.fontStyle;
                    var textAnchor         = text.alignment;
                    var richText           = text.supportRichText;
                    var horizontalWrapMode = text.horizontalOverflow;
                    var verticalWrapMode   = text.verticalOverflow;
                    var raycastTarget      = text.raycastTarget;
                    DestroyImmediate(text);

                    var textMeshPro = target.gameObject.AddComponent<TextMeshProUGUI>();
                    textMeshPro.font                    = _tmpFont;
                    textMeshPro.rectTransform.sizeDelta = size;
                    textMeshPro.text                    = strContent;
                    textMeshPro.color                   = color;
                    textMeshPro.fontSize                = fontSize;
                    textMeshPro.fontStyle =
                        fontStyle == FontStyle.BoldAndItalic ? FontStyles.Bold : (FontStyles)fontStyle;
                    switch (textAnchor)
                    {
                        case TextAnchor.UpperLeft:
                            textMeshPro.alignment = TextAlignmentOptions.TopLeft;
                            break;
                        case TextAnchor.UpperCenter:
                            textMeshPro.alignment = TextAlignmentOptions.Top;
                            break;
                        case TextAnchor.UpperRight:
                            textMeshPro.alignment = TextAlignmentOptions.TopRight;
                            break;
                        case TextAnchor.MiddleLeft:
                            textMeshPro.alignment = TextAlignmentOptions.MidlineLeft;
                            break;
                        case TextAnchor.MiddleCenter:
                            textMeshPro.alignment = TextAlignmentOptions.Midline;
                            break;
                        case TextAnchor.MiddleRight:
                            textMeshPro.alignment = TextAlignmentOptions.MidlineRight;
                            break;
                        case TextAnchor.LowerLeft:
                            textMeshPro.alignment = TextAlignmentOptions.BottomLeft;
                            break;
                        case TextAnchor.LowerCenter:
                            textMeshPro.alignment = TextAlignmentOptions.Bottom;
                            break;
                        case TextAnchor.LowerRight:
                            textMeshPro.alignment = TextAlignmentOptions.BottomRight;
                            break;
                    }

                    textMeshPro.richText = richText;
                    if (verticalWrapMode == VerticalWrapMode.Overflow)
                    {
                        textMeshPro.enableWordWrapping = true;
                        textMeshPro.overflowMode       = TextOverflowModes.Overflow;
                    }
                    else
                    {
                        textMeshPro.enableWordWrapping =
                            horizontalWrapMode == HorizontalWrapMode.Overflow ? false : true;
                    }

                    textMeshPro.raycastTarget = raycastTarget;
                }
            }

            PrefabUtility.SaveAsPrefabAsset(root, path, out var success);
            if (!success) Debug.LogError($"预制体：{path} 保存失败!");

            if (_isAutoFixLinkedScripts) ChangeScriptsText2TextMeshPro();
        }

        private void ShowScriptsFolders()
        {
            for (var i = 0; i < _scriptsFolders.Count; i++)
            {
                var index = i;
                EditorGUILayout.BeginHorizontal();
                SelectScriptsFolder(index);
                EditorGUILayout.EndHorizontal();
            }
        }

        private void SelectScriptsFolder(int index)
        {
            var e = Event.current;
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label($"scriptsFolder{index + 1}:", GUILayout.Width(80));
            _scriptsFolders[index] = GUILayout.TextField(_scriptsFolders[index], GUILayout.Width(200));
            if (Event.current.type == EventType.DragExited || Event.current.type == EventType.DragUpdated)
            {
                if (GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition))
                {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Link;
                    if (Event.current.type == EventType.DragExited)
                    {
                        DragAndDrop.AcceptDrag();
                        if (DragAndDrop.paths != null && DragAndDrop.paths.Length > 0)
                        {
                            if (File.Exists(DragAndDrop.paths[0]))
                            {
                                EditorUtility.DisplayDialog("警告", "请选择文件夹！", "确定");
                                return;
                            }

                            _scriptsFolders[index] = DragAndDrop.paths[0];
                        }
                    }

                    e.Use();
                }
                else
                {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
                }
            }

            if (GUILayout.Button("添加路径")) _scriptsFolders.Add(_scriptsFolders[_scriptsFolders.Count - 1]);

            if (GUILayout.Button("删除路径"))
            {
                if (_scriptsFolders.Count == 1)
                {
                    EditorUtility.DisplayDialog("警告", "仅剩最后一个文件夹，删除将会出错！", "确定");
                    return;
                }

                _scriptsFolders.RemoveAt(index);
            }

            EditorGUILayout.EndHorizontal();
        }

        private void ChangeScriptsText2TextMeshPro()
        {
            _allAssetPaths.Clear();
            Debug.LogError(_directoryPrefabs.Count);
            if (_directoryPrefabs != null && _directoryPrefabs.Count > 0)
            {
                for (var i = 0; i < _directoryPrefabs.Count; i++)
                {
                    var index       = i;
                    var scriptsName = System.IO.Path.GetFileNameWithoutExtension(_directoryPrefabs[index]);
                    var tmp         = AssetDatabase.FindAssets($"{scriptsName} t:Script", _scriptsFolders.ToArray());
                    if (tmp != null && tmp.Length > 0) _allAssetPaths.AddRange(tmp);
                }

                for (var i = 0; i < _allAssetPaths.Count; i++)
                {
                    var index = i;
                    ChangeScriptsText2TextMeshPro(AssetDatabase.GUIDToAssetPath(_allAssetPaths[index]));
                }
            }

            AssetDatabase.Refresh();
        }

        private void ChangeScriptsText2TextMeshPro(string script)
        {
            var sr  = new StreamReader(script);
            var str = sr.ReadToEnd();
            sr.Close();
            str = str.Replace("<Text>", "<TMPro.TextMeshProUGUI>");
            str = str.Replace(" Text ", " TMPro.TextMeshProUGUI ");
            var sw = new StreamWriter(script, false, Encoding.UTF8);
            sw.Write(str);
            sw.Close();
        }
    }
}