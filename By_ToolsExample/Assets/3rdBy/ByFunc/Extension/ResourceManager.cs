using System.IO;
using System.Text;
using System.Threading;
using _3rdBy.MetaFramework.Extension;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Video;

/// <summary>
/// 通用加载对象
/// </summary>
public class ResourceManager
{
    private const string UIPath = "UI/";
    private const string XMLPath = "XML/";
    private const string PlayerPath = "Players/";
    private const string PrefabPath = "Prefabs/";
    private const string SpritePath = "Sprites/";
    private const string VideoPath = "Videos/";
    private const string AudioPath = "Audios/";

    public static T Inst<T>(GameObject obj, Transform parent = null) where T : Component
    {
        var objNew = Object.Instantiate(obj, parent);
        objNew.transform.localScale = Vector3.one;
        objNew.SetActive(true);
        return objNew.GetComponent<T>();
    }

    /// <summary>
    /// 本地资源加载通用方法1，所有本地资源都要通过它加载
    /// </summary>
    public static Object LoadAsset(string name, string path)
    {
        return LoadAsset<Object>(name, path);
    }

    /// <summary>
    /// 本地资源加载通用方法2，所有本地资源都要通过它加载
    /// </summary>
    public static T LoadAsset<T>(string name, string path) where T : Object
    {
        var obj = Resources.Load<T>(path + name);
        if (obj != null) return obj;
        Debug.LogWarning($"加载资源失败：{name}");
        return null;
    }

    public static GameObject LoadUI(string name)
    {
        var ui = LoadAsset(name, UIPath) as GameObject;
        if (ui != null) return ui;
        Debug.LogWarning($"加载UI失败：{name}");
        return null;
    }

    public static TextAsset LoadXMLAsset(string name)
    {
        var txt = LoadAsset(name, XMLPath) as TextAsset;
        if (txt != null) return txt;
        Debug.LogWarning($"加载XML失败：{name}");
        return null;
    }

    public static GameObject LoadPlayer(string name)
    {
        var player = LoadAsset(name, PlayerPath) as GameObject;
        if (player != null) return player;
        Debug.LogWarning($"加载玩家失败：{name}");
        return null;
    }

    public static GameObject LoadPrefab(string name)
    {
        var prefab = LoadAsset(name, PrefabPath) as GameObject;
        if (prefab != null) return prefab;
        Debug.LogWarning($"加载预制体失败：{name}");
        return null;
    }

    public static Sprite LoadSprite(string name)
    {
        var sprite = Resources.Load(SpritePath + name, typeof(Sprite)) as Sprite;
        if (sprite != null) return sprite;
        Debug.LogWarning($"加载图片失败：{name}");
        return null;
    }

    public static VideoClip LoadVideo(string name)
    {
        var videoClip = Resources.Load(VideoPath + name, typeof(VideoClip)) as VideoClip;
        if (videoClip != null) return videoClip;
        Debug.LogWarning($"加载视频失败：{name}");
        return null;
    }

    public static AudioClip LoadAudio(string name)
    {
        var audioClip = Resources.Load(AudioPath + name, typeof(AudioClip)) as AudioClip;
        if (audioClip != null) return audioClip;
        Debug.LogWarning($"加载音频失败：{name}");
        return null;
    }

    /// <summary>
    /// 写入Json数据并保存
    /// </summary>
    /// <param name="folder">StreamingAssetsPath路径相关文件夹</param>
    /// <param name="fileName">文件名称</param>
    /// <param name="content">文件内容</param>
    public static void WriteJsonFile(string folder, string fileName, string content)
    {
        var       filePath = $"{Application.streamingAssetsPath}/{folder}/{fileName}";
        using var fs       = new FileStream(filePath, FileMode.Create, FileAccess.Write);
        using var sw       = new StreamWriter(fs, Encoding.UTF8);
        sw.Write(content);
    }

    private static readonly SemaphoreSlim semaphore = new(1); //锁定只允许一个并发写入

    /// <summary>
    /// 写入Json数据并保存
    /// </summary>
    /// <param name="folder">StreamingAssetsPath路径相关文件夹</param>
    /// <param name="fileName">文件名称</param>
    /// <param name="content">文件内容</param>
    public static async UniTask WriteJsonFileAsync(string folder, string fileName, string content)
    {
        var filePath = $"{Application.streamingAssetsPath}/{folder}/{fileName}";
        try
        {
            await semaphore.WaitAsync();
            await using var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write);
            await using var sw = new StreamWriter(fs, Encoding.UTF8);
            await sw.WriteAsync(content);
        }
        finally
        {
            semaphore.Release();
        }
    }

    /// <summary>
    /// 主线程中异步写入，并发只支持1个
    /// </summary>
    /// <param name="folder"></param>
    /// <param name="fileName"></param>
    /// <param name="jsonData"></param>
    public static async UniTask WriteJsonFileLock(string folder, string fileName, string jsonData)
    {
        await semaphore.WaitAsync(); // 等待进入写入区域  
        try
        {
            await WriteJsonFileAsync(folder, fileName, jsonData);
        }
        finally
        {
            semaphore.Release(); // 释放写入区域  
        }
    }

    /// <summary>
    /// 子线程异步IO操作
    /// </summary>
    public static void WriteJsonFileAsyncThread(string folder, string fileName, string jsonData)
    {
        ThreadDispatcher.RunAsync(() => { _ = WriteJsonFileAsync(folder, fileName, jsonData); });
    }
}