namespace Demos.示例_UI曲面叠加滚动.无限滚动模块化
{
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// 滚动视图使用示例
    /// 演示如何直接使用各个模块化组件
    /// </summary>
    public class RollViewCoverFlowExample : MonoBehaviour
    {
        [Header("滚动视图管理器")] public RollViewManager rollViewManager;
        [Header("布局管理器")] public RollViewLayoutManager layoutManager;
        [Header("分页管理器")] public RollViewPaginationManager paginationManager;
        [Header("输入处理器")] public RollViewInputHandler inputHandler;
        [Header("箭头控制器")] public RollViewArrowController arrowController;

        [Header("UI 文本显示")] public Text pageText; // 显示当前页码
        [Header("")] public Text centerItemText;  // 显示中心项
        [Header("")] public Text infoPanel;       // 显示详细信息

        [Header("测试输入")] public InputField testPageInput;  // 输入页码
        [Header("")] public InputField testItemIndexInput; // 输入项目索引

        private void Start()
        {
            // 如果没有手动配置，可以从 RollViewManager 获取
            if (!layoutManager && rollViewManager)
                layoutManager = rollViewManager.GetLayoutManager();
            if (!paginationManager && rollViewManager)
                paginationManager = rollViewManager.GetPaginationManager();
            if (!inputHandler && rollViewManager)
                inputHandler = rollViewManager.GetInputHandler();
            if (!arrowController && rollViewManager)
                arrowController = rollViewManager.GetArrowController();

            // 订阅事件
            SubscribeToAllEvents();

            // 初始化显示
            UpdateDisplay();
        }

        private void OnDestroy()
        {
            UnsubscribeAllEvents();
        }

        #region 订阅事件

        private void SubscribeToAllEvents()
        {
            // 订阅管理器事件
            if (rollViewManager)
            {
                rollViewManager.OnCenterChanged += HandleCenterChanged;
                rollViewManager.OnPageChanged   += HandlePageChanged;
            }

            // 订阅分页管理器事件
            if (paginationManager)
            {
                paginationManager.OnPageChanged += (page) => { Debug.Log($"[分页管理器] 页码已改变: {page}"); };

                paginationManager.OnPageIndicatorClicked += (pageIndex) => { Debug.Log($"[分页管理器] 页码指示器被点击: {pageIndex}"); };
            }

            // 订阅输入处理器事件
            if (inputHandler)
            {
                inputHandler.OnBeginDragAction += () => { Debug.Log("[输入处理器] 开始拖拽"); };

                inputHandler.OnDragAction += (delta) => { Debug.Log($"[输入处理器] 拖拽中，偏移量: {delta}"); };

                inputHandler.OnEndDragAction += () => { Debug.Log("[输入处理器] 拖拽结束"); };

                inputHandler.OnMoveNextAction += () => { Debug.Log("[输入处理器] 键盘右箭头按下"); };

                inputHandler.OnMovePrevAction += () => { Debug.Log("[输入处理器] 键盘左箭头按下"); };
            }
        }

        private void UnsubscribeAllEvents()
        {
            if (rollViewManager)
            {
                rollViewManager.OnCenterChanged -= HandleCenterChanged;
                rollViewManager.OnPageChanged   -= HandlePageChanged;
            }
        }

        #endregion

        #region 事件处理

        private void HandleCenterChanged(int centerIndex)
        {
            Debug.Log($"[管理器] 中心项已改变: {centerIndex}");
            UpdateDisplay();
        }

        private void HandlePageChanged(int pageIndex)
        {
            Debug.Log($"[管理器] 页码已改变: {pageIndex}");
            UpdateDisplay();
        }

        #endregion

        #region UI 更新

        private void UpdateDisplay()
        {
            if (!rollViewManager)
                return;

            int centerIndex = rollViewManager.GetCenterIndex();
            int currentPage = rollViewManager.GetCurrentPage();
            int totalPages  = rollViewManager.GetTotalPages();

            if (pageText)
                pageText.text = $"页码: {currentPage + 1} / {totalPages}";

            if (centerItemText)
                centerItemText.text = $"中心项索引: {centerIndex}";

            if (infoPanel)
            {
                string info = $"=== 滚动视图信息 ===\n" +
                              $"中心项索引: {centerIndex}\n" +
                              $"当前页码: {currentPage + 1}\n" +
                              $"总页数: {totalPages}\n" +
                              $"===================";
                infoPanel.text = info;
            }
        }

        #endregion

        #region 布局管理器相关方法

        /// <summary>
        /// 修改布局参数
        /// </summary>
        public void ModifyLayoutSettings()
        {
            if (!layoutManager)
            {
                Debug.LogWarning("布局管理器未配置");
                return;
            }

            layoutManager.itemSpacing = 280f;
            layoutManager.minScale    = 0.6f;
            layoutManager.maxScale    = 1.3f;
            layoutManager.maxYOffset  = 50f;
            layoutManager.minAlpha    = 0.3f;

            Debug.Log("[布局管理器] 已修改布局参数");
        }

        /// <summary>
        /// 重置布局参数
        /// </summary>
        public void ResetLayoutSettings()
        {
            if (!layoutManager)
            {
                Debug.LogWarning("布局管理器未配置");
                return;
            }

            layoutManager.itemSpacing = 240f;
            layoutManager.minScale    = 0.7f;
            layoutManager.maxScale    = 1.25f;
            layoutManager.maxYOffset  = 0.0f;
            layoutManager.minAlpha    = 0.4f;

            Debug.Log("[布局管理器] 已重置布局参数");
        }

        #endregion

        #region 分页管理器相关方法

