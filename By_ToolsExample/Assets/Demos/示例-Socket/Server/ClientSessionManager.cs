namespace Network.Server
{
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Sockets;
    using Client;

    public static class ClientSessionManager
    {
        /// <summary>
        /// 客户端会话管理池
        /// </summary>
        public static readonly Dictionary<int, ClientSession> ClientSessions = new();

        /// <summary>
        /// 最大客户端连接数量
        /// </summary>
        public const int MaxConnections = 10;

        /// <summary>
        /// 创建新客户端会话
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="session"></param>
        /// <param name="receiveBuffer"></param>
        /// <param name="endPoint"></param>
        public static void CreatClientSession(Socket socket, out ClientSession session,
            out ReceiveBuffer receiveBuffer, out EndPoint endPoint)
        {
            session = new ClientSession(socket);
            endPoint = socket.RemoteEndPoint; // 获取客户端Ip地址
            receiveBuffer = session.receiveBuffer; // 获取客户端接收缓冲区
            ClientSessions.AddClientSession(session); // 添加客户端会话
        }

        /// <summary>
        /// 获取客户端会话
        /// </summary>
        /// <param name="dictionary"></param>
        /// <param name="session"></param>
        public static ClientSession GetClientSession(this Dictionary<int, ClientSession> dictionary,
            ClientSession session)
        {
            lock (dictionary)
            {
                // if (!dictionary.ContainsKey(session.socket.GetHashCode())) return null;
                dictionary.TryGetValue(session.socket.GetHashCode(), out var value);
                return value;
            }
        }

        /// <summary>
        /// 检测客户端是否存在
        /// </summary>
        /// <param name="dictionary"></param>
        /// <param name="session"></param>
        /// <returns></returns>
        public static bool IsContainClient(this Dictionary<int, ClientSession> dictionary, ClientSession session)
        {
            lock (dictionary)
            {
                return dictionary.ContainsKey(session.clientHashCode);
            }
        }

        /// <summary>
        /// 检测客户端连接是否已满
        /// </summary>
        /// <returns></returns>
        public static bool IsClientFull(this Dictionary<int, ClientSession> dictionary)
        {
            lock (dictionary)
            {
                return dictionary.Count >= MaxConnections;
            }
        }

        /// <summary>
        /// 获取客户端数量
        /// </summary>
        /// <returns></returns>
        public static int GetClientCount(this Dictionary<int, ClientSession> dictionary)
        {
            lock (dictionary)
            {
                return dictionary.Count;
            }
        }

        /// <summary>
        /// 添加客户端会话
        /// </summary>
        /// <param name="dictionary"></param>
        /// <param name="session"></param>
        public static void AddClientSession(this Dictionary<int, ClientSession> dictionary, ClientSession session)
        {
            lock (dictionary)
            {
                if (dictionary.IsContainClient(session)) return;
                dictionary.Add(session.clientHashCode, session);
            }
        }

        /// <summary>
        /// 移除单个客户端会话
        /// </summary>
        /// <param name="dictionary"></param>
        /// <param name="session"></param>
        public static void RemoveClientSession(this Dictionary<int, ClientSession> dictionary, ClientSession session)
        {
            lock (dictionary)
            {
                if (!dictionary.ContainsKey(session.clientHashCode)) return;
                session.CloseConnection();
                dictionary.Remove(session.clientHashCode);
            }
        }

        /// <summary>
        /// 移除所有客户端会话
        /// </summary>
        /// <param name="dictionary"></param>
        public static void RemoveAllClientSession(this Dictionary<int, ClientSession> dictionary)
        {
            lock (dictionary)
            {
                foreach (var session in dictionary.Values)
                {
                    session.CloseConnection();
                }

                dictionary.Clear();
            }
        }
    }
}