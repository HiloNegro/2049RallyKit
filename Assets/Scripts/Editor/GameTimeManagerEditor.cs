using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(GameTimeManager))]
public class GameTimeManagerEditor : Editor
{
    SerializedProperty _script;
    SerializedProperty _timeToIdle;

    void OnEnable()
    {
        _script = serializedObject.FindProperty("m_Script");
        _timeToIdle = serializedObject.FindProperty("_timeToIdle");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // Show the script property in the Inspector
        EditorGUILayout.PropertyField(_script, true, new GUILayoutOption[0]);
        // Show an info message
        EditorGUILayout.HelpBox("Manager that controls time related events in the game.", MessageType.Info);
        // Show class properties
        EditorGUILayout.PropertyField(_timeToIdle);
        if (EditorApplication.isPlaying)
        {
            // Show scene play time
            EditorGUILayout.LabelField("Time playing this scene: " + (int)((GameTimeManager)serializedObject.targetObject).PlayTime);
            // Show idle time
            EditorGUILayout.LabelField("Idle time: " + (int)((GameTimeManager)serializedObject.targetObject).IdleTime);
            // Show current idle timer
            EditorGUILayout.LabelField("Idle timer: " + (int)((GameTimeManager)serializedObject.targetObject).IdleTimer);
        }

        serializedObject.ApplyModifiedProperties();

        EditorUtility.SetDirty(target);
    }
}