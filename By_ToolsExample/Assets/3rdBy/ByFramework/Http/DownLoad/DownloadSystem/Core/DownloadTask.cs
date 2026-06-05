namespace _3rdBy.ByFramework.Http.DownLoad.DownloadSystem.Core
{
    using System.Threading;
    using Cysharp.Threading.Tasks;

    public class DownloadTask
    {
        public DownloadInfo Info;
        public IDownloadListener Listener;
        public CancellationToken Token;

        public UniTask Task => new UnityHttpDownloader().DownloadAsync(Info, Listener, Token);
    }
}
