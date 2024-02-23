using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;

#if UNITY_2021_3_OR_NEWER
using System;
using UnityEngine.UIElements;
#endif

namespace SaintsField.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(MinValueAttribute))]
    public class MinValueAttributeDrawer : SaintsPropertyDrawer
    {
        #region IMGUI

        private string _error = "";

        protected override bool DrawPostFieldImGui(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, int index, bool valueChanged, FieldInfo info, object parent)
        {
            if (!valueChanged)
            {
                return true;
            }

            // object parentTarget = GetParentTarget(property);

            MinValueAttribute minValueAttribute = (MinValueAttribute)saintsAttribute;
            (string error, float valueLimit) = GetLimitFloat(minValueAttribute, parent);

            _error = error;

            if (_error != "")
            {
                return true;
            }

            // ReSharper disable once ConvertIfStatementToSwitchStatement
            if (property.propertyType == SerializedPropertyType.Float)
            {
                float curValue = property.floatValue;

                if (valueLimit > curValue)
                {
                    property.floatValue = valueLimit;
                    SetValueChanged(property);
                }
            }
            else if (property.propertyType == SerializedPropertyType.Integer)
            {
                int curValue = property.intValue;

                if (valueLimit > curValue)
                {
                    property.intValue = (int)valueLimit;
                    SetValueChanged(property);
                }
            }
            return true;
        }

        protected override bool WillDrawBelow(SerializedProperty property, ISaintsAttribute saintsAttribute,
            FieldInfo info,
            object parent) => _error != "";

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width,
            ISaintsAttribute saintsAttribute, FieldInfo info, object parent) => _error == "" ? 0 : ImGuiHelpBox.GetHeight(_error, width, MessageType.Error);

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, FieldInfo info, object parent) => _error == "" ? position : ImGuiHelpBox.Draw(position, _error, MessageType.Error);
        #endregion

        private static (string error, float valueLimit) GetLimitFloat(MinValueAttribute minValueAttribute, object parentTarget)
        {
            return minValueAttribute.ValueCallback == null
                ? ("", minValueAttribute.Value)
                : Util.GetCallbackFloat(parentTarget, minValueAttribute.ValueCallback);
        }

#if UNITY_2021_3_OR_NEWER

        #region UIToolkit

        private static string NameHelpBox(SerializedProperty property, int index) =>
            $"{property.propertyPath}_{index}__MinValue_HelpBox";

        protected override VisualElement CreateBelowUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index,
            VisualElement container, FieldInfo info, object parent)
        {
            return new HelpBox("", HelpBoxMessageType.Error)
            {
                name = NameHelpBox(property, index),
                style =
                {
                    display = DisplayStyle.None,
                },
            };
        }

        protected override void OnUpdateUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            VisualElement container, Action<object> onValueChangedCallback, object parent)
        {
            HelpBox helpBox = container.Q<HelpBox>(NameHelpBox(property, index));
            MinValueAttribute minValueAttribute = (MinValueAttribute)saintsAttribute;
            (string error, float valueLimit) = GetLimitFloat(minValueAttribute, parent);

            if(helpBox.text != error)
            {
                helpBox.style.display = error == "" ? DisplayStyle.None : DisplayStyle.Flex;
                helpBox.text = error;
            }

            if (error != "")
            {
                return;
            }

            if(property.propertyType == SerializedPropertyType.Float && property.floatValue < valueLimit)
            {
                property.floatValue = valueLimit;
                property.serializedObject.ApplyModifiedProperties();
                onValueChangedCallback.Invoke(valueLimit);
            }
            else if(property.propertyType == SerializedPropertyType.Integer && property.intValue < (int)valueLimit)
            {
                property.intValue = (int)valueLimit;
                property.serializedObject.ApplyModifiedProperties();
                onValueChangedCallback.Invoke((int)valueLimit);
            }
        }

        #endregion

#endif
    }
}
