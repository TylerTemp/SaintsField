using System;
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

            _error = InvokeCallback(saintsAttribute, onGUIPayload.newValue, arrayIndex, parent);
        }

        protected override bool WillDrawBelow(SerializedProperty property, ISaintsAttribute saintsAttribute,
            FieldInfo info,
            object parent) => _error != "";

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width,
            ISaintsAttribute saintsAttribute, FieldInfo info, object parent) => _error == "" ? 0 : ImGuiHelpBox.GetHeight(_error, width, MessageType.Error);

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, FieldInfo info, object parent) => _error == "" ? position : ImGuiHelpBox.Draw(position, _error, MessageType.Error);
        #endregion

        private static string InvokeCallback(ISaintsAttribute saintsAttribute, object newValue, int index, object target)
        {
            // Debug.Log(saintsAttribute);
            string callback = ((OnValueChangedAttribute)saintsAttribute).Callback;

            const BindingFlags bindAttr = BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic |
                                          BindingFlags.Public | BindingFlags.DeclaredOnly;
            MethodInfo methodInfo =  target.GetType().GetMethod(callback, bindAttr);
            if (methodInfo == null)
            {
                return $"No method found `{callback}` on `{target}`";
            }

            ParameterInfo[] methodParams = methodInfo.GetParameters();
            object[] paramValues = ReflectUtils.MethodParamsFill(methodParams,  index == -1? new[] { newValue }: new[] { newValue, index });
            try
            {
                methodInfo.Invoke(target, paramValues);
            }
            catch (TargetInvocationException e)
            {
                Debug.LogException(e);
                Debug.Assert(e.InnerException != null);
                return e.InnerException.Message;

            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return e.Message;
            }

            return "";
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
            object newValue)
        {
            // Debug.Log($"OK I got a new value {newValue}; {this}");
            string error = InvokeCallback(saintsAttribute, newValue, SerializedUtils.PropertyPathIndex(property.propertyPath), parent);
            HelpBox helpBox = container.Q<HelpBox>(NameHelpBox(property, index));
            helpBox.text = error;
            helpBox.style.display = error == "" ? DisplayStyle.None : DisplayStyle.Flex;
        }

        #endregion

#endif
    }
}
