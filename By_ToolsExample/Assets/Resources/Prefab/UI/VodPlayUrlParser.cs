using System;
using System.Collections.Generic;
using System.Linq;

public static class VodPlayUrlParser
{
    /// <summary>
    /// 解析 vod_play_url
    /// </summary>
    /// <param name="vodPlayUrl">vod.vod_play_url</param>
    /// <param name="vodPlayFrom">vod.vod_play_from</param>
    /// <returns>线路字典</returns>
    public static Dictionary<string, List<VideoEpisode>> Parse(string vodPlayUrl, string vodPlayFrom)
    {
        var result = new Dictionary<string, List<VideoEpisode>>();

        if (string.IsNullOrEmpty(vodPlayUrl))
            return result;

        // 先分线路
        // 线路分隔符：$$$
        var lines = vodPlayUrl.Split(new string[] { "$$$" }, StringSplitOptions.RemoveEmptyEntries);

        // vodPlayFrom 对应的线路列表（可选）
        var fromLines = !string.IsNullOrEmpty(vodPlayFrom)
            ? vodPlayFrom.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
            : null;

        for (int i = 0; i < lines.Length; i++)
        {
            string lineName = fromLines != null && i < fromLines.Length ? fromLines[i] : $"线路{i + 1}";
            var episodes = new List<VideoEpisode>();

            // 每集用 # 分割
            var eps = lines[i].Split(new char[] { '#' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var ep in eps)
            {
                // 名称与地址用 $ 分割
                var parts = ep.Split(new char[] { '$' }, 2, StringSplitOptions.None);
                if (parts.Length == 2)
                {
                    episodes.Add(new VideoEpisode
                    {
                        Name = parts[0],
                        Url = parts[1]
                    });
                }
                else
                {
                    // 没有集名，直接用地址
                    episodes.Add(new VideoEpisode
                    {
                        Name = $"集{episodes.Count + 1}",
                        Url = parts[0]
                    });
                }
            }

            result[lineName] = episodes;
        }

        return result;
    }
}