namespace Net
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Net.Sockets;
    using UnityEngine;

    public enum DisType
    {
        /// <summary>
        /// 异常断开
        /// </summary>
        Exception,

        /// <summary>
        /// 正常断开
        /// </summary>
        Disconnect,

        /// <summary>
        /// 数据包异常断开
        /// </summary>
        PackageException,
    }

    /// <summary>
    /// 会话客户端
    /// </summary>
    public class ClientSession
    {
        /// <summary>
        /// 用于连接服务器
        /// </summary>
        private TcpClient _client;

        /// <summary>
        /// 用于发送数据的 NetworkStream 网络流（在客户端连接到服务器时创建）
        /// </summary>
        private NetworkStream _outStream;

        /// <summary>
        /// 用于接收数据的 MemoryStream 数据流（存储解析后的数据）
        /// </summary>
        private MemoryStream _memStream;

        /// <summary>
        /// 用于读取接收数据_memStream 的 BinaryReader（读取十六进制数据）
        /// </summary>
        private BinaryReader _binaryReader;

        /// <summary>
        /// 字节缓冲区大小
        /// </summary>
        private const int BufferSize = 8192;

        /// <summary>
        /// 用于接收数据的字节数组（字节缓冲区）
        /// </summary>
        private readonly byte[] _byteBuffer = new byte[BufferSize];


        /// <summary>
        /// 注册客户端会话
        /// </summary>
        public void OnRegister()
        {
            _memStream = new MemoryStream();
            _binaryReader = new BinaryReader(_memStream);
        }

        /// <summary>
        /// 网络连接状态
        /// </summary>
        /// <returns>true:连接，false:断开</returns>
        public bool ConnectedState()
        {
            return _client != null && _client.Connected; //判断是否连接
        }

        #region 服务的创建与关闭

        /// <summary>
        /// 发送连接请求
        /// </summary>
        public void SendConnect()
        {
            ConnectServer(AppConst.address, AppConst.port);
        }

        /// <summary>
        /// 连接服务器
        /// </summary>
        private void ConnectServer(string ip, int port)
        {
            _client = null;
            _client = new TcpClient();
            _client.SendTimeout = 1000; //发送数据的超时时间/毫秒
            _client.ReceiveTimeout = 1000; //接收数据的超时时间/毫秒
            _client.NoDelay = true; //启用减少网络中的小数据包数量来提高网络的整体效率。（即立即发送数据包，而不是等待更多数据到达后再发送）
            try
            {
                _client.BeginConnect(ip, port, OnConnectServer, null);
            }
            catch (Exception e)
            {
                CloseConnect();
                Debug.LogError(e.Message);
            }
        }

        /// <summary>
        /// 连接服务器成功后的回调
        /// </summary>
        private void OnConnectServer(IAsyncResult aRes)
        {
            _outStream = _client.GetStream();
            _client.GetStream().BeginRead(_byteBuffer, 0, BufferSize, OnReadData, null);
            // _client.GetStream().BeginRead(_byteBuffer, 0, BufferSize, OnReadDataStickyPackageHandle, null);//粘包处理

            Debug.Log("已成功连接到服务器!!!");
        }

        /// <summary>
        /// 关闭网络连接与释放二进制读取器和内存流
        /// </summary>
        public void OnRemove()
        {
            // 关闭网络连接
            CloseConnect();

            // 关闭 BinaryReader 对象并释放资源
            if (_binaryReader != null)
            {
                _binaryReader.Close();
                _binaryReader.Dispose();
                _binaryReader = null;
            }

            // 关闭 MemoryStream 对象并释放资源
            if (_memStream != null)
            {
                _memStream.Close();
                _memStream.Dispose();
                _memStream = null;
            }
        }

        /// <summary>
        /// 异常断开连接
        /// </summary>
        /// <param name="disType"></param>
        /// <param name="msg"></param>
        private void OnDisConnect(DisType disType, string msg)
        {
            Debug.Log($"与服务断开连接: {disType}.  Msg: {msg}");
            CloseConnect();
        }

        /// <summary>
        /// 关闭连接
        /// </summary>
        private void CloseConnect()
        {
            if (_client == null) return;
            if (_client.Connected)
                _client.Close();
            _client = null;
        }

        #endregion

        #region 接收消息处理流程

        /// <summary>
        /// 读取服务器下发数据的回调
        /// </summary>
        /// <param name="aRes"></param>
        private void OnReadData(IAsyncResult aRes)
        {
            try
            {
                int length;
                lock (_client.GetStream())
                {
                    length = _client.GetStream().EndRead(aRes); //获取包的长度
                }

                if (length < 1)
                {
                    OnDisConnect(DisType.PackageException, "bytesRead < 1"); //包尺寸有问题，断线处理
                    return;
                }

                OnAnalysisData(_byteBuffer, length); //解析数据包内容，抛给逻辑层

                lock (_client.GetStream()) //等待解析结束
                {
                    Array.Clear(_byteBuffer, 0, _byteBuffer.Length); //清空数组
                    _client.GetStream().BeginRead(_byteBuffer, 0, BufferSize, OnReadData, null); //再次监听服务器发送过来的消息
                }
            }
            catch (Exception e)
            {
                OnDisConnect(DisType.Exception, e.Message);
            }
        }

        /// <summary>
        /// 分析接收到的数据
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="length"></param>
        private void OnAnalysisData(byte[] bytes, int length)
        {
            _memStream.Seek(0, SeekOrigin.End); //写入位置被设置为末尾
            _memStream.Write(bytes, 0, length); //从bytes数组的第一个元素写入数据
            _memStream.Seek(0, SeekOrigin.Begin); //将内存流的当前位置重置为流的开头，这样后续的读取操作将从流的开头开始
            while (RemCapacity() > 2)
            {
                var msgLenght = _binaryReader.ReadUInt16(); //从_memStream流中读取一个 16 位无符号整数
                if (RemCapacity() >= msgLenght)
                {
                    using var ms = new MemoryStream();
                    var bw = new BinaryWriter(ms);
                    bw.Write(_binaryReader.ReadBytes(msgLenght));
                    ms.Seek(0, SeekOrigin.Begin);
                    OnRecMessage(ms);
                }
                else
                {
                    _memStream.Position -= 2;
                    break;
                }
            }

            var leftover = _binaryReader.ReadBytes((int)RemCapacity());
            _memStream.SetLength(0);
            _memStream.Write(leftover, 0, leftover.Length);
        }

        /// <summary>
        /// 处理接收到的消息
        /// </summary>
        /// <param name="ms"></param>
        private static void OnRecMessage(Stream ms)
        {
            var br = new BinaryReader(ms);
            var msgBytes = br.ReadBytes((int)(ms.Length - ms.Position));
            var buffer = new ByteBuffer(msgBytes);
            int mainId = buffer.ReadShort();
            var pbDataLen = msgBytes.Length - 2;
            var pbData = buffer.ReadBytes(pbDataLen);
            ClientManager.Instance.DispatchProto(mainId, pbData); //广播消息
        }

        /// <summary>
        /// 计算剩余容量
        /// </summary>
        /// <returns>返回内存流中剩余的字节数</returns>
        private long RemCapacity()
        {
            var len = _memStream.Length - _memStream.Position;
            return len;
        }

        #endregion


        #region 发送消息处理流程

        /// <summary>
        /// 发送消息
        /// </summary>
        public void SendMsg(ByteBuffer buffer)
        {
            CreatSession(buffer.ToBytes());
            buffer.Close();
        }

        /// <summary>
        /// 创建会话
        /// </summary>
        private void CreatSession(byte[] bytes)
        {
            WriteMessage(bytes);
        }

        /// <summary>
        /// 写入消息
        /// </summary>
        private void WriteMessage(byte[] message)
        {
            MemoryStream ms;
            using (ms = new MemoryStream())
            {
                var br = new BinaryWriter(ms); //将消息转换为字节流
                br.Write(message); //数据以二进制格式写入到数据流中
                br.Flush(); //确保所有缓冲区数据都被写入到内存流中

                if (_client == null || _client.Connected == false)
                {
                    Debug.LogError("连接服务失败？或未链接服务端?");
                    return;
                }

                var payload = ms.ToArray();
                //为了不阻塞主线程，写入操作将在后台进行，采用异步将数据写入到网络中。
                _outStream.BeginWrite(payload, 0, payload.Length, OnWrite, null);
            }
        }

        /// <summary>
        /// 写入消息后的回调
        /// </summary>
        private void OnWrite(IAsyncResult r)
        {
            try
            {
                _outStream.EndWrite(r); //异步写入操作完成时消息发送给了服务端                
            }
            catch (Exception ex)
            {
                Debug.LogError($"数据写入异常 : {ex.Message}");
            }
        }

        #endregion
    }
}