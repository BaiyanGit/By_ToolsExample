//=====================================================
// 文件名称: FFmpegCommand
// 创 建 者: wangbaiyan
// 创建日期: 2026-05-23
// 描    述: 保存一次 FFmpeg 调用的可执行文件、参数列表和完整命令行文本。
//=====================================================

namespace Demos.RecorderSdk.Core.Commands
{
    using System.Collections.Generic;

    /// <summary>
    /// FFmpeg 命令数据。
    /// </summary>
    public class FFmpegCommand
    {
        public string executablePath;
        public List<string> arguments = new();
        public string rawCommandLine;

        /// <summary>
        /// 获取 FFmpegProcessRunner 可直接使用的参数字符串。
        /// </summary>
        public string ArgumentsText => rawCommandLine;
    }
}
