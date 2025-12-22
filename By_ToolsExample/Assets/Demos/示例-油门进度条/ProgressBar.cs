using UnityEngine;
using UnityEngine.UI;

public class ProgressBar : MonoBehaviour
{
    public Image[] progressImages;
    public Color defaultColor = Color.gray;
    public Color progressColor = Color.green;

    [Range(0, 100)] public int testProgress;

    // 各个范围的起始值
    private readonly float[] _progressRanges =
        { 4, 8, 14, 18, 24, 28, 34, 38, 44, 48, 54, 58, 64, 68, 74, 78, 84, 88, 94, 98 };

    private void Start()
    {
        progressImages = GetComponentsInChildren<Image>();
        ResetProgressBar();
    }

    private void Update()
    {
        UpdateProgress(testProgress);
    }

    private void UpdateProgress(float progress)
    {
        progress = Mathf.Clamp(progress, 0, 100);

        for (var i = 0; i < progressImages.Length; i++)
        {
            progressImages[i].color = IsInRange(progress, i) ? progressColor : defaultColor;
        }
    }

    private void ResetProgressBar()
    {
        foreach (var img in progressImages)
        {
            img.color = defaultColor;
        }
    }

    private bool IsInRange(float progress, int index)
    {
        if (index < 0 || index >= _progressRanges.Length)
        {
            return false;
        }

        return progress >= _progressRanges[index];
    }
}