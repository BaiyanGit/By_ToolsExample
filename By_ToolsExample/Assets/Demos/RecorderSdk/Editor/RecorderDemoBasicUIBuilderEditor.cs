//=====================================================
// 文件名称: RecorderDemoUIBuilderEditor
// 创 建 者: wangbaiyan
// 创建日期: 2026-05-25
// 描    述: Recorder SDK 屏幕录制界面编辑器生成工具，用于一次性创建并绑定场景 UI。
//=====================================================

#if UNITY_EDITOR
namespace Demos.RecorderSdk.Editor
{
    using System.IO;
    using Demos.RecorderSdk.Core.Runtime;
    using Demos.RecorderSdk.Demo.Scripts;
    using UnityEditor;
    using UnityEditor.SceneManagement;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.InputSystem.UI;
    using UnityEngine.SceneManagement;
    using UnityEngine.UI;

    /// <summary>
    /// 在当前打开场景中创建屏幕录制 Demo UI，并绑定到 RecorderDemoBasicController。
    /// </summary>
    public static class RecorderDemoUIBuilderEditor
    {
        private const string CANVAS_NAME = "Recorder Demo Canvas";
        private const string PANEL_NAME = "Recorder Demo Panel";
        private const string RECORDER_MANAGER_NAME = "RecorderManager";
        private const int ACTIVE_INPUT_HANDLER_INPUT_SYSTEM = 1;
        private const int ACTIVE_INPUT_HANDLER_BOTH = 2;

        /// <summary>
        /// 创建屏幕录制 UI 并保存引用到控制器。
        /// </summary>
        public static void CreateBasicDemoUI()
        {
            var recorderManager            = GetOrCreateGameObject(RECORDER_MANAGER_NAME);
            var recorder                   = recorderManager.GetComponent<CrossPlatformScreenRecorder>();
            if (recorder == null) recorder = Undo.AddComponent<CrossPlatformScreenRecorder>(recorderManager);

            var controller                     = recorderManager.GetComponent<RecorderDemoBasicController>();
            if (controller == null) controller = Undo.AddComponent<RecorderDemoBasicController>(recorderManager);
            controller.recorder = recorder;

            var canvas = GetOrCreateCanvas();
            var panel  = GetOrCreatePanel(canvas.transform);

            controller.drConfigId         = GetOrCreateDropdown(panel, "当前配置ID下拉框", new Vector2(0f, -24f), "当前配置ID");
            HideObsoleteAudioControls(panel, controller);

            controller.btnStartRecording      = GetOrCreateButton(panel, "开始录制按钮", new Vector2(0f, -78f), "开始录制");
            controller.btnStopRecording       = GetOrCreateButton(panel, "停止录制按钮", new Vector2(170f, -78f), "停止录制");
            controller.btnOpenOutputFolder    = GetOrCreateButton(panel, "打开输出目录按钮", new Vector2(0f, -120f), "打开输出目录");
            controller.btnClearSessionHistory = GetOrCreateButton(panel, "清空历史按钮", new Vector2(170f, -120f), "清空历史");

            controller.txtCurrentState    = GetOrCreateText(panel, "当前状态文本", new Vector2(0f, -174f), new Vector2(380f, 26f), "当前状态：-", 14, TextAnchor.MiddleLeft);
            controller.txtLastResult      = GetOrCreateText(panel, "最近结果文本", new Vector2(0f, -204f), new Vector2(380f, 44f), "最近结果：-", 14, TextAnchor.UpperLeft);
            controller.txtCurrentConfigId = GetOrCreateText(panel, "当前配置ID文本", new Vector2(0f, -252f), new Vector2(380f, 26f), "当前配置ID：-", 14, TextAnchor.MiddleLeft);
            controller.txtOutputPath      = GetOrCreateText(panel, "输出路径文本", new Vector2(0f, -282f), new Vector2(380f, 44f), "输出路径：-", 14, TextAnchor.UpperLeft);
            controller.txtLastErrorCode   = GetOrCreateText(panel, "最近错误码文本", new Vector2(0f, -330f), new Vector2(380f, 26f), "最近错误码：None", 14, TextAnchor.MiddleLeft);
            controller.txtSessionCount    = GetOrCreateText(panel, "会话数量文本", new Vector2(0f, -360f), new Vector2(380f, 26f), "会话数量：0", 14, TextAnchor.MiddleLeft);

            EnsureEventSystem();
            EditorUtility.SetDirty(controller);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            Debug.Log("Recorder SDK 屏幕录制界面已创建并绑定。请保存当前场景。");
        }

