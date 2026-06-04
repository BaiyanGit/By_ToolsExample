//=====================================================
// 文件名称: RecorderSettingsUIBuilderEditor
// 创 建 者: wangbaiyan
// 创建日期: 2026-05-25
// 描    述: Recorder SDK 设置中心界面编辑器生成工具，用于一次性创建并绑定场景 UI。
//=====================================================

#if UNITY_EDITOR
namespace Demos.示例_录制视频Recorder.Editor
{
    using System.IO;
    using Scripts.Core;
    using Scripts.UISettings;
    using UnityEditor;
    using UnityEditor.Events;
    using UnityEditor.SceneManagement;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.InputSystem.UI;
    using UnityEngine.SceneManagement;
    using UnityEngine.UI;

    /// <summary>
    /// 按 LegacyScenes/录制视频Recorder.unity 的设置界面布局创建 UI，并绑定到 UIRecorderParamsSettings。
    /// </summary>
    public static class RecorderSettingsUIBuilderEditor
    {
        private const string CANVAS_NAME = "RecCanvas";
        private const string SETTINGS_ROOT_NAME = "RecorderParamsSettings";
        private const string RECORDER_MANAGER_NAME = "RecorderManager";
        private const int ACTIVE_INPUT_HANDLER_INPUT_SYSTEM = 1;
        private const int ACTIVE_INPUT_HANDLER_BOTH = 2;

        private static readonly Vector2 replicaResolution = new(1145f, 645f);
        private static readonly Color rootColor = new(0.08f, 0.09f, 0.11f, 1f);
        private static readonly Color panelColor = new(0.11f, 0.12f, 0.14f, 0.92f);
        private static readonly Color headerColor = new(0.10f, 0.10f, 0.12f, 0.98f);
        private static readonly Color fieldColor = new(0.16f, 0.17f, 0.18f, 1f);
        private static readonly Color buttonColor = new(0.15f, 0.17f, 0.19f, 1f);
        private static readonly Color selectedMenuColor = new(0.12f, 0.28f, 0.18f, 0.95f);
        private static readonly Color borderColor = new(0.86f, 0.86f, 0.86f, 0.72f);
        private static readonly Color textColor = Color.white;
        private static readonly Color mutedTextColor = new(0.72f, 0.72f, 0.72f, 1f);

        /// <summary>
        /// 创建设置中心 UI。
        /// </summary>
        public static void CreateSettingsUI()
        {
            var manager = GetOrCreateGameObject(RECORDER_MANAGER_NAME);
            if (manager.GetComponent<CrossPlatformScreenRecorder>() == null) Undo.AddComponent<CrossPlatformScreenRecorder>(manager);

            var canvas = GetOrCreateCanvas();
            var root   = GetOrCreatePanel(canvas.transform, SETTINGS_ROOT_NAME, Vector2.zero, Vector2.zero, rootColor);
            Stretch(root);
            SetGraphicColor(root.gameObject, rootColor);
            ClearChildren(root);

            var settings                   = root.GetComponent<UIRecorderParamsSettings>();
            if (settings == null) settings = Undo.AddComponent<UIRecorderParamsSettings>(root.gameObject);
            settings.optionDescriptionJsonName = "RecorderOptionDescriptions.json";

            BuildReplicaLayout(settings, root);
            BuildWindows(settings, canvas.transform);

            EnsureEventSystem();
            EditorUtility.SetDirty(settings);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            Debug.Log("Recorder SDK 设置中心 UI 已按原 录制视频Recorder 场景布局复刻生成。请保存当前场景。");
        }

        private static void BuildReplicaLayout(UIRecorderParamsSettings settings, RectTransform root)
        {
            var background = CreatePanel(root, "Background", 0f, 0f, replicaResolution.x, replicaResolution.y, rootColor, true);
            var mainPanel  = CreatePanel(background, "MainPanel", 0f, 0f, replicaResolution.x, replicaResolution.y, new Color(0f, 0f, 0f, 0f), true);

            BuildHeader(settings, mainPanel);
            BuildStatusPanel(settings, mainPanel);
            BuildConfigPanel(settings, mainPanel);
            BuildDescriptionPanel(settings, mainPanel);
            BuildSettingListAndPages(settings, mainPanel);

            var version = CreateText(mainPanel, "TxtVersion", 1078f, 626f, 55f, 16f, "v 1.0.0", 9, TextAnchor.MiddleRight);
            version.color = textColor;
        }

        private static void BuildHeader(UIRecorderParamsSettings settings, RectTransform parent)
        {
            var header = CreatePanel(parent, "HeaderBar", 0f, 0f, replicaResolution.x, 35f, headerColor, true);
            CreateText(header, "TxtTitle", 16f, 0f, 420f, 35f, "显示器 - 录制视频设置参数", 14, TextAnchor.MiddleLeft);
            settings.btnClose = CreateButton(header, "BtnClose", 1066f, 8f, 58f, 20f, "关闭", 10);
        }

