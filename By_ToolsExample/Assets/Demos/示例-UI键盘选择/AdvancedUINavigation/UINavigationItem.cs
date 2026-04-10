using UnityEngine;
using UnityEngine.UI;

namespace AdvancedUINavigation
{
    /// <summary>
    /// 此类用于控制UI元素的导航。
    /// 挂载此脚本的UI元素会自动成为导航目标。
    /// </summary>
    [RequireComponent(typeof(Selectable))]
    public class UINavigationItem : MonoBehaviour, INavigation
    {
        private Selectable _selectable;

        private void Awake()
        {
            _selectable = GetComponent<Selectable>();
        }

        public void OnEnter()
        {
            _selectable.image.color = Color.green;
        }

        public void OnExit()
        {
            _selectable.image.color = Color.white;
        }

        public void OnClick()
        {
            if (_selectable is Button btn)
                btn.onClick.Invoke();
        }
    }
}