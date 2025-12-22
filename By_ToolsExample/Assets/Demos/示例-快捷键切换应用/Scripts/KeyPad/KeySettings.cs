using System;
using System.Collections.Generic;
using System.Linq;
using KeyPad;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class KeySettings : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown sDropdown1;
    [SerializeField] private TMP_Dropdown sDropdown2;

    /// <summary>
    /// 第一个键
    /// </summary>
    public int key1;

    /// <summary>
    /// 第二个键
    /// </summary>
    public int key2;

    private void Awake()
    {
        var keys = KeyCodeName.keyCodeDic.Keys.ToList();
        var optionDatas = keys.Select(key => new TMP_Dropdown.OptionData { text = key }).ToList();
        sDropdown1.options = optionDatas;
        sDropdown2.options = optionDatas;

        sDropdown1.onValueChanged.AddListener(ShortcutKey1);
        sDropdown2.onValueChanged.AddListener(ShortcutKey2);
    }

    /// <summary>
    /// 快捷键1
    /// </summary>
    /// <param name="index"></param>
    private void ShortcutKey1(int index)
    {
        var str = sDropdown1.options[index].text;
        key1 = KeyCodeName.keyCodeDic[str];
    }

    /// <summary>
    /// 快捷键2
    /// </summary>
    /// <param name="index"></param>
    private void ShortcutKey2(int index)
    {
        var str = sDropdown2.options[index].text;
        key2 = KeyCodeName.keyCodeDic[str];
    }
}