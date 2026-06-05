namespace Demos.示例_进程任务管理器.Scripts
{
    using System.Collections.Generic;
    using TMPro;
    using UnityEngine;

    /// <summary>
    /// 兼容旧 TMP_Dropdown UI 的快捷键设置组件。
    /// 新版主界面已由 WindowController.OnGUI 绘制，可不再使用本组件。
    /// </summary>
    public class KeySettings : MonoBehaviour
    {
        [Header("快捷键第一个按键下拉框")]
        [SerializeField] private TMP_Dropdown sDropdown1;

        [Header("快捷键第二个按键下拉框，可选")]
        [SerializeField] private TMP_Dropdown sDropdown2;

        [Header("第一个按键的 Windows 虚拟键码")]
        public int key1;

        [Header("第二个按键的 Windows 虚拟键码，0 表示不设置")]
        public int key2;

        private void Awake()
        {
            InitializeDropdowns();
        }

        private void OnDestroy()
        {
            if (sDropdown1 != null)
            {
                sDropdown1.onValueChanged.RemoveListener(ShortcutKey1);
            }

            if (sDropdown2 != null)
            {
                sDropdown2.onValueChanged.RemoveListener(ShortcutKey2);
            }
        }

        public List<int> GetRequiredKeys()
        {
            var keys = new List<int>();

            if (key1 > 0)
            {
                keys.Add(key1);
            }

            if (key2 > 0 && key2 != key1)
            {
                keys.Add(key2);
            }

            return keys;
        }

        private void InitializeDropdowns()
        {
            SetupDropdown(sDropdown1, ShortcutKey1);
            SetupDropdown(sDropdown2, ShortcutKey2);

            ShortcutKey1(sDropdown1 != null ? sDropdown1.value : 0);
            ShortcutKey2(sDropdown2 != null ? sDropdown2.value : 0);
        }

        private static void SetupDropdown(TMP_Dropdown dropdown, UnityEngine.Events.UnityAction<int> callback)
        {
            if (dropdown == null)
            {
                return;
            }

            dropdown.options.Clear();
            for (int i = 0; i < KeyCodeName.KeyOptions.Count; i++)
            {
                dropdown.options.Add(new TMP_Dropdown.OptionData(KeyCodeName.KeyOptions[i].Key));
            }

            dropdown.onValueChanged.RemoveListener(callback);
            dropdown.onValueChanged.AddListener(callback);
            dropdown.RefreshShownValue();
        }

        private void ShortcutKey1(int index)
        {
            key1 = KeyCodeName.GetKeyValueByIndex(index);
        }

        private void ShortcutKey2(int index)
        {
            key2 = KeyCodeName.GetKeyValueByIndex(index);
        }
    }
}
