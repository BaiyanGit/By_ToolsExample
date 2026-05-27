using UnityEngine;

public class AutoRotate : MonoBehaviour
{
    [Header("旋转设置")] public float rotateSpeedX = 50f;
    public float rotateSpeedY = 30f;
    public float rotateSpeedZ = 20f;
    public bool useWorldSpace = false;

    [Header("音频响应（随音量变化）")] [Tooltip("音频源（若为空则自动查找）")]
    public AudioSource audioSource;

    [Tooltip("音量平滑速度（0~1，越小越平滑）")] public float smoothSpeed = 0.2f;

    [Header("颜色变化")] public bool enableColorChange = true;
    public Color quietColor = Color.white;
    public Color loudColor = Color.red;
    [Tooltip("颜色敏感度（音量×此值再钳制）")] public float colorIntensity = 2f;

    [Header("大小变化")] public bool enableScaleChange = true;
    public Vector3 quietScale = Vector3.one;
    public Vector3 loudScale = Vector3.one * 1.5f;
    [Tooltip("大小敏感度")] public float scaleIntensity = 1.5f;

    [Header("旋转速度变化")] public bool enableSpeedChange = true;
    public float speedMultiplierMin = 0.5f; // 静音时速度倍数
    public float speedMultiplierMax = 2.0f; // 最大音量时速度倍数
    [Tooltip("速度敏感度")] public float speedIntensity = 1.5f;

    private Renderer objRenderer;
    private Material dynamicMaterial;
    private float currentVolume = 0f;
    private Vector3 originalScale;
    private float originalSpeedX, originalSpeedY, originalSpeedZ;

    private void Awake()
    {
        // 获取渲染器并复制材质
        objRenderer = GetComponent<Renderer>();
        if (objRenderer != null)
        {
            dynamicMaterial       = Instantiate(objRenderer.material);
            objRenderer.material  = dynamicMaterial;
            dynamicMaterial.color = quietColor;
        }

        // 记录原始大小和旋转速度
        originalScale  = transform.localScale;
        originalSpeedX = rotateSpeedX;
        originalSpeedY = rotateSpeedY;
        originalSpeedZ = rotateSpeedZ;

        // 查找音频源
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            Debug.LogWarning("未找到 AudioSource，音量相关效果将无效");
    }

    private void Update()
    {
        // 1. 获取当前平滑后的音量
        if (audioSource != null && audioSource.isPlaying)
        {
            float rawVolume = GetAverageVolume();
            currentVolume = Mathf.Lerp(currentVolume, rawVolume, 1f - smoothSpeed);
        }
        else
        {
            currentVolume = Mathf.Lerp(currentVolume, 0f, 1f - smoothSpeed);
        }

        // 2. 颜色变化
        if (enableColorChange && objRenderer != null)
        {
            float t           = Mathf.Clamp01(currentVolume * colorIntensity);
            Color targetColor = Color.Lerp(quietColor, loudColor, t);
            dynamicMaterial.color = targetColor;
        }

        // 3. 大小变化
        if (enableScaleChange)
        {
            float   t           = Mathf.Clamp01(currentVolume * scaleIntensity);
            Vector3 targetScale = Vector3.Lerp(quietScale, loudScale, t);
            transform.localScale = targetScale;
        }

        // 4. 旋转速度变化（动态修改旋转速度值）
        if (enableSpeedChange)
        {
            float t          = Mathf.Clamp01(currentVolume * speedIntensity);
            float multiplier = Mathf.Lerp(speedMultiplierMin, speedMultiplierMax, t);
            // 注意：这里直接修改公共字段，会影响 Inspector 显示，但不影响原始值（原始值已记录）
            rotateSpeedX = originalSpeedX * multiplier;
            rotateSpeedY = originalSpeedY * multiplier;
            rotateSpeedZ = originalSpeedZ * multiplier;
        }

        // 5. 执行旋转（使用当前的速度值）
        float xRot = rotateSpeedX * Time.deltaTime;
        float yRot = rotateSpeedY * Time.deltaTime;
        float zRot = rotateSpeedZ * Time.deltaTime;
        if (useWorldSpace)
            transform.Rotate(xRot, yRot, zRot, Space.World);
        else
            transform.Rotate(xRot, yRot, zRot, Space.Self);
    }

    /// <summary>
    /// 获取平均音量（RMS）
    /// </summary>
    private float GetAverageVolume()
    {
        int     sampleCount = 256;
        float[] samples     = new float[sampleCount];
        audioSource.GetOutputData(samples, 0);
        float sum = 0f;
        for (int i = 0; i < sampleCount; i++)
            sum += Mathf.Abs(samples[i]);
        return sum / sampleCount;
    }
}