namespace _3rdBy.ByFramework.Socket.Example
{
    using Proto;
    using Scripts;
    using Singleton;
    using UnityEngine;

    /// <summary>
    /// 心跳消息发送与监听
    /// </summary>
    public class Heartbeat : MonoObjSingletonTemplate<Heartbeat>
    {
        private float _sendHeartTime;                  //发送心跳包的时间
        private float _receiveHeartTime;               //接收心跳包的时间
        private readonly HeartBeat _heartBeat = new(); //心跳包

        private void Awake()
        {
            NetEventHandler.Instance.AddListenHandler(typeof(HeartBeat), ReceiveHeartBeat); //监听心跳包
        }

        /// <summary>
        /// 初始化心跳包
        /// </summary>
        public void InitHeartBeat()
        {
            Debug.Log("[Heartbeat] 初始化心跳。");
            _receiveHeartTime = 0;
        }

        private void Update()
        {
            if (!ClientManager.Instance.SocketSession.ConnectedState())
            {
                Debug.Log("网络已断开连接");
                return;
            }

            SendHeartBeatMsg(); //发送心跳包
            HeartBeatConnect(); //检测心跳包是否超时
        }

        /// <summary>
        /// 发送心跳消息
        /// </summary>
        private void SendHeartBeatMsg()
        {
            _sendHeartTime += Time.deltaTime;
            if (_sendHeartTime < 4) return;                 //4秒发送一次心跳包
            ClientManager.Instance.SendMessage(_heartBeat); //发送心跳包
            _sendHeartTime = 0;                             //重置发送心跳包的时间
        }

        /// <summary>
        /// 接收心跳消息(监听心跳)
        /// </summary>
        /// <param name="data"></param>
        private void ReceiveHeartBeat(object data)
        {
            _receiveHeartTime = 0; //重置收到心跳包的时间
        }

        /// <summary>
        /// 检测心跳包是否超时
        /// </summary>
        private void HeartBeatConnect()
        {
            _receiveHeartTime += Time.deltaTime;
            Debug.Log($"检测心跳包超时计时:{(int)_receiveHeartTime}");
            if (_receiveHeartTime < 6) return;       //6秒检测一次心跳包是否超时
            ClientManager.Instance.OnOnDisConnect(); //心跳包超时，断开连接
        }
    }
}
