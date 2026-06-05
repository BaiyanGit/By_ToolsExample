namespace _3rdBy.ByFramework.Guide
{
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Events;

    /// <summary>
    /// 引导管理器
    /// </summary>
    public class GuideManager : MonoBehaviour
    {
        public static GuideManager Instance = null;

        //操作的主相机(Overlay)，由于部分插件Main Camera必须是base，所以不能使用Camera.main
        public Camera controlCam;

        //收集到本图所有相机，可能需要叠加
        public Camera[] cameras;

        public bool ShowTipWhenSucceed { get; set; }

        public UnityAction OnComplete { get; set; }

        [SerializeField] private List<GuideBase> _guideDatas; //引导数据列表
        private IGuideUITip _guideUITip; //引导文本
        private IGuideUITip _guideUIWindow;
        private int _curGuideIndex = -1; //当前引导索引
        private GuideBase _curGuideData; //当前引导数据

        [SerializeField] private bool _autoStart = false;
        [SerializeField] private int _autoStartIndex = 0;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticState()
        {
            Instance = null;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void Start()
        {
            if (_autoStart)
            {
                Begin(_autoStartIndex);
            }
        }

        /// <summary>
        /// 指定UI
        /// </summary>
        /// <param name="tip"></param>
        /// <param name="window"></param>
        public void SetUI(IGuideUITip tip, IGuideUITip window)
        {
            _guideUITip = tip;
            _guideUIWindow = window;
        }

        /// <summary>
        /// 开始引导
        /// </summary>
        /// <param name="startIndex"></param>
        public void Begin(int startIndex = 0)
        {
            if (_guideDatas.Count <= startIndex)
            {
                Debug.LogError("引导数据不正确");
                OnComplete?.Invoke();
                return;
            }

            StartGuide(startIndex); //开始引导
        }

        public void Stop()
        {
            _curGuideData = null;
        }

        private void Update()
        {
            if (_curGuideData != null)
            {
                _curGuideData.OnUpdate();
            }
        }

        /// <summary>
        /// 开始引导
        /// </summary>
        /// <param name="guideIndex"></param>
        private void StartGuide(int index)
        {
            _curGuideIndex = index;
            _curGuideData = _guideDatas[_curGuideIndex];

            _curGuideData.Begin();
            _curGuideData.NewStep = false;

            _curGuideData.GCallBack = (result, msg) =>
            {
                Debug.Log($"guide complete with {msg}");
                _curGuideData.End();
                _curGuideData = null;
                EndGuide(result, msg);
            };
            _curGuideData.GUpdateInfo = (info) => { _guideUITip?.Show(info); };
            _guideUITip?.Show(_curGuideData.GInfo);
        }

        /// <summary>
        /// 结束引导
        /// </summary>
        /// <param name="index"></param>
        private void EndGuide(bool result, object msg)
        {
            if (result)
            {
                _guideUITip?.Hide();
                _curGuideIndex++;
                if (_curGuideIndex < _guideDatas.Count)
                {
                    if (ShowTipWhenSucceed)
                    {
                        StartNext();
                    }
                    else
                    {
                        StartGuide(_curGuideIndex);
                    }
                }
                else
                {
                    CompleteTask();
                }
            }
            else
            {
                StartNext();
            }

            void StartNext()
            {
                if (_guideUIWindow != null)
                {
                    _guideUIWindow.Show(msg, () => { StartGuide(_curGuideIndex); });
                }
                else
                {
                    StartGuide(_curGuideIndex);
                }
            }

            void CompleteTask()
            {
                if (_guideUIWindow != null)
                {
                    _guideUIWindow.Show(msg, () => { OnComplete?.Invoke(); });
                }
                else
                {
                    OnComplete?.Invoke();
                }
            }
        }
    }
}
