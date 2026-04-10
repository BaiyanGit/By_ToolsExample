namespace AdvancedUINavigation
{
    /// <summary>
    /// 定义UI导航接口
    /// </summary>
    public interface INavigation
    {
        /// <summary>
        /// 当UI获得焦点时调用
        /// </summary>
        void OnEnter();

        /// <summary>
        /// 当UI失去焦点时调用
        /// </summary>
        void OnExit();

        /// <summary>
        /// 当按下确认键时调用
        /// </summary>
        void OnClick();
    }
}