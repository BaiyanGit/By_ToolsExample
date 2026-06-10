//=====================================================
// 文件名称: SaveResult.cs
// 创 建 者: Codex
// 创建日期: 2026-06-08
// 描    述: 定义 SaveSystem 的通用操作结果模型。
//=====================================================

namespace _3rdBy.ByFramework.Platform.SaveSystem
{
    /// <summary>
    /// SaveSystem 通用操作结果模型。
    /// </summary>
    public class SaveResult
    {
        /// <summary>
        /// 初始化操作结果。
        /// </summary>
        /// <param name="success">是否成功。</param>
        /// <param name="saveKey">逻辑保存键。</param>
        /// <param name="scope">存储域。</param>
        /// <param name="profileName">Profile 名称。</param>
        /// <param name="resolvedPath">解析后的物理路径。</param>
        /// <param name="providerType">Provider 类型。</param>
        /// <param name="errorCode">错误码。</param>
        /// <param name="errorMessage">错误消息。</param>
        public SaveResult(
            bool success,
            string saveKey,
            SaveScope scope,
            string profileName,
            string resolvedPath,
            string providerType,
            string errorCode = "",
            string errorMessage = "")
        {
            Success = success;
            SaveKey = saveKey ?? string.Empty;
            Scope = scope;
            ProfileName = profileName ?? string.Empty;
            ResolvedPath = resolvedPath ?? string.Empty;
            ProviderType = providerType ?? string.Empty;
            ErrorCode = errorCode ?? string.Empty;
            ErrorMessage = errorMessage ?? string.Empty;
        }

        /// <summary>
        /// 获取是否成功。
        /// </summary>
        public bool Success { get; }

        /// <summary>
        /// 获取逻辑保存键。
        /// </summary>
        public string SaveKey { get; }

        /// <summary>
        /// 获取存储域。
        /// </summary>
        public SaveScope Scope { get; }

        /// <summary>
        /// 获取 Profile 名称。
        /// </summary>
        public string ProfileName { get; }

        /// <summary>
        /// 获取解析后的物理路径。
        /// </summary>
        public string ResolvedPath { get; }

        /// <summary>
        /// 获取 Provider 类型。
        /// </summary>
        public string ProviderType { get; }

        /// <summary>
        /// 获取错误码。
        /// </summary>
        public string ErrorCode { get; }

        /// <summary>
        /// 获取错误消息。
        /// </summary>
        public string ErrorMessage { get; }
    }
}
