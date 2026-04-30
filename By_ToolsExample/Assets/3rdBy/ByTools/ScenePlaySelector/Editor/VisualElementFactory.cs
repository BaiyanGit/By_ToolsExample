namespace _3rdBy.ByTools.ScenePlaySelector.Editor
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEditor;
    using UnityEditor.UIElements;
    using UnityEngine;
    using UnityEngine.UIElements;

    /// <summary>
    /// Toolbar UIElement 创建工具。
    /// 
    /// 当前风格：
    /// 1. Source / Scene 默认透明背景，和 Unity Toolbar 融为一体。
    /// 2. 鼠标悬停时显示 Unity toolbarButton.hover 背景色。
    /// 3. 鼠标按下时显示 Unity toolbarButton.active 背景色。
    /// 4. 文字颜色跟随 Unity toolbarButton。
    /// </summary>
    public static class VisualElementFactory
    {
        /// <summary>
        /// 原生 Toolbar 下拉菜单封装。
        /// 用 ToolbarMenu 代替 PopupField，避免 PopupField 在 Toolbar 中出现白底或文字不可见的问题。
        /// </summary>
        public sealed class NativeToolbarDropdown : ToolbarMenu
        {
            [Header("下拉菜单中的所有选项")]
            private readonly List<string> _choices = new();

            [Header("显示在当前选项前面的前缀，例如 Source 或 Scene")]
            private readonly string _prefix;

            [Header("鼠标是否正在悬停在当前控件上")]
            private bool _isHover;

            [Header("鼠标是否正在按下当前控件")]
            private bool _isPressed;

            [Header("当前选中的选项索引")]
            private int _index = -1;

            /// <summary>
            /// 当前选择项变化时触发。
            /// 参数为新的选项索引。
            /// </summary>
            public event Action<int> IndexChanged;

            /// <summary>
            /// 当前选中的选项索引。
            /// </summary>
            public int Index => _index;

            /// <summary>
            /// 当前下拉菜单的所有选项。
            /// </summary>
            public IReadOnlyList<string> Choices => _choices;

            /// <summary>
            /// 创建一个 Toolbar 下拉菜单。
            /// </summary>
            public NativeToolbarDropdown(string prefix, IList<string> options, int index, float minWidth)
            {
                _prefix = string.IsNullOrWhiteSpace(prefix) ? string.Empty : prefix.Trim();

                tooltip = _prefix;

                style.minWidth = minWidth;
                style.height = 22;
                style.marginLeft = 1;
                style.marginRight = 2;
                style.paddingLeft = 6;
                style.paddingRight = 6;
                style.unityTextAlign = TextAnchor.MiddleLeft;
                style.flexShrink = 0;

                SetChoices(options, index, false);

                RegisterToolbarMenuVisualEvents();
                ApplyDropdownVisualState();

                // Unity 有些版本会在控件挂到 Toolbar 后再次套默认样式，所以延迟再同步两次。
                schedule.Execute(ApplyDropdownVisualState).ExecuteLater(50);
                schedule.Execute(ApplyDropdownVisualState).ExecuteLater(200);
            }

            /// <summary>
            /// 设置下拉菜单选项。
            /// </summary>
            /// <param name="options">新的选项列表。</param>
            /// <param name="index">选中索引。</param>
            /// <param name="notify">是否触发 IndexChanged 回调。</param>
            public void SetChoices(IList<string> options, int index, bool notify = false)
            {
                _choices.Clear();

                if (options != null)
                {
                    foreach (string option in options.Where(option => !string.IsNullOrEmpty(option)))
                    {
                        _choices.Add(option);
                    }
                }

                if (_choices.Count == 0)
                {
                    _choices.Add("<无可用项>");
                }

                RebuildMenu();
                SetIndex(index, notify);
            }

            /// <summary>
            /// 设置当前选中的索引。
            /// </summary>
            public void SetIndex(int index, bool notify = true)
            {
                index = Mathf.Clamp(index, 0, _choices.Count - 1);

                bool changed = _index != index;
                _index = index;

                string selectedText = _choices[_index];
                text = string.IsNullOrEmpty(_prefix) ? selectedText : $"{_prefix}: {selectedText}";
                tooltip = text;

                RebuildMenu();
                ApplyDropdownVisualState();

                if (notify && changed)
                {
                    IndexChanged?.Invoke(_index);
                }
            }

            /// <summary>
            /// 重新构建 ToolbarMenu 的下拉菜单项。
            /// </summary>
            private void RebuildMenu()
            {
                menu.ClearItems();

                for (int i = 0; i < _choices.Count; i++)
                {
                    int capturedIndex = i;
                    string choice = _choices[i];

                    menu.AppendAction(
                        choice,
                        _ =>
                        {
                            SetIndex(capturedIndex, true);
                            _isPressed = false;
                            ApplyDropdownVisualState();
                        },
                        _ => capturedIndex == _index
                            ? DropdownMenuAction.Status.Checked
                            : DropdownMenuAction.Status.Normal);
                }
            }

            /// <summary>
            /// 注册鼠标事件，用于实现默认透明、悬停/点击显示背景的效果。
            /// </summary>
            private void RegisterToolbarMenuVisualEvents()
            {
                RegisterCallback<MouseEnterEvent>(_ =>
                {
                    _isHover = true;
                    ApplyDropdownVisualState();
                });

                RegisterCallback<MouseLeaveEvent>(_ =>
                {
                    _isHover = false;
                    _isPressed = false;
                    ApplyDropdownVisualState();
                });

                RegisterCallback<MouseDownEvent>(_ =>
                {
                    _isPressed = true;
                    ApplyDropdownVisualState();
                });

                RegisterCallback<MouseUpEvent>(_ =>
                {
                    _isPressed = false;
                    ApplyDropdownVisualState();
                });

                RegisterCallback<BlurEvent>(_ =>
                {
                    _isPressed = false;
                    ApplyDropdownVisualState();
                });
            }

            /// <summary>
            /// 根据当前鼠标状态应用视觉样式。
            /// 默认透明，悬停显示 hover 色，按下显示 active 色。
            /// </summary>
            private void ApplyDropdownVisualState()
            {
                Color textColor = GetUnityToolbarTextColor();
                Color backgroundColor = Color.clear;

                if (_isPressed)
                {
                    backgroundColor = GetToolbarButtonActiveColor();
                }
                else if (_isHover)
                {
                    backgroundColor = GetToolbarButtonHoverColor();
                }

                ApplyBackgroundRecursive(this, backgroundColor, textColor);
                MarkDirtyRepaint();
            }
        }

        /// <summary>
        /// 创建一个原生 Toolbar 风格的下拉菜单。
        /// </summary>
        public static NativeToolbarDropdown CreateToolbarDropdown(string prefix, IList<string> options, int index, float minWidth = 140f)
        {
            return new NativeToolbarDropdown(prefix, options, index, minWidth);
        }

        /// <summary>
        /// 创建 Toolbar Label。
        /// 当前主工具栏不再使用独立 Label，但保留这个方法便于扩展。
        /// </summary>
        public static Label CreateToolbarLabel(string text, FontStyle fontStyle = FontStyle.Normal)
        {
            var label = new Label(text)
            {
                style =
                {
                    unityFontStyleAndWeight = fontStyle,
                    unityTextAlign = TextAnchor.MiddleRight,
                    marginLeft = 2,
                    marginRight = 3,
                    fontSize = 11,
                }
            };

            return label;
        }

        /// <summary>
        /// 创建 Unity 原生 ToolbarButton。
        /// </summary>
        public static Button CreateToolbarButton(string text, Action onClick, string tooltip = null)
        {
            var button = new ToolbarButton(onClick)
            {
                text = text,
                tooltip = tooltip ?? string.Empty,
                style =
                {
                    minWidth = 24,
                    height = 22,
                    marginLeft = 1,
                    marginRight = 1,
                    paddingLeft = 4,
                    paddingRight = 4,
                    unityTextAlign = TextAnchor.MiddleCenter,
                    flexShrink = 0,
                }
            };

            return button;
        }

        /// <summary>
        /// 递归设置控件和所有子节点的背景色与文字色。
        /// 用于处理不同 Unity 版本下 ToolbarMenu 内部层级差异。
        /// </summary>
        private static void ApplyBackgroundRecursive(VisualElement root, Color backgroundColor, Color textColor)
        {
            if (root == null)
            {
                return;
            }

            root.style.backgroundColor = backgroundColor;
            root.style.color = textColor;

            var children = root.Query<VisualElement>().Build().ToList();
            foreach (var child in children)
            {
                child.style.backgroundColor = backgroundColor;
                child.style.color = textColor;
            }
        }

        /// <summary>
        /// 获取 Unity 当前 Toolbar 按钮文字颜色。
        /// </summary>
        private static Color GetUnityToolbarTextColor()
        {
            Color color = EditorStyles.toolbarButton.normal.textColor;
            if (color.a > 0.01f)
            {
                color.a = 1f;
                return color;
            }

            return EditorGUIUtility.isProSkin ? Color.white : Color.black;
        }

        /// <summary>
        /// 获取 Unity Toolbar 按钮悬停背景色。
        /// 如果 Unity 内置贴图不可读，则返回接近 Unity 主题的兜底色。
        /// </summary>
        private static Color GetToolbarButtonHoverColor()
        {
            Color sampled = TrySampleStyleBackground(EditorStyles.toolbarButton.hover.background);
            if (sampled.a > 0.01f)
            {
                sampled.a = 1f;
                return sampled;
            }

            return EditorGUIUtility.isProSkin
                ? new Color(0.30f, 0.30f, 0.30f, 1f)
                : new Color(0.68f, 0.68f, 0.68f, 1f);
        }

        /// <summary>
        /// 获取 Unity Toolbar 按钮按下背景色。
        /// 如果 Unity 内置贴图不可读，则返回接近 Unity 主题的兜底色。
        /// </summary>
        private static Color GetToolbarButtonActiveColor()
        {
            Color sampled = TrySampleStyleBackground(EditorStyles.toolbarButton.active.background);
            if (sampled.a > 0.01f)
            {
                sampled.a = 1f;
                return sampled;
            }

            return EditorGUIUtility.isProSkin
                ? new Color(0.17f, 0.17f, 0.17f, 1f)
                : new Color(0.58f, 0.58f, 0.58f, 1f);
        }

        /// <summary>
        /// 尝试从 GUIStyle 背景贴图中采样颜色。
        /// </summary>
        private static Color TrySampleStyleBackground(Texture2D texture)
        {
            try
            {
                if (texture == null)
                {
                    return Color.clear;
                }

                return texture.GetPixel(
                    Mathf.Clamp(texture.width / 2, 0, texture.width - 1),
                    Mathf.Clamp(texture.height / 2, 0, texture.height - 1));
            }
            catch
            {
                // Unity 内置贴图在部分版本可能不可读。
                return Color.clear;
            }
        }
    }
}
