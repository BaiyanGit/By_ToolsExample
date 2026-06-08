//=====================================================
// 文件名称: ByFrameworkPathUtility.cs
// 创 建 者: wangbaiyan
// 创建日期: 2026-06-04
// 描    述: ByFramework Editor 路径查找工具，避免依赖固定安装目录。
//=====================================================

namespace _3rdBy.ByFramework.Editor
{
    using System;
    using System.IO;
    using UnityEditor;

    /// <summary>
    /// ByFramework Editor 路径查找工具。
    /// 通过框架标记文件自动定位根目录，避免工具依赖固定安装路径。
    /// </summary>
    public static class ByFrameworkPathUtility
    {
        /// <summary>
        /// 缓存已定位的框架根目录。该类是 Editor 纯逻辑工具，不引入 UnityEngine HeaderAttribute。
        /// </summary>
        private static string _cachedFrameworkRootPath;

        /// <summary>
        /// 获取 ByFramework 框架根目录。
        /// </summary>
        /// <returns>Unity AssetDatabase 可识别的框架根目录路径。</returns>
        /// <exception cref="DirectoryNotFoundException">无法通过标记文件定位框架根目录时抛出。</exception>
        public static string GetFrameworkRootPath()
        {
            if (string.IsNullOrEmpty(_cachedFrameworkRootPath) == false
                && IsFrameworkRoot(_cachedFrameworkRootPath))
            {
                return _cachedFrameworkRootPath;
            }

            // 框架目录可能在同一次 Editor 会话中被移动，缓存失效后必须重新扫描。
            _cachedFrameworkRootPath = string.Empty;
            _cachedFrameworkRootPath = FindFrameworkRootPath();
            return _cachedFrameworkRootPath;
        }

        /// <summary>
        /// 获取 Documentation 目录路径。
        /// </summary>
        /// <returns>Documentation 目录的 AssetDatabase 路径。</returns>
        public static string GetDocumentationPath()
        {
            return CombineAssetPath(GetFrameworkRootPath(), "Documentation");
        }

        /// <summary>
        /// 获取 Editor 目录路径。
        /// </summary>
        /// <returns>Editor 目录的 AssetDatabase 路径。</returns>
        public static string GetEditorPath()
        {
            return CombineAssetPath(GetFrameworkRootPath(), "Editor");
        }

        /// <summary>
        /// 获取 Runtime 目录路径。
        /// </summary>
        /// <returns>Runtime 目录的 AssetDatabase 路径。</returns>
        /// <exception cref="DirectoryNotFoundException">顶层 Runtime 目录不存在时抛出。</exception>
        public static string GetRuntimePath()
        {
            var runtimePath = CombineAssetPath(GetFrameworkRootPath(), "Runtime");
            if (AssetDatabase.IsValidFolder(runtimePath))
            {
                return runtimePath;
            }

            throw new DirectoryNotFoundException(
                $"ByFramework Runtime 目录不存在：{runtimePath}。请确认当前框架目录结构，或避免调用 GetRuntimePath()。");
        }

        /// <summary>
        /// 获取 Samples 目录路径。
        /// </summary>
        /// <returns>Samples 目录的 AssetDatabase 路径。</returns>
        /// <exception cref="DirectoryNotFoundException">顶层 Samples 目录不存在时抛出。</exception>
        public static string GetSamplesPath()
        {
            var samplesPath = CombineAssetPath(GetFrameworkRootPath(), "Samples");
            if (AssetDatabase.IsValidFolder(samplesPath))
            {
                return samplesPath;
            }

            throw new DirectoryNotFoundException(
                $"ByFramework 顶层 Samples 目录不存在：{samplesPath}。当前示例可能位于具体模块的 Samples 目录中。");
        }

        /// <summary>
        /// 获取 README 文件路径。
        /// </summary>
        /// <returns>README.md 文件的 AssetDatabase 路径。</returns>
        public static string GetREADMEPath()
        {
            return CombineAssetPath(GetFrameworkRootPath(), "README.md");
        }

