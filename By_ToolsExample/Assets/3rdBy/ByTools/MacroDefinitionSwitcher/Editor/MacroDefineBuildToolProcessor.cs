//=====================================================
// 文件名称: MacroDefineBuildToolProcessor
// 创 建 者: wangbaiyan
// 创建日期: 2026-4-23
// 描    述: 处理跨编译域的挂起动作。
//=====================================================

namespace MacroDefineBuildToolEditor
{
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// 处理跨编译域的挂起动作。
    /// 场景：
    /// 1. 先切平台
    /// 2. 再写宏定义
    /// 3. 等 Unity 编译完成
    /// 4. 再继续打包
    /// </summary>
    [InitializeOnLoad]
    public static class MacroDefineBuildToolProcessor
    {
        static MacroDefineBuildToolProcessor()
        {
            EditorApplication.update += OnEditorUpdate;
        }

        /// <summary>
        /// 编辑器轮询入口。
        /// </summary>
        private static void OnEditorUpdate()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating || BuildPipeline.isBuildingPlayer)
            {
                return;
            }

            var config = MacroDefineBuildToolStorage.Load();
            if (config?.pendingAction == null || !config.pendingAction.hasPendingAction)
            {
                return;
            }

            ProcessPendingAction(config);
        }

        /// <summary>
        /// 处理挂起动作。
        /// </summary>
        private static void ProcessPendingAction(MacroDefineBuildToolConfig config)
        {
            var action = config.pendingAction;
            switch (action.step)
            {
                case PendingActionStep.SwitchBuildTarget:
                    HandleSwitchBuildTarget(config, action);
                    break;
                case PendingActionStep.ApplyDefineSymbols:
                    HandleApplyDefineSymbols(config, action);
                    break;
                case PendingActionStep.BuildOrFinish:
                    HandleBuildOrFinish(config, action);
                    break;
            }
        }

        /// <summary>
        /// 处理平台切换阶段。
        /// </summary>
        private static void HandleSwitchBuildTarget(MacroDefineBuildToolConfig config, PendingActionData action)
        {
            if (MacroDefineBuildToolUtility.GetCurrentActivePlatform() == action.platform)
            {
                action.step = PendingActionStep.ApplyDefineSymbols;
                MacroDefineBuildToolStorage.Save(config);
                return;
            }

            action.step = PendingActionStep.ApplyDefineSymbols;
            MacroDefineBuildToolStorage.Save(config);

            if (!MacroDefineBuildToolUtility.SwitchActiveBuildTarget(action.platform, out string error))
            {
                Debug.LogError($"[宏定义打包工具] {error}");
                MacroDefineBuildToolUtility.ClearPendingAction(config);
                MacroDefineBuildToolWindow.RepaintAllWindows();
                return;
            }

            Debug.Log($"[宏定义打包工具] 已请求切换平台：{MacroDefineBuildToolUtility.GetPlatformDisplayName(action.platform)}");
        }

        /// <summary>
        /// 处理宏定义应用阶段。
        /// </summary>
        private static void HandleApplyDefineSymbols(MacroDefineBuildToolConfig config, PendingActionData action)
        {
            string currentDefineSymbols = MacroDefineBuildToolUtility.GetScriptingDefineSymbols(action.platform);
            string targetDefineSymbols  = action.defineSymbols ?? string.Empty;

            if (string.Equals(currentDefineSymbols, targetDefineSymbols, System.StringComparison.Ordinal))
            {
                action.step = PendingActionStep.BuildOrFinish;
                MacroDefineBuildToolStorage.Save(config);
                return;
            }

            action.step = PendingActionStep.BuildOrFinish;
            MacroDefineBuildToolStorage.Save(config);

            MacroDefineBuildToolUtility.SetScriptingDefineSymbols(action.platform, targetDefineSymbols);
            // Debug.Log($"[宏定义打包工具] 已应用宏定义：{targetDefineSymbols}");
        }

        /// <summary>
        /// 处理结束阶段：决定直接完成还是开始打包。
        /// </summary>
        private static void HandleBuildOrFinish(MacroDefineBuildToolConfig config, PendingActionData action)
        {
            bool   needBuild  = action.buildAfterApply;
            var    platform   = action.platform;
            string sourceName = action.sourceName;

            MacroDefineBuildToolUtility.ClearPendingAction(config);
            MacroDefineBuildToolWindow.RepaintAllWindows();

            if (!needBuild)
            {
                // Debug.Log($"[宏定义打包工具] 已完成应用：{sourceName}");
                return;
            }

            bool success = MacroDefineBuildToolUtility.ExecuteBuild(config, platform, out string message);
            if (success)
            {
                // Debug.Log($"[宏定义打包工具] {message}");
                var isOpenFolder = EditorUtility.DisplayDialog("打包完成", message, "确定", "取消");
                if (isOpenFolder)
                {
                    // TODO: 打开文件夹
                    // MacroDefineBuildToolUtility.OpenFolder(config.outputPath);
                    //
                    // EditorUtility.OpenFilePanel("选择文件夹", "", "");
                }
            }
            else
            {
                // Debug.LogError($"[宏定义打包工具] {message}");
                EditorUtility.DisplayDialog("打包失败", message, "确定");
            }
        }
    }
}