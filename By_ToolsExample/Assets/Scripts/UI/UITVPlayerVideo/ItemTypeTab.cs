using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;
using UnityEngine.Video;

//=====================================================
// 文件名称: ItemTypeTab
// 创 建 者: 
// 创建日期: 
// 描    述: 
//=====================================================


public class ItemTypeTab : ExcelItemBase
{
    [SerializeField] private Text _label;
    [SerializeField] private VideoPlayer _videoPlayer;

    private ExcelTVConfig _config;

    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(OnClicked);
    }

    public override void InitData<T>(T data, UnityAction<T> clickAction)
    {
        if (data is not ExcelTVConfig config)
        {
            throw new InvalidCastException("ItemTypeTab初始化数据错误：数据不是ExcelTVConfig类型");
        }

        _config     = config;
        _label.text = _config.name;
    }

    private void OnClicked()
    {
        if (_config == null) return;

        Debug.Log($"{_config.name}");
        // Http访问

        string api     = "https://api.ukuapi88.com/api.php/provide/vod";
        string keyword = "长津湖";

        string requestUrl = $"{api}?wd={UnityEngine.Networking.UnityWebRequest.EscapeURL(keyword)}";

        Debug.Log(requestUrl);
    }
}