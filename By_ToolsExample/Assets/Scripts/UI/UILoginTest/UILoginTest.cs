namespace UI.UILoginTest
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;
    using MetaFramework.UI;
    
    /// <summary>
    /// 负责接收用户界面UI元素的操作逻辑
    /// </summary>
    public class UILoginTest : UIBase<UIModelLoginTest, UIViewLoginTest>
    {
        public override UILayer GetLayer()
        {
            return UILayer.Center;
        }
    
        public override void OnInit()
        {
            
        }
    
        public override void OnEnter(params object[] args)
        {
            
        }
    
        public override void OnExit()
        {
            
        }
    }
}
