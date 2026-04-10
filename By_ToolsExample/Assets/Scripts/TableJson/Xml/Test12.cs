using System;
using System.Collections.Generic;
using _3rdBy.ByTools.TableConvertJson.XmlDataTool.XmlData;
using UnityEngine;

#region 数据结构


[Serializable]
public class XmlTest12_Col 
{
    public string StartIdx;
    public List<string> Value; 
}

[Serializable]
public class XmlTest12_TeleCodes 
{
    public List<string> Code; 
}

[Serializable]
public class XmlTest12_RopeN 
{
    public List<string> Value; 
}

[Serializable]
public class XmlTest12_TelescopeCode 
{
    public List<string> Value; 
}

[Serializable]
public class XmlTest12_Table 
{
    public XmlTest12_Col Col;
    public XmlTest12_TeleCodes TeleCodes;
    public XmlTest12_RopeN ropeN;
    public XmlTest12_TelescopeCode telescopeCode; 
}


[Serializable]
public partial class Test12Data : XmlObject, IXmlData
{
    public string ID;
    public string Text;
    public string displayid;
    public XmlTest12_Table Table;
}

#endregion

#region 数据处理
public partial class Test12Table : XmlTable
{
    public static Test12Table Instance;

    public readonly List<Test12Data> dataList;
    private readonly Dictionary<int, Test12Data> _dataDict = new ();
    
    /// <summary>
    /// 构造函数
    /// </summary>
    public Test12Table()
    {
        Instance = this;
    }
    
    /// <summary>
    /// 通过索引获取数据
    /// </summary>
    /// <param name="index">数据索引</param>
    /// <returns>返回通过索引找到的数据</returns>
    public Test12Data Get(int index)
    {
        _dataDict.TryGetValue(index, out var value);
        if (value != null) return value;
        Debug.LogError($"未找到相关数据: xml={nameof(Test12Data)} index={index}");
        return null;
    }
    
    /// <summary>
    /// 获取所有数据
    /// </summary>
    public Dictionary<int, Test12Data> GetAll()
    {
        return _dataDict;
    }
     
    /// <summary>
    /// 结束后初始化
    /// </summary>
    public override void EndInit()
    {
        if (dataList != null)
        {
            for (var i = 0; i < dataList.Count; i++)
            {
                dataList[i].EndInit();
                _dataDict.Add(i, dataList[i]);
            }
        }
 
        // 初始化之后要做的处理
        AfterEndInit();
   }
}
#endregion