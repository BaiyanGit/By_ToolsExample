using UnityEngine;
using UnityEngine.UI;

public class MultiDisplayCapture : MonoBehaviour
{
    [Header("Display 0 Cameras")] public Camera worldCamera0;
    public Camera uiCamera0;

    [Header("Display 1 Cameras")] public Camera worldCamera1;
    public Camera uiCamera1;

    [Header("RawImages for Preview")] public RawImage display0Preview;
    public RawImage display1Preview;

    [Header("RenderTexture Resolution")] public int rtWidth = 1920;
    public int rtHeight = 1080;

    private RenderTexture rt0;
    private RenderTexture rt1;

    void Start()
    {
        // 激活所有 Display
        for (int i = 1; i < Display.displays.Length; i++)
        {
            Display.displays[i].Activate();
        }

        // 初始化 RenderTexture
        rt0 = new RenderTexture(rtWidth, rtHeight, 24, RenderTextureFormat.ARGB32);
        rt1 = new RenderTexture(rtWidth, rtHeight, 24, RenderTextureFormat.ARGB32);

        // Display 0
        worldCamera0.targetTexture = rt0;
        uiCamera0.targetTexture    = rt0;
        uiCamera0.clearFlags       = CameraClearFlags.Depth; // 保留世界渲染结果
        display0Preview.texture    = rt0;

        // Display 1
        worldCamera1.targetTexture = rt1;
        uiCamera1.targetTexture    = rt1;
        uiCamera1.clearFlags       = CameraClearFlags.Depth;
        display1Preview.texture    = rt1;
    }

    /// <summary>
    /// 可抓取 Display 0 RenderTexture 的 Texture2D
    /// </summary>
    public Texture2D CaptureDisplay0()
    {
        return CaptureRT(rt0);
    }

    /// <summary>
    /// 可抓取 Display 1 RenderTexture 的 Texture2D
    /// </summary>
    public Texture2D CaptureDisplay1()
    {
        return CaptureRT(rt1);
    }

    private Texture2D CaptureRT(RenderTexture rt)
    {
        RenderTexture.active = rt;
        Texture2D tex = new Texture2D(rt.width, rt.height, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        tex.Apply();
        RenderTexture.active = null;
        return tex;
    }
}