        private static void BuildStatusPanel(UIRecorderParamsSettings settings, RectTransform parent)
        {
            var panel = CreatePanel(parent, "StatusPanel", 35f, 42f, 423f, 230f, panelColor, true);
            CreateText(panel, "Title", 20f, 8f, 120f, 22f, "状态", 14, TextAnchor.MiddleLeft);
            CreateThinLine(panel, 12f, 26f, 405f);

            settings.txtStatusUseState       = CreateStatusLine(panel, "TxtStatusUseState", 20f, 36f, "当前状态：", "正在使用", true);
            settings.txtStatusTemplateName   = CreateStatusLine(panel, "TxtStatusTemplateName", 20f, 58f, "模板：", "未选择", false);
            settings.txtStatusOutputFormat   = CreateStatusLine(panel, "TxtStatusOutputFormat", 20f, 80f, "输出格式：", "未配置", false);
            settings.txtStatusOutputScale    = CreateStatusLine(panel, "TxtStatusOutputScale", 20f, 102f, "分辨率比例：", "未配置", false);
            settings.txtStatusFrameRate      = CreateStatusLine(panel, "TxtStatusFrameRate", 20f, 124f, "帧率：", "未配置", false);
            settings.txtStatusVideoCodec     = CreateStatusLine(panel, "TxtStatusVideoCodec", 20f, 146f, "视频编码：", "未配置", false);
            settings.txtStatusAudioCodec     = CreateStatusLine(panel, "TxtStatusAudioCodec", 20f, 168f, "音频编码：", "未配置", false);
            settings.txtStatusLastUpdateTime = CreateStatusLine(panel, "TxtStatusLastUpdateTime", 20f, 190f, "当前配置最后更新时间：", "未保存", false);

            settings.btnSaveAs = CreateButton(panel, "BtnSaveAs", 302f, 210f, 47f, 18f, "另存为", 10);
            settings.btnSave   = CreateButton(panel, "BtnSave", 362f, 210f, 47f, 18f, "保存", 10);
        }

        private static void BuildConfigPanel(UIRecorderParamsSettings settings, RectTransform parent)
        {
            var panel = CreatePanel(parent, "ConfigPanel", 475f, 42f, 677f, 230f, panelColor, true);

            var configRow = CreateRow(panel, "配置文件选择", 12f);
            CreateLabel(configRow, "TxtConfigLabel", "配置文件：", 0f, 60f);
            settings.drConfig        = CreateDropdown(configRow, "DrConfig", 60f, 0f, 447f, 26f, "Option A");
            settings.btnUse          = CreateButton(configRow, "BtnUse", 515f, 0f, 42f, 26f, "使用", 10);
            settings.btnDeleteConfig = CreateButton(configRow, "BtnDeleteConfig", 562f, 0f, 42f, 26f, "删除", 10);

            var ffmpegRow = CreateRow(panel, "FFmpeg", 50f);
            CreateLabel(ffmpegRow, "TxtFfmpegLabel", "执行文件：", 0f, 60f);
            settings.ifFfmpegPath    = CreateInput(ffmpegRow, "IfFfmpegPath", 60f, 0f, 447f, 26f, "FFmpeg执行文件路径...");
            settings.btnSelectFfmpeg = CreateButton(ffmpegRow, "BtnSelectFfmpeg", 515f, 0f, 42f, 26f, "选择", 10);
            settings.btnResetFfmpeg  = CreateButton(ffmpegRow, "BtnResetFfmpeg", 562f, 0f, 42f, 26f, "重置", 10);

            var savePathRow = CreateRow(panel, "视频文件保存路径", 88f);
            settings.goVideoSavePathRoot = savePathRow.gameObject;
            CreateLabel(savePathRow, "TxtVideoSavePathLabel", "保存路径：", 0f, 60f);
            settings.ifVideoSavePath        = CreateInput(savePathRow, "IfVideoSavePath", 60f, 0f, 447f, 26f, "视频文件保存路径...");
            settings.btnSelectVideoSavePath = CreateButton(savePathRow, "BtnSelectVideoSavePath", 515f, 0f, 42f, 26f, "选择", 10);
            CreateButton(savePathRow, "BtnOpenVideoSavePath", 562f, 0f, 42f, 26f, "打开", 10);

            var streamRow = CreateRow(panel, "视频推流地址", 126f);
            settings.goStreamUrlRoot = streamRow.gameObject;
            CreateLabel(streamRow, "TxtStreamUrlLabel", "推流地址：", 0f, 60f);
            settings.ifStreamUrl = CreateInput(streamRow, "IfStreamUrl", 60f, 0f, 447f, 26f, "rtmp://server/app/streamKey");
            CreateDescButton(streamRow, 515f, 0f, 42f, 26f);
        }

