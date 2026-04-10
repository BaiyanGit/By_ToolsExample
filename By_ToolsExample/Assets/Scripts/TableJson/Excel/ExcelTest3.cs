using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using _3rdBy.ByTools.TableConvertJson.ExcelDataTool.ExcelData;
using Cysharp.Threading.Tasks;

#region 数据结构
[Serializable]
public partial class ExcelTest3 : ExcelObject, IExcelData
{
    public int Id { get; set; }

	/// <summary>
	/// 提示内容
	/// </summary>
	public string Prompt { get; set; }


}

#endregion

#region 数据处理
public partial class ExcelTest3Table : ExcelTable
{
    public static ExcelTest3Table Instance;
    
    public readonly List<ExcelTest3> dataList;
    private Dictionary<int, ExcelTest3> _dataDict = new Dictionary<int, ExcelTest3>();

    /// <summary>
    /// 构造函数
    /// </summary>
    public ExcelTest3Table()
    {
        Instance = this;
    }

    /// <summary>
    /// 通过索引获取数据
    /// </summary>
    /// <param name="index">数据索引</param>
    /// <returns>返回通过索引找到的数据</returns>
    public ExcelTest3 Get(int id)
    {
        _dataDict.TryGetValue(id, out ExcelTest3 value);
        if (value == null)
        {
            Debug.LogError($"配置找不到，配置表名: {nameof(ExcelTest3)}，配置id: {id}");
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
    public Dictionary<int, ExcelTest3> GetAll()
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