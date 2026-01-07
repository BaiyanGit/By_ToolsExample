namespace Demos.示例_卷帘门.ChatGPT.Scripts
{
    using UnityEngine;
    using UnityEngine.SceneManagement;

    public static class RoadRollSupport
    {
        public static readonly Color[] colors =
        {
            Color.red,             // 红色
            Color.green,           // 绿色
            Color.blue,            // 蓝色
            Color.yellow,          // 黄色
            Color.magenta,         // 紫色
            Color.cyan,            // 青色
            Color.white,           // 白色
            Color.black,           // 黑色
            new(1, 0.5f, 0),       // 橙色
            new(0.5f, 0, 0.5f),    // 紫色
            new(0, 0.5f, 0),       // 深绿
            new(0.5f, 0.5f, 0.5f), // 灰色

            new(0.6f, 0.2f, 0.8f), // 紫
            new(0.8f, 0.6f, 1),    // 淡紫

            new(0.2f, 0.6f, 1), // 天蓝
            new(0, 0, 0.8f),    // 深蓝
            new(0.6f, 0.8f, 1), // 浅蓝
            new(0, 0.4f, 0.6f), // 海军蓝

            new(0.2f, 0.8f, 0.2f), // 亮绿
            new(0, 0.6f, 0),       // 深绿
            new(0.6f, 1, 0.6f),    // 薄荷绿
            new(0.3f, 0.5f, 0.3f), // 森林绿

            new(1, 1, 0.2f),       // 亮黄
            new(0.9f, 0.9f, 0),    // 金黄
            new(0.8f, 0.8f, 0.3f), // 橄榄黄

            new(1, 0.5f, 0),     // 橙
            new(1, 0.65f, 0.3f), // 浅橙
            new(0.8f, 0.4f, 0),  // 深橙

            new(1, 0.2f, 0.2f),    // 亮红
            new(0.8f, 0.1f, 0.1f), // 暗红
            new(1, 0.4f, 0.4f),    // 浅红
            new(0.5f, 0, 0)        // 深红
        };


        // 查找当前激活场景中指定名字的单个对象（返回第一个找到的）
        public static GameObject FindGameObjectInActiveSceneByName(string name)
        {
            var activeScene = SceneManager.GetActiveScene();
            var rootObjects = activeScene.GetRootGameObjects();

            foreach (var rootObject in rootObjects)
            {
                // 如果根对象的名字匹配，直接返回
                if (rootObject.name == name)
                    return rootObject;

                // 在子对象中查找
                var target = rootObject.transform.Find(name);
                if (target != null)
                    return target.gameObject;

                // 如果上面的Find没有找到，可能是因为对象在更深的层级，或者有多个同名，我们可以使用递归
                // 注意：这里我们只返回第一个找到的，如果你需要所有同名的，请用List收集
                var found = FindInChildren(rootObject.transform, name);
                if (found != null)
                    return found;
            }

            return null;
        }

        // 递归查找子对象
        private static GameObject FindInChildren(Transform parent, string name)
        {
            foreach (Transform child in parent)
            {
                if (child.name == name)
                    return child.gameObject;

                var found = FindInChildren(child, name);
                if (found != null)
                    return found;
            }

            return null;
        }
    }
}