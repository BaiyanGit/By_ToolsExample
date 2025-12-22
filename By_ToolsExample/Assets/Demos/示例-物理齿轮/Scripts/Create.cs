using UnityEngine;

namespace 齿轮测试
{
    using System;
    using UnityEngine.Serialization;

    public class Create : MonoBehaviour
    {
        [Header("生成模板")] public Transform cloneObject;
        [Header("父级对象")] public Transform parent;
        [Header("创建数量")] public int insCount = 20;
        [Header("偏移距离")] public float offset = 0.193f;
        private Vector3 _defaultPos;

        private void Start()
        {
        }

        [ContextMenu("重命名")]
        private void ReName()
        {
            var childCount = parent.childCount;
            for (var i = 0; i < childCount; i++)
            {
                var childItem = parent.GetChild(i);

                childItem.name = $"锯齿_{i}";
            }
        }

        [ContextMenu("重新布局")]
        private void ResetLayout()
        {
            var childCount = parent.childCount;
            _defaultPos = cloneObject.localPosition;
            for (var i = 0; i < childCount; i++)
            {
                var child = parent.GetChild(i);
                var z     = _defaultPos.z + i * offset;
                child.localPosition = new Vector3(0, _defaultPos.y, z);
            }
        }

        [ContextMenu("生成")]
        private void CreateObject()
        {
            _defaultPos = cloneObject.localPosition;

            for (var i = 1; i <= insCount; i++)
            {
                var cloneItem = Instantiate(cloneObject, parent);
                var z         = _defaultPos.z + i * offset;
                cloneItem.localPosition = new Vector3(0, _defaultPos.y, z);

                // if (i == insCount)
                // {
                //     cloneObject = cloneItem;
                // }
            }
        }
    }
}