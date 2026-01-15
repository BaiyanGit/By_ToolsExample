namespace _3rdBy.MetaFramework.Guide
{
    using UnityEngine;
    using UnityEngine.Events;

    /// <summary>
    /// 事件引导类型
    /// </summary>
    public enum EEventGuideType
    {
        None,
        SingleClick,
        DoubleClick,
        GREAT, //可扩充，在GuideEvents中添加事件检测，在GuideManager中绑定回调方法
    }

    /// <summary>
    /// 引导数据基类
    /// </summary>
    public class GuideBase : MonoBehaviour
    {
        public int GIndex;
        public string GInfo;
        public GameObject GTargetObj;
        public UnityAction<bool, object> GCallBack;
        public UnityAction<string> GUpdateInfo;

        public bool NewStep { get; set; } = true;

        /// <summary>
        /// 初始化
        /// </summary>
        public virtual void Awake()
        {
        }

        /// <summary>
        /// 开始时
        /// </summary>
        public virtual void Begin()
        {
        }

        /// <summary>
        /// 结束时
        /// </summary>
        public virtual void End()
        {
        }

        /// <summary>
        /// 帧更新
        /// </summary>
        public virtual void OnUpdate()
        {
        }
    }

    public class UIGuideBase : MonoBehaviour
    {
        /// <summary>
        /// 目标
        /// </summary>
        public RectTransform Target;

        /// <summary>
        /// 画布
        /// </summary>
        public Canvas CurrentCanvas;

        /// <summary>
        /// 设置数据
        /// </summary>
        /// <param name="target"></param>
        /// <param name="canvas"></param>
        public virtual void SetTarget(RectTransform target, Canvas canvas)
        {
        }
    }

    public interface IGuideUITip
    {
        public void Show(object msg, UnityAction callBack = null);
        public void Hide();
    }
}