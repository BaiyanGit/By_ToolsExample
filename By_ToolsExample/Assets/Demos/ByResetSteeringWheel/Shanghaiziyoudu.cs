/*
 * 脚本名称：Shanghaiziyoudu.cs
 * 修改功能：上海自由度通信功能类
 *     我司引进上海自由度平台厂家带自动回中功能的方向机，此方向机与原自由度平台使用同一端口通信，
 *     （硬件上级联到同一控制板卡），为兼容方向机与平台功能，开发此功能类
 *     注意：此功能类适用于与平台联合使用的方向机，单独使用的方向机通信协议可能会发生改变
 * 修 改 者：陆巍
 * 修改时间：2024.4.1
 *
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using UnityEngine;
using System.Linq;
using UnityEngine.Serialization;

public class Shanghaiziyoudu : MonoBehaviour
{
    /// <summary>
    /// 自由度的ip
    /// </summary>
    public string ziyouduIp;

    /// <summary>
    /// 自由度的端口
    /// </summary>
    public int ziyouduPort;

    /// <summary>
    /// UDP发送客户端
    /// </summary>
    private UdpClient udpClient;

    /// <summary>
    /// 连接上了自由度
    /// </summary>
    public bool _islianjie;

    public float _accelerationX;
    public float _accelerationY;
    public float _accelerationZ;
    public float _angleX;
    public float _angleZ;
    public float _angleY;

    /// <summary>
    /// 进入课题后当前要与自由度同步的对象
    /// </summary>
    public Transform currentTarget;

    public Transform orgianTarget;
    private Thread _dataRequestThread;
    public static Shanghaiziyoudu Instance;

    //进入课题后循环发送数据
    private bool xunhuanSend;
    public float _var3;
    public float _dianhuoValue;
    public float _zhuangjiValue;
    public float _idlingValue; //怠速值

    // 方向机命令队列
    public bool _UseSteeringWheel = false;
    private Queue<SendMsg_SteeringWheel> cmdSteeringWheelQueue = new Queue<SendMsg_SteeringWheel>();
    Thread receiveThread;
    private UdpClient udpClient_Recv;
    private List<byte> recvDataList = new List<byte>();
    RecMsg_SteeringWheel coderValue = new RecMsg_SteeringWheel();

    private void Awake()
    {
        Instance = this;
        LoadSteeringFile();
        InitConfigInfo();

        orgianTarget = transform;
        currentTarget = orgianTarget;
    }

    /// <summary>
    /// 基础信息配置
    /// </summary>
    void InitConfigInfo()
    {
        string path;
        string peizhi;
        path = Application.dataPath + "/StreamingAssets/Config/ZiyouduPeizhi.txt";
        if (File.Exists(@path))
        {
            peizhi = File.ReadAllText(@path);
            string[] info = peizhi.Split('|');
            ziyouduIp = info[1];
            ziyouduPort = int.Parse(info[3]);
            _dianhuoValue = float.Parse(info[5]);
            _zhuangjiValue = float.Parse(info[7]);
            _idlingValue = float.Parse(info[9]);
        }
    }


    public float maxSteering = 17.5f;
    public float minSteering = -14f;

    /// <summary>
    /// 加载方向盘文件
    /// </summary>
    private void LoadSteeringFile()
    {
        var path = Application.dataPath + "/StreamingAssets/Config/Steering.txt";

        if (!File.Exists(path)) return;
        var strKeys = File.ReadAllText(path);
        var info = strKeys.Split('|');
        maxSteering = float.Parse(info[0]);
        minSteering = float.Parse(info[1]);
    }

    private void Start()
    {
        //建立UDP通信
        try
        {
            udpClient = new UdpClient();
            udpClient.Connect(ziyouduIp, ziyouduPort);

            System.Net.IPEndPoint local = new System.Net.IPEndPoint(System.Net.IPAddress.Any, 8410);
            udpClient_Recv = new UdpClient(local);

            _islianjie = true;

            //ChushihuaSend();

            BackChushiSend();

            //建立发送线程
            _dataRequestThread = new Thread(new ThreadStart(DataRequestFunction));
            _dataRequestThread.Start();

            // 建立接收线程
            receiveThread = new Thread(new ThreadStart(DataReceiveFunction));
            receiveThread.Start();

            // 暂时初始加一次，如不合适可循环发送
            SetSteeringWheel();
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
        }
    }

    /// <summary>
    /// 数据接收方法，主要处理返回的编码器数据
    /// </summary>
    private void DataReceiveFunction()
    {
        // 自由度和方向机的端口和ip
        System.Net.IPEndPoint remote = new System.Net.IPEndPoint(System.Net.IPAddress.Parse(ziyouduIp), ziyouduPort);
        while (_islianjie)
        {
            try
            {
                byte[] receBytes = udpClient_Recv.Receive(ref remote);
                recvDataList.AddRange(receBytes);

                while (recvDataList.Count > 16)
                {
                    byte[] data = recvDataList.Take(16).ToArray();
                    coderValue = (RecMsg_SteeringWheel)Converter.BytesToStruct(data, typeof(RecMsg_SteeringWheel));
                    recvDataList.RemoveRange(0, 16);
                }
            }

            catch (System.Exception ex)
            {
                Debug.Log("数据接收错误: " + ex.Message);
            }
        }
    }

    private void SetSteeringWheel()
    {
        if (_islianjie)
        {
            // 发送方向机参数配置 以下参数为测试值，后期可放在配置表里
            SendMsg_SteeringWheel msg = new SendMsg_SteeringWheel();
            msg.start1 = 0x55;
            msg.start2 = 0xAA;
            msg.start3 = 0x90;
            msg.cmd = 0x02;
            msg.maxTouch = 219;
            msg.vel = 420;
            msg.pos = 0;
            msg.MaxPos = maxSteering;
            msg.MinPos = minSteering;
            cmdSteeringWheelQueue.Enqueue(msg);
        }
    }

    public void DianhuozhendongSend()
    {
        _var3 = _dianhuoValue;
        isIdling = false;
    }

    private bool isIdling;

    /// <summary>
    /// 怠速震动
    /// </summary>
    public void IdlingSend()
    {
        _var3 = _idlingValue;
        isIdling = true;
    }

    public void ZhuangjizhendongSend()
    {
        _var3 = _zhuangjiValue;
        isIdling = false;
    }

    /// <summary>
    /// 发送请求数据报文  新版单片机需要请求才发送数据
    /// </summary>
    private void DataRequestFunction()
    {
        while (_islianjie)
        {
            sendmsg.start1 = 0x55;
            sendmsg.start2 = 0xaa;
            sendmsg.start3 = 0xbb;

            if (_var3 != 0)
            {
                sendmsg.cmd = 0x10; //  1为初始化， 2为急停  3为回初始位置  4为降到最底部 6为循环运行数据  0x10震动标志位
                sendmsg.time = 2;
            }
            else
            {
                sendmsg.cmd = 0x06; //  1为初始化， 2为急停  3为回初始位置  4为降到最底部 6为循环运行数据
                sendmsg.time = 100;
            }

            sendmsg.tx = _accelerationX;
            sendmsg.ty = _accelerationZ;
            sendmsg.tz = _accelerationY;
            sendmsg.rx = -_angleZ / 57.29578049044297f;
            sendmsg.ry = _angleX / 57.29578049044297f;
            sendmsg.rz = (_angleY / 57.29578049044297f) * 2;
            sendmsg.var3 = _var3;

            byte[] sendBytes = Converter.StructToBytes(sendmsg);

            if (xunhuanSend)
            {
                udpClient.Send(sendBytes, sendBytes.Length);
            }

            // 发送方向机的命令
            while (cmdSteeringWheelQueue.Count > 0)
            {
                var cmd = cmdSteeringWheelQueue.Dequeue();
                byte[] cmdByte = Converter.StructToBytes(cmd);
                udpClient.Send(cmdByte, cmdByte.Length);
                //Debug.Log("配置方向机力矩等参数成功！限制速度"+cmd.vel+" 限制扭矩 "+cmd.maxTouch+ " 限制位置 "+cmd.pos +" 最大位置 "+cmd.MaxPos +" 最小位置 "+cmd.MinPos);
            }

            if (_var3 != 0)
            {
                // Debug.Log("抖动" + _var3);
                // _var3 = isIdling ? _var3 : 0;
                _var3 = 0;
                Thread.Sleep(1000);
            }
            else
            {
                Thread.Sleep(10);
            }
        }
    }

    /// <summary>
    /// 编码器位置
    /// </summary>
    /// <returns></returns>
    public float GetCoderPosition()
    {
        return coderValue.positionActualValue;
    }

    /// <summary>
    /// 编码器力
    /// </summary>
    /// <returns></returns>
    public int GetCoderTorque()
    {
        return coderValue.torqueActualValue;
    }

    private void Update()
    {
        UpdatePosState();
        // Debug.Log("方向机位置为： " + coderValue.positionActualValue.ToString("n2") + " 力度： " +
        // coderValue.torqueActualValue.ToString("n2"));
    }

    private float last_accelerationX;
    private float last_accelerationY;
    private float last_accelerationZ;

    private void UpdatePosState()
    {
        if (FollowPosRot.Instance != null)
        {
            _accelerationX = -(last_accelerationX - FollowPosRot.Instance._accelerationX);

            _accelerationY = -(last_accelerationY - FollowPosRot.Instance._accelerationY);

            _accelerationZ = -(last_accelerationZ - FollowPosRot.Instance._accelerationZ);


            _accelerationX = Mathf.Clamp(_accelerationX, -0.1f, 0.1f);
            _accelerationY = Mathf.Clamp(_accelerationY, -0.1f, 0.1f);
            _accelerationZ = Mathf.Clamp(_accelerationZ, -0.1f, 0.1f);


            last_accelerationX = FollowPosRot.Instance._accelerationX;

            last_accelerationY = FollowPosRot.Instance._accelerationY;

            last_accelerationZ = FollowPosRot.Instance._accelerationZ;


            _angleY = FollowPosRot.Instance.offsetY;

            if (_angleY < -180)
            {
                _angleY = 360 + _angleY;
            }


            //if (Mathf.Abs(_accelerationZ) > 0.001f)
            //{
            //    Debug.Log(_accelerationZ*2);
            //}
        }
        else
        {
            _accelerationX = 0;

            _accelerationY = 0;

            _accelerationZ = 0;
        }

        if (currentTarget == null)
        {
            // Debug.LogError("当前目标为空");
            xunhuanSend = false;
            return;
        }

        if (currentTarget.eulerAngles.x > 180)
        {
            _angleX = 360 - currentTarget.eulerAngles.x;
        }
        else
        {
            _angleX = currentTarget.eulerAngles.x;
            _angleX = -_angleX;
        }

        _angleX = Mathf.Clamp(_angleX, -5, 5);

        if (currentTarget.eulerAngles.z > 180)
        {
            _angleZ = currentTarget.eulerAngles.z - 360;
        }
        else
        {
            _angleZ = currentTarget.eulerAngles.z;
        }

        _angleZ = Mathf.Clamp(_angleZ, -5, 5);


        if (currentTarget == orgianTarget)
        {
            if (xunhuanSend)
            {
                BackChushiSend();
            }
        }
        else
        {
            xunhuanSend = true;
        }
    }

    public void ChushihuaSend()
    {
        xunhuanSend = false;

        sendmsg.start1 = 0x55;
        sendmsg.start2 = 0xaa;
        sendmsg.start3 = 0xbb;
        sendmsg.cmd = 0x01; //  1为初始化， 2为急停  3为回初始位置  4为降到最底部 6为循环运行数据
        sendmsg.rx = 0;
        sendmsg.ry = 0;
        sendmsg.rz = 0;
        sendmsg.tx = 0;
        sendmsg.ty = 0;
        sendmsg.tz = 0;

        sendmsg.var3 = 0;

        sendmsg.time = 1000;

        byte[] sendBytes = Converter.StructToBytes(sendmsg);

        udpClient.Send(sendBytes, sendBytes.Length);

        currentTarget.position = orgianTarget.position;

        currentTarget.eulerAngles = orgianTarget.eulerAngles;
    }

    public void BackChushiSend()
    {
        xunhuanSend = false;

        sendmsg.start1 = 0x55;
        sendmsg.start2 = 0xaa;
        sendmsg.start3 = 0xbb;
        sendmsg.cmd = 0x03; //  1为初始化， 2为急停  3为回初始位置  4为降到最底部 6为循环运行数据
        sendmsg.rx = 0;
        sendmsg.ry = 0;
        sendmsg.rz = 0;
        sendmsg.tx = 0;
        sendmsg.ty = 0;
        sendmsg.tz = 0;

        sendmsg.var3 = 0;

        sendmsg.time = 1000;

        byte[] sendBytes = Converter.StructToBytes(sendmsg);

        udpClient.Send(sendBytes, sendBytes.Length);

        currentTarget.position = orgianTarget.position;

        currentTarget.eulerAngles = orgianTarget.eulerAngles;
    }

    private void OnDestroy()
    {
        _islianjie = false;

        xunhuanSend = false;

        sendmsg.start1 = 0x55;
        sendmsg.start2 = 0xaa;
        sendmsg.start3 = 0xbb;
        sendmsg.cmd = 0x04; //  1为初始化， 2为急停  3为回初始位置  4为降到最底部 6为循环运行数据
        sendmsg.rx = 0;
        sendmsg.ry = 0;
        sendmsg.rz = 0;
        sendmsg.tx = 0;
        sendmsg.ty = 0;
        sendmsg.tz = 0;

        sendmsg.var3 = 0;

        sendmsg.time = 1000;

        byte[] sendBytes = Converter.StructToBytes(sendmsg);

        udpClient.Send(sendBytes, sendBytes.Length);
    }

    struct RecMsg
    {
        public byte start1;
        public byte start2;
        public byte start3;
        public byte cmd;
        public float tx;
        public float ty;
        public float tz;

        public float rx;
        public float ry;
        public float rz;

        public float var1;
        public float var2;
        public float var3;

        public float var4;
        public float var5;
        public float var6;

        public int time;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SendMsg_SteeringWheel
    {
        public byte start1; // 起始标志位1  为 0x55
        public byte start2; // 起始标志位2  为 0xaa
        public byte start3; // 起始标志位3  为 0x90
        public byte cmd; // 1 清零 2 回中
        public int maxTouch; // 限制扭矩
        public int vel; // 限制速度
        public float pos; // 限制位置
        public float MaxPos; // 最大位置
        public float MinPos; // 最小位置
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct RecMsg_SteeringWheel
    {
        public byte start1; // 起始标志位1  为 0xaa
        public byte start2; // 起始标志位1  为 0xbb
        public byte start3; // 起始标志位1  为 0xcc
        public byte start4; // 起始标志位1  为 0xcc
        public short StatusWord; // 错误码
        public short ControlWord;
        public float positionActualValue; // 位置
        public short torqueActualValue; // 力
    };

    RecMsg sendmsg = new RecMsg();
}

public class Converter
{
    //Structure转为Byte数组，实现了序列化
    public static Byte[] StructToBytes(System.Object structure)
    {
        Int32 size = Marshal.SizeOf(structure);
        Console.WriteLine(size);
        IntPtr buffer = Marshal.AllocHGlobal(size);
        try
        {
            Marshal.StructureToPtr(structure, buffer, false);
            Byte[] bytes = new Byte[size];
            Marshal.Copy(buffer, bytes, 0, size);
            return bytes;
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }

    //Byte数组转为Structure，实现了反序列化
    public static System.Object BytesToStruct(Byte[] bytes, Type strcutType)
    {
        Int32 size = Marshal.SizeOf(strcutType);
        IntPtr buffer = Marshal.AllocHGlobal(size);
        try
        {
            Marshal.Copy(bytes, 0, buffer, size);
            return Marshal.PtrToStructure(buffer, strcutType);
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }
}