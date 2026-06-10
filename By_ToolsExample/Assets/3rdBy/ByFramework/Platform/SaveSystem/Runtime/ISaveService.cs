//=====================================================
// 文件名称: ISaveService.cs
// 创 建 者: Codex
// 创建日期: 2026-06-08
// 描    述: 定义 SaveSystem Runtime 服务接口。
//=====================================================

namespace _3rdBy.ByFramework.Platform.SaveSystem
{
    using System.Collections.Generic;

    /// <summary>
    /// SaveSystem Runtime 服务接口。
    /// </summary>
    public interface ISaveService : _3rdBy.ByFramework.Platform.PlatformServiceRegistry.IPlatformService
    {
        /// <summary>
        /// 保存指定请求的数据。
        /// </summary>
        /// <typeparam name="TData">数据类型。</typeparam>
        /// <param name="request">保存请求。</param>
        /// <returns>保存结果。</returns>
        SaveResult Save<TData>(SaveRequest<TData> request);

        /// <summary>
        /// 尝试保存指定请求的数据。
        /// </summary>
        /// <typeparam name="TData">数据类型。</typeparam>
        /// <param name="request">保存请求。</param>
        /// <param name="result">输出的保存结果。</param>
        /// <returns>成功返回 true，否则返回 false。</returns>
        bool TrySave<TData>(SaveRequest<TData> request, out SaveResult result);

        /// <summary>
        /// 加载指定请求的数据。
        /// </summary>
        /// <typeparam name="TData">数据类型。</typeparam>
        /// <param name="request">加载请求。</param>
        /// <returns>加载结果。</returns>
        LoadResult<TData> Load<TData>(SaveRequest request);

        /// <summary>
        /// 尝试加载指定请求的数据。
        /// </summary>
        /// <typeparam name="TData">数据类型。</typeparam>
        /// <param name="request">加载请求。</param>
        /// <param name="data">输出的数据对象。</param>
        /// <param name="result">输出的加载结果。</param>
        /// <returns>成功返回 true，否则返回 false。</returns>
        bool TryLoad<TData>(SaveRequest request, out TData data, out LoadResult<TData> result);

        /// <summary>
        /// 删除指定请求对应的数据。
        /// </summary>
        /// <param name="request">删除请求。</param>
        /// <returns>删除结果。</returns>
        SaveResult Delete(SaveRequest request);

        /// <summary>
        /// 判断指定请求对应的数据是否存在。
        /// </summary>
        /// <param name="request">检查请求。</param>
        /// <returns>存在返回 true，否则返回 false。</returns>
        bool Exists(SaveRequest request);

        /// <summary>
        /// 获取指定存储域下的全部条目元数据。
        /// </summary>
        /// <param name="scope">存储域过滤条件。</param>
        /// <param name="profileName">Profile 名称过滤条件。</param>
        /// <returns>条目元数据列表。</returns>
        IReadOnlyList<SaveEntryInfo> GetEntries(SaveScope? scope = null, string profileName = null);

        /// <summary>
        /// 为指定请求对应的数据创建备份。
        /// </summary>
        /// <param name="request">备份请求。</param>
        /// <returns>备份信息。</returns>
        SaveBackupInfo Backup(SaveRequest request);

        /// <summary>
        /// 将指定备份恢复为当前数据。
        /// </summary>
        /// <param name="request">恢复目标请求。</param>
        /// <param name="backupId">备份标识。</param>
        /// <returns>恢复结果。</returns>
        SaveResult Restore(SaveRequest request, string backupId);
    }
}
