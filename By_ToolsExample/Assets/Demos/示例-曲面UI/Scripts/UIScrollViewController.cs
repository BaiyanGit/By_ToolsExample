namespace Demos.示例_曲面UI.Scripts
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.UI;

    [RequireComponent(typeof(ScrollRect))]
    public class UIScrollViewController : MonoBehaviour, IBeginDragHandler, IEndDragHandler
    {
        [Header("是否在启动时自动初始化内容")] public bool autoInitOnStart = true;
        [Header("滚动方向是否为水平")] public bool horizontal = true;
        [Header("滚动平滑的时间（秒）")] public float smoothTime = 0.25f;
        [Header("是否启用项目居中快照")] public bool enableSnap = true;
        [Header("居中速度阈值（单位/秒）")] public float snapVelocityThreshold = 50f;
        [Header("居中延迟的时间（秒）")] public float snapDelay = 0.05f;

        [Header("滚动区域")] private ScrollRect _scrollRect;
        [Header("内容区域")] private RectTransform _content;
        [Header("视口")] private RectTransform _viewport;
        [Header("内容尺寸调整器")] private ContentSizeFitter _contentSizeFitter;

        [Header("所有Item")] private readonly List<RectTransform> _items = new();

        [Header("当前选中Item")] private RectTransform _currentSelected;
        [Header("滚动协程")] private Coroutine _scrollCoroutine;
        [Header("是否初始化完成")] private bool _initialized;

        #region Unity

        private void Awake()
        {
            _scrollRect        = GetComponent<ScrollRect>();
            _content           = _scrollRect.content;
            _viewport          = _scrollRect.viewport;
            _contentSizeFitter = _content.GetComponent<ContentSizeFitter>();
        }

        private void Start()
        {
            if (autoInitOnStart)
                Init();
        }

        #endregion

        #region Init (核心)

        /// <summary>
        /// 初始化 ScrollView（只需调用一次）
        /// </summary>
        public void Init()
        {
            if (_initialized) return;
            _initialized = true;

            StartCoroutine(InitRoutine());
        }

        private IEnumerator InitRoutine()
        {
            CollectItems();
            BindClickEvents();

            // 等一帧，让 LayoutGroup + CSF 完成第一次计算
            yield return null;

            LayoutRebuilder.ForceRebuildLayoutImmediate(_content);

            // 初始化完成后禁用 ContentSizeFitter（关键）
            if (_contentSizeFitter != null)
                _contentSizeFitter.enabled = false;

            // 默认选中第一个
            if (_items.Count > 0)
                SelectItem(_items[0], false);
        }

        private void CollectItems()
        {
            _items.Clear();
            foreach (Transform child in _content)
            {
                if (child.TryGetComponent<Button>(out _))
                    _items.Add(child as RectTransform);
            }
        }

        private void BindClickEvents()
        {
            foreach (var item in _items)
            {
                var btn = item.GetComponent<Button>();
                btn.onClick.AddListener(() => SelectItem(item));
            }
        }

        #endregion

        #region Public API

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                SelectIndex(_items.IndexOf(_currentSelected) - 1);
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                SelectIndex(_items.IndexOf(_currentSelected) + 1);
            }
        }

        public void SelectIndex(int index, bool smooth = true)
        {
            if (index < 0 || index >= _items.Count) return;
            SelectItem(_items[index], smooth);
        }

        public void SelectItem(RectTransform item, bool smooth = true)
        {
            if (item == null) return;

            _currentSelected = item;
            StartCoroutine(CenterRoutine(item, smooth));
        }

        #endregion

        #region Center & Snap

        private IEnumerator CenterRoutine(RectTransform target, bool smooth)
        {
            yield return null;
            LayoutRebuilder.ForceRebuildLayoutImmediate(_content);

            var targetPos = CalculateContentPosition(target);

            if (_scrollCoroutine != null)
                StopCoroutine(_scrollCoroutine);

            if (smooth)
                _scrollCoroutine = StartCoroutine(SmoothMove(targetPos));
            else
                _content.anchoredPosition = targetPos;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (_scrollCoroutine != null)
                StopCoroutine(_scrollCoroutine);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!enableSnap)
                return;

            StartCoroutine(SnapAfterDrag());
        }

        private IEnumerator SnapAfterDrag()
        {
            yield return new WaitForSeconds(snapDelay);

            while (_scrollRect.velocity.magnitude > snapVelocityThreshold)
                yield return null;

            var nearest = FindNearestToCenter();
            if (nearest != null)
                SelectItem(nearest);
        }

        #endregion

        #region Math Core（居中不漂）

        private Vector2 CalculateContentPosition(RectTransform target)
        {
            var viewportCenterWorld = _viewport.TransformPoint(_viewport.rect.center);
            var itemCenterWorld     = target.TransformPoint(target.rect.center);
            var worldOffset         = viewportCenterWorld - itemCenterWorld;
            var localOffset         = _content.InverseTransformVector(worldOffset);
            var pos                 = _content.anchoredPosition;

            if (horizontal)
                pos.x += localOffset.x;
            else
                pos.y += localOffset.y;

            return ClampContentPosition(pos);
        }

        private Vector2 ClampContentPosition(Vector2 pos)
        {
            var contentSize  = _content.rect.size;
            var viewportSize = _viewport.rect.size;

            if (horizontal)
            {
                pos.x = contentSize.x <= viewportSize.x ? 0 : Mathf.Clamp(pos.x, viewportSize.x - contentSize.x, 0);
            }
            else
            {
                pos.y = contentSize.y <= viewportSize.y ? 0 : Mathf.Clamp(pos.y, 0, contentSize.y - viewportSize.y);
            }

            return pos;
        }

        private IEnumerator SmoothMove(Vector2 target)
        {
            var velocity = Vector2.zero;

            while (Vector2.Distance(_content.anchoredPosition, target) > 0.1f)
            {
                _content.anchoredPosition = Vector2.SmoothDamp(_content.anchoredPosition, target, ref velocity, smoothTime);
                yield return null;
            }

            _content.anchoredPosition = target;
        }

        private RectTransform FindNearestToCenter()
        {
            var viewportCenterWorld = _viewport.TransformPoint(_viewport.rect.center);

            float         minDistance = float.MaxValue;
            RectTransform nearest     = null;

            foreach (var item in _items)
            {
                var itemCenterWorld = item.TransformPoint(item.rect.center);

                float distance = horizontal ? Mathf.Abs(itemCenterWorld.x - viewportCenterWorld.x) : Mathf.Abs(itemCenterWorld.y - viewportCenterWorld.y);

                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearest     = item;
                }
            }

            return nearest;
        }

        #endregion
    }
}