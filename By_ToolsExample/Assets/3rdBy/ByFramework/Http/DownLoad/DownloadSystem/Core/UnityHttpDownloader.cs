namespace _3rdBy.ByFramework.Http.DownLoad.DownloadSystem.Core
{
    using System;
    using System.IO;
    using System.Threading;
    using Cysharp.Threading.Tasks;
    using UnityEngine;
    using UnityEngine.Networking;

    public class UnityHttpDownloader
    {
        private const int Gb = 1024 * 1024 * 1024;
        private const int Mb = 1024 * 1024;
        private const int Kb = 1024;

        private void DownloadedShow(ulong downloadedBytes)
        {
            var gbSize = downloadedBytes / Gb;
            var mbPart = downloadedBytes % Gb / Mb;
            var kbPart = downloadedBytes % Mb / Kb;
            var bPart = downloadedBytes % Kb;
            try
            {
                if (gbSize > 0)
                {
                    Debug.Log($"Downloaded: {gbSize} GB / {mbPart} MB / {kbPart} KB / {bPart} B");
                }
                else if (mbPart > 0)
                {
                    Debug.Log($"Downloaded: {mbPart} MB / {kbPart} KB / {bPart} B");
                }
                else if (kbPart > 0)
                {
                    Debug.Log($"Downloaded: {kbPart} KB / {bPart} B");
                }
                else
                {
                    Debug.Log($"Downloaded: {bPart} B");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error in calculating downloaded size: {ex.Message}");
            }
        }

        public async UniTask DownloadAsync(DownloadInfo info, IDownloadListener listener = null,
            CancellationToken ct = default)
        {
            // 使用 Path.Combine 构造路径，避免分隔符错误
            var downloadsDir = Path.Combine(Application.persistentDataPath, "Downloads");

            // 确保下载目录存在
            if (!Directory.Exists(downloadsDir))
                Directory.CreateDirectory(downloadsDir);

            // 临时文件和最终文件路径
            var tempPath = Path.Combine(downloadsDir, info.FileName + ".tmp");
            var finalPath = Path.Combine(downloadsDir, info.FileName);

            long existingLength = 0;
            Debug.Log($"[Download URL]={info.Url}\n[TempPath]={tempPath}\n[SavePath]={finalPath}");

            // 确保临时文件目录存在
            var tempDir = Path.GetDirectoryName(tempPath);
            if (!Directory.Exists(tempDir))
            {
                Directory.CreateDirectory(tempDir);
            }

            // 确保最终文件目录存在
            var finalDir = Path.GetDirectoryName(finalPath);
            if (!Directory.Exists(finalDir))
            {
                Directory.CreateDirectory(finalDir);
            }

            // 先判断文件是否存在
            if (File.Exists(tempPath))
            {
                existingLength = new FileInfo(tempPath).Length;
                Debug.Log($"Existing file size: {existingLength}");
            }

            using var request = UnityWebRequest.Get(info.Url);

            if (existingLength > 0)
            {
                request.SetRequestHeader("Range", $"bytes={existingLength}-");
            }
            else
            {
                Debug.Log("Start downloading");
            }

            request.downloadHandler = new DownloadHandlerFile(tempPath, true);

            var op = request.SendWebRequest();

            ulong lastDownloadedBytes = 0;
            var lastUpdateTime = DateTime.UtcNow;

            while (!op.isDone)
            {
                ct.ThrowIfCancellationRequested();

                if (request.downloadedBytes > 0)
                {
                    var downloadedSize = request.downloadedBytes + (ulong)existingLength;
                    // 计算下载速度（bytes/s）
                    var now = DateTime.UtcNow;
                    var deltaTime = (now - lastUpdateTime).TotalSeconds;
                    if (deltaTime > 0)
                    {
                        var deltaBytes = downloadedSize - lastDownloadedBytes;
                        var speed = deltaBytes / deltaTime;

                        // 反馈下载进度和速度
                        listener?.OnProgress(info, downloadedSize, request.downloadProgress, speed);

                        lastDownloadedBytes = downloadedSize;
                        lastUpdateTime = now;

                        Debug.Log($"Progress: {request.downloadProgress:P2}, Speed: {speed / 1024f:F2} KB/s");
                    }
                }

                await UniTask.Yield(ct);
            }

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Download failed: {request.error}");
                listener?.OnError(info, request.error);
                return;
            }

            try
            {
                // 下载完成，移动文件
                if (File.Exists(finalPath))
                {
                    File.Delete(finalPath);
                }

                File.Move(tempPath, finalPath);
                Debug.Log($"Download completed: {finalPath}");
                listener?.OnCompleted(info);
            }
            catch (Exception ex)
            {
                Debug.LogError($"File save failed: {ex.Message}");
                listener?.OnError(info, ex.Message);

                // 删除临时文件避免残留
                if (File.Exists(tempPath))
                {
                    try
                    {
                        File.Delete(tempPath);
                    }
                    catch (Exception deleteEx)
                    {
                        Debug.LogError($"Delete temp file failed: {deleteEx.Message}");
                    }
                }
            }
        }
    }
}