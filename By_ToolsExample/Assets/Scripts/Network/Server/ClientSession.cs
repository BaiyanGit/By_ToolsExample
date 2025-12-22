namespace Network.Server
{
    using System;
    using System.IO;
    using System.Net.Sockets;
    using System.Runtime.Serialization.Formatters.Binary;
    using Client;
    using JetBrains.Annotations;
    using UnityEngine;

    /// <summary>
    /// Socket会话类
    /// </summary>
    public sealed class ClientSession
    {
        /// <summary>
        /// 当前客户端Id
        /// </summary>
        public readonly int clientHashCode;

        /// <summary>
        /// 当前客户端的套接字
        /// </summary>
        public readonly Socket socket;

        /// <summary>
        /// 当前客户端的Ip和端口
        /// </summary>
        public string RemoteEndPoint => socket.RemoteEndPoint.ToString();

        /// <summary>
        /// 接收数据字节缓冲区
        /// </summary>
        public ReceiveBuffer receiveBuffer;

        /// <summary>
        /// 初始化变量
        /// </summary>
        public ClientSession([NotNull] Socket socket, int bufferSize = 1024)
        {
            this.socket = socket;
            clientHashCode = socket.GetHashCode();
            receiveBuffer = new ReceiveBuffer((int)Mathf.Pow(bufferSize, 2));
        }

        /// <summary>
        /// 关闭连接
        /// </summary>
        public void CloseConnection()
        {
            // socket为空或者未连接
            if (socket is { Connected: false }) return;

            // 禁用收发
            socket.Shutdown(SocketShutdown.Both);
            // 断开连接
            socket.Close();
            // 移除当前客户端会话
            ClientSessionManager.ClientSessions.RemoveClientSession(this);
        }
        
        

        /// <summary>
        /// 发送消息给客户端
        /// </summary>
        /// <param name="data"></param>
        public void SendMsgToClient(byte[] data)
        {
            if (socket is not { Connected: true })
            {
                Debug.Log($"客户端未连接, 无法发送消息{socket.RemoteEndPoint}");
                return;
            }

            Log(data);

            try
            {
                socket.BeginSend(data, 0, data.Length, SocketFlags.None, SendCallBack, null);
            }
            catch (SocketException e)
            {
                Debug.Log(e.Message);
                throw;
            }
        }

        private static void SendCallBack(IAsyncResult res)
        {
            var currentSession = (ClientSession)res.AsyncState;
            try
            {
                Debug.Log($"发送成功{currentSession.socket.Connected}");
            }
            catch (SocketException e)
            {
                Debug.Log(e.Message);
                throw;
            }
        }

        /// <summary>
        /// 实现消息头和消息体合并CombineBytes方法
        /// </summary>
        /// <param name="headBytes"></param>
        /// <param name="bodyBytes"></param>
        /// <returns></returns>
        public static byte[] CombineBytes(byte[] headBytes, byte[] bodyBytes)
        {
            var bytes = new byte[headBytes.Length + bodyBytes.Length];
            Buffer.BlockCopy(headBytes, 0, bytes, 0, headBytes.Length);
            Buffer.BlockCopy(bodyBytes, 0, bytes, headBytes.Length, bodyBytes.Length);
            return bytes;
        }


        /// <summary>
        /// 日志
        /// </summary>
        /// <param name="data"></param>
        private static void Log(byte[] data)
        {
            var bytes = new byte[8];
            Buffer.BlockCopy(data, 0, bytes, 0, 8);
            // 获取消息头和长度
            var msgBase = (MsgBase)Deserializer(bytes, 0, 8);
            Debug.Log($"发送消息头 {msgBase.msgHeader} {msgBase.msgLength}");

            // 反序列化消息体
            var login = (LoginData)Deserializer(data, 8, data.Length);

            Debug.Log($"发送消息体==> 长度：{data.Length}  用户名：{login.userName} 密码：{login.password}");
        }

        /// <summary>
        /// 反序列化方法
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        private static object Deserializer(byte[] bytes, int offset, int count)
        {
            using var ms = new MemoryStream(bytes, offset, count);
            var formatter = new BinaryFormatter();
            return formatter.Deserialize(ms);
        }

        /// <summary>
        /// 序列化方法
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static byte[] Serializer(object obj)
        {
            using var ms = new MemoryStream();
            var formatter = new BinaryFormatter();
            formatter.Serialize(ms, obj);
            return ms.ToArray();
        }

        /// <summary>
        /// 处理来自客户端的消息
        /// </summary>
        public void HandleClientMessage()
        {
            var l = receiveBuffer.Length;
            var s = System.Text.Encoding.UTF8.GetString(receiveBuffer.byteBuffer, 0, l);
            Debug.Log($"长度：{l} 内容：{s}");
        }

        /*
        ------------------------占位---------------------------
        ------------------------------------------------------
        ------------------------------------------------------
        ------------------------------------------------------
        ------------------------------------------------------
        ------------------------------------------------------
        ------------------------------------------------------
        ------------------------------------------------------
        ------------------------------------------------------
        ------------------------------------------------------
        ------------------------------------------------------
        ------------------------------------------------------
        ------------------------------------------------------
        ------------------------------------------------------
        ------------------------------------------------------
        ------------------------------------------------------
        ------------------------------------------------------
        ------------------------------------------------------
        ------------------------------------------------------
        ------------------------------------------------------
        ------------------------------------------------------
        ------------------------------------------------------
        ------------------------------------------------------
        ------------------------------------------------------
        ------------------------------------------------------
        ------------------------------------------------------
        ------------------------------------------------------
        ------------------------------------------------------
        ------------------------------------------------------
        ------------------------------------------------------
        ------------------------------------------------------
        ------------------------------------------------------
        ------------------------------------------------------
        ------------------------------------------------------
        ------------------------------------------------------
        ------------------------------------------------------
        ------------------------------------------------------
        ------------------------------------------------------
        ------------------------------------------------------
        ------------------------------------------------------
        ------------------------------------------------------
        ------------------------------------------------------
        ------------------------------------------------------
        ------------------------------------------------------
        ------------------------------------------------------
        ------------------------------------------------------
        ------------------------------------------------------
        ------------------------------------------------------
        ------------------------------------------------------
        ------------------------------------------------------
        */
    }
}