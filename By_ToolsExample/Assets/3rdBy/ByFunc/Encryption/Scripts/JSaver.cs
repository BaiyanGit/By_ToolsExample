namespace _3rdBy.ByFunc.Encryption.Scripts
{
    using System;
    using System.Linq;
    using System.Text;
    using ByFramework.Http;
    using UnityEngine;

    public static class JSaver
    {
        private const string JSaverKeys = "JSAVER_KEYS";

        /// <summary>
        /// 加密字符串用来混淆16位
        /// </summary>
        private const string DefaultEncryptKey = "encrypt_fuck_key";

        private const int Length = 16;

        private static void AddJSaverKeys(string key)
        {
            var s = PlayerPrefs.GetString(JSaverKeys, key);
            if (!s.Split(',').ToList().Contains(key))
            {
                var sb = new StringBuilder(s);
                sb.Append($",{key}");
                s = sb.ToString();
            }

            PlayerPrefs.SetString(JSaverKeys, s);
        }

        /// <summary>
        /// Save a data to local storage as string
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="val"></param>
        /// <param name="encryptKey"></param>
        /// <returns>Saved value</returns>
        public static string SaveAsString<T>(string dataName, T val, string encryptKey = null)
        {
            if (String.IsNullOrEmpty(encryptKey))
            {
                encryptKey = DefaultEncryptKey;
            }

            if (encryptKey.Length != Length)
            {
                var ex = new Exception("encryptKey needs to be 16 characters!");
                Debug.LogError($"[JSaver] 错误：{ex.Message}, {ex.Data["StackTrace"]}");
                return null;
            }

            var strData = val.ToString();
            var result = CryptoMgr.EncryptStr(strData, encryptKey);

            PlayerPrefs.SetString(dataName, result);
            AddJSaverKeys(dataName);
            return result;
        }

        /// <summary>
        /// Save a data to local storage as JSON
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="val"></param>
        /// <param name="encryptKey"></param>
        /// <returns>Saved value</returns>
        public static string SaveAsJSON<T>(string dataName, T val, string encryptKey = null)
        {
            if (String.IsNullOrEmpty(encryptKey))
            {
                encryptKey = DefaultEncryptKey;
            }

            if (encryptKey.Length != Length)
            {
                var ex = new Exception("encryptKey needs to be 16 characters!");
                Debug.LogError($"[JSaver] 错误：{ex.Message}, {ex.Data["StackTrace"]}");
                return null;
            }

            try
            {
                var strData = StringifyHelper.JsonSerialize(val);
                var result = CryptoMgr.EncryptStr(strData, encryptKey);
                PlayerPrefs.SetString(dataName, result);
                AddJSaverKeys(dataName);
                return result;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[JSaver] 错误：{ex.Message}, {ex.Data["StackTrace"]}");
                return null;
            }
        }

        /// <summary>
        /// Get string from local storage
        /// </summary>
        /// <param name="val"></param>
        /// <param name="encryptKey"></param>
        /// <returns>Saved value</returns>
        public static string GetString(string dataName, string encryptKey = null)
        {
            if (!HasData(dataName))
            {
                // var ex = new Exception($"<{dataName}> does not exist\n" +
                //                        $"<{dataName}>不存在");
                // Debug.LogError($"[JSaver] 错误：{ex.Message}, {ex.Data["StackTrace"]}");
                return null;
            }

            if (String.IsNullOrEmpty(encryptKey))
            {
                encryptKey = DefaultEncryptKey;
            }

            if (encryptKey.Length != Length)
            {
                var ex = new Exception("encryptKey needs to be 16 characters!\n" +
                                       "秘钥长度不对");
                Debug.LogError($"[JSaver] 错误：{ex.Message}, {ex.Data["StackTrace"]}");
                return null;
            }

            var result = PlayerPrefs.GetString(dataName);
            try

            {
                result = CryptoMgr.DecryptStr(result, encryptKey);
                return result;
            }
            catch
                (Exception ex)
            {
                Debug.LogError(
                    $"can not decrypt <{dataName}>, error message: {ex.Message}, returns local data: {result}" +
                    $"无法解密<{dataName}>，错误：{ex.Message}，已返回本地数据：{result}");
                return result;
            }
        }

        /// <summary>
        /// Get int from local storage
        /// </summary>
        /// <param name="val"></param>
        /// <param name="encryptKey"></param>
        /// <returns>Saved value</returns>
        public static int GetInt(string dataName, int defaultValue = 0, string encryptKey = null)
        {
            if (int.TryParse(GetString(dataName, encryptKey), out var result))
            {
                return result;
            }

            return defaultValue;
        }

        /// <summary>
        /// Get short from local storage
        /// </summary>
        /// <param name="val"></param>
        /// <param name="encryptKey"></param>
        /// <returns>Saved value</returns>
        public static short GetShort(string dataName, short defaultValue = 0, string encryptKey = null)
        {
            if (short.TryParse(GetString(dataName, encryptKey), out var result))
            {
                return result;
            }

            return defaultValue;
        }

        /// <summary>
        /// Get long from local storage
        /// </summary>
        /// <param name="val"></param>
        /// <param name="encryptKey"></param>
        /// <returns>Saved value</returns>
        public static long GetLong(string dataName, long defaultValue = 0, string encryptKey = null)
        {
            if (long.TryParse(GetString(dataName, encryptKey), out var result))
            {
                return result;
            }

            return defaultValue;
        }

        /// <summary>
        /// Get decimal from local storage
        /// </summary>
        /// <param name="val"></param>
        /// <param name="encryptKey"></param>
        /// <returns>Saved value</returns>
        public static decimal GetDecimal(string dataName, decimal defaultValue = 0m, string encryptKey = null)
        {
            if (decimal.TryParse(GetString(dataName, encryptKey), out var result))
            {
                return result;
            }

            return defaultValue;
        }

        /// <summary>
        /// Get double from local storage
        /// </summary>
        /// <param name="val"></param>
        /// <param name="encryptKey"></param>
        /// <returns>Saved value</returns>
        public static double GetDouble(string dataName, double defaultValue = 0d, string encryptKey = null)
        {
            if (double.TryParse(GetString(dataName, encryptKey), out var result))
            {
                return result;
            }

            return defaultValue;
        }

        /// <summary>
        /// Get float from local storage
        /// </summary>
        /// <param name="val"></param>
        /// <param name="encryptKey"></param>
        /// <returns>Saved value</returns>
        public static float GetFloat(string dataName, float defaultValue = 0f, string encryptKey = null)
        {
            if (float.TryParse(GetString(dataName, encryptKey), out var result))
            {
                return result;
            }

            return defaultValue;
        }

        /// <summary>
        /// Get bool from local storage
        /// </summary>
        /// <param name="val"></param>
        /// <param name="encryptKey"></param>
        /// <returns>Saved value</returns>
        public static bool GetBool(string dataName, bool defaultValue = false, string encryptKey = null)
        {
            if (bool.TryParse(GetString(dataName, encryptKey), out var result))
            {
                return result;
            }

            return defaultValue;
        }

        /// <summary>
        /// Get object from local storage from JSON
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="val"></param>
        /// <param name="encryptKey"></param>
        /// <returns>Saved value</returns>
        public static T GetObjectFromJSON<T>(string dataName, string encryptKey = null) where T : class
        {
            if (!HasData(dataName))
            {
                // var ex = new Exception($"<{dataName}> does not exist\n" +
                //                        $"<{dataName}>不存在");
                // Debug.LogError($"[JSaver] 错误：{ex.Message}, {ex.Data["StackTrace"]}");
                return null;
            }

            if (String.IsNullOrEmpty(encryptKey))
            {
                encryptKey = DefaultEncryptKey;
            }

            if (encryptKey.Length != Length)
            {
                throw new Exception("encryptKey needs to be 16 characters!");
            }

            var result = PlayerPrefs.GetString(dataName);
            try
            {
                result = CryptoMgr.DecryptStr(result, encryptKey);
                return StringifyHelper.JsonDeSerialize<T>(result);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[JSaver] 错误：{ex.Message}, {ex.Data["StackTrace"]}");
                return default(T);
            }
        }

        /// <summary>
        /// Whether or not has specific data
        /// </summary>
        /// <param name="dataName"></param>
        /// <returns></returns>
        public static bool HasData(string dataName)
        {
            return PlayerPrefs.HasKey(dataName);
        }

        /// <summary>
        /// Delete specific data
        /// </summary>
        /// <param name="dataName"></param>
        public static void DeleteData(string dataName)
        {
            PlayerPrefs.DeleteKey(dataName);
        }

        /// <summary>
        /// Delete all data
        /// </summary>
        public static void DeleteAll()
        {
            var s = PlayerPrefs.GetString(JSaverKeys);
            if (!String.IsNullOrEmpty(s))
            {
                var keys = s.Split(',');
                foreach (var key in keys)
                {
                    PlayerPrefs.DeleteKey(key);
                }
            }
        }
    }
}
