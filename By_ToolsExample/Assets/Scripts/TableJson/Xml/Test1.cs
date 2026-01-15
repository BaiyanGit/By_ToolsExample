using System;
using System.Collections.Generic;
using _3rdBy.UniTools.XMLDataTool.XmlData;
using UnityEngine;

#region 数据结构


[Serializable]
public class XmlTest1_Col 
{
    public string StartIdx;
    public List<string> Value; 
}

[Serializable]
public class XmlTest1_TeleCodes 
{
    public List<string> Code; 
}

[Serializable]
public class XmlTest1_RopeN 
{
    public List<string> Value; 
}

[Serializable]
public class XmlTest1_TelescopeCode 
{
    public List<string> Value; 
}

[Serializable]
public class XmlTest1_Table 
{
    public XmlTest1_Col Col;
    public XmlTest1_TeleCodes TeleCodes;
    public XmlTest1_RopeN ropeN;
    public XmlTest1_TelescopeCode telescopeCode; 
}


[Serializable]
public partial class Test1Data : XmlObject, IXmlData
{
    public string ID;
    public string Text;
    public string displayid;
    public XmlTest1_Table Table;
}

#endregion

#region 数据处理
public partial class Test1Table : XmlTable
{
    public static Test1Table Instance;

    public readonly List<Test1Data> dataList;
    private readonly Dictionary<int, Test1Data> _dataDict = new ();
    
    public Test1Table()
    {
        Instance = this;
    }
    /// <summary>
    /// 通过索引获取数据
    /// </summary>
    /// <param name="index">数据索引</param>
    /// <returns>返回通过索引找到的数据</returns>
    public Test1Data Get(int index)
    {
        _dataDict.TryGetValue(index, out var value);
        if (value != null) return value;
        Debug.LogError($"未找到相关数据: xml={nameof(Test1Data)} index={index}");
        return null;
    }
    
    public Dictionary<int, Test1Data> GetAll()
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