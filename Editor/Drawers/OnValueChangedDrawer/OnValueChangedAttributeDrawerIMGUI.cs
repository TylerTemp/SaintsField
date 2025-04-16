using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.OnValueChangedDrawer
{
    public partial class OnValueChangedAttributeDrawer
    {

        private string _error = "";

        protected override void OnPropertyEndImGui(SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, int saintsIndex, OnGUIPayload onGUIPayload, FieldInfo info, object parent)
        {
            if (!onGUIPayload.changed)
            {
                return;
            }

            int arrayIndex = SerializedUtils.PropertyPathIndex(property.propertyPath);

            _error = InvokeCallback(((OnValueChangedAttribute)saintsAttribute).Callback, onGUIPayload.newValue,
                arrayIndex, parent);
        }

        protected override bool WillDrawBelow(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            FieldInfo info,
            object parent) => _error != "";

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width,
            ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent) =>
            _error == "" ? 0 : ImGuiHelpBox.GetHeight(_error, width, MessageType.Error);

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, int index, IReadOnlyList<PropertyAttribute> allAttributes,
            OnGUIPayload onGuiPayload, FieldInfo info, object parent) =>
            _error == "" ? position : ImGuiHelpBox.Draw(position, _error, MessageType.Error);

    }
}
