// 能用：无声音
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Unity.Collections;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Demos.示例_录制视频Recorder.RecorderVideo
{
    public class AsyncFFmpegRecorderPooledAv : MonoBehaviour
    {
        [Header("视频")] public Camera targetCamera;
        public int width = 1920;
        public int height = 1080;
        public int frameRate = 30;

        [Header("缓冲池")] public int bufferCount = 90;

        [Header("音频")] public AudioCapture audioCapture;

        private RenderTexture _rt;
        private Texture2D _frame;

        private Process _ffmpeg;
        private BinaryWriter _stdinWriter;
        private Thread _ffmpegErrorThread;

        private Thread _videoThread;
        private Thread _audioThread;

        // 保证在多个线程之间可见
        private volatile bool _recording;

        private readonly object _stdinLock = new object();

        private ConcurrentQueue<FrameBuffer> _freePool = new();
        private ConcurrentQueue<FrameBuffer> _videoQueue = new();
        [Header("音频模式")] public RecorderAudioMode audioMode = RecorderAudioMode.Microphone;

        private void Start()
        {
            Application.targetFrameRate = frameRate;
        }

        #region Public API

        public void StartRecording()
        {
            if (_recording) return;

            RecorderPathUtil.EnsureDirectories();
            InitPool();

            _rt                        = new RenderTexture(width, height, 24);
            _frame                     = new Texture2D(width, height, TextureFormat.RGB24, false);
            targetCamera.targetTexture = _rt;

            string outputPath = RecorderPathUtil.NewVideoPath();

            try
            {
                StartFFmpeg(outputPath);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to start ffmpeg: {e}");
                // 清理已分配资源
                targetCamera.targetTexture = null;
                Destroy(_rt);
                Destroy(_frame);
                return;
            }

            if (audioMode == RecorderAudioMode.Microphone)
                audioCapture?.StartCapture();

            _recording = true;

            _videoThread = new Thread(VideoWriteLoop) { IsBackground = true };
            _videoThread.Start();

            if (audioMode == RecorderAudioMode.Microphone)
            {
                _audioThread = new Thread(AudioWriteLoop) { IsBackground = true };
                _audioThread.Start();
            }

            Debug.Log($"🎥 Recording Start | AudioMode = {audioMode}");
        }


        public void StopRecording()
        {
            if (!_recording) return;

            _recording = false;

            if (audioMode == RecorderAudioMode.Microphone)
                audioCapture?.StopCapture();

            if (_videoThread != null && _videoThread.IsAlive)
                _videoThread.Join();

            if (audioMode == RecorderAudioMode.Microphone)
            {
                if (_audioThread != null && _audioThread.IsAlive)
                    _audioThread.Join();
            }

            // 关闭 stdin 前确保线程都已经退出
            lock (_stdinLock)
            {
                try
                {
                    _stdinWriter?.Close();
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"Error closing ffmpeg stdin: {e}");
                }
            }

            try
            {
                if (_ffmpeg != null && !_ffmpeg.HasExited)
                {
                    // 等待有限时间以避免永久阻塞（例如 5 秒）
                    if (!_ffmpeg.WaitForExit(5000))
                    {
                        try
                        {
                            _ffmpeg.Kill();
                        }
                        catch
                        {
                            // ignored
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Error waiting ffmpeg exit: {e}");
            }
            finally
            {
                try
                {
                    _ffmpeg?.Close();
                }
                catch
                {
                    // ignored
                }

                _ffmpeg      = null;
                _stdinWriter = null;
            }

            // 停止并释放 stderr 读取线程
            try
            {
                if (_ffmpegErrorThread != null && _ffmpegErrorThread.IsAlive)
                {
                    _ffmpegErrorThread.Join(500);
                }
            }
            catch
            {
                // ignored
            }

            _ffmpegErrorThread = null;

            targetCamera.targetTexture = null;
            Destroy(_rt);
            Destroy(_frame);

            Debug.Log("✅ Recording Finished");
        }

        #endregion

        private void LateUpdate()
        {
            if (!_recording) return;

            if (!_freePool.TryDequeue(out var buffer))
                return;

            // 在每帧末尾读取最终渲染内容，避免丢失在 LateUpdate/OnRender 之后发生的移动
            StartCoroutine(CaptureFrameCoroutine(buffer));
        }

        private IEnumerator CaptureFrameCoroutine(FrameBuffer buffer)
        {
            // 等到帧渲染完成（包含所有 Update / LateUpdate / 渲染更新）
            yield return new WaitForEndOfFrame();

            // 如果摄像机被设置为 targetTexture，则 Unity 会在渲染阶段把画面写入该 RenderTexture
            RenderTexture.active = _rt;
            _frame.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            _frame.Apply();
            RenderTexture.active = null;

            // 使用泛型 GetRawTextureData<byte>() 并拷贝到池 buffer
            var raw = _frame.GetRawTextureData<byte>();
            // NativeArray<T>.CopyTo(T[]) 可用
            raw.CopyTo(buffer.data);

            _videoQueue.Enqueue(buffer);
        }

        #region Pool

        private void InitPool()
        {
            _freePool   = new ConcurrentQueue<FrameBuffer>();
            _videoQueue = new ConcurrentQueue<FrameBuffer>();

            int size = width * height * 3;
            for (int i = 0; i < bufferCount; i++)
            {
                _freePool.Enqueue(new FrameBuffer { data = new byte[size] });
            }
        }

        #endregion

        #region FFmpeg

        private void StartFFmpeg(string outputPath)
        {
            string ffmpegPath = Path.Combine(
                Application.streamingAssetsPath,
                "ffmpeg.exe"
            );

            string args = "-y ";

            // ===== 视频输入（始终有）=====
            args += "-f rawvideo -pix_fmt rgb24 " +
                    $"-s {width}x{height} -r {frameRate} -i - ";

            // ===== 音频输入 =====
            switch (audioMode)
            {
                case RecorderAudioMode.Microphone:
                    args += "-f s16le -ar 44100 -ac 2 -i - ";
                    break;

                case RecorderAudioMode.System:
                    // Windows 系统声音（WASAPI）
                    args += "-f wasapi -i default ";
                    break;

                case RecorderAudioMode.None:
                    // 不加任何音频输入
                    break;
            }

            // ===== 编码 =====
            args += "-c:v libx264" +
                    " -preset veryfast" +
                    " -vf vflip" +
                    " -pix_fmt yuv420p ";

            if (audioMode != RecorderAudioMode.None)
            {
                args += "-c:a aac -b:a 128k ";
            }

            args += $"\"{outputPath}\"";

            var psi = new ProcessStartInfo
            {
                FileName              = ffmpegPath,
                Arguments             = args,
                UseShellExecute       = false,
                RedirectStandardInput = true,
                RedirectStandardError = true,
                CreateNoWindow        = true
            };

            try
            {
                _ffmpeg = new Process { StartInfo = psi };
                _ffmpeg.Start();

                lock (_stdinLock)
                {
                    _stdinWriter = new BinaryWriter(_ffmpeg.StandardInput.BaseStream);
                }

                // 启动一个线程以读取 stderr（方便调试 ffmpeg 报错）
                _ffmpegErrorThread = new Thread(() =>
                    {
                        try
                        {
                            while (_ffmpeg is { HasExited: false } && _ffmpeg.StandardError.ReadLine() is { } line)
                            {
                                if (!string.IsNullOrEmpty(line))
                                    Debug.Log("[ffmpeg] " + line);
                            }
                        }
                        catch (Exception e)
                        {
                            Debug.LogWarning($"ffmpeg stderr reader ended: {e}");
                        }
                    })
                    { IsBackground = true };
                _ffmpegErrorThread.Start();
            }
            catch (Exception ex)
            {
                // 清理可能部分创建的资源
                try
                {
                    _ffmpeg?.Kill();
                }
                catch
                {
                    // ignored
                }

                _ffmpeg      = null;
                _stdinWriter = null;
                Debug.LogError($"StartFFmpeg failed: {ex}");
                throw;
            }
        }

        #endregion

        #region Threads

        private void VideoWriteLoop()
        {
            while (_recording || !_videoQueue.IsEmpty)
            {
                if (_videoQueue.TryDequeue(out var buffer))
                {
                    lock (_stdinLock)
                    {
                        try
                        {
                            _stdinWriter?.Write(buffer.data);
                        }
                        catch (Exception e)
                        {
                            Debug.LogWarning($"Error writing video frame to ffmpeg stdin: {e}");
                        }
                    }

                    _freePool.Enqueue(buffer);
                }
                else
                {
                    Thread.Sleep(1);
                }
            }
        }

        private void AudioWriteLoop()
        {
            while (_recording || (audioCapture != null && !audioCapture.pcmQueue.IsEmpty))
            {
                if (audioCapture != null && audioCapture.pcmQueue.TryDequeue(out var pcm))
                {
                    lock (_stdinLock)
                    {
                        try
                        {
                            _stdinWriter?.Write(pcm);
                        }
                        catch (Exception e)
                        {
                            Debug.LogWarning($"Error writing audio pcm to ffmpeg stdin: {e}");
                        }
                    }
                }
                else
                {
                    Thread.Sleep(1);
                }
            }
        }

        #endregion

        private void OnDestroy()
        {
            if (_recording)
                StopRecording();
        }
    }


    public class FrameBuffer
    {
        public byte[] data;
    }

    public enum RecorderAudioMode
    {
        [InspectorName("不录声音")] None,
        [InspectorName("麦克风")] Microphone,
        [InspectorName("系统声音")] System
    }
}