namespace Demos.示例_录制视频Recorder.Scripts.UISettings
{
    using UnityEngine;
    using UnityEngine.UI;

    public partial class UIRecorderParamsSettings
    {
        /// <summary>
        /// 执行 EnsureUseButton 相关逻辑
        /// </summary>
        private void EnsureUseButton()
        {
            if (btnUse != null || btnSave == null) return;
            var go = Instantiate(btnSave.gameObject, btnSave.transform.parent);
            go.name = "BtnUse";
            btnUse  = go.GetComponent<Button>();

            if (btnUse != null) btnUse.onClick.RemoveAllListeners();

            var rect     = go.GetComponent<RectTransform>();
            var saveRect = btnSave.GetComponent<RectTransform>();
            if (rect != null && saveRect != null)
                rect.anchoredPosition = saveRect.anchoredPosition + new Vector2(saveRect.rect.width + 12f, 0f);

            var text                    = go.GetComponentInChildren<Text>(true);
            if (text != null) text.text = "使用";
        }

        /// <summary>
        /// 确保删除用户配置按钮存在
        /// </summary>
        private void EnsureDeleteButton()
        {
            if (btnDeleteConfig != null) return;
            foreach (var button in GetComponentsInChildren<Button>(true))
            {
                if (button == null) continue;
                if (button.name != "BtnDeleteConfig" && button.name != "BtnDelete") continue;
                btnDeleteConfig = button;
                btnDeleteConfig.onClick.RemoveAllListeners();
                return;
            }

            Button sourceButton = btnUse != null ? btnUse : btnSave;
            if (sourceButton == null) return;
            var go = Instantiate(sourceButton.gameObject, sourceButton.transform.parent);
            go.name         = "BtnDeleteConfig";
            btnDeleteConfig = go.GetComponent<Button>();
            if (btnDeleteConfig != null) btnDeleteConfig.onClick.RemoveAllListeners();

            var rect       = go.GetComponent<RectTransform>();
            var sourceRect = sourceButton.GetComponent<RectTransform>();
            if (rect != null && sourceRect != null)
                rect.anchoredPosition = sourceRect.anchoredPosition + new Vector2(sourceRect.rect.width + 12f, 0f);

            var text                    = go.GetComponentInChildren<Text>(true);
            if (text != null) text.text = "删除";
        }

        /// <summary>
        /// 确保视频推流地址输入框存在；场景已绑定时直接复用，未绑定时创建一个兜底输入框。
        /// </summary>
        private void EnsureStreamUrlInput()
        {
            if (ifStreamUrl != null) return;
            Transform parent = goVideoSavePathRoot != null && goVideoSavePathRoot.transform.parent != null
                                   ? goVideoSavePathRoot.transform.parent
                                   : transform;

            var root = CreatePanel("StreamUrlRoot", parent, new Vector2(520f, 42f), Vector2.zero);
            goStreamUrlRoot = root;
            CreateText("TxtStreamUrlLabel", root.transform, "推流地址", new Vector2(100f, 32f), new Vector2(-205f, 0f), 16, TextAnchor.MiddleLeft);
            ifStreamUrl = CreateInputField("IfStreamUrl", root.transform, new Vector2(390f, 34f), new Vector2(55f, 0f), "rtmp://server/app/streamKey");

            var rootRect = root.GetComponent<RectTransform>();
            var sourceRect = goVideoSavePathRoot != null ? goVideoSavePathRoot.GetComponent<RectTransform>() : null;
            if (rootRect != null && sourceRect != null)
            {
                rootRect.anchorMin        = sourceRect.anchorMin;
                rootRect.anchorMax        = sourceRect.anchorMax;
                rootRect.pivot            = sourceRect.pivot;
                rootRect.anchoredPosition = sourceRect.anchoredPosition;
            }

            root.SetActive(false);
        }

