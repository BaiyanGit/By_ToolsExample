namespace _3rdBy.ByFunc.SoundRecord
{
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// 语音播放管理类
    /// </summary>
    public static class VoiceSoundManager
    {
        private static Dictionary<string, AudioSource> _audioSourceList = new();

        /// <summary>
        /// 播放一个录音时，其它录音暂停
        /// </summary>
        /// <param name="audioSource"></param>
        public static void CtrlVoice(AudioSource audioSource)
        {
            var clipName = audioSource.clip.name;
            _audioSourceList.TryAdd(clipName, audioSource);

            foreach (var (key, value) in _audioSourceList)
            {
                if (clipName != key)
                {
                    value.Pause();
                }
            }
        }
    }
}