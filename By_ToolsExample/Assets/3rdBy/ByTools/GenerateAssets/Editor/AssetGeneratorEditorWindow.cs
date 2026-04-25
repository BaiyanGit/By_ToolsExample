namespace _3rdBy.ByTools.GenerateAssets.Editor
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Assets资源脚本生成器
    /// </summary>
    public class AssetGeneratorEditorWindow : EditorWindow
    {
        #region 列表数据持久化

        [Serializable]
        public class AssetPlanInfo
        {
            [Header("资产唯一标识")] public string key;
            [Header("资产名称")] public string name;
            [Header("是否为预览资产")] public bool isPreviewAsset;
            [Header("编辑器草稿Json")] public string editorJson;
        }

        [Serializable]
        public class ScriptableObjectInfo
        {
            [Header("脚本名称（基础名）")] public string name;
            [Header("脚本路径")] public string path;
            [Header("待生成资产列表")] public List<AssetPlanInfo> assetPlans = new();
            [Header("已生成资产路径")] public List<string> outputAssetPaths = new();
        }

        [Serializable]
        private class SoDictWrapper
        {
            [Header("文件序号")] public List<int> keys = new();
            [Header("文件信息")] public List<ScriptableObjectInfo> values = new();
        }

        private void OnEnable()
        {
            LoadSessionData();
        }

        private void OnDisable()
        {
            UpdateSessionData();
        }

        /// <summary>
        /// 加载持久化数据
        /// </summary>
        private void LoadSessionData()
        {
            _soDict = new Dictionary<int, ScriptableObjectInfo>();

            // 文件列表记录
            var fileList = SessionState.GetString(SESSION_KEY_ASSETS, string.Empty);
            if (!string.IsNullOrEmpty(fileList))
            {
                var wrapper = JsonUtility.FromJson<SoDictWrapper>(fileList);
                if (wrapper != null)
                {
                    for (int i = 0; i < wrapper.keys.Count && i < wrapper.values.Count; i++)
                    {
                        var soInfo = wrapper.values[i] ?? new ScriptableObjectInfo();
                        EnsureAssetPlanData(soInfo);
                        _soDict.TryAdd(wrapper.keys[i], soInfo);
                    }
                }
            }

            // 输出目录记录
            var outputFolder = SessionState.GetString(OUTPUT_FOLDER_KEY, string.Empty);
            if (!string.IsNullOrEmpty(outputFolder))
            {
                _outputFolder = outputFolder;
                return;
            }

            // 记录输出目录为空，尝试获取默认路径
            var monoScript     = MonoScript.FromScriptableObject(this);
            var scriptFullPath = AssetDatabase.GetAssetPath(monoScript);
            _outputFolder = GetUpperDirectory(scriptFullPath, 2, "AssetFiles");
            SessionState.SetString(OUTPUT_FOLDER_KEY, _outputFolder);
        }

        /// <summary>
        /// 更新持久化数据
        /// </summary>
        private void UpdateSessionData()
        {
            // 列表无数据，则删除Session数据
            if (_soDict == null || _soDict.Count == 0)
            {
                SessionState.EraseString(SESSION_KEY_ASSETS);
                return;
            }

            // 列表有数据，则更新Session数据
            var wrapper = new SoDictWrapper();
            foreach (var (key, value) in _soDict.OrderBy(item => item.Key))
            {
                wrapper.keys.Add(key);
                wrapper.values.Add(value);
            }

            var json = JsonUtility.ToJson(wrapper);
            SessionState.SetString(SESSION_KEY_ASSETS, json);
        }

        #endregion

        [Header("说明")] private const string DESC = "生成.Asset配置文件工具，支持批量生成 ScriptableObject 资产。(支持所有的Mono脚本)";
        [Header("输出目录会话Key")] private const string OUTPUT_FOLDER_KEY = "OUTPUT_FOLDER_KEY";
        [Header("列表会话数据Key")] private const string SESSION_KEY_ASSETS = "SESSION_KEY_ASSETS";
        [Header("默认资产Key")] private const string DEFAULT_ASSET_KEY = "__default_asset_key__";
        [Header("输出目录")] private static string _outputFolder = "";
        [Header("列表滚动位置")] private Vector2 _scrollPos;
        [Header("文件列表背景样式")] private GUIStyle _listFileBgStyle;

        [Header("小标题样式")] private GUIStyle _sectionTitleStyle;

        // [Header("待生成资产项样式")] private GUIStyle _assetPlanItemStyle;
        private Dictionary<int, ScriptableObjectInfo> _soDict = new();
        [Header("已生成资产折叠状态")] private readonly Dictionary<int, bool> _generatedFoldoutStates = new();
        [Header("待生成资产折叠状态")] private readonly Dictionary<int, bool> _planFoldoutStates = new();
        [Header("待生成资产项折叠状态")] private readonly Dictionary<string, bool> _assetPlanItemFoldoutStates = new();

        /// <summary>
        /// 资产覆盖模式
        /// </summary>
        private enum AssetOverwriteMode
        {
            Cancel = 0,
            OverwriteAndCarryData = 1,
            OverwriteOnly = 2
        }

        [MenuItem("ByTools/🗂️ Asset资产生成工具")]
        public static void Open()
        {
            GetWindow<AssetGeneratorEditorWindow>("Asset资产生成工具");
        }

        private void OnGUI()
        {
            EditorGUILayout.HelpBox(DESC, MessageType.Info);

            EditorGUILayout.Space();
            DrawOutputPath();
            EditorGUILayout.Space();
            DrawToolbar();
            EditorGUILayout.Space();
            DrawDropArea();
            EditorGUILayout.Space();
            DrawFileList();
            EditorGUILayout.Space();
        }

        /// <summary>
        /// 绘制拖拽区域
        /// </summary>
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
                    string outputTip = "";
                    foreach (var obj in DragAndDrop.objectReferences)
                    {
                        if (obj is MonoScript monoScript)
                        {
                            var type = monoScript.GetClass();
                            if (type != null && type.IsSubclassOf(typeof(ScriptableObject)))
                            {
                                if (IsDuplicate(monoScript.name))
                                {
                                    // 处理同名文件
                                    AssetsInputDialogWindow.ShowDialog(monoScript.name, _soDict, currentInput =>
                                    {
                                        var soInfo = CreateScriptableObjectInfo(monoScript, currentInput);
                                        AddScriptableObject(soInfo);
                                    });
                                }
                                else
                                {
                                    var soInfo = CreateScriptableObjectInfo(monoScript, monoScript.name);
                                    AddScriptableObject(soInfo);
                                }
                            }
                            else
                            {
                                if (type != null)
                                {
                                    outputTip = $"生成.asset失败\n {type.Name}.cs 未继承 ScriptableObject";
                                }
                            }
                        }
                        else
                        {
                            outputTip = "只支持继承 ScriptableObject 的 Mono脚本";
                        }
                    }

                    if (!string.IsNullOrEmpty(outputTip))
                    {
                        EditorUtility.DisplayDialog("提示", outputTip, "确定");
                    }
                }

                evt.Use();
            }
        }

        /// <summary>
        /// 绘制文件列表
        /// </summary>
        private void DrawFileList()
        {
            _listFileBgStyle ??= new GUIStyle(EditorStyles.helpBox)
            {
                padding = new RectOffset(10, 10, 10, 10)
            };

            _sectionTitleStyle ??= new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 12,
                normal   = { textColor = Color.white }
            };

            EditorGUILayout.LabelField("文件列表", EditorStyles.boldLabel);
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

            _soDict ??= new Dictionary<int, ScriptableObjectInfo>();
            if (_soDict.Count == 0)
            {
                EditorGUILayout.HelpBox("暂无脚本，请拖拽 ScriptableObject 脚本到上方区域。", MessageType.None);
                EditorGUILayout.EndScrollView();
                return;
            }

            int removeKey   = -1;
            var orderedList = _soDict.OrderBy(item => item.Key).ToList();
            for (int index = 0; index < orderedList.Count; index++)
            {
                var item = orderedList[index];
                var key  = item.Key;
                var info = item.Value;
                EnsureAssetPlanData(info);

                EditorGUILayout.BeginVertical(_listFileBgStyle);
                DrawScriptHeader(index, key, info, ref removeKey);
                EditorGUILayout.Space(4);
                DrawPlannedAssetList(key, info, ref removeKey);
                EditorGUILayout.Space(4);
                DrawGeneratedAssetList(key, info);
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(6);
            }

            if (removeKey >= 0 && _soDict.ContainsKey(removeKey))
            {
                _soDict.Remove(removeKey);
            }

            EditorGUILayout.EndScrollView();
        }

        /// <summary>
        /// 绘制脚本头部区域
        /// </summary>
        private void DrawScriptHeader(int index, int key, ScriptableObjectInfo info, ref int removeKey)
        {
            EditorGUILayout.LabelField($"📑 {index + 1}. {info.name}", _sectionTitleStyle);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"引用脚本：", GUILayout.Width(60));
            using (new EditorGUI.DisabledScope(true))
            {
                var scriptObj = AssetDatabase.LoadAssetAtPath<MonoScript>(info.path);
                EditorGUILayout.ObjectField(scriptObj, typeof(MonoScript), false);
            }

            if (GUILayout.Button("添加资产📩", GUILayout.Width(80)))
            {
                AddManualAssetPlan(info);
            }

            if (GUILayout.Button("移除脚本🪠", GUILayout.Width(80)))
            {
                removeKey = key;
            }

            if (GUILayout.Button("一键生成🔨", GUILayout.Width(80)))
            {
                GenerateAssets(info);
            }

            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// 绘制待生成资产列表
        /// </summary>
        private void DrawPlannedAssetList(int scriptKey, ScriptableObjectInfo info, ref int removeKey)
        {
            info.assetPlans ??= new List<AssetPlanInfo>();
            if (info.assetPlans.Count == 0)
            {
                EditorGUILayout.HelpBox("当前脚本暂无待生成资产。", MessageType.None);
                return;
            }

            // bool needFoldout = info.assetPlans.Count > 2;
            bool isExpanded  = true;
            // if (needFoldout)
            {
                _planFoldoutStates.TryAdd(scriptKey, false);
                _planFoldoutStates[scriptKey] = EditorGUILayout.Foldout(_planFoldoutStates[scriptKey], $"🔍 预览{info.assetPlans.Count} 个资产名", true);
                isExpanded                    = _planFoldoutStates[scriptKey];
            }

            if (isExpanded == false) return;

            int removeIndex = -1;
            for (int i = 0; i < info.assetPlans.Count; i++)
            {
                var planInfo = info.assetPlans[i];
                if (planInfo == null) continue;
                string plannedAssetPath  = GetPlannedAssetPath(_outputFolder, planInfo.name);
                bool   hasGeneratedAsset = IsAssetAlreadyGenerated(plannedAssetPath);
                string itemFoldKey       = GetAssetPlanFoldoutKey(scriptKey, planInfo);
                _assetPlanItemFoldoutStates.TryAdd(itemFoldKey, false);

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.BeginHorizontal();
                var state = hasGeneratedAsset ? "🟢 资产已生成" : "🔘 资产待生成";
                var rect  = GUILayoutUtility.GetRect(10, EditorGUIUtility.singleLineHeight);
                _assetPlanItemFoldoutStates[itemFoldKey] = EditorGUI.Foldout(rect, _assetPlanItemFoldoutStates[itemFoldKey], state, true);

                using (new EditorGUI.DisabledScope(true))
                {
                    EditorGUILayout.TextField($"{SanitizeAssetName(planInfo.name)}.asset");
                }

                if (GUILayout.Button("重命名✏️", GUILayout.Width(90)))
                {
                    var tempDict = BuildAssetNameDict(info.assetPlans, i);
                    var iIndex   = i;
                    AssetsInputDialogWindow.ShowDialog(planInfo.name, tempDict, currentInput => { info.assetPlans[iIndex].name = SanitizeAssetName(currentInput); });
                }

                if (GUILayout.Button("编辑📝", GUILayout.Width(80)))
                {
                    OpenPlanAssetEditor(info, planInfo);
                }

                if (GUILayout.Button("移除🧹", GUILayout.Width(80)))
                {
                    removeIndex = i;
                }

                if (GUILayout.Button("生成🥏", GUILayout.Width(80)))
                {
                    GenerateSingleAsset(info, planInfo);
                }

                EditorGUILayout.EndHorizontal();

                if (_assetPlanItemFoldoutStates[itemFoldKey])
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(24);
                    EditorGUILayout.BeginVertical();
                    EditorGUILayout.LabelField($"状态：{(hasGeneratedAsset ? "当前路径下已存在同名资产" : "当前路径下未生成同名资产")}");
                    using (new EditorGUI.DisabledScope(true))
                    {
                        EditorGUILayout.LabelField("");
                        EditorGUILayout.LabelField("");
                        EditorGUILayout.TextField("计划路径", plannedAssetPath);
                        EditorGUILayout.TextField("生成方式", planInfo.isPreviewAsset ? "自定义拆分预览资产" : "普通单资产/手动新增资产");
                    }

                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.EndVertical();
            }

            if (removeIndex >= 0)
            {
                info.assetPlans.RemoveAt(removeIndex);
                if (info.assetPlans.Count == 0)
                {
                    removeKey = scriptKey;
                }
            }
        }

        /// <summary>
        /// 绘制已生成资产列表
        /// </summary>
        private void DrawGeneratedAssetList(int key, ScriptableObjectInfo info)
        {
            CleanupOutputAssetPaths(info);
            info.outputAssetPaths ??= new List<string>();
            _generatedFoldoutStates.TryAdd(key, false);

            var generatedCount = info.outputAssetPaths.Count;
            _generatedFoldoutStates[key] = EditorGUILayout.Foldout(_generatedFoldoutStates[key], $"🧩 已生成 {generatedCount} 个资产", true);

            if (_generatedFoldoutStates[key] == false) return;

            if (generatedCount == 0)
            {
                EditorGUILayout.HelpBox("暂无已生成资产。", MessageType.None);
                return;
            }

            EditorGUI.indentLevel++;
            foreach (var assetPath in info.outputAssetPaths)
            {
                EditorGUILayout.BeginHorizontal();
                using (new EditorGUI.DisabledScope(true))
                {
                    var assetObj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath);
                    if (assetObj != null)
                    {
                        EditorGUILayout.ObjectField(assetObj, typeof(UnityEngine.Object), false);
                    }
                    else
                    {
                        EditorGUILayout.TextField(assetPath);
                    }
                }

                if (GUILayout.Button("编辑📝", GUILayout.Width(80)))
                {
                    OpenGeneratedAssetEditor(assetPath);
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUI.indentLevel--;
        }

        /// <summary>
        /// 生成当前脚本下的全部资产
        /// </summary>
        private void GenerateAssets(ScriptableObjectInfo info)
        {
            if (TryGetGenerateContext(info, out var classType, out var outputFolder) == false)
            {
                return;
            }

            foreach (var assetPlan in info.assetPlans.ToList())
            {
                GenerateSingleAssetInternal(info, assetPlan, classType, outputFolder);
            }

            CleanupOutputAssetPaths(info);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// 单独生成一个资产
        /// </summary>
        /// <param name="info"></param>
        /// <param name="planInfo"></param>
        private void GenerateSingleAsset(ScriptableObjectInfo info, AssetPlanInfo planInfo)
        {
            if (TryGetGenerateContext(info, out var classType, out var outputFolder) == false)
            {
                return;
            }

            GenerateSingleAssetInternal(info, planInfo, classType, outputFolder);
            CleanupOutputAssetPaths(info);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// 单独生成一个资产（内部实现）
        /// </summary>
        /// <param name="info"></param>
        /// <param name="planInfo"></param>
        /// <param name="classType"></param>
        /// <param name="outputFolder"></param>
        private void GenerateSingleAssetInternal(ScriptableObjectInfo info, AssetPlanInfo planInfo, Type classType, string outputFolder)
        {
            if (info == null || planInfo == null || classType == null || string.IsNullOrEmpty(outputFolder)) return;

            if (PrepareAssetForGeneration(planInfo, outputFolder, out var oldAssetJson) == false)
            {
                return;
            }

            var  generatedPaths     = new List<string>();
            bool useCustomGenerator = ShouldUseCustomGenerator(info, planInfo, classType);
            if (useCustomGenerator)
            {
                generatedPaths = CustomAssetGenerator.TryGenerate(classType, info.path, outputFolder, ConvertToCustomPlans(new List<AssetPlanInfo> { planInfo }));
            }

            if (generatedPaths == null || generatedPaths.Count == 0)
            {
                var defaultPath = CreateDefaultAsset(classType, planInfo, outputFolder, oldAssetJson);
                if (!string.IsNullOrEmpty(defaultPath))
                {
                    generatedPaths?.Add(defaultPath);
                }
            }
            else
            {
                foreach (var generatedPath in generatedPaths)
                {
                    ApplyJsonToGeneratedAsset(generatedPath, oldAssetJson);
                }

                ApplyDraftJsonToGeneratedAssets(new List<AssetPlanInfo> { planInfo }, generatedPaths);
            }

            if (generatedPaths != null)
            {
                foreach (var generatedPath in generatedPaths)
                {
                    RegisterGeneratedAssetPath(info, generatedPath);
                }
            }
        }

        /// <summary>
        /// 创建默认资产
        /// </summary>
        /// <param name="classType"></param>
        /// <param name="planInfo"></param>
        /// <param name="outputFolder"></param>
        /// <param name="oldAssetJson"></param>
        /// <returns></returns>
        private string CreateDefaultAsset(Type classType, AssetPlanInfo planInfo, string outputFolder, string oldAssetJson)
        {
            var assetName = SanitizeAssetName(planInfo.name);
            var assetPath = GetPlannedAssetPath(outputFolder, assetName);
            var asset     = CreateInstance(classType);
            AssetDatabase.CreateAsset(asset, assetPath);
            ApplyJsonToGeneratedAsset(assetPath, oldAssetJson);
            ApplyDraftJsonToAsset(asset, planInfo.editorJson);
            Debug.Log($"[ <color=green>🗂️Assets</color> ] {assetName}.asset Path: {assetPath}");
            return assetPath;
        }

        /// <summary>
        /// 绘制输出路径
        /// </summary>
        private void DrawOutputPath()
        {
            EditorGUILayout.LabelField("输出路径", EditorStyles.boldLabel);
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
                        _outputFolder = $"Assets{path[Application.dataPath.Length..]}";
                        SessionState.SetString(OUTPUT_FOLDER_KEY, _outputFolder);
                    }
                }
            }

            if (GUILayout.Button("重置", GUILayout.Width(60)))
            {
                var monoScript     = MonoScript.FromScriptableObject(this);
                var scriptFullPath = AssetDatabase.GetAssetPath(monoScript);
                _outputFolder = GetUpperDirectory(scriptFullPath, 2, "AssetFiles");
                SessionState.SetString(OUTPUT_FOLDER_KEY, _outputFolder);
            }

            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// 绘制工具栏
        /// </summary>
        private void DrawToolbar()
        {
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("清除列表", GUILayout.Height(30)))
            {
                _soDict.Clear();
            }

            if (GUILayout.Button("批量生成", GUILayout.Height(30)))
            {
                foreach (var (_, value) in _soDict.OrderBy(item => item.Key))
                {
                    GenerateAssets(value);
                }
            }

            GUILayout.EndHorizontal();
        }

        #region 辅助

        /// <summary>
        /// 重复文件检查（基础名）
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private bool IsDuplicate(string fileName)
        {
            _soDict ??= new Dictionary<int, ScriptableObjectInfo>();
            foreach (var (_, value) in _soDict)
            {
                var soName = value.name;
                if (soName.Equals(fileName, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 创建脚本信息
        /// </summary>
        /// <param name="monoScript"></param>
        /// <param name="baseAssetName"></param>
        /// <returns></returns>
        private ScriptableObjectInfo CreateScriptableObjectInfo(MonoScript monoScript, string baseAssetName)
        {
            var soInfo = new ScriptableObjectInfo
            {
                name             = SanitizeAssetName(baseAssetName),
                path             = AssetDatabase.GetAssetPath(monoScript),
                assetPlans       = new List<AssetPlanInfo>(),
                outputAssetPaths = new List<string>()
            };

            EnsureAssetPlanData(soInfo);
            return soInfo;
        }

        /// <summary>
        /// 添加手动资产计划
        /// </summary>
        /// <param name="soInfo"></param>
        private void AddManualAssetPlan(ScriptableObjectInfo soInfo)
        {
            if (soInfo == null) return;
            EnsureAssetPlanData(soInfo);
            soInfo.assetPlans ??= new List<AssetPlanInfo>();
            soInfo.assetPlans.Add(new AssetPlanInfo
            {
                key            = Guid.NewGuid().ToString("N"),
                name           = GenerateNextManualAssetName(soInfo.assetPlans, soInfo.name),
                isPreviewAsset = false
            });
        }

        /// <summary>
        /// 确保资产计划数据完整
        /// </summary>
        /// <param name="soInfo"></param>
        private void EnsureAssetPlanData(ScriptableObjectInfo soInfo)
        {
            if (soInfo == null) return;

            soInfo.assetPlans       ??= new List<AssetPlanInfo>();
            soInfo.outputAssetPaths ??= new List<string>();
            soInfo.name             =   SanitizeAssetName(soInfo.name);

            var entry         = AssetDatabase.LoadAssetAtPath<MonoScript>(soInfo.path);
            var classType     = entry != null ? entry.GetClass() : null;
            var previewKeySet = classType != null ? GetPreviewPlanKeySet(classType, soInfo.path, soInfo.name) : new HashSet<string>();

            if (soInfo.assetPlans.Count > 0)
            {
                foreach (var assetPlan in soInfo.assetPlans)
                {
                    if (assetPlan == null) continue;
                    assetPlan.name = SanitizeAssetName(assetPlan.name);
                    if (string.IsNullOrEmpty(assetPlan.key))
                    {
                        assetPlan.key = Guid.NewGuid().ToString("N");
                    }

                    assetPlan.isPreviewAsset = previewKeySet.Contains(assetPlan.key);
                }

                return;
            }

            if (classType == null)
            {
                soInfo.assetPlans.Add(new AssetPlanInfo
                {
                    key            = DEFAULT_ASSET_KEY,
                    name           = SanitizeAssetName(soInfo.name),
                    isPreviewAsset = false
                });
                return;
            }

            var previewPlans = CustomAssetGenerator.TryPreview(classType, soInfo.path, SanitizeAssetName(soInfo.name));
            if (previewPlans is { Count: > 0 })
            {
                soInfo.assetPlans = previewPlans
                    .Where(item => item != null && !string.IsNullOrEmpty(item.name))
                    .Select(item => new AssetPlanInfo
                    {
                        key            = string.IsNullOrEmpty(item.key) ? Guid.NewGuid().ToString("N") : item.key,
                        name           = SanitizeAssetName(item.name),
                        isPreviewAsset = true
                    })
                    .ToList();

                if (soInfo.assetPlans.Count > 0) return;
            }

            soInfo.assetPlans.Add(new AssetPlanInfo
            {
                key            = DEFAULT_ASSET_KEY,
                name           = SanitizeAssetName(soInfo.name),
                isPreviewAsset = false
            });
        }

        /// <summary>
        /// 添加脚本对象
        /// </summary>
        /// <param name="soInfo"></param>
        private void AddScriptableObject(ScriptableObjectInfo soInfo)
        {
            _soDict ??= new Dictionary<int, ScriptableObjectInfo>();
            int nextKey = GetNextKey();
            _soDict[nextKey] = soInfo;
        }

        /// <summary>
        /// 获取下一个可用Key
        /// </summary>
        /// <returns></returns>
        private int GetNextKey()
        {
            if (_soDict == null || _soDict.Count == 0) return 0;
            return _soDict.Keys.Max() + 1;
        }

        /// <summary>
        /// 生成临时重命名字典
        /// </summary>
        /// <param name="assetPlans"></param>
        /// <param name="currentIndex"></param>
        /// <returns></returns>
        private Dictionary<int, ScriptableObjectInfo> BuildAssetNameDict(List<AssetPlanInfo> assetPlans, int currentIndex)
        {
            var tempDict = new Dictionary<int, ScriptableObjectInfo>();
            if (assetPlans == null) return tempDict;

            int tempKey = 0;
            for (int i = 0; i < assetPlans.Count; i++)
            {
                if (i == currentIndex) continue;
                tempDict.Add(tempKey, new ScriptableObjectInfo { name = assetPlans[i].name });
                tempKey++;
            }

            return tempDict;
        }

        /// <summary>
        /// 获取预览资产Key集合
        /// </summary>
        /// <param name="classType"></param>
        /// <param name="scriptPath"></param>
        /// <param name="baseAssetName"></param>
        /// <returns></returns>
        private HashSet<string> GetPreviewPlanKeySet(Type classType, string scriptPath, string baseAssetName)
        {
            var result       = new HashSet<string>();
            var previewPlans = CustomAssetGenerator.TryPreview(classType, scriptPath, SanitizeAssetName(baseAssetName));
            if (previewPlans == null) return result;

            foreach (var previewPlan in previewPlans)
            {
                if (previewPlan == null || string.IsNullOrEmpty(previewPlan.key)) continue;
                result.Add(previewPlan.key);
            }

            return result;
        }

        /// <summary>
        /// 生成下一个手动资产名称
        /// </summary>
        /// <param name="assetPlans"></param>
        /// <param name="baseName"></param>
        /// <returns></returns>
        private string GenerateNextManualAssetName(List<AssetPlanInfo> assetPlans, string baseName)
        {
            var usedNames = new HashSet<string>();
            if (assetPlans != null)
            {
                foreach (var assetPlan in assetPlans)
                {
                    if (assetPlan == null || string.IsNullOrEmpty(assetPlan.name)) continue;
                    usedNames.Add(SanitizeAssetName(assetPlan.name));
                }
            }

            baseName = SanitizeAssetName(baseName);
            if (!usedNames.Contains(baseName)) return baseName;

            int index = 1;
            while (true)
            {
                var candidateName = $"{baseName}_{index}";
                if (!usedNames.Contains(candidateName)) return candidateName;
                index++;
            }
        }

        /// <summary>
        /// 转换为自定义生成器计划数据
        /// </summary>
        /// <param name="assetPlans"></param>
        /// <returns></returns>
        private List<CustomAssetPlanInfo> ConvertToCustomPlans(List<AssetPlanInfo> assetPlans)
        {
            var result = new List<CustomAssetPlanInfo>();
            if (assetPlans == null) return result;

            foreach (var assetPlan in assetPlans)
            {
                if (assetPlan == null || string.IsNullOrEmpty(assetPlan.name)) continue;
                result.Add(new CustomAssetPlanInfo
                {
                    key  = assetPlan.key,
                    name = SanitizeAssetName(assetPlan.name)
                });
            }

            return result;
        }


        /// <summary>
        /// 获取生成上下文
        /// </summary>
        /// <param name="info"></param>
        /// <param name="classType"></param>
        /// <param name="outputFolder"></param>
        /// <returns></returns>
        private bool TryGetGenerateContext(ScriptableObjectInfo info, out Type classType, out string outputFolder)
        {
            classType    = null;
            outputFolder = string.Empty;
            EnsureAssetPlanData(info);
            if (info == null || info.assetPlans == null || info.assetPlans.Count == 0)
            {
                EditorUtility.DisplayDialog("提示", "当前没有可生成的资产。", "确定");
                return false;
            }

            var entry = AssetDatabase.LoadAssetAtPath<MonoScript>(info.path);
            if (entry == null)
            {
                EditorUtility.DisplayDialog("❌ 生成失败", "脚本文件不存在或已丢失。", "OK");
                return false;
            }

            classType = entry.GetClass();
            if (classType == null)
            {
                EditorUtility.DisplayDialog("❌ 生成失败", "脚本类型为空", "OK");
                return false;
            }

            outputFolder = SessionState.GetString(OUTPUT_FOLDER_KEY, string.Empty);
            if (string.IsNullOrEmpty(outputFolder))
            {
                EditorUtility.DisplayDialog("❌ 生成失败", "输出目录为空", "OK");
                return false;
            }

            if (!Directory.Exists(outputFolder))
            {
                Directory.CreateDirectory(outputFolder);
            }

            return true;
        }

        /// <summary>
        /// 是否使用自定义生成器
        /// </summary>
        /// <param name="info"></param>
        /// <param name="planInfo"></param>
        /// <param name="classType"></param>
        /// <returns></returns>
        private bool ShouldUseCustomGenerator(ScriptableObjectInfo info, AssetPlanInfo planInfo, Type classType)
        {
            if (info == null || planInfo == null || classType == null) return false;
            if (planInfo.isPreviewAsset == false) return false;
            var previewKeys = GetPreviewPlanKeySet(classType, info.path, info.name);
            return previewKeys.Contains(planInfo.key);
        }

        /// <summary>
        /// 准备资产生成前的覆盖逻辑
        /// </summary>
        /// <param name="planInfo"></param>
        /// <param name="outputFolder"></param>
        /// <param name="oldAssetJson"></param>
        /// <returns></returns>
        private bool PrepareAssetForGeneration(AssetPlanInfo planInfo, string outputFolder, out string oldAssetJson)
        {
            oldAssetJson = string.Empty;
            if (planInfo == null) return false;

            var assetPath     = GetPlannedAssetPath(outputFolder, planInfo.name);
            var existingAsset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath);
            if (existingAsset == null)
            {
                return true;
            }

            var overwriteMode = ShowAssetOverwriteDialog(assetPath);
            switch (overwriteMode)
            {
                case AssetOverwriteMode.Cancel:
                    return false;
                case AssetOverwriteMode.OverwriteAndCarryData:
                    oldAssetJson = EditorJsonUtility.ToJson(existingAsset, true);
                    break;
                case AssetOverwriteMode.OverwriteOnly:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            AssetDatabase.DeleteAsset(assetPath);
            return true;
        }

        /// <summary>
        /// 显示资产覆盖弹窗
        /// </summary>
        /// <param name="assetPath"></param>
        /// <returns></returns>
        private AssetOverwriteMode ShowAssetOverwriteDialog(string assetPath)
        {
            string message     = "当前路径下已存在同名资产：\n" + assetPath + "\n\n请选择覆盖方式。";
            int    selectIndex = EditorUtility.DisplayDialogComplex("检测到同名资产", message, "覆盖并带入数据", "取消", "仅覆盖");

            return selectIndex switch
            {
                0 => AssetOverwriteMode.OverwriteAndCarryData,
                2 => AssetOverwriteMode.OverwriteOnly,
                _ => AssetOverwriteMode.Cancel
            };
        }

        /// <summary>
        /// 应用Json到已生成资产
        /// </summary>
        /// <param name="assetPath"></param>
        /// <param name="json"></param>
        private void ApplyJsonToGeneratedAsset(string assetPath, string json)
        {
            if (string.IsNullOrEmpty(assetPath) || string.IsNullOrEmpty(json)) return;
            var assetObj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath);
            ApplyDraftJsonToAsset(assetObj, json);
        }

        /// <summary>
        /// 注册已生成资产路径
        /// </summary>
        /// <param name="info"></param>
        /// <param name="assetPath"></param>
        private void RegisterGeneratedAssetPath(ScriptableObjectInfo info, string assetPath)
        {
            if (info == null || string.IsNullOrEmpty(assetPath)) return;
            info.outputAssetPaths ??= new List<string>();
            info.outputAssetPaths.RemoveAll(path => string.IsNullOrEmpty(path) || path.Equals(assetPath, StringComparison.Ordinal));
            info.outputAssetPaths.Add(assetPath);
        }

        /// <summary>
        /// 清理无效的已生成资产路径
        /// </summary>
        /// <param name="info"></param>
        private void CleanupOutputAssetPaths(ScriptableObjectInfo info)
        {
            if (info == null)
            {
                return;
            }

            info.outputAssetPaths ??= new List<string>();
            info.outputAssetPaths = info.outputAssetPaths
                .Where(path => string.IsNullOrEmpty(path) == false && AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path) != null)
                .Distinct()
                .ToList();
        }

        /// <summary>
        /// 获取计划资产路径
        /// </summary>
        /// <param name="outputFolder"></param>
        /// <param name="assetName"></param>
        /// <returns></returns>
        private string GetPlannedAssetPath(string outputFolder, string assetName)
        {
            var normalizedOutputFolder = string.IsNullOrEmpty(outputFolder) ? _outputFolder : outputFolder;
            return Path.Combine(normalizedOutputFolder, $"{SanitizeAssetName(assetName)}.asset").Replace("\\", "/");
        }

        /// <summary>
        /// 当前路径下是否已经生成了同名资产
        /// </summary>
        /// <param name="assetPath"></param>
        /// <returns></returns>
        private bool IsAssetAlreadyGenerated(string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath)) return false;
            return AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath) != null;
        }

        /// <summary>
        /// 获取待生成资产项折叠Key
        /// </summary>
        /// <param name="scriptKey"></param>
        /// <param name="planInfo"></param>
        /// <returns></returns>
        private string GetAssetPlanFoldoutKey(int scriptKey, AssetPlanInfo planInfo)
        {
            var planKey = planInfo == null || string.IsNullOrEmpty(planInfo.key) ? "default" : planInfo.key;
            return $"{scriptKey}_{planKey}";
        }

        /// <summary>
        /// 打开待生成资产编辑窗口
        /// </summary>
        /// <param name="soInfo"></param>
        /// <param name="planInfo"></param>
        private void OpenPlanAssetEditor(ScriptableObjectInfo soInfo, AssetPlanInfo planInfo)
        {
            if (soInfo == null || planInfo == null) return;

            if (planInfo.isPreviewAsset)
            {
                EditorUtility.DisplayDialog("提示", "当前项属于自定义拆分预览资产。为了避免覆盖拆分器自动填充值，建议先生成后再编辑真实资产。", "确定");
                return;
            }

            var scriptObj = AssetDatabase.LoadAssetAtPath<MonoScript>(soInfo.path);
            if (scriptObj == null)
            {
                EditorUtility.DisplayDialog("提示", "脚本文件不存在。", "确定");
                return;
            }

            var classType = scriptObj.GetClass();
            if (classType == null || classType.IsSubclassOf(typeof(ScriptableObject)) == false)
            {
                EditorUtility.DisplayDialog("提示", "当前脚本不是 ScriptableObject 类型。", "确定");
                return;
            }

            var tempAsset = ScriptableObject.CreateInstance(classType);
            tempAsset.hideFlags = HideFlags.HideAndDontSave;
            ApplyDraftJsonToAsset(tempAsset, planInfo.editorJson);

            UniversalAssetEditorWindow.ShowWindow(
                tempAsset,
                $"编辑待生成资产 - {planInfo.name}",
                targetObject =>
                {
                    if (targetObject == null) return;
                    planInfo.editorJson = EditorJsonUtility.ToJson(targetObject, true);
                },
                destroyTargetOnClose: true,
                autoSavePersistentAsset: false,
                description: "当前正在编辑待生成资产草稿。点击“保存”或直接关闭窗口，都会把草稿写回列表项。"
            );
        }

        /// <summary>
        /// 打开已生成资产编辑窗口
        /// </summary>
        /// <param name="assetPath"></param>
        private void OpenGeneratedAssetEditor(string assetPath)
        {
            var assetObj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath);
            if (assetObj == null)
            {
                EditorUtility.DisplayDialog("提示", "资产不存在或路径无效。", "确定");
                return;
            }

            UniversalAssetEditorWindow.ShowWindow(
                assetObj,
                $"编辑已生成资产 - {assetObj.name}",
                targetObject =>
                {
                    if (targetObject == null) return;
                    EditorUtility.SetDirty(targetObject);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                },
                destroyTargetOnClose: false,
                autoSavePersistentAsset: true,
                description: "当前正在编辑已生成的真实资产。保存后会直接写入 Asset 文件。"
            );
        }

        /// <summary>
        /// 应用草稿Json到资产对象
        /// </summary>
        /// <param name="assetObj"></param>
        /// <param name="editorJson"></param>
        private static void ApplyDraftJsonToAsset(UnityEngine.Object assetObj, string editorJson)
        {
            if (assetObj == null || string.IsNullOrEmpty(editorJson)) return;
            EditorJsonUtility.FromJsonOverwrite(editorJson, assetObj);
            EditorUtility.SetDirty(assetObj);
        }

        /// <summary>
        /// 将草稿数据应用到已生成的资产
        /// </summary>
        /// <param name="assetPlans"></param>
        /// <param name="generatedPaths"></param>
        private void ApplyDraftJsonToGeneratedAssets(List<AssetPlanInfo> assetPlans, List<string> generatedPaths)
        {
            if (assetPlans == null || generatedPaths == null) return;

            foreach (var assetPlan in assetPlans)
            {
                if (assetPlan == null || string.IsNullOrEmpty(assetPlan.editorJson)) continue;
                var assetPath = FindGeneratedAssetPathByPlanName(generatedPaths, assetPlan.name);
                if (string.IsNullOrEmpty(assetPath)) continue;

                var assetObj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath);
                ApplyDraftJsonToAsset(assetObj, assetPlan.editorJson);
            }
        }

        /// <summary>
        /// 根据计划名称查找生成后的资产路径
        /// </summary>
        /// <param name="generatedPaths"></param>
        /// <param name="planName"></param>
        /// <returns></returns>
        private string FindGeneratedAssetPathByPlanName(List<string> generatedPaths, string planName)
        {
            if (generatedPaths == null || string.IsNullOrEmpty(planName)) return string.Empty;

            var normalizedPlanName = SanitizeAssetName(planName);
            foreach (var generatedPath in generatedPaths)
            {
                if (string.IsNullOrEmpty(generatedPath)) continue;
                var fileName = Path.GetFileNameWithoutExtension(generatedPath);
                if (fileName.Equals(normalizedPlanName, StringComparison.Ordinal))
                {
                    return generatedPath;
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// 清理资产名称
        /// </summary>
        /// <param name="assetName"></param>
        /// <returns></returns>
        private static string SanitizeAssetName(string assetName)
        {
            assetName = string.IsNullOrWhiteSpace(assetName) ? "NewAsset" : assetName.Trim();
            assetName = Path.GetFileNameWithoutExtension(assetName);
            return string.IsNullOrWhiteSpace(assetName) ? "NewAsset" : assetName;
        }

        /// <summary>
        /// 获取上级目录
        /// </summary>
        /// <param name="path"></param>
        /// <param name="level"></param>
        /// <param name="lastFold"></param>
        /// <returns></returns>
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