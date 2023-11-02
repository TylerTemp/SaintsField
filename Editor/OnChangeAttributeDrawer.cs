using System;
using System.Reflection;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor
{
    [CustomPropertyDrawer(typeof(OnChangeAttribute))]
    public class OnChangeAttributeDrawer : SaintsPropertyDrawer
    {
        private string _error = "";

        protected override bool DrawPostField(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, bool valueChanged)
        {
            if (!valueChanged)
            {
                return true;
            }

            string callback = ((OnChangeAttribute)saintsAttribute).Callback;
            object target = property.serializedObject.targetObject;

            MethodInfo methodInfo = GetMethodInfo(target.GetType(), callback);
            if (methodInfo == null)
            {
                _error = $"no method found `{callback}` on `{target}`";
            }
        }

        protected MethodInfo GetMethodInfo(Type targetType, string fieldName)
        {
            const BindingFlags bindAttr = BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic |
                                          BindingFlags.Public | BindingFlags.DeclaredOnly;
            return targetType.GetMethod(fieldName, bindAttr);
        }

        protected override bool WillDrawBelow(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute) => _error != "";

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width, ISaintsAttribute saintsAttribute) => _error == "" ? 0 : HelpBox.GetHeight(_error, width, MessageType.Error);

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute) => _error == "" ? position : HelpBox.Draw(position, _error, MessageType.Error);
    }
}
