//=====================================================
// 文件名称: CustomAssetGenerator
// 创 建 者: wangbaiyan
// 创建日期: 2026-4-17
// 描    述: 示例自定义资源生成器
//=====================================================

using System.Collections.Generic;
using UnityEditor;

[InitializeOnLoad]
public class AssetExampleSplitter : AssetSplitGenerator<AssetExamplePreset>
{
    /// <summary>
    /// 触发基类构造函数⚠️必须存在
    /// </summary>
    private static AssetExampleSplitter _instance = new();

    protected override IEnumerable<object> GetSplitDataList()
    {
        return new List<AssetExampleData>
        {
            new() { id = 0, description = "示例_1", name = "[插件] 资源生成示例_1.0.0", price = 100 },
            new() { id = 1, description = "示例_2", name = "[插件] 资源生成示例_1.0.1", price = 200 },
            new() { id = 2, description = "示例_3", name = "[插件] 资源生成示例_1.0.2", price = 300 }
        };
    }

    protected override string GetAssetName(object dataItem)
    {
        var data = dataItem as AssetExampleData;
        return data?.description ?? "item";
    }

    protected override void FillAsset(AssetExamplePreset asset, object dataItem)
    {
        if (dataItem is AssetExampleData data)
        {
            // 假设 AssetExamplePreset 有一个 presets 字段
            asset.presets = data; // 或者 asset.presets = new List<AssetExampleData>{ data };
        }
    }
}