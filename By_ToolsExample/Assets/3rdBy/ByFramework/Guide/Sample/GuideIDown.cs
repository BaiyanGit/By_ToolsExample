namespace _3rdBy.MetaFramework.Guide.Sample
{
    using UnityEngine;

    /// <summary>
    /// 键盘按键引导
    /// </summary>
    public class GuideIDown : GuideBase
    {
        /// <summary>
        /// 按键
        /// </summary>
        public KeyCode gInput;

        public override void Begin()
        {

        }

        public override void End()
        {

        }

        public override void OnUpdate()
        {
            if (gInput != KeyCode.None && Input.GetKeyDown(gInput))
            {
                GCallBack?.Invoke(true, "");
            }
        }
    }
}
