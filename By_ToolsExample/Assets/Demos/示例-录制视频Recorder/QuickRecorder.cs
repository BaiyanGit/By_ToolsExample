using UnityEngine;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Collections.Concurrent;


public class AsyncFFmpegRecorder_Pooled : MonoBehaviour
{
    private class FrameBuffer
    {
        public byte[] data;
    }

    [Header("Capture")] public Camera targetCamera;
    public int width = 1920;
    public int height = 1080;
    public int frameRate = 30;

    [Header("Buffer Pool")] public int bufferCount = 90; // 建议 = FPS * 3

    private RenderTexture rt;
    private Texture2D frame;

    private Process ffmpeg;
    private BinaryWriter ffmpegStdin;
    private Thread writeThread;

    private bool recording;

    // 双队列
    private ConcurrentQueue<FrameBuffer> freePool = new();
    private ConcurrentQueue<FrameBuffer> writeQueue = new();

    void Start()
    {
        Application.targetFrameRate = frameRate;
    }

    #region Public API

    public void StartRecording()
    {
        if (recording) return;

        InitPool();

        rt                         = new RenderTexture(width, height, 24);
        frame                      = new Texture2D(width, height, TextureFormat.RGB24, false);
        targetCamera.targetTexture = rt;

        string outputPath = Path.Combine(
            Application.persistentDataPath,
            $"record_{System.DateTime.Now:yyyyMMdd_HHmmss}.mp4"
        );

        StartFFmpeg(outputPath);
        recording   = true;
        writeThread = new Thread(WriteLoop) { IsBackground = true };
        writeThread.Start();

        UnityEngine.Debug.Log("🎥 Pooled Async Recording Start");
    }

    public void StopRecording()
    {
        if (!recording) return;

        recording = false;
        writeThread.Join();

        ffmpegStdin.Close();
        ffmpeg.WaitForExit();
        ffmpeg.Close();

        targetCamera.targetTexture = null;
        Destroy(rt);
        Destroy(frame);

        ClearQueues();

        UnityEngine.Debug.Log("✅ Pooled Recording Finished");
    }

    #endregion

    void LateUpdate()
    {
        if (!recording) return;

        if (!freePool.TryDequeue(out var buffer))
        {
            // 缓冲不够，丢帧（比卡死好）
            return;
        }

        RenderTexture.active = rt;
        frame.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        frame.Apply();
        RenderTexture.active = null;

        var raw = frame.GetRawTextureData();
        System.Buffer.BlockCopy(raw, 0, buffer.data, 0, raw.Length);

        writeQueue.Enqueue(buffer);
    }

    #region Pool

    void InitPool()
    {
        freePool   = new ConcurrentQueue<FrameBuffer>();
        writeQueue = new ConcurrentQueue<FrameBuffer>();

        int frameSize = width * height * 3;

        for (int i = 0; i < bufferCount; i++)
        {
            freePool.Enqueue(new FrameBuffer
            {
                data = new byte[frameSize]
            });
        }
    }

    void ClearQueues()
    {
        freePool   = new ConcurrentQueue<FrameBuffer>();
        writeQueue = new ConcurrentQueue<FrameBuffer>();
    }

    #endregion

    #region FFmpeg

    void StartFFmpeg(string outputPath)
    {
        string ffmpegPath = Path.Combine(
            Application.streamingAssetsPath,
            "ffmpeg.exe"
        );

        string args =
            $"-y " +
            $"-f rawvideo " +
            $"-pix_fmt rgb24 " +
            $"-s {width}x{height} " +
            $"-r {frameRate} " +
            $"-i - " +
            $"-an " +
            $"-c:v libx264 " +
            $"-preset veryfast " +
            $"-pix_fmt yuv420p " +
            $"\"{outputPath}\"";

        var psi = new ProcessStartInfo
        {
            FileName              = ffmpegPath,
            Arguments             = args,
            UseShellExecute       = false,
            RedirectStandardInput = true,
            CreateNoWindow        = true
        };

        ffmpeg = new Process { StartInfo = psi };
        ffmpeg.Start();
        ffmpegStdin = new BinaryWriter(ffmpeg.StandardInput.BaseStream);
    }

    #endregion

    #region Thread

    void WriteLoop()
    {
        while (recording || !writeQueue.IsEmpty)
        {
            if (writeQueue.TryDequeue(out var buffer))
            {
                ffmpegStdin.Write(buffer.data);
                freePool.Enqueue(buffer); // 回收
            }
            else
            {
                Thread.Sleep(1);
            }
        }
    }

    #endregion

    void OnDestroy()
    {
        if (recording)
            StopRecording();
    }
}