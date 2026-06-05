namespace Demos.示例_记录编辑场景对象.Editor
{
    using Scripts;
    using UnityEditor;
    using UnityEngine;

    [CustomEditor(typeof(ObjectDataSnap))]
    public class SaverEditorSnap : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var saver = (ObjectDataSnap)target;
            // if (saver.gameObject != null)
            // {
            //     
            //     GUILayout.BeginHorizontal();
            //     GUILayout.Label("Rotation");
            //     EditorGUILayout.TextField(saver.baseData.rotation[0].ToString(CultureInfo.InvariantCulture));
            //     EditorGUILayout.TextField(saver.baseData.rotation[1].ToString(CultureInfo.InvariantCulture));
            //     EditorGUILayout.TextField(saver.baseData.rotation[2].ToString(CultureInfo.InvariantCulture));
            //     GUILayout.EndHorizontal();
            //     
            //     GUILayout.BeginHorizontal();
            //     GUILayout.Label("Position");
            //     EditorGUILayout.TextField(saver.baseData.position[0].ToString(CultureInfo.InvariantCulture));
            //     EditorGUILayout.TextField(saver.baseData.position[1].ToString(CultureInfo.InvariantCulture));
            //     EditorGUILayout.TextField(saver.baseData.position[2].ToString(CultureInfo.InvariantCulture));
            //     GUILayout.EndHorizontal();
            // }

            if (GUILayout.Button("获取属性"))
            {
                saver.GetObjectAttributes();
            }

            if (GUILayout.Button("保存属性"))
            {
                saver.SaveObjectAttributes();
            }
        }
    }
}