        /// <summary>
        /// 获取或创建指定名称的 GameObject。
        /// </summary>
        private static GameObject GetOrCreateGameObject(string name)
        {
            var existing = GameObject.Find(name);
            if (existing != null) return existing;
            var go = new GameObject(name);
            Undo.RegisterCreatedObjectUndo(go, "Create " + name);
            return go;
        }

        /// <summary>
        /// 获取或创建 Canvas。
        /// </summary>
        private static Canvas GetOrCreateCanvas()
        {
            var existing = GameObject.Find(CANVAS_NAME);
            if (existing != null && existing.TryGetComponent(out Canvas existingCanvas)) return existingCanvas;

            var go = new GameObject(CANVAS_NAME, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Undo.RegisterCreatedObjectUndo(go, "Create Recorder Demo Canvas");
            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280f, 720f);
            return canvas;
        }

        /// <summary>
        /// 获取或创建主面板。
        /// </summary>
        private static RectTransform GetOrCreatePanel(Transform parent)
        {
            var existing = FindChild(parent, PANEL_NAME);
            if (existing != null)
            {
                var existingRect = existing.GetComponent<RectTransform>();
                existingRect.sizeDelta = new Vector2(420f, 400f);
                return existingRect;
            }

            var go = new GameObject(PANEL_NAME, typeof(RectTransform), typeof(Image));
            Undo.RegisterCreatedObjectUndo(go, "Create Recorder Demo Panel");
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin                 = new Vector2(0f, 1f);
            rect.anchorMax                 = new Vector2(0f, 1f);
            rect.pivot                     = new Vector2(0f, 1f);
            rect.sizeDelta                 = new Vector2(420f, 400f);
            rect.anchoredPosition          = new Vector2(24f, -24f);
            go.GetComponent<Image>().color = new Color(0.08f, 0.1f, 0.12f, 0.92f);
            return rect;
        }

        /// <summary>
        /// 隐藏旧版屏幕录制音频快捷控件，音频参数统一从配置文件读取。
        /// </summary>
        private static void HideObsoleteAudioControls(Transform panel, RecorderDemoBasicController controller)
        {
            controller.togEnableAudio = HideObsoleteSelectable<Toggle>(panel, "启用音频开关");
            controller.togEnableAudioGain = HideObsoleteSelectable<Toggle>(panel, "启用音量增强开关");
            controller.slAudioGainDb = HideObsoleteSelectable<Slider>(panel, "音量增益滑动条");
            controller.txtAudioGainDb = HideObsoleteGraphic<Text>(panel, "音量增益文本");
        }

        private static T HideObsoleteSelectable<T>(Transform parent, string name) where T : Selectable
        {
            var existing = FindChild(parent, name);
            if (existing == null || !existing.TryGetComponent(out T selectable)) return null;
            selectable.gameObject.SetActive(false);
            return selectable;
        }

        private static T HideObsoleteGraphic<T>(Transform parent, string name) where T : Graphic
        {
            var existing = FindChild(parent, name);
            if (existing == null || !existing.TryGetComponent(out T graphic)) return null;
            graphic.gameObject.SetActive(false);
            return graphic;
        }

        /// <summary>
        /// 获取或创建按钮。
        /// </summary>
        private static Button GetOrCreateButton(Transform parent, string name, Vector2 position, string label)
        {
            var existing = FindChild(parent, name);
            if (existing != null && existing.TryGetComponent(out Button existingButton))
            {
                DisableNavigation(existingButton);
                return existingButton;
            }

            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            Undo.RegisterCreatedObjectUndo(go, "Create " + name);
            go.transform.SetParent(parent, false);
            var rect = SetupTopLeftRect(go, new Vector2(150f, 32f), position);
            go.GetComponent<Image>().color = new Color(0.18f, 0.32f, 0.52f, 1f);
            GetOrCreateText(rect, "Text", Vector2.zero, rect.sizeDelta, label, 14, TextAnchor.MiddleCenter);
            var button = go.GetComponent<Button>();
            DisableNavigation(button);
            return button;
        }

        /// <summary>
        /// 获取或创建 Text。
        /// </summary>
        private static Text GetOrCreateText(Transform parent, string name, Vector2 position, Vector2 size, string value, int fontSize, TextAnchor alignment)
        {
            var existing = FindChild(parent, name);
            if (existing != null && existing.TryGetComponent(out Text existingText))
            {
                existingText.text = value;
                existingText.font = GetDefaultUnityFont();
                return existingText;
            }

            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            Undo.RegisterCreatedObjectUndo(go, "Create " + name);
            go.transform.SetParent(parent, false);
            SetupTopLeftRect(go, size, position);
            var text = go.GetComponent<Text>();
            text.font      = GetDefaultUnityFont();
            text.fontSize  = fontSize;
            text.color     = Color.white;
            text.alignment = alignment;
            text.text      = value;
            return text;
        }

