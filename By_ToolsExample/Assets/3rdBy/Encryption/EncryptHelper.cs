namespace Helper
{
    using System.Security.Cryptography;
    using System.Text;
    using System.IO;
    using System;

    /// <summary>
    /// 加密帮助类
    /// </summary>
    public abstract class EncryptHelper
    {
        private const string KeyDes = "xz&rykj^"; //8位
        private const string IvDes = "ryjob$01"; //8位
        private const string KeyAes = "$rykj^xuzhou^&js"; //16位
        private const string IvAes = "$js^rykj^xuzhou_"; //16位

        /// <summary>
        /// MD5加密
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string MD5Encrypt(string data)
        {
            if (data == null) return string.Empty;
            var md5 = MD5.Create();
            var bytes = Encoding.UTF8.GetBytes(data);
            var hashBytes = md5.ComputeHash(bytes);
            var sb = new StringBuilder();
            foreach (var hashByte in hashBytes)
            {
                sb.Append(hashByte.ToString("X2"));
            }

            return sb.ToString();
        }

        /// <summary>
        /// DES算法加密
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string DecEncrypt(string data)
        {
            return DesEncrypt(KeyDes, IvDes, data);
        }

        /// <summary>
        /// DES算法解密
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string DecDecrypt(string data)
        {
            return DesDecrypt(KeyDes, IvDes, data);
        }

        /// <summary>
        /// AES算法加密
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string AesEncrypt(string data)
        {
            return AesEncrypt(KeyAes, IvAes, data);
        }

        /// <summary>
        /// AES算法解密
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string AesDecrypt(string data)
        {
            return AesDecrypt(KeyAes, IvAes, data);
        }

        /// <summary>
        /// DES算法加密
        /// </summary>
        /// <param name="key">密钥，8位有效字符，不能出现中文</param>
        /// <param name="iv">偏移量，8位有效字符，不能出现中文</param>
        /// <param name="data">需要加密的字符串</param>
        /// <returns>DES加密后的字符串</returns>
        public static string DesEncrypt(string key, string iv, string data)
        {
            if (data == null) return string.Empty;
            var ivBytes = Encoding.UTF8.GetBytes(iv);
            var keyBytes = Encoding.UTF8.GetBytes(key);
            var base64Str = Encoding.Default.GetBytes(data);
            var desc = new DESCryptoServiceProvider
            {
                IV = ivBytes,
                Key = keyBytes
            };
            var encryptor = desc.CreateEncryptor();
            var ms = new MemoryStream();
            var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write);
            cs.Write(base64Str, 0, base64Str.Length);
            cs.FlushFinalBlock();
            return Convert.ToBase64String(ms.ToArray());
        }

        /// <summary>
        /// DES算法解密
        /// </summary>
        /// <param name="key">密钥，8位有效字符，不能出现中文</param>
        /// <param name="iv">偏移量，8位有效字符，不能出现中文</param>
        /// <param name="data">需要加密的字符串</param>
        /// <returns>DES解密后的字符串</returns>
        public static string DesDecrypt(string key, string iv, string data)
        {
            if (data == null) return string.Empty;
            var ivBytes = Encoding.UTF8.GetBytes(iv);
            var keyBytes = Encoding.UTF8.GetBytes(key);
            var base64Str = Convert.FromBase64String(data);
            var desc = new DESCryptoServiceProvider
            {
                IV = ivBytes,
                Key = keyBytes
            };
            var encryptor = desc.CreateDecryptor();
            var ms = new MemoryStream(base64Str, 0, base64Str.Length);
            var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Read);
            var sr = new StreamReader(cs, Encoding.Default);
            return sr.ReadToEnd();
        }

        /// <summary>
        /// AES算法加密
        /// </summary>
        /// <param name="key">密钥，16位有效字符，不能出现中文</param>
        /// <param name="iv">偏移量，16位有效字符，不能出现中文</param>
        /// <param name="data">需要加密的字符串</param>
        /// <returns>AES加密后的字符串</returns>
        public static string AesEncrypt(string key, string iv, string data)
        {
            if (data == null) return string.Empty;
            var keyBytes = Encoding.UTF8.GetBytes(key);
            var ivBytes = Encoding.UTF8.GetBytes(iv);
            var base64Str = Encoding.Default.GetBytes(data);

            var rijndaelManaged = new RijndaelManaged
            {
                Padding = PaddingMode.Zeros,
                Mode = CipherMode.CBC,
                KeySize = 128,
                BlockSize = 128
            };
            var encryptor = rijndaelManaged.CreateEncryptor(keyBytes, ivBytes);
            var ms = new MemoryStream();
            var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write);
            cs.Write(base64Str, 0, base64Str.Length);
            cs.FlushFinalBlock();
            return Convert.ToBase64String(ms.ToArray());
        }

        /// <summary>
        /// AES算法解密
        /// </summary>
        /// <param name="key">密钥，16位有效字符，不能出现中文</param>
        /// <param name="iv">偏移量，16位有效字符，不能出现中文</param>
        /// <param name="data">需要加密的字符串</param>
        /// <returns>AES解密后的字符串</returns>
        public static string AesDecrypt(string key, string iv, string data)
        {
            if (data == null) return string.Empty;
            var base64Str = Convert.FromBase64String(data);
            var ivBytes = Encoding.UTF8.GetBytes(iv);
            var keyBytes = Encoding.UTF8.GetBytes(key);

            var rijndaelManaged = new RijndaelManaged
            {
                Padding = PaddingMode.Zeros,
                Mode = CipherMode.CBC,
                KeySize = 128,
                BlockSize = 128
            };
            var encryptor = rijndaelManaged.CreateDecryptor(keyBytes, ivBytes);
            var ms = new MemoryStream(base64Str, 0, base64Str.Length);
            var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Read);
            var sr = new StreamReader(cs, Encoding.Default);
            return sr.ReadToEnd();
        }
    }
}