namespace UI.UILoginTest
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;
    using MetaFramework.UI;
    using TMPro;
    
    /// <summary>
    /// 用户界面UI元素
    /// </summary>
    public class UIViewLoginTest : IUIView
    {
    	public Image imgBg;

        public void Init(GameObject go)
        {
    		imgBg = go.transform.Find("Img_Bg").GetComponent<Image>();
        }
    }
}