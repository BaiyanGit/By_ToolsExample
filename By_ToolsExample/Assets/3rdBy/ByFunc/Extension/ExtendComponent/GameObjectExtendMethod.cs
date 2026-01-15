namespace _3rdBy.ByFunc.Extension.ExtendComponent
{
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// [GameObject] 与 [Transform]的扩展方法
    /// </summary>
    public static class GameObjectExtendMethod
    {
        /// <summary>
        /// 获取或者添加组件
        /// </summary>
        /// <param name="self"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T GetOrAddComponent<T>(this GameObject self) where T : MonoBehaviour
        {
            var t = self.GetComponent<T>();
            if (t == null)
            {
                t = self.AddComponent<T>();
            }

            return t;
        }

        /// <summary>
        /// 获取或者添加组件
        /// </summary>
        /// <param name="self"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T GetOrAddComponent<T>(this Transform self) where T : MonoBehaviour
        {
            var t = self.gameObject.GetComponent<T>();
            if (t == null)
            {
                t = self.gameObject.AddComponent<T>();
            }

            return t;
        }

        /// <summary>
        /// 获取或者添加组件
        /// </summary>
        /// <param name="self"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T GetOrAddComponent<T>(this Component self) where T : MonoBehaviour
        {
            var t = self.gameObject.GetComponent<T>();
            if (t == null)
            {
                t = self.gameObject.AddComponent<T>();
            }

            return t;
        }

        public static T GetOrAddComponentBehaviour<T>(this GameObject self) where T : Component
        {
            var t = self.gameObject.GetComponent<T>();
            if (t == null)
            {
                t = self.gameObject.AddComponent<T>();
            }

            return t;
        }

        /// <summary>
        /// 查找所有子对象
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static List<GameObject> ChildList(this Transform self)
        {
            var rtnList    = new List<GameObject>();
            var childCount = self.childCount;
            for (var i = 0; i < childCount; i++)
            {
                rtnList.Add(self.GetChild(i).gameObject);
            }

            return rtnList;
        }

        /// <summary>
        /// 将当前GameObject的位置和旋转设置为target的位置和旋转
        /// </summary>
        /// <param name="self"></param>
        /// <param name="target"></param>
        public static void Set2TargetGameObject(this GameObject self, GameObject target)
        {
            self.transform.SetPositionAndRotation(target.transform.position, target.transform.rotation);
        }
    }
}