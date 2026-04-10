using System.Collections.Generic;

public class VideoEpisode
{
    public string Name;
    public string Url;
}

public class VideoSource
{
    public string Line; // 线路名称
    public List<VideoEpisode> Episodes;
}