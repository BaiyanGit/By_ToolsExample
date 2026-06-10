//=====================================================
// 文件名称: UIOperationResult.cs
// 创建者: Codex
// 创建日期: 2026-06-09
// 描    述: 表达 UI 操作结果。
//=====================================================

namespace _3rdBy.ByFramework.Platform.UISystem
{
    public sealed class UIOperationResult
    {
        public UIOperationResult(
            bool success,
            UIKey requestedUIKey,
            string operationType,
            int previousVersion,
            int currentVersion,
            int statusCode)
        {
            Success = success;
            RequestedUIKey = requestedUIKey;
            OperationType = operationType ?? string.Empty;
            PreviousVersion = previousVersion;
            CurrentVersion = currentVersion;
            StatusCode = statusCode;
        }

        public bool Success { get; }

        public UIKey RequestedUIKey { get; }

        public string OperationType { get; }

        public int PreviousVersion { get; }

        public int CurrentVersion { get; }

        public int StatusCode { get; }
    }
}
