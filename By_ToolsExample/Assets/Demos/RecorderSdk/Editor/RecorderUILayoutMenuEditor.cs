//=====================================================
// 文件名称: RecorderUILayoutMenuEditor
// 创 建 者: wangbaiyan
// 创建日期: 2026-05-25
// 描    述: Recorder SDK UI 布局导出和应用菜单。
//=====================================================

#if UNITY_EDITOR
namespace Demos.示例_录制视频Recorder.Editor
{
    using UnityEditor;

    /// <summary>
    /// 基础演示 UI 布局导出工具。
    /// </summary>
    public static class RecorderDemoUILayoutExporterEditor
    {
        public static void ExportDemoLayout()
        {
            RecorderUILayoutProfileUtilityEditor.ExportProfile(RecorderUILayoutProfileUtilityEditor.FindDemoRoot(), RecorderUILayoutProfileUtilityEditor.DEMO_PROFILE_FILE_NAME);
        }
    }

    /// <summary>
    /// 设置中心 UI 布局导出工具。
    /// </summary>
    public static class RecorderSettingsUILayoutExporterEditor
    {
        public static void ExportSettingsLayout()
        {
            RecorderUILayoutProfileUtilityEditor.ExportProfile(RecorderUILayoutProfileUtilityEditor.FindSettingsRoot(), RecorderUILayoutProfileUtilityEditor.SETTINGS_PROFILE_FILE_NAME);
        }
    }

    /// <summary>
    /// 基础演示 UI 布局应用工具。
    /// </summary>
    public static class RecorderDemoUILayoutApplierEditor
    {
        public static void ApplyDemoLayout()
        {
            RecorderUILayoutProfileUtilityEditor.ApplyProfile(RecorderUILayoutProfileUtilityEditor.FindDemoRoot(), RecorderUILayoutProfileUtilityEditor.DEMO_PROFILE_FILE_NAME);
        }
    }

    /// <summary>
    /// 设置中心 UI 布局应用工具。
    /// </summary>
    public static class RecorderSettingsUILayoutApplierEditor
    {
        public static void ApplySettingsLayout()
        {
            RecorderUILayoutProfileUtilityEditor.ApplyProfile(RecorderUILayoutProfileUtilityEditor.FindSettingsRoot(), RecorderUILayoutProfileUtilityEditor.SETTINGS_PROFILE_FILE_NAME);
        }
    }

    /// <summary>
    /// 记录器SDK FFmpeg指南菜单项。
    /// </summary>
    public static class RecorderFFmpegGuideMenuEditor
    {
        /// <summary>
        /// Opens the FFmpeg parameter guide window.
        /// </summary>
        public static void OpenFFmpegParameterGuide()
        {
            global::FFmpegAllInOneParameterGuideWindow.ShowWindow();
        }

        /// <summary>
        /// 打开视频编码指南窗口。
        /// </summary>
        public static void OpenVideoEncodingGuide()
        {
            global::VideoEncodingAllInOneGuideWindow.ShowWindow();
        }
    }
}
#endif
