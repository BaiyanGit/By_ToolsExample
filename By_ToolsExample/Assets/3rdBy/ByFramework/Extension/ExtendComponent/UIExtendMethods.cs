namespace _3rdBy.ByFramework.Extension.ExtendComponent
{
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using UnityEngine.Events;
    using UnityEngine.UI;

    /// <summary>
    /// UGUI组件扩展方法
    /// </summary>
    public static class UIExtendMethods
    {
        #region Toggle

        /// <summary>
        /// 复选框监听方法
        /// </summary>
        /// <param name="toggle"></param>
        /// <param name="selectEventHandler"></param>
        public static void OnValueChanged(this Toggle toggle, UnityAction<Toggle, bool> selectEventHandler)
        {
            toggle.onValueChanged.RemoveAllListeners();
            toggle.onValueChanged.AddListener((isOn) => { selectEventHandler(toggle, isOn); });
        }

        /// <summary>
        /// 强制设置Toggle的值，并触发事件
        /// </summary>
        /// <param name="toggle"></param>
        /// <param name="isOn"></param>
        public static void ForceSet(this Toggle toggle, bool isOn)
        {
            toggle.SetIsOnWithoutNotify(isOn);
            toggle.onValueChanged.Invoke(isOn);
        }

        /// <summary>
        /// 获取ToggleGroup下的所有toggle
        /// </summary>
        /// <param name="toggleGroup"></param>
        /// <returns></returns>
        public static List<Toggle> ChildToggles(this ToggleGroup toggleGroup)
        {
            return toggleGroup.GetComponentsInChildren<Toggle>().ToList();
        }

        /// <summary>
        /// 设置激活的Toggle
        /// </summary>
        /// <param name="toggleGroup">ToggleGroup</param>
        /// <param name="index">哪个Toggle</param>
        public static void SwitchToggle(this ToggleGroup toggleGroup, int index = 0)
        {
            var toggles = toggleGroup.ChildToggles();
            toggleGroup.SetAllTogglesOff();
            if (toggles.Count > 0 && index < toggles.Count)
            {
                toggles[index].isOn = true;
            }
            else
            {
                Debug.LogError($"ToggleGroup:{toggleGroup.name}的子Toggle数量为0或者 index:{index}超出索引");
            }
        }

        /// <summary>
        /// 获取ToggleGroup下toggle的点击事件
        /// </summary>
        /// <param name="toggleGroup"></param>
        /// <param name="selectEventHandler"></param>
        public static void OnValueChanged(this ToggleGroup toggleGroup,
            UnityAction<int, Toggle, bool> selectEventHandler)
        {
            var togglesList = toggleGroup.ChildToggles();
            for (int i = 0; i < togglesList.Count; i++)
            {
                int index = i;
                togglesList[i].OnValueChanged((toggle, isOn) => { selectEventHandler(index, toggle, isOn); });
            }
        }

        #endregion

        #region Dropdown

        /// <summary>
        /// 获取Dropdown下拉列表打开时的Content容器。如果列表未打开，返回null。
        /// WR: 注意如果用户自定义了Dropdown的结构，可能会导致该方法失效。
        /// </summary>
        public static Transform GetDropdownContent(this Dropdown dropdown)
        {
            if (dropdown == null) return null;

            // 下拉列表打开时，它通常被创建为名字是 "Dropdown List" 的根对象
            var listRoot = dropdown.transform.Find("Dropdown List/Viewport/Content");
            return listRoot;
        }

        /// <summary>
        /// 获取Dropdown下拉列表中的选项对象列表。如果列表未打开，返回空列表。
        /// </summary>
        /// <param name="dropdown"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static List<Toggle> GetDropdownOptions(this Dropdown dropdown)
        {
            if (dropdown == null) throw new System.Exception("Dropdown不能为空！");

            var content = dropdown.transform.Find("Dropdown List/Viewport/Content");
            if (content == null)
            {
                throw new System.Exception("检查是否自定义了Dropdown结构，这可能会导致该方法失效！！");
            }

            var options = content.GetComponentsInChildren<Toggle>().ToList();
            foreach (var option in options)
            {
                Debug.LogError(option.name);
            }

            return options;
        }

        /// <summary>
        /// Dropdown的下拉事件
        /// </summary>
        /// <param name="dropdown"></param>
        /// <param name="selectEventHandler"></param>
        public static void OnValueChanged(this Dropdown dropdown, UnityAction<Dropdown, int> selectEventHandler)
        {
            dropdown.onValueChanged.RemoveAllListeners();
            dropdown.onValueChanged.AddListener((optionIndex) => { selectEventHandler(dropdown, optionIndex); });
        }

        /// <summary>
        /// 当前对象下的所有Dropdown
        /// </summary>
        /// <param name="dropdownParent"></param>
        /// <returns></returns>
        public static List<Dropdown> ChildDropdowns(this Transform dropdownParent)
        {
            return dropdownParent.GetComponentsInChildren<Dropdown>().ToList();
        }

        /// <summary>
        /// 设置激活的Dropdown
        /// </summary>
        /// <param name="dropdownParent">Dropdown父物体</param>
        /// <param name="dIndex">哪个Dropdown</param>
        /// <param name="cIndex">Dropdown中的第几项</param>
        /// <param name="resetOther">重置其他Dropdown？</param>
        public static void SwitchDropdown(this Transform dropdownParent, int dIndex = 0, int cIndex = 0,
            bool resetOther = true)
        {
            var dropdowns = dropdownParent.ChildDropdowns();

            if (dropdowns.Count > 0 && dIndex < dropdowns.Count)
            {
                for (var i = 0; i < dropdowns.Count; i++)
                {
                    var dropdown = dropdowns[i];
                    if (i == dIndex)
                    {
                        dropdown.value = cIndex;
                    }
                    else
                    {
                        if (resetOther) dropdown.value = 0;
                    }
                }
            }
            else
            {
                Debug.LogError($"Dropdown父物体:{dropdownParent.name}的子Dropdown数量为0或者 dIndex:{dIndex}超出索引");
            }
        }

        /// <summary>
        /// 当前对象下的所有Dropdown点击事件
        /// </summary>
        /// <param name="dropdownParent"></param>
        /// <param name="selectEventHandler"></param>
        public static void OnValueChanged(this Transform dropdownParent,
            UnityAction<int, Dropdown, int> selectEventHandler)
        {
            var dropdownList = dropdownParent.ChildDropdowns();
            for (var i = 0; i < dropdownList.Count; i++)
            {
                var index = i;
                dropdownList[i].OnValueChanged((dropdown, optionIndex) => { selectEventHandler(index, dropdown, optionIndex); });
            }
        }

        #endregion

        #region Button

        /// <summary>
        /// 按钮始终只绑定一个事件
        /// </summary>
        /// <param name="button"></param>
        /// <param name="clickEventHandler"></param>
        public static void OnClick(this Button button, UnityAction clickEventHandler)
        {
            //每次绑定应该先清空，不然可能会造成listener多次添加
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(clickEventHandler);
        }

        /// <summary>
        /// 按钮点击一次的事件
        /// </summary>
        /// <param name="button"></param>
        /// <param name="clickEventHandler"></param>
        public static void OnClickOnce(this Button button, UnityAction clickEventHandler)
        {
            button.gameObject.SetActive(true);
            button.OnClick(() =>
            {
                button.gameObject.SetActive(false);
                clickEventHandler?.Invoke();
            });
        }

        /// <summary>
        /// 设置按钮交互与不可以交互时的颜色
        /// </summary>
        /// <param name="button"></param>
        /// <param name="value"></param>
        /// <param name="color"></param>
        public static void SetInteractable(this Button button, bool value, Color color)
        {
            button.interactable                         = value;
            button.GetComponentInChildren<Text>().color = color;
        }

        /// <summary>
        /// 设置按钮交互与不可以交互时的颜色
        /// </summary>
        /// <param name="button"></param>
        /// <param name="value"></param>
        /// <param name="hexColor"></param>
        public static void SetInteractable(this Button button, bool value, string hexColor)
        {
            button.SetInteractable(value, ExtensionMethods.HexToColor(hexColor));
        }

        #endregion

        #region Slider

        public static void OnValueChanged(this Slider slider, UnityAction<float, Slider> valueEventHandler)
        {
            slider.onValueChanged.RemoveAllListeners();
            slider.onValueChanged.AddListener((value) => { valueEventHandler(value, slider); });
        }

        #endregion
    }
}