namespace _3rdBy.UniTools.GuiEditor.Editor
{
    using UnityEngine;

    /// <summary>
    /// GUI布局样式
    /// </summary>
    public static class GUILayoutStyle
    {
        /// <summary>
        /// 标题文字样式
        /// </summary>
        /// <returns></returns>
        public static GUIStyle TitleStyle()
        {
            return LabelStyle(20, FontStyle.Bold, TextAnchor.MiddleCenter, Color.white);
        }


        /// <summary>
        /// 提示文本的样式
        /// </summary>
        /// <returns></returns>
        public static GUIStyle TipStyle()
        {
            return LabelStyle(12, FontStyle.Normal, TextAnchor.MiddleLeft, Color.red);
        }

        /// <summary>
        /// 内容文本样式
        /// </summary>
        /// <returns></returns>
        public static GUIStyle ContentStyle()
        {
            return LabelStyle(12, FontStyle.Normal, TextAnchor.MiddleLeft, Color.white);
        }

        /// <summary>
        /// 说明文字标题样式
        /// </summary>
        /// <returns></returns>
        public static GUIStyle DescTitleStyle()
        {
            return LabelStyle(12, FontStyle.Bold, TextAnchor.MiddleLeft, Color.white);
        }

        /// <summary>
        /// 设置、字体大小（样式默认、居左、白色）
        /// </summary>
        /// <param name="fontSize"></param>
        /// <returns></returns>
        public static GUIStyle LabelStyle(int fontSize)
        {
            return LabelStyle(fontSize, FontStyle.Normal, TextAnchor.MiddleLeft, Color.white);
        }

        /// <summary>
        /// 设置字体大小、样式（居左、白色）
        /// </summary>
        /// <param name="fontSize"></param>
        /// <param name="fontStyle"></param>
        /// <returns></returns>
        public static GUIStyle LabelStyle(int fontSize, FontStyle fontStyle)
        {
            return LabelStyle(fontSize, fontStyle, TextAnchor.MiddleLeft, Color.white);
        }

        /// <summary>
        /// 设置字体大小、样式、锚点 （白色）
        /// </summary>
        /// <param name="fontSize"></param>
        /// <param name="fontStyle"></param>
        /// <param name="anchor"></param>
        /// <returns></returns>
        public static GUIStyle LabelStyle(int fontSize, FontStyle fontStyle, TextAnchor anchor)
        {
            return LabelStyle(fontSize, fontStyle, anchor, Color.white);
        }

        /// <summary>
        /// 设置字体大小、样式、颜色（默认居左）
        /// </summary>
        /// <param name="fontSize"></param>
        /// <param name="fontStyle"></param>
        /// <param name="color"></param>
        /// <returns></returns>
        public static GUIStyle LabelStyle(int fontSize, FontStyle fontStyle, Color color)
        {
            return LabelStyle(fontSize, fontStyle, TextAnchor.MiddleLeft, color);
        }

        /// <summary>
        /// 设置字体大小、颜色（字体样式默认、默认居左）
        /// </summary>
        /// <param name="fontSize"></param>
        /// <param name="color"></param>
        /// <returns></returns>
        public static GUIStyle LabelStyle(int fontSize, Color color)
        {
            return LabelStyle(fontSize, FontStyle.Normal, TextAnchor.MiddleLeft, color);
        }

        /// <summary>
        /// 设置字体颜色
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        public static GUIStyle LabelStyle(Color color)
        {
            return LabelStyle(12, FontStyle.Normal, TextAnchor.MiddleLeft, color);
        }

        /// <summary>
        /// 设置字体大小、样式、颜色
        /// </summary>
        /// <param name="fontSize"></param>
        /// <param name="fontStyle"></param>
        /// <param name="anchor"></param>
        /// <returns></returns>
        public static GUIStyle LabelStyle(int fontSize, TextAnchor anchor, Color color)
        {
            return LabelStyle(fontSize, FontStyle.Normal, anchor, color);
        }

        /// <summary>
        /// 设置字体大小、样式、颜色
        /// </summary>
        /// <param name="fontSize"></param>
        /// <param name="fontStyle"></param>
        /// <param name="anchor"></param>
        /// <returns></returns>
        public static GUIStyle LabelStyle(TextAnchor anchor, Color color)
        {
            return LabelStyle(12, FontStyle.Normal, anchor, color);
        }

        public static GUIStyle LabelStyle(int fontSize, FontStyle fontStyle, TextAnchor anchor, Color color)
        {
            var labelStyle = new GUIStyle
            {
                fontSize = fontSize,
                fontStyle = fontStyle,
                alignment = anchor,
                normal = { textColor = color }
            };
            return labelStyle;
        }
    }
}