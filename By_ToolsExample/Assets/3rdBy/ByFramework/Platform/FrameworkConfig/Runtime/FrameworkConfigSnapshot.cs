//=====================================================
// 文件名称: FrameworkConfigSnapshot.cs
// 创 建 者: Codex
// 创建日期: 2026-06-08
// 描    述: 表示 FrameworkConfig 在某一时刻生成的只读不可变配置快照。
//=====================================================

namespace _3rdBy.ByFramework.Platform.FrameworkConfig
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    /// <summary>
    /// 只读不可变配置快照。
    /// </summary>
    public sealed class FrameworkConfigSnapshot
    {
        /// <summary>
        /// 初始化配置快照。
        /// </summary>
        /// <param name="version">快照版本号。</param>
        /// <param name="mergedJson">最终合并后的 JSON 文本。</param>
        /// <param name="layers">参与合并的配置层集合。</param>
        /// <param name="validationMessages">校验消息集合。</param>
        /// <param name="reloadCapability">当前暴露的重载能力状态。</param>
        /// <param name="createdTimeUtc">快照创建时间（UTC）。</param>
        public FrameworkConfigSnapshot(
            int version,
            string mergedJson,
            IReadOnlyList<ConfigLayer> layers,
            IReadOnlyList<ConfigValidationMessage> validationMessages,
            ConfigReloadCapability reloadCapability,
            DateTime createdTimeUtc)
        {
            Version = version;
            MergedJson = string.IsNullOrWhiteSpace(mergedJson) ? "{}" : mergedJson;
            Layers = layers == null
                ? Array.Empty<ConfigLayer>()
                : new ReadOnlyCollection<ConfigLayer>(new List<ConfigLayer>(layers));
            ValidationMessages = validationMessages == null
                ? Array.Empty<ConfigValidationMessage>()
                : new ReadOnlyCollection<ConfigValidationMessage>(new List<ConfigValidationMessage>(validationMessages));
            ReloadCapability = reloadCapability;
            CreatedTimeUtc = createdTimeUtc;
        }

        /// <summary>
        /// 获取快照版本号。
        /// </summary>
        public int Version { get; }

        /// <summary>
        /// 获取最终合并后的 JSON 文本。
        /// </summary>
        public string MergedJson { get; }

        /// <summary>
        /// 获取参与合并的配置层集合。
        /// </summary>
        public IReadOnlyList<ConfigLayer> Layers { get; }

        /// <summary>
        /// 获取校验消息集合。
        /// </summary>
        public IReadOnlyList<ConfigValidationMessage> ValidationMessages { get; }

        /// <summary>
        /// 获取当前暴露的重载能力状态。
        /// </summary>
        public ConfigReloadCapability ReloadCapability { get; }

        /// <summary>
        /// 获取快照创建时间（UTC）。
        /// </summary>
        public DateTime CreatedTimeUtc { get; }

        /// <summary>
        /// 获取当前快照是否包含阻塞性错误。
        /// </summary>
        public bool HasBlockingValidationMessage
        {
            get
            {
                for (int index = 0; index < ValidationMessages.Count; index++)
                {
                    if (ValidationMessages[index].IsBlocking)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        /// <summary>
        /// 强制获取指定模块配置。
        /// </summary>
        /// <typeparam name="TConfig">模块配置类型。</typeparam>
        /// <returns>已解析的模块配置对象。</returns>
        /// <exception cref="InvalidOperationException">当模块配置不存在或无法解析时抛出。</exception>
        public TConfig GetModuleConfig<TConfig>() where TConfig : class
        {
            if (TryGetModuleConfig(out TConfig config))
            {
                return config;
            }

            throw new InvalidOperationException(
                $"[FrameworkConfig] Module config was not found: {typeof(TConfig).FullName}. Please ensure it exists under the modules node.");
        }

        /// <summary>
        /// 安全获取指定模块配置。
        /// </summary>
        /// <typeparam name="TConfig">模块配置类型。</typeparam>
        /// <param name="config">输出的模块配置对象。</param>
        /// <returns>获取成功返回 true，否则返回 false。</returns>
        public bool TryGetModuleConfig<TConfig>(out TConfig config) where TConfig : class
        {
            return FrameworkConfigJsonUtility.TryReadModuleConfig(MergedJson, out config);
        }
    }
}
