namespace _3rdBy.ByFramework.Socket.Scripts
{
    using System;
    using System.IO;
    using System.Text;

    /// <summary>
    /// 处理字节缓冲区
    /// </summary>
    public class ByteBuffer
    {
        private MemoryStream _stream;
        private BinaryWriter _writer;
        private BinaryReader _reader;

        #region 构造函数

        /// <summary>
        /// 创建一个空的字节缓冲区，用于写入数据
        /// </summary>
        public ByteBuffer()
        {
            _stream = new MemoryStream();
            _writer = new BinaryWriter(_stream);
        }

        /// <summary>
        /// 使用给定的字节数组创建一个字节缓冲区，用于读取数据。
        /// </summary>
        /// <param name="data"></param>
        public ByteBuffer(byte[] data)
        {
            if (data != null)
            {
                _stream = new MemoryStream(data);
                _reader = new BinaryReader(_stream);
            }
            else
            {
                _stream = new MemoryStream();
                _writer = new BinaryWriter(_stream);
            }
        }

        #endregion


        #region 写入数据的方法

        /// <summary>
        /// 将一个字节写入字节缓冲区
        /// </summary>
        /// <param name="v"></param>
        public void WriteByte(byte v)
        {
            _writer.Write(v);
        }

        /// <summary>
        /// 将一个整数写入字节缓冲区
        /// </summary>
        /// <param name="v"></param>
        public void WriteInt(int v)
        {
            _writer.Write((int)v);
        }

        /// <summary>
        /// 将一个无符号短整数写入字节缓冲区
        /// </summary>
        /// <param name="v"></param>
        public void WriteShort(ushort v)
        {
            _writer.Write(v);
        }

        /// <summary>
        /// 将一个长整数写入字节缓冲区
        /// </summary>
        /// <param name="v"></param>
        public void WriteLong(long v)
        {
            _writer.Write(v);
        }

        /// <summary>
        /// 将一个浮点数写入字节缓冲区
        /// </summary>
        /// <param name="v"></param>
        public void WriteFloat(float v)
        {
            var temp = BitConverter.GetBytes(v);
            Array.Reverse(temp);
            _writer.Write(BitConverter.ToSingle(temp, 0));
        }

        /// <summary>
        /// 将一个双精度浮点数写入字节缓冲区
        /// </summary>
        /// <param name="v"></param>
        public void WriteDouble(double v)
        {
            var temp = BitConverter.GetBytes(v);
            Array.Reverse(temp);
            _writer.Write(BitConverter.ToDouble(temp, 0));
        }

        /// <summary>
        /// 将一个字符串写入字节缓冲区
        /// </summary>
        /// <param name="v"></param>
        public void WriteString(string v)
        {
            var bytes = Encoding.UTF8.GetBytes(v);
            _writer.Write((ushort)bytes.Length);
            _writer.Write(bytes);
        }

        /// <summary>
        /// 将一个字节数组写入字节缓冲区
        /// </summary>
        /// <param name="v"></param>
        public void WriteBytes(byte[] v)
        {
            // Debug.Log("Int16位字节数据长度：" + v.Length);
            // _writer.Write((ushort)v.Length);
            _writer.Write(v);
        }

        #endregion

        #region 读取数据的方法

        /// <summary>
        /// 从字节缓冲区中读取一个字节
        /// </summary>
        /// <returns></returns>
        public byte ReadByte()
        {
            return _reader.ReadByte();
        }

        /// <summary>
        /// 从字节缓冲区中读取一个整数
        /// </summary>
        /// <returns></returns>
        public int ReadInt()
        {
            return (int)_reader.ReadInt32();
        }

        /// <summary>
        /// 从字节缓冲区中读取一个无符号短整数
        /// </summary>
        /// <returns></returns>
        public ushort ReadShort()
        {
            return (ushort)_reader.ReadInt16();
        }

        /// <summary>
        /// 从字节缓冲区中读取一个长整数
        /// </summary>
        /// <returns></returns>
        public long ReadLong()
        {
            return (long)_reader.ReadInt64();
        }

        /// <summary>
        /// 从字节缓冲区中读取一个浮点数
        /// </summary>
        /// <returns></returns>
        public float ReadFloat()
        {
            var temp = BitConverter.GetBytes(_reader.ReadSingle());
            Array.Reverse(temp);
            return BitConverter.ToSingle(temp, 0);
        }

        /// <summary>
        /// 从字节缓冲区中读取一个双精度浮点数
        /// </summary>
        /// <returns></returns>
        public double ReadDouble()
        {
            var temp = BitConverter.GetBytes(_reader.ReadDouble());
            Array.Reverse(temp);
            return BitConverter.ToDouble(temp, 0);
        }

        /// <summary>
        /// 从字节缓冲区中读取一个字符串
        /// </summary>
        /// <returns></returns>
        public string ReadString()
        {
            var len = ReadShort();
            var buffer = _reader.ReadBytes(len);
            return Encoding.UTF8.GetString(buffer);
        }

        /// <summary>
        /// 从字节缓冲区中读取一个字节数组
        /// </summary>
        /// <returns></returns>
        public byte[] ReadBytes()
        {
            var len = ReadInt();
            return _reader.ReadBytes(len);
        }

        /// <summary>
        /// 从字节缓冲区中读取指定长度的字节数据
        /// </summary>
        /// <param name="len"></param>
        /// <returns></returns>
        public byte[] ReadBytes(int len)
        {
            return _reader.ReadBytes(len);
        }

        #endregion

        #region 其他方法

        /// <summary>
        /// 关闭字节缓冲区，释放资源
        /// </summary>
        public void Close()
        {
            _writer?.Close();
            _reader?.Close();
            _stream?.Close();

            _writer = null;
            _reader = null;
            _stream = null;
        }

        /// <summary>
        /// 将字节缓冲区转换为字节数组
        /// </summary>
        /// <returns></returns>
        public byte[] ToBytes()
        {
            _writer.Flush();
            return _stream.ToArray();
        }

        /// <summary>
        /// 清空字节缓冲区中的数据
        /// </summary>
        public void Flush()
        {
            _writer.Flush();
        }

        #endregion
    }
}