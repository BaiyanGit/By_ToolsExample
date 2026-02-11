namespace Demos.示例_UI曲面叠加滚动.Scripts
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.UI;

    /// <summary>
    /// 滚动视图盖流效果
    /// 不需要ScrollRect组件，通过拖拽来控制滚动，并实现了苹果级的布局优化
    /// </summary>
    public class RollViewCoverFlow : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [Header("无限循环")] public bool infiniteLoop = true;       // 是否启用无限循环滚动
        [Header("Item容器")] public RectTransform content;        // 包含所有滚动项的容器
        [Header("Item间距")] public float itemSpacing = 240f;     // 每个滚动项之间的间距
        [Header("拖拽力度")] public float dragSensitivity = 0.001f; // 拖拽时滚动的敏感度

        [Header("最小缩放")] public float minScale = 0.7f;      // 当项目远离中心时的最小缩放比例
        [Header("最大缩放")] public float maxScale = 1.25f;     // 中心项目时的最大缩放比例
        [Header("Y轴向上偏移量")] public float maxYOffset = 0.0f; // 中心项目时的最大Y轴偏移量
        [Header("最小透明度")] public float minAlpha = 0.4f;     // 当项目远离中心时的最小透明度

        [Header("平滑时长")] public float snapSmoothTime = 0.18f; // 平滑滚动到中心项的时间
        [Header("滚轮速度")] public float wheelSpeed = 0.8f;      // 鼠标滚轮的滚动速度

        [Header("分页系统")] public Transform pageIndicatorParent;     // 分页指示器的父容器
        [Header("分页预制体")] public GameObject pagePrefab;            // 分页指示器预制体
        [Header("每页项目数")] public int itemsPerPage = 1;             // 每页显示的项目数
        [Header("左箭头")] public Button leftArrow;                   // 左箭头图像
        [Header("右箭头")] public Button rightArrow;                  // 右箭头图像
        [Header("箭头隐藏透明度")] public float arrowDisableAlpha = 0.3f; // 箭头禁用时的透明度

        [Header("滚动项")] private readonly List<RectTransform> _items = new();
        [Header("分页指示器")] private readonly List<Image> _pageIndicators = new();
        [Header("当前滚动值")] private float _scroll;
        [Header("目标滚动值")] private float _targetScroll;
        [Header("滚动速度")] private float _scrollVelocity;
        [Header("是否正在拖拽")] private bool _dragging;
        [Header("上一次的中心索引")] private int _lastCenterIndex = -1;
        [Header("上一次的页码")] private int _lastPageIndex = -1;
        [Header("中心项变化事件")] public Action<int> onCenterChanged { get; set; } // 当中心项发生变化时触发的事件
        [Header("页码变化事件")]  public Action<int> onPageChanged   { get; set; } // 当页码发生变化时触发的事件

        private void Start()
        {
            // 确保每页项目数至少为1
            itemsPerPage = Mathf.Max(1, itemsPerPage);

            for (int i = 0; i < content.childCount; i++)
            {
                int index = i;
                var item  = content.GetChild(i) as RectTransform;
                _items.Add(item);

                if (item)
                {
                    if (!item.GetComponent<CanvasGroup>())
                        item.gameObject.AddComponent<CanvasGroup>();

                    var btn       = item.GetComponent<Button>();
                    if (!btn) btn = item.gameObject.AddComponent<Button>();
                    btn.onClick.AddListener(() => CenterOn(index));
                }
            }

            _targetScroll = _scroll;
            InitializePageIndicators();
            UpdateLayout();
        }

        private void Update()
        {
            // 平滑捕捉
            if (!_dragging) _scroll = Mathf.SmoothDamp(_scroll, _targetScroll, ref _scrollVelocity, snapSmoothTime);

            // 布局更新
            UpdateLayout();

            // 中心项变化检测
            DetectCenterChanged();

            // 页码变化检测
            DetectPageChanged();

            // 箭头显示检测
            UpdateArrowsDisplay();
        }

        #region 鼠标键盘控制

