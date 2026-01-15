using System.Collections.Generic;
using UnityEngine;

namespace Demos.示例_Toggle测试
{
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