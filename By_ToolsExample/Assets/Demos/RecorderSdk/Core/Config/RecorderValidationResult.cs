//=====================================================
// 文件名称: RecorderValidationResult
// 创 建 者: wangbaiyan
// 创建日期: 2026-05-23
// 描    述: 保存录制配置校验结果，区分错误和警告。
//=====================================================

using System.Collections.Generic;

namespace Demos.示例_录制视频Recorder.Scripts.Core
{
    /// <summary>
    /// 录制配置校验结果。
    /// </summary>
    public class RecorderValidationResult
    {
        public bool isValid => errors.Count == 0;
        public readonly List<string> errors = new();
        public readonly List<RecorderErrorCode> errorCodes = new();
        public readonly List<string> warnings = new();
        public readonly List<RecorderErrorCode> warningCodes = new();

        /// <summary>
        /// 添加错误。
        /// </summary>
        public void AddError(string message, RecorderErrorCode errorCode = RecorderErrorCode.ConfigInvalid)
        {
            if (string.IsNullOrWhiteSpace(message)) return;

            errors.Add(message);
            errorCodes.Add(errorCode);
        }

        /// <summary>
        /// 添加警告。
        /// </summary>
        public void AddWarning(string message, RecorderErrorCode warningCode = RecorderErrorCode.ConfigInvalid)
        {
            if (string.IsNullOrWhiteSpace(message)) return;

            warnings.Add(message);
            warningCodes.Add(warningCode);
        }

        /// <summary>
        /// 获取第一条错误或默认说明。
        /// </summary>
        public string GetFirstErrorOrDefault(string defaultMessage)
        {
            return errors.Count > 0 ? errors[0] : defaultMessage;
        }

        /// <summary>
        /// 获取第一条错误码或默认错误码。
        /// </summary>
        public RecorderErrorCode GetFirstErrorCodeOrDefault(RecorderErrorCode defaultErrorCode)
        {
            return errorCodes.Count > 0 ? errorCodes[0] : defaultErrorCode;
        }
    }
}
