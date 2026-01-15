namespace Net.Scripts
{
    using System;
    using System.IO;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;
    using UnityEngine;

    public class ServerManager : MonoBehaviour
    {
        private TcpListener _listener; // 用于监听客户端连接的 TcpListener 对象
        private Thread _listenThread; // 用于监听客户端连接的线程
        private bool _isListening;

        ///<summary>
        /// 创建一个新的 Server 对象，用于监听指定端口上的客户端连接
        /// </summary>
        ///<param name="port">要监听的端口号</param>
        public ServerManager(int port)
        {
            _listener = new TcpListener(IPAddress.Any, port); // 创建一个 TcpListener 对象，监听指定端口上的客户端连接
            _listenThread = new Thread(Listen); // 创建一个线程，用于监听客户端连接
            _isListening = true;
        }

        ///<summary>
        /// 启动 TcpListener 对象，开始监听客户端连接
        /// </summary>
        public void Start()
        {
            _listener.Start(); // 启动 TcpListener 对象
            _listenThread.Start(); // 启动监听线程
        }

        ///<summary>
        /// 停止 TcpListener 对象，停止监听客户端连接
        /// </summary>
        public void Stop()
        {
            _listener.Stop(); // 停止 TcpListener 对象
            _listenThread.Abort(); // 中止监听线程
        }

        ///<summary>
        /// 监听客户端连接的线程的入口点
        /// </summary>
        private void Listen()
        {
            while (_isListening)
            {
                try
                {
                    var client = _listener.AcceptTcpClient(); // 接受客户端连接
                    var clientThread = new Thread(HandleClient); // 创建一个线程，用于处理客户端连接
                    clientThread.Start(client); // 启动客户端连接处理线程
                }
                catch (Exception e)
                {
                    _isListening = false;
                    Debug.LogError(e.Message);
                    throw;
                }
            }
        }

        ///<summary>
        /// 处理客户端连接的线程的入口点
        /// </summary>
        ///<param name="obj">客户端连接对象</param>
        private void HandleClient(object obj)
        {
            var client = (TcpClient)obj; // 获取客户端连接对象
            var stream = client.GetStream(); // 获取客户端连接的网络流
            var reader = new BinaryReader(stream); // 创建一个 BinaryReader 对象，用于从网络流中读取数据
            var memStream = new MemoryStream(); // 创建一个 MemoryStream 对象，用于存储从客户端接收到的数据

            while (true)
            {
                try
                {
                    var buffer = new byte[client.ReceiveBufferSize]; // 创建一个缓冲区，用于存储从客户端接收到的数据
                    var bytesRead = stream.Read(buffer, 0, buffer.Length); // 从网络流中读取数据，并将其存储到缓冲区中

                    if (bytesRead == 0)
                    {
                        break; // 如果没有读取到数据，则退出循环
                    }

                    memStream.Write(buffer, 0, bytesRead); // 将缓冲区中的数据写入到 MemoryStream 对象中

                    memStream.Seek(0, SeekOrigin.Begin); // 将 MemoryStream 对象的当前位置重置为流的开头

                    while (RemCapacity(memStream) > 2)
                    {
                        var msgLength = reader.ReadUInt16(); // 从 MemoryStream 对象中读取一个 16 位无符号整数，表示消息的长度

                        if (RemCapacity(memStream) >= msgLength)
                        {
                            var ms = new MemoryStream(); // 创建一个新的 MemoryStream 对象，用于存储消息的内容
                            var writer = new BinaryWriter(ms); // 创建一个 BinaryWriter 对象，用于将消息的内容写入到新的 MemoryStream 对象中
                            writer.Write(reader.ReadBytes(msgLength)); // 从 MemoryStream 中读取消息内容，并写入到新的 MemoryStream 中
                            ms.Seek(0, SeekOrigin.Begin); // 将新的 MemoryStream 对象的当前位置重置为流的开头
                            OnRecMessage(ms); // 调用 OnRecMessage 方法，处理接收到的消息
                        }
                        else
                        {
                            memStream.Position -= 2; // 如果剩余的字节数不足以表示消息的长度，则将 MemoryStream 对象的当前位置回退 2 个字节
                            break; // 退出循环
                        }
                    }

                    var leftover = reader.ReadBytes((int)RemCapacity(memStream)); // 读取 MemoryStream 对象中剩余的字节数
                    memStream.SetLength(0); // 清空 MemoryStream 对象
                    memStream.Write(leftover, 0, leftover.Length); // 将剩余的字节数写入到 MemoryStream 对象中
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: " + ex.Message); // 如果发生异常，则输出错误信息
                    break; // 退出循环
                }
            }

            client.Close(); // 关闭客户端连接
        }

        ///<summary>
        /// 计算 MemoryStream 对象中剩余的字节数
        /// </summary>
        ///<param name="memStream">MemoryStream 对象</param>
        ///<returns>剩余的字节数</returns>
        private long RemCapacity(Stream memStream)
        {
            return memStream.Length - memStream.Position; // 计算 MemoryStream 对象中剩余的字节数
        }

        ///<summary>
        /// 处理接收到的消息
        /// </summary>
        ///<param name="ms">消息的内容</param>
        private void OnRecMessage(MemoryStream ms)
        {
            // 处理接收到的消息
        }
    }
}