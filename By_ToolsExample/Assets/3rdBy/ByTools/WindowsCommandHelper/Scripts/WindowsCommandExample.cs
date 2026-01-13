namespace ByTools.WindowsCommandHelper.Scripts
{
    using System;
    using System.Diagnostics;
    using UnityEngine;
    using Debug = UnityEngine.Debug;

    /// <summary>
    /// 命令执行器
    /// </summary>
    public static class WindowsCommandExecutor
    {
        public static void RunAsAdmin(string command)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName        = "cmd.exe",
                    Arguments       = "/c " + command,
                    Verb            = "runas",
                    UseShellExecute = true,
                    CreateNoWindow  = true
                };
                Process.Start(psi);
                Debug.Log($"[命令执行器] 以管理员身份执行命令: {command}");
            }
            catch (Exception e)
            {
                Debug.LogError($"执行命令失败: {e.Message}");
            }
        }
    }

    /// <summary>
    /// 这是命令行工具的示例类
    /// </summary>
    public class WindowsCommandExample : MonoBehaviour
    {
        private Vector2 _scroll;
        private string _search = "";

        private void OnGUI()
        {
            GUILayout.Label("<b>Windows命令助手</b>", new GUIStyle(GUI.skin.label) { richText = true, fontSize = 18 });
            _search = GUILayout.TextField(_search, GUILayout.Width(300));

            _scroll = GUILayout.BeginScrollView(_scroll, GUILayout.Width(600), GUILayout.Height(400));
            foreach (var cmd in WindowsCommandData.Commands)
            {
                if (!string.IsNullOrEmpty(_search) && !cmd.command.ToLower().Contains(_search.ToLower()))
                    continue;

                GUILayout.BeginVertical("box");
                GUILayout.Label($"命令: {cmd.command}");
                GUILayout.Label($"描述: {cmd.description}");
                GUILayout.Label($"语法: {cmd.syntax}");
                GUILayout.Label($"示例: {cmd.example}");
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("📋 复制")) GUIUtility.systemCopyBuffer = cmd.example;
                if (GUILayout.Button("▶️ 运行")) WindowsCommandExecutor.RunAsAdmin(cmd.example);
                GUILayout.EndHorizontal();
                GUILayout.EndVertical();
            }

            GUILayout.EndScrollView();
        }
    }
}