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
            ISaintsAttribute saintsAttribute, FieldInfo info, object parent) => 0;

        protected override bool DrawPostFieldImGui(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute,
            int index,
            OnGUIPayload onGUIPayload, FieldInfo info, object parent)
        {
            _error = BindButtonEvent(property, saintsAttribute, parent);
            return true;
        }

        private static string BindButtonEvent(SerializedProperty property, ISaintsAttribute saintsAttribute, object objTarget)
        {
            ButtonAddOnClickAttribute buttonAddOnClickAttribute = (ButtonAddOnClickAttribute) saintsAttribute;

            string funcName = buttonAddOnClickAttribute.FuncName;
            string buttonComp = buttonAddOnClickAttribute.ButtonComp;

            #region Button

            Button uiButton = null;
            if (string.IsNullOrEmpty(buttonComp))
            {
                if (property.propertyType == SerializedPropertyType.ObjectReference)
                {
                    uiButton = GetUiButton(property.objectReferenceValue);
                }

                if(uiButton is null)
                {
                    if (objTarget == null)
                    {
                        return "Can not find parent target";
                    }

                    uiButton = GetUiButton(objTarget);

                    if (uiButton is null)
                    {
                        return "Parent target is not GameObject or Component";
                    }
                }
            }
            else
            {
                if (objTarget == null)
                {
                    return "Can not find parent target";
                }

                (ReflectUtils.GetPropType getPropType, object fieldOrMethodInfo) =
                    ReflectUtils.GetProp(objTarget.GetType(), buttonComp);
                switch (getPropType)
                {
                    case ReflectUtils.GetPropType.Field:
                        uiButton = GetUiButton(((FieldInfo)fieldOrMethodInfo).GetValue(objTarget));
                        break;

                    case ReflectUtils.GetPropType.Property:
                        uiButton = GetUiButton(((PropertyInfo)fieldOrMethodInfo).GetValue(objTarget));
                        break;

                    case ReflectUtils.GetPropType.Method:
                    {
                        MethodInfo methodInfo = (MethodInfo)fieldOrMethodInfo;
                        ParameterInfo[] methodParams = methodInfo.GetParameters();
                        Debug.Assert(methodParams.All(p => p.IsOptional));
                        if (methodInfo.ReturnType != typeof(Button))
                        {
                            return
                                $"Expect returning Button from `{buttonComp}`, get {methodInfo.ReturnType}";
                        }

                        try
                        {
                            uiButton = (Button)methodInfo.Invoke(objTarget,
                                methodParams.Select(p => p.DefaultValue).ToArray());
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
                        break;
                    case ReflectUtils.GetPropType.NotFound:
                    {
                        return $"not found `{buttonComp}` on `{objTarget}`";
                    }
                    default:
                        throw new ArgumentOutOfRangeException(nameof(getPropType), getPropType, null);
                }
            }

            if (uiButton == null)
            {
                return "Can not find Button";
            }

            #endregion

            // Debug.Log($"found button {uiButton}");

            #region Func

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

            // TODO: support arguments action
            UnityAction action = (UnityAction) Delegate.CreateDelegate(typeof(UnityAction), objTarget, funcName);

            UnityEventTools.AddPersistentListener(uiButton.onClick, action);
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
            ISaintsAttribute saintsAttribute, FieldInfo info, object parent) => _error != "";

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width,
            ISaintsAttribute saintsAttribute, FieldInfo info, object parent) => _error == ""? 0: ImGuiHelpBox.GetHeight(_error, width, EMessageType.Error);
        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, FieldInfo info, object parent) => _error == ""? position: ImGuiHelpBox.Draw(position, _error, EMessageType.Error);

#if UNITY_2021_3_OR_NEWER
        #region UIToolkit

        protected override VisualElement CreateAboveUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
            VisualElement container, object parent)
        {
            BindButtonEvent(property, saintsAttribute, parent);
            return null;
        }

        #endregion
#endif
    }
}
