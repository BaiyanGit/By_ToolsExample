//=====================================================
// 文件名称: RecorderSdkHubWindow
// 创 建 者: wangbaiyan
// 创建日期: 2026-05-26
// 描    述: Recorder SDK 编辑器统一控制台，集中管理 UI 工具、参数参考、环境检查、打包发布和文档查看。
//=====================================================

#if UNITY_EDITOR
namespace Demos.示例_录制视频Recorder.Editor
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text;
    using UnityEditor;
    using UnityEditor.SceneManagement;
    using UnityEngine;
    using Debug = UnityEngine.Debug;

    /// <summary>
    /// Recorder SDK 编辑器统一控制台。
    /// </summary>
    public sealed class RecorderSdkHubWindow : EditorWindow
    {
        private const string SDK_VERSION = "Recorder SDK v0.3.0-beta";
        private const string PREFERRED_SDK_ROOT = "Assets/Demos/RecorderSdk";
        private const string STREAMING_ASSETS_ROOT = "Assets/StreamingAssets/FFmpegTools";
        private const string WINDOW_TITLE = "Recorder SDK 控制台";
        private const string SDK_ROOT_EDITOR_PREFS_KEY = "RecorderSdkHubWindow.SdkRoot";
        private const int MAX_DOCUMENT_DISPLAY_CHARS = 50000;

        private readonly string[] _tabs =
        {
            "首页",
            "UI 工具",
            "API 参数",
            "SDK API",
            "环境检查",
            "打包发布",
            "文档中心",
            "更新日志",
            "验证记录",
            "关于 SDK"
        };

        private readonly List<CheckItem> _environmentChecks = new();
        private readonly List<CheckItem> _packageChecks = new();
        private Vector2 _leftScroll;
        private Vector2 _rightScroll;
        private Vector2 _documentListScroll;
        private Vector2 _documentContentScroll;
        private Vector2 _changeLogScroll;
        private Vector2 _validationScroll;
        private Vector2 _sdkApiCategoryScroll;
        private Vector2 _sdkApiContentScroll;
        private int _selectedTab;
        private int _selectedApiTab;
        private int _selectedSdkApiCategory;
        private int _selectedValidationTab;
        private int _selectedDocumentIndex = -1;
        private string _sdkRoot;
        private string _documentationRoot;
        private string _lastEnvironmentCheckTime = "未执行";
        private string _lastPackageCheckTime = "未执行";
        private string _selectedDocumentContent = string.Empty;
        private string _sdkRootSource = "未检测";
        private string _currentConfigIdCache = "未读取";
        private string _changeLogPath = string.Empty;
        private string _fullChangeLogPath = string.Empty;
        private string _changeLogText = string.Empty;
        private string _changeLogError = string.Empty;
        private string _validationSummaryPath = string.Empty;
        private string _runtimeValidationPath = string.Empty;
        private string _unityPackageValidationPath = string.Empty;
        private DateTime _changeLogLastWriteTime;
        private bool _changeLogLoaded;
        private bool _includeFFmpegApp = true;
        private bool _includeWasapiDll = true;
        private bool _includeDocumentation = true;
        private bool _includeDemoScenes = true;
        private bool _includeVideosReadme = true;
        private GUIStyle _headerStyle;
        private GUIStyle _sectionStyle;
        private GUIStyle _statusOkStyle;
        private GUIStyle _statusWarningStyle;
        private GUIStyle _statusErrorStyle;
        private GUIStyle _mutedStyle;
        private GUIStyle _markdownStyle;
        private GUIStyle _cardStyle;
        private GUIStyle _mainCardStyle;
        private GUIStyle _smallTextStyle;
        private GUIStyle _tableHeaderStyle;
        private GUIStyle _navItemStyle;
        private GUIStyle _selectedNavItemStyle;
        private GUIStyle _navAccentStyle;
        private FFmpegParameterGuideContent _ffmpegGuideContent;
        private VideoEncodingGuideContent _videoEncodingGuideContent;
        private readonly Dictionary<string, CachedDocument> _documentCache = new();
        private bool _ffmpegGuideInitialized;
        private bool _videoEncodingGuideInitialized;

        private List<DocumentEntry> _documentEntries;
        private List<SdkApiDocEntry> _sdkApiEntries;

        /// <summary>
        /// 打开 Recorder SDK 控制台。
        /// </summary>
        [MenuItem("ByTools/🔴 Recorder SDK控制台")]
        public static void ShowWindow()
        {
            var window = GetWindow<RecorderSdkHubWindow>(WINDOW_TITLE);
            window.minSize = new Vector2(980f, 640f);
            window.Show();
        }

        /// <summary>
        /// 初始化窗口路径和文档列表。
        /// </summary>
        private void OnEnable()
        {
            _sdkRoot = ResolveSdkRoot();
            _documentationRoot = ResolveDocumentationRoot();
            _documentEntries = CreateDocumentEntries();
            _sdkApiEntries = CreateDetailedSdkApiEntries();
            _changeLogPath = ResolveDocumentPath("Changelog/RecorderSdkChangeLogSummary.md");
            _fullChangeLogPath = ResolveDocumentPath("Changelog/RecorderSettingsChangeLog.md");
            _validationSummaryPath = ResolveDocumentPath("Validation/RecorderSdkValidationSummary.md");
            _runtimeValidationPath = ResolveDocumentPath("Validation/RecorderSdkRuntimeValidation.md");
            _unityPackageValidationPath = ResolveDocumentPath("Validation/RecorderSdkUnityPackageValidation.md");
            _selectedDocumentIndex = 0;
            RefreshDocumentContent();
            RunBasicEnvironmentChecks();
            RunPackageChecks();
        }

        /// <summary>
        /// 绘制控制台窗口。
        /// </summary>
        private void OnGUI()
        {
            EnsureStyles();
            EditorGUILayout.BeginHorizontal();
            DrawNavigation();
            DrawContent();
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// 绘制左侧导航。
        /// </summary>
        private void DrawNavigation()
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(190f));
            GUILayout.Space(12f);
            GUILayout.Label("Recorder SDK", _headerStyle);
            GUILayout.Label("SDK 工作台", _mutedStyle);
            GUILayout.Space(12f);

            _leftScroll = EditorGUILayout.BeginScrollView(_leftScroll);
            for (int i = 0; i < _tabs.Length; i++)
            {
                bool selected = _selectedTab == i;
                EditorGUILayout.BeginHorizontal(GUILayout.Height(26f));
                GUILayout.Label(selected ? "|" : string.Empty, selected ? _navAccentStyle : GUIStyle.none, GUILayout.Width(4f), GUILayout.Height(24f));
                bool nextSelected = GUILayout.Toggle(selected, _tabs[i], selected ? _selectedNavItemStyle : _navItemStyle, GUILayout.Height(26f));
                EditorGUILayout.EndHorizontal();
                if (nextSelected && !selected)
                {
                    _selectedTab = i;
                    GUI.FocusControl(null);
                    OnSelectedTabChanged();
                }

                GUILayout.Space(2f);
            }

            EditorGUILayout.EndScrollView();
            GUILayout.Space(8f);
            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// 绘制右侧内容区。
        /// </summary>
        private void DrawContent()
        {
            EditorGUILayout.BeginVertical();
            if (_selectedTab == 3 || _selectedTab == 6 || _selectedTab == 7 || _selectedTab == 8)
            {
                switch (_selectedTab)
                {
                    case 3:
                        DrawSdkApiPage();
                        break;
                    case 6:
                        DrawDocumentationPage();
                        break;
                    case 7:
                        DrawChangeLogPage();
                        break;
                    case 8:
                        DrawValidationPage();
                        break;
                }

                EditorGUILayout.EndVertical();
                return;
            }

            _rightScroll = EditorGUILayout.BeginScrollView(_rightScroll);
            switch (_selectedTab)
            {
                case 0:
                    DrawHomePage();
                    break;
                case 1:
                    DrawUiToolsPage();
                    break;
                case 2:
                    DrawApiPage();
                    break;
                case 4:
                    DrawEnvironmentPage();
                    break;
                case 5:
                    DrawPackagePage();
                    break;
                case 9:
                    DrawAboutPage();
                    break;
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// 绘制首页信息。
        /// </summary>
        private void DrawHomePage()
        {
            DrawPageHeader("Recorder SDK 控制台", "版本：v0.3.0-beta。专业录制 SDK 管理与集成工具。");

            bool ffmpegExists = File.Exists(GetDefaultFFmpegPath());
            bool isWindows = IsWindowsEditorPlatform();
            bool wasapiExists = isWindows && File.Exists(ResolveAssetPath("Plugins/Windows/x86_64/WASAPILoopbackRecorder.dll"));
            bool configsExists = Directory.Exists(GetConfigsDirectory());
            string currentConfig = _currentConfigIdCache;
            string outputDirectory = Path.Combine(STREAMING_ASSETS_ROOT, "Videos").Replace("\\", "/");

            DrawMainCard("当前环境状态", "先确认依赖和路径是否可用，再进入演示或设置。", () =>
            {
                DrawStatusLine("FFmpeg", ffmpegExists ? "正常" : "未找到", ffmpegExists ? CheckLevel.Ok : CheckLevel.Error);
                if (isWindows) DrawStatusLine("WASAPI DLL", wasapiExists ? "正常" : "未找到", wasapiExists ? CheckLevel.Ok : CheckLevel.Error);
                DrawStatusLine("Configs", configsExists ? "正常" : "异常", configsExists ? CheckLevel.Ok : CheckLevel.Error);
                GUILayout.Space(8f);
                DrawInfo("SDK 根目录", _sdkRoot);
                DrawInfo("当前平台", Application.platform.ToString());
                DrawInfo("当前版本", SDK_VERSION);
                DrawInfo("当前配置", string.IsNullOrEmpty(currentConfig) ? "未读取" : currentConfig);
                DrawInfo("输出目录", outputDirectory);
            });

            GUILayout.Space(12f);
            DrawSectionTitle("快速开始");
            DrawTwoColumnLayout(
                () => DrawHomeActionGroup("屏幕录制", "快速验证录制功能。", () =>
                {
                    if (DrawActionButton("打开屏幕录制场景", GUILayout.Width(150f), GUILayout.Height(32f))) OpenSceneByName("录制器_屏幕录制");
                }),
                () => DrawHomeActionGroup("设置中心", "编辑视频、音频、推流、FFmpeg 等参数。", () =>
                {
                    if (DrawActionButton("打开设置中心场景", GUILayout.Width(150f), GUILayout.Height(32f))) OpenSceneByName("录制器_设置中心");
                    if (DrawActionButton("打开推流演示场景", GUILayout.Width(150f), GUILayout.Height(32f))) OpenSceneByName("录制器_推流演示");
                }));

            GUILayout.Space(12f);
            DrawSectionTitle("常用工具");
            EditorGUILayout.BeginHorizontal();
            if (DrawActionButton("打开 SDK 根目录", GUILayout.Width(140f), GUILayout.Height(26f))) OpenPath(_sdkRoot);
            if (DrawActionButton("打开 StreamingAssets", GUILayout.Width(160f), GUILayout.Height(26f))) OpenPath(STREAMING_ASSETS_ROOT);
            if (DrawActionButton("打开输出目录", GUILayout.Width(130f), GUILayout.Height(26f))) OpenPath(outputDirectory);
            if (DrawActionButton("打开 Configs", GUILayout.Width(110f), GUILayout.Height(26f))) OpenPath(GetConfigsDirectory());
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(12f);
            DrawSectionTitle("最近状态");
            DrawInfo("最近输出文件", "未检测");
            DrawInfo("最近 Session", "未检测");
            DrawInfo("最近错误码", "未检测");

            GUILayout.Space(12f);
            DrawSectionTitle("最近更新");
            DrawRecentUpdateSummary();
        }

        /// <summary>
        /// 绘制 UI 工具页。
        /// </summary>
        private void DrawUiToolsPage()
        {
            DrawPageHeader("UI 工具", "当前功能暂未开放。");

            DrawMainCard("后续规划", "UI 工具页暂时保留入口，不再作为 Beta 收口阶段的主要工作流。", () =>
            {
                GUILayout.Label("当前阶段聚焦 Recorder SDK Beta 收口、Runtime 稳定性、实机验证和 UnityPackage 导出检查。", _smallTextStyle);
                GUILayout.Label("后续如需扩展 Editor 辅助能力，可在此页重新规划独立工具。", _smallTextStyle);
            });
        }

        /// <summary>
        /// 绘制 UI 工具功能卡片。
        /// </summary>
        private void DrawUiToolCard(string title, string description, string buttonText, Action onClick)
        {
            EditorGUILayout.BeginVertical("box", GUILayout.MinWidth(280f), GUILayout.ExpandWidth(true));
            GUILayout.Label(title, _sectionStyle);
            GUILayout.Label(description, _mutedStyle);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(buttonText, GUILayout.Height(30f)))
            {
                onClick?.Invoke();
            }

            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// 绘制 API 参数参考页。
        /// </summary>
        private void DrawApiPage()
        {
            DrawPageHeader("API 参数", "查看 FFmpeg 与视频编码相关参数说明。");
            EditorGUILayout.BeginHorizontal();
            DrawApiTabButton(0, "FFmpeg 参数");
            DrawApiTabButton(1, "视频编码参数");
            if (DrawActionButton("刷新 API 缓存", GUILayout.Width(120f), GUILayout.Height(30f))) RefreshCurrentApiCache();
            EditorGUILayout.EndHorizontal();
            DrawInfoBox("左侧分类用于快速定位参数组，右侧参数说明可滚动查看。");

            GUILayout.Space(6f);
            float contentWidth = Mathf.Max(position.width - 230f, 720f);
            float contentHeight = Mathf.Max(position.height - 150f, 480f);
            if (_selectedApiTab == 0)
            {
                EnsureFFmpegGuideContent();
                _ffmpegGuideContent.DrawEmbedded(contentWidth, contentHeight);
            }
            else
            {
                EnsureVideoEncodingGuideContent();
                _videoEncodingGuideContent.DrawEmbedded(contentWidth, contentHeight);
            }
        }

        /// <summary>
        /// 绘制 SDK API 开发者说明页。
        /// </summary>
        private void DrawSdkApiPage()
        {
            DrawPageHeader("SDK API", "开发者接入 Recorder SDK 时常用的组件、函数、事件和示例。");
            if (_sdkApiEntries == null || _sdkApiEntries.Count == 0)
            {
                _sdkApiEntries = CreateDetailedSdkApiEntries();
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical(GUILayout.Width(220f));
            _sdkApiCategoryScroll = EditorGUILayout.BeginScrollView(_sdkApiCategoryScroll, GUILayout.Height(Mathf.Max(260f, position.height - 130f)));
            for (int i = 0; i < _sdkApiEntries.Count; i++)
            {
                bool selected = _selectedSdkApiCategory == i;
                bool nextSelected = GUILayout.Toggle(selected, _sdkApiEntries[i].title, "Button", GUILayout.Height(30f));
                if (nextSelected && !selected)
                {
                    _selectedSdkApiCategory = i;
                    _sdkApiContentScroll = Vector2.zero;
                }
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical();
            var entry = _sdkApiEntries[Mathf.Clamp(_selectedSdkApiCategory, 0, _sdkApiEntries.Count - 1)];
            DrawSectionTitle(entry.title);
            GUILayout.Label(entry.description, _smallTextStyle);
            GUILayout.Space(4f);
            DrawSdkApiDocument(entry.title, entry.content, ref _sdkApiContentScroll, Mathf.Max(260f, position.height - 170f));
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// 绘制环境检查页。
        /// </summary>
        private void DrawEnvironmentPage()
        {
            DrawPageHeader("环境检查", "检查 Recorder SDK 运行依赖、配置目录和输出目录状态。");
            EditorGUILayout.BeginHorizontal();
            if (DrawActionButton("重新检查", GUILayout.Width(110f), GUILayout.Height(30f))) RunFullEnvironmentChecks();
            GUILayout.Label("最后检查：" + _lastEnvironmentCheckTime, _mutedStyle);
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(8f);
            DrawCheckTable(_environmentChecks);
        }

        /// <summary>
        /// 绘制打包发布页。
        /// </summary>
        private void DrawPackagePage()
        {
            DrawPackagePageRedesigned();
            return;

            DrawTitle("打包发布");
            DrawInfo("当前 SDK 根目录", _sdkRoot);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("重新扫描 SDK 根目录", GUILayout.Width(160f), GUILayout.Height(30f))) RescanSdkRoot();
            if (GUILayout.Button("选择 SDK 根目录", GUILayout.Width(140f), GUILayout.Height(30f))) SelectSdkRoot();
            EditorGUILayout.EndHorizontal();
            string sdkRootWarning = GetSdkRootStructureWarning();
            if (!string.IsNullOrEmpty(sdkRootWarning))
            {
                EditorGUILayout.HelpBox(sdkRootWarning, MessageType.Warning);
            }

            GUILayout.Space(8f);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("检查 unitypackage 必要文件", GUILayout.Width(180f), GUILayout.Height(30f))) RunPackageChecks();
            if (GUILayout.Button("复制建议导出清单", GUILayout.Width(150f), GUILayout.Height(30f))) CopySuggestedExportList();
            GUILayout.Label("最后检查：" + _lastPackageCheckTime, _mutedStyle);
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(8f);
            DrawSection("打包内容");
            using (new EditorGUI.DisabledScope(true))
            {
                GUILayout.Toggle(true, "SDK 根目录（必选）");
                GUILayout.Toggle(true, "Configs（必选）");
            }

            _includeFFmpegApp = GUILayout.Toggle(_includeFFmpegApp, "包含 FFmpegApp");
            _includeWasapiDll = GUILayout.Toggle(_includeWasapiDll, "包含 WASAPI 原生 DLL");
            _includeDocumentation = GUILayout.Toggle(_includeDocumentation, "包含 Documentation");
            _includeDemoScenes = GUILayout.Toggle(_includeDemoScenes, "包含 Demo 场景");
            _includeVideosReadme = GUILayout.Toggle(_includeVideosReadme, "包含 Videos 空目录说明");

            GUILayout.Space(8f);
            DrawCheckItems(_packageChecks);
            EditorGUILayout.HelpBox("打包页只做检查和导出，不会自动删除文件或清理目录。Videos 目录里的实际录制视频不会被打进 unitypackage。", MessageType.Info);
        }

        /// <summary>
        /// 绘制新版打包发布页，只提供检查和导出清单，不自动导出 unitypackage。
        /// </summary>
        private void DrawPackagePageRedesigned()
        {
            DrawPageHeader("打包发布", "检查 unitypackage 交付清单和必要文件。导出动作由用户在 Unity Editor 中手动执行。");
            DrawCard("SDK 根目录", "当前识别到的 SDK 根目录与路径来源。", () =>
            {
                DrawInfo("当前 SDK 根目录", _sdkRoot);
                DrawInfo("路径来源", _sdkRootSource);
                EditorGUILayout.BeginHorizontal();
                if (DrawActionButton("重新扫描", GUILayout.Width(110f), GUILayout.Height(30f))) RescanSdkRoot();
                if (DrawActionButton("手动选择", GUILayout.Width(110f), GUILayout.Height(30f))) SelectSdkRoot();
                if (DrawActionButton("打开目录", GUILayout.Width(110f), GUILayout.Height(30f))) OpenPath(_sdkRoot);
                EditorGUILayout.EndHorizontal();
            });

            string sdkRootWarning = GetSdkRootStructureWarning();
            if (!string.IsNullOrEmpty(sdkRootWarning))
            {
                DrawInfoBox(sdkRootWarning, MessageType.Warning);
            }

            GUILayout.Space(8f);
            EditorGUILayout.BeginHorizontal();
            if (DrawActionButton("检查必要文件", GUILayout.Width(130f), GUILayout.Height(30f))) RunPackageChecks();
            if (DrawActionButton("复制建议导出清单", GUILayout.Width(150f), GUILayout.Height(30f))) CopySuggestedExportList();
            if (DrawActionButton("打开 StreamingAssets", GUILayout.Width(150f), GUILayout.Height(30f))) OpenPath(STREAMING_ASSETS_ROOT);
            GUILayout.Label("最后检查：" + _lastPackageCheckTime, _mutedStyle);
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(8f);
            DrawSectionTitle("打包内容检查");
            using (new EditorGUI.DisabledScope(true))
            {
                GUILayout.Toggle(true, "SDK 根目录（必选）");
                GUILayout.Toggle(true, "Configs（必选）");
            }

            _includeFFmpegApp = GUILayout.Toggle(_includeFFmpegApp, "包含 FFmpegApp");
            _includeWasapiDll = GUILayout.Toggle(_includeWasapiDll, "包含 WASAPI 原生 DLL");
            _includeDocumentation = GUILayout.Toggle(_includeDocumentation, "包含 Documentation");
            _includeDemoScenes = GUILayout.Toggle(_includeDemoScenes, "包含 Demo 场景");
            _includeVideosReadme = GUILayout.Toggle(_includeVideosReadme, "包含 Videos 空目录说明");

            GUILayout.Space(8f);
            DrawCheckTable(_packageChecks);

            GUILayout.Space(8f);
            DrawCard("导出说明", "unitypackage 由用户在 Unity Editor 中手动导出。", () =>
            {
                GUILayout.Label("本工具只负责检查导出清单和必要文件，不再自动执行导出。Videos 目录里的实际 mp4、webm 和临时文件不应进入 unitypackage。", _smallTextStyle);
                if (DrawActionButton("复制建议导出清单")) CopySuggestedExportList();
            });
        }

        /// <summary>
        /// 绘制文档中心页。
        /// </summary>
        private void DrawDocumentationPage()
        {
            DrawPageHeader("文档中心", "查看使用说明、项目结构、验收摘要、发布说明和历史文档。");
            DrawDocumentBrowser(_documentEntries);
        }

        /// <summary>
        /// 绘制更新日志页。
        /// </summary>
        private void DrawChangeLogPage()
        {
            DrawPageHeader("更新日志", "默认显示简化版更新摘要，可按需打开完整日志。");
            if (!_changeLogLoaded) ReloadChangeLog();
            if (!string.IsNullOrEmpty(_changeLogError))
            {
                DrawInfoBox(_changeLogError, MessageType.Warning);
            }

            if (!_changeLogLoaded)
            {
                EditorGUILayout.HelpBox("更新日志尚未加载。点击刷新更新日志后读取文件。", MessageType.Info);
                return;
            }

            EditorGUILayout.BeginHorizontal();
            if (DrawActionButton("查看完整日志", GUILayout.Width(120f)))
            {
                OpenPath(_fullChangeLogPath);
            }

            if (DrawActionButton("打开完整日志所在目录", GUILayout.Width(160f)))
            {
                RevealPath(_fullChangeLogPath);
            }

            EditorGUILayout.EndHorizontal();
            DrawMarkdownViewer("更新日志摘要", "RecorderSdkChangeLogSummary.md", _changeLogPath, _changeLogText, ref _changeLogScroll, ReloadChangeLog);
        }

        /// <summary>
        /// 绘制验证记录页。
        /// </summary>
        private void DrawValidationPage()
        {
            DrawPageHeader("验证记录", "默认显示验证摘要，可切换查看完整运行时验证和 UnityPackage 验证。");
            _selectedValidationTab = GUILayout.Toolbar(_selectedValidationTab, new[] { "验证摘要", "完整运行时验证", "完整 UnityPackage 验证" }, GUILayout.Height(28f));
            GUILayout.Space(6f);

            string path = _selectedValidationTab == 0 ? _validationSummaryPath : _selectedValidationTab == 1 ? _runtimeValidationPath : _unityPackageValidationPath;
            string title = _selectedValidationTab == 0 ? "验证摘要" : _selectedValidationTab == 1 ? "运行时验证记录" : "UnityPackage 验证记录";
            string fileName = _selectedValidationTab == 0 ? "RecorderSdkValidationSummary.md" : _selectedValidationTab == 1 ? "RecorderSdkRuntimeValidation.md" : "RecorderSdkUnityPackageValidation.md";
            if (!_documentCache.TryGetValue(path, out var cached))
            {
                cached = LoadDocumentToCache(path);
            }

            DrawMarkdownViewer(title, fileName, path, cached.content, ref _validationScroll, () => ReloadDocumentCache(path));
        }

        /// <summary>
        /// 绘制关于页。
        /// </summary>
        private void DrawAboutPage()
        {
            DrawPageHeader("关于 SDK", "Recorder SDK v0.3.0-beta 的能力边界和推荐入口。");
            DrawTwoColumnLayout(
                () => DrawCard("基本信息", "版本和平台信息。", () =>
                {
                    DrawInfo("名称", "Recorder SDK");
                    DrawInfo("版本", SDK_VERSION);
                    DrawInfo("当前阶段", "Beta");
                    DrawInfo("支持平台", "Windows / Linux");
                    DrawInfo("作者/维护者", "wangbaiyan");
                }),
                () => DrawCard("推荐入口", "建议新用户优先打开的场景和工具。", () =>
                {
                    DrawInfo("屏幕录制", "录制器_屏幕录制");
                    DrawInfo("设置中心", "录制器_设置中心");
                    DrawInfo("控制台", "Recorder SDK 控制台");
                }));

            GUILayout.Space(8f);
            DrawCard("能力说明", "当前 Beta 版本已提供的主要能力。", () =>
            {
                GUILayout.Label("本地录制、系统音频录制、推流、配置管理、SessionHistory、Editor 工具、unitypackage 分发检查。", _smallTextStyle);
            });
        }

        /// <summary>
        /// 绘制文档选择和内容区。
        /// </summary>
        private void DrawDocumentBrowser(List<DocumentEntry> documents)
        {
            if (documents == null || documents.Count == 0)
            {
                EditorGUILayout.HelpBox("未配置文档列表。", MessageType.Warning);
                return;
            }

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("刷新文档缓存", GUILayout.Width(120f), GUILayout.Height(28f))) RefreshDocumentCache();
            GUILayout.Label("文档内容仅在点击时读取，已读取内容会按文件修改时间缓存。", _mutedStyle);
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(6f);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical(GUILayout.Width(260f));
            _documentListScroll = EditorGUILayout.BeginScrollView(_documentListScroll, GUILayout.Height(Mathf.Max(260f, position.height - 150f)));
            string lastCategory = string.Empty;
            for (int i = 0; i < documents.Count; i++)
            {
                var document = documents[i];
                if (lastCategory != document.category)
                {
                    lastCategory = document.category;
                    GUILayout.Space(4f);
                    GUILayout.Label(lastCategory, _sectionStyle);
                }

                GUI.enabled = _selectedDocumentIndex != i;
                if (GUILayout.Button(document.title, GUILayout.Height(26f)))
                {
                    _selectedDocumentIndex = i;
                    RefreshDocumentContent();
                }

                GUI.enabled = true;
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical();
            if (_selectedDocumentIndex < 0)
            {
                DrawSection("请选择文档");
                EditorGUILayout.HelpBox("左侧只初始化文档索引，点击某个文档后才会读取内容。", MessageType.Info);
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
                return;
            }

            var selected = documents[Mathf.Clamp(_selectedDocumentIndex, 0, documents.Count - 1)];
            DrawSection(selected.title);
            GUILayout.Label(selected.fileName, _mutedStyle);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("打开文件所在目录", GUILayout.Width(140f))) RevealDocument(selected);
            if (GUILayout.Button("用默认编辑器打开", GUILayout.Width(140f))) OpenDocument(selected);
            EditorGUILayout.EndHorizontal();
            string documentContent = RemoveRepeatedDocumentHeading(selected.title, _selectedDocumentContent);
            DrawScrollableText(documentContent, ref _documentContentScroll, Mathf.Max(260f, position.height - 230f), Mathf.Max(360f, position.width - 500f));
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// 绘制单个文档内容。
        /// </summary>
        private void DrawSingleDocument(string path, string title, string fileName)
        {
            if (!_documentCache.TryGetValue(path, out var cached))
            {
                cached = LoadDocumentToCache(path);
            }

            DrawMarkdownViewer(title, fileName, path, cached.content, ref _documentContentScroll, () => ReloadDocumentCache(path));
        }

        /// <summary>
        /// 绘制统一 Markdown 文档查看器。
        /// </summary>
        private void DrawMarkdownViewer(string title, string fileName, string path, string content, ref Vector2 scrollPosition, Action refreshAction)
        {
            DrawSection(title);
            GUILayout.Label(fileName, _mutedStyle);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(title == "设置中心更新日志" ? "刷新更新日志" : "刷新文档缓存", GUILayout.Width(120f))) refreshAction?.Invoke();
            if (GUILayout.Button("用默认编辑器打开", GUILayout.Width(140f))) OpenPath(path);
            if (GUILayout.Button("打开所在目录", GUILayout.Width(120f))) RevealPath(path);
            EditorGUILayout.EndHorizontal();

            float scrollHeight = Mathf.Max(220f, position.height - 190f);
            string viewerContent = RemoveRepeatedDocumentHeading(title, content);
            DrawScrollableText(viewerContent, ref scrollPosition, scrollHeight, Mathf.Max(420f, position.width - 260f));
        }

        /// <summary>
        /// 绘制稳定高度的文本滚动区域，避免 SelectableLabel 自动撑高导致底部不可达。
        /// </summary>
        private void DrawScrollableText(string content, ref Vector2 scrollPosition, float scrollHeight, float contentWidth)
        {
            string safeContent = content ?? string.Empty;
            float width = Mathf.Max(300f, contentWidth);
            var guiContent = new GUIContent(safeContent);
            float textHeight = Mathf.Max(24f, _markdownStyle.CalcHeight(guiContent, width) + 40f);
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, true, true, GUILayout.Height(scrollHeight), GUILayout.ExpandWidth(true));
            EditorGUILayout.SelectableLabel(safeContent, _markdownStyle, GUILayout.Width(width), GUILayout.Height(textHeight));
            EditorGUILayout.EndScrollView();
        }

        /// <summary>
        /// 绘制 SDK API 开发者文档内容。
        /// </summary>
        private void DrawSdkApiDocument(string title, string content, ref Vector2 scrollPosition, float scrollHeight)
        {
            string safeContent = RemoveRepeatedDocumentHeading(title, content ?? string.Empty);
            float width = Mathf.Max(360f, position.width - 470f);
            var guiContent = new GUIContent(safeContent);
            float textHeight = Mathf.Max(120f, _markdownStyle.CalcHeight(guiContent, width) + 48f);
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, true, true, GUILayout.Height(scrollHeight), GUILayout.ExpandWidth(true));
            EditorGUILayout.SelectableLabel(safeContent, _markdownStyle, GUILayout.Width(width), GUILayout.Height(textHeight));
            EditorGUILayout.EndScrollView();
        }

        /// <summary>
        /// 移除正文开头与页面标题重复的 Markdown 标题。
        /// </summary>
        private static string RemoveRepeatedDocumentHeading(string title, string content)
        {
            if (string.IsNullOrWhiteSpace(content)) return "暂无内容。";
            string normalized = content.Replace("\r\n", "\n");
            string[] lines = normalized.Split('\n');
            int start = 0;
            while (start < lines.Length && string.IsNullOrWhiteSpace(lines[start])) start++;
            while (start < lines.Length && IsRepeatedDocumentHeading(title, lines[start])) start++;
            return string.Join("\n", lines.Skip(start)).Trim();
        }

        /// <summary>
        /// 判断一行是否是当前内容区已经展示过的标题。
        /// </summary>
        private static bool IsRepeatedDocumentHeading(string title, string line)
        {
            if (string.IsNullOrWhiteSpace(line)) return false;

            string trimmed = line.Trim();
            if (string.Equals(trimmed, title, StringComparison.OrdinalIgnoreCase)) return true;

            string headingText = GetMarkdownHeadingText(trimmed, out int headingLevel);
            if (string.IsNullOrWhiteSpace(headingText)) return false;
            if (string.Equals(headingText, title, StringComparison.OrdinalIgnoreCase)) return true;

            return headingLevel == 1
                && headingText.StartsWith("Recorder SDK", StringComparison.OrdinalIgnoreCase)
                && !string.IsNullOrWhiteSpace(title);
        }

        /// <summary>
        /// 提取 Markdown 标题文本，仅用于显示层去重。
        /// </summary>
        private static string GetMarkdownHeadingText(string line, out int level)
        {
            level = 0;
            if (string.IsNullOrWhiteSpace(line) || line[0] != '#') return string.Empty;

            while (level < line.Length && line[level] == '#') level++;
            if (level == 0 || level > 6 || level >= line.Length || line[level] != ' ') return string.Empty;
            return line.Substring(level + 1).Trim();
        }

        /// <summary>
        /// 执行基础环境检查。
        /// </summary>
        private void RunBasicEnvironmentChecks()
        {
            _environmentChecks.Clear();
            _currentConfigIdCache = ReadCurrentConfigId();
            AddPathCheck(_environmentChecks, "ffmpeg.exe 是否存在", GetDefaultFFmpegPath(), false);
            AddPathCheck(_environmentChecks, "WASAPILoopbackRecorder.dll 是否存在", ResolveAssetPath("Plugins/Windows/x86_64/WASAPILoopbackRecorder.dll"), false);
            AddPathCheck(_environmentChecks, "StreamingAssets/FFmpegTools/Configs 是否存在", GetConfigsDirectory(), true);
            AddConfigJsonCheck();
            AddCurrentConfigCheck();
            AddOutputDirectoryCheck();
            _lastEnvironmentCheckTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }

        /// <summary>
        /// 执行完整环境检查，包含 FFmpeg 命令探测。
        /// </summary>
        private void RunFullEnvironmentChecks()
        {
            RunBasicEnvironmentChecks();
            string ffmpegPath = GetDefaultFFmpegPath();
            if (!File.Exists(ffmpegPath))
            {
                _environmentChecks.Add(CheckItem.Error("ffmpeg -version 是否可执行", "未找到 FFmpeg，无法执行命令检查。"));
                AddPlatformAudioDeviceCheckUnavailable();
                _environmentChecks.Add(CheckItem.Warning("ffmpeg -filters 是否支持 alimiter", "未找到 FFmpeg，无法检测 alimiter。"));
                return;
            }

            string versionOutput = RunProcess(ffmpegPath, "-version", 5000, out bool versionOk);
            _environmentChecks.Add(versionOk
                ? CheckItem.Ok("ffmpeg -version 是否可执行", FirstLine(versionOutput))
                : CheckItem.Error("ffmpeg -version 是否可执行", versionOutput));

            string devicesOutput = RunProcess(ffmpegPath, "-hide_banner -devices", 8000, out bool devicesOk);
            AddPlatformAudioDeviceCheck(devicesOutput, devicesOk);

            string filtersOutput = RunProcess(ffmpegPath, "-hide_banner -filters", 8000, out bool filtersOk);
            bool hasAlimiter = filtersOk && filtersOutput.IndexOf("alimiter", StringComparison.OrdinalIgnoreCase) >= 0;
            _environmentChecks.Add(hasAlimiter
                ? CheckItem.Ok("ffmpeg -filters 是否支持 alimiter", "已检测到 alimiter。")
                : CheckItem.Warning("ffmpeg -filters 是否支持 alimiter", filtersOk ? "未检测到 alimiter，音量增强 limiter 会降级。" : filtersOutput));
        }

        /// <summary>
        /// 根据当前 Editor 平台显示对应的 FFmpeg 实时音频输入能力检查。
        /// </summary>
        private void AddPlatformAudioDeviceCheck(string devicesOutput, bool devicesOk)
        {
            if (IsWindowsEditorPlatform())
            {
                bool hasWasapi = devicesOk && devicesOutput.IndexOf("wasapi", StringComparison.OrdinalIgnoreCase) >= 0;
                _environmentChecks.Add(hasWasapi
                    ? CheckItem.Ok("FFmpeg wasapi 输入支持", "支持：Windows 推流音频可用。")
                    : CheckItem.Warning("FFmpeg wasapi 输入支持", devicesOk ? "不支持：Windows 推流音频不可用，但本地 MP4/WebM 录屏音频仍可能通过 WASAPILoopbackRecorder.dll + Merge 可用。" : devicesOutput));
                return;
            }

            if (IsLinuxEditorPlatform())
            {
                bool hasPulse = devicesOk && devicesOutput.IndexOf("pulse", StringComparison.OrdinalIgnoreCase) >= 0;
                bool hasAlsa = devicesOk && devicesOutput.IndexOf("alsa", StringComparison.OrdinalIgnoreCase) >= 0;
                bool hasPipeWire = devicesOk && devicesOutput.IndexOf("pipewire", StringComparison.OrdinalIgnoreCase) >= 0;
                bool hasLinuxAudio = hasPulse || hasAlsa || hasPipeWire;
                _environmentChecks.Add(hasLinuxAudio
                    ? CheckItem.Ok("FFmpeg Linux 音频输入支持", $"检测到实时音频输入：{BuildLinuxAudioDeviceSummary(hasPulse, hasAlsa, hasPipeWire)}。Linux/国产系统推流音频按当前系统音频源与 FFmpeg 编译能力判断。")
                    : CheckItem.Warning("FFmpeg Linux 音频输入支持", devicesOk ? "未检测到 pulse/alsa/pipewire 输入；Linux/国产系统推流音频可能不可用，但不会使用 wasapi 判断。" : devicesOutput));
            }
        }

        /// <summary>
        /// FFmpeg 不可用时，按当前 Editor 平台显示对应提示。
        /// </summary>
        private void AddPlatformAudioDeviceCheckUnavailable()
        {
            if (IsWindowsEditorPlatform())
            {
                _environmentChecks.Add(CheckItem.Warning("FFmpeg wasapi 输入支持", "未找到 FFmpeg，无法检测 Windows 推流音频能力。本地录屏音频仍可能通过 WASAPILoopbackRecorder.dll 可用。"));
            }
            else if (IsLinuxEditorPlatform())
            {
                _environmentChecks.Add(CheckItem.Warning("FFmpeg Linux 音频输入支持", "未找到 FFmpeg，无法检测 pulse/alsa/pipewire。Linux/国产系统不会使用 wasapi 判断。"));
            }
        }

        private static string BuildLinuxAudioDeviceSummary(bool hasPulse, bool hasAlsa, bool hasPipeWire)
        {
            var parts = new List<string>();
            if (hasPulse) parts.Add("pulse");
            if (hasAlsa) parts.Add("alsa");
            if (hasPipeWire) parts.Add("pipewire");
            return parts.Count > 0 ? string.Join(" / ", parts) : "未检测到";
        }

        private static bool IsWindowsEditorPlatform()
        {
            return Application.platform == RuntimePlatform.WindowsEditor;
        }

        private static bool IsLinuxEditorPlatform()
        {
            return Application.platform == RuntimePlatform.LinuxEditor;
        }

        /// <summary>
        /// 执行打包文件检查。
        /// </summary>
        private void RunPackageChecks()
        {
            _packageChecks.Clear();
            AddPathCheck(_packageChecks, "Recorder SDK 根目录（必选）", _sdkRoot, true);
            AddSdkRootStructureCheck();
            AddPathCheck(_packageChecks, "Configs（必选）", GetConfigsDirectory(), true);
            if (_includeFFmpegApp) AddPathCheck(_packageChecks, "FFmpegApp", GetFFmpegAppDirectory(), true);
            AddPathCheck(_packageChecks, "WASAPILoopbackRecorder.dll（正式包必须存在）", ResolveAssetPath("Plugins/Windows/x86_64/WASAPILoopbackRecorder.dll"), false);
            if (_includeDocumentation) AddPathCheck(_packageChecks, "Documentation", ResolveAssetPath("Documentation"), true);
            if (_includeDemoScenes) AddPathCheck(_packageChecks, "Demo 场景目录", ResolveAssetPath("Demo/Scenes"), true);
            if (_includeVideosReadme) AddPathCheck(_packageChecks, "Videos 空目录说明", GetVideosReadmePath(), false);
            AddConfigsCleanCheck();
            _lastPackageCheckTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }

        /// <summary>
        /// 复制建议 unitypackage 导出清单。
        /// </summary>
        private void CopySuggestedExportList()
        {
            var exportPaths = BuildUnityPackageExportPaths();
            EditorGUIUtility.systemCopyBuffer = string.Join(Environment.NewLine, exportPaths);
            ShowNotification(new GUIContent("已复制建议导出清单"));
        }

        /// <summary>
        /// 重新扫描 SDK 根目录并刷新相关检查。
        /// </summary>
        private void RescanSdkRoot()
        {
            EditorPrefs.DeleteKey(SDK_ROOT_EDITOR_PREFS_KEY);
            _sdkRoot = DetectSdkRootFromScript();
            _sdkRootSource = IsValidSdkRoot(_sdkRoot) ? "脚本反推" : "fallback";
            if (!IsValidSdkRoot(_sdkRoot)) _sdkRoot = PREFERRED_SDK_ROOT;
            _documentationRoot = ResolveDocumentationRoot();
            _documentEntries = CreateDocumentEntries();
            RunPackageChecks();
        }

        /// <summary>
        /// 手动选择 SDK 根目录并保存到 EditorPrefs。
        /// </summary>
        private void SelectSdkRoot()
        {
            string currentPath = AssetPathToAbsolutePath(_sdkRoot);
            string selectedPath = EditorUtility.OpenFolderPanel("选择 Recorder SDK 根目录", currentPath, string.Empty);
            if (string.IsNullOrEmpty(selectedPath)) return;

            string assetPath = AbsolutePathToAssetPath(selectedPath);
            if (string.IsNullOrEmpty(assetPath))
            {
                EditorUtility.DisplayDialog("路径无效", "请选择当前 Unity 项目 Assets 目录下的 RecorderSdk 根目录。", "知道了");
                return;
            }

            _sdkRoot = assetPath;
            _sdkRootSource = "手动选择";
            EditorPrefs.SetString(SDK_ROOT_EDITOR_PREFS_KEY, _sdkRoot);
            _documentationRoot = ResolveDocumentationRoot();
            _documentEntries = CreateDocumentEntries();
            RunPackageChecks();
        }

        /// <summary>
        /// 添加路径存在性检查。
        /// </summary>
        private static void AddPathCheck(List<CheckItem> list, string title, string path, bool isDirectory)
        {
            bool exists = isDirectory ? Directory.Exists(path) : File.Exists(path);
            list.Add(exists ? CheckItem.Ok(title, path) : CheckItem.Error(title, "未找到：" + path));
        }

        /// <summary>
        /// 检查 SDK 根目录是否保持标准内部结构。
        /// </summary>
        private void AddSdkRootStructureCheck()
        {
            string warning = GetSdkRootStructureWarning();
            _packageChecks.Add(string.IsNullOrEmpty(warning)
                ? CheckItem.Ok("SDK 内部目录结构", "Core / UI / Editor / Demo / Documentation / Plugins 目录可识别。")
                : CheckItem.Warning("SDK 内部目录结构", warning));
        }

        /// <summary>
        /// 添加配置 JSON 解析检查。
        /// </summary>
        private void AddConfigJsonCheck()
        {
            string configsDir = GetConfigsDirectory();
            if (!Directory.Exists(configsDir))
            {
                _environmentChecks.Add(CheckItem.Error("Config JSON 是否能解析", "Configs 目录不存在。"));
                return;
            }

            var files = Directory.GetFiles(configsDir, "*.json", SearchOption.TopDirectoryOnly);
            int invalidCount = 0;
            foreach (string file in files)
            {
                try
                {
                    string json = File.ReadAllText(file, Encoding.UTF8);
                    JsonUtility.FromJson<JsonProbe>(json);
                }
                catch (Exception ex)
                {
                    invalidCount++;
                    Debug.LogWarning(Path.GetFileName(file) + " 解析失败：" + ex.Message);
                }
            }

            _environmentChecks.Add(invalidCount == 0
                ? CheckItem.Ok("Config JSON 是否能解析", "共检查 " + files.Length + " 个 JSON。")
                : CheckItem.Error("Config JSON 是否能解析", "解析失败数量：" + invalidCount));
        }

        /// <summary>
        /// 添加当前配置 ID 检查。
        /// </summary>
        private void AddCurrentConfigCheck()
        {
            string configsDir = GetConfigsDirectory();
            if (!Directory.Exists(configsDir))
            {
                _environmentChecks.Add(CheckItem.Error("当前配置 currentConfigId 是否有效", "Configs 目录不存在。"));
                return;
            }

            string useConfigName = Application.platform == RuntimePlatform.LinuxEditor ? "UseLinuxRecordConfig.json" : "UseWinRecordConfig.json";
            string useConfigPath = Path.Combine(configsDir, useConfigName);
            if (!File.Exists(useConfigPath))
            {
                _environmentChecks.Add(CheckItem.Warning("当前配置 currentConfigId 是否有效", "未找到：" + useConfigName));
                return;
            }

            string currentConfigId = ExtractJsonString(File.ReadAllText(useConfigPath, Encoding.UTF8), "currentConfigId");
            if (string.IsNullOrEmpty(currentConfigId))
            {
                _environmentChecks.Add(CheckItem.Warning("当前配置 currentConfigId 是否有效", "currentConfigId 为空。"));
                return;
            }

            bool exists = Directory.GetFiles(configsDir, "*.json", SearchOption.TopDirectoryOnly)
                .Any(file => ExtractJsonString(File.ReadAllText(file, Encoding.UTF8), "configId") == currentConfigId);
            _environmentChecks.Add(exists
                ? CheckItem.Ok("当前配置 currentConfigId 是否有效", currentConfigId)
                : CheckItem.Error("当前配置 currentConfigId 是否有效", "未找到 configId：" + currentConfigId));
        }

        /// <summary>
        /// 添加输出目录可写检查。
        /// </summary>
        private void AddOutputDirectoryCheck()
        {
            string videosDir = Path.Combine(STREAMING_ASSETS_ROOT, "Videos").Replace("\\", "/");
            if (!Directory.Exists(videosDir))
            {
                _environmentChecks.Add(CheckItem.Warning("输出目录是否可写", "输出目录不存在：" + videosDir));
                return;
            }

            try
            {
                string tempFile = Path.Combine(videosDir, ".recorder_write_test.tmp");
                File.WriteAllText(tempFile, "test");
                File.Delete(tempFile);
                _environmentChecks.Add(CheckItem.Ok("输出目录是否可写", videosDir));
            }
            catch (Exception ex)
            {
                _environmentChecks.Add(CheckItem.Error("输出目录是否可写", ex.Message));
            }
        }

        /// <summary>
        /// 添加配置目录干净度检查。
        /// </summary>
        private void AddConfigsCleanCheck()
        {
            string configsDir = GetConfigsDirectory();
            if (!Directory.Exists(configsDir))
            {
                _packageChecks.Add(CheckItem.Error("Configs 是否干净", "Configs 目录不存在。"));
                return;
            }

            var files = Directory.GetFiles(configsDir, "*.json", SearchOption.TopDirectoryOnly);
            bool hasLegacy = files.Any(file =>
            {
                string json = File.ReadAllText(file, Encoding.UTF8);
                return json.Contains("\"fileName\"") || json.Contains("\"configName\"");
            });

            _packageChecks.Add(hasLegacy
                ? CheckItem.Warning("Configs 是否干净", "检测到 Legacy 字段 fileName/configName。")
                : CheckItem.Ok("Configs 是否干净", "未检测到 Legacy 身份字段。"));
        }

        /// <summary>
        /// 绘制检查项列表。
        /// </summary>
        private void DrawCheckItems(List<CheckItem> checks)
        {
            if (checks == null || checks.Count == 0)
            {
                EditorGUILayout.HelpBox("尚未执行检查。", MessageType.Info);
                return;
            }

            foreach (var item in checks)
            {
                DrawCheckItemCard(item);
            }
        }

        /// <summary>
        /// 绘制检查项表格。
        /// </summary>
        private void DrawCheckTable(List<CheckItem> checks)
        {
            if (checks == null || checks.Count == 0)
            {
                DrawCheckItemCard(CheckItem.Warning("未检测", "请点击重新检查。"));
                return;
            }

            foreach (var item in checks)
            {
                DrawCheckItemCard(item);
            }
        }

        /// <summary>
        /// 绘制可换行的检查项卡片，避免长路径和命令输出在横向表格中被裁切。
        /// </summary>
        private void DrawCheckItemCard(CheckItem item)
        {
            EditorGUILayout.BeginVertical("box", GUILayout.ExpandWidth(true));
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(item.title, _sectionStyle, GUILayout.ExpandWidth(true));
            GUILayout.Label(GetCheckLevelText(item.level), GetStatusStyle(item.level), GUILayout.Width(70f));
            EditorGUILayout.EndHorizontal();

            if (!string.IsNullOrEmpty(item.message))
            {
                DrawWrappedText(item.message);
            }

            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// 绘制自动换行文本。
        /// </summary>
        private void DrawWrappedText(string text)
        {
            var style = new GUIStyle(_smallTextStyle)
            {
                wordWrap = true,
                clipping = TextClipping.Overflow
            };

            EditorGUILayout.LabelField(text ?? string.Empty, style, GUILayout.ExpandWidth(true));
        }

        /// <summary>
        /// 绘制页面标题和说明。
        /// </summary>
        private void DrawPageHeader(string title, string description)
        {
            GUILayout.Label(title, _headerStyle);
            if (!string.IsNullOrEmpty(description)) GUILayout.Label(description, _smallTextStyle);
            GUILayout.Space(10f);
        }

        /// <summary>
        /// 绘制标题。
        /// </summary>
        private void DrawTitle(string title)
        {
            GUILayout.Label(title, _headerStyle);
            GUILayout.Space(8f);
        }

        /// <summary>
        /// 绘制分组标题。
        /// </summary>
        private void DrawSection(string title)
        {
            GUILayout.Label(title, _sectionStyle);
        }

        /// <summary>
        /// 绘制分区标题。
        /// </summary>
        private void DrawSectionTitle(string title)
        {
            GUILayout.Label(title, _sectionStyle);
            GUILayout.Space(4f);
        }

        /// <summary>
        /// 绘制统一卡片。
        /// </summary>
        private void DrawCard(string title, string description, Action body)
        {
            EditorGUILayout.BeginVertical(_cardStyle, GUILayout.MinHeight(96f), GUILayout.ExpandWidth(true));
            GUILayout.Label(title, _sectionStyle);
            if (!string.IsNullOrEmpty(description)) GUILayout.Label(description, _smallTextStyle);
            GUILayout.Space(6f);
            body?.Invoke();
            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// 绘制首页主区域。
        /// </summary>
        private void DrawMainCard(string title, string description, Action body)
        {
            EditorGUILayout.BeginVertical(_mainCardStyle, GUILayout.MinHeight(150f), GUILayout.ExpandWidth(true));
            GUILayout.Label(title, _sectionStyle);
            if (!string.IsNullOrEmpty(description)) GUILayout.Label(description, _smallTextStyle);
            GUILayout.Space(10f);
            body?.Invoke();
            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// 绘制首页主操作组。
        /// </summary>
        private void DrawHomeActionGroup(string title, string description, Action body)
        {
            EditorGUILayout.BeginVertical(_cardStyle, GUILayout.MinHeight(112f), GUILayout.ExpandWidth(true));
            GUILayout.Label(title, _sectionStyle);
            GUILayout.Label(description, _smallTextStyle);
            GUILayout.Space(8f);
            body?.Invoke();
            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// 绘制首页底部最近更新摘要。
        /// </summary>
        private void DrawRecentUpdateSummary()
        {
            GUILayout.Label("v0.3.0-beta", _sectionStyle);
            GUILayout.Label("- 完成控制台重构\n- 完成 SDK API 页面\n- 完成打包检查\n- 完成布局管理", _smallTextStyle);
        }

        /// <summary>
        /// 绘制状态文本。
        /// </summary>
        private void DrawStatusBadge(string status, CheckLevel level)
        {
            GUILayout.Label(status, GetStatusStyle(level), GUILayout.Width(70f));
        }

        /// <summary>
        /// 绘制操作按钮。
        /// </summary>
        private bool DrawActionButton(string text, params GUILayoutOption[] options)
        {
            return GUILayout.Button(text, options.Length > 0 ? options : new[] { GUILayout.Width(170f), GUILayout.Height(28f) });
        }

        /// <summary>
        /// 绘制说明框。
        /// </summary>
        private void DrawInfoBox(string text, MessageType type = MessageType.Info)
        {
            EditorGUILayout.HelpBox(text, type);
        }

        /// <summary>
        /// 绘制双列布局。
        /// </summary>
        private void DrawTwoColumnLayout(Action left, Action right)
        {
            EditorGUILayout.BeginHorizontal();
            left?.Invoke();
            GUILayout.Space(8f);
            right?.Invoke();
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// 绘制表格标题。
        /// </summary>
        private void DrawTableHeader(string item, string status, string result, string operation)
        {
            EditorGUILayout.BeginHorizontal("box");
            GUILayout.Label(item, _tableHeaderStyle, GUILayout.Width(210f));
            GUILayout.Label(status, _tableHeaderStyle, GUILayout.Width(80f));
            GUILayout.Label(result, _tableHeaderStyle, GUILayout.MinWidth(320f));
            GUILayout.Label(operation, _tableHeaderStyle, GUILayout.Width(80f));
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// 绘制表格行。
        /// </summary>
        private void DrawTableRow(string item, CheckLevel level, string result, Action action)
        {
            EditorGUILayout.BeginHorizontal("box");
            GUILayout.Label(item, GUILayout.Width(210f));
            DrawStatusBadge(GetCheckLevelText(level), level);
            GUILayout.Label(result ?? string.Empty, _smallTextStyle, GUILayout.MinWidth(320f));
            if (action != null)
            {
                if (GUILayout.Button("查看", GUILayout.Width(70f))) action.Invoke();
            }
            else
            {
                GUILayout.Label("-", _mutedStyle, GUILayout.Width(70f));
            }

            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// 绘制状态行。
        /// </summary>
        private void DrawStatusLine(string label, string status, CheckLevel level)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(label, GUILayout.Width(95f));
            DrawStatusBadge(status, level);
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// 绘制键值信息。
        /// </summary>
        private void DrawInfo(string label, string value)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(label, GUILayout.Width(160f));
            EditorGUILayout.SelectableLabel(value ?? string.Empty, GUILayout.Height(18f));
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// 打开指定示例场景。
        /// </summary>
        private void OpenSceneByName(string sceneName)
        {
            string[] guids = AssetDatabase.FindAssets(sceneName + " t:Scene");
            if (guids == null || guids.Length == 0)
            {
                EditorUtility.DisplayDialog("未找到场景", "未找到场景：" + sceneName, "知道了");
                return;
            }

            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                EditorSceneManager.OpenScene(path);
            }
        }

        /// <summary>
        /// 处理控制台主页面切换时的一次性加载。
        /// </summary>
        private void OnSelectedTabChanged()
        {
            if (_selectedTab == 7 && !_changeLogLoaded)
            {
                ReloadChangeLog();
            }
        }

        /// <summary>
        /// 绘制 API 参数子页签。
        /// </summary>
        private void DrawApiTabButton(int index, string title)
        {
            if (GUILayout.Toggle(_selectedApiTab == index, title, "Button", GUILayout.Height(30f)))
            {
                _selectedApiTab = index;
            }
        }

        /// <summary>
        /// 确保 FFmpeg 参数内容已初始化。
        /// </summary>
        private void EnsureFFmpegGuideContent()
        {
            if (_ffmpegGuideContent == null) _ffmpegGuideContent = new FFmpegParameterGuideContent();
            if (_ffmpegGuideInitialized) return;
            _ffmpegGuideContent.Initialize();
            _ffmpegGuideInitialized = true;
        }

        /// <summary>
        /// 确保视频编码参数内容已初始化。
        /// </summary>
        private void EnsureVideoEncodingGuideContent()
        {
            if (_videoEncodingGuideContent == null) _videoEncodingGuideContent = new VideoEncodingGuideContent();
            if (_videoEncodingGuideInitialized) return;
            _videoEncodingGuideContent.Initialize();
            _videoEncodingGuideInitialized = true;
        }

        /// <summary>
        /// 刷新当前 API 页缓存。
        /// </summary>
        private void RefreshCurrentApiCache()
        {
            if (_selectedApiTab == 0)
            {
                _ffmpegGuideContent = new FFmpegParameterGuideContent();
                _ffmpegGuideContent.Initialize();
                _ffmpegGuideInitialized = true;
                return;
            }

            _videoEncodingGuideContent = new VideoEncodingGuideContent();
            _videoEncodingGuideContent.Initialize();
            _videoEncodingGuideInitialized = true;
        }

        /// <summary>
        /// 刷新当前选中文档内容。
        /// </summary>
        private void RefreshDocumentContent()
        {
            if (_documentEntries == null || _documentEntries.Count == 0)
            {
                _selectedDocumentContent = "未配置文档列表。";
                return;
            }

            if (_selectedDocumentIndex < 0)
            {
                _selectedDocumentContent = "请选择左侧文档。";
                return;
            }

            var selected = _documentEntries[Mathf.Clamp(_selectedDocumentIndex, 0, _documentEntries.Count - 1)];
            string path = ResolveDocumentPath(selected.relativePath);
            _selectedDocumentContent = GetDocumentContent(path);
        }

        /// <summary>
        /// 获取带修改时间校验的文档缓存内容。
        /// </summary>
        private string GetDocumentContent(string path)
        {
            if (string.IsNullOrEmpty(path)) return "未找到文档路径。";
            if (!File.Exists(path)) return "未找到：" + path;

            var lastWriteTime = File.GetLastWriteTimeUtc(path);
            if (_documentCache.TryGetValue(path, out var cached) && cached.lastWriteTime == lastWriteTime)
            {
                return cached.content;
            }

            return LoadDocumentToCache(path).content;
        }

        /// <summary>
        /// 读取指定文档并写入缓存。
        /// </summary>
        private CachedDocument LoadDocumentToCache(string path)
        {
            var lastWriteTime = File.Exists(path) ? File.GetLastWriteTimeUtc(path) : DateTime.MinValue;
            string content = ReadDocumentFromDisk(path, "未找到：" + path, out _);
            var cached = new CachedDocument(content, lastWriteTime);
            if (!string.IsNullOrEmpty(path)) _documentCache[path] = cached;
            return cached;
        }

        /// <summary>
        /// 重新读取指定文档缓存。
        /// </summary>
        private void ReloadDocumentCache(string path)
        {
            if (!string.IsNullOrEmpty(path)) _documentCache.Remove(path);
            LoadDocumentToCache(path);
        }

        /// <summary>
        /// 重新加载更新日志缓存。
        /// </summary>
        private void ReloadChangeLog()
        {
            _changeLogLoaded = true;
            _changeLogError = string.Empty;
            if (string.IsNullOrEmpty(_changeLogPath))
            {
                _changeLogText = "未找到更新日志文件。";
                _changeLogError = "更新日志路径为空。";
                return;
            }

            if (!File.Exists(_changeLogPath))
            {
                _changeLogText = "未找到更新日志文件。";
                _changeLogError = "未找到：" + _changeLogPath;
                return;
            }

            _changeLogLastWriteTime = File.GetLastWriteTimeUtc(_changeLogPath);
            _changeLogText = ReadDocumentFromDisk(_changeLogPath, "未找到更新日志文件。", out _changeLogError);
        }

        /// <summary>
        /// 从磁盘读取文档并限制最大显示长度。
        /// </summary>
        private static string ReadDocumentFromDisk(string path, string missingMessage, out string error)
        {
            error = string.Empty;
            try
            {
                if (!File.Exists(path))
                {
                    error = "未找到：" + path;
                    return missingMessage;
                }

                string decoded = DecodeDocumentBytes(File.ReadAllBytes(path));
                string content = decoded.Length > MAX_DOCUMENT_DISPLAY_CHARS ? decoded.Substring(0, MAX_DOCUMENT_DISPLAY_CHARS) : decoded;
                if (decoded.Length <= MAX_DOCUMENT_DISPLAY_CHARS) return content;
                error = "内容过长，已截断显示，可点击打开文件查看完整内容。";
                return content + "\n\n内容过长，已截断显示，可点击打开文件查看完整内容。";
            }
            catch (Exception ex)
            {
                error = "读取失败：" + ex.Message;
                return error;
            }
        }

        /// <summary>
        /// 解码文档内容，兼容 UTF-8、GBK 以及少量历史错码文本。
        /// </summary>
        private static string DecodeDocumentBytes(byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0) return string.Empty;

            string utf8Text = FixMojibakeIfNeeded(Encoding.UTF8.GetString(bytes).TrimStart('\uFEFF'));
            string gbkText = string.Empty;
            try
            {
                gbkText = Encoding.GetEncoding(936).GetString(bytes);
            }
            catch
            {
                return utf8Text;
            }

            return GetMojibakeScore(gbkText) < GetMojibakeScore(utf8Text) ? gbkText : utf8Text;
        }

        /// <summary>
        /// 修复少量历史文档中 UTF-8 被 GBK 错读后保存的中文乱码。
        /// </summary>
        private static string FixMojibakeIfNeeded(string text)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;
            int originalScore = GetMojibakeScore(text);
            if (originalScore < 4) return text;

            try
            {
                string repaired = Encoding.UTF8.GetString(Encoding.GetEncoding(936).GetBytes(text));
                return GetMojibakeScore(repaired) < originalScore ? repaired : text;
            }
            catch
            {
                return text;
            }
        }

        /// <summary>
        /// 粗略识别常见中文错码片段，只在读取文档时执行。
        /// </summary>
        private static int GetMojibakeScore(string text)
        {
            if (string.IsNullOrEmpty(text)) return 0;
            string[] markers =
            {
                "銆", "€", "鍙", "鎺", "鏂", "楠", "鐩", "绋", "缁", "灏", "佺", "増", "馃"
            };

            int score = text.Count(ch => ch == '�');
            foreach (string marker in markers)
            {
                int index = -marker.Length;
                while ((index = text.IndexOf(marker, index + marker.Length, StringComparison.Ordinal)) >= 0)
                {
                    score++;
                }
            }

            return score;
        }

        /// <summary>
        /// 刷新全部文档缓存并重新读取当前文档。
        /// </summary>
        private void RefreshDocumentCache()
        {
            _documentCache.Clear();
            RefreshDocumentContent();
        }

        /// <summary>
        /// 刷新指定文档缓存。
        /// </summary>
        private void RefreshDocumentCache(string path)
        {
            if (!string.IsNullOrEmpty(path)) _documentCache.Remove(path);
        }

        /// <summary>
        /// 显示指定文档所在目录。
        /// </summary>
        private void RevealDocument(DocumentEntry entry)
        {
            RevealPath(ResolveDocumentPath(entry.relativePath));
        }

        /// <summary>
        /// 使用系统默认编辑器打开指定文档。
        /// </summary>
        private void OpenDocument(DocumentEntry entry)
        {
            OpenPath(ResolveDocumentPath(entry.relativePath));
        }

        /// <summary>
        /// 显示路径所在目录。
        /// </summary>
        private static void RevealPath(string path)
        {
            if (File.Exists(path) || Directory.Exists(path))
            {
                EditorUtility.RevealInFinder(path);
                return;
            }

            EditorUtility.DisplayDialog("未找到", "未找到：" + path, "知道了");
        }

        /// <summary>
        /// 使用默认应用打开路径。
        /// </summary>
        private static void OpenPath(string path)
        {
            if (!File.Exists(path) && !Directory.Exists(path))
            {
                EditorUtility.DisplayDialog("未找到", "未找到：" + path, "知道了");
                return;
            }

            EditorUtility.OpenWithDefaultApp(path);
        }

        /// <summary>
        /// 获取默认 FFmpeg 路径。
        /// </summary>
        private static string GetDefaultFFmpegPath()
        {
            string fileName = Application.platform == RuntimePlatform.WindowsEditor ? "ffmpeg.exe" : "ffmpeg";
            return Path.Combine(STREAMING_ASSETS_ROOT, "FFmpegApp", fileName).Replace("\\", "/");
        }

        /// <summary>
        /// 获取配置目录。
        /// </summary>
        private static string GetConfigsDirectory()
        {
            return Path.Combine(STREAMING_ASSETS_ROOT, "Configs").Replace("\\", "/");
        }

        /// <summary>
        /// 获取 FFmpegApp 目录。
        /// </summary>
        private static string GetFFmpegAppDirectory()
        {
            return Path.Combine(STREAMING_ASSETS_ROOT, "FFmpegApp").Replace("\\", "/");
        }

        /// <summary>
        /// 获取 Videos 空目录说明文件路径。
        /// </summary>
        private static string GetVideosReadmePath()
        {
            return Path.Combine(STREAMING_ASSETS_ROOT, "Videos", "README.md").Replace("\\", "/");
        }

        /// <summary>
        /// 读取当前平台使用的 configId。
        /// </summary>
        private static string ReadCurrentConfigId()
        {
            try
            {
                string configsDir = GetConfigsDirectory();
                string useConfigName = Application.platform == RuntimePlatform.LinuxEditor ? "UseLinuxRecordConfig.json" : "UseWinRecordConfig.json";
                string useConfigPath = Path.Combine(configsDir, useConfigName);
                if (!File.Exists(useConfigPath)) return string.Empty;
                return ExtractJsonString(File.ReadAllText(useConfigPath, Encoding.UTF8), "currentConfigId");
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// 构建 unitypackage 导出路径，避免包含 Videos 实际录制文件。
        /// </summary>
        private List<string> BuildUnityPackageExportPaths()
        {
            var paths = new List<string>();
            AddExportPathIfExists(paths, ResolveAssetPath("Core"));
            AddExportPathIfExists(paths, ResolveAssetPath("UI"));
            AddExportPathIfExists(paths, ResolveAssetPath("Editor"));
            if (_includeDocumentation) AddExportPathIfExists(paths, ResolveAssetPath("Documentation"));
            if (_includeDemoScenes) AddExportPathIfExists(paths, ResolveAssetPath("Demo"));
            AddExportPathIfExists(paths, ResolveAssetPath("Plugins"));
            AddExportPathIfExists(paths, GetConfigsDirectory());
            if (_includeFFmpegApp) AddExportPathIfExists(paths, GetFFmpegAppDirectory());
            if (_includeVideosReadme) AddExportPathIfExists(paths, GetVideosReadmePath());

            return paths.Distinct().ToList();
        }

        /// <summary>
        /// 添加存在的导出路径。
        /// </summary>
        private static void AddExportPathIfExists(List<string> paths, string path)
        {
            if (string.IsNullOrEmpty(path)) return;
            if (File.Exists(path) || Directory.Exists(path) || AssetDatabase.IsValidFolder(path)) paths.Add(path.Replace("\\", "/"));
        }

        /// <summary>
        /// 解析 SDK 根目录。
        /// </summary>
        private string ResolveSdkRoot()
        {
            string detectedRoot = DetectSdkRootFromScript();
            if (IsValidSdkRoot(detectedRoot))
            {
                _sdkRootSource = "脚本反推";
                return detectedRoot;
            }

            string savedRoot = EditorPrefs.GetString(SDK_ROOT_EDITOR_PREFS_KEY, string.Empty);
            if (IsValidSdkRoot(savedRoot))
            {
                _sdkRootSource = "EditorPrefs";
                return savedRoot;
            }

            string editorPath = ResolveEditorScriptPath();
            if (!string.IsNullOrEmpty(editorPath))
            {
                string parent = Path.GetDirectoryName(Path.GetDirectoryName(editorPath))?.Replace("\\", "/");
                if (IsValidSdkRoot(parent))
                {
                    _sdkRootSource = "脚本父目录";
                    return parent;
                }
            }

            _sdkRootSource = "fallback";
            return PREFERRED_SDK_ROOT;
        }

        /// <summary>
        /// 通过当前 EditorWindow 脚本路径反推 SDK 根目录。
        /// </summary>
        private static string DetectSdkRootFromScript()
        {
            string editorPath = ResolveEditorScriptPath();
            if (string.IsNullOrEmpty(editorPath)) return string.Empty;
            string marker = "/RecorderSdk/Editor/";
            int index = editorPath.IndexOf(marker, StringComparison.Ordinal);
            if (index >= 0) return editorPath.Substring(0, index + "/RecorderSdk".Length);
            string editorDirectory = Path.GetDirectoryName(editorPath)?.Replace("\\", "/");
            return string.IsNullOrEmpty(editorDirectory) ? string.Empty : Path.GetDirectoryName(editorDirectory)?.Replace("\\", "/");
        }

        /// <summary>
        /// 判断指定目录是否可以作为 Recorder SDK 根目录。
        /// </summary>
        private static bool IsValidSdkRoot(string path)
        {
            if (string.IsNullOrEmpty(path)) return false;
            path = path.Replace("\\", "/");
            return AssetDatabase.IsValidFolder(path)
                && AssetDatabase.IsValidFolder(path + "/Core")
                && AssetDatabase.IsValidFolder(path + "/Editor");
        }

        /// <summary>
        /// 获取 SDK 内部标准目录缺失提示。
        /// </summary>
        private string GetSdkRootStructureWarning()
        {
            if (string.IsNullOrEmpty(_sdkRoot) || !AssetDatabase.IsValidFolder(_sdkRoot)) return "未找到 SDK 根目录。";
            string[] standardFolders = { "Core", "UI", "Editor", "Demo", "Documentation", "Plugins" };
            var missingFolders = standardFolders.Where(folder => !AssetDatabase.IsValidFolder((_sdkRoot + "/" + folder).Replace("\\", "/"))).ToList();
            return missingFolders.Count == 0
                ? string.Empty
                : "检测到 SDK 内部标准目录缺失或被改名：" + string.Join(", ", missingFolders) + "。不建议修改 Core/UI/Editor/Demo/Documentation/Plugins 等内部目录名。";
        }

        /// <summary>
        /// 解析 Documentation 根目录。
        /// </summary>
        private string ResolveDocumentationRoot()
        {
            string preferred = PREFERRED_SDK_ROOT + "/Documentation";
            if (File.Exists(preferred + "/README.md")) return preferred;
            string detected = ResolveAssetPath("Documentation");
            return Directory.Exists(detected) ? detected : preferred;
        }

        /// <summary>
        /// 解析 SDK 内部资源路径。
        /// </summary>
        private string ResolveAssetPath(string relativePath)
        {
            string detected = (_sdkRoot + "/" + relativePath).Replace("\\", "/");
            if (File.Exists(detected) || Directory.Exists(detected)) return detected;

            string preferred = (PREFERRED_SDK_ROOT + "/" + relativePath).Replace("\\", "/");
            if (File.Exists(preferred) || Directory.Exists(preferred)) return preferred;

            string fileName = Path.GetFileName(relativePath);
            if (!string.IsNullOrEmpty(fileName))
            {
                string[] guids = AssetDatabase.FindAssets(Path.GetFileNameWithoutExtension(fileName));
                foreach (string guid in guids)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid).Replace("\\", "/");
                    if (path.EndsWith(relativePath.Replace("\\", "/"), StringComparison.OrdinalIgnoreCase)) return path;
                }
            }

            return preferred;
        }

        /// <summary>
        /// 解析文档路径。
        /// </summary>
        private string ResolveDocumentPath(string relativePath)
        {
            string detected = (_documentationRoot + "/" + relativePath).Replace("\\", "/");
            if (File.Exists(detected)) return detected;

            string preferred = (PREFERRED_SDK_ROOT + "/Documentation/" + relativePath).Replace("\\", "/");
            if (File.Exists(preferred)) return preferred;

            string fileName = Path.GetFileName(relativePath);
            string[] guids = AssetDatabase.FindAssets(Path.GetFileNameWithoutExtension(fileName));
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid).Replace("\\", "/");
                if (path.EndsWith("/Documentation/" + relativePath.Replace("\\", "/"), StringComparison.OrdinalIgnoreCase)) return path;
            }

            return preferred;
        }

        /// <summary>
        /// 解析当前脚本路径。
        /// </summary>
        private static string ResolveEditorScriptPath()
        {
            string[] guids = AssetDatabase.FindAssets("RecorderSdkHubWindow t:MonoScript");
            return guids != null && guids.Length > 0 ? AssetDatabase.GUIDToAssetPath(guids[0]).Replace("\\", "/") : string.Empty;
        }

        /// <summary>
        /// 将 Assets 相对路径转换为磁盘绝对路径。
        /// </summary>
        private static string AssetPathToAbsolutePath(string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath)) return Application.dataPath;
            string projectRoot = Path.GetDirectoryName(Application.dataPath)?.Replace("\\", "/") ?? string.Empty;
            return Path.Combine(projectRoot, assetPath).Replace("\\", "/");
        }

        /// <summary>
        /// 将磁盘绝对路径转换为 Assets 相对路径。
        /// </summary>
        private static string AbsolutePathToAssetPath(string absolutePath)
        {
            if (string.IsNullOrEmpty(absolutePath)) return string.Empty;
            string normalized = absolutePath.Replace("\\", "/");
            string projectRoot = Path.GetDirectoryName(Application.dataPath)?.Replace("\\", "/") ?? string.Empty;
            if (string.IsNullOrEmpty(projectRoot) || !normalized.StartsWith(projectRoot + "/", StringComparison.OrdinalIgnoreCase)) return string.Empty;
            return normalized.Substring(projectRoot.Length + 1);
        }

        /// <summary>
        /// 创建文档列表。
        /// </summary>
        private static List<DocumentEntry> CreateDocumentEntries()
        {
            return new List<DocumentEntry>
            {
                new("使用说明", "使用说明", "README.md", "README.md"),
                new("项目结构", "项目结构", "ProjectStructure.md", "ProjectStructure.md"),
                new("验收摘要", "验收摘要", "RecorderSdkAcceptanceSummary.md", "Acceptance/RecorderSdkAcceptanceSummary.md"),
                new("稳定性总体验收", "稳定性总体验收", "RecorderSdkStabilityAcceptance.md", "Archive/RecorderSdkStabilityAcceptance.md"),
                new("发布说明", "v0.3.0-beta 发布说明", "ReleaseNotes_v0.3.0-beta.md", "ReleaseNotes/ReleaseNotes_v0.3.0-beta.md"),
                new("验证摘要", "验证摘要", "RecorderSdkValidationSummary.md", "Validation/RecorderSdkValidationSummary.md"),
                new("历史文档", "完整运行时验证记录", "RecorderSdkRuntimeValidation.md", "Validation/RecorderSdkRuntimeValidation.md"),
                new("历史文档", "完整 UnityPackage 验证记录", "RecorderSdkUnityPackageValidation.md", "Validation/RecorderSdkUnityPackageValidation.md"),
                new("历史文档", "完整更新日志", "RecorderSettingsChangeLog.md", "Changelog/RecorderSettingsChangeLog.md"),
                new("历史文档", "第1阶段验收说明", "RecorderSdkPhase1Acceptance.md", "Archive/RecorderSdkPhase1Acceptance.md"),
                new("历史文档", "第2阶段验收说明", "RecorderSdkPhase2Acceptance.md", "Archive/RecorderSdkPhase2Acceptance.md"),
                new("历史文档", "第3阶段验收说明", "RecorderSdkPhase3Acceptance.md", "Archive/RecorderSdkPhase3Acceptance.md")
            };
        }

        /// <summary>
        /// 创建新版 SDK API 说明内容。
        /// </summary>
        private static List<SdkApiDocEntry> CreateDetailedSdkApiEntries()
        {
            return new List<SdkApiDocEntry>
            {
                new("快速接入流程", "从场景对象到 Start / Stop 的最短接入路径。",
@"快速接入流程

1. 在场景中创建 RecorderManager。
2. 给 RecorderManager 挂载 CrossPlatformScreenRecorder。
3. 确认 Configs 中存在当前平台配置，并通过 configId 选择要使用的配置。
4. 调用 StartRecordingAsync() 开始录制或推流。
5. 调用 StopRecordingAsync() 停止录制或推流。
6. 读取 RecorderResult 判断 success、errorCode、message、outputPath。
7. 订阅 OnRecorderStateChanged / OnRecorderError / OnRecorderWarning / OnRecorderProgress 监听状态和错误。
8. 通过 GetSessionHistory() 读取最近会话，用于 UI、日志、上传和排错。

推荐新代码使用 async API。StartRecording() / StopRecording() 主要用于 Unity Button 兼容。"),

                new("核心组件", "Recorder SDK 主要类型和职责。",
@"CrossPlatformScreenRecorder
- Recorder SDK 的 Unity 层入口。
- 负责 Start / Stop、状态流转、事件派发、SessionHistory、调用 FFmpeg 命令。
- 建议挂载在 RecorderManager 对象上。

RecorderConfigRegistry
- 配置注册表。
- 负责扫描配置、按 configId 建索引、迁移旧配置、自动修复、切换当前配置、克隆、保存、删除用户配置。

RecordConfigProvider
- 设置 UI 使用的配置入口。
- 负责连接 UI 和配置读写，不负责录制。

RecorderParamsConfig
- JSON 配置数据结构。
- 包含视频、音频、推流、FFmpeg、音量增强等参数。

FFmpegCommandBuilder
- 负责构建 FFmpeg 命令。
- 不启动进程，不修改 RecorderState，不访问 UI。

WindowsLoopbackAudioRecorder
- Windows 系统音频采集相关组件。
- 依赖 WASAPI 能力或原生插件。缺失时应通过错误码或 Warning 提示。"),

                new("生命周期接口", "Start / Stop 相关函数、时机和失败处理。",
@"StartRecordingAsync()
作用：异步开始录制或推流。
调用时机：Recorder 处于 Ready / Completed / 部分允许状态时调用。
返回值：RecorderResult，包含 success、message、errorCode、sessionId、outputPath。
可能失败：InvalidConfig、FFmpegNotFound、ProcessLaunchFailed、AudioStartFailed、StateTransitionInvalid。
示例：
var result = await recorder.StartRecordingAsync();
if (!result.success)
{
    Debug.LogError(result.errorCodeText + "" "" + result.message);
}

StopRecordingAsync()
作用：异步停止录制或推流。
调用时机：Recording 或 Streaming 中调用。
返回值：RecorderResult。停止本地录制后可能进入 Merging。
可能失败：StateTransitionInvalid、ProcessStopFailed、MergeFailed。
示例：
var result = await recorder.StopRecordingAsync();
Debug.Log(result.message);

StartRecording()
作用：Unity Button 兼容入口。
调用时机：按钮点击。
返回值：无，内部调用 StartRecordingAsync()。
建议：开发者代码优先使用 StartRecordingAsync()。

StopRecording()
作用：Unity Button 兼容入口。
调用时机：按钮点击。
返回值：无，内部调用 StopRecordingAsync()。
建议：开发者代码优先使用 StopRecordingAsync()。"),

                new("状态机", "RecorderState 各状态含义和进入时机。",
@"RecorderState

Uninitialized
- 录制器尚未初始化。

Ready
- 已就绪，可开始录制。

Starting
- 正在校验配置、构建命令、启动 FFmpeg 或音频采集。

Recording
- 正在录制本地视频或发送推流。

Stopping
- 正在停止 FFmpeg、停止音频采集或等待临时文件就绪。

Merging
- 正在执行音视频合并。
- 后台 Merge 完成时不会覆盖新的 Recording 状态。

Completed
- 本次录制流程完成。

Error
- 出现致命错误，RecorderResult 和事件中会带 RecorderErrorCode。"),

                new("事件系统", "推荐事件和 Legacy 兼容事件说明。",
@"OnRecorderStateChanged
- 触发时机：RecorderState 发生变化。
- 参数：RecorderEventArgs，含 sessionId、state、message。
- 推荐新代码使用。

OnRecorderWarning
- 触发时机：非致命问题，例如非法 Stop、配置自动修复、音频滤镜降级。
- 参数：RecorderErrorCode + message。
- 推荐新代码使用。

OnRecorderError
- 触发时机：启动失败、FFmpeg 启动失败、音频启动失败、合并失败等错误。
- 参数：RecorderErrorCode、Exception、sessionId。
- 推荐新代码使用。

OnRecorderProgress
- 触发时机：StartSucceeded、StopRequested、MergeStarted、MergeSucceeded 等业务里程碑。
- 参数：eventType、sessionId、outputPath、message。
- 推荐新代码使用。

OnRecordStarted
- Legacy 兼容事件。
- 开始录制成功时触发。
- 新代码建议改用 OnRecorderProgress / OnRecorderStateChanged。

OnRecordStopped
- Legacy 兼容事件。
- 停止录制后触发。
- 新代码建议改用 OnRecorderProgress。

OnMergeStarted
- 合并开始时触发。

OnMergeCompleted
- 合并完成时触发。"),

                new("返回结果", "异步调用返回结果字段说明。",
@"RecorderResult 字段

success
- 本次调用是否成功。

message
- 可显示到 UI 或写入日志的说明。

outputPath
- 输出文件路径。推流模式可能为空。

errorCode
- RecorderErrorCode 枚举值。

errorCodeText
- 错误码字符串，便于 UI 展示和日志检索。

sessionId
- 当前调用关联的会话 ID。

exception
- 捕获到的异常对象。没有异常时为空。"),

                new("错误码", "常用错误码含义和处理建议。",
@"None
- 无错误。

InvalidConfig
- 配置无效。检查 FFmpeg 路径、输出目录、推流地址、帧率、编码器。

FFmpegNotFound
- 未找到 FFmpeg。请放入 FFmpegApp 或在设置中心选择路径。

ProcessLaunchFailed
- FFmpeg 进程启动失败。检查权限、路径、命令参数。

AudioStartFailed
- 音频启动失败。检查 WASAPI、原生 DLL、音频设备。

NativePluginMissing
- 原生插件缺失，例如 WASAPILoopbackRecorder.dll。

StateTransitionInvalid
- 当前状态不允许该调用，例如未录制时 Stop。
- 通常应作为 Warning 处理。

MergeFailed
- 音视频合并失败。检查临时文件和 FFmpeg 日志。

ConfigNotFound
- configId 不存在。应回退模板或提示用户重新选择配置。

ConfigSaveFailed
- 配置保存失败。检查目录权限和文件占用。

AudioFilterInvalid
- 音频滤镜不可用或拼接异常。可关闭 limiter 或检查 FFmpeg filters。"),

                new("会话历史", "会话历史读取、清空和写入规则。",
@"GetSessionHistory()
- 获取最近会话历史。
- 返回只读列表，适合 UI 展示、日志上传和排错。

ClearSessionHistory()
- 清空历史。
- 不影响当前正在执行的录制。

RecorderSessionInfo 常用字段
- sessionId：会话 ID。
- configId：配置 ID。
- displayName：配置显示名。
- state：记录时状态。
- eventType：StartSucceeded、StopFailed、MergeSucceeded 等。
- errorCode：错误码。
- message：说明。
- outputPath：输出路径。
- startedAt / endedAt：开始和结束时间。
- durationMs：持续时间。
- isStreaming：是否推流。
- isMerged：是否完成合并。
- mergeOutputPath：合并输出路径。
- audioGainEnabled / audioGainDb：本次音量增强参数。

写入规则
- Start 成功 / 失败会写入。
- Stop 请求 / 成功 / 失败会写入。
- Merge 开始 / 成功 / 失败会写入。
- Warning / Error 会写入。
- 后台 Merge 完成时会写入旧 session，不覆盖新录制状态。"),

                new("配置系统", "configId 配置体系和常用操作。",
@"核心字段

configId
- 内部唯一 ID，用于查找配置和生成文件名。

displayName
- UI 显示名，可中文，不参与路径。

captureDisplayName
- 显示器名称，例如 \\.\DISPLAY1。

UseWinRecordConfig / UseLinuxRecordConfig
- 当前平台使用配置引用文件。
- 新版使用 currentConfigId。

template 配置
- 默认模板配置，例如 template_win_medium。
- 可调整 UI 参数，但模板文件默认不应被用户直接覆盖为自定义配置。

user 配置
- 用户克隆或另存的配置。
- 可 Save / Delete / Rename。

常用操作
- CloneConfig：从模板或现有配置克隆用户配置。
- SaveConfig：保存配置。
- DeleteConfig：删除用户配置，模板不可删除。
- SetCurrentConfig：切换当前 configId。"),

                new("音量增强", "录制声音偏小时的音频增益配置。",
@"JSON 字段

enableAudioGain
- 是否启用录制音量增强。

audioGainDb
- 音量增益，单位 dB。
- 建议范围：-20 到 20。

audioLimiterEnabled
- 启用 limiter，降低爆音和削波风险。

FFmpeg 滤镜
- 启用音量增强时生成 volume，例如 volume=6dB。
- limiter 开启时追加 alimiter=limit=0.95。
- 如果 FFmpeg 不支持 alimiter，应降级只使用 volume 并给 Warning。

常用值
- 3dB：轻微增大。
- 6dB：明显增大，约 2 倍。
- 9dB：较大。
- 12dB：很大，可能失真。

爆音处理
- 如果出现破音，先降到 3dB 或关闭增强。
- 不建议超过 12dB，除非确认不会削波。"),

                new("示例代码", "可复制到 Unity C# 脚本中的常用示例。",
@"示例 1：最小 Start / Stop

using UnityEngine;

public sealed class RecorderStartStopExample : MonoBehaviour
{
    public CrossPlatformScreenRecorder recorder;

    public async void StartAndStop()
    {
        var start = await recorder.StartRecordingAsync();
        if (!start.success)
        {
            Debug.LogError(start.errorCodeText + "" "" + start.message);
            return;
        }

        await recorder.StopRecordingAsync();
    }
}

示例 2：监听事件

private void OnEnable()
{
    recorder.OnRecorderStateChanged += OnStateChanged;
    recorder.OnRecorderError += OnError;
    recorder.OnRecorderWarning += OnWarning;
}

private void OnDisable()
{
    recorder.OnRecorderStateChanged -= OnStateChanged;
    recorder.OnRecorderError -= OnError;
    recorder.OnRecorderWarning -= OnWarning;
}

private void OnStateChanged(RecorderEventArgs args)
{
    Debug.Log(args.state + "" "" + args.sessionId);
}

private void OnError(RecorderEventArgs args)
{
    Debug.LogError(args.errorCode + "" "" + args.message);
}

private void OnWarning(RecorderEventArgs args)
{
    Debug.LogWarning(args.errorCode + "" "" + args.message);
}

示例 3：处理失败错误码

var result = await recorder.StartRecordingAsync();
if (!result.success)
{
    switch (result.errorCode)
    {
        case RecorderErrorCode.FFmpegNotFound:
            Debug.LogError(""请先配置 FFmpeg 路径"");
            break;
        case RecorderErrorCode.InvalidConfig:
            Debug.LogError(""录制配置无效："" + result.message);
            break;
        default:
            Debug.LogError(result.errorCodeText + "" "" + result.message);
            break;
    }
}

示例 4：读取 SessionHistory

var history = recorder.GetSessionHistory();
foreach (var session in history)
{
    Debug.Log(session.sessionId + "" "" + session.eventType + "" "" + session.outputPath);
}

示例 5：切换 configId 后录制

var registry = new RecorderConfigRegistry();
var setResult = registry.SetCurrentConfig(""template_win_medium"");
if (!setResult.success)
{
    Debug.LogError(setResult.message);
    return;
}

await recorder.StartRecordingAsync();

示例 6：打开输出目录

string outputPath = recorder.CurrentOutputFilePath;
if (!string.IsNullOrEmpty(outputPath))
{
    string folder = System.IO.Path.GetDirectoryName(outputPath);
    Application.OpenURL(folder);
}")
            };
        }

        /// <summary>
        /// 创建 SDK API 说明内容，避免 OnGUI 每帧拼接大段字符串。
        /// </summary>
        private static List<SdkApiDocEntry> CreateSdkApiEntries()
        {
            return new List<SdkApiDocEntry>
            {
                new("核心组件", "CrossPlatformScreenRecorder 的职责和挂载方式。",
@"CrossPlatformScreenRecorder

- 负责 Recorder SDK 的录制生命周期。
- 建议挂载在场景中的 RecorderManager 对象上。
- 开发者通过它调用 Start / Stop、监听事件、读取状态和会话历史。
- Settings UI 和 Demo UI 都应引用同一个 RecorderManager 上的 CrossPlatformScreenRecorder。"),

                new("常用函数", "开发者最常用的 Start / Stop / History API。",
@"StartRecordingAsync()

- 异步开始录制。
- 返回 RecorderResult。
- 适合开发者代码调用，可以 await 并读取失败原因。

StopRecordingAsync()

- 异步停止录制。
- 返回 RecorderResult。
- 如果当前是本地录制，可能触发音视频合并流程。

StartRecording()

- Unity Button 兼容入口。
- 内部调用 StartRecordingAsync()。

StopRecording()

- Unity Button 兼容入口。
- 内部调用 StopRecordingAsync()。

GetSessionHistory()

- 获取最近录制历史。
- 适合用于 UI、日志、上传、审计和排错。

ClearSessionHistory()

- 清空会话历史。
- 不会影响当前正在录制的会话。"),

                new("常用属性", "状态、输出路径和当前会话。",
@"CurrentState

- 当前 Recorder 状态。
- 常见值：Ready、Starting、Recording、Stopping、Merging、Completed、Error。

CurrentOutputFilePath

- 最近输出文件路径。
- 用于 UI 展示、打开输出目录、后续上传或文件检查。

CurrentSession

- 当前录制会话信息。
- 包含 sessionId、configId、displayName、输出路径、是否推流等信息。"),

                new("常用事件", "SDK 调用方监听 Recorder 变化的统一入口。",
@"OnRecorderStateChanged

- 状态变化时触发。
- 例如 Ready、Starting、Recording、Stopping、Merging、Completed、Error。

OnRecorderError

- 出现错误时触发。
- 事件参数带 RecorderErrorCode。

OnRecorderWarning

- 出现非致命问题时触发。
- 例如非法状态调用、配置自动修复、可降级的音频滤镜问题。

OnRecorderProgress

- 业务里程碑事件。
- 例如 StartSucceeded、StopRequested、MergeStarted、MergeSucceeded。

OnRecordStarted

- 旧兼容事件。
- 开始录制成功时触发。

OnRecordStopped

- 旧兼容事件。
- 停止录制后触发。

OnMergeStarted

- 合并开始时触发。

OnMergeCompleted

- 合并完成时触发。"),

                new("返回结果", "RecorderResult 字段说明。",
@"RecorderResult

success
- 本次调用是否成功。

message
- 面向开发者和日志的说明信息。

outputPath
- 输出文件路径。

errorCode
- RecorderErrorCode 枚举值。

errorCodeText
- 错误码文本。

sessionId
- 本次调用关联的会话 ID。

exception
- 捕获到的异常对象。没有异常时为空。"),

                new("错误码", "常见 RecorderErrorCode 含义和处理建议。",
@"RecorderErrorCode

None
- 无错误。

InvalidConfig
- 配置无效。检查 FFmpeg 路径、输出目录、推流地址、帧率、编码器等。

FFmpegNotFound
- 未找到 FFmpeg。请放入 FFmpegApp 或在 Settings UI 中选择自定义路径。

ProcessLaunchFailed
- FFmpeg 进程启动失败。检查路径、权限和命令参数。

AudioStartFailed
- 音频启动失败。Windows 重点检查 wasapi / 原生 DLL / 音频设备。

NativePluginMissing
- 原生插件缺失，例如 WASAPILoopbackRecorder.dll。

StateTransitionInvalid
- 当前状态不允许执行该操作，例如未录制时 Stop。

MergeFailed
- 音视频合并失败。检查临时文件、编码器和 FFmpeg 日志。

ConfigNotFound
- 找不到指定 configId。

ConfigSaveFailed
- 配置保存失败。检查目录权限和 JSON 写入。

AudioFilterInvalid
- 音频滤镜不可用或拼接异常。音量增强可能降级或需要检查 alimiter 支持。"),

                new("会话历史", "RecorderSessionInfo 字段说明。",
@"RecorderSessionInfo

sessionId
- 会话唯一 ID。

configId
- 本次录制使用的配置 ID。

displayName
- 配置显示名。

state
- 当前或记录时的 RecorderState。

eventType
- 历史事件类型，例如 StartSucceeded、StopFailed、MergeSucceeded。

errorCode
- 关联错误码。

message
- 事件说明。

outputPath
- 输出路径。

startedAt / endedAt
- 会话开始和结束时间。

durationMs
- 会话持续时间，单位毫秒。

isStreaming
- 是否为推流会话。

isMerged
- 是否已完成合并。

mergeOutputPath
- 合并输出路径。

audioGainEnabled
- 本次录制是否启用音量增强。

audioGainDb
- 本次录制使用的音量增益 dB。"),

                new("配置系统", "配置管理相关类型。",
@"RecorderConfigRegistry

- 通过 configId 管理配置。
- 支持扫描、切换、保存、删除、克隆配置。
- 负责旧配置迁移和自动修复。

RecordConfigProvider

- Settings UI 使用的配置入口。
- 负责把 UI 选择和当前配置读写连接起来。

RecorderParamsConfig

- JSON 配置数据结构。
- 包含视频、音频、推流、FFmpeg、音量增强等参数。
- 新版配置使用 schemaVersion = 2、configId、displayName、captureDisplayName。"),

                new("最小示例", "异步 Start / Stop 调用示例。",
@"using UnityEngine;

public sealed class RecorderExample : MonoBehaviour
{
    public CrossPlatformScreenRecorder recorder;

    public async void Run()
    {
        var result = await recorder.StartRecordingAsync();

        if (!result.success)
        {
            Debug.LogError(result.errorCodeText + "" "" + result.message);
            return;
        }

        await recorder.StopRecordingAsync();
    }
}")
            };
        }

        /// <summary>
        /// 执行外部进程并返回输出。
        /// </summary>
        private static string RunProcess(string executablePath, string arguments, int timeoutMs, out bool success)
        {
            success = false;
            try
            {
                using var process = new Process();
                process.StartInfo = new ProcessStartInfo
                {
                    FileName = executablePath,
                    Arguments = arguments,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };
                process.Start();
                var outputBuilder = new StringBuilder();
                var errorBuilder = new StringBuilder();
                process.OutputDataReceived += (_, args) =>
                {
                    if (args.Data != null) outputBuilder.AppendLine(args.Data);
                };
                process.ErrorDataReceived += (_, args) =>
                {
                    if (args.Data != null) errorBuilder.AppendLine(args.Data);
                };
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                bool exited = process.WaitForExit(timeoutMs);
                if (!exited)
                {
                    process.Kill();
                    return "执行超时。";
                }

                success = process.ExitCode == 0;
                string output = outputBuilder.ToString();
                string error = errorBuilder.ToString();
                return string.IsNullOrWhiteSpace(output) ? error : output + Environment.NewLine + error;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        /// <summary>
        /// 提取 JSON 中的字符串字段。
        /// </summary>
        private static string ExtractJsonString(string json, string fieldName)
        {
            string token = "\"" + fieldName + "\"";
            int fieldIndex = json.IndexOf(token, StringComparison.Ordinal);
            if (fieldIndex < 0) return string.Empty;
            int colonIndex = json.IndexOf(':', fieldIndex);
            if (colonIndex < 0) return string.Empty;
            int startQuote = json.IndexOf('"', colonIndex + 1);
            if (startQuote < 0) return string.Empty;
            int endQuote = json.IndexOf('"', startQuote + 1);
            return endQuote < 0 ? string.Empty : json.Substring(startQuote + 1, endQuote - startQuote - 1);
        }

        /// <summary>
        /// 获取第一行文本。
        /// </summary>
        private static string FirstLine(string text)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;
            using var reader = new StringReader(text);
            return reader.ReadLine() ?? string.Empty;
        }

        /// <summary>
        /// 获取检查状态样式。
        /// </summary>
        private GUIStyle GetStatusStyle(CheckLevel level)
        {
            return level switch
            {
                CheckLevel.Ok => _statusOkStyle,
                CheckLevel.Warning => _statusWarningStyle,
                _ => _statusErrorStyle
            };
        }

        /// <summary>
        /// 获取检查状态中文文本。
        /// </summary>
        private static string GetCheckLevelText(CheckLevel level)
        {
            return level switch
            {
                CheckLevel.Ok => "正常",
                CheckLevel.Warning => "警告",
                _ => "错误"
            };
        }

        /// <summary>
        /// 初始化 GUI 样式。
        /// </summary>
        private void EnsureStyles()
        {
            _headerStyle ??= new GUIStyle(EditorStyles.boldLabel) { fontSize = 18 };
            _sectionStyle ??= new GUIStyle(EditorStyles.boldLabel) { fontSize = 13 };
            _mutedStyle ??= new GUIStyle(EditorStyles.label) { normal = { textColor = new Color(0.62f, 0.62f, 0.62f) } };
            _smallTextStyle ??= new GUIStyle(EditorStyles.wordWrappedLabel)
            {
                fontSize = 11,
                normal = { textColor = new Color(0.70f, 0.70f, 0.70f) }
            };
            _statusOkStyle ??= new GUIStyle(EditorStyles.boldLabel) { normal = { textColor = new Color(0.15f, 0.65f, 0.25f) } };
            _statusWarningStyle ??= new GUIStyle(EditorStyles.boldLabel) { normal = { textColor = new Color(0.9f, 0.58f, 0.12f) } };
            _statusErrorStyle ??= new GUIStyle(EditorStyles.boldLabel) { normal = { textColor = new Color(0.9f, 0.25f, 0.2f) } };
            _cardStyle ??= new GUIStyle(EditorStyles.helpBox)
            {
                padding = new RectOffset(12, 12, 10, 10),
                margin = new RectOffset(0, 0, 4, 8)
            };
            _mainCardStyle ??= new GUIStyle(EditorStyles.helpBox)
            {
                padding = new RectOffset(14, 14, 12, 12),
                margin = new RectOffset(0, 0, 4, 8)
            };
            _tableHeaderStyle ??= new GUIStyle(EditorStyles.boldLabel)
            {
                normal = { textColor = new Color(0.82f, 0.82f, 0.82f) }
            };
            _navItemStyle ??= new GUIStyle(EditorStyles.label)
            {
                padding = new RectOffset(8, 4, 4, 4),
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = new Color(0.72f, 0.72f, 0.72f) }
            };
            _selectedNavItemStyle ??= new GUIStyle(_navItemStyle)
            {
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.92f, 0.92f, 0.92f) }
            };
            _navAccentStyle ??= new GUIStyle(EditorStyles.label)
            {
                normal = { textColor = new Color(0.24f, 0.52f, 0.95f) }
            };
            _markdownStyle ??= new GUIStyle(EditorStyles.wordWrappedLabel)
            {
                richText = false,
                wordWrap = true,
                padding = new RectOffset(4, 8, 4, 8)
            };
        }

        [Serializable]
        private sealed class JsonProbe
        {
            public int schemaVersion;
            public string configId;
            public string currentConfigId;
        }

        private readonly struct DocumentEntry
        {
            public readonly string category;
            public readonly string title;
            public readonly string fileName;
            public readonly string relativePath;

            public DocumentEntry(string category, string title, string fileName, string relativePath)
            {
                this.category = category;
                this.title = title;
                this.fileName = fileName;
                this.relativePath = relativePath;
            }
        }

        private readonly struct SdkApiDocEntry
        {
            public readonly string title;
            public readonly string description;
            public readonly string content;

            public SdkApiDocEntry(string title, string description, string content)
            {
                this.title = title;
                this.description = description;
                this.content = content;
            }
        }

        private readonly struct CachedDocument
        {
            public readonly string content;
            public readonly DateTime lastWriteTime;

            public CachedDocument(string content, DateTime lastWriteTime)
            {
                this.content = content;
                this.lastWriteTime = lastWriteTime;
            }
        }

        private readonly struct CheckItem
        {
            public readonly CheckLevel level;
            public readonly string title;
            public readonly string status;
            public readonly string message;

            private CheckItem(CheckLevel level, string title, string message)
            {
                this.level = level;
                this.title = title;
                this.message = message;
                status = level switch
                {
                    CheckLevel.Ok => "正常",
                    CheckLevel.Warning => "警告",
                    _ => "错误"
                };
            }

            public static CheckItem Ok(string title, string message)
            {
                return new CheckItem(CheckLevel.Ok, title, message);
            }

            public static CheckItem Warning(string title, string message)
            {
                return new CheckItem(CheckLevel.Warning, title, message);
            }

            public static CheckItem Error(string title, string message)
            {
                return new CheckItem(CheckLevel.Error, title, message);
            }
        }

        private enum CheckLevel
        {
            Ok,
            Warning,
            Error
        }
    }
}
#endif
