namespace _3rdBy.ByFunc.SoundRecord.Scripts
{
    using _3rdBy.MetaFramework.Singleton;
    using UnityEngine;
    using UnityEngine.Events;

    /// <summary>
    /// 声音录制模块
    /// </summary>
    public class VoiceModule : MonoObjSingletonTemplate<VoiceModule>
    {
        private AudioClip _audioClip; // 录制时的音频
        private float _recordStartTime; // 录音开始的时间
        private float _recordTimeCount; // 记录录制声音时的时长（显示用）
        private bool _isStartRecord; // 是否开始录制声音
        public int lengthSec = 180; //声音录制限制时长

        [HideInInspector] public UnityEvent<int, float> recordTimeEvent = new(); // 录制声音时显示的事件（返回：时间和进度）
        [HideInInspector] public UnityEvent<bool, AudioClip> recordStopEvent = new(); // 录制声音结束事件

        private void Update()
        {
            ShowRecordTime();
        }

        /// <summary>
        /// 开始录制声音
        /// </summary>
        public void StartRecording()
        {
            if (_isStartRecord) return;
            _audioClip = Microphone.Start(Microphone.devices[0], true, lengthSec, 44100);
            _recordStartTime = Time.time; // 记录开始时间
            _recordTimeCount = 0; // 重置录音计时
            _isStartRecord = true; //开始计时
        }

        /// <summary>
        /// 结束录制声音
        /// </summary>
        public void StopRecording()
        {
            if (!_isStartRecord) return;
            _isStartRecord = false; // 结束计时显示
            Microphone.End(Microphone.devices[0]); // 结束录制
            var timeLength = Time.time - _recordStartTime;

            // 录入时间小于1秒
            if (timeLength < 1)
            {
                _audioClip = null;
                recordStopEvent?.Invoke(false, null);
                return;
            }

            var sampleLength = (int)timeLength * 44100; // 获取样本长度
            var samples = new float[sampleLength]; // 采样数据
            var trimmedClip = AudioClip.Create("TrimmedClip", sampleLength, 1, 44100, false); // 创建一个新的音频剪辑，长度为n秒
            _audioClip.GetData(samples, 0); // 从录音剪辑中获取采样数据
            trimmedClip.SetData(samples, 0); // 将采样数据复制到新的音频剪辑中
            _audioClip = trimmedClip;
            recordStopEvent?.Invoke(true, _audioClip);
        }

        /// <summary>
        /// 显示录音时的时间
        /// </summary>
        private void ShowRecordTime()
        {
            if (!_isStartRecord) return;
            _recordTimeCount = Time.time - _recordStartTime;
            var progress = Mathf.Round(_recordTimeCount / lengthSec * 100) / 100;

            if (progress > 1)
            {
                StopRecording();
                recordTimeEvent?.Invoke((int)_recordTimeCount, 1);
                return;
            }

            recordTimeEvent?.Invoke((int)_recordTimeCount, progress);
        }
    }
}