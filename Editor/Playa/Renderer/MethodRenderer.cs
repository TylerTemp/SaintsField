using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Linq;
using SaintsField.Editor.Utils;
using SaintsField.Playa;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.Events;
#if UNITY_2021_3_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE
using UnityEditor.UIElements;
using UnityEngine.UIElements;
#endif

namespace SaintsField.Editor.Playa.Renderer
{
    public class MethodRenderer: AbsRenderer
    {
        public MethodRenderer(SerializedObject serializedObject, SaintsFieldWithInfo fieldWithInfo, bool tryFixUIToolkit=false) : base(fieldWithInfo)
        {
            // Debug.Assert(FieldWithInfo.MethodInfo.GetParameters().All(p => p.IsOptional), $"{FieldWithInfo.MethodInfo.Name} has non-optional parameters");
        }

        private static void CheckMethodBind(IPlayaMethodBindAttribute playaMethodBindAttribute, SaintsFieldWithInfo fieldWithInfo)
        {
            ParameterInfo[] methodParams = fieldWithInfo.MethodInfo.GetParameters();

            MethodBind methodBind = playaMethodBindAttribute.MethodBind;
            string eventTarget = playaMethodBindAttribute.EventTarget;
            object value = playaMethodBindAttribute.Value;

            UnityEventBase unityEventBase = null;
            List<Type> invokeRequiredTypes = new List<Type>();
            if (methodBind == MethodBind.ButtonOnClick)
            {
                UnityEngine.UI.Button uiButton = eventTarget is null
                    ? TryFindButton(fieldWithInfo.Target)
                    : GetButton(eventTarget, fieldWithInfo.Target);

#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_EDITOR_METHOD_RENDERER
                Debug.Log($"find button `{uiButton}`");
#endif

                if (uiButton == null)
                {
                    return;
                }

                unityEventBase = uiButton.onClick;
            }
            else  // custom event at the moment
            {
                List<string> attrNames = new List<string>();
                if (eventTarget.Contains("."))
                {
                    attrNames.AddRange(eventTarget.Split('.'));
                }
                else
                {
                    attrNames.Add(eventTarget);
                }

                object target = fieldWithInfo.Target;
                while (attrNames.Count > 0)
                {
                    string searchAttr = attrNames[0];
                    attrNames.RemoveAt(0);
                    if (attrNames.Count == 0)
                    {
                        (string error, UnityEventBase foundValue) =
                            Util.GetOfNoParams<UnityEventBase>(target, searchAttr, null);
                        // Debug.Log($"{searchAttr}, {foundValue}");
                        if (error != "")
                        {
                            return;
                        }

                        unityEventBase = foundValue;
                    }
                    else
                    {
                        (string error, object foundValue) =
                            Util.GetOfNoParams<object>(target, searchAttr, null);
                        // Debug.Log($"{searchAttr}, {foundValue}");
                        if (error != "")
                        {
                            return;
                        }

                        if (foundValue == null)
                        {
                            return;
                        }

                        target = foundValue;
                    }
                }

                if (unityEventBase == null)
                {
                    return;
                }

                Type unityEventType = unityEventBase.GetType();
                if (unityEventType.IsGenericType)
                {
                    invokeRequiredTypes.AddRange(unityEventType.GetGenericArguments());
                }
            }

            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (int eventIndex in Enumerable.Range(0, unityEventBase.GetPersistentEventCount()))
            {
                UnityEngine.Object persistentTarget = unityEventBase.GetPersistentTarget(eventIndex);
                string persistentMethodName = unityEventBase.GetPersistentMethodName(eventIndex);
                if (ReferenceEquals(persistentTarget, fieldWithInfo.Target) && persistentMethodName == fieldWithInfo.MethodInfo.Name)
                {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_EDITOR_METHOD_RENDERER
                    Debug.Log($"`{persistentMethodName}` already added to `{unityEventBase}`");
#endif
                    return;
                }
            }

            // UnityAction action = (UnityAction) Delegate.CreateDelegate(typeof(UnityAction), fieldWithInfo.Target, fieldWithInfo.MethodInfo);

#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_EDITOR_METHOD_RENDERER
            Debug.Log($"add `{fieldWithInfo.MethodInfo.Name}` to `{unityEventBase}`.onClick");
#endif

            // Undo.RecordObject(unityEventBase, "AddOnClick");
            if (methodParams.Length == 0)
            {
                UnityEventTools.AddVoidPersistentListener(
                    unityEventBase,
                    (UnityAction)Delegate.CreateDelegate(typeof(UnityAction),
                        fieldWithInfo.Target, fieldWithInfo.MethodInfo));
                return;
            }

            if (playaMethodBindAttribute.IsCallback)
            {
                (string error, object foundValue) = Util.GetOfNoParams<object>(fieldWithInfo.Target, (string)value, null);

                if (error != "")
                {
                    return;
                }

                value = foundValue;
            }

            Util.BindEventWithValue(unityEventBase, fieldWithInfo.MethodInfo, invokeRequiredTypes.ToArray(), fieldWithInfo.Target, value);

            // switch (value)
            // {
            //     case bool boolValue:
            //         UnityEventTools.AddBoolPersistentListener(
            //             unityEventBase,
            //             (UnityAction<bool>)Delegate.CreateDelegate(typeof(UnityAction<bool>),
            //                 fieldWithInfo.Target, fieldWithInfo.MethodInfo),
            //             boolValue);
            //         return;
            //     case float floatValue:
            //         UnityEventTools.AddFloatPersistentListener(
            //             unityEventBase,
            //             (UnityAction<float>)Delegate.CreateDelegate(typeof(UnityAction<float>),
            //                 fieldWithInfo.Target, fieldWithInfo.MethodInfo),
            //             floatValue);
            //         return;
            //     case int intValue:
            //         UnityEventTools.AddIntPersistentListener(
            //             unityEventBase,
            //             (UnityAction<int>)Delegate.CreateDelegate(typeof(UnityAction<int>),
            //                 fieldWithInfo.Target, fieldWithInfo.MethodInfo),
            //             intValue);
            //         return;
            //
            //     case string stringValue:
            //         UnityEventTools.AddStringPersistentListener(
            //             unityEventBase,
            //             (UnityAction<string>)Delegate.CreateDelegate(typeof(UnityAction<string>),
            //                 fieldWithInfo.Target, fieldWithInfo.MethodInfo),
            //             stringValue);
            //         return;
            //
            //     case UnityEngine.Object unityObjValue:
            //         UnityEventTools.AddObjectPersistentListener(
            //             unityEventBase,
            //             (UnityAction<UnityEngine.Object>)Delegate.CreateDelegate(typeof(UnityAction<UnityEngine.Object>),
            //                 fieldWithInfo.Target, fieldWithInfo.MethodInfo),
            //             unityObjValue);
            //         return;
            //
            //     default:
            //     {
            //         Type[] invokeRequiredTypeArr = invokeRequiredTypes.ToArray();
            //         // when method requires 1 parameter
            //         // if value given, will go to the logic above, which is static parameter value
            //         // otherwise, it's a method dynamic bind
            //
            //         // so, all logic here must be dynamic bind
            //         Debug.Assert(methodParams.Length == invokeRequiredTypeArr.Length);
            //
            //         Type genericAction;
            //
            //         // ReSharper disable once ConvertSwitchStatementToSwitchExpression
            //         switch (invokeRequiredTypeArr.Length)
            //         {
            //             case 0:
            //                 genericAction = typeof(UnityAction);
            //                 break;
            //             case 1:
            //                 genericAction = typeof(UnityAction<>);
            //                 break;
            //             case 2:
            //                 genericAction = typeof(UnityAction<,>);
            //                 break;
            //             case 3:
            //                 genericAction = typeof(UnityAction<,,>);
            //                 break;
            //             case 4:
            //                 genericAction = typeof(UnityAction<,,,>);
            //                 break;
            //             default:
            //                 throw new ArgumentOutOfRangeException(nameof(invokeRequiredTypeArr.Length), invokeRequiredTypeArr.Length, null);
            //         }
            //
            //         Type genericActionIns = genericAction.MakeGenericType(invokeRequiredTypeArr);
            //         MethodInfo addPersistentListenerMethod = unityEventBase
            //             .GetType()
            //             .GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
            //             .First(each => each.Name == "AddPersistentListener" && each.GetParameters().Length == 1);
            //         Delegate callback = Delegate.CreateDelegate(genericActionIns, fieldWithInfo.Target,
            //             fieldWithInfo.MethodInfo);
            //         addPersistentListenerMethod.Invoke(unityEventBase, new object[]
            //         {
            //             callback,
            //         });
            //     }
            //         return;
            // }
            // UnityEventTools.AddPersistentListener(uiButton.onClick, action);
        }

