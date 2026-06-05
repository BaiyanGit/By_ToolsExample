namespace _3rdBy.ByFramework.Http.DownLoad.DownloadSystem.Core
{
    using System.Collections.Generic;
    using Cysharp.Threading.Tasks;
    using UnityEngine;

    public class DownloadManager : MonoBehaviour
    {
        public static DownloadManager Instance { get; private set; }

        private readonly Queue<DownloadTask> _taskQueue = new();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticState()
        {
            Instance = null;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public void Enqueue(DownloadTask task)
        {
            _taskQueue.Enqueue(task);
            if (_taskQueue.Count == 1)
                ProcessQueue().Forget();
        }

        private async UniTaskVoid ProcessQueue()
        {
            while (_taskQueue.Count > 0)
            {
                var task = _taskQueue.Dequeue();
                await task.Task;
            }
        }
    }
}
