namespace _3rdBy.MetaFramework.Extension
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Globalization;
    using System.IO;
    using UnityEngine;

    /// <summary>
    /// 扩展方法
    /// </summary>
    public static class ExtensionMethods
    {
        public static string ColorToHex(Color32 color)
        {
            return color.r.ToString("X2") + color.g.ToString("X2") + color.b.ToString("X2") + color.a.ToString("X2");
        }

        public static Color HexToColor(string hex)
        {
            hex = hex.Replace("0x", string.Empty);
            hex = hex.Replace("#", string.Empty);
            var a = byte.MaxValue;
            var r = byte.Parse(hex.Substring(0, 2), NumberStyles.HexNumber);
            var g = byte.Parse(hex.Substring(2, 2), NumberStyles.HexNumber);
            var b = byte.Parse(hex.Substring(4, 2), NumberStyles.HexNumber);
            if (hex.Length == 8)
            {
                a = byte.Parse(hex.Substring(6, 2), NumberStyles.HexNumber);
            }

            return new Color32(r, g, b, a);
        }

        /// <summary>
        /// 获取枚举的描述信息
        /// </summary>
        public static string GetDescription(this Enum em)
        {
            var type = em.GetType();
            var fd = type.GetField(em.ToString());
            if (fd == null)
                return string.Empty;
            var attrs = fd.GetCustomAttributes(typeof(DescriptionAttribute), false);
            var name = string.Empty;
            foreach (DescriptionAttribute attr in attrs)
            {
                name = attr.Description;
            }

            return name;
        }

        /// <summary>
        /// 拆分list为两个list
        /// </summary>
        /// <param name="predicate">条件</param>
        /// <param name="trueList">true列表</param>
        /// <param name="falseList">false列表</param>
        public static void Partition<T>(this List<T> list, Predicate<T> predicate, List<T> trueList, List<T> falseList)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (predicate(list[i]))
                    trueList.Add(list[i]);
                else
                    falseList.Add(list[i]);
            }
        }

        /// <summary>
        /// 从列表随机一个T
        /// </summary>
        /// <param name="list"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T RandomList<T>(this List<T> list)
        {
            return list[UnityEngine.Random.Range(0, list.Count)];
        }

        /// <summary>
        /// 根据给定的概率返回该元素或default(T)
        /// </summary>
        /// <param name="list"></param>
        /// <param name="chance"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T RandomRandomList<T>(this List<T> list, float chance)
        {
            if (chance <= 0)
            {
                return default;
            }

            return UnityEngine.Random.Range(0.0f, 1.0f) <= chance
                ? list[UnityEngine.Random.Range(0, list.Count)]
                : default;
        }

        /// <summary>
        /// 字符转小写
        /// </summary>
        /// <param name="strList"></param>
        /// <returns></returns>
        public static List<string> ConvertToLower(this List<string> strList)
        {
            var rtn = new List<string>();
            strList.ForEach(c => rtn.Add(c.ToLower()));
            return rtn;
        }

        /// <summary>
        /// 字符转大写
        /// </summary>
        /// <param name="strList"></param>
        /// <returns></returns>
        public static List<string> ConvertToUpper(this List<string> strList)
        {
            var rtn = new List<string>();
            strList.ForEach(c => rtn.Add(c.ToUpper()));
            return rtn;
        }
        
    }
}