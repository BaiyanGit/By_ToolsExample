//=====================================================
// File: RecorderStreamDemoController
// Description: Recorder SDK streaming demo controller.
//=====================================================

namespace Demos.RecorderSdk.Demo.Scripts
{
    using System;
    using System.Collections.Generic;
    using Core.Config;
    using Core.Events;
    using Core.Runtime;
    using UI.Settings;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.InputSystem.UI;
    using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
#endif

    /// <summary>
    /// Demo-only controller for RTMP/RTMPS streaming.
    /// It creates a temporary stream config for the entered URL and restores the
    /// user's previous config selection after the start request has completed.
    /// </summary>
    public class RecorderStreamDemoController : MonoBehaviour
    {
        [Header("Recorder")]
        public CrossPlatformScreenRecorder recorder;

        [Header("Scene")]
        public Transform rotateTarget;
        public float rotateSpeed = 40f;

        [Header("UI")]
        public InputField ifStreamUrl;
        public Dropdown drConfigId;
        public Button btnStartStream;
        public Button btnStopStream;
        public Text txtCurrentState;
        public Text txtLastWarningOrError;
        public Text txtDescription;

        private readonly List<RecorderParamsConfig> _configs = new();
        private RecorderConfigRegistry _registry;
        private RecorderParamsConfig _selectedConfig;
        private RecorderResult _lastResult;
        private RecorderErrorCode _lastErrorCode = RecorderErrorCode.None;
        private string _lastWarningOrError = "-";
        private bool _isRefreshingUi;

        private void Awake()
        {
            recorder = recorder != null ? recorder : GetComponent<CrossPlatformScreenRecorder>();
            if (recorder == null) recorder = CrossPlatformScreenRecorder.ins;
            if (recorder == null) recorder = gameObject.AddComponent<CrossPlatformScreenRecorder>();

            EnsureDemoSceneObjects();
            EnsureDemoUi();
            EnsureEventSystem();
            BindUiEvents();
            LoadConfigs();
            SubscribeRecorderEvents();
            RefreshStatus("Ready");
        }

        private void Update()
        {
            if (rotateTarget != null)
            {
                rotateTarget.Rotate(new Vector3(18f, 35f, 9f), rotateSpeed * Time.deltaTime, Space.World);
            }
        }

        private void OnDestroy()
        {
            UnsubscribeRecorderEvents();
        }

        private void BindUiEvents()
        {
            if (btnStartStream != null)
            {
                btnStartStream.onClick.RemoveListener(StartStream);
                btnStartStream.onClick.AddListener(StartStream);
            }

            if (btnStopStream != null)
            {
                btnStopStream.onClick.RemoveListener(StopStream);
                btnStopStream.onClick.AddListener(StopStream);
            }

            if (drConfigId != null)
            {
                drConfigId.onValueChanged.RemoveListener(ChangeConfig);
                drConfigId.onValueChanged.AddListener(ChangeConfig);
            }
        }

        private void LoadConfigs()
        {
            _registry = new RecorderConfigRegistry();
            _registry.Scan();
            var current = _registry.GetCurrentConfig();
            _selectedConfig = current.config;
            _configs.Clear();

            if (drConfigId == null) return;

            _isRefreshingUi = true;
            drConfigId.ClearOptions();
            var options = new List<string>();
            int selectedIndex = 0;
            for (int i = 0; i < _registry.Configs.Count; i++)
            {
                var config = _registry.Configs[i];
                if (config == null || IsTemporaryStreamDemoConfig(config.configId)) continue;
                _configs.Add(config);
                options.Add(GetConfigDropdownLabel(config));
                if (_selectedConfig != null && config.configId == _selectedConfig.configId) selectedIndex = _configs.Count - 1;
            }

            if (options.Count == 0) options.Add("No Config");
            drConfigId.AddOptions(options);
            drConfigId.SetValueWithoutNotify(Mathf.Clamp(selectedIndex, 0, options.Count - 1));
            _isRefreshingUi = false;

            if (_configs.Count > 0 && _selectedConfig == null) _selectedConfig = _configs[0];
            if (ifStreamUrl != null && _selectedConfig != null && !string.IsNullOrWhiteSpace(_selectedConfig.streamUrl))
            {
                ifStreamUrl.SetTextWithoutNotify(_selectedConfig.streamUrl);
            }
        }

