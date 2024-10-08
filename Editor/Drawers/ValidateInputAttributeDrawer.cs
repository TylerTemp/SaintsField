using System;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;
#if UNITY_2021_3_OR_NEWER
using UnityEngine.UIElements;
#endif

namespace SaintsField.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(ValidateInputAttribute))]
    public class ValidateInputAttributeDrawer : SaintsPropertyDrawer
    {
        #region IMGUI
        private string _error = "";

        // ensure first time render will check the value
        // private bool _againRender;

        protected override bool DrawPostFieldImGui(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, int index, OnGUIPayload onGUIPayload, FieldInfo info, object parent)
        {
            // if (!valueChanged)
            // {
            //     if(_againRender)
            //     {
            //         return true;
            //     }
            // }

            // _againRender = true;

            if(onGUIPayload.changed)
            {
                property.serializedObject.ApplyModifiedProperties();
            }
            // Debug.Log($"call on {property.intValue}");

            string callback = ((ValidateInputAttribute)saintsAttribute).Callback;
            string labelText = label.text;
#if SAINTSFIELD_NAUGHYTATTRIBUTES
            labelText = property.displayName;
#endif
            _error = CallValidateMethod(callback, labelText, property, info, parent);

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

#if UNITY_2021_3_OR_NEWER

        #region UIToolkit

        private static string NameHelpBox(SerializedProperty property, int index) => $"{property.propertyPath}_{index}__ValidateInput";

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
            VisualElement container, Action<object> onValueChanged, FieldInfo info)
        {
            object parent = SerializedUtils.GetFieldInfoAndDirectParent(property).parent;
            if (parent == null)
            {
                Debug.LogWarning($"{property.propertyPath} parent disposed unexpectedly.");
                return;
            }

            string callback = ((ValidateInputAttribute)saintsAttribute).Callback;

            string validateResult = CallValidateMethod(callback, property.displayName, property, info, parent);

            HelpBox helpBox = container.Q<HelpBox>(NameHelpBox(property, index));
            // ReSharper disable once InvertIf
            if(helpBox.text != validateResult)
            {
                helpBox.style.display = string.IsNullOrEmpty(validateResult) ? DisplayStyle.None : DisplayStyle.Flex;
                helpBox.text = validateResult;
            }
        }

        #endregion

#endif

        private static string CallValidateMethod(string callback, string label, SerializedProperty property, FieldInfo fieldInfo, object parent)
        {
            (string error, object validateResult) = Util.GetMethodOf<object>(callback, null, property, fieldInfo, parent);
            // Debug.Log($"parent {parent}, call {callback} get {validateResult}, error={error}");
            if(error != "")
            {
                return error;
            }

            // ReSharper disable once ConvertSwitchStatementToSwitchExpression
            switch (validateResult)
            {
                case bool boolValue:
                    return boolValue? "" : $"`{label}` is invalid";
                case string stringContent:
                    return stringContent;
                case null:
                    return "";
                default:
                    throw new ArgumentOutOfRangeException(nameof(validateResult), validateResult, null);
            }
        }
    }
}
