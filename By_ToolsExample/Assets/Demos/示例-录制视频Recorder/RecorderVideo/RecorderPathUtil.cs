namespace Demos.示例_录制视频Recorder.RecorderVideo
{
    using System.IO;
    using UnityEngine;

    /// <summary>
    /// 路径工具类
    /// </summary>
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
            return Path.Combine(screenshotDir, $"Shot_{System.DateTime.Now:yyyyMMdd_HH_mm_ss}.png");
        }

        public static string NewVideoPath()
        {
            return Path.Combine(videoDir, $"Record_{System.DateTime.Now:yyyyMMdd_HH_mm_ss}.mp4");
        }

        public static string ShareComputerVideoPath()
        {
            const string sharePath = @"\\DESKTOP-MNEF9HR\testshare\ShareVideo\";
            return Path.Combine(sharePath, $"ShareRecord_{System.DateTime.Now:yyyyMMdd_HH_mm_ss}.mp4");
        }
    }
}