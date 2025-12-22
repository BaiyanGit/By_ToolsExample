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
public partial class MonsterConfig : ExcelObject, IExcelData
{
    public int Id { get; set; }

	/// <summary>
	/// 名字
	/// </summary>
	public string Name { get; set; }

	/// <summary>
	/// 攻击
	/// </summary>
	public int Attack { get; set; }

	/// <summary>
	/// 生命值
	/// </summary>
	public int Health { get; set; }

	/// <summary>
	/// 速度
	/// </summary>
	public float Speed { get; set; }

	/// <summary>
	/// 为Boss?
	/// </summary>
	public bool IsBoss { get; set; }

	/// <summary>
	/// 属性值
	/// </summary>
	public float[] Param { get; set; }

	/// <summary>
	/// LevelId
	/// </summary>
	public int LevelId { get; set; }

	/// <summary>
	/// Array2
	/// </summary>
	public int[][] Array2 { get; set; }


}

/// <summary>
/// Auto Generate Class!!!
/// </summary>
public partial class MonsterConfigTable : ExcelTable
{
    public static MonsterConfigTable Instance;
    
    public readonly List<MonsterConfig> dataList;
    private Dictionary<int, MonsterConfig> _dataDict = new Dictionary<int, MonsterConfig>();

    public MonsterConfigTable()
    {
        Instance = this;
    }

    public MonsterConfig Get(int id)
    {
        _dataDict.TryGetValue(id, out MonsterConfig value);
        if (value == null)
        {
            Debug.LogError($"配置找不到，配置表名: {nameof(MonsterConfig)}，配置id: {id}");
        }
        return value;
    }
    
    public bool Contain(int id)
    {
        return _dataDict.ContainsKey(id);
    }
    
    public Dictionary<int, MonsterConfig> GetAll()
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