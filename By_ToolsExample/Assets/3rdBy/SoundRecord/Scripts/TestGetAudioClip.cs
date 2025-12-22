namespace _3rdBy.SoundRecord.Scripts
{
    using System.IO;
    using MetaFramework.HttpNetwork;
    using Cysharp.Threading.Tasks;
    using UnityEngine;
    public class TestGetAudioClip : MonoBehaviour
    {
        public string inputName = "";
        private AudioSource _audioSource;

        private void Start()
        {
            if (!_audioSource) _audioSource = gameObject.AddComponent<AudioSource>();
            GetAudioClip().Forget();
        }

        private async UniTaskVoid GetAudioClip()
        {
            var url = Path.Combine(Application.dataPath, inputName + ".wav");
            var audioClip = await NetworkHelper.GetAudioClipAsync(url, 50);
            _audioSource.clip = audioClip;
            // _audioSource.loop = true;
            _audioSource.Play();
        }
    }
}