        /// <summary>
        /// 获取或创建 Toggle。
        /// </summary>
        private static Toggle GetOrCreateToggle(Transform parent, string name, Vector2 position, string label)
        {
            var existing = FindChild(parent, name);
            if (existing != null && existing.TryGetComponent(out Toggle existingToggle))
            {
                DisableNavigation(existingToggle);
                return existingToggle;
            }

            var go = new GameObject(name, typeof(RectTransform), typeof(Toggle));
            Undo.RegisterCreatedObjectUndo(go, "Create " + name);
            go.transform.SetParent(parent, false);
            SetupTopLeftRect(go, new Vector2(260f, 28f), position);

            var background = new GameObject("Background", typeof(RectTransform), typeof(Image));
            background.transform.SetParent(go.transform, false);
            var bgRect = background.GetComponent<RectTransform>();
            bgRect.sizeDelta        = new Vector2(20f, 20f);
            bgRect.anchorMin        = bgRect.anchorMax = new Vector2(0f, 0.5f);
            bgRect.anchoredPosition = new Vector2(10f, 0f);

            var checkmark = new GameObject("Checkmark", typeof(RectTransform), typeof(Image));
            checkmark.transform.SetParent(background.transform, false);
            checkmark.GetComponent<RectTransform>().sizeDelta = new Vector2(14f, 14f);
            checkmark.GetComponent<Image>().color             = new Color(0.2f, 0.8f, 0.45f, 1f);

            var toggle = go.GetComponent<Toggle>();
            toggle.targetGraphic = background.GetComponent<Image>();
            toggle.graphic       = checkmark.GetComponent<Image>();
            GetOrCreateText(go.transform, "Label", new Vector2(36f, 0f), new Vector2(220f, 28f), label, 14, TextAnchor.MiddleLeft);
            DisableNavigation(toggle);
            return toggle;
        }

        /// <summary>
        /// 获取或创建 Slider。
        /// </summary>
        private static Slider GetOrCreateSlider(Transform parent, string name, Vector2 position)
        {
            var existing = FindChild(parent, name);
            if (existing != null && existing.TryGetComponent(out Slider existingSlider))
            {
                DisableNavigation(existingSlider);
                return existingSlider;
            }

            var go = new GameObject(name, typeof(RectTransform), typeof(Slider));
            Undo.RegisterCreatedObjectUndo(go, "Create " + name);
            go.transform.SetParent(parent, false);
            SetupTopLeftRect(go, new Vector2(220f, 28f), position);

            var background = CreateStretchImage(go.transform, "Background", new Color(0.2f, 0.22f, 0.24f, 1f));
            background.anchorMin = new Vector2(0f, 0.5f);
            background.anchorMax = new Vector2(1f, 0.5f);
            background.sizeDelta = new Vector2(0f, 6f);

            var fill = CreateStretchImage(go.transform, "Fill", new Color(0.2f, 0.65f, 0.95f, 1f));
            fill.anchorMin = new Vector2(0f, 0.5f);
            fill.anchorMax = new Vector2(0f, 0.5f);
            fill.sizeDelta = new Vector2(0f, 6f);

            var handle = new GameObject("Handle", typeof(RectTransform), typeof(Image));
            handle.transform.SetParent(go.transform, false);
            var handleRect = handle.GetComponent<RectTransform>();
            handleRect.sizeDelta               = new Vector2(18f, 18f);
            handle.GetComponent<Image>().color = Color.white;

            var slider = go.GetComponent<Slider>();
            slider.fillRect      = fill;
            slider.handleRect    = handleRect;
            slider.targetGraphic = handle.GetComponent<Image>();
            slider.minValue      = -20f;
            slider.maxValue      = 20f;
            slider.value         = 6f;
            DisableNavigation(slider);
            return slider;
        }

