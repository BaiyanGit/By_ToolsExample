namespace _3rdBy.ByTools.MacroDefinitionSwitcher.Editor
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using UnityEditor;
    using UnityEditor.Build;
    using UnityEngine;

    /// <summary>
    /// 宏定义切换工具的编辑器窗口
    /// </summary>
    public class DefinitionSwitcherEditorWindow : EditorWindow
    {
        [MenuItem("ByTools/🔖 宏定义切换工具")]
        public static void ShowWindow()
        {
            GetWindow<DefinitionSwitcherEditorWindow>("宏定义切换工具");
        }

        // 预定义的宏定义配置
        private readonly List<MacroConfig> _predefinedMacros = new()
        {
            new MacroConfig("日志窗口模式", "DEVELOP_DEBUG", "日志调试信息专用"),
            new MacroConfig("日志文件模式", "DEVELOP_LOGFILE", "日志文件模式专用"),
            new MacroConfig("发布模式", "", "发布版本配置"),
        };

        private Vector2 _scrollPosition;
        private string _platformName;
        private string _customMacros = "";
        private string _currentMacros = "";
        private bool _showCustomPanel;

        // 新增：打包相关字段
        private bool _buildAfterSwitch;
        private string _buildOutputPath = "Builds";
        private bool _showBuildSettings;
        private bool _developmentBuild;
        private bool _buildNameByTime;
        private bool _buildNameByPlatform;
        private bool _autoRunAfterBuild;
        private bool _customBuildName;
        private string _buildProductName;

        [Serializable]
        private class MacroConfig
        {
            public string name;
            public string macros;
            public string description;

            public MacroConfig(string name, string macros, string description)
            {
                this.name        = name;
                this.macros      = macros;
                this.description = description;
            }
        }

        private void OnEnable()
        {
            RefreshCurrentMacros();
            // 加载保存的设置
            _buildAfterSwitch    = EditorPrefs.GetBool("MacroSwitcher_BuildAfterSwitch", false);
            _buildOutputPath     = EditorPrefs.GetString("MacroSwitcher_BuildOutputPath", "Builds");
            _developmentBuild    = EditorPrefs.GetBool("MacroSwitcher_DevelopmentBuild", false);
            _buildNameByTime     = EditorPrefs.GetBool("MacroSwitcher_BuildNameByTime", false);
            _buildNameByPlatform = EditorPrefs.GetBool("MacroSwitcher_BuildNameByPlatform", false);
            _autoRunAfterBuild   = EditorPrefs.GetBool("MacroSwitcher_AutoRunAfterBuild", false);
            _customBuildName     = EditorPrefs.GetBool("MacroSwitcher_CustomBuildName", false);
            _buildProductName    = EditorPrefs.GetString("MacroSwitcher_BuildProductName", "");
            if (string.IsNullOrEmpty(_buildProductName))
            {
                _buildProductName = Application.productName;
            }
        }

        private BuildTargetGroup _buildTargetGroup;

        private void RefreshCurrentMacros()
        {
            _buildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
#if UNITY_2020_1_OR_NEWER
            var selected = NamedBuildTarget.FromBuildTargetGroup(_buildTargetGroup);
            PlayerSettings.GetScriptingDefineSymbols(selected, out var allMacros);
            _currentMacros = string.Join(";", allMacros);
#else
            _currentMacros = PlayerSettings.GetScriptingDefineSymbolsForGroup(_buildTargetGroup);
#endif
        }

        private void OnGUI()
        {
            EditorGUILayout.Space();

            // 显示当前宏定义
            EditorGUILayout.LabelField("当前宏定义", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox($"[{_platformName}] {_currentMacros}", MessageType.Info);

            EditorGUILayout.Space();

            // 预定义配置区域
            EditorGUILayout.LabelField("快速切换配置", EditorStyles.boldLabel);

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, GUILayout.Height(100));

            foreach (var config in _predefinedMacros)
            {
                DrawMacroConfigButton(config);
            }

            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space();

            // 自定义宏定义区域
            _showCustomPanel = EditorGUILayout.Foldout(_showCustomPanel, "自定义宏定义", true);
            if (_showCustomPanel)
            {
                DrawCustomMacrosPanel();
            }

            EditorGUILayout.Space();
            // 工具按钮区域
            DrawUtilityButtons();

            EditorGUILayout.Space();
            // 新增：打包设置区域
            DrawBuildSettingsPanel();
        }

        private void DrawMacroConfigButton(MacroConfig config)
        {
            GUILayout.BeginVertical("box");

            GUILayout.BeginHorizontal();

            // 配置名称和描述
            GUILayout.BeginVertical();
            EditorGUILayout.LabelField(config.name, EditorStyles.boldLabel);
            if (!string.IsNullOrEmpty(config.description))
            {
                EditorGUILayout.LabelField(config.description, EditorStyles.miniLabel);
            }

            GUILayout.EndVertical();

            GUILayout.FlexibleSpace();

            // 应用按钮
#if UNITY_2020_1_OR_NEWER
            var selected = NamedBuildTarget.FromBuildTargetGroup(_buildTargetGroup);
            PlayerSettings.GetScriptingDefineSymbols(selected, out var allMacros);
            var allMacrosStr = string.Join(";", allMacros);
            // var isActive = allMacrosStr.Contains(config.macros);
            var isActive = allMacrosStr == config.macros;
#else
            var currentMacros = PlayerSettings.GetScriptingDefineSymbolsForGroup(_buildTargetGroup);
            var isActive = currentMacros == config.macros;
#endif
            GUI.enabled   = !isActive;
            _platformName = isActive ? config.name : "无模式";

            if (GUILayout.Button(isActive ? "已激活" : "应用", GUILayout.Width(60)))
            {
                ApplyMacros(config.macros, config.name);
            }

            GUI.enabled = true;

            GUILayout.EndHorizontal();

            // 显示宏定义内容
            if (!string.IsNullOrEmpty(config.macros))
            {
                EditorGUILayout.LabelField($"宏定义: {config.macros}", EditorStyles.miniLabel);
            }

            GUILayout.EndVertical();
        }

        private void DrawCustomMacrosPanel()
        {
            EditorGUILayout.HelpBox("输入自定义宏定义，多个宏定义用分号(;)分隔", MessageType.Info);

            _customMacros = EditorGUILayout.TextField("宏定义", _customMacros);

            GUILayout.BeginHorizontal();

            if (GUILayout.Button("应用自定义宏定义"))
            {
                if (!string.IsNullOrEmpty(_customMacros))
                {
                    ApplyMacros(_customMacros, "自定义配置");
                }
                else
                {
                    EditorUtility.DisplayDialog("提示", "请输入有效的宏定义", "确定");
                }
            }

            if (GUILayout.Button("添加到预定义"))
            {
                ShowAddToPredefinedWindow();
            }

            GUILayout.EndHorizontal();
        }

        // 新增：打包设置面板
        private void DrawBuildSettingsPanel()
        {
            EditorGUILayout.LabelField("打包设置", EditorStyles.boldLabel);

            EditorGUILayout.HelpBox($"当前平台: {EditorUserBuildSettings.activeBuildTarget.ToString()}({_buildTargetGroup})", MessageType.Info);


            GUILayout.BeginVertical("box");

            // 打包输出路径
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("输出路径", GUILayout.Width(80));
            _buildOutputPath = EditorGUILayout.TextField(_buildOutputPath);
            if (GUILayout.Button("浏览", GUILayout.Width(50)))
            {
                var selectedPath = EditorUtility.SaveFolderPanel("选择打包输出目录", _buildOutputPath, "");
                if (!string.IsNullOrEmpty(selectedPath))
                {
                    _buildOutputPath = selectedPath;
                    EditorPrefs.SetString("MacroSwitcher_BuildOutputPath", _buildOutputPath);
                }
            }

            GUILayout.EndHorizontal();

            // 打包选项
            _showBuildSettings = EditorGUILayout.Foldout(_showBuildSettings, "高级打包选项", true);
            if (_showBuildSettings)
            {
                GUILayout.BeginHorizontal();

                // 构建名称
                var newNameByTime = EditorGUILayout.Toggle(_buildNameByTime, GUILayout.Width(15));
                EditorGUILayout.LabelField("以时间构建名称", GUILayout.Width(100));
                if (newNameByTime != _buildNameByTime)
                {
                    _buildNameByTime = newNameByTime;
                    EditorPrefs.SetBool("MacroSwitcher_BuildNameByTime", _buildNameByTime);
                }

                // 构建名称
                var newNameByPlatform = EditorGUILayout.Toggle(_buildNameByPlatform, GUILayout.Width(15));
                EditorGUILayout.LabelField("以平台构建名称", GUILayout.Width(100));
                if (newNameByPlatform != _buildNameByPlatform)
                {
                    _buildNameByPlatform = newNameByPlatform;
                    EditorPrefs.SetBool("MacroSwitcher_BuildNameByPlatform", _buildNameByPlatform);
                }

                // 开发模式构建
                var newDevBuild = EditorGUILayout.Toggle(_developmentBuild, GUILayout.Width(15));
                EditorGUILayout.LabelField("开发模式构建", GUILayout.Width(100));
                if (newDevBuild != _developmentBuild)
                {
                    _developmentBuild = newDevBuild;
                    EditorPrefs.SetBool("MacroSwitcher_DevelopmentBuild", _developmentBuild);
                }

                // 构建后自动运行
                var newAutoRun = EditorGUILayout.Toggle(_autoRunAfterBuild, GUILayout.Width(15));
                EditorGUILayout.LabelField("构建后自动运行", GUILayout.Width(100));
                if (newAutoRun != _autoRunAfterBuild)
                {
                    _autoRunAfterBuild = newAutoRun;
                    EditorPrefs.SetBool("MacroSwitcher_AutoRunAfterBuild", _autoRunAfterBuild);
                }

                // 是否切换后直接打包
                var newBuildAfterSwitch = EditorGUILayout.Toggle(_buildAfterSwitch, GUILayout.Width(15));
                EditorGUILayout.LabelField("切换宏定义后直接打包", GUILayout.Width(130));
                if (newBuildAfterSwitch != _buildAfterSwitch)
                {
                    _buildAfterSwitch = newBuildAfterSwitch;
                    EditorPrefs.SetBool("MacroSwitcher_BuildAfterSwitch", _buildAfterSwitch);
                }

                // 自定义构建名称
                var newCustomBuildName = EditorGUILayout.Toggle(_customBuildName, GUILayout.Width(15));
                EditorGUILayout.LabelField("自定义构建名称", GUILayout.Width(100));
                if (newCustomBuildName != _customBuildName)
                {
                    _customBuildName = newCustomBuildName;
                    EditorPrefs.SetBool("MacroSwitcher_CustomBuildName", newCustomBuildName);
                }

                var newBuildProductName = _customBuildName ? EditorGUILayout.TextField(_buildProductName) : "";
                if (!_buildProductName.Equals(newBuildProductName))
                {
                    _buildProductName = newBuildProductName;
                    EditorPrefs.SetString("MacroSwitcher_BuildProductName", _buildProductName);
                }

                GUILayout.EndHorizontal();
            }

            // 立即打包按钮
            if (GUILayout.Button("立即打包（使用当前设置）"))
            {
                BuildWithCurrentSettings();
            }

            GUILayout.EndVertical();
        }

        private void DrawUtilityButtons()
        {
            EditorGUILayout.LabelField("工具", EditorStyles.boldLabel);

            GUILayout.BeginHorizontal();

            if (GUILayout.Button("刷新显示"))
            {
                RefreshCurrentMacros();
                Repaint();
            }

            if (GUILayout.Button("复制当前宏定义"))
            {
                EditorGUIUtility.systemCopyBuffer = _currentMacros;
                ShowNotification(new GUIContent("已复制到剪贴板"));
            }

            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();

            if (GUILayout.Button("清空宏定义"))
            {
                if (EditorUtility.DisplayDialog("确认", "确定要清空所有宏定义吗？", "确定", "取消"))
                {
                    ApplyMacros("", "清空配置");
                }
            }

            // 平台切换
            if (GUILayout.Button($"切换平台: {_buildTargetGroup}"))
            {
                ShowPlatformSelectionMenu();
            }

            GUILayout.EndHorizontal();
        }

        private void ApplyMacros(string macros, string configName)
        {
            try
            {
                // 设置宏定义
#if UNITY_2020_1_OR_NEWER
                var selected = NamedBuildTarget.FromBuildTargetGroup(_buildTargetGroup);
                PlayerSettings.SetScriptingDefineSymbols(selected, macros);
#else
                PlayerSettings.SetScriptingDefineSymbolsForGroup(_buildTargetGroup, macros);
#endif

                // 刷新显示
                RefreshCurrentMacros();

                // 显示提示
                ShowNotification(new GUIContent($"已应用配置: {configName}"));

                // 强制重新编译
                AssetDatabase.Refresh();

                // Debug.Log($"宏定义已切换: {configName}\n新的宏定义: {macros}");

                // 新增：如果启用打包选项，则自动打包
                if (_buildAfterSwitch)
                {
                    EditorApplication.delayCall += () =>
                    {
                        if (EditorUtility.DisplayDialog("开始打包", $"宏定义已切换为: {configName}\n是否开始打包？", "开始打包", "取消"))
                        {
                            BuildWithCurrentSettings();
                        }
                    };
                }
            }
            catch (Exception e)
            {
                EditorUtility.DisplayDialog("错误", $"应用宏定义时出错: {e.Message}", "确定");
            }
        }

        // 新增：使用当前设置进行打包
        private void BuildWithCurrentSettings()
        {
            try
            {
                // 获取场景列表
                var scenes = EditorBuildSettings.scenes
                    .Where(scene => scene.enabled)
                    .Select(scene => scene.path)
                    .ToArray();

                if (scenes.Length == 0)
                {
                    EditorUtility.DisplayDialog("错误", "没有找到可用的场景，请在Build Settings中添加场景", "确定");
                    return;
                }

                // 准备构建选项
                var options                     = BuildOptions.None;
                if (_developmentBuild) options  |= BuildOptions.Development;
                if (_autoRunAfterBuild) options |= BuildOptions.AutoRunPlayer;

                // 构建输出路径目录
                // var productName        = string.IsNullOrEmpty(PlayerSettings.productName) ? "UnityGame" : PlayerSettings.productName;
                string productName        = GetProductName();
                var    timestamp          = _buildNameByTime ? $"({DateTime.Now:HH.mm})" : "";
                var    platform           = _buildNameByPlatform ? $"_{PlatformName()}" : "";
                var    outBuildFolderName = $"{productName}{platform}{timestamp}";
                var    buildOutPath       = $"{_buildOutputPath}/{outBuildFolderName}";

                Debug.Log($"准备构建输出路径: {buildOutPath}     -- {outBuildFolderName}");
                // 确保输出目录存在
                if (!Directory.Exists(buildOutPath))
                {
                    Directory.CreateDirectory(buildOutPath);
                }

                // 构建玩家
                var buildPath = Path.Combine(buildOutPath, GetBuildFileName());
                // Debug.Log($"准备构建输出路径: {buildPath}");
                // return;

#if UNITY_2018_3_OR_NEWER
                var report       = BuildPipeline.BuildPlayer(scenes, buildPath, EditorUserBuildSettings.activeBuildTarget, options);
                var buildSuccess = report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded;
                var errorMsg     = buildSuccess ? "" : report.summary.ToString();
                // 测试
                // var buildSuccess = true;
                // var errorMsg     = "";
#else
                string errorMsg = BuildPipeline.BuildPlayer(scenes, buildPath, EditorUserBuildSettings.activeBuildTarget, options);
                bool buildSuccess = string.IsNullOrEmpty(errorMsg);
#endif

                if (buildSuccess)
                {
                    ShowNotification(new GUIContent("打包成功！"));
                    Debug.Log($"打包成功！输出路径: {buildPath}");

                    // 在文件管理器中显示
                    EditorUtility.RevealInFinder(buildPath);
                }
                else
                {
                    ShowNotification(new GUIContent("打包失败！"));
                    Debug.LogError($"打包失败！错误信息: {errorMsg}");
                }
            }
            catch (Exception e)
            {
                EditorUtility.DisplayDialog("打包错误", $"打包过程中发生错误: {e.Message}", "确定");
                Debug.LogError($"打包错误: {e}");
            }
        }

        private static string PlatformName()
        {
            var platform = EditorUserBuildSettings.activeBuildTarget.ToString();
            return platform.Replace("Standalone", "").Replace("Windows", "Win").Replace("OSX", "Mac");
        }

        private string GetProductName()
        {
            string productName;

            if (!string.IsNullOrEmpty(_buildProductName))
            {
                productName = _buildProductName;
            }
            else if (string.IsNullOrEmpty(PlayerSettings.productName))
            {
                productName = "UnityGame";
            }
            else
            {
                productName = PlayerSettings.productName;
            }

            Debug.Log($"获取产品名称: {productName}");
            return productName;
        }

        // 新增：生成构建文件名
        private string GetBuildFileName()
        {
            string productName = GetProductName();

            switch (EditorUserBuildSettings.activeBuildTarget)
            {
                case BuildTarget.StandaloneWindows:
                case BuildTarget.StandaloneWindows64:
                    productName += ".exe";
                    break;
                case BuildTarget.StandaloneOSX:
                    productName += ".app";
                    break;
                case BuildTarget.Android:
                    productName += ".apk";
                    break;
                case BuildTarget.iOS:
                    break;
            }

            Debug.Log($"生成构建文件名: {productName}");
            return productName;
        }

        private void ShowAddToPredefinedWindow()
        {
            var window = CreateInstance<AddMacroConfigWindow>();
            window.Initialize(_customMacros, this);
            window.ShowUtility();
        }

        private void ShowPlatformSelectionMenu()
        {
            var menu = new GenericMenu();

            foreach (BuildTargetGroup platform in Enum.GetValues(typeof(BuildTargetGroup)))
            {
                if (platform == BuildTargetGroup.Unknown || ObsoletePlatform(platform))
                    continue;

                menu.AddItem(new GUIContent(platform.ToString()), _buildTargetGroup == platform, () => SwitchPlatform(platform));
            }

            menu.ShowAsContext();
        }

        // 新增：判断平台是否过时
        private static bool ObsoletePlatform(BuildTargetGroup platform)
        {
            var obsoletePlatforms = new List<BuildTargetGroup>
            {
                BuildTargetGroup.XBOX360,
                BuildTargetGroup.PS3,
                BuildTargetGroup.PSP2
            };

            return obsoletePlatforms.Contains(platform);
        }

        private void SwitchPlatform(BuildTargetGroup platform)
        {
            _buildTargetGroup = platform;
            RefreshCurrentMacros();
            Repaint();
            ShowNotification(new GUIContent($"已切换到平台: {platform}"));
        }

        public void AddPredefinedConfig(string defineName, string macros, string description)
        {
            _predefinedMacros.Add(new MacroConfig(defineName, macros, description));
            Repaint();
        }
    }

    // 添加新配置的窗口（保持不变）
    public class AddMacroConfigWindow : EditorWindow
    {
        private string _configName = "";
        private string _configMacros = "";
        private string _configDescription = "";
        private DefinitionSwitcherEditorWindow _parentWindow;

        public void Initialize(string macros, DefinitionSwitcherEditorWindow parent)
        {
            _configMacros = macros;
            _parentWindow = parent;
            titleContent  = new GUIContent("添加预定义配置");
            minSize       = new Vector2(400, 200);
        }

        private void OnGUI()
        {
            EditorGUILayout.Space();

            _configName        = EditorGUILayout.TextField("配置名称", _configName);
            _configMacros      = EditorGUILayout.TextField("宏定义", _configMacros);
            _configDescription = EditorGUILayout.TextField("描述", _configDescription);

            EditorGUILayout.HelpBox("配置名称和宏定义不能为空", MessageType.Info);

            EditorGUILayout.Space();

            GUILayout.BeginHorizontal();

            if (GUILayout.Button("确认添加"))
            {
                if (string.IsNullOrEmpty(_configName) || string.IsNullOrEmpty(_configMacros))
                {
                    EditorUtility.DisplayDialog("错误", "配置名称和宏定义不能为空", "确定");
                    return;
                }

                _parentWindow.AddPredefinedConfig(_configName, _configMacros, _configDescription);
                Close();
            }

            if (GUILayout.Button("取消"))
            {
                Close();
            }

            GUILayout.EndHorizontal();
        }
    }
}