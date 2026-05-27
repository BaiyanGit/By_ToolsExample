//=====================================================
// 文件名称: IAudioCaptureBackend
// 创 建 者: wangbaiyan
// 创建日期: 2026-05-23
// 描    述: 定义系统音频采集插件接口，供 Windows WASAPI、Linux PulseAudio 等实现扩展。
//=====================================================

namespace Demos.示例_录制视频Recorder.Scripts.Core
{
    /// <summary>
    /// 系统音频采集后端接口。
    /// </summary>
    public interface IAudioCaptureBackend
    {
        bool Start(string outputPath, out string errorMessage);
        bool Stop(out string errorMessage);
    }
}
