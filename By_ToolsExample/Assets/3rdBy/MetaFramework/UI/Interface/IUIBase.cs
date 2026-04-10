namespace _3rdBy.MetaFramework.UI
{
    using UnityEngine;

    public interface IUIBase
    {
        string uiName { get; set; }
        GameObject uiGo { get; set; }

        UIHierarchy uiHierarchy { get; set; }

        UIType uiType { get; set; }
        
        bool isShowing { get; set; }

        IUIModel uiModel { get; set; }
        IUIView uiView { get; set; }

        UILayer GetLayer();

        void OnInit();

        void OnEnter(params object[] args);

        void OnPause();

        void OnResume();

        void OnExit();

        void OnUpdate();
    }
}