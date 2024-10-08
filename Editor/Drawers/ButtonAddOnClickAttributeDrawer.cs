using System;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.Events;
#if UNITY_2021_3_OR_NEWER
using UnityEngine.UIElements;
#endif
using Button = UnityEngine.UI.Button;
using Object = UnityEngine.Object;

namespace SaintsField.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(ButtonAddOnClickAttribute))]
    public class ButtonAddOnClickAttributeDrawer: SaintsPropertyDrawer
    {
        private string _error = "";

        protected override float GetPostFieldWidth(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, int index, OnGUIPayload onGuiPayload, FieldInfo info, object parent) => 0;

        protected override bool DrawPostFieldImGui(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute,
            int index,
            OnGUIPayload onGUIPayload, FieldInfo info, object parent)
        {
            _error = BindButtonEvent(property, saintsAttribute, info, parent);
            return true;
        }

        private static string BindButtonEvent(SerializedProperty property, ISaintsAttribute saintsAttribute, FieldInfo info, object objTarget)
        {
            ButtonAddOnClickAttribute buttonAddOnClickAttribute = (ButtonAddOnClickAttribute) saintsAttribute;

            string funcName = buttonAddOnClickAttribute.FuncName;
            string buttonComp = buttonAddOnClickAttribute.ButtonComp;

            #region Button

            Button uiButton = null;
            if (string.IsNullOrEmpty(buttonComp))
            {
                // search current field
                if (property.propertyType == SerializedPropertyType.ObjectReference)
                {
                    uiButton = GetUiButton(property.objectReferenceValue);
                }

                if(uiButton is null)  // search serialized target
                {
                    uiButton = GetUiButton(property.serializedObject.targetObject);

                    if (uiButton is null)
                    {
                        return "Parent target is not GameObject or Component";
                    }
                }
            }
            else  // has name, try find it
            {

                (string error, Object button) = Util.GetOf<Object>(buttonComp, null, property, info, objTarget);
                if(error != "")
                {
                    return error;
                }
                uiButton = GetUiButton(button);
            }

            if (uiButton == null)
            {
                return "Can not find Button";
            }

            #endregion

            #region Method

            if (objTarget == null)
            {
                return "Can not find parent target";
            }

            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (int eventIndex in Enumerable.Range(0, uiButton.onClick.GetPersistentEventCount()))
            {
                Object persistentTarget = uiButton.onClick.GetPersistentTarget(eventIndex);
                string persistentMethodName = uiButton.onClick.GetPersistentMethodName(eventIndex);
                if (ReferenceEquals(persistentTarget, objTarget) && persistentMethodName == funcName)
                {
                    // _error = $"`{funcName}` already added to `{uiButton}`";
                    // already there
                    // Debug.Log($"`{funcName}` already added to `{uiButton}`");
                    return "";
                }
            }
            #endregion

            MethodInfo methodInfo = objTarget.GetType().GetMethod(funcName, BindingFlags.Instance | BindingFlags.Public);
            if (methodInfo == null)
            {
                return $"Can not find method `{funcName}` in `{objTarget.GetType()}`";
            }

            if (methodInfo.GetParameters().Length == 0)
            {
                UnityEventTools.AddVoidPersistentListener(
                    uiButton.onClick,
                    (UnityAction)Delegate.CreateDelegate(typeof(UnityAction),
                        objTarget, methodInfo));
            }

            object value = buttonAddOnClickAttribute.Value;
            if (buttonAddOnClickAttribute.IsCallback)
            {
                (string error, object foundValue) = Util.GetOf<object>((string) value, null, property, info, objTarget);

                if (error != "")
                {
                    return error;
                }

                value = foundValue;
            }

            Util.BindEventWithValue(uiButton.onClick, methodInfo, Array.Empty<Type>(), objTarget, value);
            // UnityAction action = (UnityAction) Delegate.CreateDelegate(typeof(UnityAction), objTarget, funcName);
            //
            // UnityEventTools.AddPersistentListener(uiButton.onClick, action);
            return "";
        }

        private static Button GetUiButton(object objTarget)
        {
            // ReSharper disable once ConvertSwitchStatementToSwitchExpression
            switch (objTarget)
            {
                case GameObject go:
                    return go.GetComponent<Button>();
                case Button btn:
                    return btn;
                case Component component:
                    return component.GetComponent<Button>();
                default:
                    // _error = "Parent target is not GameObject or Component";
                    return null;
            }
        }

        protected override bool WillDrawBelow(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent) => _error != "";

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width,
            ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent) => _error == ""? 0: ImGuiHelpBox.GetHeight(_error, width, EMessageType.Error);
        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent) => _error == ""? position: ImGuiHelpBox.Draw(position, _error, EMessageType.Error);

#if UNITY_2021_3_OR_NEWER
        #region UIToolkit

        private static string NameHelpBox(SerializedProperty property, int index) => $"{property.propertyPath}_{index}__ButtonAddOnClick";

        protected override VisualElement CreateBelowUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index,
            VisualElement container, FieldInfo info, object parent)
        {
            HelpBox helpBox = new HelpBox
            {
                text = "",
                messageType = HelpBoxMessageType.Error,
                style =
                {
                    display = DisplayStyle.None,
                },
                name = NameHelpBox(property, index),
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
                Debug.LogWarning($"{property.propertyPath} parent disposed unexpectedly");
                return;
            }

            string error = BindButtonEvent(property, saintsAttribute, info, parent);
            HelpBox helpBox = container.Q<HelpBox>(NameHelpBox(property, index));
            // ReSharper disable once InvertIf
            if (error != helpBox.text)
            {
                helpBox.style.display = error == ""? DisplayStyle.None: DisplayStyle.Flex;
                helpBox.text = error;
            }
        }

        #endregion
#endif
    }
}
