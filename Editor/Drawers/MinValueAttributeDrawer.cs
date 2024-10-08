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
            ISaintsAttribute saintsAttribute, int index, OnGUIPayload onGUIPayload, FieldInfo info, object parent)
        {
            if (!onGUIPayload.changed)
            {
                return true;
            }

            // object parentTarget = GetParentTarget(property);

            MinValueAttribute minValueAttribute = (MinValueAttribute)saintsAttribute;
            (string error, float valueLimit) = GetLimitFloat(property, minValueAttribute, info, parent);

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
                    onGUIPayload.SetValue(valueLimit);
                    if(ExpandableIMGUIScoop.IsInScoop)
                    {
                        property.serializedObject.ApplyModifiedProperties();
                    }
                }
            }
            else if (property.propertyType == SerializedPropertyType.Integer)
            {
                int curValue = property.intValue;

                if (valueLimit > curValue)
                {
                    property.intValue = (int)valueLimit;
                    onGUIPayload.SetValue((int)valueLimit);
                    if(ExpandableIMGUIScoop.IsInScoop)
                    {
                        property.serializedObject.ApplyModifiedProperties();
                    }
                }
            }
            return true;
        }

        protected override bool WillDrawBelow(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            FieldInfo info,
            object parent) => _error != "";

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width,
            ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent) => _error == "" ? 0 : ImGuiHelpBox.GetHeight(_error, width, MessageType.Error);

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent) => _error == "" ? position : ImGuiHelpBox.Draw(position, _error, MessageType.Error);
        #endregion

        private static (string error, float valueLimit) GetLimitFloat(SerializedProperty property, MinValueAttribute minValueAttribute, FieldInfo info, object parentTarget)
        {
            return minValueAttribute.ValueCallback == null
                ? ("", minValueAttribute.Value)
                : Util.GetOf(minValueAttribute.ValueCallback, 0f, property, info, parentTarget);
        }

#if UNITY_2021_3_OR_NEWER

        #region UIToolkit

        private static string NameHelpBox(SerializedProperty property, int index) =>
            $"{property.propertyPath}_{index}__MinValue_HelpBox";

        protected override VisualElement CreateBelowUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index,
            VisualElement container, FieldInfo info, object parent)
        {
            HelpBox helpBox = new HelpBox("", HelpBoxMessageType.Error)
            {
                name = NameHelpBox(property, index),
                style =
                {
                    display = DisplayStyle.None,
                },
            };
            helpBox.AddToClassList(ClassAllowDisable);
            return helpBox;
        }

        protected override void OnUpdateUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            VisualElement container, Action<object> onValueChangedCallback, FieldInfo info)
        {
            HelpBox helpBox = container.Q<HelpBox>(NameHelpBox(property, index));
            MinValueAttribute minValueAttribute = (MinValueAttribute)saintsAttribute;

            object parent = SerializedUtils.GetFieldInfoAndDirectParent(property).parent;
            (string error, float valueLimit) = GetLimitFloat(property, minValueAttribute, info, parent);

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
