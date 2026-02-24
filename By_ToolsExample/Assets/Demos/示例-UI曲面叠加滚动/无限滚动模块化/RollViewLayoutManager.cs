namespace Demos.示例_UI曲面叠加滚动.无限滚动模块化
{
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// 布局管理器 - 处理滚动视图的布局和显示效果
    /// </summary>
    public class RollViewLayoutManager : MonoBehaviour
    {
        [Header("Item间距")] public float itemSpacing = 240f; // 每个滚动项之间的间距
        [Header("最小缩放")] public float minScale = 0.7f;      // 当项目远离中心时的最小缩放比例
        [Header("最大缩放")] public float maxScale = 1.25f;     // 中心项目时的最大缩放比例
        [Header("Y轴向上偏移量")] public float maxYOffset = 0.0f; // 中心项目时的最大Y轴偏移量
        [Header("最小透明度")] public float minAlpha = 0.4f;     // 当项目远离中心时的最小透明度

        private List<RectTransform> _items = new();
        private bool _infiniteLoop;

        public void Initialize(List<RectTransform> items, bool infiniteLoop)
        {
            _items        = items;
            _infiniteLoop = infiniteLoop;
        }

        /// <summary>
        /// 更新布局
        /// </summary>
        public void UpdateLayout(float scroll)
        {
            int count = _items.Count;

            // 图层排序列表
            List<(float t, RectTransform item)> sort = new();

            for (int i = 0; i < count; i++)
            {
                float offset = i - scroll;
                if (_infiniteLoop)
                    offset = Wrap(offset, count);

                float x = offset * itemSpacing;
                float t = Mathf.Clamp01(1f - Mathf.Abs(offset));

                float scale = Mathf.Lerp(minScale, maxScale, t);
                float y     = Mathf.Lerp(0, maxYOffset, t);
                float alpha = Mathf.Lerp(minAlpha, 1f, t);

                var item = _items[i];
                item.anchoredPosition                  = new Vector2(x, y);
                item.localScale                        = Vector3.one * scale;
                item.GetComponent<CanvasGroup>().alpha = alpha;

                sort.Add((t, item));
            }

            // 苹果级排序：远→近
            sort.Sort((a, b) => a.t.CompareTo(b.t));
            for (int i = 0; i < sort.Count; i++)
                sort[i].item.SetSiblingIndex(i);
        }

        /// <summary>
        /// 循环滚动
        /// </summary>
        private static float Wrap(float v, int length)
        {
            return Mathf.Repeat(v + length / 2f, length) - length / 2f;
        }

        /// <summary>
        /// 计算偏移量（考虑无限循环）
        /// </summary>
        public float CalculateOffset(int itemIndex, float scroll)
        {
            float offset = itemIndex - scroll;
            if (_infiniteLoop)
                offset = Wrap(offset, _items.Count);
            return offset;
        }
    }
}