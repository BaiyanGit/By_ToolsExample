namespace MacroDefineBuildToolEditor
{
    using UnityEditor;
    using UnityEngine;

    public class EditorStylesEx
    {
        // 全局只创建 1 次，永久复用
        public static readonly GUIStyle FoldoutBold;
        public static readonly GUIStyle DefineItemStyle;
        public static readonly GUIStyle DefineItemStyleBold;

        // 静态构造函数 → 程序运行只执行一次
        static EditorStylesEx()
        {
            FoldoutBold = new GUIStyle(EditorStyles.foldout)
            {
                fontStyle = FontStyle.Bold,
                richText  = true
            };

            DefineItemStyle = new GUIStyle(EditorStyles.label)
            {
                normal =
                {
                    textColor = Color.gray
                },
                richText = true
            };

            DefineItemStyleBold = new GUIStyle(EditorStyles.boldLabel)
            {
                richText = true
            };
        }
    }
}