        private void ChangeConfig(int index)
        {
            if (_isRefreshingUi || index < 0 || index >= _configs.Count) return;
            _selectedConfig = _configs[index];
            if (ifStreamUrl != null && _selectedConfig != null && !string.IsNullOrWhiteSpace(_selectedConfig.streamUrl))
            {
                ifStreamUrl.SetTextWithoutNotify(_selectedConfig.streamUrl);
            }

            RefreshStatus("Selected config: " + (_selectedConfig?.configId ?? "-"));
        }

        private static string GetConfigDropdownLabel(RecorderParamsConfig config)
        {
            if (config == null) return "Unnamed Config";
            if (!string.IsNullOrWhiteSpace(config.displayName)) return config.displayName;
            return string.IsNullOrWhiteSpace(config.configId) ? "Unnamed Config" : config.configId;
        }

        private async void StartStream()
        {
            string streamUrl = ifStreamUrl != null ? ifStreamUrl.text.Trim() : string.Empty;
            if (!IsRtmpUrl(streamUrl))
            {
                SetWarningOrError("Enter a valid RTMP / RTMPS stream URL.");
                RefreshStatus("Invalid stream URL.");
                return;
            }

            if (_selectedConfig == null)
            {
                SetWarningOrError("No available config. Create or select one in settings first.");
                RefreshStatus("No available config.");
                return;
            }

            string originalConfigId = _registry?.GetCurrentConfig().config?.configId ?? string.Empty;
            string tempConfigId = BuildTemporaryConfigId();

            try
            {
                var tempConfig = _selectedConfig.Clone();
                tempConfig.configId = tempConfigId;
                tempConfig.displayName = "Stream Demo Temporary";
                tempConfig.description = "Temporary config generated by RecorderStreamDemoController. Safe to delete.";
                tempConfig.isDefault = false;
                tempConfig.fileName = string.Empty;
                tempConfig.useMode = 1;
                tempConfig.streamUrl = streamUrl;
                if (ShouldForceVideoOnlyStream())
                {
                    tempConfig.streamIncludeAudio = false;
                }

                var save = _registry.SaveConfig(tempConfig);
                if (!save.success)
                {
                    SetWarningOrError(save.message);
                    RefreshStatus("Temporary stream config create failed.");
                    return;
                }

                var use = _registry.SetCurrentConfig(tempConfigId);
                if (!use.success)
                {
                    SetWarningOrError(use.message);
                    RefreshStatus("Temporary stream config switch failed.");
                    return;
                }

                _lastResult = await recorder.StartRecordingAsync();
                _lastErrorCode = _lastResult.errorCode;
                if (!_lastResult.success) SetWarningOrError(_lastResult.message);
                RefreshStatus(_lastResult.message);
            }
            catch (Exception exception)
            {
                SetWarningOrError(exception.Message);
                RefreshStatus("Start stream failed.");
            }
            finally
            {
                RestoreConfigAndDeleteTemporary(originalConfigId, tempConfigId);
            }
        }

        private async void StopStream()
        {
            _lastResult = await recorder.StopRecordingAsync();
            _lastErrorCode = _lastResult.errorCode;
            if (!_lastResult.success) SetWarningOrError(_lastResult.message);
            RefreshStatus(_lastResult.message);
        }

        private void RestoreConfigAndDeleteTemporary(string originalConfigId, string tempConfigId)
        {
            try
            {
                if (_registry == null) _registry = new RecorderConfigRegistry();
                if (!string.IsNullOrWhiteSpace(originalConfigId)) _registry.SetCurrentConfig(originalConfigId);
                if (!string.IsNullOrWhiteSpace(tempConfigId)) _registry.DeleteConfig(tempConfigId);
                recorder.ReloadConfig();
                _registry.Scan();
            }
            catch (Exception exception)
            {
                Debug.LogWarning("Recorder stream demo temporary config cleanup failed: " + exception.Message);
            }
        }

        private void SubscribeRecorderEvents()
        {
            if (recorder == null) return;
            recorder.OnRecorderStateChanged += OnRecorderEvent;
            recorder.OnRecorderWarning += OnRecorderWarning;
            recorder.OnRecorderError += OnRecorderError;
            recorder.OnRecorderProgress += OnRecorderEvent;
            recorder.OnConfigChanged += OnRecorderEvent;
        }

