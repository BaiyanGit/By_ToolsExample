
namespace _3rdBy.ByFramework.Http.DownLoad.DownloadSystem.Core
{
    public class DownloadInfo
    {
        public string Url;
        public string FileName;

        public DownloadInfo(string url, string fileName)
        {
            Url = url;
            FileName = fileName;
        }
    }
}
