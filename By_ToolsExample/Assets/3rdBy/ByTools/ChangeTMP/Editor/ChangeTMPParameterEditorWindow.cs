namespace _3rdBy.ByTools.ChangeTMP.Editor
{
    using System;
    using System.IO;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using TMPro;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// 批量修改Unity项目中所有预制件（prefabs）中的 TextMeshPro 和 TextMeshProUGUI 组件的参数，并在这些组件的文本内容包含中文时添加指定的脚本。
    /// </summary>
    public class ChangeTMPParameterEditorWindow : EditorWindow
    {
        /// <summary>
        /// TextMeshPro的文本内容包装模式枚举
        /// </summary>
        private enum WrappingModes
        {
            [InspectorName("不换行")] NoWrap = 0,
            [InspectorName("自动换行")] Normal = 1,
            [InspectorName("保持空白符")] PreserveWhitespace = 2,
            [InspectorName("保持空白符且不换行")] PreserveWhitespaceNoWrap = 3
        }

        [Header("窗口大小")] private static readonly Vector2 windowSize = new(800, 600);
        [Header("添加的脚本")] private MonoScript _scriptMonoComponent;

        [Header("字体文件")] private TMP_FontAsset _font;
        [Header("启用自动大小")] private bool _enableAutoSize;

        [Header("启用字体最大最小值")] private bool _enableFontSizeMinMax;
        [Header("设置字体最小值")] private float _fontSizeMin = 12;
        [Header("设置字体最大值")] private float _fontSizeMax = 12;

        [Header("Prefab列表背景样式")] private GUIStyle _prefabTableBgStyle;
        [Header("处理的Prefab集合滚动列表位置")] private Vector2 _prefabTableScrollPos;
        [Header("Prefab列表")] private readonly Dictionary<GameObject, string> _prefabPaths = new(); // key: 预制体对象, value: 预制体路径

        [Header("启用TextMeshPro内容中文检测")] private bool _enableChineseCheck;
        [Header("TextMeshPro的文本内容包装模式")] private WrappingModes _wrappingModes;

        private const string TMP_DESC_STR = "<color=yellow><size=12><b>使用说明：</b></size></color>\n" +
                                          "<color=green>1. 批量修改Project中所有UI预制件（prefabs）中的 TextMeshPro 和 TextMeshProUGUI 组件的参数；</color>\n" +
                                          "<color=green>2. TextMeshPro 和 TextMeshProUGUI 组件的文本内容包含中文时(英文字符不受影响)，添加指定的Mono脚本。</color>\n" +
                                          "<color=green>3. 修改TextMeshPro的字体大小范围、字体自动缩放、文字自动换行。</color>\n" +
                                          "<color=green>4. 只能修改Project中UI预制件下Active为true的TextMeshPro和TextMeshProUGUI组件</color>\n";

        [MenuItem("ByTools/🛠️ TMP参数修改")]
        public static void ShowWindow()
        {
            var window = GetWindow<ChangeTMPParameterEditorWindow>("TMP添加组件");
            window.minSize           = windowSize;
            window.maxSize           = windowSize;
            window.titleContent.text = "TMP参数修改";
        }

        private void OnGUI()
        {
            DrawDesc();
            DragAndDropFiles();
            DrawPrefabList();
            DrawTextMeshProSetting();
            ApplyTextMeshProSettingsToAllPrefabs();
        }

        private static void DrawDesc()
        {
            var helpBoxStyle = new GUIStyle(EditorStyles.helpBox)
            {
                richText = true
            };
            EditorGUILayout.LabelField($"{TMP_DESC_STR}", helpBoxStyle);
        }

        private void DrawTextMeshProSetting()
        {
            GUILayout.BeginHorizontal();
            // 挂载脚本
            EditorGUILayout.LabelField("挂载脚本：", GUILayout.Width(60));
            _scriptMonoComponent = EditorGUILayout.ObjectField(_scriptMonoComponent, typeof(MonoScript), false) as MonoScript;
            GUILayout.Space(10);
            if (_scriptMonoComponent)
            {
                var scriptType = _scriptMonoComponent.GetClass();
                if (!scriptType.IsSubclassOf(typeof(MonoBehaviour)))
                {
                    EditorUtility.DisplayDialog("提示", "脚本类型必须是MonoBehaviour的子类", "确定");
                    _scriptMonoComponent = null;
                }
            }

            // 挂载字体
            EditorGUILayout.LabelField("挂载字体：", GUILayout.Width(60));
            _font = EditorGUILayout.ObjectField(_font, typeof(TMP_FontAsset), false) as TMP_FontAsset;
            GUILayout.Space(10);

            // 参数设置
            EditorGUILayout.LabelField("启用内容包装：", GUILayout.Width(90));
            _wrappingModes = (WrappingModes)EditorGUILayout.EnumPopup(_wrappingModes);
            GUILayout.Space(10);
            EditorGUILayout.LabelField("启用字体自动缩放：", GUILayout.Width(120));
            _enableAutoSize = EditorGUILayout.Toggle(_enableAutoSize, GUILayout.Width(20));
            GUILayout.Space(10);
            EditorGUILayout.LabelField("启用内容中文检测：", GUILayout.Width(120));
            _enableChineseCheck = EditorGUILayout.Toggle(_enableChineseCheck, GUILayout.Width(20));
            GUILayout.Space(10);
            GUILayout.EndHorizontal();

            // 字体大小设置
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("启用字体最大/小值：", GUILayout.Width(130));
            _enableFontSizeMinMax = EditorGUILayout.Toggle(_enableFontSizeMinMax, GUILayout.Width(20));
            if (_enableFontSizeMinMax)
            {
                EditorGUILayout.LabelField("字体最小值：", GUILayout.Width(90));
                _fontSizeMin = EditorGUILayout.FloatField(_fontSizeMin, GUILayout.Width(50));
                EditorGUILayout.LabelField("字体最大值：", GUILayout.Width(90));
                _fontSizeMax = EditorGUILayout.FloatField(_fontSizeMax, GUILayout.Width(50));

                if (_fontSizeMin <= 0 || _fontSizeMax <= 0)
                {
                    EditorUtility.DisplayDialog("提示", "字体最小值和最大值必须大于0", "确定");
                }
            }

            GUILayout.EndHorizontal();
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
            GUI.Box(rect, "拖拽设置的TextMeshPro&TextMeshProUGUI预制物(Prefab)到此处", label);
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
                        if (ex is ".prefab")
                        {
                            var pb = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                            if (pb) _prefabPaths.TryAdd(pb, path);
                        }
                    }
                }

                evt.Use();
            }
        }

        private void DrawPrefabList()
        {
            if (_prefabPaths.Count == 0) return;

            _prefabTableScrollPos = GUILayout.BeginScrollView(_prefabTableScrollPos, false, false, GUIStyle.none, GUI.skin.verticalScrollbar);
            var index = 0;
            foreach (var prefab in _prefabPaths)
            {
                _prefabTableBgStyle ??= new GUIStyle(EditorStyles.helpBox)
                {
                    padding = new RectOffset(6, 6, 6, 6)
                };
                GUILayout.BeginHorizontal(_prefabTableBgStyle);
                GUILayout.Label($"{index}.", EditorStyles.boldLabel, GUILayout.Width(20));
                EditorGUILayout.ObjectField(prefab.Key, typeof(GameObject), false);
                if (GUILayout.Button("移除", GUILayout.Width(60)))
                {
                    if (_prefabPaths.ContainsKey(prefab.Key))
                    {
                        _prefabPaths.Remove(prefab.Key);

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

        private void ApplyTextMeshProSettingsToAllPrefabs()
        {
            if (_prefabPaths.Count == 0)
            {
                EditorGUILayout.HelpBox("请先选择Prefab", MessageType.Info);
                return;
            }

            if (!GUILayout.Button("一键设置TextMeshPro参数")) return;

            int totalPrefabs   = _prefabPaths.Count;
            int processedCount = 0;
            int modifiedCount  = 0;
            try
            {
                // 显示进度条
                EditorUtility.DisplayProgressBar("处理中", "正在设置TextMeshPro参数...", 0);
                foreach (var (prefab, prefabPath) in _prefabPaths)
                {
                    // 更新进度
                    float progress = (float)processedCount / totalPrefabs;
                    EditorUtility.DisplayProgressBar("处理中", $"正在处理: {Path.GetFileName(prefabPath)} ({processedCount + 1}/{totalPrefabs})", progress);

                    try
                    {
                        bool wasModified = ApplyTMPToPrefab(prefab, prefabPath);
                        if (wasModified) modifiedCount++;
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"处理Prefab失败 {prefabPath}: {ex.Message}");
                    }

                    processedCount++;
                }

                // 保存所有修改
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                // 显示结果
                // string message = $"处理完成!\n" +
                //                  $"总Prefab数: {totalPrefabs}\n" +
                //                  $"成功处理: {modifiedCount}";
                //
                // EditorUtility.DisplayDialog("完成", message, "确定");
                Debug.Log($"TMP参数处理完成! 总Prefab数: {totalPrefabs}, 成功处理: {modifiedCount}");
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        private bool ApplyTMPToPrefab(GameObject prefab, string prefabPath)
        {
            bool wasModified = false;
            return ApplyTMPToPrefabUsingLoadContents(prefabPath, ref wasModified);
        }

        // 推荐的方法：使用LoadPrefabContents
        private bool ApplyTMPToPrefabUsingLoadContents(string prefabPath, ref bool wasModified)
        {
            // 加载Prefab内容到临时场景
            var prefabInstance = PrefabUtility.LoadPrefabContents(prefabPath);

            if (prefabInstance == null)
            {
                Debug.LogError($"无法加载Prefab: {prefabPath}");
                return false;
            }

            try
            {
                // 查找所有TextMeshPro组件
                var tmp3DList = prefabInstance.GetComponentsInChildren<TextMeshPro>(true);
                var tmpUIList = prefabInstance.GetComponentsInChildren<TextMeshProUGUI>(true);

                // 应用设置
                foreach (var tmp in tmp3DList)
                {
                    if (ApplyTMPParameter(tmp))
                    {
                        wasModified = true;
                        EditorUtility.SetDirty(tmp);
                    }
                }

                foreach (var tmpUgui in tmpUIList)
                {
                    if (ApplyTMPParameter(tmpUgui))
                    {
                        wasModified = true;
                        EditorUtility.SetDirty(tmpUgui);
                    }
                }

                // 如果有修改，保存Prefab
                if (wasModified)
                {
                    // 记录Undo（如果需要）
                    Undo.RegisterCompleteObjectUndo(prefabInstance, "设置TMP参数");

                    // 保存修改回原Prefab
                    PrefabUtility.SaveAsPrefabAsset(prefabInstance, prefabPath);

                    Debug.Log($"已保存Prefab修改: {Path.GetFileName(prefabPath)}");
                    return true;
                }

                return false;
            }
            finally
            {
                // 卸载Prefab内容
                PrefabUtility.UnloadPrefabContents(prefabInstance);
            }
        }

        private bool ApplyTMPParameter<T>(T component) where T : TMP_Text
        {
            if (component == null) return false;
            bool changed = _font ||
                           component.enableAutoSizing != _enableAutoSize ||
                           component.textWrappingMode != (TextWrappingModes)_wrappingModes ||
                           _enableFontSizeMinMax && (_fontSizeMin > 0 || _fontSizeMax > 0) &&
                           (!Mathf.Approximately(component.fontSizeMin, _fontSizeMin) || !Mathf.Approximately(component.fontSizeMax, _fontSizeMax));

            // 启用文本内容中文检测
            if (_enableChineseCheck)
            {
                if (!ContainsChinese(component.text))
                {
                    Debug.Log("TextMeshPro 组件的文本内容不包含中文字符，不处理");
                    return false; // 内容中没有中文，不处理
                }
            }

            AddScriptToGameObject(component.gameObject); // 添加脚本
            // 设置TextMeshPro相关参数
            component.font             = _font ? _font : component.font;
            component.enableAutoSizing = _enableAutoSize;                                          // 字体自动缩放
            component.textWrappingMode = (TextWrappingModes)_wrappingModes;                        // 自动换行
            component.fontSizeMin      = _fontSizeMin >= 0 ? _fontSizeMin : component.fontSizeMin; // 设置字体最小值
            component.fontSizeMax      = _fontSizeMax >= 0 ? _fontSizeMax : component.fontSize;    // 设置字体最大值
            Debug.Log(component.gameObject.name + " TextMeshPro参数设置完成");
            // component.ForceMeshUpdate();
            return changed;
        }

        private void AddScriptToGameObject(GameObject go)
        {
            if (!_scriptMonoComponent) return; // 未选择添加的脚本
            var scriptType = _scriptMonoComponent.GetClass();
            if (scriptType == null) return;          // 脚本类型为空
            if (go.GetComponent(scriptType)) return; // 对象已经包含此脚本
            go.AddComponent(scriptType);
        }

        /// <summary>
        /// 是否包含中文检测
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private static bool ContainsChinese(string text)
        {
            return Regex.IsMatch(text, @"[\u4e00-\u9fa5]");
        }
    }
}