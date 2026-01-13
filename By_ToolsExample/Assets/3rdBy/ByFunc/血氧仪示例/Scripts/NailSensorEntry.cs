using UnityEngine;

/// <summary>
/// 调用血氧仪入口
/// </summary>
public class NailSensorEntry : MonoBehaviour
{
    private SerialDevice _serialDevice;

    private void OnEnable()
    {
        //连接指甲血氧仪
        _serialDevice = new SerialDevice();
        _serialDevice.ConnectCom("COM4");
    }

    private void OnDisable()
    {
        //停止指甲血氧仪
        _serialDevice.StopConnect();
    }

    private void Update()
    {
        //读取指甲血氧仪数据
        _serialDevice?.ReadPacketData();
    }
}