        private void UnsubscribeRecorderEvents()
        {
            if (recorder == null) return;
            recorder.OnRecorderStateChanged -= OnRecorderEvent;
            recorder.OnRecorderWarning -= OnRecorderWarning;
            recorder.OnRecorderError -= OnRecorderError;
            recorder.OnRecorderProgress -= OnRecorderEvent;
            recorder.OnConfigChanged -= OnRecorderEvent;
        }

        private void OnRecorderEvent(RecorderEventArgs args)
        {
            if (args != null && args.errorCode != RecorderErrorCode.None) _lastErrorCode = args.errorCode;
            RefreshStatus(args?.message ?? string.Empty);
        }

        private void OnRecorderWarning(RecorderEventArgs args)
        {
            if (args != null)
            {
                _lastErrorCode = args.errorCode;
                SetWarningOrError(args.message);
            }

            RefreshStatus(args?.message ?? string.Empty);
        }

        private void OnRecorderError(RecorderEventArgs args)
        {
            if (args != null)
            {
                _lastErrorCode = args.errorCode;
                SetWarningOrError(args.message);
            }

            RefreshStatus(args?.message ?? string.Empty);
        }

        private void RefreshStatus(string message)
        {
            if (txtCurrentState != null)
            {
                string configId = _selectedConfig != null ? _selectedConfig.configId : "-";
                txtCurrentState.text = $"State: {recorder.State}\nConfig: {configId}\nLast: {(string.IsNullOrWhiteSpace(message) ? "-" : message)}\nSession: {recorder.SessionHistory.Count}";
            }

            if (txtLastWarningOrError != null)
            {
                txtLastWarningOrError.text = $"Last error/warning: {_lastErrorCode}\n{_lastWarningOrError}";
            }
        }

        private void SetWarningOrError(string message)
        {
            _lastWarningOrError = string.IsNullOrWhiteSpace(message) ? "-" : message;
        }

        private static bool IsRtmpUrl(string value)
        {
            return !string.IsNullOrWhiteSpace(value) &&
                   (value.StartsWith("rtmp://", StringComparison.OrdinalIgnoreCase) ||
                    value.StartsWith("rtmps://", StringComparison.OrdinalIgnoreCase));
        }

        private static bool IsTemporaryStreamDemoConfig(string configId)
        {
            return !string.IsNullOrWhiteSpace(configId) &&
                   configId.StartsWith("create_stream_demo_temp_", StringComparison.OrdinalIgnoreCase);
        }

        private static string BuildTemporaryConfigId()
        {
            return "create_stream_demo_temp_" + DateTime.UtcNow.ToString("yyyyMMddHHmmssfff");
        }

        private static bool ShouldForceVideoOnlyStream()
        {
            return Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer;
        }

        private void EnsureDemoSceneObjects()
        {
            if (rotateTarget == null)
            {
                var cube = GameObject.Find("StreamDemo_RotatingCube");
                if (cube == null)
                {
                    cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    cube.name = "StreamDemo_RotatingCube";
                    cube.transform.position = new Vector3(0f, 1f, 0f);
                    cube.transform.localScale = new Vector3(1.6f, 1.6f, 1.6f);
                }

                rotateTarget = cube.transform;
            }

            if (Camera.main == null)
            {
                var cameraGo = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
                cameraGo.tag = "MainCamera";
                cameraGo.transform.position = new Vector3(0f, 2.2f, -6f);
                cameraGo.transform.rotation = Quaternion.Euler(18f, 0f, 0f);
                cameraGo.GetComponent<Camera>().clearFlags = CameraClearFlags.Skybox;
            }

            if (FindObjectOfType<Light>() == null)
            {
                var lightGo = new GameObject("Directional Light", typeof(Light));
                lightGo.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
                lightGo.GetComponent<Light>().type = LightType.Directional;
                lightGo.GetComponent<Light>().intensity = 1.2f;
            }
        }