        private static void BuildDescriptionPanel(UIRecorderParamsSettings settings, RectTransform parent)
        {
            var panel = CreatePanel(parent, "DescriptionPanel", 35f, 285f, 423f, 328f, panelColor, true);
            CreateText(panel, "Title", 20f, 8f, 120f, 24f, "参数说明", 14, TextAnchor.MiddleLeft);
            CreateThinLine(panel, 20f, 34f, 385f);
            CreateScrollbarVisual(panel, 412f, 36f, 9f, 292f);

            settings.drPlatform = CreateDropdownRow(panel, "运行平台", "运行平台：", "Dr_Platform", 48f, "Option A", 320f);

            settings.goVideoPrefixRoot = CreateRow(panel, "文件前缀", 86f).gameObject;
            CreateLabel(settings.goVideoPrefixRoot.transform, "TxtLabel", "文件前缀：", 0f, 78f);
            settings.ifVideoPrefix = CreateInput(settings.goVideoPrefixRoot.transform, "IfVideoPrefix", 82f, 0f, 280f, 26f, "视频名前缀...");
            CreateDescButton(settings.goVideoPrefixRoot.transform, 366f, 0f, 26f, 26f);

            settings.drUseMode     = CreateDropdownRow(panel, "使用方式", "使用方式：", "DrUseMode", 124f, "Option A", 320f);
            settings.drDisplay     = CreateDropdownRow(panel, "多屏幕选择录制", "录制屏幕：", "DrDisplay", 162f, "Option A", 320f);
            settings.drVideoFormat = CreateDropdownRow(panel, "视频格式", "视频格式：", "DrVideoFormat", 200f, "Option A", 320f);
        }

        private static void BuildSettingListAndPages(UIRecorderParamsSettings settings, RectTransform parent)
        {
            var menu                 = CreatePanel(parent, "SettingListPanel", 475f, 285f, 191f, 328f, panelColor, true);
            var group                = menu.GetComponent<ToggleGroup>();
            if (group == null) group = menu.gameObject.AddComponent<ToggleGroup>();

            var content = CreatePanel(parent, "ContentPanel", 675f, 285f, 477f, 328f, panelColor, true);
            CreateScrollbarVisual(content, 466f, 36f, 9f, 292f);
            var pageHost = CreatePanel(content, "PageHost", 0f, 0f, 477f, 328f, new Color(0f, 0f, 0f, 0f), false);

            var audioPage  = CreatePage(pageHost, "音频参数", "音频 - 设置参数");
            var videoPage  = CreatePage(pageHost, "视频参数", "视频 - 设置参数");
            var streamPage = CreatePage(pageHost, "推流设置", "推流 - 设置参数");

            audioPage.gameObject.SetActive(true);
            videoPage.gameObject.SetActive(false);
            streamPage.gameObject.SetActive(false);

            CreateMenuToggle(menu, group, "Tog音频设置", 18f, 22f, "▣  音频设置", audioPage.gameObject, true);
            CreateMenuToggle(menu, group, "Tog视频设置", 18f, 74f, "▣  视频设置", videoPage.gameObject, false);
            CreateMenuToggle(menu, group, "Tog推流设置", 18f, 126f, "▣  推流设置", streamPage.gameObject, false);

            BuildAudioPage(settings, audioPage);
            BuildVideoPage(settings, videoPage);
            BuildStreamPage(settings, streamPage);
        }

        private static RectTransform CreatePage(Transform parent, string name, string title)
        {
            var page = CreatePanel(parent, name, 0f, 0f, 477f, 328f, new Color(0f, 0f, 0f, 0f), false);
            CreateText(page, "Title", 22f, 14f, 260f, 22f, title, 14, TextAnchor.MiddleLeft);
            return page;
        }

        private static void BuildAudioPage(UIRecorderParamsSettings settings, RectTransform page)
        {
            settings.drAudioMode           = CreateDropdownRow(page, "音频采集模式", "音频采集模式：", "DrAudioMode", 48f, "Option A", 345f);
            settings.drAudioCoder          = CreateDropdownRow(page, "音频编码", "音频编码器：", "DrAudioCoder", 86f, "Option A", 345f);
            settings.drAudioBitrate        = CreateDropdownRow(page, "音频码率", "音频码率：", "DrAudioBitrate", 124f, "Option A", 345f);
            settings.drAudioSampleRate     = CreateDropdownRow(page, "音频采样率", "音频采样率：", "DrAudioSampleRate", 162f, "Option A", 345f);
            settings.drAudioChannel        = CreateDropdownRow(page, "音频声道数", "音频声道数：", "DrAudioChannel", 200f, "Option A", 345f);
            settings.drEnableAudioGain     = CreateDropdownRow(page, "录制音量增强", "音量增强：", "DrEnableAudioGain", 238f, "使用", 345f);
            settings.drAudioGainDb         = CreateDropdownRow(page, "录制音量增益", "增益 dB：", "DrAudioGainDb", 276f, "0 dB", 345f);
            settings.drAudioLimiterEnabled = CreateDropdownRow(page, "音频限幅器", "限幅器：", "DrAudioLimiterEnabled", 314f, "使用", 345f);
        }

