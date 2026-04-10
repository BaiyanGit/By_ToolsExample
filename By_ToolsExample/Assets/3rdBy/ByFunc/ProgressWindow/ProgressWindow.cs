namespace _3rdBy.ByFunc.ProgressWindow
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using UnityEngine;
    using Debug = UnityEngine.Debug;
    using Screen = UnityEngine.Screen;

    /// <summary>
    /// 进程窗口，用于显示当前运行的进程信息（需要挂载到GameObject上）
    /// </summary>
    public class ProgressWindow : MonoBehaviour
    {
        [Header("是否显示进程窗口"), SerializeField] private bool _isShow;
        private List<Process> _processList;
        private int _refreshProcessWay = 1; // 0:所有进程 1:窗口进程 2:无窗进程
        private bool _isAutoRefresh = true;
        private float _refreshTime = 5f;
        private int _pageIndex = 0;
        private int _totalPages = 1;
        private const int ShowCount = 6;

        private static ProgressWindow _instance;

        private const string Title = "<color=red>进程信息</color>\n<color=green>Tips: 按住Shift+F1 显示/隐藏窗口</color>  " +
                                     "<color=green>若没有显示你所需要的进程信息，请把当前软件设置为管理员身份运行。\n(软件右键菜单-> 属性 -> 兼容性 -> 以管理员身份运行)</color>";
#if UNITY_ProgressWindow
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
#endif
        private static void Init()
        {
            Debug.LogWarning("ProgressWindow Start");
            if (!_instance)
            {
                var go = new GameObject("ProgressWindow").AddComponent<ProgressWindow>();
                _instance = go;
                DontDestroyOnLoad(go);
            }
        }

        private void Start()
        {
            RefreshProcessList();
        }

        private void Update()
        {
            if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.F1))
            {
                _isShow = !_isShow;
            }


            if (_isAutoRefresh)
            {
                if (_refreshTime <= 0)
                {
                    _refreshTime = 5f;
                    RefreshProcessList();
                }
                else
                {
                    _refreshTime -= Time.deltaTime;
                }
            }
            else
            {
                _refreshTime = 2f;
            }
        }

        private void OnGUI()
        {
            if (_isShow == false) return;
            GUILayout.Label(Title, TitleLabelStyle(), GUILayout.Width(Screen.width), GUILayout.Height(100));
            WindowOperation();
            OnTitle();
            OnShowProgressList();
            OnPage();
        }

        private void WindowOperation()
        {
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("所有进程", ButtonStyle(), CalculateWidth(), CalculateHeight()))
            {
                _refreshProcessWay = 0;
                RefreshProcessList();
            }

            if (GUILayout.Button("窗口进程", ButtonStyle(), CalculateWidth(), CalculateHeight()))
            {
                _refreshProcessWay = 1;
                RefreshProcessList();
            }

            if (GUILayout.Button("无窗进程", ButtonStyle(), CalculateWidth(), CalculateHeight()))
            {
                _refreshProcessWay = 2;
                RefreshProcessList();
            }

            if (GUILayout.Button($"刷新{_refreshTime:F1}", ButtonStyle(), CalculateWidth(),
                    CalculateHeight()))
            {
                RefreshProcessList();
            }

            var refreshText = _isAutoRefresh ? "自动刷新" : "手动刷新";
            if (GUILayout.Button(refreshText, ButtonStyle(), CalculateWidth(), CalculateHeight()))
            {
                _isAutoRefresh = !_isAutoRefresh;
            }

            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// 刷新进程列表
        /// </summary>
        private void RefreshProcessList()
        {
            var allProcess = Process.GetProcesses().ToList();
            _processList = _refreshProcessWay switch
            {
                0 => allProcess,
                1 => allProcess.Where(p => p.MainWindowHandle != IntPtr.Zero).ToList(),
                2 => allProcess.Where(p => p.MainWindowHandle == IntPtr.Zero).ToList(),
                _ => _processList
            };
        }

        private void OnTitle()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("进程ID", LabelStyle(), CalculateWidth(), CalculateHeight());
            GUILayout.Label("进程名", LabelStyle(), CalculateWidth(), CalculateHeight());
            GUILayout.Label("窗口标题", LabelStyle(), CalculateWidth(), CalculateHeight());
            GUILayout.Label("窗口句柄", LabelStyle(), CalculateWidth(), CalculateHeight());
            GUILayout.Label("杀掉进程", LabelStyle(), CalculateWidth(), CalculateHeight());
            GUILayout.EndHorizontal();
        }

        private void OnPage()
        {
            // 获取窗口的高度
            float windowHeight = Screen.height;

            // 创建一个矩形区域（假设高度为50，宽度为窗口宽度）
            var bottomArea = new Rect(0, windowHeight - 50, Screen.width, 50);

            GUILayout.BeginArea(bottomArea);

            GUILayout.BeginHorizontal();

            if (GUILayout.Button("+", ButtonStyle(), CalculateWidth(), CalculateHeight()))
            {
                if (_contentFontSize >= 25)
                {
                    _contentFontSize = 25;
                    return;
                }

                _contentFontSize++;
            }

            if (GUILayout.Button("-", ButtonStyle(), CalculateWidth(), CalculateHeight()))
            {
                if (_contentFontSize <= 5)
                {
                    _contentFontSize = 5;
                    return;
                }

                _contentFontSize--;
            }

            if (GUILayout.Button("上一页", ButtonStyle(), CalculateWidth(), CalculateHeight()))
            {
                _pageIndex--;
            }

            GUILayout.Label($"第 {_pageIndex + 1} / {_totalPages} 页", LabelStyle(), CalculateWidth(), CalculateHeight());

            if (GUILayout.Button("下一页", ButtonStyle(), CalculateWidth(), CalculateHeight()))
            {
                _pageIndex++;
            }


            GUILayout.EndHorizontal();

            GUILayout.EndArea();
        }

        private void OnShowProgressList()
        {
            _processList ??= Process.GetProcesses().ToList();
            _totalPages  =   (int)Math.Ceiling((double)_processList.Count / ShowCount);

            _pageIndex = _totalPages <= 1 ? 0 : Math.Max(0, Math.Min(_pageIndex, _totalPages - 1));
            var startIndex  = _pageIndex * ShowCount;
            var endIndex    = Math.Min(_pageIndex * ShowCount + ShowCount, _processList.Count);
            var processList = _processList.Skip(startIndex).Take(endIndex - startIndex).ToList();

            foreach (var process in processList)
            {
                try
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(process.Id.ToString(), LabelContentStyle(), CalculateWidth(), CalculateHeight());
                    GUILayout.Label(process.ProcessName, LabelContentStyle(), CalculateWidth(), CalculateHeight());
                    GUILayout.Label(process.MainWindowTitle, LabelContentStyle(), CalculateWidth(), CalculateHeight());
                    GUILayout.Label(process.MainWindowHandle.ToString(), LabelContentStyle(), CalculateWidth(),
                        CalculateHeight());
                    if (GUILayout.Button("Kill", ButtonStyle(), CalculateWidth(), CalculateHeight()))
                    {
                        process.Kill();
                    }
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                    GUILayout.EndHorizontal();
                    continue;
                }

                GUILayout.EndHorizontal();
            }
        }


        #region 自定义样式

        private static GUILayoutOption CalculateWidth()
        {
            var w = Screen.width / 5;
            return GUILayout.Width(w);
        }

        private static GUILayoutOption CalculateHeight()
        {
            return GUILayout.Height(40);
        }

        /// <summary>
        /// 按钮样式
        /// </summary>
        private GUIStyle _buttonStyle;

        private GUIStyle ButtonStyle()
        {
            _buttonStyle ??= new GUIStyle(GUI.skin.button)
            {
                fontStyle = FontStyle.Bold,
                fontSize  = 18,
                alignment = TextAnchor.MiddleCenter,
            };
            _buttonStyle.normal.background = Texture2D.grayTexture;
            return _buttonStyle;
        }

        /// <summary>
        /// 说明文字样式
        /// </summary>
        private GUIStyle _titleStyle;

        private GUIStyle TitleLabelStyle()
        {
            _titleStyle ??= new GUIStyle(GUI.skin.label)
            {
                fontStyle = FontStyle.Bold,
                fontSize  = 18,
                alignment = TextAnchor.MiddleCenter,
            };
            _titleStyle.normal.background = Texture2D.whiteTexture;
            return _titleStyle;
        }

        /// <summary>
        /// 标题文字样式
        /// </summary>
        private GUIStyle _labelStyle;

        private GUIStyle LabelStyle()
        {
            _labelStyle ??= new GUIStyle(GUI.skin.label)
            {
                fontStyle = FontStyle.Bold,
                fontSize  = 17,
                alignment = TextAnchor.MiddleCenter,
                normal    = { textColor = Color.black }
            };
            _labelStyle.normal.background = Texture2D.whiteTexture;
            return _labelStyle;
        }

        /// <summary>
        /// 进程内容文字样式
        /// </summary>
        private GUIStyle _labelContentStyle;

        private int _contentFontSize = 13;

        private GUIStyle LabelContentStyle()
        {
            _labelContentStyle ??= new GUIStyle(GUI.skin.label)
            {
                fontStyle = FontStyle.Normal,
                // fontSize = contentFontSize,
                alignment = TextAnchor.MiddleLeft,
                normal    = { textColor = Color.black }
            };

            _labelContentStyle.fontSize          = _contentFontSize;
            _labelContentStyle.normal.background = Texture2D.grayTexture;
            return _labelContentStyle;
        }

        #endregion
    }
}