namespace _3rdBy.ByFunc.SoundRecord.Scripts
{
    using ByFramework.Extension.ExtendComponent;
    using Cysharp.Threading.Tasks;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// 测试
    /// </summary>
    public class VoiceCall : MonoBehaviour
    {
        public string prefix = "张三";
        public Text textTime; // 录音时间
        public Text textTip; // 录音提示 
        public Text textProgress; // 录音进度
        public Image slider; // 录音进度
        public ButtonClickState btnStartRecord; // 录音按钮
        private AudioSource _audioSource; // 播放源

        private void Awake()
        {
            textTip.text = "";
            _audioSource = gameObject.AddComponent<AudioSource>();
            _audioSource.loop = true;

            btnStartRecord.pressCallback = RecordingAudioClip;
            VoiceModule.Instance.recordTimeEvent.AddListener(ShowRecordTime);
            VoiceModule.Instance.recordStopEvent.AddListener(RecordStop);
        }

        private void Start()
        {
            textTime.text = "";
            textProgress.text = "";
            slider.fillAmount = 0;
            textTime.text = "开始录制";
        }

        /// <summary>
        /// 录音与停止录音
        /// </summary>
        /// <param name="isPress"></param>
        private void RecordingAudioClip(bool isPress)
        {
            if (isPress)
            {
                VoiceModule.Instance.StartRecording();
            }
            else
            {
                VoiceModule.Instance.StopRecording();
                Start();
            }
        }

        /// <summary>
        /// 录制完播放音频
        /// </summary>
        /// <param name="state"></param>
        /// <param name="audioClip"></param>
        private void RecordStop(bool state, AudioClip audioClip)
        {
            if (!state)
            {
                textTip.text = "录入时间过短!";
                Invoke("ResetTip", 1);
                return;
            }

            audioClip.FormatAudioClipName(prefix);
            audioClip.SaveAudioAsWavAsync(audioClip.SaveLocalFullPath()).Forget();
            CreatVoiceChat(audioClip);
        }

        /// <summary>
        /// 重置提示文字
        /// </summary>
        private void ResetTip()
        {
            textTip.text = "";
        }

        /// <summary>
        /// 显示录入时长
        /// </summary>
        /// <param name="timeCount"></param>
        /// <param name="progress"></param>
        private void ShowRecordTime(int timeCount, float progress)
        {
            textTime.text = $"{timeCount}s";
            slider.fillAmount = progress;
            textProgress.text = $"{progress * 100}%";
        }

        public ItemChat itemChat;
        public Transform content;

        private void CreatVoiceChat(AudioClip audioClip)
        {
            var newItem = Instantiate(itemChat, content);
            newItem.gameObject.SetActive(true);
            newItem.SetContent(audioClip);
        }
    }
}