        private static void BuildVideoPage(UIRecorderParamsSettings settings, RectTransform page)
        {
            settings.videoCaptureFrameRate      = CreateDropdownRow(page, "视频录制帧率", "帧率：", "DrVideoCaptureFrameRate", 48f, "Option A", 345f);
            settings.videoOutputScale           = CreateDropdownRow(page, "分辨率比例", "分辨率比例：", "DrVideoOutputScale", 86f, "Option A", 345f);
            settings.videoCodec                 = CreateDropdownRow(page, "视频编码器", "编码器：", "DrVideoCodec", 124f, "Option A", 345f);
            settings.videoPreset                = CreateDropdownRow(page, "视频编码预设", "编码预设：", "DrVideoPreset", 162f, "Option A", 345f);
            settings.videoCrf                   = CreateDropdownRow(page, "视频画质档位", "画质档位：", "DrVideoCrf", 200f, "Option A", 345f);
            settings.videoPixelFormat           = CreateDropdownRow(page, "视频像素格式", "像素格式：", "DrVideoPixelFormat", 238f, "Option A", 345f);
            settings.webmVideoBitrate           = CreateDropdownRow(page, "视频码率", "视频码率：", "DrWebmVideoBitrate", 276f, "Option A", 345f);
            settings.webmVideoDeadlineMode      = CreateDropdownRow(page, "视频实时编码模式", "实时编码模式：", "DrWebmVideoDeadlineMode", 314f, "Option A", 345f);
            settings.webmVideoCpuUsed           = CreateDropdownRow(page, "视频CPU使用等级", "CPU使用等级：", "DrWebmVideoCpuUsed", 352f, "Option A", 345f);
            settings.stopVideoTimeoutMs         = CreateDropdownRow(page, "视频等待ffmpeg退出", "停止等待：", "DrStopVideoTimeoutMs", 390f, "Option A", 345f);
            settings.waitTempFileReadyTimeoutMs = CreateDropdownRow(page, "视频等待临时文件释放超时", "临时文件等待：", "DrWaitTempFileReadyTimeoutMs", 428f, "Option A", 345f);
            settings.mergeTimeoutMs             = CreateDropdownRow(page, "视频后台合并音视频等待超时", "合并超时：", "DrMergeTimeoutMs", 466f, "Option A", 345f);
            settings.deleteTempFilesAfterMerge  = CreateDropdownRow(page, "删除临时文件", "删除临时文件：", "DrDeleteTempFilesAfterMerge", 504f, "Option A", 345f);
        }

        private static void BuildStreamPage(UIRecorderParamsSettings settings, RectTransform page)
        {
            settings.goStreamSettingsRoot      = page.gameObject;
            settings.drStreamVideoBitrate      = CreateDropdownRow(page, "推流码率", "推流码率：", "DrStreamVideoBitrate", 48f, "Option A", 345f);
            settings.drStreamGop               = CreateDropdownRow(page, "GOP", "GOP：", "DrStreamGop", 86f, "Option A", 345f);
            settings.drStreamBufferSize        = CreateDropdownRow(page, "缓冲区", "缓冲区：", "DrStreamBufferSize", 124f, "Option A", 345f);
            settings.drStreamReconnectCount    = CreateDropdownRow(page, "重连次数", "重连次数：", "DrStreamReconnectCount", 162f, "Option A", 345f);
            settings.drStreamReconnectInterval = CreateDropdownRow(page, "重连间隔", "重连间隔：", "DrStreamReconnectInterval", 200f, "Option A", 345f);
            settings.togStreamLowLatency       = CreateToggleRow(page, "低延迟", "低延迟：", "TogStreamLowLatency", 238f, "启用");
            settings.togStreamAutoReconnect    = CreateToggleRow(page, "自动重连", "自动重连：", "TogStreamAutoReconnect", 276f, "启用");
            settings.togStreamIncludeAudio     = CreateToggleRow(page, "包含系统音频", "包含系统音频：", "TogStreamIncludeAudio", 314f, "启用");
        }

        private static void BuildWindows(UIRecorderParamsSettings settings, Transform canvas)
        {
            var saveAs = GetOrCreatePanel(canvas, "SaveAsConfigWindow", new Vector2(460f, 190f), Vector2.zero, panelColor);
            ClearChildren(saveAs);
            settings.saveAsWindowRoot = saveAs.gameObject;
            CreateTextCentered(saveAs, "TxtTitle", 0f, 62f, 420f, 32f, "另存为配置", 20, TextAnchor.MiddleCenter);
            settings.ifSaveAsName        = CreateInputCentered(saveAs, "IfSaveAsName", 0f, 18f, 390f, 36f, "输入配置名称...");
            settings.btnSaveAsConfirm    = CreateButtonCentered(saveAs, "BtnConfirmSaveAs", -72f, -54f, 120f, 36f, "确定", 14);
            settings.btnSaveAsCancel     = CreateButtonCentered(saveAs, "BtnCancelSaveAs", 72f, -54f, 120f, 36f, "取消", 14);
            settings.txtSaveAsTips       = CreateTextCentered(saveAs, "TxtSaveAsTips", 0f, -18f, 390f, 28f, string.Empty, 14, TextAnchor.MiddleLeft);
            settings.txtSaveAsTips.color = new Color(1f, 0.45f, 0.35f, 1f);
            settings.txtSaveAsTips.gameObject.SetActive(false);
            settings.saveAsWindowRoot.SetActive(false);

            var desc = GetOrCreatePanel(canvas, "RecorderOptionDescWindow", new Vector2(520f, 220f), Vector2.zero, panelColor);
            ClearChildren(desc);
            settings.optionDescWindowRoot = desc.gameObject;
            settings.txtOptionDescTitle   = CreateTextCentered(desc, "TxtDescTitle", 0f, 84f, 480f, 28f, "选项说明", 18, TextAnchor.MiddleLeft);
            settings.txtOptionDescContent = CreateTextCentered(desc, "TxtDescContent", 0f, -8f, 480f, 150f, string.Empty, 15, TextAnchor.UpperLeft);
            settings.optionDescWindowRoot.SetActive(false);
        }