        private void EnsureDemoUi()
        {
            if (ifStreamUrl != null && drConfigId != null && btnStartStream != null && btnStopStream != null && txtCurrentState != null && txtLastWarningOrError != null) return;

            var canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                var canvasGo = new GameObject("StreamDemoCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                canvas = canvasGo.GetComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                var scaler = canvasGo.GetComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1280f, 720f);
            }

            var panel = CreateRect("StreamDemoPanel", canvas.transform, new Vector2(460f, 430f), new Vector2(24f, -24f));
            panel.anchorMin = new Vector2(0f, 1f);
            panel.anchorMax = new Vector2(0f, 1f);
            panel.pivot = new Vector2(0f, 1f);
            var image = panel.gameObject.GetComponent<Image>() ?? panel.gameObject.AddComponent<Image>();
            image.color = new Color(0.07f, 0.09f, 0.11f, 0.92f);

            CreateText("TxtTitle", panel, new Vector2(20f, -18f), new Vector2(420f, 32f), "Recorder SDK 鎺ㄦ祦婕旂ず", 22, TextAnchor.MiddleLeft);
            CreateText("TxtUrlLabel", panel, new Vector2(20f, -64f), new Vector2(420f, 24f), "RTMP / RTMPS 鍦板潃", 14, TextAnchor.MiddleLeft);
            ifStreamUrl = CreateInputField("IfStreamUrl", panel, new Vector2(20f, -92f), new Vector2(420f, 34f), "rtmp://server/app/streamKey");

            CreateText("TxtConfigLabel", panel, new Vector2(20f, -136f), new Vector2(420f, 24f), "Config", 14, TextAnchor.MiddleLeft);
            drConfigId = CreateDropdown("DrConfigId", panel, new Vector2(20f, -164f), new Vector2(420f, 34f));

            btnStartStream = CreateButton("BtnStartStream", panel, new Vector2(20f, -214f), new Vector2(190f, 36f), "Start Stream");
            btnStopStream = CreateButton("BtnStopStream", panel, new Vector2(250f, -214f), new Vector2(190f, 36f), "Stop Stream");

            txtCurrentState = CreateText("TxtCurrentState", panel, new Vector2(20f, -268f), new Vector2(420f, 72f), "State: -", 14, TextAnchor.UpperLeft);
            txtLastWarningOrError = CreateText("TxtLastWarningOrError", panel, new Vector2(20f, -344f), new Vector2(420f, 52f), "Last error/warning: -", 14, TextAnchor.UpperLeft);
            txtDescription = CreateText("TxtDescription", panel, new Vector2(20f, -396f), new Vector2(420f, 88f), "Use a valid RTMP/RTMPS URL. Do not commit stream keys. Test video first; Windows stream audio requires FFmpeg wasapi support.", 12, TextAnchor.UpperLeft);
        }

        private static void EnsureEventSystem()
        {
            if (FindObjectOfType<EventSystem>() != null) return;
            var go = new GameObject("EventSystem", typeof(EventSystem));
#if ENABLE_INPUT_SYSTEM
            go.AddComponent<InputSystemUIInputModule>();
#else
            go.AddComponent<StandaloneInputModule>();
#endif
            DontDestroyOnLoad(go);
        }

        private static RectTransform CreateRect(string name, Transform parent, Vector2 size, Vector2 position)
        {
            var existing = parent != null ? parent.Find(name) : null;
            var go = existing != null ? existing.gameObject : new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.sizeDelta = size;
            rect.anchoredPosition = position;
            return rect;
        }

        private static Text CreateText(string name, Transform parent, Vector2 position, Vector2 size, string text, int fontSize, TextAnchor anchor)
        {
            var rect = CreateRect(name, parent, size, position);
            var label = rect.gameObject.GetComponent<Text>() ?? rect.gameObject.AddComponent<Text>();
            label.text = text;
            label.font = GetDefaultUiFont();
            label.fontSize = fontSize;
            label.color = Color.white;
            label.alignment = anchor;
            label.horizontalOverflow = HorizontalWrapMode.Wrap;
            label.verticalOverflow = VerticalWrapMode.Overflow;
            return label;
        }

        private static Font GetDefaultUiFont()
        {
            return Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }

        private static InputField CreateInputField(string name, Transform parent, Vector2 position, Vector2 size, string placeholder)
        {
            var rect = CreateRect(name, parent, size, position);
            var image = rect.gameObject.GetComponent<Image>() ?? rect.gameObject.AddComponent<Image>();
            image.color = Color.white;
            var field = rect.gameObject.GetComponent<InputField>() ?? rect.gameObject.AddComponent<InputField>();
            var text = CreateText(name + "_Text", rect, new Vector2(10f, -4f), new Vector2(size.x - 20f, size.y - 8f), string.Empty, 14, TextAnchor.MiddleLeft);
            text.color = Color.black;
            var hint = CreateText(name + "_Placeholder", rect, new Vector2(10f, -4f), new Vector2(size.x - 20f, size.y - 8f), placeholder, 14, TextAnchor.MiddleLeft);
            hint.color = new Color(0.45f, 0.45f, 0.45f, 1f);
            field.textComponent = text;
            field.placeholder = hint;
            DisableNavigation(field);
            return field;
        }

