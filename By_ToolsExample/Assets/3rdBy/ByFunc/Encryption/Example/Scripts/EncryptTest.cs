using System;
using System.Globalization;
using System.Runtime.InteropServices;
using UnityEngine;

namespace _3rdBy.ByFunc.Encryption.Example.Scripts
{
    using Core;
    using Helper;

    /// <summary>
    /// 加密测试
    /// </summary>
    public class EncryptTest : MonoBehaviour
    {
        [ContextMenu("清除激活数据")]
        private void DeletedData()
        {
            PlayerPrefsHelper.DeleteAll();
        }

        [ContextMenu("删除激活文件")]
        private void DeletedFile()
        {
            FileHelper.DeletedFile("SecretKey", FileHelper.PathType.Desktop);
        }

        [ContextMenu("设置使用次数")]
        private void SetDataCount()
        {
            var computerInfo = HardwareInfo.GetComputerComponents();
            computerInfo.softwareInfo = new SoftwareInfo
            {
                softwareName   = Application.productName,
                activationTime = DateTime.Now.ToString(CultureInfo.InvariantCulture),
                limitType      = (int)SoftLimitType.Count,
                lifeDate       = new DateTime(2025, 4, 8).ToString(CultureInfo.InvariantCulture),
                useCount       = 2,
                isVrSupported  = false,
                engineerType   = 0
            };
            SetData(computerInfo);
        }

        [ContextMenu("设置使用时间")]
        private void SetDataTime()
        {
            var computerInfo = HardwareInfo.GetComputerComponents();
            computerInfo.softwareInfo = new SoftwareInfo
            {
                softwareName   = Application.productName,
                activationTime = new DateTime(2024, 4, 5).ToString(CultureInfo.InvariantCulture),
                limitType      = (int)SoftLimitType.Date,
                lifeDate       = new DateTime(2025, 4, 8).ToString(CultureInfo.InvariantCulture),
                useCount       = 3,
                isVrSupported  = false,
                engineerType   = 0
            };
            SetData(computerInfo);
        }

        [ContextMenu("设置永久使用")]
        private void SetDataForever()
        {
            var computerInfo = HardwareInfo.GetComputerComponents();
            computerInfo.softwareInfo = new SoftwareInfo
            {
                softwareName   = Application.productName,
                activationTime = DateTime.Now.ToString(CultureInfo.InvariantCulture),
                limitType      = (int)SoftLimitType.Forever,
                lifeDate       = new DateTime(2024, 4, 8).ToString(CultureInfo.InvariantCulture),
                useCount       = 3,
                isVrSupported  = false,
                engineerType   = 0
            };
            SetData(computerInfo);
        }

        [ContextMenu("检查激活状态")]
        private void CheckActivation()
        {
            ClientLockEntry.CheckActivation();
        }

        [ContextMenu("打开SecretKey文件桌面路径")]
        private void ShowExplorer()
        {
            FileHelper.ShowExplorer(FileHelper.DesktopPath, "SecretKey.txt");
        }

        [ContextMenu("打开文件浏览器")]
        public void OpenFileBrowser()
        {
#if UNITY_STANDALONE_WIN
            var ofn = new OpenFileName();
            ofn.structSize   = Marshal.SizeOf(ofn);
            ofn.filter       = "Txt Files (*.txt)\0*.txt\0All Files (*.*)\0*.*\0"; // 
            ofn.file         = new string(new char[256]);                          // 文件
            ofn.maxFile      = ofn.file.Length;                                    // 文件路径长度
            ofn.fileTitle    = new string(new char[64]);                           // 文件标题
            ofn.maxFileTitle = ofn.fileTitle.Length;                               // 文件标题长度
            ofn.initialDir   = Application.streamingAssetsPath.Replace('/', '\\'); // 设置初始目录
            ofn.title        = "Select a file";                                    // 标题
            ofn.defExt       = ".txt";                                             // 默认扩展名
            ofn.flags = Flags.OFN_EXPLORER
                        | Flags.OFN_FILEMUSTEXIST
                        | Flags.OFN_PATHMUSTEXIST
                        | Flags.OFN_NOCHANGEDIR
                        | Flags.OFN_ALLOWMULTISELECT;

            if (!WindowExplorer.GetOpenFileName(ofn)) return;
            secretKeyFilePath = ofn.file;
            Debug.Log("Selected file: " + ofn.file);
#endif
        }

        public string secretKeyFilePath;

        [ContextMenu("激活软件")]
        private void ActiveSoftWare()
        {
            // 此操作是重新写入激活文件，激活记录更新，且会出现一些问题，比如：同一个文件可以激活数次。
            var fileContent = FileHelper.ReadFile(secretKeyFilePath);
            var isActive    = VerifyCore.SoftWareActivation(fileContent);
            Debug.Log($"软件是否激活：{isActive}");
        }

        /// <summary>
        /// 设置激活数据并保存至相关路径
        /// </summary>
        /// <param name="computerInfo"></param>
        private static void SetData(ComputerInfo computerInfo)
        {
            var json   = JsonUtility.ToJson(computerInfo);
            var enData = EncryptHelper.AesEncrypt(json);
            var deData = EncryptHelper.AesDecrypt(enData);
            Debug.Log($"加密:\n{enData}\n解密:\n{deData}\n");
            FileHelper.WriteFile("SecretKey", $"{enData}", FileHelper.PathType.Desktop);
        }
    }
}