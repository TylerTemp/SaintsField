using System;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(RequiredAttribute))]
    public class RequiredAttributeDrawer: SaintsPropertyDrawer
    {
        private string _error = "";

        protected override bool DrawPostFieldImGui(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, bool valueChanged)
        {
            property.serializedObject.ApplyModifiedProperties();
            if (Truly(property))
            {
                _error = "";
                return true;
            }

            string errorMessage = ((RequiredAttribute)saintsAttribute).ErrorMessage;
            _error = errorMessage ?? $"{property.displayName} is required";
            return true;
        }

        private static bool Truly(SerializedProperty property)
        {
            // UnityEngine.Object target = property.serializedObject.targetObject;
            //
            // (ReflectUtils.GetPropType getPropType, object fieldOrMethodInfo) found = ReflectUtils.GetProp(target.GetType(), property.name);
            // switch (found)
            // {
            //     case (ReflectUtils.GetPropType.Property, PropertyInfo propertyInfo):
            //     {
            //         return ReflectUtils.Truly(propertyInfo.GetValue(target));
            //     }
            //     case (ReflectUtils.GetPropType.Field, FieldInfo foundFieldInfo):
            //     {
            //         return ReflectUtils.Truly(foundFieldInfo.GetValue(target));
            //     }
            //     // ReSharper disable once RedundantCaseLabel
            //     case (ReflectUtils.GetPropType.NotFound, _):
            //     // ReSharper disable once RedundantCaseLabel
            //     case (ReflectUtils.GetPropType.Method, MethodInfo _):
            //     default:
            //         throw new ArgumentOutOfRangeException(nameof(found.getPropType), found.getPropType, null);
            // }

            UnityEngine.Object target = property.serializedObject.targetObject;

            (ReflectUtils.GetPropType getPropType, object fieldOrMethodInfo) found = ReflectUtils.GetProp(target.GetType(), property.name);

            if (found.getPropType == ReflectUtils.GetPropType.Property && found.fieldOrMethodInfo is PropertyInfo propertyInfo)
            {
                return ReflectUtils.Truly(propertyInfo.GetValue(target));
            }
            else if (found.getPropType == ReflectUtils.GetPropType.Field && found.fieldOrMethodInfo is FieldInfo foundFieldInfo)
            {
                return ReflectUtils.Truly(foundFieldInfo.GetValue(target));
            }
            else if (found.getPropType == ReflectUtils.GetPropType.NotFound || found.getPropType == ReflectUtils.GetPropType.Method)
            {
                throw new ArgumentOutOfRangeException(nameof(found.getPropType), found.getPropType, null);
            }
            else
            {
                // Handle any other cases here, if needed
                throw new NotImplementedException("Unexpected case");
            }

        }

        protected override bool WillDrawBelow(SerializedProperty property, ISaintsAttribute saintsAttribute) => ValidateType(property) != "";

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width, ISaintsAttribute saintsAttribute) => ValidateType(property) == "" ? 0 : ImGuiHelpBox.GetHeight(_error, width, MessageType.Error);

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute) => ValidateType(property) == "" ? position : ImGuiHelpBox.Draw(position, _error, MessageType.Error);

        private string ValidateType(SerializedProperty property)
        {
            if (property.propertyType == SerializedPropertyType.Integer || property.propertyType == SerializedPropertyType.Float)
            {
                return _error = $"`{property.displayName}` can not be int or float.";
            }

            return _error;
        }
    }
}