        private static UnityEngine.UI.Button GetButton(string by, object target)
        {
            if (by == null)
            {
                return TryFindButton(target);
            }

            (string error, object value) = Util.GetOfNoParams<object>(target, by, null);
            return error != ""
                ? null
                : TryFindButton(value);
        }

        private static UnityEngine.UI.Button TryFindButton(object target)
        {
            // ReSharper disable once ConvertSwitchStatementToSwitchExpression
            switch (target)
            {
                case UnityEngine.UI.Button button:
                    return button;
                case GameObject gameObject:
                    return gameObject.GetComponent<UnityEngine.UI.Button>();
                case Component component:
                    return component.GetComponent<UnityEngine.UI.Button>();
                default:
                    return null;
            }
        }

        #region UI Toolkit

#if UNITY_2021_3_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE
        public override VisualElement CreateVisualElement()
        {
            object target = FieldWithInfo.Target;
            MethodInfo methodInfo = FieldWithInfo.MethodInfo;
            // Debug.Assert(methodInfo.GetParameters().All(p => p.IsOptional));

            ButtonAttribute buttonAttribute = null;
            List<IPlayaMethodBindAttribute> methodBindAttributes = new List<IPlayaMethodBindAttribute>();

            foreach (IPlayaAttribute playaAttribute in FieldWithInfo.PlayaAttributes)
            {
                if(playaAttribute is ButtonAttribute button)
                {
                    buttonAttribute = button;
                }
                else if(playaAttribute is IPlayaMethodBindAttribute methodBindAttribute)
                {
                    methodBindAttributes.Add(methodBindAttribute);
                }
            }

            foreach (IPlayaMethodBindAttribute playaMethodBindAttribute in methodBindAttributes)
            {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_EDITOR_METHOD_RENDERER
                Debug.Log($"button click {playaMethodBindAttribute}");
#endif
                CheckMethodBind(playaMethodBindAttribute, FieldWithInfo);
            }

            if (buttonAttribute == null)
            {
                return null;
            }
            // Debug.Assert(methodInfo.GetParameters().All(p => p.IsOptional));

            string buttonText = string.IsNullOrEmpty(buttonAttribute.Label) ? ObjectNames.NicifyVariableName(methodInfo.Name) : buttonAttribute.Label;
            // object[] defaultParams = methodInfo.GetParameters().Select(p => p.DefaultValue).ToArray();
            ParameterInfo[] parameters = methodInfo.GetParameters();
            bool hasParameters = parameters.Length > 0;
            List<VisualElement> parameterElements = new List<VisualElement>();
            VisualElement root = null;

            if (hasParameters)
            {
                root = new VisualElement
                {
                    style =
                    {
                        backgroundColor = new Color(64f/255, 64f/255, 64f/255, 1f),
                        borderTopWidth = 1,
                        borderLeftWidth = 1,
                        borderRightWidth = 1,
                        borderBottomWidth = 1,
                        borderLeftColor = EColor.MidnightAsh.GetColor(),
                        borderRightColor = EColor.MidnightAsh.GetColor(),
                        borderTopColor = EColor.MidnightAsh.GetColor(),
                        borderBottomColor = EColor.MidnightAsh.GetColor(),
                        borderTopLeftRadius = 3,
                        borderTopRightRadius = 3,
                        borderBottomLeftRadius = 3,
                        borderBottomRightRadius = 3,
                        marginTop = 1,
                        marginBottom = 1,
                        marginLeft = 3,
                        marginRight = 3,
                        paddingTop = 3,
                    },
                };

                foreach (ParameterInfo parameterInfo in parameters)
                {
                    VisualElement element = UIToolkitLayout(GetParameterDefaultValue(parameterInfo),
                        ObjectNames.NicifyVariableName(parameterInfo.Name), parameterInfo.ParameterType);
                    element.style.marginRight = 3;
                    element.SetEnabled(true);
                    parameterElements.Add(element);
                    root.Add(element);
                }

            }

            Button buttonElement = new Button(() =>
            {
                object[] paraValues = parameterElements.Select(each => each.GetType().GetProperty("value")!.GetValue(each)).ToArray();
                methodInfo.Invoke(target, paraValues);
            })
            {
                text = buttonText,
                enableRichText = true,
                style =
                {
                    flexGrow = 1,
                },
            };
            if (FieldWithInfo.PlayaAttributes.Count(each => each is PlayaShowIfAttribute || each is PlayaEnableIfAttribute ||
                                                            each is PlayaDisableIfAttribute) > 0)
            {
                buttonElement.RegisterCallback<AttachToPanelEvent>(_ => buttonElement.schedule.Execute(() => UIToolkitOnUpdate(FieldWithInfo, buttonElement, true)).Every(100));
            }

            if (!hasParameters)
            {
                return buttonElement;
            }
            buttonElement.style.marginTop = buttonElement.style.marginBottom = buttonElement.style.marginLeft = buttonElement.style.marginRight = 0;
            buttonElement.style.borderTopLeftRadius = buttonElement.style.borderTopRightRadius = 0;
            buttonElement.style.borderLeftWidth = buttonElement.style.borderRightWidth = buttonElement.style.borderBottomWidth = 0;
            root.Add(buttonElement);
            return root;
        }
#endif

