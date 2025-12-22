namespace _3rdBy.UniTools.GuiEditor.Editor
{
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    /// <summary>
    /// OnGuiToggle互斥管理器
    /// </summary>
    public class ToggleManager
    {
        private readonly List<GUIToggle> _guiTogList = new();

        /// <summary>
        /// 是否保留一个Toggle的开启状态
        /// </summary>
        private readonly bool _allowSingleOn;

        public ToggleManager(bool allowSingleOn = true)
        {
            _allowSingleOn = allowSingleOn;
        }

        /// <summary>
        /// 获取Toggle
        /// </summary>
        /// <param name="togName"></param>
        /// <returns></returns>
        public GUIToggle Toggle(string togName)
        {
            return _guiTogList.Find(t => t.togName == togName);
        }

        public bool ToggleIsOn(string togName)
        {
            return Toggle(togName).isOn;
        }

        public GUIToggle AddToggle(string togName, float nameWidth, bool isOn = false)
        {
            var tog = new GUIToggle(togName, nameWidth, isOn, this);
            _guiTogList.Add(tog);
            return tog;
        }


        public GUIToggle AddToggle(string togName, bool isOn = false)
        {
            var tog = new GUIToggle(togName, 50, isOn, this);
            _guiTogList.Add(tog);
            return tog;
        }

        public void OnDrawAll()
        {
            GUILayout.BeginHorizontal();
            foreach (var t in _guiTogList)
            {
                t.DrawComponent();
            }

            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// Toggle状态切换
        /// </summary>
        /// <param name="toggle"></param>
        /// <param name="isOn"></param>
        public void SetToggleExclusive(GUIToggle toggle, bool isOn)
        {
            if (_allowSingleOn)
            {
                if (isOn)
                {
                    // 互斥，仅保留一个toggle打开
                    foreach (var tog in _guiTogList.Where(tog => tog != toggle))
                    {
                        tog.isOn = false;
                    }
                }
                else
                {
                    // 至少保留一个toggle为打开状态
                    var otherToggles = _guiTogList.Where(t => t != toggle).ToList();
                    if (otherToggles.All(t => !t.isOn))
                    {
                        toggle.isOn = true;
                    }
                }
            }
            else
            {
                if (!isOn) return;
                // 关闭其他所有toggle
                foreach (var tog in _guiTogList.Where(tog => tog != toggle))
                {
                    tog.isOn = false;
                }
            }
        }
    }
}