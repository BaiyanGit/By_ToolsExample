//=====================================================
// 文件名称: LoadResult.cs
// 创 建 者: Codex
// 创建日期: 2026-06-08
// 描    述: 定义 SaveSystem 的加载结果模型。
//=====================================================

namespace _3rdBy.ByFramework.Platform.SaveSystem
{
    /// <summary>
    /// SaveSystem 加载结果模型。
    /// </summary>
    /// <typeparam name="TData">数据类型。</typeparam>
    public sealed class LoadResult<TData> : SaveResult
    {
        /// <summary>
        /// 初始化加载结果。
        /// </summary>
        /// <param name="success">是否成功。</param>
        /// <param name="data">加载出的数据对象。</param>
        /// <param name="saveKey">逻辑保存键。</param>
        /// <param name="scope">存储域。</param>
        /// <param name="profileName">Profile 名称。</param>
        /// <param name="resolvedPath">解析后的物理路径。</param>
        /// <param name="providerType">Provider 类型。</param>
        /// <param name="errorCode">错误码。</param>
        /// <param name="errorMessage">错误消息。</param>
        public LoadResult(
            bool success,
            TData data,
            string saveKey,
            SaveScope scope,
            string profileName,
            string resolvedPath,
            string providerType,
            string errorCode = "",
            string errorMessage = "")
            : base(success, saveKey, scope, profileName, resolvedPath, providerType, errorCode, errorMessage)
        {
            Data = data;
        }

        /// <summary>
        /// 获取加载出的数据对象。
        /// </summary>
        public TData Data { get; }
    }
}
