namespace _3rdBy.MetaFramework.UI
{
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// UI基类
    /// </summary>
    /// <typeparam name="TM">Model</typeparam>
    /// <typeparam name="TV">View</typeparam>
    public abstract class UIBase<TM, TV> : IUIBase
        where TM : class, IUIModel
        where TV : class, IUIView
    {
        /// <summary>
        /// UI名称
        /// </summary>
        public string uiName { get; set; }

        /// <summary>
        /// UI实体
        /// </summary>
        public GameObject uiGo { get; set; }

        /// <summary>
        /// UI视觉效果属性
        /// </summary>
        public UIHierarchy uiHierarchy { get; set; }

        /// <summary>
        /// UI类型
        /// </summary>
        public UIType uiType { get; set; }

        /// <summary>
        /// 是否正在显示
        /// </summary>
        public bool isShowing { get; set; }

        /// <summary>
        /// 获取Model
        /// </summary>
        public IUIModel uiModel { get; set; }

        /// <summary>
        /// 获取View
        /// </summary>
        public IUIView uiView { get; set; }

        /// <summary>
        /// 获取Model
        /// </summary>
        /// <returns></returns>
        public TM GetModel()
        {
            return uiModel as TM;
        }

        /// <summary>
        /// 获取View
        /// </summary>
        /// <returns></returns>
        public TV GetView()
        {
            return uiView as TV;
        }

        /// <summary>
        /// 获取(设置)当前UI显示的层级
        /// </summary>
        /// <returns></returns>
        public abstract UILayer GetLayer();

        /// <summary>
        /// 注册关闭按钮，不卸载
        /// </summary>
        /// <param name="button"></param>
        protected void RegisterCloseHandler(Button button)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => { CloseSelf(); });
        }

        /// <summary>
        /// 关闭自己，不卸载
        /// </summary>
        protected void CloseSelf()
        {
            if (uiType == UIType.List)
            {
                UIManager.Instance.CloseNormal(uiName);
            }
            else
            {
                UIManager.Instance.CloseStack();
            }
        }

        #region 生命周期

        /// <summary>
        /// 初始化
        /// </summary>
        public virtual void OnInit()
        {
        }

        /// <summary>
        /// 进入
        /// </summary>
        public virtual void OnEnter(params object[] args)
        {
        }

        /// <summary>
        /// Stack 暂停
        /// </summary>
        public virtual void OnPause()
        {
        }

        /// <summary>
        /// Stack 恢复
        /// </summary>
        public virtual void OnResume()
        {
        }

        /// <summary>
        /// 退出
        /// </summary>
        public virtual void OnExit()
        {
        }

        /// <summary>
        /// 更新
        /// </summary>
        public virtual void OnUpdate()
        {
        }

        #endregion
    }
}