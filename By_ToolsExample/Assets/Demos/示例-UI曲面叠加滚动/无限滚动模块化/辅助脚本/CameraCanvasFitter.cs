namespace Demos.示例_UI曲面叠加滚动.无限滚动模块化.辅助脚本
{
    using UnityEngine;

    /// <summary>
    /// 此脚本以不同的方式将相机安装到画布上。
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class CameraCanvasFitter : MonoBehaviour
    {
        public Canvas targetCanvas;

        public enum FitMode
        {
            Orthographic,
            PerspectiveFixedDistance,
            PerspectiveFixedFOV
        }

        [Header("适配模式")] public FitMode fitMode = FitMode.Orthographic;
        [Header("正交模式")] public float orthographicDistance = 100f;
        [Header("透视模式-固定距离")] public float perspectiveDistance = 100f;
        [Header("透视模式-固定FOV")] public float perspectiveFOV = 60f;

        [ContextMenu("将相机安装到画布")]
        public void FitCameraToCanvas()
        {
            if (targetCanvas == null) return;

            var cam        = GetComponent<Camera>();
            var canvasRect = targetCanvas.GetComponent<RectTransform>();

            // 获取Canvas世界尺寸
            var corners = new Vector3[4];
            canvasRect.GetWorldCorners(corners);
            float worldWidth   = Vector3.Distance(corners[0], corners[3]);
            float worldHeight  = Vector3.Distance(corners[0], corners[1]);
            var   canvasCenter = canvasRect.position;

            switch (fitMode)
            {
                case FitMode.Orthographic:
                    cam.orthographic = true;
                    float screenAspect      = Screen.width / (float)Screen.height;
                    float orthoSizeByHeight = worldHeight / 2f;
                    float orthoSizeByWidth  = (worldWidth / 2f) / screenAspect;
                    cam.orthographicSize   = Mathf.Max(orthoSizeByHeight, orthoSizeByWidth);
                    cam.transform.position = canvasCenter - canvasRect.forward * orthographicDistance;
                    cam.transform.rotation = Quaternion.LookRotation(canvasRect.forward);
                    break;

                case FitMode.PerspectiveFixedDistance:
                    cam.orthographic       = false;
                    cam.transform.position = canvasCenter - canvasRect.forward * perspectiveDistance;
                    cam.transform.rotation = Quaternion.LookRotation(canvasRect.forward);
                    float fovV        = 2f * Mathf.Atan(worldHeight / (2f * perspectiveDistance)) * Mathf.Rad2Deg;
                    float fovH        = 2f * Mathf.Atan(worldWidth / (2f * perspectiveDistance)) * Mathf.Rad2Deg;
                    float fovVByWidth = fovH / (Screen.width / (float)Screen.height);
                    cam.fieldOfView = Mathf.Max(fovV, fovVByWidth);
                    break;

                case FitMode.PerspectiveFixedFOV:
                    cam.orthographic = false;
                    cam.fieldOfView  = perspectiveFOV;
                    float aspect                = Screen.width / (float)Screen.height;
                    float distanceByHeight      = (worldHeight / 2f) / Mathf.Tan(perspectiveFOV * 0.5f * Mathf.Deg2Rad);
                    float requiredHorizontalFOV = perspectiveFOV * aspect;
                    float distanceByWidth       = (worldWidth / 2f) / Mathf.Tan(requiredHorizontalFOV * 0.5f * Mathf.Deg2Rad);
                    float requiredDistance      = Mathf.Max(distanceByHeight, distanceByWidth);
                    cam.transform.position = canvasCenter - canvasRect.forward * requiredDistance;
                    cam.transform.rotation = Quaternion.LookRotation(canvasRect.forward);
                    break;
            }
        }
    }
}