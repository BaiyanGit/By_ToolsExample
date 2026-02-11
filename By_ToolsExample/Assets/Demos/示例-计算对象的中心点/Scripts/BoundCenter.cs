using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Demos.示例_计算对象的中心点
{
    /// <summary>
    /// 计算一个游戏对象及其所有子对象的边界中心，并创建或更新一个名为“BoundsCenter_游戏对象名称”的子对象来表示这个边界中心。
    /// </summary>
    public class BoundCenter : MonoBehaviour
    {
        [ContextMenu("Calculate Bounds Center")]
        private void CalculateBoundsCenter()
        {
            var childList = new List<Transform>();
            TraverseWithStack(transform, child => { childList.Add(child); });

            var bounds      = new Bounds();
            var initialized = false;
            foreach (var child in childList)
            {
                if (!initialized)
                {
                    bounds      = new Bounds(child.position, Vector3.zero);
                    initialized = true;
                }
                else
                {
                    bounds.Encapsulate(child.position);
                }
            }

            var       boundsCenter  = transform.GetChild(0);
            var       centerObjName = $"BoundsCenter_{transform.name}";
            Transform center;
            if (boundsCenter.name != centerObjName)
            {
                var go = new GameObject(centerObjName);
                go.transform.SetParent(transform);
                center = go.transform;
            }
            else
            {
                center = boundsCenter;
            }

            center.position = bounds.center;
            center.SetAsFirstSibling();
        }


        /// <summary>
        /// 迭代深度遍历方法(防止堆栈溢出)
        /// </summary>
        /// <param name="root"></param>
        /// <param name="action"></param>
        private static void TraverseWithStack(Transform root, UnityAction<Transform> action)
        {
            var stack = new Stack<Transform>();
            stack.Push(root);

            while (stack.Count > 0)
            {
                var current = stack.Pop();
                action(current);

                // 反向压入子节点以保证遍历顺序正确
                for (var i = current.childCount - 1; i >= 0; i--)
                {
                    stack.Push(current.GetChild(i));
                }
            }
        }
    }
}