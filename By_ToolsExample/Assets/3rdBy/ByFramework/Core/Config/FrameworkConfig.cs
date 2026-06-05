//=====================================================
// 文件名称: FrameworkConfig.cs
// 创 建 者: wangbaiyan
// 创建日期: 2026-06-05
// 描    述: 定义 ByFramework Runtime 全局配置结构。
//=====================================================

namespace _3rdBy.ByFramework.Core.Config
{
    using System;
    using UnityEngine;

    /// <summary>
    /// 框架模块开关配置。
    /// </summary>
    [Serializable]
    public sealed class ModuleSettings
    {
        [Header("是否启用 Guide 模块")]
        public bool enableGuide = true;
    }

    /// <summary>
    /// UI Runtime 配置。
    /// </summary>
    [Serializable]
    public sealed class UISettings
    {
        [Header("UI 预制体的 Resources 加载路径格式")]
        public string resourcesPrefabPath = "Prefab/UI/{0}";
    }

    /// <summary>
    /// 网络 Runtime 配置。
    /// </summary>
    [Serializable]
    public sealed class NetworkSettings
    {
        [Header("Socket 服务地址")]
        public string socketAddress = "192.168.0.106";

        [Header("Socket 服务端口")]
        public int socketPort = 7788;
    }

    /// <summary>
    /// 下载 Runtime 配置。
    /// </summary>
    [Serializable]
    public sealed class DownloadSettings
    {
        [Header("Application.persistentDataPath 下的下载目录名称")]
        public string downloadDirectoryName = "Downloads";
    }

    /// <summary>
    /// ByFramework Runtime 统一配置资源。
    /// 第一阶段仅建立配置结构，不改变现有模块的配置读取行为。
    /// </summary>
    [CreateAssetMenu(fileName = "FrameworkConfig", menuName = "ByFramework/Framework Config")]
    public sealed class FrameworkConfig : ScriptableObject
    {
        [Header("模块开关配置")]
        public ModuleSettings moduleSettings = new();

        [Header("UI Runtime 配置")]
        public UISettings uiSettings = new();

        [Header("网络 Runtime 配置")]
        public NetworkSettings networkSettings = new();

        [Header("下载 Runtime 配置")]
        public DownloadSettings downloadSettings = new();
    }
}
