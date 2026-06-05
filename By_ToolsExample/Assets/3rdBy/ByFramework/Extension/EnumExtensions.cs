namespace _3rdBy.ByFramework.Extension
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using UnityEngine;

    public static class EnumExtensions
    {
        /// <summary>
        /// 获取枚举值的 InspectorName 描述
        /// </summary>
        /// <param name="enumValue">枚举值</param>
        /// <returns>InspectorName描述，如果没有则返回枚举名</returns>
        public static string GetInspectorName(this Enum enumValue)
        {
            var    enumType = enumValue.GetType();
            string name     = Enum.GetName(enumType, enumValue);

            if (string.IsNullOrEmpty(name))
                return enumValue.ToString();

            var field             = enumType.GetField(name);
            var inspectorNameAttr = field.GetCustomAttribute<InspectorNameAttribute>(false);

            return inspectorNameAttr != null ? inspectorNameAttr.displayName : name;
        }

        /// <summary>
        /// 获取枚举值对应的 Tooltip
        /// </summary>
        public static string GetTooltip(this Enum enumValue)
        {
            var    enumType = enumValue.GetType();
            string name     = Enum.GetName(enumType, enumValue);

            var field       = enumType.GetField(name);
            var tooltipAttr = field.GetCustomAttribute<TooltipAttribute>(false);

            return tooltipAttr?.tooltip ?? string.Empty;
        }

        /// <summary>
        /// 获取枚举类型的所有值及其 InspectorName 描述
        /// </summary>
        public static Dictionary<T, string> GetInspectorNameDictionary<T>(this T enumType) where T : Enum
        {
            var dict = new Dictionary<T, string>();
            var type = typeof(T);

            foreach (T value in Enum.GetValues(type))
            {
                dict[value] = value.GetInspectorName();
            }

            return dict;
        }

        /// <summary>
        /// 获取枚举类型的所有 InspectorName 描述（字符串数组形式）
        /// 使用示例：
        /// 1）string[] names = EnumExtensions.GetInspectorNameArray(typeof(MyEnum));
        /// 2）string[] names = = default(MyEnum).GetInspectorNameArray();
        /// </summary>
        public static string[] GetInspectorNameArray<T>(this T enumType) where T : Enum
        {
            var type   = typeof(T);
            var values = Enum.GetValues(type);
            var names  = new List<string>();

            foreach (T value in values)
            {
                names.Add(value.GetInspectorName());
            }

            return names.ToArray();
        }

        /// <summary>
        /// 将枚举值转换为包含 InspectorName 的 GUIContent
        /// </summary>
        public static GUIContent ToGUIContent(this Enum enumValue)
        {
            string displayName = enumValue.GetInspectorName();
            string tooltip     = enumValue.GetTooltip();

            return new GUIContent(displayName, tooltip);
        }

        /// <summary>
        /// 通过 InspectorName 查找对应的枚举值
        /// </summary>
        public static T FromInspectorName<T>(this string inspectorName, T defaultValue = default) where T : Enum
        {
            var enumType = typeof(T);

            foreach (T value in Enum.GetValues(enumType))
            {
                if (value.GetInspectorName() == inspectorName)
                    return value;
            }

            return defaultValue;
        }
    }
}