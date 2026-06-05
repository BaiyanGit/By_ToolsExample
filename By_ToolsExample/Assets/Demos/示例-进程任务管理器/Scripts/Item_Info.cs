namespace Demos.示例_进程任务管理器.Scripts
{
    using TMPro;
    using UnityEngine;
    using UnityEngine.Events;
    using UnityEngine.UI;

    /// <summary>
    /// 兼容旧 UGUI 的进程列表 Item。
    /// 新版 WindowController 已用 OnGUI 绘制进程列表，可不再使用本预制体脚本。
    /// </summary>
    public class Item_Info : MonoBehaviour
    {
        [Header("显示序号的文本")]
        [SerializeField] private TextMeshProUGUI txtOrderNumber;

        [Header("显示进程 ID 的文本")]
        [SerializeField] private TextMeshProUGUI txtProcessId;

        [Header("显示主窗口句柄的文本")]
        [SerializeField] private TextMeshProUGUI txtWindowHandle;

        [Header("显示进程句柄的文本")]
        [SerializeField] private TextMeshProUGUI txtProcessHandle;

        [Header("显示进程名称的文本")]
        [SerializeField] private TextMeshProUGUI txtProcessName;

        [Header("把当前进程设置为需要显示的按钮")]
        [SerializeField] private Button btnShow;

        [Header("把当前进程设置为需要隐藏的按钮")]
        [SerializeField] private Button btnHide;

        [Header("当前行对应的真实进程名称")]
        [SerializeField] private string processName;

        public UnityAction<string> onClickShowAction;
        public UnityAction<string> onClickHideAction;

        private void Awake()
        {
            if (btnShow != null)
            {
                btnShow.onClick.AddListener(OnClickShow);
            }

            if (btnHide != null)
            {
                btnHide.onClick.AddListener(OnClickHide);
            }
        }

        private void OnDestroy()
        {
            if (btnShow != null)
            {
                btnShow.onClick.RemoveListener(OnClickShow);
            }

            if (btnHide != null)
            {
                btnHide.onClick.RemoveListener(OnClickHide);
            }

            ClearActions();
        }

        public void SetContent(int orderNumber, int processId, string windowHandle, string processHandle, string processName)
        {
            this.processName = processName ?? string.Empty;

            SetText(txtOrderNumber, orderNumber.ToString());
            SetText(txtProcessId, processId.ToString());
            SetText(txtWindowHandle, windowHandle ?? string.Empty);
            SetText(txtProcessHandle, processHandle ?? string.Empty);
            SetText(txtProcessName, this.processName);
        }

        public void ClearActions()
        {
            onClickShowAction = null;
            onClickHideAction = null;
        }

        private static void SetText(TextMeshProUGUI text, string value)
        {
            if (text != null)
            {
                text.text = value;
            }
        }

        private void OnClickHide()
        {
            if (onClickHideAction != null)
            {
                onClickHideAction(processName);
            }
        }

        private void OnClickShow()
        {
            if (onClickShowAction != null)
            {
                onClickShowAction(processName);
            }
        }
    }
}