        private static Text CreateStatusLine(Transform parent, string textName, float x, float y, string label, string value, bool green)
        {
            CreateText(parent, textName + "_Label", x, y, 92f, 18f, label, 10, TextAnchor.MiddleLeft);
            var text = CreateText(parent, textName, x + 82f, y, 290f, 18f, value, 10, TextAnchor.MiddleLeft);
            text.color = green ? new Color(0.2f, 1f, 0.25f, 1f) : textColor;
            return text;
        }

        private static Dropdown CreateDropdownRow(Transform parent, string rowName, string label, string controlName, float y, string placeholder, float controlWidth)
        {
            var row = CreateRow(parent, rowName, y);
            CreateLabel(row, "TxtLabel", label, 0f, 78f);
            var dropdown = CreateDropdown(row, controlName, 82f, 0f, controlWidth - 42f, 26f, placeholder);
            CreateDescButton(row, controlWidth + 44f, 0f, 26f, 26f);
            return dropdown;
        }

        private static InputField CreateInputRow(Transform parent, string rowName, string label, string controlName, float y, string placeholder)
        {
            var row = CreateRow(parent, rowName, y);
            CreateLabel(row, "TxtLabel", label, 0f, 78f);
            var input = CreateInput(row, controlName, 82f, 0f, 303f, 26f, placeholder);
            CreateDescButton(row, 389f, 0f, 26f, 26f);
            return input;
        }

        private static Toggle CreateToggleRow(Transform parent, string rowName, string label, string controlName, float y, string text)
        {
            var row = CreateRow(parent, rowName, y);
            CreateLabel(row, "TxtLabel", label, 0f, 78f);
            var toggle = CreateToggle(row, controlName, 82f, 0f, 130f, 26f, text);
            CreateDescButton(row, 389f, 0f, 26f, 26f);
            return toggle;
        }

        private static Slider CreateSliderRow(Transform parent, string rowName, string label, string controlName, float y)
        {
            var row = CreateRow(parent, rowName, y);
            CreateLabel(row, "TxtLabel", label, 0f, 78f);
            var slider = CreateSlider(row, controlName, 82f, 4f, 303f, 18f);
            CreateDescButton(row, 389f, 0f, 26f, 26f);
            return slider;
        }

        private static Toggle CreateMenuToggle(Transform parent, ToggleGroup group, string name, float x, float y, string label, GameObject page, bool isOn)
        {
            var toggle = CreateMenuToggleVisual(parent, name, x, y, 168f, 30f, label);
            toggle.group = group;
            toggle.isOn  = isOn;
            toggle.onValueChanged.RemoveAllListeners();
            UnityEventTools.AddPersistentListener(toggle.onValueChanged, page.SetActive);
            return toggle;
        }

        private static RectTransform CreateRow(Transform parent, string name, float y)
        {
            return CreatePanel(parent, name, 20f, y, 620f, 26f, new Color(0f, 0f, 0f, 0f), false);
        }

        private static Text CreateLabel(Transform parent, string name, string text, float x, float width)
        {
            return CreateText(parent, name, x, 0f, width, 26f, text, 10, TextAnchor.MiddleRight);
        }

        private static Button CreateDescButton(Transform parent, float x, float y, float w, float h)
        {
            return CreateButton(parent, "Btn_Desc", x, y, w, h, "?", 11);
        }

        private static GameObject GetOrCreateGameObject(string name)
        {
            var existing = GameObject.Find(name);
            if (existing != null) return existing;
            var go = new GameObject(name);
            Undo.RegisterCreatedObjectUndo(go, "Create " + name);
            return go;
        }

        private static Canvas GetOrCreateCanvas()
        {
            var existing = GameObject.Find(CANVAS_NAME);
            if (existing != null && existing.TryGetComponent(out Canvas existingCanvas))
            {
                ConfigureCanvas(existingCanvas);
                return existingCanvas;
            }

            var go = new GameObject(CANVAS_NAME, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Undo.RegisterCreatedObjectUndo(go, "Create " + CANVAS_NAME);
            var canvas = go.GetComponent<Canvas>();
            ConfigureCanvas(canvas);
            return canvas;
        }

        private static void ConfigureCanvas(Canvas canvas)
        {
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler                 = canvas.GetComponent<CanvasScaler>();
            if (scaler == null) scaler = canvas.gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = replicaResolution;
            scaler.screenMatchMode     = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight  = 0f;
        }

        private static RectTransform GetOrCreatePanel(Transform parent, string name, Vector2 size, Vector2 position, Color color)
        {
            var existing = parent.Find(name);
            if (existing != null && existing.TryGetComponent(out RectTransform existingRect))
            {
                SetCenter(existingRect, position.x, position.y, size.x, size.y);
                SetGraphicColor(existingRect.gameObject, color);
                return existingRect;
            }

            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            Undo.RegisterCreatedObjectUndo(go, "Create " + name);
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            SetCenter(rect, position.x, position.y, size.x, size.y);
            go.GetComponent<Image>().color = color;
            return rect;
        }

        private static RectTransform CreatePanel(Transform parent, string name, float x, float y, float w, float h, Color color, bool border)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            Undo.RegisterCreatedObjectUndo(go, "Create " + name);
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            SetTopLeft(rect, x, y, w, h);
            var image = go.GetComponent<Image>();
            image.color         = color;
            image.raycastTarget = color.a > 0.01f;
            if (border) AddBorder(go);
            return rect;
        }

