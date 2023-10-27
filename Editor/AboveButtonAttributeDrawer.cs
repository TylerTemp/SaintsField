using System;
using System.Linq;
using System.Reflection;
using ExtInspector.Editor.Standalone;
using ExtInspector.Editor.Utils;
using ExtInspector.Standalone;
using UnityEditor;
using UnityEngine;

namespace ExtInspector.Editor
{
    [CustomPropertyDrawer(typeof(AboveButtonAttribute))]
    public class AboveButtonAttributeDrawer: DecButtonAttributeDrawer
    {
        protected override float GetAboveExtraHeight(SerializedProperty property, GUIContent label,
            float width,
            ISaintsAttribute saintsAttribute) => GetExtraHeight(property, label, width, saintsAttribute);


        protected override bool WillDrawAbove(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute)
        {
            return true;
        }

        protected override Rect DrawAbove(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute) => Draw(position, property, label, saintsAttribute);
    }
}
