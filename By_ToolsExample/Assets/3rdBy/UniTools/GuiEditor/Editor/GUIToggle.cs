namespace _3rdBy.UniTools.GuiEditor.Editor
{
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.Events;

    /// <summary>
    /// OnGuiToggle
    /// </summary>
    public class GUIToggle
    {
        public class ToggleEvent : UnityEvent<bool>
        {
        }

        public readonly ToggleEvent onValueChanged = new();

        private readonly ToggleManager _toggleManager;
        public bool isOn { get; set; }
        public string togName { get; }

        public float nameWidth { get; set; }

        public GUIToggle(string togName, float nameWidth, bool isOn, ToggleManager toggleManager)
        {
            this.isOn = isOn;
            this.togName = togName;
            this.nameWidth = nameWidth;
            _toggleManager = toggleManager;
        }

        public void DrawComponent()
        {
            isOn = EditorGUILayout.Toggle("", isOn, GUILayout.Width(30));
            _toggleManager.SetToggleExclusive(this, isOn);
            GUILayout.Label(togName, GUILayout.Width(nameWidth));
            onValueChanged?.Invoke(isOn);
        }
    }
}