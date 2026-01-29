namespace Demos.示例_录制视频Recorder.RecorderVideo
{
    using System;
    using UnityEngine;

    public class RecorderTestInput : MonoBehaviour
    {
        [Header("开始录制按键")] public KeyCode recordKey = KeyCode.F5;
        [Header("结束录制按键")] public KeyCode stopKey = KeyCode.F6;
        [Header("更改颜色")] public KeyCode changeColorKey = KeyCode.F4;
        [Header("录制器")] public AsyncFFmpegRecorderPooledAv recorder;

        public ColorType recordColor = ColorType.Default;
        public MeshRenderer meshRenderer;

        public enum ColorType
        {
            [InspectorName("默认")] Default,
            [InspectorName("红色")] Green,
            [InspectorName("蓝色")] Blue,
            [InspectorName("绿色")] Red,
            [InspectorName("黄色")] Yellow,
            [InspectorName("紫色")] Purple,
            [InspectorName("粉色")] Pink,
            [InspectorName("青色")] Cyan,
            [InspectorName("白色")] White,
            [InspectorName("黑色")] Black,
        }


        private void ChangeColor()
        {
            var currentColor = recordColor switch
            {
                ColorType.Default => Color.white,
                ColorType.Green   => Color.green,
                ColorType.Blue    => Color.blue,
                ColorType.Red     => Color.red,
                ColorType.Yellow  => Color.yellow,
                ColorType.Purple  => Color.magenta,
                ColorType.Pink    => new Color(1, 0.5f, 0.5f),
                ColorType.Cyan    => Color.cyan,
                ColorType.White   => Color.white,
                ColorType.Black   => Color.black,
                _                 => throw new ArgumentOutOfRangeException()
            };

            meshRenderer.material.color = currentColor;
        }

        private void Update()
        {
            if (Input.GetKeyDown(recordKey))
            {
                recordColor += 1;
                if (recordColor > ColorType.Black)
                    recordColor = ColorType.Default;
                ChangeColor();
                recorder.StartRecording();
            }

            if (Input.GetKeyDown(stopKey))
                recorder.StopRecording();
        }
    }
}