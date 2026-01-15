namespace _3rdBy.ByFunc.血氧仪示例.Scripts
{
    using System;
    using System.Collections.Generic;
    using System.IO.Ports;
    using System.Threading;
    using UnityEngine;

    public partial class SerialDevice
    {
        /// <summary>
        /// 连接串口类
        /// </summary>
        private SerialPort _serialPortDevice;

        /// <summary>
        /// 接收数据线程
        /// </summary>
        private Thread _recThread;


        /// <summary>
        /// 表示包信息,包含包头、长度、ID、各种数据
        /// </summary>
        private Queue<byte[]> _packedQueue = new();
    }

    /// <summary>
    /// 串口设备连接、接收数据、读取数据
    /// </summary>
    public partial class SerialDevice
    {
        /// <summary>
        /// 连接串口
        /// </summary>
        /// <param name="comId">串口ID</param>
        public void ConnectCom(string comId)
        {
            _serialPortDevice = new SerialPort(comId, 115200, Parity.None, 8, StopBits.One);

            try
            {
                _serialPortDevice.Open();
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
                throw;
            }

            ThreadStart tStart = ReceiveData;
            _recThread = new Thread(tStart);
            _recThread.Start();
        }

        /// <summary>
        /// 停止连接
        /// </summary>
        public void StopConnect()
        {
            _serialPortDevice?.Close();
            _serialPortDevice = null;

            _recThread?.Abort();
            _recThread = null;
        }


        /// <summary>
        /// 接收数据
        /// </summary>
        private void ReceiveData()
        {
            while (true)
            {
                if (_serialPortDevice == null || _serialPortDevice.IsOpen == false)
                    return;

                try
                {
                    //获取包的数据
                    var packedBuffer = new byte[256];
                    _serialPortDevice.Read(packedBuffer, 0, 256);

                    // Debug.LogError($"十进制转16进制：包头：{packedBuffer[0]:X2}，byte长度：{packedBuffer[1]}，包ID：{packedBuffer[2]:X2}");

                    switch (packedBuffer[2])
                    {
                        //数据
                        case ProtocolId.Ox80Signal when packedBuffer[1] == 6:
                            //50hz速度传一次
                            // DebugOutput.Ins.OutputReceiveData0X80(packedBuffer);
                            _packedQueue.Enqueue(packedBuffer);
                            break;
                        case ProtocolId.Ox81Signal when packedBuffer[1] == 11:
                            //1hz速度传一次
                            //DebugOutput.Ins.OutputReceiveData0X81(packedBuffer);
                            _packedQueue.Enqueue(packedBuffer);
                            break;
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError(e.Message);
                    throw;
                }

                // -1Hz - 一次每秒,用于低速传感器
                // - 10Hz - 10次每秒,用于语音传输
                // - 25Hz - 25次每秒,用于视频传输
                // - 1000Hz(1kHz) - 1000次每秒,用于高速数据采集
                Thread.Sleep(20); //20为毫秒
            }
        }

        /// <summary>
        /// 读取包体数据
        /// </summary>
        public void ReadPacketData()
        {
            if (_packedQueue.Count == 0) return;

            var packedBuffer = _packedQueue.Dequeue();
            switch (packedBuffer[2])
            {
                //0x80数据
                case ProtocolId.Ox80Signal when packedBuffer[1] == 6:
                    Ox80Packet ox80Packet = new();
                    var        waveByte   = packedBuffer[3];                                                  //描记波
                    ox80Packet.waveData = Mathf.Clamp(Mathf.RoundToInt(waveByte / 255f * 100 - 50), -50, 50); //计算在-50至50之间
                    // Debug.Log("描记波：" + ox80Packet.waveData);

                    ox80Packet.pulseSound = packedBuffer[4] >> 7 == 1; //脉搏音
                    ox80Packet.barValue   = packedBuffer[4] & 0x0F;    //棒图
                    //Debug.Log($"脉搏音：{ox80Packet.pulseSound}，棒图：{ox80Packet.barValue}");
                    DebugOutput.Ins.Broadcast0X80(ox80Packet);
                    break;
                //0x81数据
                case ProtocolId.Ox81Signal when packedBuffer[1] == 11:
                    Ox81Packet ox81Packet = new();
                    ox81Packet.spo2       = packedBuffer[3];                                       //血氧
                    ox81Packet.pulseRate  = (short)(packedBuffer[4] | packedBuffer[5] << 8);       //脉率
                    ox81Packet.breathFreq = packedBuffer[6];                                       //呼吸率
                    ox81Packet.piOrigin   = (short)(packedBuffer[7] + packedBuffer[8] << 8);       //灌注指数,放大1000倍的值
                    ox81Packet.pi         = Mathf.Clamp(ox81Packet.piOrigin / 1000, 0.02f, 20.0f); //缩小1000倍，限制在0.02-20之间
                    ox81Packet.alarms     = packedBuffer[9];                                       //报警信息

                    DebugOutput.Ins.Broadcast0X81(ox81Packet);
                    break;
            }
        }
    }
}