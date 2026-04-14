//=====================================================
// 文件名称: CrossPlatformScreenRecorderUI
// 创 建 者: wangbaiyan
// 创建日期: 2026-4-13
// 描    述: 跨平台桌面录屏 UI 控制器，负责下拉框、按钮与录屏核心控制器之间的交互。
//=====================================================

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CrossPlatformScreenRecorderUI : MonoBehaviour
{
    [Header("录屏核心控制器")] public CrossPlatformScreenRecorder recorder;

    [Header("显示器选择下拉框")] public Dropdown displayDropdown;

    [Header("开始录制按钮")] public Button startButton;

    [Header("停止录制按钮")] public Button stopButton;

    [Header("是否启动时自动刷新显示器列表")] public bool refreshOnStart = true;

    /// <summary>
    /// 启动时绑定事件。
    /// </summary>
    private void Start()
    {
        if (recorder == null)
        {
            Debug.LogError("未指定 CrossPlatformScreenRecorder。");
            return;
        }

        recorder.OnDisplayListChanged    += HandleDisplayListChanged;
        recorder.OnRecordingStateChanged += HandleRecordingStateChanged;
        recorder.OnRecordStarted         += HandleRecordStarted;
        recorder.OnRecordStopped         += HandleRecordStopped;

        BindButtons();

        if (refreshOnStart)
        {
            recorder.Initialize();
            recorder.RefreshDisplayList();
        }

        HandleRecordingStateChanged(recorder.IsRecording);
    }

    /// <summary>
    /// 绑定按钮事件。
    /// </summary>
    private void BindButtons()
    {
        if (startButton != null)
        {
            startButton.onClick.RemoveListener(OnClickStartRecord);
            startButton.onClick.AddListener(OnClickStartRecord);
        }

        if (stopButton != null)
        {
            stopButton.onClick.RemoveListener(OnClickStopRecord);
            stopButton.onClick.AddListener(OnClickStopRecord);
        }
    }

    /// <summary>
    /// 点击开始录制按钮时执行。
    /// </summary>
    private void OnClickStartRecord()
    {
        if (recorder == null)
        {
            return;
        }

        int selectedIndex = 0;

        if (displayDropdown != null)
        {
            selectedIndex = displayDropdown.value;
        }

        recorder.StartRecording(selectedIndex);
    }

    /// <summary>
    /// 点击停止录制按钮时执行。
    /// </summary>
    private void OnClickStopRecord()
    {
        if (recorder == null)
        {
            return;
        }

        recorder.StopRecording();
    }

    /// <summary>
    /// 处理显示器列表刷新事件。
    /// </summary>
    /// <param name="displays">显示器列表。</param>
    private void HandleDisplayListChanged(List<RecorderDisplayInfo> displays)
    {
        if (displayDropdown == null)
        {
            return;
        }

        displayDropdown.ClearOptions();

        var options = new List<string>();
        foreach (var display in displays)
        {
            options.Add(display.ToOptionText());
        }

        displayDropdown.AddOptions(options);
    }

    /// <summary>
    /// 处理录制状态变化事件。
    /// </summary>
    /// <param name="isRecording">是否正在录制。</param>
    private void HandleRecordingStateChanged(bool isRecording)
    {
        if (startButton != null)
        {
            startButton.interactable = !isRecording;
        }

        if (stopButton != null)
        {
            stopButton.interactable = isRecording;
        }

        if (displayDropdown != null)
        {
            displayDropdown.interactable = !isRecording;
        }
    }

    /// <summary>
    /// 处理录制开始事件。
    /// </summary>
    /// <param name="outputPath">输出文件路径。</param>
    private void HandleRecordStarted(string outputPath)
    {
        Debug.Log("UI 接收到录制开始事件: " + outputPath);
    }

    /// <summary>
    /// 处理录制停止事件。
    /// </summary>
    /// <param name="outputPath">输出文件路径。</param>
    private void HandleRecordStopped(string outputPath)
    {
        Debug.Log("UI 接收到录制停止事件: " + outputPath);
    }

    /// <summary>
    /// 销毁对象时解绑事件。
    /// </summary>
    private void OnDestroy()
    {
        if (startButton != null)
        {
            startButton.onClick.RemoveListener(OnClickStartRecord);
        }

        if (stopButton != null)
        {
            stopButton.onClick.RemoveListener(OnClickStopRecord);
        }

        if (recorder != null)
        {
            recorder.OnDisplayListChanged    -= HandleDisplayListChanged;
            recorder.OnRecordingStateChanged -= HandleRecordingStateChanged;
            recorder.OnRecordStarted         -= HandleRecordStarted;
            recorder.OnRecordStopped         -= HandleRecordStopped;
        }
    }
}