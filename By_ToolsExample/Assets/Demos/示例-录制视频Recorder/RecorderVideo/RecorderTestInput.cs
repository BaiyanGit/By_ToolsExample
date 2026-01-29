namespace Demos.示例_录制视频Recorder.RecorderVideo
{
    using UnityEngine;

    public class RecorderTestInput : MonoBehaviour
    {
        [Header("开始录制按键")] public KeyCode recordKey = KeyCode.F9;
        [Header("结束录制按键")] public KeyCode stopKey = KeyCode.F10;
        [Header("录制器")] public AsyncFFmpegRecorderPooledAv recorder;


        [ContextMenu("创建录制器")]
        public void CreateRecorder()
        {
            RecorderPathUtil.EnsureDirectories();
        }

        private void Update()
        {
            if (Input.GetKeyDown(recordKey))
                recorder.StartRecording();

            if (Input.GetKeyDown(stopKey))
                recorder.StopRecording();
        }
    }
}