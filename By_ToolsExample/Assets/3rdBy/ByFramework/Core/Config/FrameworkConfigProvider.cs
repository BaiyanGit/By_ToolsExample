//=====================================================
// 文件名称: FrameworkConfigProvider.cs
// 创 建 者: wangbaiyan
// 创建日期: 2026-06-05
// 描    述: 加载并提供 ByFramework Runtime 全局配置。
//=====================================================

namespace _3rdBy.ByFramework.Core.Config
{
    using UnityEngine;

    /// <summary>
    /// FrameworkConfig 全局访问入口。
    /// </summary>
    public static class FrameworkConfigProvider
    {
        /// <summary>
        /// Runtime 配置在 Resources 中的加载路径。静态配置提供器不引入 Unity HeaderAttribute。
        /// </summary>
        private const string ConfigResourcePath = "FrameworkConfig";

        /// <summary>
        /// 缓存已加载的 Runtime 配置。静态配置提供器不引入 Unity HeaderAttribute。
        /// </summary>
        private static FrameworkConfig _config;

        /// <summary>
        /// 获取 Runtime 全局配置。
        /// 首次访问时从 Resources 加载，资源缺失时返回仅供本次运行使用的默认配置。
        /// </summary>
        public static FrameworkConfig Config
        {
            get
            {
                if (_config == null)
                {
                    _config = LoadConfig();
                }

                return _config;
            }
        }

        private static FrameworkConfig LoadConfig()
        {
            FrameworkConfig config = Resources.Load<FrameworkConfig>(ConfigResourcePath);
            if (config != null)
            {
                return config;
            }

            Debug.LogWarning(
                $"[ByFramework][FrameworkConfigProvider] 无法从 Resources/{ConfigResourcePath} 加载配置，"
                + "本次运行将使用内存默认配置。请确认 FrameworkConfig.asset 位于 Resources 目录中。");
            return ScriptableObject.CreateInstance<FrameworkConfig>();
        }
    }
}
