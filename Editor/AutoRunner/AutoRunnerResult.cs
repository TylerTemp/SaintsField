using System;
using SaintsField.Editor.Utils;
using UnityEditor;

namespace SaintsField.Editor.AutoRunner
{
    [Serializable]
    public struct AutoRunnerResult : IEquatable<AutoRunnerResult>
    {
        // public UnityEngine.Object mainTarget;
        public string mainTargetString;
        public bool mainTargetIsAssetPath;
        public UnityEngine.Object subTarget;
        public string propertyPath;
        public SerializedObject SerializedObject;
        [NonSerialized]
        public AutoRunnerFixerResult FixerResult;

        public bool Equals(AutoRunnerResult other)
        {
            return Equals(mainTargetString, other.mainTargetString) && Equals(mainTargetIsAssetPath, other.mainTargetIsAssetPath) && Equals(subTarget, other.subTarget) && propertyPath == other.propertyPath;
        }

        public override bool Equals(object obj)
        {
            return obj is AutoRunnerResult other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Util.CombineHashCode(mainTargetString, mainTargetIsAssetPath, subTarget, propertyPath);
            // return HashCode.Combine(mainTarget, subTarget, propertyPath);
        }
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
