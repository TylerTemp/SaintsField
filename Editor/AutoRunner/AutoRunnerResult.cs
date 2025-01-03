using System;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.AutoRunner
{
    [Serializable]
    public struct AutoRunnerResult
    {
        public UnityEngine.Object MainTarget;
        public UnityEngine.Object SubTarget;
        public SerializedProperty SerializedProperty;
        public SerializedObject SerializedObject;

        public AutoRunnerFixerResult FixerResult;
    }

    // [CustomPropertyDrawer(typeof(AutoRunnerResult))]
    // public class AutoRunnerResultDrawer: PropertyDrawer
    // {
    //     public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    //     {
    //         EditorGUI.BeginProperty(position, label, property);
    //         EditorGUI.indentLevel = 0;
    //         position.height = EditorGUIUtility.singleLineHeight;
    //         EditorGUI.PropertyField(position, property.FindPropertyRelative("MainTarget"));
    //         position.y += EditorGUIUtility.singleLineHeight;
    //         EditorGUI.PropertyField(position, property.FindPropertyRelative("SubTarget"));
    //         position.y += EditorGUIUtility.singleLineHeight;
    //         // EditorGUI.PropertyField(position, property.FindPropertyRelative("SerializedProperty"));
    //         position.y += EditorGUIUtility.singleLineHeight;
    //         // EditorGUI.PropertyField(position, property.FindPropertyRelative("SerializedObject"));
    //         position.y += EditorGUIUtility.singleLineHeight;
    //         EditorGUI.PropertyField(position, property.FindPropertyRelative("FixerResult"));
    //         EditorGUI.EndProperty();
    //     }
    //
    //     public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    //     {
    //         return EditorGUIUtility.singleLineHeight * 5;
    //     }
    // }
}
