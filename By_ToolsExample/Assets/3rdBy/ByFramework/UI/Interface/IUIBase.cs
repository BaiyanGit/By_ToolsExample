namespace _3rdBy.ByFramework.UI.Interface
{
    using UnityEngine;

    public interface IUIBase
    {
        string UIName { get; set; }
        GameObject UIGo { get; set; }

        UIHierarchy UIHierarchy { get; set; }

        UIType UIType { get; set; }
        
        bool IsShowing { get; set; }

        IUIModel UIModel { get; set; }
        IUIView UIView { get; set; }

        UILayer GetLayer();

        void OnInit();

        void OnEnter(params object[] args);

        void OnPause();

        void OnResume();

        void OnExit();

        void OnUpdate();
    }
}