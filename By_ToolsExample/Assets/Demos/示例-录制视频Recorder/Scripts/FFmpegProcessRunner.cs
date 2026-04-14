//=====================================================
// 文件名称: FFmpegProcessRunner
// 创 建 者: wangbaiyan
// 创建日期: 2026-4-13
// 描    述: FFmpeg 进程运行器，用于启动、停止并管理 ffmpeg 录屏进程。
//=====================================================

using System;
using System.Diagnostics;
using UnityEngine;

public class FFmpegProcessRunner : IDisposable
{
    [Header("ffmpeg进程实例")] private Process _ffmpegProcess;

    /// <summary>
    /// 当前 ffmpeg 进程是否正在运行。
    /// </summary>
    public bool IsRunning => _ffmpegProcess != null && !_ffmpegProcess.HasExited;

    /// <summary>
    /// 启动 ffmpeg 进程。
    /// </summary>
    /// <param name="executablePath">ffmpeg 可执行文件路径。</param>
    /// <param name="arguments">ffmpeg 启动参数。</param>
    /// <param name="onStdOut">标准输出回调。</param>
    /// <param name="onStdErr">标准错误回调。</param>
    /// <returns>是否启动成功。</returns>
    public bool Start(
        string executablePath,
        string arguments,
        Action<string> onStdOut = null,
        Action<string> onStdErr = null)
    {
        if (IsRunning)
        {
            return false;
        }

        try
        {
            _ffmpegProcess                                  = new Process();
            _ffmpegProcess.StartInfo.FileName               = executablePath;
            _ffmpegProcess.StartInfo.Arguments              = arguments;
            _ffmpegProcess.StartInfo.UseShellExecute        = false;
            _ffmpegProcess.StartInfo.CreateNoWindow         = true;
            _ffmpegProcess.StartInfo.RedirectStandardInput  = true;
            _ffmpegProcess.StartInfo.RedirectStandardOutput = true;
            _ffmpegProcess.StartInfo.RedirectStandardError  = true;

            _ffmpegProcess.OutputDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    onStdOut?.Invoke(e.Data);
                }
            };

            _ffmpegProcess.ErrorDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    onStdErr?.Invoke(e.Data);
                }
            };

            _ffmpegProcess.Start();
            _ffmpegProcess.BeginOutputReadLine();
            _ffmpegProcess.BeginErrorReadLine();

            return true;
        }
        catch
        {
            Cleanup();
            throw;
        }
    }

    /// <summary>
    /// 停止 ffmpeg 进程。
    /// </summary>
    /// <param name="waitMilliseconds">等待退出的超时时间，单位毫秒。</param>
    public void Stop(int waitMilliseconds = 3000)
    {
        if (!IsRunning)
        {
            Cleanup();
            return;
        }

        try
        {
            _ffmpegProcess.StandardInput.Write('q');
            _ffmpegProcess.StandardInput.Flush();

            if (!_ffmpegProcess.WaitForExit(waitMilliseconds))
            {
                _ffmpegProcess.Kill();
            }
        }
        finally
        {
            Cleanup();
        }
    }

    /// <summary>
    /// 释放当前进程资源。
    /// </summary>
    public void Dispose()
    {
        try
        {
            Stop(1000);
        }
        catch
        {
            Cleanup();
        }
    }

    /// <summary>
    /// 清理 ffmpeg 进程对象。
    /// </summary>
    private void Cleanup()
    {
        if (_ffmpegProcess == null)
        {
            return;
        }

        try
        {
            _ffmpegProcess.Close();
        }
        catch
        {
            // ignored
        }

        try
        {
            _ffmpegProcess.Dispose();
        }
        catch
        {
            // ignored
        }

        _ffmpegProcess = null;
    }
}