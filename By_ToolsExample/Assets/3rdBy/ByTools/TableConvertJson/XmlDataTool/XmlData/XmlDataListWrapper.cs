namespace _3rdBy.ByTools.TableConvertJson.XmlDataTool.XmlData
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// 表示所需的JSON结构
    /// </summary>
    [Serializable]
    public class XmlDataListWrapper<T> where T : XmlObject
    {
        public List<T> dataList;
    }
}