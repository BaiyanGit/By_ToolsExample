namespace _3rdBy.ByFunc.Encryption.Example.Scripts.Core
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using Helper;
    using UnityEngine;

    /// <summary>
    /// 验证核心类
    /// </summary>
    public abstract class VerifyCore
    {
        private const string UseSoftKey = "UseSoftKey"; // 使用软件密钥

        public delegate void VerifyCallback(int code, string msg);

        /// <summary>
        /// 验证回调
        /// </summary>
        public static event VerifyCallback OnVerifyCallback;

        /// <summary>
        /// 软件激活
        /// </summary>
        /// <param name="secretKey">激活码</param>
        /// <returns>软件是否已被激活</returns>
        public static bool SoftWareActivation(string secretKey)
        {
            // 激活码是否被使用过
            if (CheckKeyUsage(secretKey))
            {
                Debug.Log("激活码已使用过！");
                return false;
            }

            var isSelfComputer = VerifyDevice(secretKey, out var computerInfo); // 验证当前激活码是否是本机
            if (isSelfComputer)
            {
                SaveInitData(computerInfo);
            }

            return isSelfComputer;
        }

        /// <summary>
        /// 检查或保存密钥是否被使用过
        /// </summary>
        private static bool CheckKeyUsage(string secretKey)
        {
            bool isUse;
            var  keys = PlayerPrefs.GetString("UseKey"); //获取已经使用的激活码
            // 没有使用过
            if (string.IsNullOrEmpty(keys))
            {
                SecretKeyData keyData = new()
                {
                    key = new List<string> { secretKey }
                };
                var keyJson = JsonUtility.ToJson(keyData);
                PlayerPrefs.SetString("UseKey", keyJson);
                isUse = false;
            }
            else
            {
                var localData = JsonUtility.FromJson<SecretKeyData>(keys);
                if (localData.key.Contains(secretKey))
                {
                    isUse = true; //激活码已使用过
                }
                else
                {
                    localData.key.Add(secretKey);
                    var keyJson = JsonUtility.ToJson(localData);
                    PlayerPrefs.SetString("UseKey", keyJson);
                    isUse = false;
                }
            }

            return isUse;
        }

        /// <summary>
        /// 保存激活数据
        /// </summary>
        private static void SaveInitData(ComputerInfo computerInfo)
        {
            computerInfo.softwareInfo.openTime = DateTime.Now.ToString(CultureInfo.InvariantCulture);
            var strJson = JsonUtility.ToJson(computerInfo);
            PlayerPrefsHelper.SetString(UseSoftKey, strJson); //保存使用数据至注册表
            // Debug.Log($"激活码验证成功!\n{strJson}");
            OnOnVerifyCallback(VerifyCode.Success);
            VerifyData();
        }

        /// <summary>
        /// 保存激活或者每次打记录限制的数据
        /// </summary>
        /// <param name="computerInfo"></param>
        private static void SaveComputerInfo(ComputerInfo computerInfo)
        {
            PlayerPrefsHelper.SetString(UseSoftKey, JsonUtility.ToJson(computerInfo));
        }

        /// <summary>
        /// 获取注册表中的激活数据
        /// </summary>
        /// <returns></returns>
        public static ComputerInfo GetStorageData()
        {
            var jsonData = PlayerPrefsHelper.GetString(UseSoftKey);
            // Debug.Log($"激活成功，数据:\n{jsonData}");
            if (string.IsNullOrEmpty(jsonData))
            {
                return null;
            }

            var computerInfo = JsonUtility.FromJson<ComputerInfo>(jsonData);
            return computerInfo;
        }

        /// <summary>
        /// 验证设备硬件信息是否是本机
        /// </summary>
        /// <param name="jsonData"></param>
        /// <param name="deComputerInfo"></param>
        /// <returns></returns>
        public static bool VerifyDevice(string data, out ComputerInfo computerInfo)
        {
            var native   = HardwareInfo.GetComputerComponents();
            var jsonData = EncryptHelper.AesDecrypt(data);

            computerInfo = JsonUtility.FromJson<ComputerInfo>(jsonData);
            return computerInfo.deviceName == native.deviceName &&
                   computerInfo.deviceModel == native.deviceModel &&
                   computerInfo.deviceUniqueIdentifier == native.deviceUniqueIdentifier &&
                   computerInfo.graphicsDeviceID == native.graphicsDeviceID;
        }

        /// <summary>
        /// 验证数据是否达到限制
        /// </summary>
        public static void VerifyData()
        {
            var computerInfo = GetStorageData();
            var softwareInfo = computerInfo.softwareInfo;

            if (softwareInfo.softwareName != Application.productName)
            {
                OnOnVerifyCallback(VerifyCode.SoftwareNameError);
                return;
            }

            switch ((SoftLimitType)softwareInfo.limitType)
            {
                case SoftLimitType.Date:
                    VerifyDateLimit(computerInfo);
                    break;
                case SoftLimitType.Count:
                    VerifyCountLimit(computerInfo);
                    break;
                case SoftLimitType.Forever:
                    OnOnVerifyCallback(VerifyCode.Forever);
                    Debug.Log("永久使用！");
                    break;
                case SoftLimitType.None:
                default:
                    OnOnVerifyCallback(VerifyCode.UnknownLimitType);
                    break;
            }
        }

        /// <summary>
        /// 验证时间限制
        /// </summary>
        /// <param name="softwareInfo"></param>
        /// <param name="computerInfo"></param>
        private static void VerifyDateLimit(ComputerInfo computerInfo)
        {
            var softwareInfo = computerInfo.softwareInfo;
            var activeTime   = DateTime.Parse(softwareInfo.activationTime);
            var openTime     = DateTime.Parse(softwareInfo.openTime);
            var lifeDate     = DateTime.Parse(softwareInfo.lifeDate);

            // 使用时间已到
            if (DateTime.Now > lifeDate)
            {
                OnOnVerifyCallback(VerifyCode.TimeOut);
                return;
            }

            // 用户修改在激活日期之前
            if (DateTime.Now < activeTime)
            {
                OnOnVerifyCallback(VerifyCode.SystemTimeError);
                return;
            }

            // 用户修改在激活日期和打开日期之间
            if (DateTime.Now < openTime)
            {
                OnOnVerifyCallback(VerifyCode.SystemTimeChange);
                return;
            }

            Debug.Log("在只用期限内！");
            softwareInfo.openTime = DateTime.Now.ToString(CultureInfo.InvariantCulture);
            SaveComputerInfo(computerInfo);
        }

        /// <summary>
        /// 验证次数限制
        /// </summary>
        /// <param name="softwareInfo"></param>
        /// <param name="computerInfo"></param>
        private static void VerifyCountLimit(ComputerInfo computerInfo)
        {
            var softwareInfo = computerInfo.softwareInfo;
            if (softwareInfo.useCount <= 0)
            {
                OnOnVerifyCallback(VerifyCode.UseCountOut);
                return;
            }

            softwareInfo.useCount--;
            Debug.Log($"剩余次数：{softwareInfo.useCount}");
            SaveComputerInfo(computerInfo);
        }

        /// <summary>
        /// 验证回调
        /// </summary>
        /// <param name="code"></param>
        private static void OnOnVerifyCallback(int code)
        {
            OnVerifyCallback?.Invoke(code, VerifyCode.VerifyCodeDic[code]);
        }
    }

    /// <summary>
    /// 验证提示码
    /// </summary>
    public abstract class VerifyCode
    {
        public const int Success = 10000;           //验证成功
        public const int Forever = 10001;           //永久使用
        public const int SoftwareNameError = 10002; //软件名称不匹配
        public const int UnknownLimitType = 10003;  //未知的限制类型
        public const int TimeOut = 10100;           //使用时间已到
        public const int SystemTimeError = 10101;   //系统时间错误
        public const int SystemTimeChange = 10102;  //系统检测到您修改了电脑时间
        public const int UseCountOut = 10200;       //该软件体验次数已用完

        public static readonly Dictionary<int, string> VerifyCodeDic = new()
        {
            { Success, "激活码验证成功！" },
            { Forever, "永久使用！" },
            { SoftwareNameError, "软件名称不匹配，请联系管理员！" },
            { UnknownLimitType, "未知的限制类型，请联系管理员！" },
            { TimeOut, "使用时间已到，请联系管理员！" },
            { SystemTimeError, "系统时间错误，请联系管理员！" },
            { SystemTimeChange, "系统检测到作弊行为，请不要尝试修改时间！" },
            { UseCountOut, "该软件体验次数已用完，请联系管理员！" }
        };
    }
}