        private static Button CreateButton(Transform parent, string name, float x, float y, float w, float h, string label, int fontSize)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            Undo.RegisterCreatedObjectUndo(go, "Create " + name);
            go.transform.SetParent(parent, false);
            SetTopLeft(go.GetComponent<RectTransform>(), x, y, w, h);
            go.GetComponent<Image>().color = buttonColor;
            var text = CreateText(go.transform, "Text", 0f, 0f, w, h, label, fontSize, TextAnchor.MiddleCenter);
            text.raycastTarget = false;
            var button = go.GetComponent<Button>();
            button.transition = Selectable.Transition.None;
            DisableNavigation(button);
            return button;
        }

        private static Button CreateButtonCentered(Transform parent, string name, float x, float y, float w, float h, string label, int fontSize)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            Undo.RegisterCreatedObjectUndo(go, "Create " + name);
            go.transform.SetParent(parent, false);
            SetCenter(go.GetComponent<RectTransform>(), x, y, w, h);
            go.GetComponent<Image>().color = buttonColor;
            var text = CreateText(go.transform, "Text", 0f, 0f, w, h, label, fontSize, TextAnchor.MiddleCenter);
            Stretch(text.GetComponent<RectTransform>());
            text.raycastTarget = false;
            var button = go.GetComponent<Button>();
            button.transition = Selectable.Transition.None;
            DisableNavigation(button);
            return button;
        }

        private static Toggle CreateToggle(Transform parent, string name, float x, float y, float w, float h, string label)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Toggle));
            Undo.RegisterCreatedObjectUndo(go, "Create " + name);
            go.transform.SetParent(parent, false);
            SetTopLeft(go.GetComponent<RectTransform>(), x, y, w, h);
            var background = CreateImage(go.transform, "Background", 0f, 3f, 20f, 20f, fieldColor);
            var checkmark  = CreateImage(background.transform, "Checkmark", 4f, 4f, 12f, 12f, new Color(0.2f, 0.8f, 0.45f, 1f));
            var text       = CreateText(go.transform, "Label", 28f, 0f, w - 28f, h, label, 10, TextAnchor.MiddleLeft);
            text.raycastTarget = false;

            var toggle = go.GetComponent<Toggle>();
            toggle.transition    = Selectable.Transition.None;
            toggle.targetGraphic = background;
            toggle.graphic       = checkmark;
            DisableNavigation(toggle);
            return toggle;
        }

        private static Toggle CreateMenuToggleVisual(Transform parent, string name, float x, float y, float w, float h, string label)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Toggle));
            Undo.RegisterCreatedObjectUndo(go, "Create " + name);
            go.transform.SetParent(parent, false);
            SetTopLeft(go.GetComponent<RectTransform>(), x, y, w, h);
            var background = go.GetComponent<Image>();
            background.color = selectedMenuColor;
            var checkmark = CreateImage(go.transform, "Checkmark", 7f, 8f, 12f, 12f, new Color(0.25f, 0.9f, 0.45f, 1f));
            var text      = CreateText(go.transform, "Label", 12f, 0f, w - 16f, h, label, 12, TextAnchor.MiddleLeft);
            text.raycastTarget = false;

            var toggle = go.GetComponent<Toggle>();
            toggle.transition    = Selectable.Transition.None;
            toggle.targetGraphic = background;
            toggle.graphic       = checkmark;
            DisableNavigation(toggle);
            return toggle;
        }

        private static Dropdown CreateDropdown(Transform parent, string name, float x, float y, float w, float h, string label)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Dropdown));
            Undo.RegisterCreatedObjectUndo(go, "Create " + name);
            go.transform.SetParent(parent, false);
            SetTopLeft(go.GetComponent<RectTransform>(), x, y, w, h);
            go.GetComponent<Image>().color = fieldColor;
            AddBorder(go);

            var caption = CreateText(go.transform, "Label", 8f, 0f, w - 28f, h, label, 10, TextAnchor.MiddleLeft);
            caption.raycastTarget                                                                                  = false;
            CreateText(go.transform, "Arrow", w - 18f, 0f, 14f, h, "⌄", 12, TextAnchor.MiddleCenter).raycastTarget = false;

            var dropdown = go.GetComponent<Dropdown>();
            dropdown.transition    = Selectable.Transition.None;
            dropdown.captionText   = caption;
            dropdown.targetGraphic = go.GetComponent<Image>();
            dropdown.template      = CreateDropdownTemplate(go.transform, w, out var itemText);
            dropdown.itemText      = itemText;
            dropdown.itemImage     = null;
            dropdown.options.Clear();
            dropdown.options.Add(new Dropdown.OptionData(label));
            DisableNavigation(dropdown);
            return dropdown;
        }

        private static RectTransform CreateDropdownTemplate(Transform parent, float width, out Text itemText)
        {
            var templateGo = new GameObject("Template", typeof(RectTransform), typeof(Image), typeof(ScrollRect));
            Undo.RegisterCreatedObjectUndo(templateGo, "Create Dropdown Template");
            templateGo.SetActive(false);
            templateGo.transform.SetParent(parent, false);
            var templateRect = templateGo.GetComponent<RectTransform>();
            templateRect.anchorMin                 = new Vector2(0f, 0f);
            templateRect.anchorMax                 = new Vector2(1f, 0f);
            templateRect.pivot                     = new Vector2(0.5f, 1f);
            templateRect.sizeDelta                 = new Vector2(0f, 120f);
            templateRect.anchoredPosition          = new Vector2(0f, 2f);
            templateGo.GetComponent<Image>().color = fieldColor;

            var viewport = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
            viewport.transform.SetParent(templateGo.transform, false);
            var viewportRect = viewport.GetComponent<RectTransform>();
            Stretch(viewportRect);
            viewport.GetComponent<Image>().color          = new Color(1f, 1f, 1f, 0.08f);
            viewport.GetComponent<Mask>().showMaskGraphic = false;

            var content = new GameObject("Content", typeof(RectTransform));
            content.transform.SetParent(viewport.transform, false);
            var contentRect = content.GetComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0f, 1f);
            contentRect.anchorMax = new Vector2(1f, 1f);
            contentRect.pivot     = new Vector2(0.5f, 1f);
            contentRect.sizeDelta = new Vector2(0f, 28f);

            var item = new GameObject("Item", typeof(RectTransform), typeof(Toggle));
            item.transform.SetParent(content.transform, false);
            var itemRect = item.GetComponent<RectTransform>();
            itemRect.anchorMin = new Vector2(0f, 0.5f);
            itemRect.anchorMax = new Vector2(1f, 0.5f);
            itemRect.pivot     = new Vector2(0.5f, 0.5f);
            itemRect.sizeDelta = new Vector2(0f, 26f);

            var itemBackground = CreateImage(item.transform, "Item Background", 0f, 0f, width, 26f, fieldColor);
            Stretch(itemBackground.GetComponent<RectTransform>());
            var itemCheckmark = CreateImage(item.transform, "Item Checkmark", 6f, 7f, 12f, 12f, new Color(0.2f, 0.8f, 0.45f, 1f));
            itemText = CreateText(item.transform, "Item Label", 24f, 0f, width - 24f, 26f, "Option", 10, TextAnchor.MiddleLeft);

            var itemToggle = item.GetComponent<Toggle>();
            itemToggle.targetGraphic = itemBackground;
            itemToggle.graphic       = itemCheckmark;
            DisableNavigation(itemToggle);

            var scroll = templateGo.GetComponent<ScrollRect>();
            scroll.content    = contentRect;
            scroll.viewport   = viewportRect;
            scroll.horizontal = false;
            return templateRect;
        }

        private static InputField CreateInput(Transform parent, string name, float x, float y, float w, float h, string placeholder)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(InputField));
            Undo.RegisterCreatedObjectUndo(go, "Create " + name);
            go.transform.SetParent(parent, false);
            SetTopLeft(go.GetComponent<RectTransform>(), x, y, w, h);
            go.GetComponent<Image>().color = fieldColor;
            AddBorder(go);

            var text = CreateText(go.transform, "Text", 8f, 0f, w - 16f, h, string.Empty, 10, TextAnchor.MiddleLeft);
            text.color = textColor;
            var hint = CreateText(go.transform, "Placeholder", 8f, 0f, w - 16f, h, placeholder, 10, TextAnchor.MiddleLeft);
            hint.color = mutedTextColor;

            var input = go.GetComponent<InputField>();
            input.textComponent = text;
            input.placeholder   = hint;
            DisableNavigation(input);
            return input;
        }

        private static InputField CreateInputCentered(Transform parent, string name, float x, float y, float w, float h, string placeholder)
        {
            var input = CreateInput(parent, name, 0f, 0f, w, h, placeholder);
            SetCenter(input.GetComponent<RectTransform>(), x, y, w, h);
            return input;
        }

        private static Slider CreateSlider(Transform parent, string name, float x, float y, float w, float h)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Slider));
            Undo.RegisterCreatedObjectUndo(go, "Create " + name);
            go.transform.SetParent(parent, false);
            SetTopLeft(go.GetComponent<RectTransform>(), x, y, w, h);

            var background = CreateImage(go.transform, "Background", 0f, 6f, w, 4f, fieldColor);
            var fill       = CreateImage(go.transform, "Fill", 0f, 6f, 0f, 4f, new Color(0.25f, 0.65f, 0.95f, 1f));
            var handle     = CreateImage(go.transform, "Handle", 0f, 0f, 14f, 16f, Color.white);

            var slider = go.GetComponent<Slider>();
            slider.fillRect      = fill.GetComponent<RectTransform>();
            slider.handleRect    = handle.GetComponent<RectTransform>();
            slider.targetGraphic = handle;
            slider.minValue      = -20f;
            slider.maxValue      = 20f;
            DisableNavigation(slider);
            return slider;
        }

        private static void DisableNavigation(Selectable selectable)
        {
            if (selectable == null) return;

            var navigation = selectable.navigation;
            navigation.mode       = Navigation.Mode.None;
            selectable.navigation = navigation;
        }

        private static Text CreateText(Transform parent, string name, float x, float y, float w, float h, string value, int fontSize, TextAnchor alignment)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            Undo.RegisterCreatedObjectUndo(go, "Create " + name);
            go.transform.SetParent(parent, false);
            SetTopLeft(go.GetComponent<RectTransform>(), x, y, w, h);
            var text = go.GetComponent<Text>();
            text.font      = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize  = fontSize;
            text.color     = textColor;
            text.alignment = alignment;
            text.text      = value;
            return text;
        }

        private static Text CreateTextCentered(Transform parent, string name, float x, float y, float w, float h, string value, int fontSize, TextAnchor alignment)
        {
            var text = CreateText(parent, name, 0f, 0f, w, h, value, fontSize, alignment);
            SetCenter(text.GetComponent<RectTransform>(), x, y, w, h);
            return text;
        }

        private static Image CreateImage(Transform parent, string name, float x, float y, float w, float h, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            Undo.RegisterCreatedObjectUndo(go, "Create " + name);
            go.transform.SetParent(parent, false);
            SetTopLeft(go.GetComponent<RectTransform>(), x, y, w, h);
            var image = go.GetComponent<Image>();
            image.color = color;
            return image;
        }

        private static void CreateThinLine(Transform parent, float x, float y, float width)
        {
            CreateImage(parent, "Line", x, y, width, 1f, new Color(0.75f, 0.75f, 0.75f, 0.55f));
        }

        private static void CreateScrollbarVisual(Transform parent, float x, float y, float w, float h)
        {
            CreateImage(parent, "Scrollbar", x, y, w, h, new Color(0.02f, 0.02f, 0.02f, 0.75f));
            CreateImage(parent, "Handle", x + 1f, y, w - 2f, 70f, new Color(0.72f, 0.72f, 0.72f, 0.9f));
        }

        private static void AddBorder(GameObject go)
        {
            var outline                  = go.GetComponent<Outline>();
            if (outline == null) outline = go.AddComponent<Outline>();
            outline.effectColor     = borderColor;
            outline.effectDistance  = new Vector2(1f, -1f);
            outline.useGraphicAlpha = false;
        }

        private static void SetGraphicColor(GameObject go, Color color)
        {
            var image                = go.GetComponent<Image>();
            if (image == null) image = go.AddComponent<Image>();
            image.color = color;
        }

        private static void SetTopLeft(RectTransform rect, float x, float y, float w, float h)
        {
            rect.anchorMin        = new Vector2(0f, 1f);
            rect.anchorMax        = new Vector2(0f, 1f);
            rect.pivot            = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(x, -y);
            rect.sizeDelta        = new Vector2(w, h);
        }

        private static void SetCenter(RectTransform rect, float x, float y, float w, float h)
        {
            rect.anchorMin        = new Vector2(0.5f, 0.5f);
            rect.anchorMax        = new Vector2(0.5f, 0.5f);
            rect.pivot            = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(x, y);
            rect.sizeDelta        = new Vector2(w, h);
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin        = Vector2.zero;
            rect.anchorMax        = Vector2.one;
            rect.pivot            = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta        = Vector2.zero;
        }

        private static void ClearChildren(Transform parent)
        {
            for (int i = parent.childCount - 1; i >= 0; i--)
            {
                Undo.DestroyObjectImmediate(parent.GetChild(i).gameObject);
            }
        }

        private static void EnsureEventSystem()
        {
            if (Object.FindObjectOfType<EventSystem>() != null) return;
            var go = new GameObject("EventSystem", typeof(EventSystem));
            AddInputModuleByProjectSettings(go);
            Undo.RegisterCreatedObjectUndo(go, "Create EventSystem");
        }

        /// <summary>
        /// 根据 Project Settings / Active Input Handling 添加对应的 UI 输入模块。
        /// </summary>
        private static void AddInputModuleByProjectSettings(GameObject eventSystem)
        {
#if ENABLE_INPUT_SYSTEM
            if (ShouldUseInputSystemUiInputModule())
            {
                eventSystem.AddComponent<InputSystemUIInputModule>();
                return;
            }
#endif
            eventSystem.AddComponent<StandaloneInputModule>();
        }

        /// <summary>
        /// Active Input Handling 为 Input System 或 Both 时，优先使用新输入系统 UI 模块。
        /// </summary>
        private static bool ShouldUseInputSystemUiInputModule()
        {
            int activeInputHandler = ReadActiveInputHandler();
            return activeInputHandler == ACTIVE_INPUT_HANDLER_INPUT_SYSTEM || activeInputHandler == ACTIVE_INPUT_HANDLER_BOTH;
        }

        /// <summary>
        /// 读取 ProjectSettings.asset 中的 activeInputHandler。
        /// </summary>
        private static int ReadActiveInputHandler()
        {
            string path = Path.Combine(Directory.GetCurrentDirectory(), "ProjectSettings", "ProjectSettings.asset");
            if (!File.Exists(path)) return 0;

            foreach (string line in File.ReadAllLines(path))
            {
                string trimmed = line.Trim();
                if (!trimmed.StartsWith("activeInputHandler:")) continue;
                string value = trimmed.Substring("activeInputHandler:".Length).Trim();
                return int.TryParse(value, out int parsed) ? parsed : 0;
            }

            return 0;
        }
    }
}
#endif