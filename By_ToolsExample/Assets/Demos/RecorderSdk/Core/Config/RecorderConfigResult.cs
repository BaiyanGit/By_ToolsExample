//=====================================================
// 文件名称: RecorderConfigResult
// 创 建 者: wangbaiyan
// 创建日期: 2026-05-23
// 描    述: 封装录制配置管理操作结果，包含错误码、警告、配置和说明。
//=====================================================

namespace Demos.RecorderSdk.Core.Config
{
    using System;
    using System.Collections.Generic;
    using Runtime;
    using UI.Settings;

    /// <summary>
    /// 录制配置操作结果。
    /// </summary>
    [Serializable]
    public class RecorderConfigResult
    {
        public bool success;
        public RecorderErrorCode errorCode;
        public readonly List<string> warnings = new();
        public string configId;
        public RecorderParamsConfig config;
        public string message;

        /// <summary>
        /// 创建成功结果。
        /// </summary>
        public static RecorderConfigResult Success(RecorderParamsConfig config, string message = "")
        {
            return new RecorderConfigResult
            {
                success = true,
                errorCode = RecorderErrorCode.None,
                configId = config?.configId ?? string.Empty,
                config = config,
                message = message
            };
        }

        /// <summary>
        /// 创建失败结果。
        /// </summary>
        public static RecorderConfigResult Failed(RecorderErrorCode errorCode, string message, string configId = "")
        {
            return new RecorderConfigResult
            {
                success = false,
                errorCode = errorCode,
                configId = configId,
                message = message
            };
        }

        /// <summary>
        /// 添加警告。
        /// </summary>
        public void AddWarning(string warning)
        {
            if (!string.IsNullOrWhiteSpace(warning)) warnings.Add(warning);
        }
    }
}
