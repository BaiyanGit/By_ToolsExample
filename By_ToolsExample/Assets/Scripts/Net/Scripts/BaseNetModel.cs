namespace Net
{
    using System;
    using _3rdBy.MetaFramework.Singleton;
    using Google.Protobuf;

    /// <summary>
    /// 如果是普通界面非UI框架，可以继承此类。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class BaseNetModel<T> : MonoSingletonTemplate<BaseNetModel<T>> where T : BaseNetModel<T>
    {
        protected override void Awake()
        {
            base.Awake();
            if (Instance != this)
            {
                throw new InvalidOperationException("Can't have two instances of a view");
            }
        }

        protected override void Start()
        {
            base.Start();
            InitAddTocHandler();
        }

        /// <summary>
        /// 用于初始化添加网络消息处理程序
        /// </summary>
        protected abstract void InitAddTocHandler();

        /// <summary>
        /// 添加网络消息处理程序
        /// </summary>
        /// <param name="type">消息类型</param>
        /// <param name="handler"></param>
        protected void AddListenHandler(Type type, MsgHandler handler)
        {
            NetEventHandler.Instance.AddListenHandler(type, handler);
        }

        /// <summary>
        /// 发送网络消息
        /// </summary>
        /// <param name="obj"></param>
        protected void SendMessage(IMessage obj)
        {
            ClientManager.Instance.SendMessage(obj);
        }
    }
}