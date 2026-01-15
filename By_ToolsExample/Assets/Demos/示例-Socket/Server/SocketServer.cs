namespace Network.Server
{
    using System;
    using System.Net;
    using System.Net.Sockets;
    using _3rdBy.MetaFramework.Singleton;
    using UnityEngine;

    /// <summary>
    /// Socket服务端,启动Socket接受客户端消息
    /// </summary>
    public class SocketServer : SingletonTemplate<SocketServer>
    {
        /// <summary>
        /// 监听并接受客户端的连接请求
        /// </summary>
        private Socket _listenSocket;

        public void CloseServer()
        {
            ClientSessionManager.ClientSessions.RemoveAllClientSession();
            _listenSocket.Close();
            _listenSocket = null;
        }

        /// <summary>
        /// 启动服务端
        /// </summary>
        public void StartServer()
        {
            _listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _listenSocket.Bind(new IPEndPoint(IPAddress.Any, 8006));
            _listenSocket.Listen(ClientSessionManager.MaxConnections);
            _listenSocket.BeginAccept(AsyncConnectionCallback, null);
            Debug.Log("<color=green> Service Started Successfully </color>");
        }

        /// <summary>
        /// 客户端与服务器建立连接回调
        /// </summary>
        private void AsyncConnectionCallback(IAsyncResult res)
        {
            try
            {
                if (_listenSocket == null) return;

                // 判断当前客户端数量是否已满
                if (ClientSessionManager.ClientSessions.IsClientFull())
                {
                    Debug.Log("Client Count is Full");
                    return;
                }

                // 停止监听和接受新的连接请求
                var clientSocket = _listenSocket.EndAccept(res);

                // 创建客户端会话
                ClientSessionManager.CreatClientSession(clientSocket, out var session, out var receiveBuffer,
                    out var clientAddress);

                Debug.Log($"有新客户端连接：{clientAddress}，" +
                          $"当前客户端数量：{ClientSessionManager.ClientSessions.GetClientCount()}，" +
                          $"客户端ID：{session.socket.GetHashCode()}");

                // 开始接收已连接的客户端的消息
                clientSocket.BeginReceive(receiveBuffer.byteBuffer, receiveBuffer.writeIndex, receiveBuffer.RemainSize,
                    SocketFlags.None, ReceiveMsgFromClient, session);

                // 递归监听新客户端是否建立连接
                _listenSocket.BeginAccept(AsyncConnectionCallback, null);
            }
            catch (Exception e)
            {
                Debug.Log("Accept失败: " + e.Message);
                throw;
            }
        }

        /// <summary>
        /// 接收客户端消息回调
        /// </summary>
        /// <param name="res"></param>
        private static void ReceiveMsgFromClient(IAsyncResult res)
        {
            // 当前客户端会话
            var currentSession = (ClientSession)res.AsyncState;

            try
            {
                // 结束本次异步接收的数据，返回当前消息的字节数，如果接收到的消息长度为0，则说明客户端断开连接
                var msgLength = currentSession.socket.EndReceive(res);
                if (msgLength == 0)
                {
                    currentSession.CloseConnection();
                    Debug.Log($"<color=red> Client Disconnected </color>{msgLength}");
                    return;
                }

                // 处理接收当前客户端的消息
                currentSession.HandleClientMessage();
            }
            catch (Exception e)
            {
                Debug.Log($"Client {currentSession.RemoteEndPoint} Disconnected {e.Message}");
                currentSession.CloseConnection();
            }
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