        /// <summary>
        /// 拼接 AssetDatabase 路径。
        /// </summary>
        /// <param name="basePath">基础路径。</param>
        /// <param name="relativePath">相对路径。</param>
        /// <returns>使用正斜杠分隔的 AssetDatabase 路径。</returns>
        public static string CombineAssetPath(string basePath, string relativePath)
        {
            if (string.IsNullOrEmpty(basePath))
            {
                return NormalizeAssetPath(relativePath);
            }

            if (string.IsNullOrEmpty(relativePath))
            {
                return NormalizeAssetPath(basePath);
            }

            return NormalizeAssetPath($"{basePath.TrimEnd('/')}/{relativePath.TrimStart('/')}");
        }

        /// <summary>
        /// 将 AssetDatabase 路径转换为本机完整文件系统路径。
        /// </summary>
        /// <param name="assetPath">AssetDatabase 路径。</param>
        /// <returns>本机完整文件系统路径。</returns>
        public static string ToFullPath(string assetPath)
        {
            return Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), assetPath));
        }

        private static string FindFrameworkRootPath()
        {
            if (TryFindByAgentFile(out var agentRootPath))
            {
                return agentRootPath;
            }

            if (TryFindByReadmeFile(out var readmeRootPath))
            {
                return readmeRootPath;
            }

            if (TryFindByArchitectureFile(out var architectureRootPath))
            {
                return architectureRootPath;
            }

            throw new DirectoryNotFoundException("无法定位 ByFramework 根目录，请确认 AGENTS.md、README.md 与 Documentation/01_Architecture.md 位于框架根目录结构中。");
        }

        private static bool TryFindByAgentFile(out string rootPath)
        {
            foreach (var path in FindAssetPaths("AGENTS"))
            {
                if (Path.GetFileName(path) != "AGENTS.md")
                {
                    continue;
                }

                var candidateRoot = NormalizeAssetPath(Path.GetDirectoryName(path));
                if (IsFrameworkRoot(candidateRoot))
                {
                    rootPath = candidateRoot;
                    return true;
                }
            }

            rootPath = string.Empty;
            return false;
        }

        private static bool TryFindByReadmeFile(out string rootPath)
        {
            foreach (var path in FindAssetPaths("README"))
            {
                if (Path.GetFileName(path) != "README.md")
                {
                    continue;
                }

                var candidateRoot = NormalizeAssetPath(Path.GetDirectoryName(path));
                if (IsFrameworkRoot(candidateRoot))
                {
                    rootPath = candidateRoot;
                    return true;
                }
            }

            rootPath = string.Empty;
            return false;
        }

        private static bool TryFindByArchitectureFile(out string rootPath)
        {
            foreach (var path in FindAssetPaths("Architecture"))
            {
                var normalizedPath = NormalizeAssetPath(path);
                if (normalizedPath.EndsWith("/Documentation/01_Architecture.md", StringComparison.Ordinal) == false)
                {
                    continue;
                }

                var documentationPath = NormalizeAssetPath(Path.GetDirectoryName(normalizedPath));
                var candidateRoot = NormalizeAssetPath(Path.GetDirectoryName(documentationPath));
                if (IsFrameworkRoot(candidateRoot))
                {
                    rootPath = candidateRoot;
                    return true;
                }
            }

            rootPath = string.Empty;
            return false;
        }

        private static string[] FindAssetPaths(string filter)
        {
            var guids = AssetDatabase.FindAssets(filter);
            var paths = new string[guids.Length];
            for (var i = 0; i < guids.Length; i++)
            {
                paths[i] = NormalizeAssetPath(AssetDatabase.GUIDToAssetPath(guids[i]));
            }

            return paths;
        }

        private static bool IsFrameworkRoot(string rootPath)
        {
            return File.Exists(ToFullPath(CombineAssetPath(rootPath, "AGENTS.md")))
                   && File.Exists(ToFullPath(CombineAssetPath(rootPath, "README.md")))
                   && File.Exists(ToFullPath(CombineAssetPath(rootPath, "Documentation/01_Architecture.md")));
        }

        private static string NormalizeAssetPath(string path)
        {
            return string.IsNullOrEmpty(path) ? string.Empty : path.Replace('\\', '/').TrimEnd('/');
        }
    }
}
