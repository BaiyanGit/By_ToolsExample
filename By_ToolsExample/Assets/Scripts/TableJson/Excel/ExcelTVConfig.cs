using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using _3rdBy.ByTools.TableConvertJson.ExcelDataTool.ExcelData;
using Cysharp.Threading.Tasks;

#region 数据结构
[Serializable]
public partial class ExcelTVConfig : ExcelObject, IExcelData
{
    public int Id { get; set; }

	/// <summary>
	/// 标题
	/// </summary>
	public string Title { get; set; }

	/// <summary>
	/// 名字
	/// </summary>
	public string name { get; set; }

	/// <summary>
	/// 地址
	/// </summary>
	public string url { get; set; }

	/// <summary>
	/// 详情链接
	/// </summary>
	public string detailUrl { get; set; }

	/// <summary>
	/// 是否启用
	/// </summary>
	public bool isEnabled { get; set; }


}

#endregion

#region 数据处理
public partial class ExcelTVConfigTable : ExcelTable
{
    public static ExcelTVConfigTable Instance;
    
    public readonly List<ExcelTVConfig> dataList;
    private Dictionary<int, ExcelTVConfig> _dataDict = new Dictionary<int, ExcelTVConfig>();

    /// <summary>
    /// 构造函数
    /// </summary>
    public ExcelTVConfigTable()
    {
        Instance = this;
    }

    /// <summary>
    /// 通过索引获取数据
    /// </summary>
    /// <param name="index">数据索引</param>
    /// <returns>返回通过索引找到的数据</returns>
    public ExcelTVConfig Get(int id)
    {
        _dataDict.TryGetValue(id, out ExcelTVConfig value);
        if (value == null)
        {
            Debug.LogError($"配置找不到，配置表名: {nameof(ExcelTVConfig)}，配置id: {id}");
        }
        return value;
    }
    
    /// <summary>
    /// 是否包含数据
    /// </summary>
    /// <param name="id">数据索引</param>
    /// <returns>返回是否包含数据</returns>
    public bool Contain(int id)
    {
        return _dataDict.ContainsKey(id);
    }
    
    /// <summary>
    /// 获取所有数据
    /// </summary>
    /// <returns>返回所有数据</returns>
    public Dictionary<int, ExcelTVConfig> GetAll()
    {
        return _dataDict;
    }
    
    /// <summary>
    /// 结束后初始化
    /// </summary>
    public override void EndInit()
    {
        foreach (var edItemBase in dataList)
        {
            edItemBase.EndInit();
            _dataDict.Add(edItemBase.Id, edItemBase);
        }
        // 初始化之后要做的处理
        AfterEndInit();
    }
}
#endregion