using Demos.示例_录制视频Recorder.Scripts.Core;
using UnityEngine;
using UnityEngine.UI;

//=====================================================
// 文件名称: UIRecTest
// 创 建 者: 
// 创建日期: 
// 描    述: 
//=====================================================


public class UIRecTest : MonoBehaviour
{
    public Button btnStart;
    public Button btnStop;

    private void Awake()
    {
        btnStart.onClick.AddListener(OnStartClick);
        btnStop.onClick.AddListener(OnStopClick);
    }

    private static void OnStartClick()
    {
        Debug.Log("Start Recording");
        CrossPlatformScreenRecorder.ins.StartRecording();
    }

    private static void OnStopClick()
    {
        Debug.Log("Stop Recording");
        CrossPlatformScreenRecorder.ins.StopRecording();
    }
}