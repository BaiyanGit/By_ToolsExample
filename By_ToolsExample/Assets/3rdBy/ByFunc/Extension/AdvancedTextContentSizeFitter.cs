namespace _3rdBy.ByFunc.Extension
{
    using UnityEngine;
    using UnityEngine.UI;
    using UnityEngine.EventSystems;
#if TMP_PRESENT
    using TMPro;
#endif

    [ExecuteAlways]
    [DisallowMultipleComponent]
    [AddComponentMenu("Layout/Advanced Text Content Size Fitter")]
    public class AdvancedTextContentSizeFitter : UIBehaviour
    {
        [Header("文本内容边距")] [SerializeField] private Vector2 _padding = new(40, 20);

        [Header("最大宽度")] [SerializeField] private float _maxWidth = 500f;

        [Header("使用父宽度百分比")] [SerializeField] private bool _useParentWidthPercent = false;

        [SerializeField, Range(0.1f, 1f)] private float _parentWidthPercent = 0.8f;

        [Header("自动刷新")] [SerializeField] private bool _autoRefresh = true;

        private RectTransform _rect;
        private RectTransform _parentRect;

        private Text _uiText;
#if TMP_PRESENT
        private TMP_Text _tmpText;
#endif

        private string _lastText;

        protected override void Awake()
        {
            Cache();
            Refresh();
        }

        protected override void OnEnable()
        {
            Cache();
            RegisterTMPEvent();
            Refresh();
        }

        protected override void OnDisable()
        {
            UnregisterTMPEvent();
        }

        private void Update()
        {
            if (!_autoRefresh) return;

#if TMP_PRESENT
            if (_tmpText != null)
                return; // TMP 用事件监听
#endif

            if (_uiText != null && _uiText.text != _lastText)
            {
                Refresh();
            }
        }

        private void Cache()
        {
            _rect       = GetComponent<RectTransform>();
            _parentRect = transform.parent as RectTransform;

            _uiText = GetComponent<Text>();
#if TMP_PRESENT
            _tmpText = GetComponent<TMP_Text>();
#endif
        }

        private void RegisterTMPEvent()
        {
#if TMP_PRESENT
            if (_tmpText != null)
                TMPro.TMP_Text.onTextChanged += OnTMPTextChanged;
#endif
        }

        private void UnregisterTMPEvent()
        {
#if TMP_PRESENT
            TMPro.TMP_Text.onTextChanged -= OnTMPTextChanged;
#endif
        }

#if TMP_PRESENT
        private void OnTMPTextChanged(Object obj)
        {
            if (obj == _tmpText)
                Refresh();
        }
#endif

        /// <summary>
        /// 刷新文本内容的大小(当赋值时调用)
        /// </summary>
        public void Refresh()
        {
            if (_rect == null) return;

            float preferredWidth  = GetPreferredWidth();
            float preferredHeight = GetPreferredHeight();

            float finalMaxWidth = _maxWidth;

            if (_useParentWidthPercent && _parentRect != null)
                finalMaxWidth = _parentRect.rect.width * _parentWidthPercent;

            bool exceed = preferredWidth > finalMaxWidth;

            float width  = exceed ? finalMaxWidth : preferredWidth;
            float height = exceed ? GetPreferredHeightWithWidth(width) : preferredHeight;

            _rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width + _padding.x);
            _rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height + _padding.y);

            if (_uiText)
                _lastText = _uiText.text;
#if TMP_PRESENT
            if (_tmpText != null)
                _lastText = _tmpText.text;
#endif
        }

        private float GetPreferredWidth()
        {
#if TMP_PRESENT
            if (_tmpText != null)
                return _tmpText.GetPreferredValues(_tmpText.text, Mathf.Infinity, Mathf.Infinity).x;
#endif
            return _uiText != null ? _uiText.preferredWidth : _rect.rect.width;
        }

        private float GetPreferredHeight()
        {
#if TMP_PRESENT
            if (_tmpText != null)
                return _tmpText.GetPreferredValues(_tmpText.text, Mathf.Infinity, Mathf.Infinity).y;
#endif
            return _uiText != null ? _uiText.preferredHeight : _rect.rect.height;
        }

        private float GetPreferredHeightWithWidth(float width)
        {
#if TMP_PRESENT
            if (_tmpText != null)
                return _tmpText.GetPreferredValues(_tmpText.text, width, Mathf.Infinity).y;
#endif
            if (_uiText != null)
            {
                var settings = _uiText.GetGenerationSettings(new Vector2(width, 0));
                return _uiText.cachedTextGeneratorForLayout
                           .GetPreferredHeight(_uiText.text, settings) / _uiText.pixelsPerUnit;
            }

            return _rect.rect.height;
        }
    }
}