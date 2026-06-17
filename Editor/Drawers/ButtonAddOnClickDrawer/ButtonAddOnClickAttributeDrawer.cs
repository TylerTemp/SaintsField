using System;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.Events;
using Button = UnityEngine.UI.Button;
using Object = UnityEngine.Object;

namespace SaintsField.Editor.Drawers.ButtonAddOnClickDrawer
{
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.Editor.DrawerPriority(Sirenix.OdinInspector.Editor.DrawerPriorityLevel.WrapperPriority)]
#endif
    [CustomPropertyDrawer(typeof(ButtonAddOnClickAttribute), true)]
    public partial class ButtonAddOnClickAttributeDrawer: SaintsPropertyDrawer
    {
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

                (string error, MemberInfo _, Object button) = Util.GetOf<Object>(buttonComp, null, property, info, objTarget, null);
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

            for (int eventIndex = 0; eventIndex < uiButton.onClick.GetPersistentEventCount(); eventIndex++)
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
                (string error, MemberInfo _, object foundValue) = Util.GetOf<object>((string) value, null, property, info, objTarget, null);

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
    }
}
