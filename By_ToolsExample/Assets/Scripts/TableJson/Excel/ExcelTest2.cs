using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using XFramework.ExcelData;
using Cysharp.Threading.Tasks;

/// <summary>
/// Auto Generate Class!!!
/// </summary>
[System.Serializable]
public partial class ExcelTest2 : ExcelObject, IExcelData
{
    public int Id { get; set; }

	/// <summary>
	/// 提示内容
	/// </summary>
	public string Prompt { get; set; }


}

/// <summary>
/// Auto Generate Class!!!
/// </summary>
public partial class ExcelTest2Table : ExcelTable
{
    public static ExcelTest2Table Instance;
    
    public readonly List<ExcelTest2> dataList;
    private Dictionary<int, ExcelTest2> _dataDict = new Dictionary<int, ExcelTest2>();

    public ExcelTest2Table()
    {
        Instance = this;
    }

    public ExcelTest2 Get(int id)
    {
        _dataDict.TryGetValue(id, out ExcelTest2 value);
        if (value == null)
        {
            Debug.LogError($"配置找不到，配置表名: {nameof(ExcelTest2)}，配置id: {id}");
        }
        return value;
    }
    
    public bool Contain(int id)
    {
        return _dataDict.ContainsKey(id);
    }
    
    public Dictionary<int, ExcelTest2> GetAll()
    {
        return _dataDict;
    }
    
    public override void EndInit()
    {
        foreach (var edItemBase in dataList)
        {
            edItemBase.EndInit();
            _dataDict.Add(edItemBase.Id, edItemBase);
        }
        AfterEndInit();
    }
}