//=====================================================
// 文件名称: SaveService.cs
// 创 建 者: Codex
// 创建日期: 2026-06-08
// 描    述: 实现 SaveSystem Runtime 的统一存储调度服务。
//=====================================================

namespace _3rdBy.ByFramework.Platform.SaveSystem
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// SaveSystem Runtime 的统一存储调度服务。
    /// </summary>
    public sealed class SaveService : ISaveService
    {
        private readonly object _syncRoot = new();
        private readonly Dictionary<string, SaveProfile> _profiles;
        private readonly Dictionary<string, ISaveProvider> _providers;

        /// <summary>
        /// 初始化 SaveSystem Runtime 服务。
        /// </summary>
        /// <param name="profiles">保存策略快照集合。</param>
        /// <param name="providers">Provider 集合。</param>
        public SaveService(
            IReadOnlyList<SaveProfile> profiles = null,
            IReadOnlyList<ISaveProvider> providers = null)
        {
            _profiles = BuildProfileMap(profiles ?? CreateDefaultProfiles());
            _providers = BuildProviderMap(providers ?? CreateDefaultProviders());
        }

        /// <summary>
        /// 保存指定请求的数据。
        /// </summary>
        /// <typeparam name="TData">数据类型。</typeparam>
        /// <param name="request">保存请求。</param>
        /// <returns>保存结果。</returns>
        public SaveResult Save<TData>(SaveRequest<TData> request)
        {
            if (!TrySave(request, out SaveResult result))
            {
                throw new InvalidOperationException($"[SaveSystem] Save failed: {result.ErrorMessage}");
            }

            return result;
        }

        /// <summary>
        /// 尝试保存指定请求的数据。
        /// </summary>
        /// <typeparam name="TData">数据类型。</typeparam>
        /// <param name="request">保存请求。</param>
        /// <param name="result">输出的保存结果。</param>
        /// <returns>成功返回 true，否则返回 false。</returns>
        public bool TrySave<TData>(SaveRequest<TData> request, out SaveResult result)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            try
            {
                lock (_syncRoot)
                {
                    SaveProfile profile = ResolveProfile(request);
                    ISaveProvider provider = ResolveProvider(profile);
                    SaveDataEnvelope existingEnvelope = null;
                    if (provider.Exists(request, profile))
                    {
                        existingEnvelope = provider.Load(request, profile).Data;
                    }

                    SaveDataEnvelope envelope = BuildEnvelope(request.Data, existingEnvelope);
                    result = provider.Save(request, envelope, profile);
                    return result.Success;
                }
            }
            catch (Exception exception)
            {
                result = new SaveResult(
                    false,
                    request.SaveKey,
                    request.Scope,
                    request.ProfileName,
                    string.Empty,
                    string.Empty,
                    "TrySaveFailed",
                    exception.Message);
                return false;
            }
        }

        /// <summary>
        /// 加载指定请求的数据。
        /// </summary>
        /// <typeparam name="TData">数据类型。</typeparam>
        /// <param name="request">加载请求。</param>
        /// <returns>加载结果。</returns>
        public LoadResult<TData> Load<TData>(SaveRequest request)
        {
            if (!TryLoad(request, out TData data, out LoadResult<TData> result))
            {
                throw new InvalidOperationException($"[SaveSystem] Load failed: {result.ErrorMessage}");
            }

            return result;
        }

        /// <summary>
        /// 尝试加载指定请求的数据。
        /// </summary>
        /// <typeparam name="TData">数据类型。</typeparam>
        /// <param name="request">加载请求。</param>
        /// <param name="data">输出的数据对象。</param>
        /// <param name="result">输出的加载结果。</param>
        /// <returns>成功返回 true，否则返回 false。</returns>
        public bool TryLoad<TData>(SaveRequest request, out TData data, out LoadResult<TData> result)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            try
            {
                lock (_syncRoot)
                {
                    SaveProfile profile = ResolveProfile(request);
                    ISaveProvider provider = ResolveProvider(profile);
                    LoadResult<SaveDataEnvelope> envelopeResult = provider.Load(request, profile);
                    if (!envelopeResult.Success || envelopeResult.Data == null)
                    {
                        data = default;
                        result = new LoadResult<TData>(
                            false,
                            default,
                            request.SaveKey,
                            request.Scope,
                            profile.ProfileName,
                            envelopeResult.ResolvedPath,
                            envelopeResult.ProviderType,
                            envelopeResult.ErrorCode,
                            envelopeResult.ErrorMessage);
                        return false;
                    }

                    data = SaveJsonUtility.DeserializeObject<TData>(envelopeResult.Data.Payload);
                    result = new LoadResult<TData>(
                        true,
                        data,
                        request.SaveKey,
                        request.Scope,
                        profile.ProfileName,
                        envelopeResult.ResolvedPath,
                        envelopeResult.ProviderType);
                    return true;
                }
            }
            catch (Exception exception)
            {
                data = default;
                result = new LoadResult<TData>(
                    false,
                    default,
                    request.SaveKey,
                    request.Scope,
                    request.ProfileName,
                    string.Empty,
                    string.Empty,
                    "TryLoadFailed",
                    exception.Message);
                return false;
            }
        }

        /// <summary>
        /// 删除指定请求对应的数据。
        /// </summary>
        /// <param name="request">删除请求。</param>
        /// <returns>删除结果。</returns>
        public SaveResult Delete(SaveRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            lock (_syncRoot)
            {
                SaveProfile profile = ResolveProfile(request);
                ISaveProvider provider = ResolveProvider(profile);
                return provider.Delete(request, profile);
            }
        }

        /// <summary>
        /// 判断指定请求对应的数据是否存在。
        /// </summary>
        /// <param name="request">检查请求。</param>
        /// <returns>存在返回 true，否则返回 false。</returns>
        public bool Exists(SaveRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            lock (_syncRoot)
            {
                SaveProfile profile = ResolveProfile(request);
                ISaveProvider provider = ResolveProvider(profile);
                return provider.Exists(request, profile);
            }
        }

        /// <summary>
        /// 获取指定存储域下的全部条目元数据。
        /// </summary>
        /// <param name="scope">存储域过滤条件。</param>
        /// <param name="profileName">Profile 名称过滤条件。</param>
        /// <returns>条目元数据列表。</returns>
        public IReadOnlyList<SaveEntryInfo> GetEntries(SaveScope? scope = null, string profileName = null)
        {
            lock (_syncRoot)
            {
                List<SaveEntryInfo> entries = new();
                foreach (KeyValuePair<string, SaveProfile> pair in _profiles)
                {
                    SaveProfile profile = pair.Value;
                    if (!string.IsNullOrWhiteSpace(profileName)
                        && !string.Equals(profile.ProfileName, profileName, StringComparison.Ordinal))
                    {
                        continue;
                    }

                    ISaveProvider provider = ResolveProvider(profile);
                    IReadOnlyList<SaveEntryInfo> providerEntries = provider.GetEntries(profile, scope);
                    for (int index = 0; index < providerEntries.Count; index++)
                    {
                        entries.Add(providerEntries[index]);
                    }
                }

                return entries;
            }
        }

        /// <summary>
        /// 为指定请求对应的数据创建备份。
        /// </summary>
        /// <param name="request">备份请求。</param>
        /// <returns>备份信息。</returns>
        public SaveBackupInfo Backup(SaveRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            lock (_syncRoot)
            {
                SaveProfile profile = ResolveProfile(request);
                ISaveProvider provider = ResolveProvider(profile);
                return provider.Backup(request, profile);
            }
        }

        /// <summary>
        /// 将指定备份恢复为当前数据。
        /// </summary>
        /// <param name="request">恢复目标请求。</param>
        /// <param name="backupId">备份标识。</param>
        /// <returns>恢复结果。</returns>
        public SaveResult Restore(SaveRequest request, string backupId)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            lock (_syncRoot)
            {
                SaveProfile profile = ResolveProfile(request);
                ISaveProvider provider = ResolveProvider(profile);
                return provider.Restore(request, profile, backupId);
            }
        }

        private static Dictionary<string, SaveProfile> BuildProfileMap(IReadOnlyList<SaveProfile> profiles)
        {
            Dictionary<string, SaveProfile> map = new(StringComparer.Ordinal);
            for (int index = 0; index < profiles.Count; index++)
            {
                SaveProfile profile = profiles[index];
                if (profile == null)
                {
                    continue;
                }

                map[profile.ProfileName] = profile;
            }

            return map;
        }

        private static Dictionary<string, ISaveProvider> BuildProviderMap(IReadOnlyList<ISaveProvider> providers)
        {
            Dictionary<string, ISaveProvider> map = new(StringComparer.Ordinal);
            for (int index = 0; index < providers.Count; index++)
            {
                ISaveProvider provider = providers[index];
                if (provider == null)
                {
                    continue;
                }

                map[provider.ProviderType] = provider;
            }

            return map;
        }

        private static IReadOnlyList<SaveProfile> CreateDefaultProfiles()
        {
            return new[]
            {
                new SaveProfile("DefaultGlobal", SaveScope.Global, "FileSaveProvider", string.Empty, "json", true, false, false, true),
                new SaveProfile("DefaultUser", SaveScope.User, "FileSaveProvider", string.Empty, "json", true, false, false, true),
                new SaveProfile("DefaultProject", SaveScope.Project, "FileSaveProvider", string.Empty, "json", true, false, false, true),
                new SaveProfile("DefaultRuntime", SaveScope.Runtime, "FileSaveProvider", string.Empty, "json", false, false, false, false),
                new SaveProfile("DefaultCache", SaveScope.Cache, "FileSaveProvider", string.Empty, "json", false, false, false, false),
                new SaveProfile("DefaultTraining", SaveScope.Training, "FileSaveProvider", string.Empty, "json", true, false, false, false),
                new SaveProfile("DefaultDevice", SaveScope.Device, "FileSaveProvider", string.Empty, "json", true, false, false, false),
                new SaveProfile("DefaultConfig", SaveScope.Config, "FileSaveProvider", string.Empty, "json", true, false, false, true),
            };
        }

        private static IReadOnlyList<ISaveProvider> CreateDefaultProviders()
        {
            return new ISaveProvider[]
            {
                new FileSaveProvider(),
            };
        }

        private SaveProfile ResolveProfile(SaveRequest request)
        {
            if (!string.IsNullOrWhiteSpace(request.ProfileName))
            {
                if (_profiles.TryGetValue(request.ProfileName, out SaveProfile requestedProfile))
                {
                    if (requestedProfile.Scope != request.Scope)
                    {
                        throw new InvalidOperationException(
                            $"[SaveSystem] Save profile scope mismatch: request={request.Scope}, profile={requestedProfile.Scope}");
                    }

                    return requestedProfile;
                }

                throw new InvalidOperationException($"[SaveSystem] Save profile was not found: {request.ProfileName}");
            }

            string defaultProfileName = $"Default{request.Scope}";
            if (_profiles.TryGetValue(defaultProfileName, out SaveProfile defaultProfile))
            {
                return defaultProfile;
            }

            foreach (KeyValuePair<string, SaveProfile> pair in _profiles)
            {
                if (pair.Value.Scope == request.Scope)
                {
                    return pair.Value;
                }
            }

            throw new InvalidOperationException($"[SaveSystem] No default save profile matched scope: {request.Scope}");
        }

        private ISaveProvider ResolveProvider(SaveProfile profile)
        {
            if (!_providers.TryGetValue(profile.ProviderType, out ISaveProvider provider))
            {
                throw new InvalidOperationException($"[SaveSystem] Save provider was not found: {profile.ProviderType}");
            }

            if (!provider.CanHandle(profile))
            {
                throw new InvalidOperationException(
                    $"[SaveSystem] Save provider {provider.ProviderType} cannot handle profile {profile.ProfileName}.");
            }

            return provider;
        }

        private static SaveDataEnvelope BuildEnvelope<TData>(TData data, SaveDataEnvelope existingEnvelope)
        {
            DateTime nowUtc = DateTime.UtcNow;
            SaveDataEnvelope envelope = new()
            {
                SchemaVersion = existingEnvelope?.SchemaVersion ?? "1.0.0",
                DataVersion = existingEnvelope?.DataVersion ?? "1.0.0",
                CreatedTimeUtc = existingEnvelope?.CreatedTimeUtc ?? nowUtc.ToString("O"),
                ModifiedTimeUtc = nowUtc.ToString("O"),
                PayloadType = typeof(TData).FullName ?? typeof(TData).Name,
                Payload = SaveJsonUtility.SerializeObject(data),
            };

            return envelope;
        }
    }
}
