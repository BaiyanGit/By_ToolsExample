using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 拆分生成器的抽象基类。
/// 子类只需提供数据源列表，即可自动将每个数据项生成为一个独立的 ScriptableObject 资产。
/// </summary>
/// <typeparam name="T">目标 ScriptableObject 类型</typeparam>
public abstract class AssetSplitGenerator<T> where T : ScriptableObject
{
    // 静态构造函数会在类型被访问时执行，但为了在 Unity 启动时自动注册，子类需要加上 [InitializeOnLoad]，并且子类必须new一个实例。
    protected AssetSplitGenerator()
    {
        // Debug.Log("初始化 注册");
        // 注册当前类型的生成器与预览器
        CustomAssetGenerator.RegisterHandler(typeof(T), GenerateSplitAssets, PreviewSplitAssets);
    }

    /// <summary>
    /// 子类需实现此方法，返回要拆分的数据列表。
    /// 每个数据项会单独生成一个 T 类型的资产。
    /// </summary>
    protected abstract IEnumerable<object> GetSplitDataList();

    /// <summary>
    /// 子类可选实现：根据数据项生成资产文件名（不含扩展名）。
    /// 默认使用数据项的 ToString()，建议覆盖以获得有意义的名称。
    /// </summary>
    protected virtual string GetAssetName(object dataItem)
    {
        return dataItem?.ToString() ?? "unnamed";
    }

    /// <summary>
    /// 子类可选实现：根据数据项生成唯一Key。
    /// 默认使用资产名称作为唯一标识。
    /// </summary>
    /// <param name="dataItem"></param>
    /// <returns></returns>
    protected virtual string GetAssetKey(object dataItem)
    {
        return GetAssetName(dataItem);
    }

    /// <summary>
    /// 子类可选实现：将数据项填充到新创建的 ScriptableObject 实例中。
    /// 默认不做任何填充（保留空对象）。
    /// </summary>
    protected virtual void FillAsset(T asset, object dataItem)
    {
        // 默认不填充，在子类中重写
    }

    /// <summary>
    /// 预览待生成的资产列表
    /// </summary>
    /// <param name="scriptType"></param>
    /// <param name="scriptPath"></param>
    /// <param name="baseAssetName"></param>
    /// <returns></returns>
    private List<CustomAssetPlanInfo> PreviewSplitAssets(Type scriptType, string scriptPath, string baseAssetName)
    {
        var dataList = GetSplitDataList();
        var previewList = new List<CustomAssetPlanInfo>();
        if (dataList == null) return previewList;

        foreach (var dataItem in dataList)
        {
            if (dataItem == null) continue;
            var itemName = GetAssetName(dataItem);
            previewList.Add(new CustomAssetPlanInfo
            {
                key  = GetAssetKey(dataItem),
                name = $"{baseAssetName}_{itemName}"
            });
        }

        return previewList;
    }

    /// <summary>
    /// 生成逻辑（私有，供委托调用）
    /// </summary>
    /// <param name="scriptType"></param>
    /// <param name="scriptPath"></param>
    /// <param name="outputFolder"></param>
    /// <param name="assetPlans"></param>
    /// <returns></returns>
    private List<string> GenerateSplitAssets(Type scriptType, string scriptPath, string outputFolder, List<CustomAssetPlanInfo> assetPlans)
    {
        var dataList = GetSplitDataList();
        if (dataList == null) return new List<string>();
        if (assetPlans == null || assetPlans.Count == 0) return new List<string>();

        if (!Directory.Exists(outputFolder))
            Directory.CreateDirectory(outputFolder);

        var assetPlanDict = new Dictionary<string, CustomAssetPlanInfo>();
        foreach (var assetPlan in assetPlans)
        {
            if (assetPlan == null || string.IsNullOrEmpty(assetPlan.key) || string.IsNullOrEmpty(assetPlan.name)) continue;
            assetPlanDict[assetPlan.key] = assetPlan;
        }

        var createdPaths = new List<string>();
        foreach (var dataItem in dataList)
        {
            if (dataItem == null) continue;
            var assetKey = GetAssetKey(dataItem);
            if (string.IsNullOrEmpty(assetKey)) continue;
            if (assetPlanDict.TryGetValue(assetKey, out var assetPlan) == false) continue;

            var asset = ScriptableObject.CreateInstance<T>();
            FillAsset(asset, dataItem);

            string fileName  = $"{Path.GetFileNameWithoutExtension(assetPlan.name)}.asset";
            string assetPath = Path.Combine(outputFolder, fileName).Replace("\\", "/");

            if (AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath) != null)
            {
                AssetDatabase.DeleteAsset(assetPath);
            }

            assetPath = AssetDatabase.GenerateUniqueAssetPath(assetPath);
            AssetDatabase.CreateAsset(asset, assetPath);
            createdPaths.Add(assetPath);
            Debug.Log($"[{GetType().Name}] 已创建: {assetPath}");
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        return createdPaths;
    }
}
