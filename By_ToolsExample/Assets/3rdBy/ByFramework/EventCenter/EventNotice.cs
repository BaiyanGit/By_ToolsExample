using UnityEngine;

public enum EventNotice
{
    [InspectorName("心跳消息Id")] HeartBeat = 0x0001,
    [InspectorName("切换应用消息Id")] ChangeAppMsgID = 0x1002,
    [InspectorName("窗口状态消息Id")] WndStateMsgID = 0x1003,
    [InspectorName("电脑关机消息Id")] CompShutMsgID = 0x1004,
}