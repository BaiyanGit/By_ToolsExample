namespace _3rdBy.UniTools.GuiEditor.Editor
{
    using UnityEngine;
    using UnityEngine.Events;

    public class GUISlider
    {
        public class ToggleEvent : UnityEvent<float>
        {
        }

        public readonly ToggleEvent onValueChanged = new();
        public string sliderName { get; set; }
        public float progress { get; set; }

        private readonly GUILayoutOption[] _sliderOptions = { GUILayout.Width(300), GUILayout.Height(10) };

        public GUISlider(string sliderName, float progress)
        {
            this.sliderName = sliderName;
            this.progress = progress;
        }

        public void OnDraw()
        {
            GUILayout.Label($"{sliderName}值：{progress:F0}");
            progress = GUILayout.HorizontalSlider(progress, 0, 100, _sliderOptions);
            onValueChanged?.Invoke(progress);
        }
    }
}