        private static Dropdown CreateDropdown(string name, Transform parent, Vector2 position, Vector2 size)
        {
            var rect = CreateRect(name, parent, size, position);
            var image = rect.gameObject.GetComponent<Image>() ?? rect.gameObject.AddComponent<Image>();
            image.color = Color.white;
            var dropdown = rect.gameObject.GetComponent<Dropdown>() ?? rect.gameObject.AddComponent<Dropdown>();
            var label = CreateText(name + "_Label", rect, new Vector2(10f, -4f), new Vector2(size.x - 40f, size.y - 8f), string.Empty, 14, TextAnchor.MiddleLeft);
            label.color = Color.black;
            dropdown.captionText = label;
            CreateDropdownTemplate(name, rect, dropdown, size);
            dropdown.ClearOptions();
            DisableNavigation(dropdown);
            return dropdown;
        }

        private static void CreateDropdownTemplate(string name, RectTransform dropdownRoot, Dropdown dropdown, Vector2 size)
        {
            var template = CreateRect(name + "_Template", dropdownRoot, new Vector2(size.x, 150f), new Vector2(0f, -size.y));
            template.gameObject.SetActive(false);
            var templateImage = template.gameObject.GetComponent<Image>() ?? template.gameObject.AddComponent<Image>();
            templateImage.color = new Color(0.92f, 0.92f, 0.92f, 1f);
            var scrollRect = template.gameObject.GetComponent<ScrollRect>() ?? template.gameObject.AddComponent<ScrollRect>();

            var viewport = CreateRect("Viewport", template, new Vector2(size.x, 150f), Vector2.zero);
            var viewportImage = viewport.gameObject.GetComponent<Image>() ?? viewport.gameObject.AddComponent<Image>();
            viewportImage.color = Color.white;
            var mask = viewport.gameObject.GetComponent<Mask>() ?? viewport.gameObject.AddComponent<Mask>();
            mask.showMaskGraphic = false;

            var content = CreateRect("Content", viewport, new Vector2(size.x, 32f), Vector2.zero);
            var layout = content.gameObject.GetComponent<VerticalLayoutGroup>() ?? content.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = true;

            var item = CreateRect("Item", content, new Vector2(size.x, 30f), Vector2.zero);
            var itemImage = item.gameObject.GetComponent<Image>() ?? item.gameObject.AddComponent<Image>();
            itemImage.color = new Color(0.86f, 0.9f, 0.96f, 1f);
            var toggle = item.gameObject.GetComponent<Toggle>() ?? item.gameObject.AddComponent<Toggle>();
            toggle.targetGraphic = itemImage;
            var itemLabel = CreateText("Item Label", item, new Vector2(10f, -4f), new Vector2(size.x - 20f, 24f), "Option", 14, TextAnchor.MiddleLeft);
            itemLabel.color = Color.black;
            DisableNavigation(toggle);

            scrollRect.content = content;
            scrollRect.viewport = viewport;
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            dropdown.template = template;
            dropdown.itemText = itemLabel;
        }

        private static Button CreateButton(string name, Transform parent, Vector2 position, Vector2 size, string text)
        {
            var rect = CreateRect(name, parent, size, position);
            var image = rect.gameObject.GetComponent<Image>() ?? rect.gameObject.AddComponent<Image>();
            image.color = new Color(0.18f, 0.36f, 0.62f, 1f);
            var button = rect.gameObject.GetComponent<Button>() ?? rect.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            CreateText(name + "_Text", rect, Vector2.zero, size, text, 16, TextAnchor.MiddleCenter);
            DisableNavigation(button);
            return button;
        }

        private static void DisableNavigation(Selectable selectable)
        {
            if (selectable == null) return;

            Navigation navigation = selectable.navigation;
            navigation.mode = Navigation.Mode.None;
            selectable.navigation = navigation;
        }
    }
}
