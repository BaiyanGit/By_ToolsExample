namespace _3rdBy.ByTools.ScenePlaySelector.Editor
{
    using System;
    using System.Reflection;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.UIElements;

    public static class EditorToolbarUtil
    {
        private static Type _toolbarType;
        private static FieldInfo _rootField;

        public static VisualElement GetLeftZone()
        {
            _toolbarType ??= typeof(Editor).Assembly.GetType("UnityEditor.Toolbar");
            if (_toolbarType == null) return null;

            var toolbars = Resources.FindObjectsOfTypeAll(_toolbarType);
            if (toolbars.Length == 0) return null;

            _rootField ??= _toolbarType.GetField("m_Root", BindingFlags.NonPublic | BindingFlags.Instance);
            if (_rootField == null) return null;

            var root = _rootField.GetValue(toolbars[0]) as VisualElement;
            return root?.Q("ToolbarZoneLeftAlign");
        }

        public static void AddOrReplace(string name, VisualElement element)
        {
            var leftZone = GetLeftZone();
            if (leftZone == null) return;

            var old = leftZone.Q(name);
            if (old != null)
                leftZone.Remove(old);

            element.name = name;
            leftZone.Add(element);
        }
    }
}