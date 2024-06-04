using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
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

            UnityEventBase unityEventBase;
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
                (string error, UnityEventBase foundValue) = Util.GetOfNoParams<UnityEventBase>(fieldWithInfo.Target, eventTarget, null);
                if (error != "")
                {
                    return;
                }

                unityEventBase = foundValue;

                Type unityEventType = foundValue.GetType();
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

            switch (value)
            {
                case bool boolValue:
                    UnityEventTools.AddBoolPersistentListener(
                        unityEventBase,
                        (UnityAction<bool>)Delegate.CreateDelegate(typeof(UnityAction<bool>),
                            fieldWithInfo.Target, fieldWithInfo.MethodInfo),
                        boolValue);
                    return;
                case float floatValue:
                    UnityEventTools.AddFloatPersistentListener(
                        unityEventBase,
                        (UnityAction<float>)Delegate.CreateDelegate(typeof(UnityAction<float>),
                            fieldWithInfo.Target, fieldWithInfo.MethodInfo),
                        floatValue);
                    return;
                case int intValue:
                    UnityEventTools.AddIntPersistentListener(
                        unityEventBase,
                        (UnityAction<int>)Delegate.CreateDelegate(typeof(UnityAction<int>),
                            fieldWithInfo.Target, fieldWithInfo.MethodInfo),
                        intValue);
                    return;

                case string stringValue:
                    UnityEventTools.AddStringPersistentListener(
                        unityEventBase,
                        (UnityAction<string>)Delegate.CreateDelegate(typeof(UnityAction<string>),
                            fieldWithInfo.Target, fieldWithInfo.MethodInfo),
                        stringValue);
                    return;

                case UnityEngine.Object unityObjValue:
                    UnityEventTools.AddObjectPersistentListener(
                        unityEventBase,
                        (UnityAction<UnityEngine.Object>)Delegate.CreateDelegate(typeof(UnityAction<UnityEngine.Object>),
                            fieldWithInfo.Target, fieldWithInfo.MethodInfo),
                        unityObjValue);
                    return;

                default:
                {
                    Type[] invokeRequiredTypeArr = invokeRequiredTypes.ToArray();
                    // when method requires 1 parameter
                    // if value given, will go to the logic above, which is static parameter value
                    // otherwise, it's a method dynamic bind

                    // so, all logic here must be dynamic bind
                    Debug.Assert(methodParams.Length == invokeRequiredTypeArr.Length);

                    Type genericAction;

                    // ReSharper disable once ConvertSwitchStatementToSwitchExpression
                    switch (invokeRequiredTypeArr.Length)
                    {
                        case 0:
                            genericAction = typeof(UnityAction);
                            break;
                        case 1:
                            genericAction = typeof(UnityAction<>);
                            break;
                        case 2:
                            genericAction = typeof(UnityAction<,>);
                            break;
                        case 3:
                            genericAction = typeof(UnityAction<,,>);
                            break;
                        case 4:
                            genericAction = typeof(UnityAction<,,,>);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(invokeRequiredTypeArr.Length), invokeRequiredTypeArr.Length, null);
                    }

                    Type genericActionIns = genericAction.MakeGenericType(invokeRequiredTypeArr);
                    MethodInfo addPersistentListenerMethod = unityEventBase
                        .GetType()
                        .GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
                        .First(each => each.Name == "AddPersistentListener" && each.GetParameters().Length == 1);
                    Delegate callback = Delegate.CreateDelegate(genericActionIns, fieldWithInfo.Target,
                        fieldWithInfo.MethodInfo);
                    addPersistentListenerMethod.Invoke(unityEventBase, new object[]
                    {
                        callback,
                    });
                }
                    return;
            }
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
                    object defaultValue = null;
                    if (parameterInfo.IsOptional)
                    {
                        defaultValue = parameterInfo.DefaultValue;
                    }

                    // ReSharper disable once ConvertIfStatementToReturnStatement
                    if(parameterInfo.ParameterType.IsValueType)
                    {
                        defaultValue = Activator.CreateInstance(parameterInfo.ParameterType);
                    }

                    VisualElement element = UIToolkitLayout(defaultValue, parameterInfo.Name, parameterInfo.ParameterType);
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
        public override void OnDestroy()
        {
        }

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

                if (GUILayout.Button(buttonText, new GUIStyle(GUI.skin.button) { richText = true },
                        GUILayout.ExpandWidth(true)))
                {
                    object[] defaultParams = methodInfo.GetParameters().Select(p => p.DefaultValue).ToArray();
                    methodInfo.Invoke(target, defaultParams);
                }
            }
        }

        public override float GetHeight()
        {
            if(!FieldWithInfo.PlayaAttributes.OfType<ButtonAttribute>().Any())
            {
                return 0;
            }

            PreCheckResult preCheckResult = GetPreCheckResult(FieldWithInfo);
            if (!preCheckResult.IsShown)
            {
                return 0;
            }

            return SaintsPropertyDrawer.SingleLineHeight;
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

            using (new EditorGUI.DisabledScope(preCheckResult.IsDisabled))
            {
                string buttonText = string.IsNullOrEmpty(buttonAttribute.Label)
                    ? ObjectNames.NicifyVariableName(methodInfo.Name)
                    : buttonAttribute.Label;

                // ReSharper disable once InvertIf
                if (GUI.Button(position, buttonText, new GUIStyle(GUI.skin.button) { richText = true }))
                {
                    object[] defaultParams = methodInfo.GetParameters().Select(p => p.DefaultValue).ToArray();
                    methodInfo.Invoke(target, defaultParams);
                }
            }
        }
    }
}
