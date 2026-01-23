namespace _3rdBy.ByTools.ScenePlaySelector.Editor
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.UIElements;

    /// <summary>
    /// 视觉元素组件创建类
    /// </summary>
    public static class VisualElementFactory
    {
        /// <summary>
        /// 创建符合 Toolbar 风格的 PopupField（无白框）
        /// </summary>
        public static PopupField<string> CreateToolbarPopup(IList<string> options, int index)
        {
            var popup = new PopupField<string>(options.ToList(), index);

            // 1. 移除所有可能干扰的 Unity 类 (通过Ul Toolkit Debugger工具可以看到)
            popup.RemoveFromClassList("unity-popup-field");
            popup.RemoveFromClassList("unity-base-popup-field");
            popup.RemoveFromClassList("unity-base-field");

            // 2. 只添加自定义类
            popup.AddToClassList("custom-popup");

            // 3. 立即设置强制样式
            popup.style.backgroundColor = Color.gray;

            // 4. 使用强制方式设置优先级
            popup.style.backgroundColor = Color.gray;

            // 5. 延迟执行详细设置
            popup.schedule.Execute(() =>
            {
                // 强制清除所有子元素的背景色
                var children = popup.Query<VisualElement>().Build().ToList();
                foreach (var child in children)
                {
                    child.style.backgroundColor = Color.clear;
                }

                // 找到输入容器并设置
                var inputContainer = popup.Q(className: "unity-base-popup-field__input");
                if (inputContainer != null)
                {
                    inputContainer.ClearClassList();                                                            // 清除所有现有样式类
                    inputContainer.style.backgroundColor = new Color(0.07843138f, 0.07843138f, 0.07843138f, 1); // 背景色
                    inputContainer.style.opacity         = 1f;                                                  // 透明度

                    // 设置布局
                    inputContainer.style.flexDirection   = FlexDirection.Row;                           // 横向排列
                    inputContainer.style.justifyContent  = Justify.SpaceBetween;                        // 主轴居中对齐
                    inputContainer.style.alignItems      = Align.Center;                                // 将子对象与此容器的横轴中间对齐。
                    inputContainer.style.height          = Length.Percent(100);                         // 布局元素的固定高度。
                    inputContainer.style.paddingLeft     = 6;                                           // 左侧内边距
                    inputContainer.style.paddingRight    = 6;                                           // 右侧内边距
                    inputContainer.style.backgroundImage = new StyleBackground(Texture2D.blackTexture); // 背景图片

                    // 强制重绘
                    inputContainer.MarkDirtyRepaint();
                }

                // 找到文本元素
                var textElement = popup.Q<VisualElement>(className: "unity-base-popup-field__text");
                if (textElement != null)
                {
                    textElement.style.color                   = Color.green;    // 文字颜色
                    textElement.style.backgroundColor         = Color.clear;    // 文字背景色
                    textElement.style.unityFontStyleAndWeight = FontStyle.Bold; // 文字样式
                }

                // 强制整个元素重绘
                popup.MarkDirtyRepaint();
            }).ExecuteLater(200); // 更长的延迟

            return popup;
        }

        /// <summary>
        /// 创建符合 Toolbar 风格的 Label
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static Label CreateToolbarLabel(string text, FontStyle fontStyle = FontStyle.Normal)
        {
            var label = new Label(text)
            {
                style =
                {
                    unityFontStyleAndWeight = fontStyle,
                    unityTextAlign          = TextAnchor.MiddleRight,
                    marginRight             = 4,
                    color                   = EditorGUIUtility.isProSkin ? Color.white : Color.black,
                }
            };
            return label;
        }

        /// <summary>
        /// 创建符合 Toolbar 风格的 Button
        /// </summary>
        public static Button CreateToolbarButton(string text, Action onClick)
        {
            var button = new Button(onClick)
            {
                text = text,

                style =
                {
                    marginLeft  = 4,
                    marginRight = 2,
                    // backgroundColor = Color.blue
                }
            };
            button.AddToClassList("unity-toolbar-button");
            return button;
        }
    }
}