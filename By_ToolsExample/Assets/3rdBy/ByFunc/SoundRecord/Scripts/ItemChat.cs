namespace _3rdBy.SoundRecord.Scripts
{
    using UnityEngine;
    using UnityEngine.UI;

    public class ItemChat : MonoBehaviour
    {
        private Text _tAudioName;
        private Image _imgProgress;
        private Button _btnPlaySound;
        private AudioSource _audioSource;
        private float _playTime;
        private float _audioTime;

        private void Awake()
        {
            _tAudioName = transform.GetChild(0).GetComponent<Text>();
            _imgProgress = transform.GetChild(1).GetComponent<Image>();
            _audioSource = gameObject.AddComponent<AudioSource>();
            _btnPlaySound = transform.GetComponent<Button>();
            _btnPlaySound.onClick.AddListener(PlaySound);
        }

        public void SetContent(AudioClip audioClip)
        {
            var fileName = audioClip.name.Replace(".WAV", "");
            _tAudioName.text = $"{fileName}         {audioClip.length}\"";
            _audioSource.clip = audioClip;
            _audioTime = audioClip.length;
        }

        private void Update()
        {
            if (_audioSource.clip == null) return;
            _playTime = _audioSource.time;
            _imgProgress.fillAmount = _playTime / _audioTime;
        }

        private void PlaySound()
        {
            switch (_playTime)
            {
                case 0 when !_audioSource.isPlaying: // 没有播放则开始播放
                    Debug.Log("播放");
                    _audioSource.Play();

                    break;
                case > 0 when _playTime < _audioTime: // 已经播放，是暂停还是继续
                {
                    if (_audioSource.isPlaying)
                    {
                        Debug.Log("暂停");
                        _audioSource.Pause();
                    }
                    else
                    {
                        Debug.Log("继续");
                        _audioSource.UnPause();
                    }

                    break;
                }
                default:
                    Debug.Log("播放完成");
                    _audioSource.Stop();
                    break;
            }
        }
    }
}