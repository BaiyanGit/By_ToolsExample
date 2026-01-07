using UnityEditor;

namespace _3rdBy.SoundManager.Editor
{
    using Editor = UnityEditor.Editor;

    [CustomEditor(typeof(ButtonSound)), CanEditMultipleObjects]
    public class ButtonSoundEditor : Editor
    {
        private SerializedProperty audioName;
        private SerializedProperty audioIndex;
        private SerializedProperty audioClip;
        private SerializedProperty buttonSoundType;

        private void OnEnable()
        {
            audioName       = serializedObject.FindProperty("audioName");
            audioIndex      = serializedObject.FindProperty("audioIndex");
            audioClip       = serializedObject.FindProperty("audioClip");
            buttonSoundType = serializedObject.FindProperty("buttonSoundType");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(buttonSoundType);

            switch (buttonSoundType.enumValueIndex)
            {
                case (int)ButtonSoundType.AudioClipIndex:
                    EditorGUILayout.PropertyField(audioIndex);
                    break;
                case (int)ButtonSoundType.AudioClipMount:
                    EditorGUILayout.PropertyField(audioClip);
                    break;
                case (int)ButtonSoundType.AudioClipName:
                    EditorGUILayout.PropertyField(audioName);
                    break;
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}