        #endregion

        private static object GetParameterDefaultValue(ParameterInfo parameterInfo)
        {
            if (parameterInfo.IsOptional)
            {
                return parameterInfo.DefaultValue;
            }

            // ReSharper disable once ConvertIfStatementToReturnStatement
            if(parameterInfo.ParameterType.IsValueType)
            {
                return Activator.CreateInstance(parameterInfo.ParameterType);
            }

            return null;
        }

        public override void OnDestroy()
        {
        }

        private object[] _imGuiParameterValues;

        public override void Render()
        {
            object target = FieldWithInfo.Target;
            MethodInfo methodInfo = FieldWithInfo.MethodInfo;

            ButtonAttribute buttonAttribute = null;
            List<IPlayaMethodBindAttribute> methodBindAttributes = new List<IPlayaMethodBindAttribute>();

            foreach (IPlayaAttribute playaAttribute in FieldWithInfo.PlayaAttributes)
            {
                switch (playaAttribute)
                {
                    case ButtonAttribute button:
                        buttonAttribute = button;
                        break;
                    case IPlayaMethodBindAttribute methodBindAttribute:
                        methodBindAttributes.Add(methodBindAttribute);
                        break;
                }
            }

            foreach (IPlayaMethodBindAttribute playaMethodBindAttribute in methodBindAttributes)
            {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_EDITOR_METHOD_RENDERER
                Debug.Log($"button click {playaMethodBindAttribute}");
#endif
                CheckMethodBind(playaMethodBindAttribute, FieldWithInfo);
            }

            if (buttonAttribute == null)
            {
                return;
            }

            PreCheckResult preCheckResult = GetPreCheckResult(FieldWithInfo);
            if (!preCheckResult.IsShown)
            {
                return;
            }

            using (new EditorGUI.DisabledScope(preCheckResult.IsDisabled))
            {
                string buttonText = string.IsNullOrEmpty(buttonAttribute.Label)
                    ? ObjectNames.NicifyVariableName(methodInfo.Name)
                    : buttonAttribute.Label;

                ParameterInfo[] parameters = methodInfo.GetParameters();

                // ReSharper disable once ConvertIfStatementToNullCoalescingAssignment
                if (_imGuiParameterValues == null)
                {
                    _imGuiParameterValues = parameters.Select(GetParameterDefaultValue).ToArray();
                }

                if (parameters.Length > 0)
                {
                    GUILayout.BeginVertical(GUI.skin.box);
                }

                object[] invokeParams = parameters.Select((p, index) =>
                {
                    return _imGuiParameterValues[index] = FieldLayout(_imGuiParameterValues[index], ObjectNames.NicifyVariableName(p.Name), p.ParameterType, false);
                }).ToArray();

                if (GUILayout.Button(buttonText, new GUIStyle(GUI.skin.button) { richText = true },
                        GUILayout.ExpandWidth(true)))
                {
                    // object[] defaultParams = methodInfo.GetParameters().Select(p => p.DefaultValue).ToArray();
                    methodInfo.Invoke(target, invokeParams);
                }

                if (parameters.Length > 0)
                {
                    GUILayout.EndVertical();
                }
            }
        }

