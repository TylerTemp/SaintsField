using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.ArraySizeDrawer
{
    public partial class ArraySizeAttributeDrawer
    {

        private string _error;

        protected override bool WillDrawAbove(SerializedProperty property, ISaintsAttribute saintsAttribute,
            FieldInfo info, object parent) => true;

        protected override float GetAboveExtraHeight(SerializedProperty property, GUIContent label, float width,
            ISaintsAttribute saintsAttribute,
            int index,
            FieldInfo info, object parent) => 0f;

        protected override Rect DrawAboveImGui(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute,
            OnGUIPayload onGUIPayload,
            FieldInfo info, object parent)
        {
            ArraySizeAttribute arraySizeAttribute = (ArraySizeAttribute)saintsAttribute;
            // int size = ((ArraySizeAttribute)saintsAttribute).Size;

            // Debug.Log(property.propertyPath);
            // SerializedProperty arrProp = property.serializedObject.FindProperty("nests.Array.data[0].arr3");
            // Debug.Log(arrProp);
            // Debug.Log(property.propertyPath);
            (string error, SerializedProperty arrProp) = SerializedUtils.GetArrayProperty(property);
            _error = error;
            if (_error != "")
            {
                return position;
            }

            (string errorCallback, bool _, int min, int max) = GetMinMax(arraySizeAttribute, property, info, parent);
            if (errorCallback != "")
            {
                _error = errorCallback;
                return position;
            }

            if (min >= 0 && arrProp.arraySize < min)
            {
                // Debug.Log(property.arraySize);
                // Debug.Log(property.propertyPath);
                arrProp.arraySize = min;
                // arrProp.serializedObject.ApplyModifiedProperties();
            }

            if (max >= 0 && arrProp.arraySize > max)
            {
                arrProp.arraySize = max;
            }

            return position;
        }

        protected override bool WillDrawBelow(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            FieldInfo info,
            object parent)
        {
            return _error != "";
        }

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width,
            ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent)
        {
            return _error == "" ? 0 : ImGuiHelpBox.GetHeight(_error, width, MessageType.Error);
        }

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, int index, IReadOnlyList<PropertyAttribute> allAttributes,
            OnGUIPayload onGuiPayload, FieldInfo info, object parent) =>
            _error == ""
                ? position
                : ImGuiHelpBox.Draw(position, _error, MessageType.Error);

    }
}
