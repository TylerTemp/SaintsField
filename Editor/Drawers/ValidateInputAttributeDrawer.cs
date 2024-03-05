using System;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
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
            ISaintsAttribute saintsAttribute, int index, bool valueChanged, FieldInfo info, object parent)
        {
            // if (!valueChanged)
            // {
            //     if(_againRender)
            //     {
            //         return true;
            //     }
            // }

            // _againRender = true;

            if(valueChanged)
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
            FieldInfo info,
            object parent) => _error != "";

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width,
            ISaintsAttribute saintsAttribute, FieldInfo info, object parent) => _error == "" ? 0 : ImGuiHelpBox.GetHeight(_error, width, MessageType.Error);

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, FieldInfo info, object parent) => _error == "" ? position : ImGuiHelpBox.Draw(position, _error, MessageType.Error);
        #endregion

#if UNITY_2021_3_OR_NEWER

        #region UIToolkit

        private static string NameHelpBox(SerializedProperty property, int index) => $"{property.propertyPath}_{index}__ValidateInput";

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

        // protected override void OnValueChanged(SerializedProperty property, ISaintsAttribute saintsAttribute, int index, VisualElement container,
        //     object parent, object newValue)
        // {
        //     string callback = ((ValidateInputAttribute)saintsAttribute).Callback;
        //     string validateResult = CallValidateMethod(callback, property.displayName, parent);
        //
        //     HelpBox helpBox = container.Q<HelpBox>(NameHelpBox(property, index));
        //     // ReSharper disable once InvertIf
        //     if(helpBox.text != validateResult)
        //     {
        //         helpBox.style.display = string.IsNullOrEmpty(validateResult) ? DisplayStyle.None : DisplayStyle.Flex;
        //         helpBox.text = validateResult;
        //     }
        // }

        protected override void OnUpdateUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            VisualElement container, Action<object> onValueChanged, FieldInfo info, object parent)
        {
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
            (ReflectUtils.GetPropType getPropType, object fieldOrMethodInfo) found = ReflectUtils.GetProp(parent.GetType(), callback);

            if (found.getPropType == ReflectUtils.GetPropType.NotFound)
            {
                return $"No field or method named `{callback}` found on `{parent}`";
            }

            object validateResult;

            // ReSharper disable once ConvertIfStatementToSwitchStatement
            if (found.getPropType == ReflectUtils.GetPropType.Property && found.fieldOrMethodInfo is PropertyInfo propertyInfo)
            {
                validateResult = propertyInfo.GetValue(parent);
            }
            else if (found.getPropType == ReflectUtils.GetPropType.Field && found.fieldOrMethodInfo is FieldInfo foundFieldInfo)
            {
                validateResult = foundFieldInfo.GetValue(parent);
            }
            else if (found.getPropType == ReflectUtils.GetPropType.Method && found.fieldOrMethodInfo is MethodInfo methodInfo)
            {
                int arrayIndex = SerializedUtils.PropertyPathIndex(property.propertyPath);
                object rawValue = fieldInfo.GetValue(parent);
                object curValue = arrayIndex == -1 ? rawValue : SerializedUtils.GetValueAtIndex(rawValue, arrayIndex);

                object[] filledParams = ReflectUtils.MethodParamsFill(methodInfo.GetParameters(), arrayIndex == -1
                    ? new[]
                    {
                        curValue,
                    }
                    : new []
                    {
                        curValue,
                        arrayIndex,
                    });

#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_VALIDATE_INPUT
                Debug.Log($"#ValidateInput# {methodInfo.Name}({string.Join(", ", filledParams)})");
#endif

                try
                {
                    validateResult = methodInfo.Invoke(parent, filledParams);
                }
                catch (TargetInvocationException e)
                {
                    Debug.Assert(e.InnerException != null);
                    Debug.LogException(e);
                    return e.InnerException.Message;
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                    return e.Message;
                }
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(found), found, null);
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
