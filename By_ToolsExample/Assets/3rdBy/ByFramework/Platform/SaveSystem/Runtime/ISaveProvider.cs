//=====================================================
// 文件名称: ISaveProvider.cs
// 创 建 者: Codex
// 创建日期: 2026-06-08
// 描    述: 定义 SaveSystem 存储 Provider 接口。
//=====================================================

namespace _3rdBy.ByFramework.Platform.SaveSystem
{
    using System.Collections.Generic;

    /// <summary>
    /// SaveSystem 存储 Provider 接口。
    /// </summary>
    public interface ISaveProvider
    {
        /// <summary>
        /// 获取 Provider 类型名称。
        /// </summary>
        string ProviderType { get; }

        /// <summary>
        /// 判断当前 Provider 是否可处理指定 Profile。
        /// </summary>
        /// <param name="profile">待检查的 Profile。</param>
        /// <returns>可处理返回 true，否则返回 false。</returns>
        bool CanHandle(SaveProfile profile);

        /// <summary>
        /// 保存 Envelope 数据。
        /// </summary>
        /// <param name="request">保存请求。</param>
        /// <param name="envelope">待保存的 Envelope。</param>
        /// <param name="profile">目标 Profile。</param>
        /// <returns>保存结果。</returns>
        SaveResult Save(SaveRequest request, SaveDataEnvelope envelope, SaveProfile profile);

        /// <summary>
        /// 加载 Envelope 数据。
        /// </summary>
        /// <param name="request">加载请求。</param>
        /// <param name="profile">目标 Profile。</param>
        /// <returns>Envelope 加载结果。</returns>
        LoadResult<SaveDataEnvelope> Load(SaveRequest request, SaveProfile profile);

        /// <summary>
        /// 删除指定数据。
        /// </summary>
        /// <param name="request">删除请求。</param>
        /// <param name="profile">目标 Profile。</param>
        /// <returns>删除结果。</returns>
        SaveResult Delete(SaveRequest request, SaveProfile profile);

        /// <summary>
        /// 判断指定数据是否存在。
        /// </summary>
        /// <param name="request">检查请求。</param>
        /// <param name="profile">目标 Profile。</param>
        /// <returns>存在返回 true，否则返回 false。</returns>
        bool Exists(SaveRequest request, SaveProfile profile);

        /// <summary>
        /// 获取指定 Profile 下的条目元数据。
        /// </summary>
        /// <param name="profile">目标 Profile。</param>
        /// <param name="scope">存储域过滤条件。</param>
        /// <returns>条目元数据列表。</returns>
        IReadOnlyList<SaveEntryInfo> GetEntries(SaveProfile profile, SaveScope? scope = null);

        /// <summary>
        /// 为指定数据创建备份。
        /// </summary>
        /// <param name="request">备份请求。</param>
        /// <param name="profile">目标 Profile。</param>
        /// <returns>备份信息。</returns>
        SaveBackupInfo Backup(SaveRequest request, SaveProfile profile);

        /// <summary>
        /// 恢复指定备份。
        /// </summary>
        /// <param name="request">恢复目标请求。</param>
        /// <param name="profile">目标 Profile。</param>
        /// <param name="backupId">备份标识。</param>
        /// <returns>恢复结果。</returns>
        SaveResult Restore(SaveRequest request, SaveProfile profile, string backupId);
    }
}
