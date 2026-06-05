namespace Demos.示例_UIToggle测试
{
    using System.Collections.Generic;
    using UnityEngine;

    public class ToggleTest : MonoBehaviour
    {
        [SerializeField] private List<ToggleControlAttribute> togglePanelMap;

        private void Start()
        {
            var uiPanelToggleController = new UIPanelToggleController();
            uiPanelToggleController.Initialize(togglePanelMap);
        }
    }
}