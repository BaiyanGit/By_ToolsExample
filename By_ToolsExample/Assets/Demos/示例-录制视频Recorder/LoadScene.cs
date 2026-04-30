namespace Demos.示例_录制视频Recorder
{
    using System;
    using System.Diagnostics;
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using Debug = UnityEngine.Debug;

    public class LoadScene : MonoBehaviour
    {
        public KeyCode keyR = KeyCode.R;
        public KeyCode keyQ = KeyCode.Q;
        public KeyCode keyScape = KeyCode.Space;
        public KeyCode keyEscape = KeyCode.Escape;


        public AudioSource audioSource;

        private void Update()
        {
            if (Input.GetKeyDown(keyR))
            {
                // 重新加载此场景
                SceneManager.LoadScene(0);
            }

            if (Input.GetKeyDown(keyQ))
            {
#if UNITY_EDITOR
                // 停止播放
                UnityEditor.EditorApplication.isPlaying = false;
#else
                // 退出游戏
                Application.Quit();
#endif
            }

            if (Input.GetKeyDown(keyScape))
            {
                Debug.LogError(audioSource.isPlaying);
                // 暂停音效
                if (audioSource.isPlaying)
                {
                    audioSource.Pause();
                }
                else
                {
                    audioSource.Play();
                }
            }


            if (Input.GetKeyDown(keyEscape))
            {
                // 最小化窗口
                MinimizeWindow();
            }
        }


        #region 最小化窗口

        [ContextMenu("Minimize Window")]
        public void MinimizeWindow()
        {
            // 获取当前窗口的标题
            string windowTitle = Application.productName;

            // 构建要执行的命令
            // 说明: -r 参数用于指定窗口，:ACTIVE: 代表当前活动窗口
            // -b 参数用于修改窗口状态，add,minimized 表示添加最小化状态
            string command = $"wmctrl -r :ACTIVE: -b add,minimized";

            ExecuteBashCommand(command);
        }

        [ContextMenu("Restore Window")]
        public void RestoreWindow()
        {
            // 恢复窗口的命令 (移除最小化状态)
            string command = $"wmctrl -r :ACTIVE: -b remove,minimized";
            ExecuteBashCommand(command);
        }

        private static void ExecuteBashCommand(string command)
        {
            try
            {
                var processInfo = new ProcessStartInfo()
                {
                    FileName               = "/bin/bash",
                    Arguments              = $"-c \"{command}\"",
                    RedirectStandardOutput = true,
                    UseShellExecute        = false,
                    CreateNoWindow         = true,
                };

                using var process = Process.Start(processInfo);
                process?.WaitForExit();
            }
            catch (Exception e)
            {
                Debug.LogError($"执行 最小化窗口 命令失败: {command}\n错误信息: {e.Message}");
            }
        }

        #endregion
    }
}