//=====================================================
// 文件名称: SaveRequest.cs
// 创 建 者: Codex
// 创建日期: 2026-06-08
// 描    述: 定义 SaveSystem 的保存与加载请求模型。
//=====================================================

namespace _3rdBy.ByFramework.Platform.SaveSystem
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    /// <summary>
    /// 保存与加载请求模型。
    /// </summary>
    public class SaveRequest
    {
        /// <summary>
        /// 初始化请求模型。
        /// </summary>
        /// <param name="saveKey">逻辑保存键。</param>
        /// <param name="scope">存储域。</param>
        /// <param name="profileName">Profile 名称。</param>
        /// <param name="slot">可选槽位。</param>
        /// <param name="tags">可选标签集合。</param>
        /// <param name="expectedType">预期数据类型。</param>
        public SaveRequest(
            string saveKey,
            SaveScope scope,
            string profileName = "",
            string slot = "",
            IReadOnlyList<string> tags = null,
            Type expectedType = null)
        {
            SaveKey = string.IsNullOrWhiteSpace(saveKey)
                ? throw new ArgumentException("Save key cannot be null or empty.", nameof(saveKey))
                : saveKey;

            string[] segments = SaveKey.Split('.');
            for (int index = 0; index < segments.Length; index++)
            {
                if (string.IsNullOrWhiteSpace(segments[index]))
                {
                    throw new ArgumentException("Save key contains an empty path segment.", nameof(saveKey));
                }
            }

            Scope = scope;
            ProfileName = profileName ?? string.Empty;
            Slot = slot ?? string.Empty;
            Tags = tags == null
                ? Array.Empty<string>()
                : new ReadOnlyCollection<string>(new List<string>(tags));
            ExpectedType = expectedType;
        }

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
        /// 获取可选槽位。
        /// </summary>
        public string Slot { get; }

        /// <summary>
        /// 获取标签集合。
        /// </summary>
        public IReadOnlyList<string> Tags { get; }

        /// <summary>
        /// 获取预期数据类型。
        /// </summary>
        public Type ExpectedType { get; }
    }

    /// <summary>
    /// 带数据载荷的保存请求模型。
    /// </summary>
    /// <typeparam name="TData">数据类型。</typeparam>
    public sealed class SaveRequest<TData> : SaveRequest
    {
        /// <summary>
        /// 初始化带载荷的保存请求。
        /// </summary>
        /// <param name="saveKey">逻辑保存键。</param>
        /// <param name="scope">存储域。</param>
        /// <param name="data">待保存的数据对象。</param>
        /// <param name="profileName">Profile 名称。</param>
        /// <param name="slot">可选槽位。</param>
        /// <param name="tags">可选标签集合。</param>
        public SaveRequest(
            string saveKey,
            SaveScope scope,
            TData data,
            string profileName = "",
            string slot = "",
            IReadOnlyList<string> tags = null)
            : base(saveKey, scope, profileName, slot, tags, typeof(TData))
        {
            Data = data;
        }

        /// <summary>
        /// 获取待保存的数据对象。
        /// </summary>
        public TData Data { get; }
    }
}