        /// <summary>
        /// 确保推流专用参数控件存在；场景已绑定则复用，未绑定时创建兜底控件。
        /// </summary>
        private void EnsureStreamSettingsControls()
        {
            if (goStreamSettingsRoot != null) return;
            Transform parent = goStreamUrlRoot != null && goStreamUrlRoot.transform.parent != null
                                   ? goStreamUrlRoot.transform.parent
                                   : transform;

            goStreamSettingsRoot = CreatePanel("StreamSettingsRoot", parent, new Vector2(720f, 210f), new Vector2(0f, -52f));
            drStreamVideoBitrate = CreateLabeledDropdown("DrStreamVideoBitrate", goStreamSettingsRoot.transform, "推流码率", new Vector2(-220f, 70f));
            drStreamGop = CreateLabeledDropdown("DrStreamGop", goStreamSettingsRoot.transform, "GOP", new Vector2(120f, 70f));
            drStreamBufferSize = CreateLabeledDropdown("DrStreamBufferSize", goStreamSettingsRoot.transform, "缓冲区", new Vector2(-220f, 20f));
            drStreamReconnectCount = CreateLabeledDropdown("DrStreamReconnectCount", goStreamSettingsRoot.transform, "重连次数", new Vector2(120f, 20f));
            drStreamReconnectInterval = CreateLabeledDropdown("DrStreamReconnectInterval", goStreamSettingsRoot.transform, "重连间隔", new Vector2(-220f, -30f));
            togStreamLowLatency = CreateToggle("TogStreamLowLatency", goStreamSettingsRoot.transform, "低延迟", new Vector2(120f, -30f));
            togStreamAutoReconnect = CreateToggle("TogStreamAutoReconnect", goStreamSettingsRoot.transform, "自动重连", new Vector2(-220f, -80f));
            togStreamIncludeAudio = CreateToggle("TogStreamIncludeAudio", goStreamSettingsRoot.transform, "包含系统音频", new Vector2(120f, -80f));
            goStreamSettingsRoot.SetActive(false);
        }

        /// <summary>
        /// 执行 EnsureSaveAsWindow 相关逻辑
        /// </summary>
        private void EnsureSaveAsWindow()
        {
            if (saveAsWindowRoot != null)
            {
                if (txtSaveAsTips == null)
                {
                    txtSaveAsTips = saveAsWindowRoot.transform.Find("TxtSaveAsTips") != null
                                        ? saveAsWindowRoot.transform.Find("TxtSaveAsTips").GetComponent<Text>()
                                        : CreateText("TxtSaveAsTips", saveAsWindowRoot.transform, string.Empty, new Vector2(390f, 28f), new Vector2(0f, -18f), 14, TextAnchor.MiddleLeft);
                    txtSaveAsTips.color = new Color(1f, 0.45f, 0.35f, 1f);

                    txtSaveAsTips.gameObject.SetActive(false);
                }

                return;
            }

            var canvas = GetCanvasTransform();
            if (canvas == null) return;
            saveAsWindowRoot = CreatePanel("SaveAsConfigWindow", canvas, new Vector2(460f, 190f), Vector2.zero);
            CreateText("TxtTitle", saveAsWindowRoot.transform, "另存为配置", new Vector2(420f, 32f), new Vector2(0f, 62f), 20, TextAnchor.MiddleCenter);
            ifSaveAsName        = CreateInputField("IfSaveAsName", saveAsWindowRoot.transform, new Vector2(390f, 36f), new Vector2(0f, 18f), "配置名称");
            btnSaveAsConfirm    = CreateButton("BtnConfirmSaveAs", saveAsWindowRoot.transform, "确定", new Vector2(120f, 36f), new Vector2(-72f, -54f));
            btnSaveAsCancel     = CreateButton("BtnCancelSaveAs", saveAsWindowRoot.transform, "取消", new Vector2(120f, 36f), new Vector2(72f, -54f));
            txtSaveAsTips       = CreateText("TxtSaveAsTips", saveAsWindowRoot.transform, string.Empty, new Vector2(390f, 28f), new Vector2(0f, -18f), 14, TextAnchor.MiddleLeft);
            txtSaveAsTips.color = new Color(1f, 0.45f, 0.35f, 1f);
            txtSaveAsTips.gameObject.SetActive(false);
            saveAsWindowRoot.SetActive(false);
        }

