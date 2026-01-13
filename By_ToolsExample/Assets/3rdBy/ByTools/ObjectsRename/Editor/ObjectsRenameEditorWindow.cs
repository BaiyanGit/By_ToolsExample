namespace _3rdBy.ByTools.ObjectsRename.Editor
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// 场景对象批量重命名
    /// </summary>
    public class ObjectsRenameEditorWindow : EditorWindow
    {
        [MenuItem("ByTools/🧩 对象批量重命名")]
        public static void ShowWindow()
        {
            var screenRes = new Vector2(Screen.currentResolution.width, Screen.currentResolution.height); // 获取当前屏幕的分辨率
            var pos       = new Vector2(screenRes.x / 2 - 300, screenRes.y / 2 - 300);                    // 计算窗口的中心位置
            var window    = GetWindow<ObjectsRenameEditorWindow>("对象批量重命名");
            window.minSize  = Vector2.one * 600;
            window.maxSize  = Vector2.one * 600;
            window.position = new Rect(pos.x, pos.y, 600, 600);
            window.Show();
        }

        private Vector2 _scrollPos;
        private bool _togFindReplace = true;          // 查找替换
        private bool _togAddCharacter;                // 增加字符/序号
        private bool _togSerialNumberFormat;          // 序号格式化
        private string _findText = string.Empty;      // 查找字符
        private string _replaceText = string.Empty;   // 替换字符
        private string _appendText = string.Empty;    // 追加的文本
        private string _appendTextPos = string.Empty; // 追加文本的指定位置
        private bool _togFront = true;                // 追加字符最前面
        private bool _togLast;                        // 追加字符最后面
        private bool _togIndexPos;                    // 追加字符指定位置
        private string _tipContent = string.Empty;

        private void OnGUI()
        {
            // TIPS:绘制头部复选框
            EditorGUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();
            DrawSearchToggle();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.LabelField
                ("---------------------------------------------------------------------------------------------------");
            // TIPS:根据复选框绘制搜索条件
            EditorGUILayout.Space(20);
            EditorGUILayout.BeginHorizontal();
            DrawSearchCondition();
            if (GUILayout.Button("应用重命名"))
            {
                if (Selection.gameObjects.Length == 0)
                {
                    EditorUtility.DisplayDialog("警告", "您没有选择任何一个对象!", "确定");
                    return;
                }

                if (_togFindReplace) FindReplace();
                if (_togAddCharacter) AddCharacter();
                if (_togSerialNumberFormat) AddSerialNumber();
            }

            if (GUILayout.Button("清除对象列表"))
            {
                _waiteReNameObjects.Clear();
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.LabelField
                ("\n---------------------------------------------------------------------------------------------------");

            GUILayout.Label("在Hierarchy/Project视图中选择对象", EditorStyles.boldLabel);

            DrawDropArea();

            DrawWaiteRename();
            GUILayout.Label(_tipContent);
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
            GUI.Box(rect, "把重命名对象拖拽到这里（支持跨路径）", label);
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
                        if (obj is GameObject go)
                        {
                            RecordGo(go);
                        }
                    }
                }

                evt.Use();
            }
        }

        private readonly Dictionary<int, GameObject> _waiteReNameObjects = new();

        private void DrawWaiteRename()
        {
            var renameObjects = _waiteReNameObjects.Values.ToArray();
            if (renameObjects.Length == 0)
                return;
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);
            foreach (var obj in renameObjects)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.ObjectField(obj, typeof(GameObject), true);
                if (GUILayout.Button("X", GUILayout.Width(20)))
                    CancelSelection(obj);
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();
        }

        private void CancelSelection(GameObject obj)
        {
            if (_waiteReNameObjects.ContainsKey(obj.GetInstanceID()))
            {
                _waiteReNameObjects.Remove(obj.GetInstanceID());
            }
        }

        private void RecordGo(GameObject go)
        {
            var isExit = _waiteReNameObjects.TryAdd(go.GetInstanceID(), go);
            if (isExit)
            {
                Debug.Log("添加成功");
            }
        }

        /// <summary>
        /// 绘制搜索复选框
        /// </summary>
        private void DrawSearchToggle()
        {
            var newTogFindReplace = EditorGUILayout.Toggle("", _togFindReplace, GUILayout.Width(30));
            EditorGUILayout.LabelField("查找替换", GUILayout.Width(160));
            if (newTogFindReplace != _togFindReplace)
            {
                _togFindReplace        = true;
                _togAddCharacter       = false;
                _togSerialNumberFormat = false;
            }

            var newTogAddCharacter = EditorGUILayout.Toggle("", _togAddCharacter, GUILayout.Width(30));
            EditorGUILayout.LabelField("增加字符/序号", GUILayout.Width(160));
            if (newTogAddCharacter != _togAddCharacter)
            {
                _togAddCharacter       = true;
                _togFindReplace        = false;
                _togSerialNumberFormat = false;
            }

            var newTogSerialNumberFormat = EditorGUILayout.Toggle("", _togSerialNumberFormat, GUILayout.Width(30));
            EditorGUILayout.LabelField("序号格式化", GUILayout.Width(160));
            if (newTogSerialNumberFormat != _togSerialNumberFormat)
            {
                _togSerialNumberFormat = true;
                _togFindReplace        = false;
                _togAddCharacter       = false;
            }
        }

        /// <summary>
        /// 绘制搜索条件
        /// </summary>
        private void DrawSearchCondition()
        {
            if (_togFindReplace)
            {
                EditorGUILayout.LabelField("查找字符", GUILayout.Width(60));
                _findText = GUILayout.TextField(_findText, GUILayout.Width(100));
                EditorGUILayout.LabelField("            =======>", GUILayout.Width(120));
                EditorGUILayout.LabelField("替换字符", GUILayout.Width(60));
                _replaceText = GUILayout.TextField(_replaceText, GUILayout.Width(100));
            }
            else if (_togAddCharacter)
            {
                #region 左侧复选框

                EditorGUILayout.BeginVertical();
                var newTogFront = EditorGUILayout.Toggle("追加字符最前面", _togFront);
                if (newTogFront != _togFront)
                {
                    _togFront    = true;
                    _togLast     = false;
                    _togIndexPos = false;
                }

                EditorGUILayout.Space(10);
                var newTogLast = EditorGUILayout.Toggle("追加字符最后面", _togLast);
                if (newTogLast != _togLast)
                {
                    _togFront    = false;
                    _togLast     = true;
                    _togIndexPos = false;
                }

                EditorGUILayout.Space(10);
                var newTogIndexPos = EditorGUILayout.Toggle("追加字符指定位置", _togIndexPos);
                if (newTogIndexPos != _togIndexPos)
                {
                    _togFront    = false;
                    _togLast     = false;
                    _togIndexPos = true;
                }

                EditorGUILayout.EndVertical();

                #endregion

                #region 右侧输入框

                EditorGUILayout.BeginVertical();
                EditorGUILayout.LabelField("追加的文本");
                _appendText = GUILayout.TextField(_appendText);
                if (_togIndexPos)
                {
                    EditorGUILayout.LabelField("追加的指定位置");
                    _appendTextPos = GUILayout.TextField(_appendTextPos);
                }

                EditorGUILayout.EndVertical();

                #endregion
            }
            else if (_togSerialNumberFormat)
            {
                EditorGUILayout.BeginVertical();
                EditorGUILayout.LabelField("前面字符", GUILayout.Width(60));
                _smFrontText = GUILayout.TextField(_smFrontText, GUILayout.Width(150));
                EditorGUILayout.EndVertical();

                EditorGUILayout.BeginVertical();
                EditorGUILayout.LabelField("起始序号", GUILayout.Width(60));
                _smStartText = GUILayout.TextField(_smStartText, GUILayout.Width(150));
                if (EditorGUILayout.DropdownButton(new GUIContent(_selectedOption), FocusType.Keyboard,
                        GUILayout.Width(150)))
                {
                    ShowDropdown();
                }

                EditorGUILayout.EndVertical();

                EditorGUILayout.BeginVertical();
                EditorGUILayout.LabelField("后面字符", GUILayout.Width(60));
                _smLastText = GUILayout.TextField(_smLastText, GUILayout.Width(150));
                EditorGUILayout.EndVertical();
            }
        }

        private string _selectedOption = "请选择序号类型";
        private string _smFrontText = string.Empty; // 序号格式化前面字符
        private string _smStartText = string.Empty; // 序号格式化起始字符
        private string _smLastText = string.Empty;  // 序号格式化后面字符

        private void ShowDropdown()
        {
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("整数数字(阿拉伯)"), false, () => SelectOption("整数数字(阿拉伯)"));
            menu.AddItem(new GUIContent("英文序号(小写)"), false, () => SelectOption("英文序号(小写)"));
            menu.AddItem(new GUIContent("英文序号(大写)"), false, () => SelectOption("英文序号(大写)"));
            menu.ShowAsContext();
        }

        private void SelectOption(string option)
        {
            _selectedOption = option;
            Repaint();
        }

        /// <summary>
        /// 查找与替换
        /// </summary>
        private void FindReplace()
        {
            var renameObjects = Selection.gameObjects;
            var index         = 0;
            foreach (var obj in renameObjects)
            {
                var objName = obj.name;
                if (!objName.Contains(_findText)) continue;
                obj.name = objName.Replace(_findText, _replaceText);
                index++;
            }

            _tipContent = $"替换成功，{index}个";
            // EditorUtility.DisplayDialog("命名结束", $" 修改了{index}个", "确定");
        }

        /// <summary>
        /// 追加文本
        /// </summary>
        private void AddCharacter()
        {
            if (string.IsNullOrEmpty(_appendText))
            {
                EditorUtility.DisplayDialog("警告", "请填写要追加的文本内容", "确定");
                return;
            }


            if (_togIndexPos)
            {
                if (string.IsNullOrEmpty(_appendTextPos))
                {
                    EditorUtility.DisplayDialog("警告", "请输入追加文本的位置", "确定");
                    return;
                }

                var isNumeric = Regex.IsMatch(_appendTextPos, @"^\d+$");
                if (!isNumeric)
                {
                    EditorUtility.DisplayDialog("警告", "追加指定位置填写错误(数字)", "确定");
                    return;
                }
            }

            var renameObjects = Selection.gameObjects;
            var index         = 0;
            foreach (var obj in renameObjects)
            {
                if (_togFront)
                {
                    obj.name = _appendText + obj.name; // 加到名称前（文本、序号）
                }
                else if (_togLast)
                {
                    obj.name += _appendText; // 加到名称后（文本、序号）
                }
                else if (_togIndexPos)
                {
                    // 加到指定位置（文本、序号）
                    var objName  = obj.name;
                    var posIndex = int.Parse(_appendTextPos);
                    if (objName.Length < posIndex)
                        continue;
                    objName  = objName.Insert(posIndex, _appendText);
                    obj.name = objName;
                }

                index++;
            }

            _tipContent = $"追加成功，{index}个";
            // EditorUtility.DisplayDialog("命名结束", $" 修改了{index}个", "确定");
        }

        private readonly char[] _letter =
        {
            'a', 'b', 'c', 'd', 'e', 'f', 'g',
            'h', 'i', 'j', 'k', 'l', 'm', 'n',
            'o', 'p', 'q', 'r', 's', 't', 'u',
            'v', 'w', 'x', 'y', 'z'
        };

        /// <summary>
        /// 追加序列
        /// </summary>
        private void AddSerialNumber()
        {
            if (!_togSerialNumberFormat) return;
            if (string.IsNullOrEmpty(_smStartText))
            {
                EditorUtility.DisplayDialog("警告", "请输入的起始序号", "确定");
                return;
            }

            var renameObjects = Selection.gameObjects;
            switch (_selectedOption)
            {
                case "整数数字(阿拉伯)":
                    if (int.TryParse(_smStartText, out var number))
                    {
                        foreach (var obj in renameObjects)
                        {
                            obj.name = $"{_smFrontText}{number}{_smLastText}";
                            number++;
                        }
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("警告", "起始序号应该为整数", "确定");
                    }

                    break;

                case "英文序号(小写)":
                case "英文序号(大写)":
                    var isTip       = false;
                    var currentChar = _smStartText.ToLower();

                    foreach (var unused in currentChar.Where(t => !char.IsLetter(t)))
                    {
                        isTip = true;
                    }

                    if (isTip)
                    {
                        EditorUtility.DisplayDialog("警告", "输入的起始序号必须为字母或由字母组合", "确定");
                        return;
                    }

                    for (var i = 0; i < renameObjects.Length; i++)
                    {
                        var charNumber = currentChar.Select(t => Array.IndexOf(_letter, t)).Where(index => index != -1)
                            .Aggregate("", (current, index) => current + _letter[index + i]);

                        charNumber            = _selectedOption == "英文序号(小写)" ? charNumber.ToLower() : charNumber.ToUpper();
                        renameObjects[i].name = $"{_smFrontText}{charNumber}{_smLastText}";
                    }

                    break;
            }
        }
    }
}