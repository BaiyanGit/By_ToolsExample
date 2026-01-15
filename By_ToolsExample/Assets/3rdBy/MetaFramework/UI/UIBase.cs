namespace _3rdBy.MetaFramework.UI
{
    using Interface;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// UI,UIBase,Model,View
    /// </summary>
    /// <typeparam name="TM">Model</typeparam>
    /// <typeparam name="TV">View</typeparam>
    public abstract class UIBase<TM, TV> : IUIBase
        where TM : class, IUIModel
        where TV : class, IUIView
    {
        /// <summary>
        /// ui name
        /// </summary>
        public string UIName { get; set; }

        /// <summary>
        /// UI实体
        /// </summary>
        public GameObject UIGo { get; set; }

        public UIHierarchy UIHierarchy { get; set; }

        public UIType UIType { get; set; }

        public bool IsShowing { get; set; }

        /// <summary>
        /// model
        /// </summary>
        public IUIModel uiModel { get; set; }

        /// <summary>
        /// view
        /// </summary>
        public IUIView uiView { get; set; }

        public TM GetModel()
        {
            return uiModel as TM;
        }

        public TV GetView()
        {
            return uiView as TV;
        }

        public abstract UILayer GetLayer();

        /// <summary>
        /// 注册关闭按钮，不卸载
        /// </summary>
        /// <param name="button"></param>
        protected void RegisterCloseHandler(Button button)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() =>
            {
                CloseSelf();
            });
        }

        /// <summary>
        /// 关闭自己，不卸载
        /// </summary>
        protected void CloseSelf()
        {
            if (UIType == UIType.List)
            {
                UIManager.Instance.CloseNormal(UIName);
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