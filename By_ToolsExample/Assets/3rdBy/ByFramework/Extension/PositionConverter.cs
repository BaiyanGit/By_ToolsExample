namespace _3rdBy.ByFramework.Extension
{
    using UnityEngine;

    public static class PositionConverter
    {
        /// <summary>
        /// 世界坐标转换为屏幕坐标
        /// </summary>
        /// <param name="worldPoint">屏幕坐标</param>
        /// <returns></returns>
        public static Vector2 WorldPointToScreenPoint(Camera camera, Vector3 worldPoint)
        {
            Vector2 screenPoint = camera.WorldToScreenPoint(worldPoint);
            return screenPoint;
        }

        /// <summary>
        /// 屏幕坐标转换为世界坐标
        /// </summary>
        /// <param name="screenPoint">屏幕坐标</param>
        /// <param name="planeZ">距离摄像机 Z 平面的距离</param>
        /// <returns></returns>
        public static Vector3 ScreenPointToWorldPoint(Camera camera, Vector2 screenPoint, float planeZ)
        {
            // Camera.main 世界摄像机
            var position   = new Vector3(screenPoint.x, screenPoint.y, planeZ);
            var worldPoint = camera.ScreenToWorldPoint(position);
            return worldPoint;
        }
    
        // RectTransformUtility.WorldToScreenPoint
        // RectTransformUtility.ScreenPointToWorldPointInRectangle
        // RectTransformUtility.ScreenPointToLocalPointInRectangle
        // 上面三个坐标转换的方法使用 Camera 的地方
        // 当 Canvas renderMode 为 RenderMode.ScreenSpaceCamera、RenderMode.WorldSpace 时 传递参数 canvas.worldCamera
        // 当 Canvas renderMode 为 RenderMode.ScreenSpaceOverlay 时 传递参数 null
    
        // UI 坐标转换为屏幕坐标
        public static Vector2 UIPointToScreenPoint(Camera camera, Vector3 worldPoint)
        {
            // RectTransform：target
            // worldPoint = target.position;

            var screenPoint = RectTransformUtility.WorldToScreenPoint(camera, worldPoint);
            return screenPoint;
        }

        // 屏幕坐标转换为 UGUI 坐标
        public static Vector3 ScreenPointToUIPoint(Camera camera, RectTransform rt, Vector2 screenPoint)
        {
            Vector3 globalMousePos;
            //UI屏幕坐标转换为世界坐标

            // 当 Canvas renderMode 为 RenderMode.ScreenSpaceCamera、RenderMode.WorldSpace 时 uiCamera 不能为空
            // 当 Canvas renderMode 为 RenderMode.ScreenSpaceOverlay 时 uiCamera 可以为空
            RectTransformUtility.ScreenPointToWorldPointInRectangle(rt, screenPoint, camera, out globalMousePos);
            // 转换后的 globalMousePos 使用下面方法赋值
            // target 为需要使用的 UI RectTransform
            // rt 可以是 target.GetComponent<RectTransform>(), 也可以是 target.parent.GetComponent<RectTransform>()
            // target.transform.position = globalMousePos;
            return globalMousePos;
        }

        // 屏幕坐标转换为 UGUI RectTransform 的 anchoredPosition
        public static Vector2 ScreenPointToUILocalPoint(Camera camera, RectTransform parentRT, Vector2 screenPoint)
        {
            Vector2 localPos;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRT, screenPoint, camera, out localPos);
            // 转换后的 localPos 使用下面方法赋值
            // target 为需要使用的 UI RectTransform
            // parentRT 是 target.parent.GetComponent<RectTransform>()
            // 最后赋值 target.anchoredPosition = localPos;
            return localPos;
        }
    }
}

