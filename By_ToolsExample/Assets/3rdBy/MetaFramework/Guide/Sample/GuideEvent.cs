using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace ZCustom
{
    /// <summary>
    /// 事件引导
    /// </summary>
    public class GuideEvent : GuideBase
    {
        /// <summary>
        /// 事件类型
        /// </summary>
        public EEventGuideType gEvent;

        public override void Begin()
        {
            switch (gEvent)
            {
                case EEventGuideType.SingleClick:
                    GTargetObj.AddComponent<GuideEvents>().SingleClick += EndEvent;
                    break;
                case EEventGuideType.DoubleClick:
                    GTargetObj.AddComponent<GuideEvents>().DoubleClick += EndEvent;
                    break;
                case EEventGuideType.GREAT:

                    break;
            }
        }

        private void EndEvent()
        {
            Destroy(GTargetObj.GetComponent<GuideEvents>());
            GCallBack?.Invoke(true, "");
        }

        public override void End()
        {

        }

        public override void OnUpdate()
        {

        }
    }
}