#if UNITY_EDITOR

        private void OnGUI()
        {
            // 鼠标滚轮
            float wheel = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(wheel) > 0.01f)
            {
                _targetScroll -= wheel * wheelSpeed;
                if (!infiniteLoop)
                    _targetScroll = Mathf.Clamp(_targetScroll, 0, _items.Count - 1);
            }

            // 键盘
            if (Input.GetKeyDown(KeyCode.RightArrow)) MoveNextPage();
            if (Input.GetKeyDown(KeyCode.LeftArrow)) MovePrevPage();
        }

#endif

        #endregion

        #region 拖拽事件

        public void OnBeginDrag(PointerEventData eventData)
        {
            _dragging = true;
        }

        public void OnDrag(PointerEventData eventData)
        {
            _targetScroll -= eventData.delta.x * dragSensitivity;
            if (!infiniteLoop)
                _targetScroll = Mathf.Clamp(_targetScroll, 0, _items.Count - 1);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            _dragging     = false;
            _targetScroll = Mathf.Round(_targetScroll);
            if (!infiniteLoop)
                _targetScroll = Mathf.Clamp(_targetScroll, 0, _items.Count - 1);
        }

        #endregion

        #region 布局更新

        private void UpdateLayout()
        {
            int count = _items.Count;

            // 图层排序列表
            List<(float t, RectTransform item)> sort = new();

            for (int i = 0; i < count; i++)
            {
                float offset = i - _scroll;
                if (infiniteLoop)
                    offset = Wrap(offset, count);

                float x = offset * itemSpacing;
                float t = Mathf.Clamp01(1f - Mathf.Abs(offset));

                float scale = Mathf.Lerp(minScale, maxScale, t);
                float y     = Mathf.Lerp(0, maxYOffset, t);
                float alpha = Mathf.Lerp(minAlpha, 1f, t);

                var item = _items[i];
                item.anchoredPosition                  = new Vector2(x, y);
                item.localScale                        = Vector3.one * scale;
                item.GetComponent<CanvasGroup>().alpha = alpha;

                sort.Add((t, item));
            }

            // 苹果级排序：远→近
            sort.Sort((a, b) => a.t.CompareTo(b.t));
            for (int i = 0; i < sort.Count; i++)
                sort[i].item.SetSiblingIndex(i);
        }

        /// <summary>
        /// 循环滚动
        /// </summary>
        /// <param name="v"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        private static float Wrap(float v, int length)
        {
            return Mathf.Repeat(v + length / 2f, length) - length / 2f;
        }

        #endregion

        #region 中心控制

        /// <summary>
        /// 将滚动视图移动到指定索引处的中心
        /// </summary>
        /// <param name="index"></param>
        public void CenterOn(int index)
        {
            float delta = index - _scroll;
            if (infiniteLoop)
                delta = Wrap(delta, _items.Count);

            _targetScroll = _scroll + delta;
        }

        /// <summary>
        /// 将滚动视图移动到下一项的中心
        /// </summary>
        public void MoveNext()
        {
            _targetScroll = Mathf.Round(_scroll) + 1;
            if (!infiniteLoop)
                _targetScroll = Mathf.Clamp(_targetScroll, 0, _items.Count - 1);
        }

        /// <summary>
        /// 将滚动视图移动到上一项的中心
        /// </summary>
        public void MovePrev()
        {
            _targetScroll = Mathf.Round(_scroll) - 1;
            if (!infiniteLoop)
                _targetScroll = Mathf.Clamp(_targetScroll, 0, _items.Count - 1);
        }

        /// <summary>
        /// 将滚动视图移动到下一页的中心
        /// </summary>
        public void MoveNextPage()
        {
            _targetScroll = Mathf.Round(_scroll) + itemsPerPage;
            if (!infiniteLoop)
                _targetScroll = Mathf.Clamp(_targetScroll, 0, _items.Count - 1);
        }

        /// <summary>
        /// 将滚动视图移动到上一页的中心
        /// </summary>
        public void MovePrevPage()
        {
            _targetScroll = Mathf.Round(_scroll) - itemsPerPage;
            if (!infiniteLoop)
                _targetScroll = Mathf.Clamp(_targetScroll, 0, _items.Count - 1);
        }

        /// <summary>
        /// 获取当前滚动视图的中心项索引
        /// </summary>
        /// <returns></returns>
        public int GetCenterIndex()
        {
            int idx = Mathf.RoundToInt(_scroll);
            return (idx % _items.Count + _items.Count) % _items.Count;
        }

        /// <summary>
        /// 获取当前页码
        /// </summary>
        /// <returns></returns>
        public int GetCurrentPage()
        {
            int centerIndex = GetCenterIndex();
            return centerIndex / itemsPerPage;
        }

        /// <summary>
        /// 获取总页数
        /// </summary>
        /// <returns></returns>
        public int GetTotalPages()
        {
            return Mathf.CeilToInt((float)_items.Count / itemsPerPage);
        }

        /// <summary>
        /// 检测中心项是否发生变化
        /// </summary>
        private void DetectCenterChanged()
        {
            int center = GetCenterIndex();
            if (center != _lastCenterIndex)
            {
                _lastCenterIndex = center;
                onCenterChanged?.Invoke(center);
            }
        }

        /// <summary>
        /// 检测页码是否发生变化
        /// </summary>
        private void DetectPageChanged()
        {
            int currentPage = GetCurrentPage();
            if (currentPage != _lastPageIndex)
            {
                _lastPageIndex = currentPage;
                onPageChanged?.Invoke(currentPage);
                UpdatePageIndicators();
            }
        }

        #endregion

        #region 分页系统

        /// <summary>
        /// 初始化分页指示器
        /// </summary>
        private void InitializePageIndicators()
        {
            if (!pageIndicatorParent || !pagePrefab)
                return;

            // 清空现有的分页指示器
            foreach (Transform child in pageIndicatorParent)
                Destroy(child.gameObject);
            _pageIndicators.Clear();

            // 计算总页数
            int totalPages = GetTotalPages();

            // 创建分页指示器
            for (int i = 0; i < totalPages; i++)
            {
                var pageObj   = Instantiate(pagePrefab, pageIndicatorParent);
                var pageImage = pageObj.GetComponent<Image>();
                if (pageImage)
                {
                    _pageIndicators.Add(pageImage);
                    int pageIndex = i;
                    // 为分页指示器添加点击事件，跳转到对应页
                    var pageBtn           = pageObj.GetComponent<Button>();
                    if (!pageBtn) pageBtn = pageObj.AddComponent<Button>();
                    pageBtn.onClick.AddListener(() => GoToPage(pageIndex));
                }
            }
        }

        /// <summary>
        /// 更新分页指示器的状态
        /// </summary>
        private void UpdatePageIndicators()
        {
            int currentPage = GetCurrentPage();
            for (int i = 0; i < _pageIndicators.Count; i++)
            {
                _pageIndicators[i].color = i == currentPage ? Color.white : new Color(1, 1, 1, 0.5f);
            }
        }

        /// <summary>
        /// 跳转到指定页码
        /// </summary>
        /// <param name="pageIndex"></param>
        public void GoToPage(int pageIndex)
        {
            int totalPages = GetTotalPages();
            pageIndex     = Mathf.Clamp(pageIndex, 0, totalPages - 1);
            _targetScroll = pageIndex * itemsPerPage;
            if (!infiniteLoop)
                _targetScroll = Mathf.Clamp(_targetScroll, 0, _items.Count - 1);
        }

        /// <summary>
        /// 更新左右箭头的显示状态
        /// </summary>
        private void UpdateArrowsDisplay()
        {
            if (!leftArrow && !rightArrow)
                return;

            int currentPage = GetCurrentPage();
            int totalPages  = GetTotalPages();

            // 更新左箭头
            if (leftArrow)
            {
                bool canMovePrev = infiniteLoop || currentPage > 0;
                SetArrowActive(leftArrow, canMovePrev);
            }

            // 更新右箭头
            if (rightArrow)
            {
                bool canMoveNext = infiniteLoop || currentPage < totalPages - 1;
                SetArrowActive(rightArrow, canMoveNext);
            }
        }

        /// <summary>
        /// 重置箭头的活动状态
        /// </summary>
        /// <param name="arrowButton"></param>
        /// <param name="active"></param>
        private void SetArrowActive(Button arrowButton, bool active)
        {
            if (!arrowButton || !arrowButton.image)
                return;

            var color = arrowButton.image.color;
            color.a                 = active ? 1f : arrowDisableAlpha;
            arrowButton.image.color = color;

            // 禁用/启用箭头按钮的交互
            arrowButton.interactable = active;
        }

        #endregion
    }
}