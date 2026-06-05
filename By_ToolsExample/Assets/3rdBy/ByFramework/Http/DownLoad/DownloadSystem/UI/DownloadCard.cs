using UnityEngine;
using UnityEngine.UI;

namespace _3rdBy.ByFramework.Http.DownLoad.DownloadSystem.UI
{
    using Core;

    public class DownloadCard : MonoBehaviour, IDownloadListener
    {
        public Text FileNameText;
        public Slider ProgressBar;
        public Text SpeedText;
        public Text StatusText;


        private void Start()
        {
            var info = new DownloadInfo("file:///E:/UnityProjects/GitLab/福州装载.zip", "福州装载.zip");
            Init(info);
            // DownloadManager.Instance.Enqueue(new DownloadTask
            // {
            //     Info = info,
            //     Listener = this,
            //     Token = CancellationToken.None
            // });
        }

        public void Init(DownloadInfo info)
        {
            FileNameText.text = info.FileName;
            StatusText.text   = "Waiting...";
            DownloadManager.Instance.Enqueue(new DownloadTask { Info = info, Listener = this });
        }

        public void OnProgress(DownloadInfo info, ulong downloadedBytes, float progress, double speed)
        {
            if (ProgressBar != null)
            {
                ProgressBar.value = progress;
            }

            SpeedText.text  = $"{speed / 1024f:F2} KB/s";
            StatusText.text = "Downloading...";
        }

        public void OnCompleted(DownloadInfo info)
        {
            StatusText.text = "Completed";
        }

        public void OnError(DownloadInfo info, string error)
        {
            StatusText.text = $"Error: {error}";
        }
    }
}