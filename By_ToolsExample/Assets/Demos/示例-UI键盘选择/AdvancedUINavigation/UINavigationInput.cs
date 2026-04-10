using UnityEngine;

namespace AdvancedUINavigation
{
    using UnityEngine.InputSystem;

    public class UINavigationInput : MonoBehaviour
    {
        [SerializeField] [Header("导航项收集管理")] private UINavigationPanel _uiNavigationPanel;
        [Header("新输入系统")] public bool newInputSystem;

        private void Awake()
        {
            newInputSystem = typeof(InputSystem).Assembly.GetType("UnityEngine.InputSystem.InputSystem") != null;
        }

        private void Update()
        {
            if (newInputSystem)
                NewInputSystemUpdate();
            else
                OldInputSystemUpdate();
        }

        private void NewInputSystemUpdate()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null)
            {
                Debug.LogError("未找到键盘!");
                return;
            }

            if (keyboard.upArrowKey.wasPressedThisFrame) _uiNavigationPanel.Navigate(NavType.Up);
            else if (keyboard.downArrowKey.wasPressedThisFrame) _uiNavigationPanel.Navigate(NavType.Down);
            else if (keyboard.leftArrowKey.wasPressedThisFrame) _uiNavigationPanel.Navigate(NavType.Left);
            else if (keyboard.rightArrowKey.wasPressedThisFrame) _uiNavigationPanel.Navigate(NavType.Right);
            else if (keyboard.spaceKey.wasPressedThisFrame) _uiNavigationPanel.Navigate(NavType.Click);
        }

        private void OldInputSystemUpdate()
        {
            if (Input.GetKeyDown(KeyCode.UpArrow)) _uiNavigationPanel.Navigate(NavType.Up);
            if (Input.GetKeyDown(KeyCode.DownArrow)) _uiNavigationPanel.Navigate(NavType.Down);
            if (Input.GetKeyDown(KeyCode.LeftArrow)) _uiNavigationPanel.Navigate(NavType.Left);
            if (Input.GetKeyDown(KeyCode.RightArrow)) _uiNavigationPanel.Navigate(NavType.Right);
            if (Input.GetKeyDown(KeyCode.Return)) _uiNavigationPanel.Navigate(NavType.Click);
        }
    }
}