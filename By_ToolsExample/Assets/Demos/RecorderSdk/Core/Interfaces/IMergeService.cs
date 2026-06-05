//=====================================================
// 文件名称: IMergeService
// 创 建 者: wangbaiyan
// 创建日期: 2026-05-23
// 描    述: 定义音视频合并服务接口，便于后续替换合并策略或接入插件。
//=====================================================

namespace Demos.RecorderSdk.Core.Interfaces
{
    using Runtime;

    /// <summary>
    /// 音视频合并服务接口。
    /// </summary>
    public interface IMergeService
    {
        void Merge(RecorderSession session);
    }
}