        /// <summary>
        /// 确保通用操作提示窗口存在
        /// </summary>
        private void EnsureMessageWindow()
        {
            if (_messageWindowRoot != null) return;
            var canvas = GetCanvasTransform();
            if (canvas == null) return;
            _messageWindowRoot = CreatePanel("RecorderMessageWindow", canvas, new Vector2(420f, 170f), Vector2.zero);
            CreateText("TxtTitle", _messageWindowRoot.transform, "提示", new Vector2(380f, 30f), new Vector2(0f, 52f), 20, TextAnchor.MiddleCenter);
            _txtMessageContent = CreateText("TxtContent", _messageWindowRoot.transform, string.Empty, new Vector2(360f, 54f), new Vector2(0f, 8f), 16, TextAnchor.MiddleCenter);
            _btnMessageConfirm = CreateButton("BtnConfirm", _messageWindowRoot.transform, "确定", new Vector2(120f, 36f), new Vector2(0f, -52f));
            if (_btnMessageConfirm != null) _btnMessageConfirm.onClick.AddListener(HideMessageTips);
            _messageWindowRoot.SetActive(false);
        }

        /// <summary>
        /// 执行 EnsureOptionDescWindow 相关逻辑
        /// </summary>
        private void EnsureOptionDescWindow()
        {
            if (optionDescWindowRoot != null) return;
            var canvas = GetCanvasTransform();
            if (canvas == null) return;
            optionDescWindowRoot = CreatePanel("RecorderOptionDescWindow", canvas, new Vector2(520f, 220f), Vector2.zero);
            txtOptionDescTitle   = CreateText("TxtDescTitle", optionDescWindowRoot.transform, "选项说明", new Vector2(480f, 28f), new Vector2(0f, 84f), 18, TextAnchor.MiddleLeft);
            txtOptionDescContent = CreateText("TxtDescContent", optionDescWindowRoot.transform, string.Empty, new Vector2(480f, 150f), new Vector2(0f, -8f), 15, TextAnchor.UpperLeft);
            optionDescWindowRoot.SetActive(false);
        }

        /// <summary>
        /// 执行 getCanvasTransform 相关逻辑
        /// </summary>
        private Transform GetCanvasTransform()
        {
            var canvas                 = GetComponentInParent<Canvas>();
            if (canvas == null) canvas = FindFirstObjectByType<Canvas>();
            return canvas != null ? canvas.transform : null;
        }

