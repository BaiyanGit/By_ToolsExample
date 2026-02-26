namespace Demos.示例_UI曲面叠加滚动.无限滚动模块化.辅助脚本
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// 滚动视图使用示例
    /// 演示如何直接使用各个模块化组件
    /// </summary>
    public class RollViewCoverFlowExample : MonoBehaviour
    {
        [Header("Item预制体")] public GameObject itemPrefab;
        [Header("Item数量")] public int itemCount = 5;
        [Header("滚动管理器")] public RollViewManager rollViewManager;
        [Header("布局管理器")] public RollViewLayoutManager layoutManager;
        [Header("分页管理器")] public RollViewPaginationManager paginationManager;
        [Header("输入处理器")] public RollViewInputHandler inputHandler;
        [Header("箭头控制器")] public RollViewArrowController arrowController;

        [Header("[Debug] 显示当前页码")] public Text pageText;
        [Header("[Debug] 显示详细信息")] public Text infoPanel;

        [Header("[Debug] 跳转页码按钮")] public Button jumpToPageButton;
        [Header("[Debug] 输入页码")] public InputField testPageInput;

        [Header("[Debug] 跳转Item按钮")] public Button jumpToItemButton;
        [Header("[Debug] 输入Item索引")] public InputField testItemIndexInput;

        [Header("[Debug] 快速跳转首页")] public Button goToFirstPageButton;
        [Header("[Debug] 快速跳转末页")] public Button goToLastPageButton;
        [Header("[Debug] 日志输出")] public Text logText;

        private void Awake()
        {
            logText.text = "";
            jumpToPageButton.onClick.AddListener(GoToPageByInput);
            jumpToItemButton.onClick.AddListener(GoToItemByInput);
            goToFirstPageButton.onClick.AddListener(GoToFirstPage);
            goToLastPageButton.onClick.AddListener(GoToLastPage);
        }

        private void Start()
        {
            InitializeItems();
        }

        /// <summary>
        /// 验证组件
        /// </summary>
        private void OnValidate()
        {
            if (!rollViewManager) rollViewManager     = GetComponent<RollViewManager>();
            if (!layoutManager) layoutManager         = GetComponent<RollViewLayoutManager>();
            if (!paginationManager) paginationManager = GetComponent<RollViewPaginationManager>();
            if (!inputHandler) inputHandler           = GetComponent<RollViewInputHandler>();
            if (!arrowController) arrowController     = GetComponent<RollViewArrowController>();
        }

        /// <summary>
        /// 初始化Item
        /// </summary>
        private void InitializeItems()
        {
            // 创建Item
            var items = new List<RectTransform>();
            for (int i = 0; i < itemCount; i++)
            {
                var item = Instantiate(itemPrefab, rollViewManager.content);
                item.name = $"Item {i}";
                item.SetActive(true);
                items.Add(item.GetComponent<RectTransform>());
            }

            rollViewManager.RegisterItem(items);

            // 订阅事件
            SubscribeToAllEvents();

            // 初始化显示
            UpdateDisplay();
        }

        /// <summary>
        /// 销毁时取消事件订阅
        /// </summary>
        private void OnDestroy()
        {
            UnsubscribeAllEvents();
        }

        #region 订阅事件

        /// <summary>
        /// 订阅所有事件
        /// </summary>
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
                paginationManager.OnPageChanged += (page) => { Log($"[分页管理器] 页码已改变: {page}"); };

                paginationManager.OnPageIndicatorClicked += (pageIndex) => { Log($"[分页管理器] 页码指示器被点击: {pageIndex}"); };
            }

            // 订阅输入处理器事件
            if (inputHandler)
            {
                inputHandler.OnBeginDragAction += () => { Log("[输入处理器] 开始拖拽"); };

                inputHandler.OnDragAction += (delta, _) => { Log($"[输入处理器] 拖拽中，偏移量: {delta}"); };

                inputHandler.OnEndDragAction += () => { Log("[输入处理器] 拖拽结束"); };

                inputHandler.OnMoveNextAction += () => { Log("[输入处理器] 键盘右箭头按下"); };

                inputHandler.OnMovePrevAction += () => { Log("[输入处理器] 键盘左箭头按下"); };
            }
        }

        /// <summary>
        /// 取消所有事件订阅
        /// </summary>
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

        /// <summary>
        /// 处理中心项变化
        /// </summary>
        /// <param name="centerIndex"></param>
        private void HandleCenterChanged(int centerIndex)
        {
            Log($"[管理器] 中心项已改变: {centerIndex}");
            UpdateDisplay();
        }

        /// <summary>
        /// 处理页码变化
        /// </summary>
        /// <param name="pageIndex"></param>
        private void HandlePageChanged(int pageIndex)
        {
            Log($"[管理器] 页码已改变: {pageIndex}");
            UpdateDisplay();
        }

        #endregion

        #region UI 更新

        /// <summary>
        /// 更新显示
        /// </summary>
        private void UpdateDisplay()
        {
            if (!rollViewManager)
                return;

            int centerIndex = rollViewManager.GetCenterIndex();
            int currentPage = rollViewManager.GetCurrentPage();
            int totalPages  = rollViewManager.GetTotalPages();

            if (pageText)
                pageText.text = $"页码: {currentPage + 1} / {totalPages}";

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
                Log("布局管理器未配置");
                return;
            }

            layoutManager.itemSpacing = 280f;
            layoutManager.minScale    = 0.6f;
            layoutManager.maxScale    = 1.3f;
            layoutManager.maxYOffset  = 50f;
            layoutManager.minAlpha    = 0.3f;

            Log("[布局管理器] 已修改布局参数");
        }

        /// <summary>
        /// 重置布局参数
        /// </summary>
        public void ResetLayoutSettings()
        {
            if (!layoutManager)
            {
                Log("布局管理器未配置");
                return;
            }

            layoutManager.itemSpacing = 240f;
            layoutManager.minScale    = 0.7f;
            layoutManager.maxScale    = 1.25f;
            layoutManager.maxYOffset  = 0.0f;
            layoutManager.minAlpha    = 0.4f;

            Log("[布局管理器] 已重置布局参数");
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
                Log("分页管理器或管理器未配置");
                return;
            }

            paginationManager.itemsPerPage = Mathf.Max(1, count);
            paginationManager.Initialize(rollViewManager.GetItems().Count);
            Log($"[分页管理器] 已修改每页项目数为: {count}");
        }

        /// <summary>
        /// 通过输入框跳转页码
        /// </summary>
        private void GoToPageByInput()
        {
            if (!rollViewManager || !testPageInput)
                return;

            if (int.TryParse(testPageInput.text, out int pageIndex))
            {
                rollViewManager.GoToPage(pageIndex - 1);
                Log($"[分页管理器] 已跳转到第 {pageIndex} 页");
            }
            else
            {
                Log("请输入有效的页码");
            }
        }

        /// <summary>
        /// 跳转到首页
        /// </summary>
        private void GoToFirstPage()
        {
            if (rollViewManager)
            {
                rollViewManager.GoToPage(0);
                Log("[分页管理器] 已跳转到首页");
            }
        }

        /// <summary>
        /// 跳转到末页
        /// </summary>
        private void GoToLastPage()
        {
            if (rollViewManager)
            {
                int lastPage = rollViewManager.GetTotalPages() - 1;
                rollViewManager.GoToPage(lastPage);
                Log($"[分页管理器] 已跳转到末页");
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
                Log("输入处理器未配置");
                return;
            }

            bool isDragging = inputHandler.isDragging;
            Log($"[输入处理器] 当前拖拽状态: {(isDragging ? "拖拽中" : "未拖拽")}");
        }

        /// <summary>
        /// 修改输入处理器参数
        /// </summary>
        public void ModifyInputHandlerSettings(float dragSensitivity)
        {
            if (!inputHandler)
            {
                Log("输入处理器未配置");
                return;
            }

            inputHandler.dragSensitivity = dragSensitivity;
            Log($"  - 拖拽敏感度: {dragSensitivity}");
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
                Log("箭头控制器未配置");
                return;
            }

            arrowController.disableAlpha = Mathf.Clamp01(alpha);
            Log($"[箭头控制器] 已修改箭头隐藏透明度为: {alpha}");
        }

        #endregion

        #region 项目相关方法

        /// <summary>
        /// 通过输入框居中到指定项目
        /// </summary>
        private void GoToItemByInput()
        {
            if (!rollViewManager || !testItemIndexInput)
                return;

            if (int.TryParse(testItemIndexInput.text, out int itemIndex))
            {
                rollViewManager.CenterOn(itemIndex);
                Log($"[管理器] 已居中到项目 {itemIndex}");
            }
            else
            {
                Log("请输入有效的项目索引");
            }
        }

        /// <summary>
        /// 打印所有信息
        /// </summary>
        public void PrintAllInfo()
        {
            if (!rollViewManager)
                return;
            // 管理器信息
            var log1 = $"  - 中心项索引: {rollViewManager.GetCenterIndex()}\n" +
                       $"  - 当前页码: {rollViewManager.GetCurrentPage() + 1}\n" +
                       $"  - 总页数: {rollViewManager.GetTotalPages()}";
            Debug.Log(log1);

            // 布局管理器信息
            if (layoutManager)
            {
                var log = $"  - 项目间距: {layoutManager.itemSpacing}\n" +
                          $"  - 最小缩放: {layoutManager.minScale}\n" +
                          $"  - 最大缩放: {layoutManager.maxScale}\n" +
                          $"  - 最大Y偏移: {layoutManager.maxYOffset}\n" +
                          $"  - 最小透明度: {layoutManager.minAlpha}";
                Debug.Log(log);
            }

            // 分页管理器信息
            if (paginationManager)
            {
                Log($"  - 每页项目数: {paginationManager.itemsPerPage}");
            }

            // 输入处理器信息
            if (inputHandler)
            {
                var log = $"  - 拖拽状态: {(inputHandler.isDragging ? "拖拽中" : "未拖拽")}\n" +
                          $"  - 拖拽敏感度: {inputHandler.dragSensitivity}";
                Log(log);
            }

            // 箭头控制器信息
            if (arrowController)
            {
                Log($"  - 箭头隐藏透明度: {arrowController.disableAlpha}");
            }

            Debug.Log("==============================");
        }


        public ScrollRect scrollRect;

        private void Log(string message)
        {
            if (logText)
            {
                StartCoroutine(AsyncLogCoroutine(message));
            }
        }

        private IEnumerator AsyncLogCoroutine(string message)
        {
            yield return new WaitForSeconds(0);
            if (logText.text.Length > 5000)
            {
                logText.text = "";
            }

            var time = System.DateTime.Now.ToString("[HH:mm:ss]");
            time = $"<color=green><b>{time}</b></color>";

            var msg = $"<color=red>{message}</color>\n\n";
            logText.text                          += $"{time} {msg}";
            scrollRect.verticalNormalizedPosition =  0f;
        }

        #endregion
    }
}