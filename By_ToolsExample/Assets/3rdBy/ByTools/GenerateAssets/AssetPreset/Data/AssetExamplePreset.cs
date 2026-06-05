namespace _3rdBy.ByTools.GenerateAssets.AssetPreset.Data
{
    using System;
    using UnityEngine;

    /// <summary>
    ///  资产数据类示例。
    /// </summary>
    [Serializable]
    public class AssetExampleData
    {
        [Header("id")] public int id;
        [Header("描述")] public string description;
        [Header("名称")] public string name;
        [Header("价格")] public float price;
    }

    /// <summary>
    ///  资产预设类示例。
    /// </summary>
    public class AssetExamplePreset : ScriptableObject
    {
        public AssetExampleData presets;
    }
}