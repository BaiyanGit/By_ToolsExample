namespace Network.Client
{
    /// <summary>
    /// 字节缓冲区
    /// </summary>
    public class ReceiveBuffer
    {
        /// <summary>
        /// 默认缓冲区大小
        /// </summary>
        private const int DefaultSize = 1024;

        /// <summary>
        /// 字节缓冲区
        /// </summary>
        public byte[] byteBuffer;

        /// <summary>
        /// 读取字节的位置
        /// </summary>
        public int readIndex;

        /// <summary>
        /// 写入字节的位置
        /// </summary>
        public int writeIndex;

        /// <summary>
        /// 写入位置 - 读取位置 = 字节长度
        /// </summary>
        public int Length => writeIndex - readIndex;

        /// <summary>
        /// 字节缓冲区初始容量
        /// </summary>
        public int initBufSize;

        /// <summary>
        /// 字节缓冲区容量
        /// </summary>
        public int bufCapacity;

        /// <summary>
        /// 字节剩下的空间大小 = 当前字节缓冲区的大小 - 用过字节缓冲区的大小
        /// </summary>
        public int RemainSize => bufCapacity - writeIndex;


        /// <summary>
        /// 设置字节缓冲区大小
        /// </summary>
        /// <param name="byteLength"></param>
        public ReceiveBuffer(int byteLength = DefaultSize)
        {
            byteBuffer = new byte[byteLength];
            initBufSize = byteLength;
            bufCapacity = byteLength;
            writeIndex = 0;
            readIndex = 0;
        }
    }
}