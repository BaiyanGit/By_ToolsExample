namespace Demos.示例_UI曲面叠加滚动.Scripts
{
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// 随机显示一张图片
    /// </summary>
    public class RollViewItemRangeSprite : MonoBehaviour
    {
        public Image imgIcon;
        public Image imgBg;
        public List<Sprite> sprites = new();

        [ContextMenu("随机显示")]
        public void OnEnable()
        {
            SetRandomSprite();
            SetRandomColor();
        }

        [ContextMenu("随机图片")]
        public void SetRandomSprite()
        {
            var sprite = sprites[Random.Range(0, sprites.Count)];
            imgIcon.sprite = sprite;
        }

        [ContextMenu("随机颜色")]
        public void SetRandomColor()
        {
            imgBg.color = Random.ColorHSV();
        }
    }

    [CustomEditor(typeof(RollViewItemRangeSprite))]
    public class EditorRollViewItemRangeSprite : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (GUILayout.Button("随机图片"))
            {
                (target as RollViewItemRangeSprite)?.SetRandomSprite();
                EditorUtility.SetDirty(target);
            }

            if (GUILayout.Button("随机颜色"))
            {
                (target as RollViewItemRangeSprite)?.SetRandomColor();
                EditorUtility.SetDirty(target);
            }

            if (GUILayout.Button("全部随机"))
            {
                (target as RollViewItemRangeSprite)?.OnEnable();
                EditorUtility.SetDirty(target);
            }
        }
    }
}