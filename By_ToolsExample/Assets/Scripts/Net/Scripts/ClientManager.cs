/*
        *********************************************
                           _ooOoo_
                          o8888888o
                          88" . "88
                          (| -_- |)
                          O\  =  /O
                       ____/`---'\____
                     .'  \\|     |//  `.
                    /  \\|||  :  |||//  \
                   /  _||||| -:- |||||-  \
                   |   | \\\  -  /// |   |
                   | \_|  ''\---/''  |   |
                   \  .-\__  `-`  ___/-. /
                 ___`. .'  /--.--\  `. . __
              ."" '<  `.___\_<|>_/___.'  >'"".
             | | :  `- \`.;`\ _ /`;.`/ - ` : | |
             \  \ `-.   \_ __\ /__ _/   .-` /  /
        ======`-.____`-.___\_____/___.-`____.-'======
                           `=---='
        ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
                    佛祖保佑       永无BUG
                    --------------------
*/

namespace Net
{
    using System.Collections.Generic;
    using System;
    using System.IO;
    using _3rdBy.MetaFramework.Singleton;
    using Google.Protobuf;
    using UnityEngine;

    /// <summary>
    /// 客户端管理
    /// </summary>
    public class ClientManager : MonoSingletonTemplate<ClientManager>
    {
        private ClientSession _socketSession;

        public ClientSession SocketSession
        {
            get { return _socketSession ??= new ClientSession(); }
        }

        protected override void Awake()
        {
            base.Awake();
            Init();
        }

        private void Init()
        {
            SocketSession.OnRegister();
        }

        #region 建立连接和断开连接

        /// <summary>
        /// 发送连接服务器请求
        /// </summary>
        public void SendConnect()
        {
            Heartbeat.Instance.InitHeartBeat(); //重置心跳包超时时间
            SocketSession.SendConnect();
        }

        /// <summary>
        /// 关闭网络连接与释放相关资源
        /// </summary>
        public void OnOnDisConnect()
        {
            SocketSession.OnRemove();
        }

        #endregion

        #region 发送消息

        /// <summary>
        /// 将 Protobuf 消息对象序列化为字节数组
        /// </summary>
        public void SendMessage(IMessage obj)
        {
            if (!ProtoDictionary.IsContainProtoType(obj.GetType()))
            {
                Debug.LogError("不存协议类型");
                return;
            }

            byte[] result;
            using (var ms = new MemoryStream())
            {
                obj.WriteTo(ms);
                result = ms.ToArray();
            }

            var buffer = new ByteBuffer();
            var length = (ushort)(result.Length + 2);
            var protoId = ProtoDictionary.GetProtoIdByProtoType(obj.GetType());
            buffer.WriteShort(length); //Protobuf消息的包体长度
            buffer.WriteShort((ushort)protoId); //协议号
            buffer.WriteBytes(result); //包体数据
            Debug.Log($"<color=yellow>SendMsg Id:{protoId} length:{length}</color>");
            SendMessage(buffer);
        }

        /// <summary>
        /// 发送Socket消息
        /// </summary>
        /// <param name="buffer"></param>
        private void SendMessage(ByteBuffer buffer)
        {
            _socketSession.SendMsg(buffer);
        }

        #endregion

        #region 消息派发

        private static Queue<KeyValuePair<Type, object>> _msgEvents = new();

        /// <summary>
        /// 派发协议
        /// </summary>
        /// <param name="protoId">协议号</param>
        /// <param name="buff">消息体</param>
        public void DispatchProto(int protoId, byte[] buff)
        {
            if (!ProtoDictionary.IsContainProtoId(protoId))
            {
                Debug.LogError("不存在的协议类型");
                return;
            }

            var protoType = ProtoDictionary.GetProtoTypeByProtoId(protoId);
            try
            {
                var msgParser = ProtoDictionary.GetMessageParser(protoType.TypeHandle);
                object toc = msgParser.ParseFrom(buff);
                Debug.Log($"<color=green>Receive Id: {protoId},  Type: {protoType}</color>");
                _msgEvents.Enqueue(new KeyValuePair<Type, object>(protoType, toc)); //消息进入队列，等待广播
            }
            catch
            {
                Debug.LogError($"DispatchProto Error: {protoType}");
            }
        }

        private void Update()
        {
            BroadcastMessage();
        }

        /// <summary>
        /// 广播消息
        /// </summary>
        private static void BroadcastMessage()
        {
            if (_msgEvents.Count == 0) return;

            while (_msgEvents.Count > 0)
            {
                var data = _msgEvents.Dequeue();
                NetEventHandler.Instance.BroadcastHandler(data.Key, data.Value);
            }
        }

        #endregion
    }
}