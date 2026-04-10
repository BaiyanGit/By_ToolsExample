using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using _3rdBy.MetaFramework.UI;
using TMPro;

/// <summary>
/// 用户界面UI元素
/// </summary>
public class UIViewTVPlayerVideo : IUIView
{
    public Image imgBg;
    public ExcelUIView evVideoSource;

    public void Init(GameObject go)
    {
        imgBg         = go.transform.Find("Img_Bg").GetComponent<Image>();
        evVideoSource = go.transform.Find("Img_Bg/Ev_VideoSource").GetComponent<ExcelUIView>();
    }
}