        /// <summary>
        /// 获取或创建 Dropdown。
        /// </summary>
        private static Dropdown GetOrCreateDropdown(Transform parent, string name, Vector2 position, string label)
        {
            var existing = FindChild(parent, name);
            if (existing != null && existing.TryGetComponent(out Dropdown existingDropdown))
            {
                DisableNavigation(existingDropdown);
                return existingDropdown;
            }

            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Dropdown));
            Undo.RegisterCreatedObjectUndo(go, "Create " + name);
            go.transform.SetParent(parent, false);
            SetupTopLeftRect(go, new Vector2(380f, 32f), position);
            go.GetComponent<Image>().color = new Color(0.16f, 0.18f, 0.2f, 1f);
            var caption  = GetOrCreateText(go.transform, "Label", new Vector2(12f, 0f), new Vector2(340f, 32f), label, 14, TextAnchor.MiddleLeft);
            var dropdown = go.GetComponent<Dropdown>();
            dropdown.captionText   = caption;
            dropdown.targetGraphic = go.GetComponent<Image>();
            dropdown.template      = CreateDropdownTemplate(go.transform, dropdown);
            DisableNavigation(dropdown);
            return dropdown;
        }

        /// <summary>
        /// 创建 Dropdown 模板。
        /// </summary>
        private static RectTransform CreateDropdownTemplate(Transform parent, Dropdown dropdown)
        {
            var template = new GameObject("Template", typeof(RectTransform), typeof(Image), typeof(ScrollRect));
            template.SetActive(false);
            template.transform.SetParent(parent, false);
            var templateRect = template.GetComponent<RectTransform>();
            templateRect.anchorMin               = new Vector2(0f, 0f);
            templateRect.anchorMax               = new Vector2(1f, 0f);
            templateRect.pivot                   = new Vector2(0.5f, 1f);
            templateRect.sizeDelta               = new Vector2(0f, 150f);
            templateRect.anchoredPosition        = new Vector2(0f, -2f);
            template.GetComponent<Image>().color = new Color(0.12f, 0.14f, 0.16f, 1f);

            var viewport = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
            viewport.transform.SetParent(template.transform, false);
            var viewportRect = viewport.GetComponent<RectTransform>();
            viewportRect.anchorMin                        = Vector2.zero;
            viewportRect.anchorMax                        = Vector2.one;
            viewportRect.sizeDelta                        = Vector2.zero;
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
            itemRect.sizeDelta = new Vector2(0f, 28f);

            var itemBackground = CreateStretchImage(item.transform, "Item Background", new Color(0.18f, 0.2f, 0.22f, 1f));
            var itemCheckmark  = new GameObject("Item Checkmark", typeof(RectTransform), typeof(Image));
            itemCheckmark.transform.SetParent(item.transform, false);
            var markRect                                                   = itemCheckmark.GetComponent<RectTransform>();
            markRect.anchorMin                        = markRect.anchorMax = new Vector2(0f, 0.5f);
            markRect.sizeDelta                        = new Vector2(12f, 12f);
            markRect.anchoredPosition                 = new Vector2(10f, 0f);
            itemCheckmark.GetComponent<Image>().color = new Color(0.2f, 0.8f, 0.45f, 1f);

            var itemLabel  = GetOrCreateText(item.transform, "Item Label", new Vector2(28f, 0f), new Vector2(320f, 28f), "Option", 14, TextAnchor.MiddleLeft);
            var itemToggle = item.GetComponent<Toggle>();
            itemToggle.targetGraphic = itemBackground.GetComponent<Image>();
            itemToggle.graphic       = itemCheckmark.GetComponent<Image>();
            DisableNavigation(itemToggle);

            var scrollRect = template.GetComponent<ScrollRect>();
            scrollRect.content    = contentRect;
            scrollRect.viewport   = viewportRect;
            scrollRect.horizontal = false;

            dropdown.itemText  = itemLabel;
            dropdown.itemImage = null;
            return templateRect;
        }

        private static void DisableNavigation(Selectable selectable)
        {
            if (selectable == null) return;

            var navigation = selectable.navigation;
            navigation.mode = Navigation.Mode.None;
            selectable.navigation = navigation;
        }

        /// <summary>
        /// 创建拉伸 Image。
        /// </summary>
        private static RectTransform CreateStretchImage(Transform parent, string name, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin                 = Vector2.zero;
            rect.anchorMax                 = Vector2.one;
            rect.sizeDelta                 = Vector2.zero;
            go.GetComponent<Image>().color = color;
            return rect;
        }

        /// <summary>
        /// 设置左上角锚点 RectTransform。
        /// </summary>
        private static RectTransform SetupTopLeftRect(GameObject go, Vector2 size, Vector2 position)
        {
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin        = new Vector2(0f, 1f);
            rect.anchorMax        = new Vector2(0f, 1f);
            rect.pivot            = new Vector2(0f, 1f);
            rect.sizeDelta        = size;
            rect.anchoredPosition = position;
            return rect;
        }

        /// <summary>
        /// 查找子对象。
        /// </summary>
        private static Transform FindChild(Transform parent, string name)
        {
            for (int i = 0; i < parent.childCount; i++)
            {
                var child = parent.GetChild(i);
                if (child.name == name) return child;
            }

            return null;
        }

        /// <summary>
        /// 确保 EventSystem 存在。
        /// </summary>
        private static void EnsureEventSystem()
        {
            if (Object.FindFirstObjectByType<EventSystem>() != null) return;
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

        /// <summary>
        /// 获取 Unity 默认运行时字体。
        /// </summary>
        private static Font GetDefaultUnityFont()
        {
            return Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }
    }
}
#endif
