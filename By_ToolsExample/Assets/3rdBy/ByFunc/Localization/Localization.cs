namespace _3rdBy.ByFunc.Localization
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using UnityEngine;

    public class Localization
    {
        private const string Path = "Localization";

        private const string SaveKey = "AppLanguage";

        private static Dictionary<string, Dictionary<string, string>> _phrases; //language,key,value

        private static List<LocalizedText> _texts;

        public static string CurrentLanguage { get; private set; }

        private static void Init()
        {
            _phrases = new Dictionary<string, Dictionary<string, string>>();
            var textAsset = Resources.Load<TextAsset>(Path);
            if (textAsset != null)
            {
                var data = textAsset.text;
                //获取全部行
                var allRows = new List<string>(0);
                var array = Encoding.UTF8.GetBytes(data);
                var stream = new MemoryStream(array);
                var sr = new StreamReader(stream, Encoding.Default);
                while (sr.ReadLine() is { } line)
                {
                    allRows.Add(line);
                }

                sr.Close();

                var pattern = ",(?=(?:[^\\" + '"' + "]*\\" + '"' + "[^\\" + '"' + "]*\\" + '"' + ")*[^\\" + '"' +
                              "]*$)";

                //获取语言
                var header = Regex.Split(allRows[0], pattern);

                for (var i = 1; i < header.Length; i++)
                {
                    //某语言
                    var lang = GetExactValue(header[i]).ToLower();
                    //获取key和value
                    var p = new Dictionary<string, string>();
                    for (var j = 1; j < allRows.Count; j++)
                    {
                        //某一行
                        string row = allRows[j];
                        //切割
                        var cells = Regex.Split(row, pattern);
                        //添加key-value
                        p.Add(GetExactValue(cells[0]), GetExactValue(cells[i]));
                    }

                    //添加lang
                    _phrases.Add(lang, p);
                }
            }
            else
            {
                Debug.LogError("多语言配置文件错误或者丢失！！！");
                return;
            }

            ChangeLanguage(PlayerPrefs.GetString(SaveKey, CultureInfo.InstalledUICulture.Name));
        }

        private static string GetExactValue(string val)
        {
            if (val[0] == '"' && val[^1] == '"')
            {
                val = val.Substring(1, val.Length - 2);
            }

            char p = '"';
            string pattern = p + p.ToString();
            string p2 = p + "";
            val = val.Replace(pattern, p2);
            return val;
        }

        /// <summary>
        /// 将组件加入列表
        /// </summary>
        /// <param name="lt"></param>
        public static void AddText(LocalizedText lt)
        {
            _texts ??= new List<LocalizedText>();

            _texts.Add(lt);
        }

        /// <summary>
        /// 更改语言
        /// Change language
        /// </summary>
        /// <param name="lang"></param>
        public static void ChangeLanguage(string lang)
        {
            CurrentLanguage = lang.ToLower();
            PlayerPrefs.SetString(SaveKey, CurrentLanguage);

            if (_phrases != null && _texts is { Count: > 0 })
            {
                foreach (var langText in _texts)
                {
                    langText.SetText();
                }
            }
        }

        /// <summary>
        /// 获取key的文字
        /// Find string of key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string GetString(string key)
        {
            if (_phrases == null)
            {
                Init();
            }

            if (_phrases != null && !_phrases.ContainsKey(CurrentLanguage))
            {
                var newLang = _phrases.Keys.ToList().Find(k => k.StartsWith($"{CurrentLanguage.Split('-')[0]}-"));
                if (CurrentLanguage != "zh-cn" && newLang == null)
                {
                    newLang = "zh-cn";
                }
                else if (newLang == null)
                {
                    return $"[invalid language: {CurrentLanguage}]";
                }

                Debug.LogError($"不存在语言{CurrentLanguage}，自动替换为{newLang}");
                ChangeLanguage(newLang);
            }

            var value = $"[invalid key: {key}]";
            _phrases?[CurrentLanguage].TryGetValue(key, out value);
            return value;
        }
    }
}