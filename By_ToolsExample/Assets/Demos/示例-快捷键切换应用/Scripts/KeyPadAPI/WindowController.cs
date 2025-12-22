using System;
using System.Diagnostics;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

namespace KeyPad
{
    public class WindowController : MonoBehaviour
    {
        [SerializeField] private Transform itemParent;
        [SerializeField] private Item_Info itemSource;

        [SerializeField] private Button btnMaximize; // 最大化按钮
        [SerializeField] private Button btnMinimize; // 最小化按钮
        [SerializeField] private Button btnRestore; // 还原按钮
        [SerializeField] private Button btnClose; // 关闭按钮
        [SerializeField] private Button btnPrintLog; // 打印日志按钮
        [SerializeField] private Toggle btnWinHandle; // 打印日志按钮

        [SerializeField] private TMP_InputField inputFieldShow; // 要显示的进程名
        [SerializeField] private TMP_InputField inputFieldHide; // 要隐藏的进程名
        [SerializeField] private TextMeshProUGUI textMeshProUGUI; // 按键按下次数

        private KeyboardHook _windowsKeyboardHook; // 全局键盘钩子
        private const string Tip = "显示窗口进程不能和隐藏窗口进程相同"; // 提示
        private bool _isPress;

        private KeySettings _keySettings;

        private void Awake()
        {
            btnMaximize.onClick.AddListener(WindowAPI.WindowMaximize);
            btnMinimize.onClick.AddListener(WindowAPI.WindowMinimize);
            btnRestore.onClick.AddListener(WindowAPI.WindowRestore);
            btnClose.onClick.AddListener(WindowAPI.WindowClose);
            btnPrintLog.onClick.AddListener(PrintLog);

            _windowsKeyboardHook = new KeyboardHook();
            _windowsKeyboardHook.KeyboardPressed += OnKeyPressed;
            _keySettings = GetComponent<KeySettings>();

            Application.runInBackground = true;
        }

        private void OnApplicationQuit()
        {
            _windowsKeyboardHook?.Dispose(); // 释放全局键盘钩子
        }


        private void SwitchApplication(int keyCode)
        {
            Debug.Log(keyCode);
            // 按下的键与当前按键一致
            if (_keySettings.key1 != keyCode) return;

            Debug.Log("切换程序");

            WindowAPI.ShowWindow(GetWindowHandle(inputFieldShow.text));
            WindowAPI.WindowMiniimize(GetWindowHandle(inputFieldHide.text));

            // 交换两个进程的显示和隐藏
            (inputFieldShow.text, inputFieldHide.text) = (inputFieldHide.text, inputFieldShow.text);
        }

        /// <summary>
        /// 键盘上的按键按下回调
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnKeyPressed(object sender, GlobalKeyboardHookEventArgs e)
        {
            var state = e.KeyboardState;
            var code = e.KeyboardData.VirtualCode;

            if (state == KeyboardState.KeyDown)
            {
                if (_isPress) return;
                SwitchApplication(code);
                Debug.Log($"{e.Handled}");
                // e.Handled = true; // true:其他程序不可以输入  false:其他程序可以输入
                _isPress = true;
            }

            if (state == KeyboardState.KeyUp)
            {
                _isPress = false;
            }
        }

        /// <summary>
        /// 根据进程获取窗口句柄
        /// </summary>
        /// <param name="processName"></param>
        /// <returns></returns>
        private IntPtr GetWindowHandle(string processName)
        {
            var mainWindowHandle = IntPtr.Zero;
            var processArray = Process.GetProcessesByName(processName);

            foreach (var process in processArray)
            {
                if (process.MainWindowHandle == IntPtr.Zero) continue; // 没有窗口句柄则跳过本次循环
                if (process.ProcessName != processName) continue;

                mainWindowHandle = process.MainWindowHandle;
                break;
            }

            return mainWindowHandle;
        }

        public void OnDragWindow()
        {
            Debug.Log("OnDragWindow");
            WindowAPI.DragWindowsMethod();
        }

        /// <summary>
        /// 打印日志
        /// </summary>
        private void PrintLog()
        {
            var processArray = Process.GetProcesses();
            var index = 0;

            foreach (var item in itemParent.GetComponentsInChildren<Item_Info>())
            {
                Destroy(item.gameObject);
            }

            foreach (var process in processArray)
            {
                var processId = process.Id; // 进程ID
                var mainWindowHandle = process.MainWindowHandle; // 主窗口句柄
                var processHandle = process.Handle.ToString(); // 进程句柄
                var processName = process.ProcessName; // 进程名称

                if (btnWinHandle.isOn && mainWindowHandle == IntPtr.Zero) continue; // 没有窗口句柄则跳过本次循环

                index++;
                var newItem = Instantiate(itemSource, itemParent);
                newItem.SetContent(index, processId, mainWindowHandle.ToString(), processHandle, processName);
                newItem.gameObject.SetActive(true);

                newItem.onClickShowAction += OnClickShow;
                newItem.onClickHideAction += OnClickHide;
            }
        }

        /// <summary>
        /// 点击显示
        /// </summary>
        /// <param name="processName"></param>
        private void OnClickShow(string processName)
        {
            if (string.IsNullOrEmpty(inputFieldShow.text) || inputFieldShow.text == Tip)
            {
                inputFieldShow.text = processName;
            }

            if (inputFieldHide.text == inputFieldShow.text)
            {
                inputFieldShow.text = Tip;
                return;
            }

            inputFieldShow.text = processName;
        }

        /// <summary>
        /// 点击隐藏
        /// </summary>
        /// <param name="processName"></param>
        private void OnClickHide(string processName)
        {
            if (string.IsNullOrEmpty(inputFieldHide.text) || inputFieldHide.text == Tip)
            {
                inputFieldHide.text = processName;
            }

            if (inputFieldHide.text == inputFieldShow.text)
            {
                inputFieldHide.text = Tip;
                return;
            }

            inputFieldHide.text = processName;
        }
    }
}