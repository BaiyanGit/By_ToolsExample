//=====================================================
// 文件名称: RecorderResult
// 创 建 者: wangbaiyan
// 创建日期: 2026-05-23
// 描    述: 封装开始录制和停止录制的执行结果，方便外部代码 await 后判断成功、错误和输出路径。
//=====================================================

namespace Demos.RecorderSdk.Core.Runtime
{
    using System;

    /// <summary>
    /// 录制操作结果。
    /// </summary>
    [Serializable]
    public class RecorderResult
    {
        public bool success;
        public string message;
        public string outputPath;
        public RecorderErrorCode errorCode;
        public string errorCodeText;
        public Exception exception;
        public string sessionId;

        /// <summary>
        /// 创建成功结果。
        /// </summary>
        public static RecorderResult Success(string message, string outputPath, string sessionId)
        {
            return new RecorderResult
            {
                success = true,
                errorCode = RecorderErrorCode.None,
                errorCodeText = RecorderErrorCode.None.ToString(),
                message = message,
                outputPath = outputPath,
                sessionId = sessionId
            };
        }

        /// <summary>
        /// 创建失败结果。
        /// </summary>
        public static RecorderResult Failed(RecorderErrorCode errorCode, string message, Exception exception = null, string outputPath = "", string sessionId = "")
        {
            return new RecorderResult
            {
                success = false,
                errorCode = errorCode,
                errorCodeText = errorCode.ToString(),
                message = message,
                exception = exception,
                outputPath = outputPath,
                sessionId = sessionId
            };
        }
    }
}
