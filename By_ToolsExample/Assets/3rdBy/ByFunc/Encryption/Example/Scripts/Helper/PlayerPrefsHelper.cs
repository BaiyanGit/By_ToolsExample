namespace _3rdBy.ByFunc.Encryption.Example.Scripts.Helper
{
    using System.Globalization;
    using UnityEngine;

    /// <summary>
    /// 注册表中存储的数据
    /// </summary>
    public class PlayerPrefsHelper
    {
        public static void DeleteKey(string key)
        {
            var keyMd5 = EncryptHelper.MD5Encrypt(key);
            PlayerPrefs.DeleteKey(keyMd5);
        }

        public static void DeleteAll()
        {
            PlayerPrefs.DeleteAll();
        }

        public static void Save()
        {
            PlayerPrefs.Save();
        }

        public static void SetBool(string key, bool value)
        {
            var keyMd5 = EncryptHelper.MD5Encrypt(key);
            var valueAes = EncryptHelper.AesEncrypt(value.ToString());
            PlayerPrefs.SetString(keyMd5, valueAes);
        }

        public static bool GetBool(string key, bool defaultValue = false)
        {
            var keyMd5 = EncryptHelper.MD5Encrypt(key);
            var valueAes = PlayerPrefs.GetString(keyMd5, defaultValue.ToString());
            if (valueAes.Equals(defaultValue.ToString()))
            {
                return bool.Parse(valueAes);
            }

            var value = EncryptHelper.AesDecrypt(valueAes);
            return bool.Parse(value);
        }

        public static void SetInt(string key, int value)
        {
            var keyMd5 = EncryptHelper.MD5Encrypt(key);
            var valueAes = EncryptHelper.AesEncrypt(value.ToString());
            PlayerPrefs.SetString(keyMd5, valueAes);
        }

        public static int GetInt(string key, int defaultValue = 0)
        {
            var keyMd5 = EncryptHelper.MD5Encrypt(key);
            var valueAes = PlayerPrefs.GetString(keyMd5, defaultValue.ToString());
            var value = EncryptHelper.AesDecrypt(valueAes);
            return int.Parse(value);
        }

        public static void SetString(string key, string value)
        {
            var keyMd5 = EncryptHelper.MD5Encrypt(key);
            var valueAes = EncryptHelper.AesEncrypt(value);
            PlayerPrefs.SetString(keyMd5, valueAes);
        }

        public static string GetString(string key, string defaultValue = "")
        {
            var keyMd5 = EncryptHelper.MD5Encrypt(key);
            var valueAes = PlayerPrefs.GetString(keyMd5, defaultValue);
            return EncryptHelper.AesDecrypt(valueAes);
        }

        public static void SetFloat(string key, float value)
        {
            var keyMd5 = EncryptHelper.MD5Encrypt(key);
            var valueAes = EncryptHelper.AesEncrypt(value.ToString(CultureInfo.InvariantCulture));
            PlayerPrefs.SetString(keyMd5, valueAes);
        }

        public static float GetFloat(string key, float defaultValue = 0f)
        {
            var keyMd5 = EncryptHelper.MD5Encrypt(key);
            var valueAes = PlayerPrefs.GetString(keyMd5, defaultValue.ToString(CultureInfo.InvariantCulture));
            var value = EncryptHelper.AesDecrypt(valueAes);
            return float.Parse(value);
        }

        public static void SetLong(string key, long value)
        {
            var keyMd5 = EncryptHelper.MD5Encrypt(key);
            var valueAes = EncryptHelper.AesEncrypt(value.ToString());
            PlayerPrefs.SetString(keyMd5, valueAes);
        }

        public static long GetLong(string key, long defaultValue = 0)
        {
            var keyMd5 = EncryptHelper.MD5Encrypt(key);
            var valueAes = PlayerPrefs.GetString(keyMd5, defaultValue.ToString());
            var value = EncryptHelper.AesDecrypt(valueAes);
            return long.Parse(value);
        }

        public static void SetDouble(string key, double value)
        {
            var keyMd5 = EncryptHelper.MD5Encrypt(key);
            var valueAes = EncryptHelper.AesEncrypt(value.ToString(CultureInfo.InvariantCulture));
            PlayerPrefs.SetString(keyMd5, valueAes);
        }

        public static double GetDouble(string key, double defaultValue = 0)
        {
            var keyMd5 = EncryptHelper.MD5Encrypt(key);
            var valueAes = PlayerPrefs.GetString(keyMd5, defaultValue.ToString(CultureInfo.InvariantCulture));
            var value = EncryptHelper.AesDecrypt(valueAes);
            return double.Parse(value);
        }

        public static void SetDate(string key, System.DateTime value)
        {
            var keyMd5 = EncryptHelper.MD5Encrypt(key);
            var valueAes = EncryptHelper.AesEncrypt(value.ToFileTime().ToString());
            PlayerPrefs.SetString(keyMd5, valueAes);
        }

        public static System.DateTime GetDate(string key, System.DateTime defaultValue = default)
        {
            var keyMd5 = EncryptHelper.MD5Encrypt(key);
            var valueAes = PlayerPrefs.GetString(keyMd5, defaultValue.ToString(CultureInfo.InvariantCulture));
            var value = EncryptHelper.AesDecrypt(valueAes);
            return System.DateTime.FromFileTime(long.Parse(value));
        }

        public static void SetDateUtc(string key, System.DateTime value)
        {
            var keyMd5 = EncryptHelper.MD5Encrypt(key);
            var valueAes = EncryptHelper.AesEncrypt(value.ToFileTimeUtc().ToString());
            PlayerPrefs.SetString(keyMd5, valueAes);
        }

        public static System.DateTime GetDateUtc(string key, System.DateTime defaultValue = default)
        {
            var keyMd5 = EncryptHelper.MD5Encrypt(key);
            var valueAes = PlayerPrefs.GetString(keyMd5, defaultValue.ToString(CultureInfo.InvariantCulture));
            var value = EncryptHelper.AesDecrypt(valueAes);
            return System.DateTime.FromFileTimeUtc(long.Parse(value));
        }
    }
}