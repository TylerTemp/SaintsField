using System;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
    public class ReadOnlyAttributeDrawer: SaintsPropertyDrawer
    {
        private string _error = "";

        protected override (bool isActive, Rect position) DrawPreLabel(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute)
        {
            if(IsDisabled(property, (ReadOnlyAttribute)saintsAttribute))
            {
                EditorGUI.BeginDisabledGroup(true);
            }
            return (true, position);
        }

        protected override bool DrawPostField(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, bool valueChanged)
        {
            EditorGUI.EndDisabledGroup();
            return true;
        }

        protected override bool WillDrawBelow(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute)
        {
            return _error != "";
        }

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute)
        {
            if (_error == "")
            {
                return position;
            }

            (Rect errorRect, Rect leftRect) = RectUtils.SplitHeightRect(position, HelpBox.GetHeight(_error, position.width, MessageType.Error));
            HelpBox.Draw(errorRect, _error, MessageType.Error);
            return leftRect;
        }

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label,
            float width,
            ISaintsAttribute saintsAttribute)
        {
            // Debug.Log("check extra height!");
            if (_error == "")
            {
                return 0;
            }

            // Debug.Log(HelpBox.GetHeight(_error));
            return HelpBox.GetHeight(_error, width, MessageType.Error);
        }

        private bool IsDisabled(SerializedProperty property, ReadOnlyAttribute targetAttribute)
        {
            string[] bys = targetAttribute.ReadOnlyBys;
            if(bys is null)
            {
                return targetAttribute.ReadOnlyDirectValue;
            }

            UnityEngine.Object target = property.serializedObject.targetObject;

            foreach (string by in bys)
            {
                (ReflectUtils.GetPropType getPropType, object fieldOrMethodInfo) found = ReflectUtils.GetProp(target.GetType(), by);
                bool result;
                switch (found)
                {
                    case (ReflectUtils.GetPropType.NotFound, _):
                    {
                        _error = $"No field or method named `{by}` found on `{target}`";
                        Debug.LogError(_error);
                        result = false;
                    }
                        break;
                    case (ReflectUtils.GetPropType.Property, PropertyInfo propertyInfo):
                    {
                        result = ReflectUtils.Truly(propertyInfo.GetValue(target));
                    }
                        break;
                    case (ReflectUtils.GetPropType.Field, FieldInfo foundFieldInfo):
                    {
                        result = ReflectUtils.Truly(foundFieldInfo.GetValue(target));
                    }
                        break;
                    case (ReflectUtils.GetPropType.Method, MethodInfo methodInfo):
                    {
                        ParameterInfo[] methodParams = methodInfo.GetParameters();
                        Debug.Assert(methodParams.All(p => p.IsOptional));
                        // Debug.Assert(methodInfo.ReturnType == typeof(bool));
                        result =  ReflectUtils.Truly(methodInfo.Invoke(target, methodParams.Select(p => p.DefaultValue).ToArray()));
                    }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(found), found, null);
                }

                if (!result)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
