namespace _3rdBy.MetaFramework.UI.Editor.UIAutoCreate
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    [Serializable]
    public class UIViewAutoCreateInfo
    {
        /// <summary>
        /// 属性名
        /// </summary>
        public string propName;

        /// <summary>
        /// 组件名
        /// </summary>
        public string comName;
    }
    [Serializable, CreateAssetMenu(menuName = "UI/CreateUIViewAutoCreateConfig")]
    public class UIViewAutoCreateConfig : ScriptableObject
    {
        public List<UIViewAutoCreateInfo> uiInfoList;
    }
}
