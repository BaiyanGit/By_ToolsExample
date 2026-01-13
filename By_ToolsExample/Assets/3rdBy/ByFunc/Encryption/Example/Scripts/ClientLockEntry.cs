using System;
using Helper;
using UnityEngine;

/// <summary>
/// 软件锁入口
/// </summary>
public class ClientLockEntry : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Init()
    {
        InitEvent();
        CheckActivation();
    }

    private static void InitEvent()
    {
        VerifyCore.OnVerifyCallback += OnVerifyCallback;
    }

    private static void OnVerifyCallback(int code, string msg)
    {
        Debug.Log($"验证结果:{code} {msg}");
    }

    /// <summary>
    /// 检查激活状态
    /// </summary>
    public static void CheckActivation()
    {
        if (VerifyCore.GetStorageData() == null)
        {
            try
            {
                var isSelf = VerifyCore.VerifyDevice(GetActivationKey(), out var computerInfo);
                if (!isSelf)
                {
                    Debug.Log("激活码与您的机器不一致!");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"激活码文件不存在或激活码错误!\n{e.Message}");
            }
        }
        else
        {
            VerifyCore.VerifyData();
        }
    }

    /// <summary>
    /// 读取激活码文件
    /// </summary>
    /// <returns></returns>
    private static string GetActivationKey()
    {
        return FileHelper.ReadFile("SecretKey", FileHelper.PathType.Desktop);
    }
}