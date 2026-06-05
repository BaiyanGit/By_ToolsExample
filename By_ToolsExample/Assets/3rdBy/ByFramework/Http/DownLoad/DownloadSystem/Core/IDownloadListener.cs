
namespace _3rdBy.ByFramework.Http.DownLoad.DownloadSystem.Core
{
    public interface IDownloadListener
    {
        void OnProgress(DownloadInfo info, ulong downloadedBytes, float progress, double speed);
        void OnCompleted(DownloadInfo info);
        void OnError(DownloadInfo info, string error);
    }
}
