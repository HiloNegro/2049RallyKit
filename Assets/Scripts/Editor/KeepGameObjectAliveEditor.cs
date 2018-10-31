using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(KeepGameObjectAlive))]
public class KeepGameObjectAliveEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Show the script property in the Inspector
        SerializedProperty prop = serializedObject.FindProperty("m_Script");
        EditorGUILayout.PropertyField(prop, true, new GUILayoutOption[0]);
        // Show an info message
        EditorGUILayout.HelpBox("GameObjects that have this script, will not be destroyed when new scenes are loaded.", MessageType.Info);
    }
}