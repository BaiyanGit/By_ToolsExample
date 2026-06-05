namespace Demos.示例_UIToggle测试
{
    using _3rdBy.ByFramework.Extension.ExtendComponent;
    using UnityEngine;
    using UnityEngine.UI;

    //=====================================================
    // 文件名称: DropdownTest
    // 创 建 者: 
    // 创建日期: 
    // 描    述: 
    //=====================================================


    public class DropdownTest : MonoBehaviour
    {
        public Dropdown dropdown;

        // 在MonoBehaviour创建后，在第一次执行Update之前调用一次Start
        private void Start()
        {
            dropdown.onValueChanged.AddListener(v => Debug.Log($"下拉值更改为: {v}"));
        }

        public void DropdownShow()
        {
            Debug.Log($"[下拉菜单] 显示");
            dropdown.Show();
            var options = dropdown.GetDropdownOptions();
            if (options is { Count: > 0 })
            {
                options[1].GetComponent<Toggle>().isOn = true;
            }
        }

        public void DropdownHide()
        {
            Debug.Log($"[下拉菜单] 隐藏");
            dropdown.Hide();
        }

        public void DropdownSelected()
        {
            Debug.Log($"[下拉菜单] 选中");
            dropdown.Select();
        }

        public void DropdownDeselected()
        {
            Debug.Log($"[下拉菜单] 取消选中");
            // BaseEventData data = new BaseEventData(EventSystem.current);
            dropdown.OnDeselect(null);
        }
    }

#if UNITY_EDITOR

    [UnityEditor.CustomEditor(typeof(DropdownTest))]
    public class DropdownTestEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var navDropdown = (DropdownTest)target;

            if (GUILayout.Button("显示下拉菜单"))
            {
                navDropdown.DropdownShow();
            }

            if (GUILayout.Button("隐藏下拉菜单"))
            {
                navDropdown.DropdownHide();
            }

            if (GUILayout.Button("选中下拉菜单"))
            {
                navDropdown.DropdownSelected();
            }

            if (GUILayout.Button("取消选中下拉菜单"))
            {
                navDropdown.DropdownDeselected();
            }
        }
    }
#endif
}