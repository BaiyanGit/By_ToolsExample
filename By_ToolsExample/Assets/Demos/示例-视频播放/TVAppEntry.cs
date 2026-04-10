using UnityEngine;

//=====================================================
// 文件名称: TVAppEntry
// 创 建 者: 
// 创建日期: 
// 描    述: 
//=====================================================


namespace Demos.示例_视频播放
{
    using _3rdBy.ByTools.TableConvertJson.ExcelDataTool;
    using _3rdBy.MetaFramework.UI;
    using Cysharp.Threading.Tasks;

    public class TVAppEntry : MonoBehaviour
    {
        private async void Start()
        {
            await LoadConfig();
        }

        private async UniTask LoadConfig()
        {
            // Tips: 加载配置
            await ExcelDataLoader.Instance.LoadStreamingAsync();
            // Tips: 打开UI
            await UIManager.Instance.OpenNormalAsync(nameof(UITVPlayerVideo));
        }
    }
}