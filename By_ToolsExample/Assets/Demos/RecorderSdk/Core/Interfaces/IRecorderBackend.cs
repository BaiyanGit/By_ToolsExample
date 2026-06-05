//=====================================================
// 文件名称: IRecorderBackend
// 创 建 者: wangbaiyan
// 创建日期: 2026-05-23
// 描    述: 定义录制后端插件接口，供后续替换 FFmpeg 或接入其它录制实现。
//=====================================================

namespace Demos.RecorderSdk.Core.Interfaces
{
    using System.Threading.Tasks;
    using Runtime;

    /// <summary>
    /// 录制后端插件接口。
    /// </summary>
    public interface IRecorderBackend
    {
        Task<RecorderResult> StartAsync(RecorderSession session);
        Task<RecorderResult> StopAsync(RecorderSession session);
    }
}
