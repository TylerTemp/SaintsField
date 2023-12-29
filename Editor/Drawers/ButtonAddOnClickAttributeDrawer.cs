using System;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace SaintsField.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(ButtonAddOnClickAttribute))]
    public class ButtonAddOnClickAttributeDrawer: SaintsPropertyDrawer
    {
        private string _error = "";

        protected override float GetPostFieldWidth(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute) => 0;

        protected override bool DrawPostField(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute,
            bool valueChanged)
        {
            _error = "";

            ButtonAddOnClickAttribute buttonAddOnClickAttribute = (ButtonAddOnClickAttribute) saintsAttribute;

            string funcName = buttonAddOnClickAttribute.FuncName;
            string buttonComp = buttonAddOnClickAttribute.ButtonComp;
            object objTarget = GetParentTarget(property);

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
                        _error = "Can not find parent target";
                        return false;
                    }

                    uiButton = GetUiButton(objTarget);

                    if (uiButton is null)
                    {
                        _error = "Parent target is not GameObject or Component";
                        return false;
                    }
                }
            }
            else
            {
                if (objTarget == null)
                {
                    _error = "Can not find parent target";
                    return false;
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
                            _error =
                                $"Expect returning Button from `{buttonComp}`, get {methodInfo.ReturnType}";
                            return false;
                        }

                        try
                        {
                            uiButton = (Button)methodInfo.Invoke(objTarget,
                                methodParams.Select(p => p.DefaultValue).ToArray());
                        }
                        catch (TargetInvocationException e)
                        {
                            Debug.Assert(e.InnerException != null);
                            _error = e.InnerException.Message;
                            Debug.LogException(e);
                            return false;
                        }
                        catch (Exception e)
                        {
                            _error = e.Message;
                            Debug.LogException(e);
                            return false;
                        }
                    }
                        break;
                    case ReflectUtils.GetPropType.NotFound:
                    {
                        _error =
                            $"not found `{buttonComp}` on `{objTarget}`";
                        return false;
                    }
                    default:
                        throw new ArgumentOutOfRangeException(nameof(getPropType), getPropType, null);
                }
            }

            if (uiButton == null)
            {
                if (_error == "")
                {
                    _error = "Can not find Button";
                }

                return false;
            }

            #endregion

            // Debug.Log($"found button {uiButton}");

            #region Func

            if (objTarget == null)
            {
                _error = "Can not find parent target";
                return false;
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
                    return false;
                }
            }
            #endregion

            // TODO: support arguments action
            UnityAction action = (UnityAction) Delegate.CreateDelegate(typeof(UnityAction), objTarget, funcName);

            UnityEventTools.AddPersistentListener(uiButton.onClick, action);

            return true;
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

        protected override bool WillDrawBelow(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute) => _error != "";

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width, ISaintsAttribute saintsAttribute) => _error == ""? 0: HelpBox.GetHeight(_error, width, EMessageType.Error);
        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute) => _error == ""? position: HelpBox.Draw(position, _error, EMessageType.Error);
    }
}