        /// <summary>
        /// 执行 CreatePanel 相关逻辑
        /// </summary>
        private static GameObject CreatePanel(string goName, Transform parent, Vector2 size, Vector2 anchoredPosition)
        {
            var go = new GameObject(goName, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.sizeDelta                 = size;
            rect.anchoredPosition          = anchoredPosition;
            go.GetComponent<Image>().color = new Color(0.12f, 0.14f, 0.16f, 0.96f);
            return go;
        }

        /// <summary>
        /// 执行 CreateText 相关逻辑
        /// </summary>
        private static Text CreateText(string goName, Transform parent, string content, Vector2 size, Vector2 anchoredPosition, int fontSize, TextAnchor alignment)
        {
            var go = new GameObject(goName, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.sizeDelta        = size;
            rect.anchoredPosition = anchoredPosition;
            var text = go.GetComponent<Text>();
            text.font               = GetBuiltinFont();
            text.text               = content;
            text.fontSize           = fontSize;
            text.alignment          = alignment;
            text.color              = Color.white;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow   = VerticalWrapMode.Overflow;
            return text;
        }

        /// <summary>
        /// 执行 CreateInputField 相关逻辑
        /// </summary>
        private static InputField CreateInputField(string goName, Transform parent, Vector2 size, Vector2 anchoredPosition, string placeholder)
        {
            var go = new GameObject(goName, typeof(RectTransform), typeof(Image), typeof(InputField));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.sizeDelta                 = size;
            rect.anchoredPosition          = anchoredPosition;
            go.GetComponent<Image>().color = Color.white;
            var text = CreateText("Text", go.transform, string.Empty, size - new Vector2(20f, 8f), Vector2.zero, 16, TextAnchor.MiddleLeft);
            text.color = Color.black;
            var hint = CreateText("Placeholder", go.transform, placeholder, size - new Vector2(20f, 8f), Vector2.zero, 16, TextAnchor.MiddleLeft);
            hint.color = new Color(0.45f, 0.45f, 0.45f, 1f);
            var input = go.GetComponent<InputField>();
            input.textComponent = text;
            input.placeholder   = hint;
            return input;
        }

        /// <summary>
        /// 执行 CreateButton 相关逻辑
        /// </summary>
        private static Button CreateButton(string goName, Transform parent, string label, Vector2 size, Vector2 anchoredPosition)
        {
            var go = new GameObject(goName, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.sizeDelta                 = size;
            rect.anchoredPosition          = anchoredPosition;
            go.GetComponent<Image>().color = new Color(0.22f, 0.45f, 0.78f, 1f);
            CreateText("Text", go.transform, label, size, Vector2.zero, 16, TextAnchor.MiddleCenter);
            return go.GetComponent<Button>();
        }

        /// <summary>
        /// 创建带标签的下拉框。
        /// </summary>
        private Dropdown CreateLabeledDropdown(string goName, Transform parent, string label, Vector2 anchoredPosition)
        {
            var root = new GameObject(goName + "Root", typeof(RectTransform));
            root.transform.SetParent(parent, false);
            var rootRect = root.GetComponent<RectTransform>();
            rootRect.sizeDelta = new Vector2(300f, 38f);
            rootRect.anchoredPosition = anchoredPosition;
            CreateText("TxtLabel", root.transform, label, new Vector2(96f, 32f), new Vector2(-102f, 0f), 15, TextAnchor.MiddleLeft);
            var source = webmVideoBitrate != null ? webmVideoBitrate : videoCaptureFrameRate;
            var dropdownGo = source != null
                                 ? Instantiate(source.gameObject, root.transform)
                                 : new GameObject(goName, typeof(RectTransform), typeof(Image), typeof(Dropdown));
            dropdownGo.name = goName;
            dropdownGo.transform.SetParent(root.transform, false);
            var rect = dropdownGo.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(180f, 34f);
            rect.anchoredPosition = new Vector2(56f, 0f);
            var dropdown = dropdownGo.GetComponent<Dropdown>();
            dropdown.onValueChanged.RemoveAllListeners();
            if (dropdown.captionText == null)
            {
                dropdownGo.GetComponent<Image>().color = Color.white;
                var labelText = CreateText("Label", dropdownGo.transform, string.Empty, new Vector2(150f, 28f), new Vector2(-10f, 0f), 15, TextAnchor.MiddleLeft);
                labelText.color = Color.black;
                dropdown.captionText = labelText;
            }
            return dropdown;
        }

        /// <summary>
        /// 创建开关控件。
        /// </summary>
        private static Toggle CreateToggle(string goName, Transform parent, string label, Vector2 anchoredPosition)
        {
            var root = new GameObject(goName, typeof(RectTransform), typeof(Toggle));
            root.transform.SetParent(parent, false);
            var rect = root.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(220f, 34f);
            rect.anchoredPosition = anchoredPosition;
            var background = new GameObject("Background", typeof(RectTransform), typeof(Image));
            background.transform.SetParent(root.transform, false);
            var bgRect = background.GetComponent<RectTransform>();
            bgRect.sizeDelta = new Vector2(22f, 22f);
            bgRect.anchoredPosition = new Vector2(-90f, 0f);
            background.GetComponent<Image>().color = Color.white;
            var checkmark = new GameObject("Checkmark", typeof(RectTransform), typeof(Image));
            checkmark.transform.SetParent(background.transform, false);
            var ckRect = checkmark.GetComponent<RectTransform>();
            ckRect.sizeDelta = new Vector2(14f, 14f);
            checkmark.GetComponent<Image>().color = new Color(0.22f, 0.45f, 0.78f, 1f);
            CreateText("Text", root.transform, label, new Vector2(170f, 30f), new Vector2(8f, 0f), 15, TextAnchor.MiddleLeft);
            var toggle = root.GetComponent<Toggle>();
            toggle.targetGraphic = background.GetComponent<Image>();
            toggle.graphic = checkmark.GetComponent<Image>();
            return toggle;
        }

        /// <summary>
        /// 执行 getBuiltinFont 相关逻辑
        /// </summary>
        private static Font GetBuiltinFont()
        {
            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            return font != null ? font : Resources.GetBuiltinResource<Font>("Arial.ttf");
        }
    }
}
