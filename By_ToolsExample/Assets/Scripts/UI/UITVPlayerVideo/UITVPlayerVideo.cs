using UnityEngine;
using _3rdBy.MetaFramework.UI;

/// <summary>
/// 负责接收用户界面UI元素的操作逻辑
/// </summary>
public class UITVPlayerVideo : UIBase<UIModelTVPlayerVideo, UIViewTVPlayerVideo>
{
    public override UILayer GetLayer()
    {
        return UILayer.Center;
    }

    public override void OnInit()
    {
    }

    public override void OnEnter(params object[] args)
    {
        InitLoader();
    }

    public override void OnExit()
    {
    }

    private void InitLoader()
    {
        var dataList = ExcelTVConfigTable.Instance.dataList;
        /*foreach (var data in dataList)
        {
            var log = $"{data.Id}\n" +
                      $"{data.Title}\n" +
                      $"{data.name}\n" +
                      $"{data.url}\n" +
                      $"{data.detailUrl}\n" +
                      $"{data.isEnabled}";
            Debug.Log(log);
        }*/

        GetView().evVideoSource.InitContent(dataList);
    }
}