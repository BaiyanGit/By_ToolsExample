using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Threading;
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

        // 命名管道 server（写端），分别用于视频和麦克风音频
        private NamedPipeServerStream _videoPipeServer;
        private NamedPipeServerStream _audioPipeServer;

        private BinaryWriter _videoPipeWriter;
        private BinaryWriter _audioPipeWriter;

        private Thread _ffmpegErrorThread;

        private Thread _videoThread;
        private Thread _audioThread;

        // 保证在多个线程之间可见
        private volatile bool _recording;

        // 分别保护两个管道写入（可分别锁）
        private readonly object _videoPipeLock = new object();
        private readonly object _audioPipeLock = new object();

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

            // 如果使用麦克风模式：先启动麦克风采集，等待就绪
            if (audioMode == RecorderAudioMode.Microphone)
            {
                audioCapture?.StartCapture();

                // 等待麦克风就绪（最多 2 秒）
                int wait = 0;
                while ((audioCapture == null || !audioCapture.isReady) && wait < 2000)
                {
                    Thread.Sleep(10);
                    wait += 10;
                }

                if (audioCapture == null || !audioCapture.isReady)
                {
                    Debug.LogWarning("AudioCapture 未能在超时内就绪，继续启动 ffmpeg 但音频参数可能不正确。");
                }
            }

            try
            {
                StartFFmpeg(outputPath);
            }
            catch (Exception e)
            {
                Debug.LogError($"启动ffmpeg失败: {e}");
                // 清理
                targetCamera.targetTexture = null;
                Destroy(_rt);
                Destroy(_frame);
                CleanupPipes();
                if (audioMode == RecorderAudioMode.Microphone)
                    audioCapture?.StopCapture();
                return;
            }

            // 启动写线程
            _recording   = true;
            _videoThread = new Thread(VideoWriteLoop) { IsBackground = true };
            _videoThread.Start();

            if (audioMode == RecorderAudioMode.Microphone)
            {
                _audioThread = new Thread(AudioWriteLoop) { IsBackground = true };
                _audioThread.Start();
            }

            Debug.Log($"🟢 录制开始|音频模式 = {audioMode}");
        }

        public void StopRecording()
        {
            if (!_recording) return;

            Debug.Log("🔴 正在停止录制...");

            // 1) 标记停止，写线程看到后会尽快退出（但可能正在阻塞写入）
            _recording = false;

            // 2) 立即停止音频采集（让 audio queue 快速空）
            if (audioMode == RecorderAudioMode.Microphone)
                audioCapture?.StopCapture();

            // 3) 断开/关闭管道的写端，通知 ffmpeg 数据结束并促使任何阻塞的写操作尽快抛异常返回
            try
            {
                lock (_videoPipeLock)
                {
                    try
                    {
                        if (_videoPipeWriter != null)
                        {
                            _videoPipeWriter.Close();
                            _videoPipeWriter = null;
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning($"关闭视频管道写入器时出错: {e}");
                    }

                    try
                    {
                        if (_videoPipeServer != null)
                        {
                            // Disconnect/Dispose to ensure client sees EOF
                            try
                            {
                                _videoPipeServer.Disconnect();
                            }
                            catch
                            {
                                Debug.LogWarning("断开视频管道服务器连接时出错");
                            }

                            _videoPipeServer.Dispose();
                        }
                    }
                    catch
                    {
                        Debug.LogWarning("关闭视频管道服务器时出错");
                    }

                    _videoPipeServer = null;
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"关闭视频管道时出错: {e}");
            }

            try
            {
                lock (_audioPipeLock)
                {
                    try
                    {
                        if (_audioPipeWriter != null)
                        {
                            _audioPipeWriter.Close();
                            _audioPipeWriter = null;
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning($"关闭音频管道写入器时出错: {e}");
                    }

                    try
                    {
                        if (_audioPipeServer != null)
                        {
                            try
                            {
                                _audioPipeServer.Disconnect();
                            }
                            catch
                            {
                                Debug.LogWarning("断开音频管道服务器连接时出错");
                            }

                            _audioPipeServer.Dispose();
                        }
                    }
                    catch
                    {
                        Debug.LogWarning("关闭音频管道服务器时出错");
                    }

                    _audioPipeServer = null;
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"关闭音频管道时出错: {e}");
            }

            // 4) 等待写线程退出（带超时），避免无限阻塞主线程
            if (_videoThread is { IsAlive: true })
            {
                bool vJoined = _videoThread.Join(3000);
                if (!vJoined)
                    Debug.LogWarning("视频写入线程未在超时内退出.");
            }

            if (audioMode == RecorderAudioMode.Microphone)
            {
                if (_audioThread is { IsAlive: true })
                {
                    bool aJoined = _audioThread.Join(3000);
                    if (!aJoined)
                        Debug.LogWarning("音频写入线程未在超时内退出.");
                }
            }

            // 5) 等待 ffmpeg 退出（带超时），超时则强制 Kill
            try
            {
                if (_ffmpeg is { HasExited: false })
                {
                    if (!_ffmpeg.WaitForExit(5000))
                    {
                        try
                        {
                            _ffmpeg.Kill();
                        }
                        catch
                        {
                            Debug.LogWarning("强制杀死 ffmpeg 进程.");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"等待ffmpeg退出时出错: {e}");
            }
            finally
            {
                try
                {
                    _ffmpeg?.Close();
                }
                catch
                {
                    Debug.LogWarning("关闭 ffmpeg 进程时出错.");
                }

                _ffmpeg = null;
            }

            // 停止并释放 stderr 读取线程
            try
            {
                if (_ffmpegErrorThread is { IsAlive: true })
                {
                    _ffmpegErrorThread.Join(500);
                }
            }
            catch
            {
                Debug.LogWarning("关闭 ffmpeg 错误读取线程时出错.");
            }

            _ffmpegErrorThread = null;

            // 最后再次确保释放管道资源（冗余安全）
            CleanupPipes();

            targetCamera.targetTexture = null;
            Destroy(_rt);
            Destroy(_frame);

            Debug.Log("✅ 录制已完成");
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
            yield return new WaitForEndOfFrame();

            RenderTexture.active = _rt;
            _frame.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            _frame.Apply();
            RenderTexture.active = null;

            var raw = _frame.GetRawTextureData<byte>();
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

        #region FFmpeg + Pipes

        private void StartFFmpeg(string outputPath)
        {
            string videoPipeName = $"unity_rec_video_{Guid.NewGuid():N}";
            string videoPipePath = $@"\\.\pipe\{videoPipeName}";

            string audioPipeName = null;
            string audioPipePath = null;

            bool useAudioPipe = audioMode == RecorderAudioMode.Microphone;

            if (useAudioPipe)
            {
                audioPipeName = $"unity_rec_audio_{Guid.NewGuid():N}";
                audioPipePath = $@"\\.\pipe\{audioPipeName}";
            }

            // 创建命名管道 server，并在后台等待连接
            _videoPipeServer = new NamedPipeServerStream(videoPipeName, PipeDirection.Out, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
            var videoConnThread = new Thread(() =>
            {
                try
                {
                    _videoPipeServer.WaitForConnection();
                    _videoPipeWriter = new BinaryWriter(_videoPipeServer);
                    Debug.Log("🖇️ 视频管已连接.");
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"视频管连接错误: {e}");
                }
            }) { IsBackground = true };
            videoConnThread.Start();

            if (useAudioPipe)
            {
                _audioPipeServer = new NamedPipeServerStream(audioPipeName, PipeDirection.Out, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
                var audioConnThread = new Thread(() =>
                {
                    try
                    {
                        _audioPipeServer.WaitForConnection();
                        _audioPipeWriter = new BinaryWriter(_audioPipeServer);
                        Debug.Log("🖇️ 音频管已连接.");
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning($"音频管连接错误: {e}");
                    }
                }) { IsBackground = true };
                audioConnThread.Start();
            }

            string ffmpegPath = Path.Combine(Application.streamingAssetsPath, "ffmpeg.exe");
            string args       = "-y ";

            // 视频输入
            args += $"-f rawvideo -pix_fmt rgb24 -s {width}x{height} -r {frameRate} -i \"{videoPipePath}\" ";


            switch (audioMode)
            {
                // 在拼接音频输入参数处（Microphone 情况）改为使用实际值：
                case RecorderAudioMode.Microphone:
                {
                    int ar = audioCapture != null ? audioCapture.sampleRate : 44100;
                    int ac = audioCapture != null ? audioCapture.channels : 1; // 若不确定，使用 1 做保守值
                    args += $"-f s16le -ar {ar} -ac {ac} -i \"{audioPipePath}\" ";
                    break;
                }
                case RecorderAudioMode.System:
                    args += "-f wasapi -i default ";
                    break;
                case RecorderAudioMode.None:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            // 编码参数
            args += "-c:v libx264 -preset veryfast -vf vflip -pix_fmt yuv420p ";
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
                RedirectStandardError = true,
                CreateNoWindow        = true
            };

            try
            {
                _ffmpeg = new Process { StartInfo = psi };
                _ffmpeg.Start();

                // stderr 读取
                _ffmpegErrorThread = new Thread(() =>
                {
                    try
                    {
                        while (_ffmpeg is { HasExited: false } && _ffmpeg.StandardError.ReadLine() is { } line)
                        {
                            if (!string.IsNullOrEmpty(line))
                            {
                                // Debug.Log("[ffmpeg] " + line);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning($"ffmpeg标准错误读取器已结束: {e}");
                    }
                }) { IsBackground = true };
                _ffmpegErrorThread.Start();
            }
            catch (Exception ex)
            {
                Debug.LogError($"启动FFmpeg失败: {ex}");
                CleanupPipes();
                throw;
            }
        }

        private void CleanupPipes()
        {
            try
            {
                _videoPipeWriter?.Close();
            }
            catch
            {
                Debug.LogWarning("关闭视频管道写入器时出错.");
            }

            _videoPipeWriter = null;

            try
            {
                if (_videoPipeServer != null)
                {
                    try
                    {
                        _videoPipeServer.Disconnect();
                    }
                    catch
                    {
                        Debug.LogWarning("断开视频管道服务器连接时出错");
                    }

                    _videoPipeServer.Dispose();
                }
            }
            catch
            {
                Debug.LogWarning("关闭视频管道服务器时出错");
            }

            _videoPipeServer = null;

            try
            {
                _audioPipeWriter?.Close();
            }
            catch
            {
                Debug.LogWarning("关闭音频管道写入器时出错.");
            }

            _audioPipeWriter = null;

            try
            {
                if (_audioPipeServer != null)
                {
                    try
                    {
                        _audioPipeServer.Disconnect();
                    }
                    catch
                    {
                        Debug.LogWarning("断开音频管道服务器连接时出错");
                    }

                    _audioPipeServer.Dispose();
                }
            }
            catch
            {
                Debug.LogWarning("关闭音频管道服务器时出错");
            }

            _audioPipeServer = null;
        }

        #endregion

        #region Threads

        private void VideoWriteLoop()
        {
            // 等待 video pipe writer 准备好（或 recording 停止）
            int waitMs = 0;
            while (_recording && _videoPipeWriter == null && waitMs < 5000)
            {
                Thread.Sleep(5);
                waitMs += 5;
            }

            if (_videoPipeWriter == null)
            {
                Debug.LogWarning("等待后视频管道写入器不可用。视频帧将被丢弃。");
            }

            while (_recording || !_videoQueue.IsEmpty)
            {
                if (_videoQueue.TryDequeue(out var buffer))
                {
                    try
                    {
                        if (_videoPipeWriter != null && _videoPipeServer is { IsConnected: true })
                        {
                            lock (_videoPipeLock)
                            {
                                try
                                {
                                    _videoPipeWriter.Write(buffer.data);
                                    _videoPipeWriter.Flush();
                                }
                                catch (IOException ioEx)
                                {
                                    Debug.LogWarning($"视频管道写入IOException（将停止写入）: {ioEx}");
                                    // 如果写入失败，置空 writer，后续帧会丢弃
                                    try
                                    {
                                        _videoPipeWriter.Close();
                                    }
                                    catch
                                    {
                                        Debug.LogWarning("关闭视频管道写入器时出错.");
                                    }

                                    _videoPipeWriter = null;
                                }
                                catch (Exception ex)
                                {
                                    Debug.LogWarning($"Video pipe write exception: {ex}");
                                    try
                                    {
                                        _videoPipeWriter.Close();
                                    }
                                    catch
                                    {
                                        Debug.LogWarning("关闭视频管道写入器时出错.");
                                    }

                                    _videoPipeWriter = null;
                                }
                            }
                        }
                        else
                        {
                            // 未连接：丢帧以避免阻塞
                        }
                    }
                    finally
                    {
                        _freePool.Enqueue(buffer);
                    }
                }
                else
                {
                    Thread.Sleep(1);
                }
            }

            Debug.Log("⏏️ 视频写入线程退出.");
        }

        private void AudioWriteLoop()
        {
            if (audioMode != RecorderAudioMode.Microphone) return;

            int waitMs = 0;
            while (_recording && _audioPipeWriter == null && waitMs < 5000)
            {
                Thread.Sleep(5);
                waitMs += 5;
            }

            if (_audioPipeWriter == null)
            {
                Debug.LogWarning("等待后音频管道写入器不可用,音频将被丢弃。");
            }

            while (_recording || (audioCapture != null && !audioCapture.pcmQueue.IsEmpty))
            {
                if (audioCapture != null && audioCapture.pcmQueue.TryDequeue(out var pcm))
                {
                    try
                    {
                        if (_audioPipeWriter != null && _audioPipeServer is { IsConnected: true })
                        {
                            lock (_audioPipeLock)
                            {
                                try
                                {
                                    _audioPipeWriter.Write(pcm);
                                    _audioPipeWriter.Flush();
                                }
                                catch (IOException ioEx)
                                {
                                    Debug.LogWarning($"音频管道写入IOException（将停止写入）: {ioEx}");
                                    try
                                    {
                                        _audioPipeWriter.Close();
                                    }
                                    catch
                                    {
                                        Debug.LogWarning("关闭音频管道写入器时出错.");
                                    }

                                    _audioPipeWriter = null;
                                }
                                catch (Exception ex)
                                {
                                    Debug.LogWarning($"音频管道写入异常: {ex}");
                                    try
                                    {
                                        _audioPipeWriter.Close();
                                    }
                                    catch
                                    {
                                        Debug.LogWarning("关闭音频管道写入器时出错.");
                                    }

                                    _audioPipeWriter = null;
                                }
                            }
                        }
                        else
                        {
                            // 未连接：丢弃音频块
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning($"音频写入外部异常: {ex}");
                    }
                }
                else
                {
                    Thread.Sleep(1);
                }
            }

            Debug.Log("⏏️ 音频写入线程退出.");
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