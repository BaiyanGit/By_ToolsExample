namespace _3rdBy.MetaFramework.Guide.Sample
{
    using UnityEngine;

    /// <summary>
    /// 鼠标滚轮引导
    /// </summary>
    public class GuideMWheel : GuideBase
    {
        /// <summary>
        /// 滑动阈值
        /// </summary>
        public float gValue = 0.09f;

        private string whellName = "Mouse ScrollWheel";

        public override void Begin()
        {

        }

        public override void End()
        {

        }

        public override void OnUpdate()
        {
            if (Mathf.Abs(Input.GetAxis(whellName)) > gValue)
            {
                GCallBack?.Invoke(true, "");
            }
        }
    }
}