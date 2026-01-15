namespace _3rdBy.ByFunc.ColorSelected.Scripts
{
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;

    public class ColorSelectorManager : MonoBehaviour
    {
        [Header("色环")] public ColorWheel colorWheel;
        [Header("色板")] public ColorSaturation colorSaturation;
        [Header("显示板")] public Graphic colorPlate;
        [Header("改变颜色的Image")] public List<Graphic> images;

        private  Color _defaultColor = Color.white;

        private void Start()
        {
            colorSaturation.colorEvent.AddListener(ChangeColor);
            colorWheel.SetSelectorWheel(_defaultColor);
        }

        /// <summary>
        /// 给所有图形赋值颜色
        /// </summary>
        /// <param name="color"></param>
        private void ChangeColor(Color color)
        {
            foreach (var img in images)
            {
                img.color = color;
            }

            //颜色样板
            if (colorPlate != null) colorPlate.color = color;
            _defaultColor = color;
        }
    }
}