namespace Demos.示例_UI曲面叠加滚动.无限滚动模块化
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// 分页管理器 - 处理分页指示器和页码逻辑
    /// </summary>
    public class RollViewPaginationManager : MonoBehaviour
    {
        [Header("分页指示器的父容器")] public Transform pageIndicatorParent;
        [Header("分页指示器预制体")] public GameObject pagePrefab;
        [Header("每页显示Item数量")] public int itemsPerPage = 1;

        private readonly List<Image> _pageIndicators = new();
        private int _lastPageIndex = -1;

        /// <summary>
        /// 当前页码发生变化时的回调
        /// </summary>
        public event Action<int> OnPageChanged;

        private void OnEnable()
        {
            itemsPerPage = Mathf.Max(1, itemsPerPage);
        }

        public void Initialize(int totalItems)
        {
            InitializePageIndicators(totalItems);
        }

        /// <summary>
        /// 初始化分页指示器
        /// </summary>
        private void InitializePageIndicators(int totalItems)
        {
            if (!pageIndicatorParent || !pagePrefab)
                return;

            // 清空现有的分页指示器
            // foreach (Transform child in pageIndicatorParent)
            //     Destroy(child.gameObject);
            _pageIndicators.Clear();

            // 计算总页数
            int totalPages = GetTotalPages(totalItems);

            // 创建分页指示器
            for (int i = 0; i < totalPages; i++)
            {
                var pageObj = Instantiate(pagePrefab, pageIndicatorParent);

                pageObj.name = "Page_" + i;
                var pageImage = pageObj.GetComponent<Image>();
                if (pageImage)
                {
                    _pageIndicators.Add(pageImage);
                    int pageIndex = i;
                    // 为分页指示器添加点击事件
                    var pageBtn           = pageObj.GetComponent<Button>();
                    if (!pageBtn) pageBtn = pageObj.AddComponent<Button>();
                    pageBtn.onClick.AddListener(() => OnPageIndicatorClicked?.Invoke(pageIndex));
                }

                pageObj.SetActive(true);
            }
        }

        /// <summary>
        /// 更新分页指示器的状态
        /// </summary>
        public void UpdatePageIndicators(int centerIndex, int totalItems)
        {
            int currentPage = GetCurrentPage(centerIndex);
            if (currentPage == _lastPageIndex)
                return;

            _lastPageIndex = currentPage;
            OnPageChanged?.Invoke(currentPage);

            for (int i = 0; i < _pageIndicators.Count; i++)
            {
                _pageIndicators[i].color = i == currentPage ? Color.white : new Color(1, 1, 1, 0.5f);
            }
        }

        /// <summary>
        /// 获取当前页码
        /// </summary>
        public int GetCurrentPage(int centerIndex)
        {
            return centerIndex / itemsPerPage;
        }

        /// <summary>
        /// 获取总页数
        /// </summary>
        public int GetTotalPages(int totalItems)
        {
            return Mathf.CeilToInt((float)totalItems / itemsPerPage);
        }

        /// <summary>
        /// 计算目标滚动值（用于跳转到指定页）
        /// </summary>
        public int CalculateTargetScroll(int pageIndex, int totalItems)
        {
            int totalPages = GetTotalPages(totalItems);
            pageIndex = Mathf.Clamp(pageIndex, 0, totalPages - 1);
            return pageIndex * itemsPerPage;
        }

        /// <summary>
        /// 计算下一页的滚动值（保留旧签名以兼容性，旧逻辑按“循环”处理）
        /// </summary>
        public int CalculateNextPageScroll(int currentScroll, int totalItems)
        {
            return CalculateNextPageScroll(currentScroll, totalItems, true);
        }

        /// <summary>
        /// 计算下一页的滚动值（可选择是否循环）
        /// </summary>
        public int CalculateNextPageScroll(int currentScroll, int totalItems, bool isLoop)
        {
            int nextScroll = currentScroll + itemsPerPage;
            int maxScroll  = totalItems - 1;
            if (nextScroll > maxScroll)
            {
                if (isLoop)
                {
                    nextScroll = 0;
                }
                else
                {
                    nextScroll = maxScroll;
                }
            }

            return Mathf.Min(nextScroll, maxScroll);
        }

        /// <summary>
        /// 计算上一页的滚动值
        /// </summary>
        public int CalculatePrevPageScroll(int currentScroll, int totalItems, bool isLoop)
        {
            int prevScroll = currentScroll - itemsPerPage;

            if (isLoop)
            {
                if (prevScroll < 0)
                {
                    prevScroll = itemsPerPage * (GetTotalPages(totalItems) - 1);
                }
            }

            return prevScroll; //Mathf.Max(prevScroll, 0);
        }

        public event Action<int> OnPageIndicatorClicked;
    }
}