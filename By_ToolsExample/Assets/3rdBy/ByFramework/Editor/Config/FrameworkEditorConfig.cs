//=====================================================
// 文件名称: FrameworkEditorConfig.cs
// 创 建 者: wangbaiyan
// 创建日期: 2026-06-05
// 描    述: 定义 ByFramework Editor 工具统一配置结构。
//=====================================================

namespace _3rdBy.ByFramework.Editor.Config
{
    using _3rdBy.ByFramework.UI.Editor.UIAutoCreate;
    using UnityEngine;

    /// <summary>
    /// ByFramework Editor 工具统一配置资源。
    /// 第一阶段仅建立配置结构，不改变现有 Editor 工具的配置读取行为。
    /// </summary>
    [CreateAssetMenu(fileName = "FrameworkEditorConfig", menuName = "ByFramework/Framework Editor Config")]
    public sealed class FrameworkEditorConfig : ScriptableObject
    {
        [Header("UI 预制体输出路径")]
        public string prefabOutputPath = "Assets/Resources/Prefab/UI/";

        [Header("UI 脚本输出路径")]
        public string scriptOutputPath = "Assets/Scripts/UI/";

        [Header("UIView 自动生成组件配置")]
        public UIViewAutoCreateConfig uiViewAutoCreateConfig;
    }
}
