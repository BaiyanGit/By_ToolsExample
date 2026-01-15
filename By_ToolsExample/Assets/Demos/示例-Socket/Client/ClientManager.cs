namespace Network.Client
{
    using System;
    using System.Net.Sockets;
    using _3rdBy.MetaFramework.Singleton;
    using UnityEngine;

    public class ClientManager : SingletonTemplate<ClientManager>
    {
        /// <summary>
        /// 服务ip地址
        /// </summary>
        private const string IP = "192.168.0.106";

        /// <summary>
        /// 端口
        /// </summary>
        private const int Port = 8006;

        /// <summary>
        /// 接收数据的缓冲区
        /// </summary>
        private static ReceiveBuffer _receiveBuffer;

        /// <summary>
        /// 通信套接字
        /// </summary>
        private Socket _socket;

        /// <summary>
        /// 初始化一些值
        /// </summary>
        public ClientManager()
        {
            _receiveBuffer = new ReceiveBuffer(80 * 1024);
        }

        /// <summary>
        /// 连接服务器
        /// </summary>
        public void ContentToServer()
        {
            // 创建Socket
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            // 将小的数据包组装成更大的包后再发送,以减少网络传输的次数
            _socket.NoDelay = true;
            _socket.BeginConnect(IP, Port, AsyncContentCallBack, _socket);
        }

        /// <summary>
        /// 异步连接服务回调
        /// </summary>
        /// <param name="res">回调结果</param>
        private static void AsyncContentCallBack(IAsyncResult res)
        {
            try
            {
                var socket = (Socket)res.AsyncState; // 返回结果状态
                socket.EndConnect(res); // 等待连接完成,获取连接结果
                Debug.Log("<color=green> Successfully Connected Server </color>");

                // TODO：广播连接服务器成功的状态 Notice To Server State

                // 开始接收消息
                socket.BeginReceive(_receiveBuffer.byteBuffer, _receiveBuffer.writeIndex, _receiveBuffer.RemainSize,
                    SocketFlags.None, ReceiveMsgCallBack, socket);
            }
            catch (SocketException e)
            {
                Debug.Log($"<color=red> Server Connected Fail </color>{e}");
                // TODO：广播连接服务器失败的状态 Notice To Server State
            }
        }

        /// <summary>
        /// 接收到消息回调
        /// </summary>
        private static void ReceiveMsgCallBack(IAsyncResult res)
        {
            try
            {
                var socket = (Socket)res.AsyncState;

                // 结束本次异步接收数据，返回当前消息的字节数
                var dataLength = socket.EndReceive(res);

                ProcessReceiveMsg(); // 处理接收到字节数据

                // 利用递归继续开始接收当前Socket消息
                // socket.BeginReceive(_receiveBuffer.byteBuffer, _receiveBuffer.writeIndex, _receiveBuffer.RemainSize,
                //     SocketFlags.None, ReceiveMsgCallBack, socket);
            }
            catch (SocketException e)
            {
                Debug.Log($"<color=red> Receive Server Message Fail </color>{e}");
            }
        }

        /// <summary>
        /// 处理接收到的二进制字节数据
        /// </summary>
        private static void ProcessReceiveMsg()
        {
            var byteBuffer = _receiveBuffer.byteBuffer;
            var l = byteBuffer.Length;
            var s = System.Text.Encoding.UTF8.GetString(byteBuffer, 0, l);
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