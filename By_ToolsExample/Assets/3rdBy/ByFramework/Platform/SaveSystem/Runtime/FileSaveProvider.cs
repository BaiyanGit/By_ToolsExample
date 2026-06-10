//=====================================================
// 文件名称: FileSaveProvider.cs
// 创 建 者: Codex
// 创建日期: 2026-06-08
// 描    述: 提供基于文件系统的 SaveSystem 基础 Provider 实现。
//=====================================================

namespace _3rdBy.ByFramework.Platform.SaveSystem
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using UnityEngine;

    /// <summary>
    /// 基于文件系统的 SaveSystem 基础 Provider。
    /// </summary>
    public sealed class FileSaveProvider : ISaveProvider
    {
        private const string DefaultProviderType = "FileSaveProvider";

        /// <summary>
        /// 获取 Provider 类型名称。
        /// </summary>
        public string ProviderType => DefaultProviderType;

        /// <summary>
        /// 判断当前 Provider 是否可处理指定 Profile。
        /// </summary>
        /// <param name="profile">待检查的 Profile。</param>
        /// <returns>可处理返回 true，否则返回 false。</returns>
        public bool CanHandle(SaveProfile profile)
        {
            return profile != null
                && string.Equals(profile.ProviderType, DefaultProviderType, StringComparison.Ordinal);
        }

        /// <summary>
        /// 保存 Envelope 数据。
        /// </summary>
        /// <param name="request">保存请求。</param>
        /// <param name="envelope">待保存的 Envelope。</param>
        /// <param name="profile">目标 Profile。</param>
        /// <returns>保存结果。</returns>
        public SaveResult Save(SaveRequest request, SaveDataEnvelope envelope, SaveProfile profile)
        {
            ValidateArguments(request, profile);

            if (envelope == null)
            {
                throw new ArgumentNullException(nameof(envelope));
            }

            try
            {
                string resolvedPath = ResolveDataPath(request, profile);
                string directory = Path.GetDirectoryName(resolvedPath) ?? string.Empty;
                Directory.CreateDirectory(directory);
                File.WriteAllText(resolvedPath, SaveJsonUtility.SerializeEnvelope(envelope));
                return CreateSuccessResult(request, profile, resolvedPath);
            }
            catch (Exception exception)
            {
                return CreateFailureResult(request, profile, string.Empty, "SaveFailed", exception.Message);
            }
        }

        /// <summary>
        /// 加载 Envelope 数据。
        /// </summary>
        /// <param name="request">加载请求。</param>
        /// <param name="profile">目标 Profile。</param>
        /// <returns>Envelope 加载结果。</returns>
        public LoadResult<SaveDataEnvelope> Load(SaveRequest request, SaveProfile profile)
        {
            ValidateArguments(request, profile);

            string resolvedPath = ResolveDataPath(request, profile);
            if (!File.Exists(resolvedPath))
            {
                return new LoadResult<SaveDataEnvelope>(
                    false,
                    null,
                    request.SaveKey,
                    request.Scope,
                    profile.ProfileName,
                    resolvedPath,
                    ProviderType,
                    "NotFound",
                    "[SaveSystem] Save entry was not found.");
            }

            try
            {
                string content = File.ReadAllText(resolvedPath);
                SaveDataEnvelope envelope = SaveJsonUtility.DeserializeEnvelope(content);
                return new LoadResult<SaveDataEnvelope>(
                    true,
                    envelope,
                    request.SaveKey,
                    request.Scope,
                    profile.ProfileName,
                    resolvedPath,
                    ProviderType);
            }
            catch (Exception exception)
            {
                return new LoadResult<SaveDataEnvelope>(
                    false,
                    null,
                    request.SaveKey,
                    request.Scope,
                    profile.ProfileName,
                    resolvedPath,
                    ProviderType,
                    "LoadFailed",
                    exception.Message);
            }
        }

        /// <summary>
        /// 删除指定数据。
        /// </summary>
        /// <param name="request">删除请求。</param>
        /// <param name="profile">目标 Profile。</param>
        /// <returns>删除结果。</returns>
        public SaveResult Delete(SaveRequest request, SaveProfile profile)
        {
            ValidateArguments(request, profile);

            string resolvedPath = ResolveDataPath(request, profile);
            if (!File.Exists(resolvedPath))
            {
                return CreateFailureResult(request, profile, resolvedPath, "NotFound", "[SaveSystem] Save entry was not found.");
            }

            try
            {
                File.Delete(resolvedPath);
                return CreateSuccessResult(request, profile, resolvedPath);
            }
            catch (Exception exception)
            {
                return CreateFailureResult(request, profile, resolvedPath, "DeleteFailed", exception.Message);
            }
        }

        /// <summary>
        /// 判断指定数据是否存在。
        /// </summary>
        /// <param name="request">检查请求。</param>
        /// <param name="profile">目标 Profile。</param>
        /// <returns>存在返回 true，否则返回 false。</returns>
        public bool Exists(SaveRequest request, SaveProfile profile)
        {
            ValidateArguments(request, profile);
            return File.Exists(ResolveDataPath(request, profile));
        }

        /// <summary>
        /// 获取指定 Profile 下的条目元数据。
        /// </summary>
        /// <param name="profile">目标 Profile。</param>
        /// <param name="scope">存储域过滤条件。</param>
        /// <returns>条目元数据列表。</returns>
        public IReadOnlyList<SaveEntryInfo> GetEntries(SaveProfile profile, SaveScope? scope = null)
        {
            if (profile == null)
            {
                throw new ArgumentNullException(nameof(profile));
            }

            if (scope.HasValue && profile.Scope != scope.Value)
            {
                return Array.Empty<SaveEntryInfo>();
            }

            string rootPath = ResolveScopeRoot(profile);
            if (!Directory.Exists(rootPath))
            {
                return Array.Empty<SaveEntryInfo>();
            }

            List<SaveEntryInfo> entries = new();
            string[] files = Directory.GetFiles(rootPath, $"*.{profile.Format}", SearchOption.AllDirectories);
            for (int index = 0; index < files.Length; index++)
            {
                string path = files[index];
                if (path.IndexOf($"{Path.DirectorySeparatorChar}.backup{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase) >= 0
                    || path.IndexOf($"{Path.AltDirectorySeparatorChar}.backup{Path.AltDirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    continue;
                }

                FileInfo fileInfo = new(path);
                SaveDataEnvelope envelope = TryReadEnvelope(path);
                entries.Add(new SaveEntryInfo(
                    RestoreSaveKeyFromPath(rootPath, path, profile.Format),
                    profile.Scope,
                    profile.ProfileName,
                    ProviderType,
                    path,
                    envelope?.PayloadType ?? string.Empty,
                    fileInfo.CreationTimeUtc,
                    fileInfo.LastWriteTimeUtc,
                    fileInfo.Length));
            }

            return entries;
        }

        /// <summary>
        /// 为指定数据创建备份。
        /// </summary>
        /// <param name="request">备份请求。</param>
        /// <param name="profile">目标 Profile。</param>
        /// <returns>备份信息。</returns>
        public SaveBackupInfo Backup(SaveRequest request, SaveProfile profile)
        {
            ValidateArguments(request, profile);

            if (!profile.EnableBackup)
            {
                throw new InvalidOperationException("[SaveSystem] Backup is disabled for the current save profile.");
            }

            string sourcePath = ResolveDataPath(request, profile);
            if (!File.Exists(sourcePath))
            {
                throw new InvalidOperationException("[SaveSystem] Cannot backup a save entry that does not exist.");
            }

            string backupId = Guid.NewGuid().ToString("N");
            string backupDirectory = ResolveBackupDirectory(request, profile);
            Directory.CreateDirectory(backupDirectory);
            string backupPath = Path.Combine(backupDirectory, $"{backupId}.{profile.Format}");
            File.Copy(sourcePath, backupPath, true);

            return new SaveBackupInfo(
                backupId,
                request.SaveKey,
                request.Scope,
                profile.ProfileName,
                DateTime.UtcNow,
                backupPath,
                "ManualBackup");
        }

        /// <summary>
        /// 恢复指定备份。
        /// </summary>
        /// <param name="request">恢复目标请求。</param>
        /// <param name="profile">目标 Profile。</param>
        /// <param name="backupId">备份标识。</param>
        /// <returns>恢复结果。</returns>
        public SaveResult Restore(SaveRequest request, SaveProfile profile, string backupId)
        {
            ValidateArguments(request, profile);

            if (!profile.EnableBackup)
            {
                return CreateFailureResult(
                    request,
                    profile,
                    string.Empty,
                    "BackupDisabled",
                    "[SaveSystem] Restore is unavailable because backup is disabled for the current save profile.");
            }

            if (string.IsNullOrWhiteSpace(backupId))
            {
                throw new ArgumentException("Backup id cannot be null or empty.", nameof(backupId));
            }

            string backupPath = Path.Combine(ResolveBackupDirectory(request, profile), $"{backupId}.{profile.Format}");
            if (!File.Exists(backupPath))
            {
                return CreateFailureResult(request, profile, backupPath, "BackupNotFound", "[SaveSystem] Backup entry was not found.");
            }

            try
            {
                string resolvedPath = ResolveDataPath(request, profile);
                Directory.CreateDirectory(Path.GetDirectoryName(resolvedPath) ?? string.Empty);
                File.Copy(backupPath, resolvedPath, true);
                return CreateSuccessResult(request, profile, resolvedPath);
            }
            catch (Exception exception)
            {
                return CreateFailureResult(request, profile, backupPath, "RestoreFailed", exception.Message);
            }
        }

        private static void ValidateArguments(SaveRequest request, SaveProfile profile)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (profile == null)
            {
                throw new ArgumentNullException(nameof(profile));
            }
        }

        private static SaveResult CreateSuccessResult(SaveRequest request, SaveProfile profile, string resolvedPath)
        {
            return new SaveResult(
                true,
                request.SaveKey,
                request.Scope,
                profile.ProfileName,
                resolvedPath,
                DefaultProviderType);
        }

        private static SaveResult CreateFailureResult(
            SaveRequest request,
            SaveProfile profile,
            string resolvedPath,
            string errorCode,
            string errorMessage)
        {
            return new SaveResult(
                false,
                request.SaveKey,
                request.Scope,
                profile.ProfileName,
                resolvedPath,
                DefaultProviderType,
                errorCode,
                errorMessage);
        }

        private static string ResolveDataPath(SaveRequest request, SaveProfile profile)
        {
            string rootPath = ResolveScopeRoot(profile);
            string[] keySegments = request.SaveKey.Split('.');
            for (int index = 0; index < keySegments.Length; index++)
            {
                keySegments[index] = SanitizePathSegment(keySegments[index]);
            }

            string keyPath = Path.Combine(keySegments);
            if (!string.IsNullOrWhiteSpace(request.Slot))
            {
                keyPath = Path.Combine(keyPath, SanitizePathSegment(request.Slot));
            }

            return Path.Combine(rootPath, $"{keyPath}.{profile.Format}");
        }

        private static string ResolveScopeRoot(SaveProfile profile)
        {
            string root = string.IsNullOrWhiteSpace(profile.StorageRoot)
                ? Path.Combine(Application.persistentDataPath, "ByFramework", "Save", profile.Scope.ToString())
                : profile.StorageRoot;

            return Path.GetFullPath(root);
        }

        private static string ResolveBackupDirectory(SaveRequest request, SaveProfile profile)
        {
            string dataDirectory = Path.GetDirectoryName(ResolveDataPath(request, profile)) ?? ResolveScopeRoot(profile);
            return Path.Combine(dataDirectory, ".backup", SanitizePathSegment(request.SaveKey));
        }

        private static string SanitizePathSegment(string segment)
        {
            string sanitized = segment ?? string.Empty;
            char[] invalidChars = Path.GetInvalidFileNameChars();
            for (int index = 0; index < invalidChars.Length; index++)
            {
                sanitized = sanitized.Replace(invalidChars[index], '_');
            }

            return sanitized;
        }

        private static SaveDataEnvelope TryReadEnvelope(string path)
        {
            try
            {
                return SaveJsonUtility.DeserializeEnvelope(File.ReadAllText(path));
            }
            catch
            {
                return null;
            }
        }

        private static string RestoreSaveKeyFromPath(string rootPath, string path, string format)
        {
            string relativePath = path.Substring(rootPath.Length).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            string extension = $".{format}";
            if (relativePath.EndsWith(extension, StringComparison.OrdinalIgnoreCase))
            {
                relativePath = relativePath.Substring(0, relativePath.Length - extension.Length);
            }

            return relativePath.Replace(Path.DirectorySeparatorChar, '.').Replace(Path.AltDirectorySeparatorChar, '.');
        }
    }
}
