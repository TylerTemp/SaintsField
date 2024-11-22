using System;
using System.Collections.Generic;
using System.Linq;
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
    [CustomPropertyDrawer(typeof(OnValueChangedAttribute))]
    public class OnValueChangedAttributeDrawer : SaintsPropertyDrawer
    {
        #region IMGUI

        private string _error = "";

        // protected override bool DrawPostFieldImGui(Rect position, SerializedProperty property, GUIContent label,
        //     ISaintsAttribute saintsAttribute, int index, OnGUIPayload onGUIPayload, FieldInfo info, object parent)
        // {
        //     // Debug.Log($"OnValueChangedAttributeDrawer={valueChanged}");
        //     if (!onGUIPayload)
        //     {
        //         return true;
        //     }
        //
        //     _error = InvokeCallback(saintsAttribute, parent);
        //
        //     return true;
        // }
        protected override void OnPropertyEndImGui(SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute, int saintsIndex, OnGUIPayload onGUIPayload, FieldInfo info, object parent)
        {
            if (!onGUIPayload.changed)
            {
                return;
            }

            int arrayIndex = SerializedUtils.PropertyPathIndex(property.propertyPath);

            _error = InvokeCallback(((OnValueChangedAttribute)saintsAttribute).Callback, onGUIPayload.newValue, arrayIndex, parent);
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

        private static string InvokeCallback(string callback, object newValue, int index, object parent)
        {
            // no, don't use this. We already have the value
            // (string error, object _) = Util.GetMethodOf<object>(callback, null, property, info, target);
            // return error != "" ? error : "";

            // object parent = SerializedUtils.GetFieldInfoAndDirectParent(property).parent;

            List<Type> types = ReflectUtils.GetSelfAndBaseTypes(parent);
            types.Reverse();

            const BindingFlags bindAttr = BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic |
                                          BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.FlattenHierarchy;

            foreach (Type type in types)
            {
                MethodInfo methodInfo = type.GetMethod(callback, bindAttr);
                if (methodInfo == null)
                {
                    continue;
                }

                object[] passParams = ReflectUtils.MethodParamsFill(methodInfo.GetParameters(), index == -1
                    ? new[]
                    {
                        newValue,
                    }
                    : new []
                    {
                        newValue,
                        index,
                    });

                try
                {
                    methodInfo.Invoke(parent, passParams);
                }
                catch (TargetInvocationException e)
                {
                    Debug.LogException(e);
                    Debug.Assert(e.InnerException != null);
                    return e.InnerException.Message;
                }
                catch (InvalidCastException e)
                {
                    Debug.LogException(e);
                    return e.Message;
                }
                catch (Exception e)
                {
                    // _error = e.Message;
                    Debug.LogException(e);
                    return e.Message;
                }

                return "";
            }

            return $"No field or method named `{callback}` found on `{parent}`";
        }

#if UNITY_2021_3_OR_NEWER

        #region UIToolkit

        private static string NameHelpBox(SerializedProperty property, int index) =>
            $"{property.propertyPath}_{index}__ONValueChanged_HelpBox";

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

        protected override void OnValueChanged(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
            VisualElement container,
            FieldInfo info,
            object parent,
            Action<object> onValueChangedCallback,
            object newValue)
        {
            // Debug.Log($"OK I got a new value {newValue}; {this}");
            string propPath = property.propertyPath;
            int propIndex = SerializedUtils.PropertyPathIndex(propPath);
            string error = InvokeCallback(((OnValueChangedAttribute)saintsAttribute).Callback, newValue, propIndex, parent);
            HelpBox helpBox = container.Q<HelpBox>(NameHelpBox(property, index));
            helpBox.text = error;
            helpBox.style.display = error == "" ? DisplayStyle.None : DisplayStyle.Flex;
        }

        #endregion

#endif
    }
}
