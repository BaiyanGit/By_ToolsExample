using System;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace _3rdBy.ByFunc.Encryption.Scripts
{
    /// <summary>
    /// 加密解密管理
    /// </summary>
    public static class CryptoMgr
    {
        /// <summary>
        /// 加密字符串
        /// </summary>
        /// <param name="value"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static string EncryptStr(string value, string key)
        {
            try
            {
                var keyArray       = Encoding.UTF8.GetBytes(key);
                var toEncryptArray = Encoding.UTF8.GetBytes(value);
                var rijndael       = new RijndaelManaged();
                rijndael.Key     = keyArray;
                rijndael.Mode    = CipherMode.ECB;
                rijndael.Padding = PaddingMode.PKCS7;
                var cTransform  = rijndael.CreateEncryptor();
                var resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
                return Convert.ToBase64String(resultArray, 0, resultArray.Length);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                return null;
            }
        }

        /// <summary>
        /// 解密字符串
        /// </summary>
        /// <param name="value"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static string DecryptStr(string value, string key)
        {
            try
            {
                var keyArray       = Encoding.UTF8.GetBytes(key);
                var toEncryptArray = Convert.FromBase64String(value);
                var rijndael       = new RijndaelManaged();
                rijndael.Key     = keyArray;
                rijndael.Mode    = CipherMode.ECB;
                rijndael.Padding = PaddingMode.PKCS7;
                var cTransform  = rijndael.CreateDecryptor();
                var resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
                return Encoding.UTF8.GetString(resultArray);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                return null;
            }
        }

        /// <summary>
        /// AES 算法加密(ECB模式) 将明文加密
        /// </summary>
        /// <param name="toEncryptArray">明文</param>
        /// <param name="key">密钥</param>
        /// <returns>加密后base64编码的密文</returns>
        public static byte[] AesEncrypt(byte[] toEncryptArray, string key)
        {
            try
            {
                var keyArray = Encoding.UTF8.GetBytes(key);

                var rDel = new RijndaelManaged();
                rDel.Key     = keyArray;
                rDel.Mode    = CipherMode.ECB;
                rDel.Padding = PaddingMode.PKCS7;

                var cTransform  = rDel.CreateEncryptor();
                var resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);

                return resultArray;
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                return null;
            }
        }

        /// <summary>
        /// AES 算法解密(ECB模式) 将密文base64解码进行解密，返回明文
        /// </summary>
        /// <param name="toDecryptArray">密文</param>
        /// <param name="key">密钥</param>
        /// <returns>明文</returns>
        public static byte[] AesDecrypt(byte[] toDecryptArray, string key)
        {
            try
            {
                var keyArray = Encoding.UTF8.GetBytes(key);

                var rDel = new RijndaelManaged();
                rDel.Key     = keyArray;
                rDel.Mode    = CipherMode.ECB;
                rDel.Padding = PaddingMode.PKCS7;

                var cTransform  = rDel.CreateDecryptor();
                var resultArray = cTransform.TransformFinalBlock(toDecryptArray, 0, toDecryptArray.Length);
                return resultArray;
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                return null;
            }
        }

        /// <summary>
        /// AES 算法加密(ECB模式) 无padding填充，用于分块解密
        /// </summary>
        /// <param name="toEncryptArray">明文</param>
        /// <param name="key">密钥</param>
        /// <returns>加密后base64编码的密文</returns>
        public static byte[] AesEncryptWithNoPadding(byte[] toEncryptArray, string key)
        {
            try
            {
                var keyArray = Encoding.UTF8.GetBytes(key);

                var rDel = new RijndaelManaged();
                rDel.Key     = keyArray;
                rDel.Mode    = CipherMode.ECB;
                rDel.Padding = PaddingMode.None;

                var cTransform  = rDel.CreateEncryptor();
                var resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);

                return resultArray;
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                return null;
            }
        }

        /// <summary>
        /// AES 算法解密(ECB模式) 无padding填充，用于分块解密
        /// </summary>
        /// <param name="toDecryptArray">密文</param>
        /// <param name="key">密钥</param>
        /// <returns>明文</returns>
        public static byte[] AesDecryptWithNoPadding(byte[] toDecryptArray, string key)
        {
            try
            {
                var keyArray = Encoding.UTF8.GetBytes(key);

                var rDel = new RijndaelManaged();
                rDel.Key     = keyArray;
                rDel.Mode    = CipherMode.ECB;
                rDel.Padding = PaddingMode.None;

                var cTransform  = rDel.CreateDecryptor();
                var resultArray = cTransform.TransformFinalBlock(toDecryptArray, 0, toDecryptArray.Length);
                return resultArray;
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                return null;
            }
        }
    }
}