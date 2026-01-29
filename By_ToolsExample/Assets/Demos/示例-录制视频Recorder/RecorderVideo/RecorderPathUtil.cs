namespace Demos.示例_录制视频Recorder.RecorderVideo
{
    using System.IO;
    using UnityEngine;

    public static class RecorderPathUtil
    {
        private static string streamingRoot => Application.streamingAssetsPath;

        private static string screenshotDir => Path.Combine(streamingRoot, "Screenshots");

        private static string videoDir => Path.Combine(streamingRoot, "RecorderVideo");

        public static void EnsureDirectories()
        {
            if (!Directory.Exists(screenshotDir))
                Directory.CreateDirectory(screenshotDir);

            if (!Directory.Exists(videoDir))
                Directory.CreateDirectory(videoDir);
        }

        public static string NewScreenshotPath()
        {
            return Path.Combine(screenshotDir, $"shot_{System.DateTime.Now:yyyyMMdd_HHmmss}.png");
        }

        public static string NewVideoPath()
        {
            return Path.Combine(videoDir, $"record_{System.DateTime.Now:yyyyMMdd_HHmmss}.mp4");
        }
    }
}