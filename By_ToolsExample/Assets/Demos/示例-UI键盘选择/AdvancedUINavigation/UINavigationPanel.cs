namespace AdvancedUINavigation
{
    using UnityEngine;
    using System.Collections.Generic;

    public enum NavType
    {
        [InspectorName("上选")] Up,
        [InspectorName("下选")] Down,
        [InspectorName("左选")] Left,
        [InspectorName("右选")] Right,
        [InspectorName("点击")] Click
    }

    /// <summary>
    /// UI导航面板：挂载每个UI界面上
    /// 收集当前UI界面下的所有导航项，并提供导航功能
    /// </summary>
    public class UINavigationPanel : MonoBehaviour
    {
        [SerializeField] [Header("导航项列表")] private List<UINavigationItem> _uiNavigationItems = new();
        [Header("当前选择项列表中的索引")] private int _curItemIndex;
        [Header("当前选择项")] private UINavigationItem _curItem;

        private void Start()
        {
            CollectNavigationItems();
        }

        public void ResetCollection()
        {
            CollectNavigationItems();
        }

        private void CollectNavigationItems()
        {
            _uiNavigationItems.Clear();

            var childrenItems = transform.GetComponentsInChildren<UINavigationItem>();
            foreach (var item in childrenItems)
            {
                if (item.gameObject.activeSelf == false) continue;
                _uiNavigationItems.Add(item);
            }

            if (_uiNavigationItems.Count == 0) return;
            _curItem = _uiNavigationItems[_curItemIndex];
            _curItem.OnEnter();
        }

        public void Navigate(NavType direction)
        {
            switch (direction)
            {
                case NavType.Up:
                    NavigateVertical(Vector2.up);
                    break;
                case NavType.Down:
                    NavigateVertical(Vector2.down);
                    break;
                case NavType.Left:
                    NavigateHorizontal(-1);
                    break;
                case NavType.Right:
                    NavigateHorizontal(1);
                    break;
                case NavType.Click:
                    if (_curItem == null) return;
                    _curItem.OnClick();
                    break;
                default:
                    break;
            }
        }

        private void NavigateVertical(Vector2 direction)
        {
            if (_uiNavigationItems.Count == 0) return;
            if (_curItem == null) _curItem = _uiNavigationItems[_curItemIndex];

            var              curItemPos      = (Vector2)_curItem.transform.position;
            var              closestDistance = float.MaxValue;
            UINavigationItem nextItem        = null;

            // 查找上下相邻的导航项
            foreach (var item in _uiNavigationItems)
            {
                if (item == _curItem) continue;
                var itemPos  = (Vector2)item.transform.position;
                var distance = Vector2.Distance(itemPos, curItemPos);
                if (!(Vector2.Dot(direction, itemPos - curItemPos) > 0) || !(distance < closestDistance))
                {
                    item.OnExit();
                    continue;
                }

                nextItem        = item;
                closestDistance = distance;
            }

            if (nextItem == null) return;
            _curItemIndex = _uiNavigationItems.IndexOf(nextItem);
            _curItem.OnExit();
            _curItem      = nextItem;
            nextItem.OnEnter();
        }

        private void NavigateHorizontal(int index)
        {
            index = (_curItemIndex + index + _uiNavigationItems.Count) % _uiNavigationItems.Count;

            _curItemIndex = index;
            _curItem.OnExit();
            _curItem = _uiNavigationItems[_curItemIndex];
            _curItem.OnEnter();
        }
    }
}