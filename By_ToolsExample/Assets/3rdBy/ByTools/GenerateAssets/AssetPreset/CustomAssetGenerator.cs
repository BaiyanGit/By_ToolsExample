//=====================================================
// 文件名称: CustomAssetGenerator
// 创 建 者: wangbaiyan
// 创建日期: 2026-4-17
// 描    述: 自定义资产生成器
//=====================================================

namespace _3rdBy.ByTools.GenerateAssets.AssetPreset
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// 自定义资产计划信息
    /// </summary>
    [Serializable]
    public class CustomAssetPlanInfo
    {
        [Header("资产唯一标识")] public string key;
        [Header("资产名称")] public string name;
    }

    public static class CustomAssetGenerator
    {
        /// <summary>
        /// 自定义生成器委托
        /// </summary>
        /// <param name="scriptType">脚本类型</param>
        /// <param name="scriptPath">原始脚本资产路径</param>
        /// <param name="outputFolder">用户指定的输出目录</param>
        /// <param name="assetPlans">当前待生成资产计划</param>
        /// <returns>返回生成的资产路径列表（可能多个）</returns>
        public delegate List<string> GenerateHandler(Type scriptType, string scriptPath, string outputFolder, List<CustomAssetPlanInfo> assetPlans);

        /// <summary>
        /// 自定义预览委托
        /// </summary>
        /// <param name="scriptType">脚本类型</param>
        /// <param name="scriptPath">原始脚本资产路径</param>
        /// <param name="baseAssetName">用户命名的资产基础名</param>
        /// <returns>返回预览的资产计划列表</returns>
        public delegate List<CustomAssetPlanInfo> PreviewHandler(Type scriptType, string scriptPath, string baseAssetName);

        /// <summary>
        /// 生成器信息
        /// </summary>
        private class HandlerInfo
        {
            public GenerateHandler generateHandler;
            public PreviewHandler previewHandler;
        }

        /// <summary>
        /// 自定义生成器字典
        /// </summary>
        private static readonly Dictionary<Type, HandlerInfo> handlers = new();

        /// <summary>
        /// 为指定的 ScriptableObject 类型注册自定义生成逻辑
        /// </summary>
        /// <param name="scriptType">脚本类型</param>
        /// <param name="generateHandler">自定义生成器委托</param>
        /// <param name="previewHandler">自定义预览委托</param>
        /// <exception cref="ArgumentException">类型必须继承 ScriptableObject</exception>
        public static void RegisterHandler(Type scriptType, GenerateHandler generateHandler, PreviewHandler previewHandler = null)
        {
            if (!scriptType.IsSubclassOf(typeof(ScriptableObject)))
                throw new ArgumentException("类型必须继承 ScriptableObject");

            // Debug.Log($"注册自定义生成器: {scriptType.Name} -> {generateHandler?.Method.DeclaringType?.Name}.{generateHandler?.Method.Name}");
            handlers[scriptType] = new HandlerInfo
            {
                generateHandler = generateHandler,
                previewHandler  = previewHandler
            };
        }

        /// <summary>
        /// 尝试调用自定义预览生成器
        /// </summary>
        /// <param name="scriptType">脚本类型</param>
        /// <param name="scriptPath">原始脚本资产路径</param>
        /// <param name="baseAssetName">用户命名的资产基础名</param>
        /// <returns>返回预览资产列表；若没有预览生成器则返回null</returns>
        public static List<CustomAssetPlanInfo> TryPreview(Type scriptType, string scriptPath, string baseAssetName)
        {
            if (handlers.TryGetValue(scriptType, out var handlerInfo) && handlerInfo.previewHandler != null)
                return handlerInfo.previewHandler(scriptType, scriptPath, baseAssetName);

            return null;
        }

        /// <summary>
        /// 尝试调用自定义生成器
        /// </summary>
        /// <param name="scriptType">脚本类型</param>
        /// <param name="scriptPath">原始脚本资产路径</param>
        /// <param name="outputFolder">用户指定的输出目录</param>
        /// <param name="assetPlans">当前待生成资产计划</param>
        /// <returns>返回生成的资产路径列表；若没有自定义生成器则返回null</returns>
        public static List<string> TryGenerate(Type scriptType, string scriptPath, string outputFolder, List<CustomAssetPlanInfo> assetPlans)
        {
            if (handlers.TryGetValue(scriptType, out var handlerInfo) && handlerInfo.generateHandler != null)
                return handlerInfo.generateHandler(scriptType, scriptPath, outputFolder, assetPlans);

            return null;
        }
    }
}