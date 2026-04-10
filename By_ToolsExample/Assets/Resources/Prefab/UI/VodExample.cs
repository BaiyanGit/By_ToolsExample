using UnityEngine;

public class VodExample : MonoBehaviour
{
    public string detailApiUrl = "https://api.ukuapi88.com/api.php/provide/vod";
    public string vodId = "37657"; // 视频ID

    private async void Start()
    {
        var detail = await VodApi.GetVodDetailAsync(detailApiUrl, vodId);

        if (detail != null)
        {
            Debug.LogError($"vod_play_from: {detail.vod_play_from}");
            Debug.LogError($"vod_play_url: {detail.vod_play_url}");

            // 解析播放地址
            var sources = VodPlayUrlParser.Parse(detail.vod_play_url, detail.vod_play_from);

            foreach (var line in sources)
            {
                Debug.LogError($"线路: {line.Key}");
                foreach (var ep in line.Value)
                {
                    Debug.LogError($"  {ep.Name} -> {ep.Url}");
                }
            }
        }
        else
        {
            Debug.LogError("获取视频详情失败");
        }
    }
}