        /// <summary>
        /// 修改每页显示的项目数
        /// </summary>
        public void SetItemsPerPage(int count)
        {
            if (!paginationManager || !rollViewManager)
            {
                Debug.LogWarning("分页管理器或管理器未配置");
                return;
            }

            paginationManager.itemsPerPage = Mathf.Max(1, count);
            paginationManager.Initialize(rollViewManager.GetItems().Count);
            Debug.Log($"[分页管理器] 已修改每页项目数为: {count}");
        }

        /// <summary>
        /// 通过输入框跳转页码
        /// </summary>
        public void GoToPageByInput()
        {
            if (!rollViewManager || !testPageInput)
                return;

            if (int.TryParse(testPageInput.text, out int pageIndex))
            {
                rollViewManager.GoToPage(pageIndex - 1);
                Debug.Log($"[分页管理器] 已跳转到第 {pageIndex} 页");
            }
            else
            {
                Debug.LogWarning("请输入有效的页码");
            }
        }

        /// <summary>
        /// 跳转到首页
        /// </summary>
        public void GoToFirstPage()
        {
            if (rollViewManager)
            {
                rollViewManager.GoToPage(0);
                Debug.Log("[分页管理器] 已跳转到首页");
            }
        }

        /// <summary>
        /// 跳转到末页
        /// </summary>
        public void GoToLastPage()
        {
            if (rollViewManager)
            {
                int lastPage = rollViewManager.GetTotalPages() - 1;
                rollViewManager.GoToPage(lastPage);
                Debug.Log($"[分页管理器] 已跳转到末页");
            }
        }

        #endregion

        #region 输入处理器相关方法

        /// <summary>
        /// 检查拖拽状态
        /// </summary>
        public void CheckDraggingState()
        {
            if (!inputHandler)
            {
                Debug.LogWarning("输入处理器未配置");
                return;
            }

            bool isDragging = inputHandler.isDragging;
            Debug.Log($"[输入处理器] 当前拖拽状态: {(isDragging ? "拖拽中" : "未拖拽")}");
        }

        /// <summary>
        /// 修改输入处理器参数
        /// </summary>
        public void ModifyInputHandlerSettings(float dragSensitivity, float wheelSpeed)
        {
            if (!inputHandler)
            {
                Debug.LogWarning("输入处理器未配置");
                return;
            }

            inputHandler.dragSensitivity = dragSensitivity;
            inputHandler.wheelSpeed      = wheelSpeed;

            Debug.Log("[输入处理器] 已修改参数");
            Debug.Log($"  - 拖拽敏感度: {dragSensitivity}");
            Debug.Log($"  - 滚轮速度: {wheelSpeed}");
        }

        #endregion

        #region 箭头控制器相关方法

        /// <summary>
        /// 修改箭头隐藏透明度
        /// </summary>
        public void SetArrowDisableAlpha(float alpha)
        {
            if (!arrowController)
            {
                Debug.LogWarning("箭头控制器未配置");
                return;
            }

            arrowController.disableAlpha = Mathf.Clamp01(alpha);
            Debug.Log($"[箭头控制器] 已修改箭头隐藏透明度为: {alpha}");
        }

        #endregion

        #region 项目相关方法

        /// <summary>
        /// 通过输入框居中到指定项目
        /// </summary>
        public void CenterOnItemByInput()
        {
            if (!rollViewManager || !testItemIndexInput)
                return;

            if (int.TryParse(testItemIndexInput.text, out int itemIndex))
            {
                rollViewManager.CenterOn(itemIndex);
                Debug.Log($"[管理器] 已居中到项目 {itemIndex}");
            }
            else
            {
                Debug.LogWarning("请输入有效的项目索引");
            }
        }

        /// <summary>
        /// 打印所有信息
        /// </summary>
        public void PrintAllInfo()
        {
            if (!rollViewManager)
                return;

            Debug.Log("========== 完整信息 ==========");

            // 管理器信息
            Debug.Log("[RollViewManager]");
            Debug.Log($"  - 中心项索引: {rollViewManager.GetCenterIndex()}");
            Debug.Log($"  - 当前页码: {rollViewManager.GetCurrentPage() + 1}");
            Debug.Log($"  - 总页数: {rollViewManager.GetTotalPages()}");

            // 布局管理器信息
            if (layoutManager)
            {
                Debug.Log("[RollViewLayoutManager]");
                Debug.Log($"  - 项目间距: {layoutManager.itemSpacing}");
                Debug.Log($"  - 最小缩放: {layoutManager.minScale}");
                Debug.Log($"  - 最大缩放: {layoutManager.maxScale}");
                Debug.Log($"  - 最大Y偏移: {layoutManager.maxYOffset}");
                Debug.Log($"  - 最小透明度: {layoutManager.minAlpha}");
            }

            // 分页管理器信息
            if (paginationManager)
            {
                Debug.Log("[RollViewPaginationManager]");
                Debug.Log($"  - 每页项目数: {paginationManager.itemsPerPage}");
            }

            // 输入处理器信息
            if (inputHandler)
            {
                Debug.Log("[RollViewInputHandler]");
                Debug.Log($"  - 拖拽状态: {(inputHandler.isDragging ? "拖拽中" : "未拖拽")}");
                Debug.Log($"  - 拖拽敏感度: {inputHandler.dragSensitivity}");
                Debug.Log($"  - 滚轮速度: {inputHandler.wheelSpeed}");
            }

            // 箭头控制器信息
            if (arrowController)
            {
                Debug.Log("[RollViewArrowController]");
                Debug.Log($"  - 箭头隐藏透明度: {arrowController.disableAlpha}");
            }

            Debug.Log("==============================");
        }

        #endregion
    }
}