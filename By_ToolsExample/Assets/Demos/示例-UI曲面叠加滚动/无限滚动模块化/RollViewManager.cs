namespace Demos.示例_UI曲面叠加滚动.无限滚动
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// 滚动视图管理器 - 轻量级的事件分发器
    /// 只负责协调各个模块化组件
    /// </summary>
    public class RollViewManager : MonoBehaviour
    {
        [Header("Item容器")] public RectTransform content;       // 包含所有滚动项的容器
        [Header("无限循环")] public bool infiniteLoop = true;    // 是否启用无限循环滚动
        [Header("平滑时长")] public float snapSmoothTime = 0.18f; // 平滑滚动到中心项的时间

        [SerializeField] private RollViewLayoutManager _layoutManager;
        [SerializeField] private RollViewPaginationManager _paginationManager;
        [SerializeField] private RollViewInputHandler _inputHandler;
        [SerializeField] private RollViewArrowController _arrowController;

        private List<RectTransform> _items = new();
        private float _scroll;
        private float _targetScroll;
        private float _scrollVelocity;
        private int _lastCenterIndex = -1;

        public event Action<int> OnCenterChanged;
        public event Action<int> OnPageChanged;

        private void OnValidate()
        {
            // 自动关联组件
            if (!_layoutManager) _layoutManager = GetComponent<RollViewLayoutManager>();
            if (!_paginationManager) _paginationManager = GetComponent<RollViewPaginationManager>();
            if (!_inputHandler) _inputHandler = GetComponent<RollViewInputHandler>();
            if (!_arrowController) _arrowController = GetComponent<RollViewArrowController>();
        }

        private void Start()
        {
            InitializeItems();
            _layoutManager.Initialize(_items, infiniteLoop);
            _paginationManager.Initialize(_items.Count);
            
            _targetScroll = _scroll;
            
            SubscribeToEvents();
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        private void Update()
        {
            // 平滑捕捉
            if (!_inputHandler.IsDragging)
                _scroll = Mathf.SmoothDamp(_scroll, _targetScroll, ref _scrollVelocity, snapSmoothTime);

            // 布局更新
            _layoutManager.UpdateLayout(_scroll);

            // 中心项和页码检测
            DetectCenterAndPageChanged();

            // 箭头显示检测
            UpdateArrowsDisplay();
        }

        #region 初始化

        private void InitializeItems()
        {
            for (int i = 0; i < content.childCount; i++)
            {
                int index = i;
                var item = content.GetChild(i) as RectTransform;
                _items.Add(item);

                if (item)
                {
                    if (!item.GetComponent<CanvasGroup>())
                        item.gameObject.AddComponent<CanvasGroup>();

                    var btn = item.GetComponent<Button>();
                    if (!btn) btn = item.gameObject.AddComponent<Button>();
                    btn.onClick.AddListener(() => CenterOn(index));
                }
            }
        }

        private void SubscribeToEvents()
        {
            // 输入事件
            _inputHandler.OnDragAction     += HandleDrag;
            _inputHandler.OnEndDragAction  += HandleEndDrag;
            _inputHandler.OnMoveNextAction += MoveNextPage;
            _inputHandler.OnMovePrevAction += MovePrevPage;

            // 分页事件
            _paginationManager.OnPageIndicatorClicked += GoToPage;
            _paginationManager.OnPageChanged += (page) => OnPageChanged?.Invoke(page);
        }

        private void UnsubscribeFromEvents()
        {
            _inputHandler.OnDragAction                -= HandleDrag;
            _inputHandler.OnEndDragAction             -= HandleEndDrag;
            _inputHandler.OnMoveNextAction            -= MoveNextPage;
            _inputHandler.OnMovePrevAction            -= MovePrevPage;
            _paginationManager.OnPageIndicatorClicked -= GoToPage;
        }

        #endregion

        #region 输入处理

        private void HandleDrag(float delta)
        {
            _targetScroll += delta;
            if (!infiniteLoop)
                _targetScroll = Mathf.Clamp(_targetScroll, 0, _items.Count - 1);
        }

        private void HandleEndDrag()
        {
            _targetScroll = Mathf.Round(_targetScroll);
            if (!infiniteLoop)
                _targetScroll = Mathf.Clamp(_targetScroll, 0, _items.Count - 1);
        }

        #endregion

        #region 滚动控制

        /// <summary>
        /// 将滚动视图移动到指定索引处的中心
        /// </summary>
        public void CenterOn(int index)
        {
            float delta = index - _scroll;
            if (infiniteLoop)
                delta = _layoutManager.CalculateOffset(index, _scroll);

            _targetScroll = _scroll + delta;
        }

        /// <summary>
        /// 移动到下一页
        /// </summary>
        private void MoveNextPage()
        {
            int roundedScroll = Mathf.RoundToInt(_scroll);
            int nextScroll = _paginationManager.CalculateNextPageScroll(roundedScroll, _items.Count);
            _targetScroll = nextScroll;
        }

        /// <summary>
        /// 移动到上一页
        /// </summary>
        private void MovePrevPage()
        {
            int roundedScroll = Mathf.RoundToInt(_scroll);
            int prevScroll = _paginationManager.CalculatePrevPageScroll(roundedScroll);
            _targetScroll = prevScroll;
        }

        /// <summary>
        /// 跳转到指定页码
        /// </summary>
        public void GoToPage(int pageIndex)
        {
            int targetScroll = _paginationManager.CalculateTargetScroll(pageIndex, _items.Count);
            _targetScroll = targetScroll;
        }

        /// <summary>
        /// 获取当前中心项索引
        /// </summary>
        public int GetCenterIndex()
        {
            int idx = Mathf.RoundToInt(_scroll);
            return (idx % _items.Count + _items.Count) % _items.Count;
        }

        /// <summary>
        /// 获取当前页码
        /// </summary>
        public int GetCurrentPage()
        {
            return _paginationManager.GetCurrentPage(GetCenterIndex());
        }

        /// <summary>
        /// 获取总页数
        /// </summary>
        public int GetTotalPages()
        {
            return _paginationManager.GetTotalPages(_items.Count);
        }

        #endregion

        #region 状态更新

        private void DetectCenterAndPageChanged()
        {
            int centerIndex = GetCenterIndex();
            if (centerIndex != _lastCenterIndex)
            {
                _lastCenterIndex = centerIndex;
                OnCenterChanged?.Invoke(centerIndex);
            }

            _paginationManager.UpdatePageIndicators(centerIndex, _items.Count);
        }

        private void UpdateArrowsDisplay()
        {
            int currentPage = GetCurrentPage();
            int totalPages = GetTotalPages();
            _arrowController.UpdateArrows(currentPage, totalPages, infiniteLoop);
        }

        #endregion

        #region 公开 Getter - 用于外部访问各个模块

        /// <summary>
        /// 获取布局管理器
        /// </summary>
        public RollViewLayoutManager GetLayoutManager() => _layoutManager;

        /// <summary>
        /// 获取分页管理器
        /// </summary>
        public RollViewPaginationManager GetPaginationManager() => _paginationManager;

        /// <summary>
        /// 获取输入处理器
        /// </summary>
        public RollViewInputHandler GetInputHandler() => _inputHandler;

        /// <summary>
        /// 获取箭头控制器
        /// </summary>
        public RollViewArrowController GetArrowController() => _arrowController;

        /// <summary>
        /// 获取所有滚动项
        /// </summary>
        public List<RectTransform> GetItems() => _items;

        #endregion
    }
}