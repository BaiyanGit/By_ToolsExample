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
            if (!createLegacyRuntimeUI)
            {
                Debug.LogWarning("UIRecorderParamsSettings: btnUse 未绑定。请在场景中放置按钮，或使用 ByTools/🔴Recorder SDK/UI创建/设置中心 生成。");
                return;
            }

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

            var sourceButton = btnUse != null ? btnUse : btnSave;
            if (sourceButton == null) return;
            if (!createLegacyRuntimeUI)
            {
                Debug.LogWarning("UIRecorderParamsSettings: btnDeleteConfig 未绑定。请在场景中放置按钮，或使用 ByTools/🔴Recorder SDK/UI创建/设置中心 生成。");
                return;
            }

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
        /// 执行 EnsureSaveAsWindow 相关逻辑
        /// </summary>
        private void EnsureSaveAsWindow()
        {
            if (saveAsWindowRoot != null)
            {
                if (txtSaveAsTips == null)
                {
                    var tips = saveAsWindowRoot.transform.Find("TxtSaveAsTips");
                    if (tips != null) txtSaveAsTips = tips.GetComponent<Text>();
                    if (txtSaveAsTips != null)
                    {
                        txtSaveAsTips.color = new Color(1f, 0.45f, 0.35f, 1f);
                        txtSaveAsTips.gameObject.SetActive(false);
                    }
                }

                return;
            }

            if (!createLegacyRuntimeUI)
            {
                Debug.LogWarning("UIRecorderParamsSettings: saveAsWindowRoot 未绑定。请在场景中放置另存为窗口，或使用 ByTools/🔴Recorder SDK/UI创建/设置中心 生成。");
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
            if (!createLegacyRuntimeUI)
            {
                var messageWindow = GetCanvasTransform()?.Find("RecorderMessageWindow");
                if (messageWindow != null)
                {
                    _messageWindowRoot = messageWindow.gameObject;
                    if (_txtMessageContent == null) _txtMessageContent = messageWindow.Find("TxtContent")?.GetComponent<Text>();
                    if (_btnMessageConfirm == null) _btnMessageConfirm = messageWindow.Find("BtnConfirm")?.GetComponent<Button>();
                    if (_btnMessageConfirm != null)
                    {
                        _btnMessageConfirm.onClick.RemoveListener(HideMessageTips);
                        _btnMessageConfirm.onClick.AddListener(HideMessageTips);
                    }
                }
                else
                {
                    Debug.LogWarning("UIRecorderParamsSettings: 通用提示窗口未绑定。请在场景中放置提示窗口，或使用 ByTools/🔴Recorder SDK/UI创建/设置中心 生成。");
                }
                return;
            }

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
            if (!createLegacyRuntimeUI)
            {
                Debug.LogWarning("UIRecorderParamsSettings: optionDescWindowRoot 未绑定。请在场景中放置选项说明窗口，或使用 ByTools/🔴Recorder SDK/UI创建/设置中心 生成。");
                return;
            }

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
            text.font               = GetDefaultUnityFont();
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
            DisableNavigation(input);
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
            var button = go.GetComponent<Button>();
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

        /// <summary>
        /// 获取 Unity 默认运行时字体。
        /// </summary>
        private static Font GetDefaultUnityFont()
        {
            return Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }
    }
}
