namespace _3rdBy.ByFramework.Http.DownLoad.DownloadSystem.Core
{
    using System.IO;
    using UnityEngine;

    public static class PathUtil
    {
        public static string DownloadDir => Path.Combine(Application.persistentDataPath, "Downloads");

        public static string GetTempPath(string fileName)
        {
            if (!Directory.Exists(DownloadDir))
                Directory.CreateDirectory(DownloadDir);
            return Path.Combine(DownloadDir, fileName + ".tmp");
        }

        public static string GetFinalPath(string fileName)
        {
            return Path.Combine(DownloadDir, fileName);
        }
    }
}
