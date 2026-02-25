namespace Demos.示例_UI曲面叠加滚动.无限滚动模块化
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// 滚动视图管理器 - 轻量级的事件分发器
    /// 只负责协调各个模块化组件
    /// </summary>
    // [RequireComponent(typeof(RollViewLayoutManager))]
    // [RequireComponent(typeof(RollViewPaginationManager))]
    // [RequireComponent(typeof(RollViewInputHandler))]
    // [RequireComponent(typeof(RollViewArrowController))]
    public class RollViewManager : MonoBehaviour
    {
        [Header("Item容器")] public RectTransform content;      // 包含所有滚动项的容器
        [Header("无限循环")] public bool infiniteLoop = true;     // 是否启用无限循环滚动
        [Header("平滑时长")] public float snapSmoothTime = 0.18f; // 平滑滚动到中心项的时间

        [Header("布局管理器"), SerializeField] private RollViewLayoutManager _layoutManager;
        [Header("分页管理器"), SerializeField] private RollViewPaginationManager _paginationManager;
        [Header("输入处理器"), SerializeField] private RollViewInputHandler _inputHandler;
        [Header("箭头控制器"), SerializeField] private RollViewArrowController _arrowController;

        private readonly List<RectTransform> _items = new();
        private float _scroll;
        private float _targetScroll;
        private float _scrollVelocity;
        private int _lastCenterIndex = -1;

        /// <summary>
        /// 当前滚动视图的中心变化事件<索引>
        /// </summary>
        public event Action<int> OnCenterChanged;

        /// <summary>
        /// 当前页码发生变化事件<索引>
        /// </summary>
        public event Action<int> OnPageChanged;

        private void OnValidate()
        {
            // 自动关联组件
            if (!_layoutManager) _layoutManager         = GetComponent<RollViewLayoutManager>();
            if (!_paginationManager) _paginationManager = GetComponent<RollViewPaginationManager>();
            if (!_inputHandler) _inputHandler           = GetComponent<RollViewInputHandler>();
            if (!_arrowController) _arrowController     = GetComponent<RollViewArrowController>();
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
            if (!_inputHandler.isDragging)
                _scroll = Mathf.SmoothDamp(_scroll, _targetScroll, ref _scrollVelocity, snapSmoothTime);

            // 布局更新
            _layoutManager.UpdateLayout(_scroll);

            // 中心项和页码检测
            DetectCenterAndPageChanged();

            // 箭头显示检测
            UpdateArrowsDisplay();
        }

        #region 初始化

        [ContextMenu("刷新Items")]
        public void RefreshItems()
        {
            _items.Clear();
            Start();
        }

        private void InitializeItems()
        {
            if (content == null) return;

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
        }

        private void SubscribeToEvents()
        {
            if (_inputHandler != null)
            {
                // 输入事件
                _inputHandler.OnDragAction     += HandleDrag;
                _inputHandler.OnEndDragAction  += HandleEndDrag;
                _inputHandler.OnMoveNextAction += MoveNextPage;
                _inputHandler.OnMovePrevAction += MovePrevPage;
            }

            if (_paginationManager != null)
            {
                // 分页事件
                _paginationManager.OnPageIndicatorClicked += GoToPage;
                _paginationManager.OnPageChanged          += (page) => OnPageChanged?.Invoke(page);
            }
        }

        private void UnsubscribeFromEvents()
        {
            if (_inputHandler != null)
            {
                _inputHandler.OnDragAction     -= HandleDrag;
                _inputHandler.OnEndDragAction  -= HandleEndDrag;
                _inputHandler.OnMoveNextAction -= MoveNextPage;
                _inputHandler.OnMovePrevAction -= MovePrevPage;
            }

            if (_paginationManager != null)
            {
                _paginationManager.OnPageIndicatorClicked -= GoToPage;
            }
        }

        #endregion

        #region 输入处理

        private void HandleDrag(float delta, bool realtimeDragInput)
        {
            _targetScroll += delta;

            if (!infiniteLoop)
            {
                _targetScroll = Mathf.Clamp(_targetScroll, 0, _items.Count - 1);
                _scroll       = _targetScroll;
            }

            if (realtimeDragInput)
                _scroll = _targetScroll; // 拖拽时立即同步
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
        /// 关键：非无限循环时以“增量 + clamp”方式移动（与拖拽一致），而不是通过 wrap/重置索引来“移动 item”。
        /// </summary>
        private void MoveNextPage()
        {
            if (_items.Count == 0 || _paginationManager == null) return;

            int step          = Mathf.Max(1, _paginationManager.itemsPerPage);
            int currentCenter = Mathf.RoundToInt(_scroll);
            int targetIndex   = currentCenter + step;

            if (infiniteLoop)
            {
                // 计算最短环绕偏移，保证目标对齐到整数索引
                if (_layoutManager)
                {
                    float delta = _layoutManager.CalculateOffset(targetIndex, _scroll);
                    _targetScroll = _scroll + delta;
                }
                else
                {
                    _targetScroll = targetIndex;
                }
            }
            else
            {
                // 非无限：直接以整数索引为目标并 clamp
                targetIndex   = Mathf.Clamp(targetIndex, 0, _items.Count - 1);
                _targetScroll = targetIndex;
            }
        }

        /// <summary>
        /// 移动到上一页
        /// 关键：非无限循环时以“增量 + clamp”方式移动（与拖拽一致），而不是通过 wrap/重置索引来“移动 item”。
        /// </summary>
        private void MovePrevPage()
        {
            if (_items.Count == 0 || _paginationManager == null) return;

            int step          = Mathf.Max(1, _paginationManager.itemsPerPage);
            int currentCenter = Mathf.RoundToInt(_scroll);
            int targetIndex   = currentCenter - step;

            if (infiniteLoop)
            {
                if (_layoutManager)
                {
                    float delta = _layoutManager.CalculateOffset(targetIndex, _scroll);
                    _targetScroll = _scroll + delta;
                }
                else
                {
                    _targetScroll = targetIndex;
                }
            }
            else
            {
                targetIndex   = Mathf.Clamp(targetIndex, 0, _items.Count - 1);
                _targetScroll = targetIndex;
            }
        }

        /// <summary>
        /// 跳转到指定页码
        /// 在无限循环时计算最短环绕偏移，以实现平滑环绕跳转；非无限时直接使用目标索引。
        /// </summary>
        public void GoToPage(int pageIndex)
        {
            if (_paginationManager == null || _items.Count == 0) return;
            int targetScroll = _paginationManager.CalculateTargetScroll(pageIndex, _items.Count);

            if (infiniteLoop)
            {
                // 计算到目标索引的最短环绕偏移（由 LayoutManager 处理 Wrap）
                float delta = _layoutManager.CalculateOffset(targetScroll, _scroll);
                _targetScroll = _scroll + delta;
            }
            else
            {
                // 非无限：直接跳到页面起始索引（已经在 CalculateTargetScroll 中做了 clamp）
                _targetScroll = Mathf.Clamp(targetScroll, 0, _items.Count - 1);
            }
        }

        /// <summary>
        /// 获取当前中心项索引
        /// </summary>
        public int GetCenterIndex()
        {
            if (_items.Count == 0) return 0;
            int idx = Mathf.RoundToInt(_scroll);
            return (idx % _items.Count + _items.Count) % _items.Count;
        }

        /// <summary>
        /// 获取当前页码
        /// </summary>
        public int GetCurrentPage()
        {
            return _paginationManager ? _paginationManager.GetCurrentPage(GetCenterIndex()) : 0;
        }

        /// <summary>
        /// 获取总页数
        /// </summary>
        public int GetTotalPages()
        {
            return _paginationManager ? _paginationManager.GetTotalPages(_items.Count) : 0;
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
            int totalPages  = GetTotalPages();
            _arrowController?.UpdateArrows(currentPage, totalPages, infiniteLoop);
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