        private const float PaddingBox = 2f;

        public override float GetHeight()
        {
            ButtonAttribute buttonAttribute = FieldWithInfo.PlayaAttributes.OfType<ButtonAttribute>().FirstOrDefault();
            if(buttonAttribute == null)
            {
                return 0;
            }

            PreCheckResult preCheckResult = GetPreCheckResult(FieldWithInfo);
            if (!preCheckResult.IsShown)
            {
                return 0;
            }

            ParameterInfo[] parameters = FieldWithInfo.MethodInfo.GetParameters();

            return SaintsPropertyDrawer.SingleLineHeight
                   + parameters.Select(each => FieldHeight(each.ParameterType, each.Name)).Sum()
                   + (parameters.Length > 0? PaddingBox * 2: 0);
        }

        public override void RenderPosition(Rect position)
        {
            object target = FieldWithInfo.Target;
            MethodInfo methodInfo = FieldWithInfo.MethodInfo;

            ButtonAttribute buttonAttribute = null;
            List<IPlayaMethodBindAttribute> methodBindAttributes = new List<IPlayaMethodBindAttribute>();

            foreach (IPlayaAttribute playaAttribute in FieldWithInfo.PlayaAttributes)
            {
                switch (playaAttribute)
                {
                    case ButtonAttribute button:
                        buttonAttribute = button;
                        break;
                    case IPlayaMethodBindAttribute methodBindAttribute:
                        methodBindAttributes.Add(methodBindAttribute);
                        break;
                }
            }

            foreach (IPlayaMethodBindAttribute playaMethodBindAttribute in methodBindAttributes)
            {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_EDITOR_METHOD_RENDERER
                Debug.Log($"button click {playaMethodBindAttribute}");
#endif
                CheckMethodBind(playaMethodBindAttribute, FieldWithInfo);
            }

            PreCheckResult preCheckResult = GetPreCheckResult(FieldWithInfo);
            if (!preCheckResult.IsShown)
            {
                return;
            }
            if(buttonAttribute == null)
            {
                return;
            }

            ParameterInfo[] parameters = FieldWithInfo.MethodInfo.GetParameters();
            // ReSharper disable once ConvertIfStatementToNullCoalescingAssignment
            if (_imGuiParameterValues == null)
            {
                _imGuiParameterValues = parameters.Select(GetParameterDefaultValue).ToArray();
            }

            float yAcc = position.y + (parameters.Length > 0 ? PaddingBox : 0);

            // draw box
            if (parameters.Length > 0)
            {
                float[] heights = parameters.Select(each => FieldHeight(each.ParameterType, each.Name)).ToArray();

                Rect boxRect = new Rect(position)
                {
                    y = yAcc,
                    height = position.height - PaddingBox * 2,
                };
                GUI.Box(boxRect, GUIContent.none);

                foreach ((ParameterInfo parameterInfo, int index) in parameters.WithIndex())
                {
                    float height = heights[index];
                    Rect rect = new Rect(position)
                    {
                        y = yAcc,
                        x = position.x + PaddingBox,
                        width = position.width - 2 * PaddingBox,
                        height = height,
                    };
                    yAcc += height;

                    _imGuiParameterValues[index] = FieldPosition(rect, _imGuiParameterValues[index],
                        ObjectNames.NicifyVariableName(parameterInfo.Name), parameterInfo.ParameterType, false);
                    position.y += rect.height;
                }
            }

            using (new EditorGUI.DisabledScope(preCheckResult.IsDisabled))
            {
                string buttonText = string.IsNullOrEmpty(buttonAttribute.Label)
                    ? ObjectNames.NicifyVariableName(methodInfo.Name)
                    : buttonAttribute.Label;

                Rect buttonRect = new Rect(position)
                {
                    y = yAcc,
                    height = SaintsPropertyDrawer.SingleLineHeight,
                };

                // ReSharper disable once InvertIf
                if (GUI.Button(buttonRect, buttonText, new GUIStyle(GUI.skin.button) { richText = true }))
                {
                    // object[] defaultParams = methodInfo.GetParameters().Select(p => p.DefaultValue).ToArray();
                    methodInfo.Invoke(target, _imGuiParameterValues);
                }
            }
        }

        public override string ToString()
        {
            return $"<{FieldWithInfo.RenderType} {FieldWithInfo.MethodInfo?.Name}/>";
        }
    }
}
