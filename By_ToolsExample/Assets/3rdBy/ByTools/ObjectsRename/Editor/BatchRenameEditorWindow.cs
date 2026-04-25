namespace _3rdBy.ByTools.ObjectsRename.Editor
{
    using UnityEditor;
    using UnityEngine;
    using System.Collections.Generic;

    /// <summary>
    /// 批量重命名器
    /// </summary>
    public class BatchRenameEditorWindow : EditorWindow
    {
        #region ===== 枚举 & 数据 =====

        private enum RenameMode
        {
            [InspectorName("名称替换")] Replace,
            [InspectorName("追加文本")] AppendText,
            [InspectorName("自动序号")] AutoIndex
        }

        private enum AppendPosition
        {
            [InspectorName("前缀")] Prefix,
            [InspectorName("中间")] Insert,
            [InspectorName("后缀")] Suffix
        }

        private enum IndexFormat
        {
            [InspectorName("阿拉伯数字")] Arabic,
            [InspectorName("中文数字")] ChineseLower,
            [InspectorName("中文大写数字")] ChineseUpper,
            [InspectorName("英文字母小写")] EnglishLower,
            [InspectorName("英文字母大写")] EnglishUpper
        }

        private class ObjectInfo
        {
            [Header("对象信息")] public Object obj;
            [Header("原始名称")] public string originalName;
            [Header("新名称")] public string newName;
            [Header("是否Scene对象")] public bool isSceneObject;
            [Header("路径")] public string path;
        }

        #endregion

        #region ===== 字段 =====

        [Header("窗口大小")] private static readonly Vector2 windowSize = new(800, 600);
        [Header("对象列表")] private readonly List<ObjectInfo> _selectedObjects = new();
        [Header("滚动视图")] private Vector2 _scroll;

        [Header("重命名规则")] private RenameMode _renameMode = RenameMode.Replace;
        [Header("起始序号")] private int _startNumber = 1;

        // 替换
        [Header("新名称")] private string _replaceName = "NewName";

        // 追加
        [Header("追加文本")] private string _appendText = "_";
        [Header("位置")] private AppendPosition _appendPosition = AppendPosition.Suffix;
        [Header("插入索引")] private int _insertIndex;
        [Header("追加自动序号")] private bool _appendUseIndex = true;
        [Header("序号格式")] private IndexFormat _appendIndexFormat = IndexFormat.Arabic;

        // 自动索引
        [Header("前缀")] private string _indexPrefix = "Object_";
        [Header("序号格式")] private IndexFormat _indexOnlyFormat = IndexFormat.Arabic;


        private const string RENAME_DESC = "<color=yellow><size=14><b> [ 从 Scene 或 Project 窗口拖拽对象到此处区域(支持混合拖拽) ] </b></size></color>\n" +
                                          "<color=green> 1、 选择重命名模式 • 名称替换：直接替换为新名称 • 追加文本：在名称 前-中-后 追加文本 • 自动序号：根据序号规则生成名称</color>\n" +
                                          "<color=green> 2、 自动序号支持阿拉伯数字、中文数字、英文字母，序号顺序与列表顺序一致</color>\n" +
                                          "<color=green> 3、 Scene 对象支持撤销(Undo)，Project 资源不可撤销(Undo)</color>";

        #endregion

        #region ===== Window =====

        [MenuItem("ByTools/🌀 对象批量重命名")]
        private static void Open()
        {
            var window = GetWindow<BatchRenameEditorWindow>("对象批量重命名");
            window.minSize           = windowSize;
            window.maxSize           = windowSize;
            window.titleContent.text = "对象批量重命名";
        }

        private GUIStyle _helpBoxStyle;
        private GUIStyle _elementStyle;

        private void OnEnable()
        {
            _helpBoxStyle ??= new GUIStyle(EditorStyles.helpBox)
            {
                richText = true,
                wordWrap = true
            };
            _elementStyle ??= new GUIStyle(EditorStyles.miniLabel)
            {
                richText = true,
            };
        }

        private void OnGUI()
        {
            DrawDropArea();

            if (_selectedObjects.Count == 0) return;

            EditorGUILayout.Space(8);
            DrawRenameSettings();
            EditorGUILayout.Space(8);
            DrawObjectList();
            EditorGUILayout.Space(8);
            DrawButtons();
        }

        #endregion

        #region ===== UI =====

        private void DrawDropArea()
        {
            var rect = GUILayoutUtility.GetRect(0, 70, GUILayout.ExpandWidth(true));
            GUI.Box(rect, RENAME_DESC, _helpBoxStyle);

            var e = Event.current;
            if (!rect.Contains(e.mousePosition)) return;

            if (e.type == EventType.DragUpdated)
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                e.Use();
            }
            else if (e.type == EventType.DragPerform)
            {
                DragAndDrop.AcceptDrag();
                ProcessDroppedObjects(DragAndDrop.objectReferences);
                e.Use();
            }
        }


        private void DrawRenameSettings()
        {
            _renameMode = (RenameMode)EditorGUILayout.EnumPopup("模式", _renameMode);

            EditorGUILayout.BeginVertical("box");

            switch (_renameMode)
            {
                case RenameMode.Replace:
                    _replaceName = EditorGUILayout.TextField("新名称", _replaceName);
                    break;

                case RenameMode.AppendText:
                    _appendText     = EditorGUILayout.TextField("追加文本", _appendText);
                    _appendPosition = (AppendPosition)EditorGUILayout.EnumPopup("位置", _appendPosition);

                    if (_appendPosition == AppendPosition.Insert)
                        _insertIndex = EditorGUILayout.IntField("插入索引", _insertIndex);

                    _appendUseIndex = EditorGUILayout.Toggle("追加自动序号", _appendUseIndex);
                    if (_appendUseIndex)
                        _appendIndexFormat = (IndexFormat)EditorGUILayout.EnumPopup("序号格式", _appendIndexFormat);
                    break;

                case RenameMode.AutoIndex:
                    _indexPrefix     = EditorGUILayout.TextField("前缀", _indexPrefix);
                    _indexOnlyFormat = (IndexFormat)EditorGUILayout.EnumPopup("序号格式", _indexOnlyFormat);
                    break;
            }

            _startNumber = EditorGUILayout.IntField("起始序号", _startNumber);

            if (GUILayout.Button("预览重命名", GUILayout.Height(28)))
                PreviewRename();

            EditorGUILayout.EndVertical();
        }

        private void DrawObjectList()
        {
            EditorGUILayout.LabelField($"对象列表 ({_selectedObjects.Count})", EditorStyles.boldLabel);

            _scroll = EditorGUILayout.BeginScrollView(_scroll, "box");

            for (int i = 0; i < _selectedObjects.Count; i++)
            {
                var info = _selectedObjects[i];

                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.BeginHorizontal();
                // 序号
                EditorGUILayout.LabelField($"{i + 1:00}、", GUILayout.Width(30));

                // 图标（可点击定位）
                var icon = EditorGUIUtility.ObjectContent(info.obj, info.obj.GetType());

                if (GUILayout.Button(icon.image, GUIStyle.none, GUILayout.Width(20), GUILayout.Height(20)))
                {
                    PingAndSelectObject(info.obj);
                }

                // 原始名称
                EditorGUILayout.LabelField(info.originalName, GUILayout.Width(220));

                // 新名称
                info.newName = EditorGUILayout.TextField(info.newName);

                // 移除
                if (GUILayout.Button("×", GUILayout.Width(22)))
                {
                    _selectedObjects.RemoveAt(i);
                    break;
                }

                EditorGUILayout.EndHorizontal();

                // 路径显示
                string path = info.isSceneObject && info.obj is GameObject go ? GetSceneObjectPath(go) : info.path;

                var sourceDesc = info.isSceneObject ? "<color=green>[Scene]</color> 路径：" : "<color=red>[Project]</color> 路径：";
                EditorGUILayout.LabelField($"<color=grey>{sourceDesc}{path}</color>", _elementStyle);

                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(2);
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawButtons()
        {
            EditorGUILayout.BeginHorizontal();

            GUI.backgroundColor = Color.green;
            if (GUILayout.Button("应用重命名", GUILayout.Height(36))) ApplyRename();

            GUI.backgroundColor = Color.red;
            if (GUILayout.Button("清空列表", GUILayout.Height(36))) _selectedObjects.Clear();

            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();
        }

        #endregion

        #region ===== 核心逻辑 =====

        private void ProcessDroppedObjects(Object[] objects)
        {
            foreach (var obj in objects)
            {
                if (_selectedObjects.Exists(o => o.obj == obj)) continue;

                bool isSceneObject = obj is GameObject go && go.scene.IsValid();

                _selectedObjects.Add(new ObjectInfo
                {
                    obj           = obj,
                    originalName  = obj.name,
                    newName       = obj.name,
                    isSceneObject = isSceneObject,
                    path          = AssetDatabase.GetAssetPath(obj)
                });
            }
        }

        private void PreviewRename()
        {
            int counter = _startNumber;

            foreach (var obj in _selectedObjects)
            {
                switch (_renameMode)
                {
                    case RenameMode.Replace:
                        obj.newName = _replaceName;
                        break;

                    case RenameMode.AppendText:
                        string indexStr = _appendUseIndex ? IndexFormatter.Format(counter, _appendIndexFormat) : "";

                        string append = _appendText + indexStr;

                        obj.newName = _appendPosition switch
                        {
                            AppendPosition.Prefix => append + obj.originalName,
                            AppendPosition.Insert => obj.originalName.Insert(Mathf.Clamp(_insertIndex, 0, obj.originalName.Length), append), AppendPosition.Suffix => obj.originalName + append,
                            _                     => obj.originalName
                        };
                        break;

                    case RenameMode.AutoIndex:
                        obj.newName = _indexPrefix + IndexFormatter.Format(counter, _indexOnlyFormat);
                        break;
                }

                counter++;
            }
        }

        private void ApplyRename()
        {
            Undo.RecordObjects(
                _selectedObjects.ConvertAll(o => o.obj).ToArray(), "批量改名");

            foreach (var obj in _selectedObjects)
            {
                if (string.IsNullOrEmpty(obj.newName) ||
                    obj.newName == obj.originalName)
                    continue;

                if (obj.isSceneObject && obj.obj is GameObject go)
                {
                    go.name = obj.newName;
                }
                else if (!string.IsNullOrEmpty(obj.path))
                {
                    AssetDatabase.RenameAsset(obj.path, obj.newName);
                }

                obj.originalName = obj.newName;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorApplication.RepaintHierarchyWindow();
            EditorApplication.RepaintProjectWindow();
        }

        #endregion

        #region ===== 工具方法 =====

        private static void PingAndSelectObject(Object obj)
        {
            Selection.activeObject = obj;
            EditorGUIUtility.PingObject(obj);

            if (obj is GameObject)
                SceneView.lastActiveSceneView?.FrameSelected();
        }

        private static string GetSceneObjectPath(GameObject go)
        {
            string path   = go.name;
            var    parent = go.transform.parent;

            while (parent != null)
            {
                path   = parent.name + "/" + path;
                parent = parent.parent;
            }

            return $"{go.scene.name}/{path}";
        }

        #endregion

        #region ===== 序号格式 =====

        private static class IndexFormatter
        {
            private static readonly string[] cnLower =
                { "零", "一", "二", "三", "四", "五", "六", "七", "八", "九" };

            private static readonly string[] cnUpper =
                { "零", "壹", "贰", "叁", "肆", "伍", "陆", "柒", "捌", "玖" };

            public static string Format(int index, IndexFormat format)
            {
                return format switch
                {
                    IndexFormat.Arabic       => index.ToString(),
                    IndexFormat.EnglishLower => ((char)('a' + (index - 1) % 26)).ToString(),
                    IndexFormat.EnglishUpper => ((char)('A' + (index - 1) % 26)).ToString(),
                    IndexFormat.ChineseLower => ToChinese(index, cnLower),
                    IndexFormat.ChineseUpper => ToChinese(index, cnUpper),
                    _                        => index.ToString()
                };
            }

            private static string ToChinese(int num, string[] map)
            {
                return num switch
                {
                    < 10  => map[num],
                    < 20  => "十" + (num % 10 == 0 ? "" : map[num % 10]),
                    < 100 => map[num / 10] + "十" + (num % 10 == 0 ? "" : map[num % 10]),
                    _     => num.ToString()
                };
            }
        }

        #endregion
    }
}