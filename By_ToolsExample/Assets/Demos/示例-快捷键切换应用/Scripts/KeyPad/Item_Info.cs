using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Demos.示例_快捷键切换应用.Scripts.KeyPad
{
    using UnityEngine.UI;

    public class Item_Info : MonoBehaviour
    {
        [Header("序号"), SerializeField] private TextMeshProUGUI txtOrderNumber;
        [Header("进程ID"), SerializeField] private TextMeshProUGUI txtProcessId;
        [Header("主窗口句柄"), SerializeField] private TextMeshProUGUI txtWindowHandle;
        [Header("进程句柄"), SerializeField] private TextMeshProUGUI txtProcessHandle;
        [Header("进程名称"), SerializeField] private TextMeshProUGUI txtProcessName;

        [Header("选择显示按钮"), SerializeField] private Button btnShow;
        [Header("选择隐藏按钮"), SerializeField] private Button btnHide;

        public UnityAction<string> onClickShowAction;
        public UnityAction<string> onClickHideAction;

        private void Awake()
        {
            btnShow.onClick.AddListener(OnClickShow);
            btnHide.onClick.AddListener(OnClickHide);
        }

        /// <summary>
        /// 设置内容
        /// </summary>
        /// <param name="orderNumber"></param>
        /// <param name="processId"></param>
        /// <param name="windowHandle"></param>
        /// <param name="processHandle"></param>
        /// <param name="processName"></param>
        public void SetContent(int orderNumber, int processId, string windowHandle, string processHandle,
            string processName)
        {
            txtOrderNumber.text   = orderNumber.ToString();
            txtProcessId.text     = processId.ToString();
            txtWindowHandle.text  = windowHandle;
            txtProcessHandle.text = processHandle;
            txtProcessName.text   = processName;
        }

        /// <summary>
        /// 点击隐藏
        /// </summary>
        private void OnClickHide()
        {
            onClickHideAction?.Invoke(txtProcessName.text);
        }

        /// <summary>
        /// 点击显示
        /// </summary>
        private void OnClickShow()
        {
            onClickShowAction?.Invoke(txtProcessName.text);
        }
    }
}