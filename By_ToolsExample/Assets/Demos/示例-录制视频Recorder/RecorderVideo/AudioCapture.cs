using System;
using System.Collections.Concurrent;
using UnityEngine;

namespace Demos.示例_录制视频Recorder.RecorderVideo
{
    public class AudioCapture : MonoBehaviour
    {
        [Header("麦克风设备（空表示默认设备）")] public string deviceName = ""; // 建议在 Inspector 留空或指定 Microphone.devices[0]

        [Header("首选采样率/声道（会根据设备能力调整）")] public int sampleRate = 44100;
        public int channels = 1;

        private AudioClip _micClip;
        private int _lastSample;
        private bool _recording;

        public readonly ConcurrentQueue<byte[]> pcmQueue = new();

        // 外部用来检测麦克风是否已就绪（有有效位置）
        public bool IsReady => _micClip != null && Microphone.GetPosition(deviceName) > 0;

        /// <summary>
        /// 启动麦克风采集。该函数会在内部尝试多次启动麦克风（带短延时），以应对驱动释放/资源未及时回收的情况。
        /// 返回 true 表示成功启动并就绪（或在短超时内开始采样），false 表示最终失败。
        /// 请在主线程调用（Microphone API 要在主线程调用）。
        /// </summary>
        public bool StartCapture()
        {
            _lastSample = 0;

            // 选择设备：如果用户没显式填写，则尝试第一个可用设备，若无则使用空串（Unity 的默认设备）
            if (string.IsNullOrEmpty(deviceName))
            {
                if (Microphone.devices != null && Microphone.devices.Length > 0)
                    deviceName = Microphone.devices[0];
                else
                    deviceName = ""; // 仍使用空串作为保险
            }
            else
            {
                // 如果指定的 deviceName 不在列表里，降级为默认设备
                bool found = false;
                foreach (var d in Microphone.devices)
                {
                    if (d == deviceName)
                    {
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    Debug.LogWarning($"指定的麦克风设备 '{deviceName}' 未找到，改为默认设备。");
                    deviceName = Microphone.devices != null && Microphone.devices.Length > 0 ? Microphone.devices[0] : "";
                }
            }

            // 获取设备支持的采样率范围并选择合适的频率
            int minFreq = 0, maxFreq = 0;
            try
            {
                Microphone.GetDeviceCaps(deviceName, out minFreq, out maxFreq);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"GetDeviceCaps failed: {e}. Will proceed with desired sampleRate.");
                minFreq = maxFreq = 0;
            }

            int chosenFreq = sampleRate;
            if (minFreq == 0 && maxFreq == 0)
            {
                // 任意采样率一般可用，使用首选值
                chosenFreq = sampleRate;
            }
            else
            {
                // 如果 maxFreq 为 0（某些平台用 0 表示不限），处理一下
                if (maxFreq == 0) maxFreq = minFreq;
                chosenFreq = Mathf.Clamp(sampleRate, minFreq, maxFreq);
            }

            // 使用较长循环缓冲（例如 5 秒），减少 wrap 频率
            int clipSeconds = 5;

            // 尝试多次启动（如果先前 Stop 还未完全释放设备，可能需要重试）
            const int attempts       = 8;
            const int attemptDelayMs = 100; // 每次尝试间隔
            bool      started        = false;

            for (int i = 0; i < attempts && !started; i++)
            {
                try
                {
                    _micClip = Microphone.Start(deviceName, true, clipSeconds, chosenFreq);
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"Microphone.Start threw exception (attempt {i + 1}/{attempts}): {ex}");
                    _micClip = null;
                }

                // 等待采样位置大于 0（麦克风开始工作），最多等待 2 秒（分多次轮询）
                if (_micClip != null)
                {
                    int waited = 0;
                    while (Microphone.GetPosition(deviceName) <= 0 && waited < 2000)
                    {
                        System.Threading.Thread.Sleep(10);
                        waited += 10;
                    }

                    if (Microphone.GetPosition(deviceName) > 0)
                    {
                        started = true;
                        break;
                    }
                    else
                    {
                        // 未就绪：结束本次尝试并等待重试
                        try
                        {
                            Microphone.End(deviceName);
                        }
                        catch
                        {
                        }

                        _micClip = null;
                        System.Threading.Thread.Sleep(attemptDelayMs);
                    }
                }
                else
                {
                    // Start 返回 null，等待并重试
                    System.Threading.Thread.Sleep(attemptDelayMs);
                }
            }

