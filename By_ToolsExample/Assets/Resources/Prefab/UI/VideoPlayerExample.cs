using UnityEngine;
using UnityEngine.Video;
using System.Collections.Generic;
using System.Linq;

public class VideoPlayerExample : MonoBehaviour
{
    public VideoPlayer videoPlayer;

    // vodPlayUrl + vodPlayFrom 是从详情接口获取的
    public string vodPlayUrl;
    public string vodPlayFrom;

    private Dictionary<string, List<VideoEpisode>> sources;
    private string currentLine;
    private int currentEpisodeIndex;

    private void Start()
    {
        sources = VodPlayUrlParser.Parse(vodPlayUrl, vodPlayFrom);
        if (sources.Count > 0)
        {
            currentLine         = sources.Keys.First();
            currentEpisodeIndex = 0;
            PlayCurrent();
        }
    }

    public void PlayCurrent()
    {
        if (sources == null || !sources.ContainsKey(currentLine)) return;

        var episodes = sources[currentLine];
        if (currentEpisodeIndex >= episodes.Count) return;

        string url = episodes[currentEpisodeIndex].Url;
        videoPlayer.url = url;
        videoPlayer.Play();
        Debug.Log($"正在播放 {currentLine} - {episodes[currentEpisodeIndex].Name}: {url}");
    }

    public void NextEpisode()
    {
        if (sources[currentLine] == null) return;
        currentEpisodeIndex++;
        if (currentEpisodeIndex >= sources[currentLine].Count)
            currentEpisodeIndex = 0;
        PlayCurrent();
    }

    public void ChangeLine(string lineName)
    {
        if (!sources.ContainsKey(lineName)) return;
        currentLine         = lineName;
        currentEpisodeIndex = 0;
        PlayCurrent();
    }
}