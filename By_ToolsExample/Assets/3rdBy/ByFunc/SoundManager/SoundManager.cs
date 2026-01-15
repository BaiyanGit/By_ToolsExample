namespace _3rdBy.ByFunc.SoundManager
{
    using System.Collections;
    using System.Collections.Generic;
    using _3rdBy.MetaFramework.Singleton;
    using UnityEngine;

    public class SoundManager : MonoObjSingletonTemplate<SoundManager>
    {
        private AudioSource _mBackgroundAudio;
        private AudioSource _oneshotAudio;
        private Queue<AudioSource> _mQueueAudio;
        public List<AudioClip> audioClips = new();

        public float bgVolume = 1;
        public float volume = 1;

        private Dictionary<int, Coroutine> _timerDict = new();

        private void Awake()
        {
            _mQueueAudio = new Queue<AudioSource>();
            //背景音效播放器
            var background = new GameObject("Background");
            background.transform.parent = this.transform;
            _mBackgroundAudio = background.AddComponent<AudioSource>();

            //音效播放一次播放器
            var oneShotAudio = new GameObject("OneShotAudio");
            oneShotAudio.transform.parent = transform;
            _oneshotAudio = oneShotAudio.AddComponent<AudioSource>();

            //队列播放器5个
            var queueRoot = new GameObject("QueueRoot");
            queueRoot.transform.parent = this.transform;
            for (var i = 0; i < 5; i++)
            {
                var queue = new GameObject("Queue_" + i);
                queue.transform.parent = queueRoot.transform;
                queue.SetActive(false);
                _mQueueAudio.Enqueue(queue.AddComponent<AudioSource>());
            }
        }

        private AudioClip FindAudioClips(string clipName)
        {
            return audioClips.Find(findClip => findClip.name == clipName);
        }

        private AudioClip LoadResAudioClip(string clipName)
        {
            if (FindAudioClips(clipName) != null) return FindAudioClips(clipName);
            var clip = Resources.Load<AudioClip>($"Sounds/{clipName}");
            audioClips.Add(clip);
            return clip;
        }


        /// <summary>
        /// 播放背景音效
        /// </summary>
        /// <param name="audioClip"></param>
        public void PlayBgm(AudioClip audioClip)
        {
            _mBackgroundAudio.clip = audioClip;
            _mBackgroundAudio.loop = true;
            BgmPlaying = true;
            _mBackgroundAudio.volume = bgVolume;
            //mBackgroundAudio.Play();
        }

        /// <summary>
        /// 停止播放背景音效
        /// </summary>
        public void StopBgm()
        {
            _mBackgroundAudio.Stop();
        }

        /// <summary>
        /// 停止音效
        /// </summary>
        public void StopOneShotClip()
        {
            _oneshotAudio.Stop();
        }

        public void PlayOneShotClip(string audioName, float stereoValue = 0)
        {
            var audioClip = LoadResAudioClip(audioName);

            if (audioClip == null)
            {
                Debug.Log(audioName);
            }

            PlayOneShotClip(audioClip, stereoValue);
        }

        /// <summary>
        /// 播放音效（不叠加）
        /// </summary>
        /// <param name="audioClip"></param>
        public void PlayOneShotClip(AudioClip audioClip, float stereoValue = 0)
        {
            if (audioClip == null) return;
            _oneshotAudio.Stop();
            _oneshotAudio.panStereo = stereoValue;
            _oneshotAudio.PlayOneShot(audioClip);
            _oneshotAudio.volume = volume;
        }

        public void PlayClip(int audioIndex, float stereoValue = 0, bool loop = false)
        {
            var audioClip = audioClips[audioIndex];
            PlayClip(audioClip, stereoValue, loop);
        }

        public void PlayClip(string audioName, float stereoValue = 0, bool loop = false)
        {
            var audioClip = LoadResAudioClip(audioName);

            if (audioClip == null)
            {
                Debug.Log(audioName);
            }

            PlayClip(audioClip, stereoValue, loop);
        }

        /// <summary>
        /// 播放音效（可以叠加）
        /// </summary>
        /// <param name="audioClip"></param>
        /// <param name="stereoValue"></param>
        /// <param name="loop"></param>
        public void PlayClip(AudioClip audioClip, float stereoValue = 0, bool loop = false)
        {
            if (audioClip == null)
                return;

            AudioSource audio = _mQueueAudio.Dequeue();
            if (audio != null)
            {
                audio.gameObject.SetActive(true);
                audio.clip = audioClip;
                audio.loop = loop;
                audio.volume = volume;
                audio.panStereo = stereoValue;
                audio.Play();
                _currentAudioSource = audio;
                RecycleSource(audio, audio.clip.length, loop);
            }
        }

        private AudioSource _currentAudioSource;

        public void RecycleSource()
        {
            if (_currentAudioSource == null) return;

            _currentAudioSource.Stop();
            _currentAudioSource.gameObject.SetActive(false);
        }


        /// <summary>
        /// 回收音频资源
        /// </summary>
        /// <param name="ad"></param>
        /// <param name="t"></param>
        private void RecycleSource(AudioSource ad, float t, bool isLoop)
        {
            int id = ad.GetInstanceID();
            if (_timerDict.ContainsKey(id))
            {
                //Note:定时回收一定要小心,某些情况造成非预期回收.这里停止计时回收
                StopCoroutine(_timerDict[id]);
                _timerDict.Remove(id);
            }

            if (Mathf.Approximately(t, 0))
            {
                ad.Stop();
                ad.gameObject.SetActive(false);
                _mQueueAudio.Enqueue(ad);
            }
            else
            {
                _timerDict.Add(id, SetTimer(t, () =>
                {
                    if (!isLoop)
                    {
                        ad.Stop();
                        ad.gameObject.SetActive(false);
                        _mQueueAudio.Enqueue(ad);
                    }
                }));
            }
        }

        public bool PlayerMuteBgm { get; set; } = false;

        public bool BgmPlaying
        {
            get { return _mBackgroundAudio.isPlaying; }
            set
            {
                if (value)
                {
                    if (!PlayerMuteBgm)
                    {
                        _mBackgroundAudio.Play();
                    }
                }
                else
                {
                    _mBackgroundAudio.Pause();
                }
            }
        }


        /// <summary>
        /// 定时器
        /// </summary>
        private Coroutine SetTimer(float t, System.Action callback, bool realTime = false)
        {
            return StartCoroutine(OnSetTimer(t, callback, realTime));
        }

        IEnumerator OnSetTimer(float t, System.Action callback, bool realTime)
        {
            if (realTime)
            {
                yield return new WaitForSecondsRealtime(t);
            }
            else
            {
                yield return new WaitForSeconds(t);
            }

            if (callback != null)
                callback();
        }

        private void CancelTimer(Coroutine coroutine)
        {
            if (coroutine == null)
                return;

            StopCoroutine(coroutine);
        }
    }
}