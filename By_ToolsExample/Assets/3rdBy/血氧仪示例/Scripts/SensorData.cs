using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 0x80数据包结构
/// </summary>
public struct Ox80Packet
{
    /// <summary>
    /// 描记波
    /// </summary>
    public int waveData;

    /// <summary>
    /// 脉搏音
    /// </summary>
    public bool pulseSound;

    /// <summary>
    /// 棒图
    /// </summary>
    /// <returns></returns>
    public int barValue;
}


/// <summary>
/// 0x81数据包结构
/// </summary>
public struct Ox81Packet
{
    /// <summary>
    /// 血氧饱和度,范围 1-100
    /// </summary>
    public int spo2;

    /// <summary>
    /// 脉率,范围 25-300
    /// </summary>
    public int pulseRate;

    /// <summary>
    /// 呼吸率,范围 4-70
    /// </summary>
    public int breathFreq;

    /// <summary>
    /// 灌注指数,显示范围 20 - 20000
    /// </summary>
    public float piOrigin;

    /// <summary>
    /// 灌注指数,映射范围 0.02 - 20.0
    /// </summary>
    public float pi;

    /// <summary>
    /// 报警状态（每个 bit 位分别代表一种报警类型，无符号数）
    /// 0:探头脱落 1:探头未接 2:探头故障 3:低灌注水平 4:搜索脉搏
    /// </summary>
    public byte alarms;
}

/// <summary>
/// 消息号
/// </summary>
public static class ProtocolId
{
    public const int Ox80Signal = 0x80; //描波形包,0x表示十六进制
    public const int Ox81Signal = 0x81; //参数包,0x表示十六进制
}

public class DebugOutput
{
    private static readonly object LockObj = new object();
    private static DebugOutput _ins;

    public static DebugOutput Ins
    {
        get
        {
            if (_ins != null) return _ins;
            lock (LockObj)
            {
                _ins ??= new DebugOutput();
            }

            return _ins;
        }
    }

    /// <summary>
    /// 打印接收FA81消息
    /// </summary>
    /// <param name="bytes"></param>
    public void OutputReceiveData0X81(IReadOnlyList<byte> bytes)
    {
        var spo2 = bytes[3]; //血氧
        var pulseRate = (short)(bytes[4] | bytes[5] << 8); //脉率
        var breathFreq = bytes[6]; //呼吸率
        var pi = (short)(bytes[7] | bytes[8] << 8); //灌注指数
        var alarms = bytes[9]; //报警状态

        Debug.Log($"血氧:{spo2}," +
                  $"脉率：{pulseRate}，" +
                  $"呼吸率：{breathFreq}，" +
                  $"灌注指数：{pi}，" +
                  $"报警状态：{alarms}");
    }

    /// <summary>
    /// 打印接收FA80消息
    /// </summary>
    /// <param name="bytes"></param>
    public void OutputReceiveData0X80(IReadOnlyList<byte> bytes)
    {
        Debug.Log($"描记波:{bytes[3]},脉搏音和棒:{bytes[4]}");
    }

    /// <summary>
    /// 广播0x80数据
    /// </summary>
    public void Broadcast0X80(Ox80Packet ox80Packet)
    {
        var wd = ox80Packet.waveData; //描记波
        var bv = ox80Packet.barValue; //棒图
        var ps = ox80Packet.pulseSound; //脉搏音
        Debug.Log($"描记波：{wd}，棒图：{bv}，脉搏音：{ps}");
    }

    /// <summary>
    /// 广播0x81数据
    /// </summary>
    public void Broadcast0X81(Ox81Packet ox81Packet)
    {
        var sp = ox81Packet.spo2; //血氧饱和度
        var pr = ox81Packet.pulseRate; //脉率
        var pf = ox81Packet.breathFreq; //呼吸率
        var po = ox81Packet.piOrigin; //灌注显示范围
        var pi = ox81Packet.pi; //灌注映射范围
        var al = ox81Packet.alarms; //报警状态
        Debug.Log($"血氧饱和度：{sp}，脉率：{pr}，呼吸率：{pf}，灌注显示范围：{po}，灌注映射范围：{pi}，报警状态：{al}");
    }
}