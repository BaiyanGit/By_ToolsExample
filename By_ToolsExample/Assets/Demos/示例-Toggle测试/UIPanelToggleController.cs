using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;


[Serializable]
public struct ToggleControlAttribute
{
    [Header("复选框")] public Toggle toggle;
    [Header("关联对象")] public GameObject objGo;

    public ToggleControlAttribute(Toggle tog, GameObject go)
    {
        toggle = tog;
        objGo = go;
    }
}

/// <summary>
/// 通用UI面板切换控制器
/// </summary>
public class UIPanelToggleController
{
    private List<ToggleControlAttribute> _togglePanelMap;

    public void Initialize(List<ToggleControlAttribute> togglePanelMap)
    {
        _togglePanelMap = togglePanelMap;
        foreach (var entry in _togglePanelMap)
        {
            entry.objGo.SetActive(false);
            entry.toggle.onValueChanged.AddListener(isOn => OnToggleSwitch(entry.toggle, isOn));
        }

        if (_togglePanelMap.Count == 0) return;
        _togglePanelMap.First().toggle.isOn = true; // 默认打开第一个
    }

    private void OnToggleSwitch(Toggle toggle, bool isOn)
    {
        var togStruct = _togglePanelMap.Find(t => t.toggle == toggle);
        togStruct.objGo.SetActive(isOn);
    }
}