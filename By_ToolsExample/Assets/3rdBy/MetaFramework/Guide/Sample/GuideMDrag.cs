using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace ZCustom
{
    /// <summary>
    /// 鼠标拖拽引导
    /// </summary>
    public class GuideMDrag : GuideBase
    {
        /// <summary>
        /// 鼠标按键
        /// </summary>
        public int gKey;

        /// <summary>
        /// 拖拽方向
        /// </summary>
        public string gDir = "Mouse X";

        /// <summary>
        /// 拖拽阈值
        /// </summary>
        public float gValue;

        public override void Begin()
        {

        }

        public override void End()
        {

        }

        public override void OnUpdate()
        {
            if (gDir != "")
            {
                if (Input.GetMouseButton(gKey))
                {
                    if (Mathf.Abs(Input.GetAxis(gDir)) >= gValue)
                    {
                        GCallBack?.Invoke(true, "");
                    }
                }
            }
            else
            {
                if (Mathf.Abs(Input.GetAxis(gDir)) >= gValue)
                {
                    GCallBack?.Invoke(true, "");
                }
            }
        }
    }
}