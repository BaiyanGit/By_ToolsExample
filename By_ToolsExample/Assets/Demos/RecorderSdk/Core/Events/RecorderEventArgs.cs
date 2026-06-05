//=====================================================
// 文件名称: RecorderEventArgs
// 创 建 者: wangbaiyan
// 创建日期: 2026-05-23
// 描    述: 统一录制器事件参数，承载会话、状态、输出路径、消息和异常信息。
//=====================================================

namespace Demos.RecorderSdk.Core.Events
{
    using System;
    using Runtime;

    /// <summary>
    /// 录制器统一事件参数。
    /// </summary>
    public class RecorderEventArgs : EventArgs
    {
        public string sessionId;
        public RecorderState state;
        public string outputPath;
        public string message;
        public RecorderSessionEventType eventType;
        public RecorderErrorCode errorCode;
        public string errorCodeText;
        public Exception exception;

        /// <summary>
        /// 创建事件参数。
        /// </summary>
        public static RecorderEventArgs Create(string sessionId, RecorderState state, string outputPath, string message, RecorderErrorCode errorCode = RecorderErrorCode.None, Exception exception = null, RecorderSessionEventType eventType = RecorderSessionEventType.None)
        {
            return new RecorderEventArgs
            {
                sessionId = sessionId,
                state = state,
                outputPath = outputPath,
                message = message,
                eventType = eventType,
                errorCode = errorCode,
                errorCodeText = errorCode.ToString(),
                exception = exception
            };
        }
    }
}
