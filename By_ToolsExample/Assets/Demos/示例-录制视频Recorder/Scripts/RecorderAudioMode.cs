//=====================================================
// 文件名称: RecorderAudioMode
// 创 建 者: wangbaiyan
// 创建日期: 2026-4-14
// 描    述: 录屏音频采集模式枚举。
//=====================================================

using UnityEngine;

public enum RecorderAudioMode
{
    [InspectorName("无音频")] None,
    [InspectorName("系统音频")] SystemAudio,
    [InspectorName("Unity音频")] UnityAudioReserved
}