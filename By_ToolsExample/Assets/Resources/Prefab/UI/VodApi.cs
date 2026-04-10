using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Cysharp.Threading.Tasks;
using LitJson;

public class VodDetailItem
{
    public int vod_id;
    public string vod_name;
    public string vod_play_from;
    public string vod_play_url;
}

public static class VodApi
{
    /*public static async UniTask<VodDetailItem> GetVodDetailAsync(string baseUrl, string vodId, float timeout = 10f)
    {
        string url = $"{baseUrl}?ac=detail&ids={vodId}";

        using var request = UnityWebRequest.Get(url);
        request.timeout = Mathf.CeilToInt(timeout);
        request.SetRequestHeader("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64)");
        request.SetRequestHeader("Referer", baseUrl);

        await request.SendWebRequest();

        if (!string.IsNullOrEmpty(request.error))
        {
            Debug.LogError($"获取视频详情失败: {request.error}");
            return null;
        }

        string json = request.downloadHandler.text;
        Debug.Log($"接口返回: {json}");

        try
        {
            // 用 LitJSON 解析
            JsonData jd = JsonMapper.ToObject(json);

            if (jd["list"] == null || jd["list"].Count == 0)
            {
                Debug.LogWarning($"接口返回为空或错误: {jd["msg"]}");
                return null;
            }

            var item = jd["list"][0];

            return new VodDetailItem
            {
                vod_id        = Convert.ToInt32(item["vod_id"]),
                vod_name      = item["vod_name"].ToString(),
                vod_play_from = item["vod_play_from"].ToString(),
                vod_play_url  = item["vod_play_url"].ToString()
            };
        }
        catch (Exception ex)
        {
            Debug.LogError($"解析视频详情失败: {ex.Message}");
            return null;
        }
    }*/

    public static async UniTask<VodDetailItem> GetVodDetailAsync(string baseUrl, string vodId, float timeout = 10f)
    {
        string url = $"{baseUrl}?ac=detail&ids={vodId}";

        using var request = UnityWebRequest.Get(url);
        request.timeout = Mathf.CeilToInt(timeout);
        request.SetRequestHeader("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64)");
        request.SetRequestHeader("Referer", baseUrl);

        await request.SendWebRequest();

        if (!string.IsNullOrEmpty(request.error))
        {
            Debug.LogError($"获取视频详情失败: {request.error}");
            return null;
        }

        string json = request.downloadHandler.text;
        Debug.Log($"接口返回: {json}");

        try
        {
            JsonData jd = JsonMapper.ToObject(json);

            if (jd["list"] == null || jd["list"].Count == 0)
            {
                Debug.LogWarning($"接口返回为空或错误: {jd["msg"]}");
                return null;
            }

            var item = jd["list"][0];

            int vodIdParsed = 0;
            int.TryParse(item["vod_id"].ToString(), out vodIdParsed);

            return new VodDetailItem
            {
                vod_id        = vodIdParsed,
                vod_name      = item["vod_name"]?.ToString(),
                vod_play_from = item["vod_play_from"]?.ToString(),
                vod_play_url  = item["vod_play_url"]?.ToString()
            };
        }
        catch (Exception ex)
        {
            Debug.LogError($"解析视频详情失败: {ex.Message}");
            return null;
        }
    }
}