            if (!started)
            {
                Debug.LogWarning("AudioCapture.StartCapture: 无法启动麦克风（超时/失败）。");
                _recording = false;
                return false;
            }

            // 成功启动，更新实际参数
            try
            {
                // AudioClip 在部分 Unity 版本上没有公开 frequency/channels 字段，但通常会存在
                // 若不存在，可保留最初的配置（sampleRate/channels）
#if UNITY_2021_1_OR_NEWER
                sampleRate = _micClip.frequency;
                channels   = _micClip.channels;
#endif
            }
            catch
            {
                // 忽略，继续使用既有值
            }

            _recording = true;
            Debug.Log($"AudioCapture.StartCapture -> device='{deviceName}', sampleRate={sampleRate}, channels={channels}, clipSamples={(_micClip != null ? _micClip.samples : 0)}");
            return true;
        }

        public void StopCapture()
        {
            _recording = false;
            try
            {
                Microphone.End(deviceName);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Microphone.End() error: {e}");
            }

            // 不在 Stop 阶段做长时间阻塞等待驱动释放（避免卡主主线程）。
            // StartCapture 里有重试逻辑，会在需要时再次尝试启动。
        }

        private void Update()
        {
            if (!_recording || _micClip == null) return;

            int pos = Microphone.GetPosition(deviceName);
            if (pos < 0) return;

            // 计算新增的样本帧数（按 sample index，不是 bytes）
            int newSamples;
            if (pos >= _lastSample)
            {
                newSamples = pos - _lastSample;
            }
            else
            {
                // 环回情况：从 lastSample 到 clipEnd，再从 0 到 pos
                newSamples = (_micClip.samples - _lastSample) + pos;
            }

            if (newSamples <= 0) return;

            // 为了简化拷贝，分两段读取（如果没有环回则只一段）
            float[] buffer = new float[newSamples * channels];
            if (pos > _lastSample)
            {
                // 连续段
                _micClip.GetData(buffer, _lastSample);
            }
            else
            {
                // 第一段：从 lastSample 到 clipEnd
                int     firstFrames = _micClip.samples - _lastSample;
                float[] firstBuf    = new float[firstFrames * channels];
                _micClip.GetData(firstBuf, _lastSample);
                Array.Copy(firstBuf, 0, buffer, 0, firstBuf.Length);

                // 第二段：从 0 到 pos-1
                if (pos > 0)
                {
                    float[] secondBuf = new float[pos * channels];
                    _micClip.GetData(secondBuf, 0);
                    Array.Copy(secondBuf, 0, buffer, firstBuf.Length, secondBuf.Length);
                }
            }

            // 转为 PCM16 并入队
            byte[] pcm = FloatToPCM16(buffer);
            pcmQueue.Enqueue(pcm);

            // 更新 lastSample
            _lastSample = pos;
        }

        private byte[] FloatToPCM16(float[] samples)
        {
            byte[] pcm = new byte[samples.Length * 2];
            for (int i = 0; i < samples.Length; i++)
            {
                short s = (short)Mathf.Clamp(samples[i] * 32767f, -32768, 32767);
                // 小端平台：BitConverter 返回小端字节序
                var b = BitConverter.GetBytes(s);
                pcm[i * 2]     = b[0];
                pcm[i * 2 + 1] = b[1];
            }

            return pcm;
        }
    }
}