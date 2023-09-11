using System;
using UnityEditor;
using UnityEngine;

namespace ExtInspector.Editor.Utils
{
    public class EditorGUIProperty: IDisposable
    {
        public readonly GUIContent Label;

        public EditorGUIProperty(Rect position, GUIContent label, SerializedProperty property)
        {
            Label = EditorGUI.BeginProperty(position, label, property);
        }

        public void Dispose()
        {
            EditorGUI.EndProperty();
        }
    }
}
