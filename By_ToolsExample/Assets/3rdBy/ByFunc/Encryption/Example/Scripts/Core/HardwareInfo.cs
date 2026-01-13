using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 电脑硬件帮助类
/// </summary>
public abstract class HardwareInfo
{
    private static string DeviceName => SystemInfo.deviceName; // 设备名称
    private static string DeviceModel => SystemInfo.deviceModel; // 电脑型号
    private static string DeviceUniqueIdentifier => SystemInfo.deviceUniqueIdentifier; // 设备唯一标识符
    private static int GraphicsDeviceID => SystemInfo.graphicsDeviceID; // 显卡的设备ID

    /// <summary>
    /// 获取计算机硬件信息
    /// </summary>
    /// <returns></returns>
    public static ComputerInfo GetComputerComponents()
    {
        var computerInfo = new ComputerInfo
        {
            deviceName = DeviceName,
            deviceModel = DeviceModel,
            deviceUniqueIdentifier = DeviceUniqueIdentifier,
            graphicsDeviceID = GraphicsDeviceID
        };
        return computerInfo;
    }
}

/// <summary>
/// 电脑硬件信息
/// </summary>
[Serializable]
public class ComputerInfo
{
    public string deviceName; // 设备名称
    public string deviceModel; // 电脑型号
    public string deviceUniqueIdentifier; // 设备唯一标识符
    public int graphicsDeviceID; // 显卡的设备ID
    public SoftwareInfo softwareInfo; // 软件信息
}

/// <summary>
/// 软件信息
/// </summary>
[Serializable]
public class SoftwareInfo
{
    public string softwareName; // 软件名称
    public string activationTime; // 激活时间
    public string openTime; // 软件打开的时间
    public int limitType; // 限制类型 0=时间限制、1=次数限制、2=永久
    public string lifeDate; // 使用期限
    public int useCount; // 使用次数
    public bool isVrSupported; // 是否支持VR
    public int engineerType; // 工程车辆类型 0=挖掘机、1=****
}

/// <summary>
/// 密钥类，用来存储使用过的激活密钥
/// </summary>
[Serializable]
public class SecretKeyData
{
    public List<string> key; // 密钥列表
}