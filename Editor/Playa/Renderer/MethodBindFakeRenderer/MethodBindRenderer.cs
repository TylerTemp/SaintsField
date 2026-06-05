using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.AutoRunner;
using SaintsField.Editor.Core;
using SaintsField.Editor.Playa.Renderer.BaseRenderer;
using SaintsField.Editor.Utils;
using SaintsField.Playa;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.Events;
using Object = UnityEngine.Object;

namespace SaintsField.Editor.Playa.Renderer.MethodBindFakeRenderer
{
    public partial class MethodBindRenderer: AbsRenderer
    {
        protected override bool AllowGuiColor => false;
        private readonly SerializedObject _serializedObject;

        private readonly IPlayaMethodBindAttribute _methodBindAttribute;

        public MethodBindRenderer(IPlayaMethodBindAttribute methodBindAttribute, SerializedObject serializedObject, SaintsFieldWithInfo fieldWithInfo) : base(serializedObject, fieldWithInfo)
        {
            _serializedObject = serializedObject;
            _methodBindAttribute = methodBindAttribute;
        }

        private (string error, Action fixer) CheckMethodBind(IPlayaMethodBindAttribute playaMethodBindAttribute, SaintsFieldWithInfo fieldWithInfo)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return (string.Empty, null);
            }

            Object targetObj;
            try
            {
                targetObj = _serializedObject.targetObject;
            }
            catch (ArgumentNullException)
            {
                return (string.Empty, null);
            }
            catch (NullReferenceException)
            {
                return (string.Empty, null);
            }

            return CheckMethodBindInternal(playaMethodBindAttribute, fieldWithInfo.MethodInfo, targetObj, fieldWithInfo.Targets[0]);
        }

        private static (string error, Action fixer) CheckMethodBindInternal(IPlayaMethodBindAttribute playaMethodBindAttribute, MethodInfo methodInfo, Object serializedTarget, object target)
        {
            ParameterInfo[] methodParams = methodInfo.GetParameters();

            MethodBind methodBind = playaMethodBindAttribute.MethodBind;
            string eventTarget = playaMethodBindAttribute.EventTarget;
            // object value = playaMethodBindAttribute.Value;

            string eventDisplayName;
            if (methodBind == MethodBind.ButtonOnClick)
            {
                eventDisplayName = $"{eventTarget ?? "Button"}.onClick";
            }
            else  // custom event at the moment
            {
                eventDisplayName = eventTarget;
            }

            (string findExistingError, bool foundExists, Object foundContainerObject, UnityEventBase foundEventBase, object expectedValue, UnityEventCallState _, bool foundHasValue, Type foundValueType, object foundExistingValue) =
                FindAlreadyAddedCallback(playaMethodBindAttribute, methodInfo, serializedTarget, target);
            object value = expectedValue;
            if (findExistingError != "")
            {
                return (findExistingError, null);
            }

            if (foundExists)
            {
                return ("", null);
            }

            if (foundContainerObject == null || foundEventBase == null)
            {
                return ("Event not found", null);
            }

//             if (methodParams.Length == 0 || foundHasValue)
//             {
// #if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_EDITOR_METHOD_BIND_RENDERER
//                 Debug.Log(methodParams.Length);
//                 Debug.Log(foundHasValue);
//                 Debug.Log(
//                     foundHasValue
//                         ? $"`{methodInfo.Name}` already added to `{foundEventBase}` on `{foundContainerObject}` with value type `{foundValueType}` and value `{foundExistingValue}`"
//                         : $"`{methodInfo.Name}` already added to `{foundEventBase}`");
// #endif
//                 return ("", null);
//             }

            List<Type> invokeRequiredTypes = new List<Type>();
            Type unityEventType = foundEventBase.GetType();
            if (unityEventType.IsGenericType)
            {
                invokeRequiredTypes.AddRange(unityEventType.GetGenericArguments());
            }

            // UnityAction action = (UnityAction) Delegate.CreateDelegate(typeof(UnityAction), target, methodInfo);

#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_EDITOR_METHOD_BIND_RENDERER
            Debug.Log($"add `{methodInfo.Name}` to `{foundEventBase}` event on target {foundContainerObject}");
#endif

            // Undo.RecordObject(unityEventBase, "AddOnClick");
            if (methodParams.Length == 0)
            {
                return ("", () =>
                {
                    Undo.RecordObject(foundContainerObject, "AddEventListener");
                    EditorUtility.SetDirty(foundContainerObject);
                    UnityEventTools.AddVoidPersistentListener(
                        foundEventBase,
                        (UnityAction)Delegate.CreateDelegate(typeof(UnityAction),
                            target, methodInfo));
                    SaintsPropertyDrawer.EnqueueSceneViewNotification(
                        $"Bind callback `{methodInfo.Name}` to `{foundContainerObject}.{eventDisplayName}`");
#if UNITY_2021_3_OR_NEWER
                    AssetDatabase.SaveAssetIfDirty(foundContainerObject);
#endif
                });
            }

            return ("", () =>
            {
                Undo.RecordObject(foundContainerObject, "AddEventListener");
                EditorUtility.SetDirty(foundContainerObject);
                Util.BindEventWithValue(foundEventBase, methodInfo, invokeRequiredTypes.ToArray(), target, value);
                SaintsPropertyDrawer.EnqueueSceneViewNotification(
                    $"Bind callback `{methodInfo.Name}` to `{foundContainerObject}.{eventDisplayName}`({value})");
#if UNITY_2021_3_OR_NEWER
                    AssetDatabase.SaveAssetIfDirty(foundContainerObject);
#endif
            });
        }

        private static (string error, bool foundExists, Object unityEventContainerObject, UnityEventBase unityEventBase, object expectedValue, UnityEventCallState unityEventCallState, bool hasValue, Type valueType, object value)
            FindAlreadyAddedCallback(IPlayaMethodBindAttribute playaMethodBindAttribute, MethodInfo methodInfo, Object serializedTarget, object target)
        {
            MethodBind methodBind = playaMethodBindAttribute.MethodBind;
            string eventTarget = playaMethodBindAttribute.EventTarget;
            object expectedValue = playaMethodBindAttribute.Value;

            if (playaMethodBindAttribute.IsCallback)
            {
                (string callbackError, object callbackValue) = Util.GetOfNoParams<object>(target, (string)expectedValue, null);
                if (callbackError != "")
                {
                    return (callbackError, false, null, null, null, default, false, null, null);
                }

                expectedValue = callbackValue;
            }

            Object unityEventContainerObject;
            UnityEventBase unityEventBase = null;
            UnityEventCallState unityEventCallState = default;
            if (methodBind == MethodBind.ButtonOnClick)
            {
                UnityEngine.UI.Button uiButton = eventTarget is null
                    ? TryFindButton(target)
                    : GetButton(eventTarget, target);
                if (!uiButton)
                {
                    return ($"{methodInfo.Name}: Button not found", false, null, null, null, default, false, null, null);
                }

                unityEventContainerObject = uiButton;
                unityEventBase = uiButton.onClick;
            }
            else
            {
                if (string.IsNullOrEmpty(eventTarget))
                {
                    return ($"{methodInfo.Name}: Event target is empty", false, null, null, null, default, false, null, null);
                }

                List<string> attrNames = new List<string>();
                if (eventTarget.Contains("."))
                {
                    attrNames.AddRange(eventTarget.Split(SerializedUtils.DotSplitSeparator));
                }
                else
                {
                    attrNames.Add(eventTarget);
                }

                object accTarget = target;
                unityEventContainerObject = serializedTarget;

                while (attrNames.Count > 0)
                {
                    string searchAttr = attrNames[0];
                    attrNames.RemoveAt(0);
                    if (attrNames.Count == 0)
                    {
                        (string error, UnityEventBase foundUnityEvent) =
                            Util.GetOfNoParams<UnityEventBase>(accTarget, searchAttr, null);
                        if (error != "")
                        {
                            return (error, false, null, null, null, default, false, null, null);
                        }

                        unityEventBase = foundUnityEvent;
                    }
                    else
                    {
                        (string error, object foundTarget) =
                            Util.GetOfNoParams<object>(accTarget, searchAttr, null);
                        if (error != "")
                        {
                            return (error, false, null, null, null, default, false, null, null);
                        }

                        if (foundTarget == null)
                        {
                            return ($"{methodInfo.Name}: {searchAttr} is null", false, null, null, null, default, false, null, null);
                        }

                        accTarget = foundTarget;
                        if (foundTarget is Object foundUObject)
                        {
                            unityEventContainerObject = foundUObject;
                        }
                    }
                }
            }

            if (unityEventBase == null)
            {
                return ($"{methodInfo.Name}: Event not found", false, unityEventContainerObject, null, expectedValue, default, false, null, null);
            }

            bool needsValue = methodInfo.GetParameters().Length > 0;
            for (int eventIndex = 0; eventIndex < unityEventBase.GetPersistentEventCount(); eventIndex++)
            {
                Object persistentTarget = unityEventBase.GetPersistentTarget(eventIndex);
                string persistentMethodName = unityEventBase.GetPersistentMethodName(eventIndex);
                UnityEventCallState callStatus = unityEventBase.GetPersistentListenerState(eventIndex);
                if (!ReferenceEquals(persistentTarget, target) || persistentMethodName != methodInfo.Name)
                {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_EDITOR_METHOD_BIND_RENDERER
                    Debug.Log($"skip {persistentTarget}->{target}, {persistentMethodName} -> {methodInfo.Name}");
#endif
                    continue;
                }

                if (!needsValue)
                {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_EDITOR_METHOD_BIND_RENDERER
                    Debug.Log("found persistent with no value");
#endif
                    return ("", true, unityEventContainerObject, unityEventBase, expectedValue, callStatus, false, null, null);
                }

                (string valueError, bool hasPersistentValue, object persistentValue) =
                    PersistentListenerValueEquals(unityEventBase, eventIndex);
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_EDITOR_METHOD_BIND_RENDERER
                Debug.Log($"hasPersistentValue={hasPersistentValue},  persistentValue={persistentValue}, expectedValue={expectedValue}");
#endif
                if (valueError != "")
                {
                    return (valueError, false, null, null, expectedValue, default, false, null, null);
                }

                if (hasPersistentValue && PersistentValueEquals(persistentValue, expectedValue))
                {
                    return ("", true, unityEventContainerObject, unityEventBase, expectedValue, callStatus, true, persistentValue?.GetType(), persistentValue);
                }

                if (!hasPersistentValue && persistentValue == null)
                {
                    return ("", true, unityEventContainerObject, unityEventBase, expectedValue, callStatus, false, null, null);
                }
            }

#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_EDITOR_METHOD_BIND_RENDERER
            Debug.Log($"found nothing from {unityEventBase.GetPersistentEventCount()} events");
#endif
            return ("", false, unityEventContainerObject, unityEventBase, expectedValue, unityEventCallState, false, null, null);
        }

        private static (string error, bool hasValue, object value) PersistentListenerValueEquals(UnityEventBase unityEventBase, int eventIndex)
        {
            (string metaError, PersistentListenerMode mode, object callArguments) = GetPersistentCallMetadata(unityEventBase, eventIndex);
            if (metaError != "")
            {
                return (metaError, false, null);
            }

            if (mode == PersistentListenerMode.Bool)
            {
                (string error, object fieldValue) = GetFieldValue(callArguments, callArguments.GetType(), "m_BoolArgument");
                return (error, error == "" , fieldValue);
            }
            if (mode == PersistentListenerMode.Int)
            {
                (string error, object fieldValue) = GetFieldValue(callArguments, callArguments.GetType(), "m_IntArgument");
                return (error, error == "", fieldValue);
            }
            if (mode == PersistentListenerMode.Float)
            {
                (string error, object fieldValue) = GetFieldValue(callArguments, callArguments.GetType(), "m_FloatArgument");
                return (error, error == "", fieldValue);
            }
            if (mode == PersistentListenerMode.String)
            {
                (string error, object fieldValue) = GetFieldValue(callArguments, callArguments.GetType(), "m_StringArgument");
                return (error, error == "", fieldValue);
            }
            if (mode == PersistentListenerMode.Object)
            {
                (string error, object fieldValue) = GetFieldValue(callArguments, callArguments.GetType(), "m_ObjectArgument");
                return (error, error == "", fieldValue);
            }

            if (mode == PersistentListenerMode.EventDefined || mode == PersistentListenerMode.Void)
            {
                return ("", false, null);
            }

            return ($"Unsupported persistent listener mode `{mode}`", false, null);
        }

        private static bool PersistentValueEquals(object persistentValue, object expectedValue)
        {
            if (persistentValue is float persistentFloat && expectedValue is float expectedFloat)
            {
                return Mathf.Approximately(persistentFloat, expectedFloat);
            }

            return Equals(persistentValue, expectedValue);
        }

        private static (string error, PersistentListenerMode mode, object callArguments) GetPersistentCallMetadata(UnityEventBase unityEventBase, int eventIndex)
        {
            (string persistentCallsFieldError, object persistentCallsFieldObj) =
                GetFieldValue(unityEventBase, typeof(UnityEventBase), "m_PersistentCalls");
            if (persistentCallsFieldError != "")
            {
                return (persistentCallsFieldError, default, null);
            }

            object persistentCalls = persistentCallsFieldObj;
            (string callsFieldError, object callsObj) = GetFieldValue(persistentCalls, persistentCalls.GetType(), "m_Calls");
            if (callsFieldError != "")
            {
                return (callsFieldError, default, null);
            }

            IList calls = callsObj as IList;
            if (calls == null || eventIndex < 0 || eventIndex >= calls.Count)
            {
                return ($"Persistent call index `{eventIndex}` is out of range", default, null);
            }

            object persistentCall = calls[eventIndex];
            if (persistentCall == null)
            {
                return ("Persistent call is null", default, null);
            }

            (string modeError, object modeObj) = GetFieldValue(persistentCall, persistentCall.GetType(), "m_Mode");
            if (modeError != "")
            {
                return (modeError, default, null);
            }
            if (!(modeObj is PersistentListenerMode mode))
            {
                return ("Persistent listener mode is invalid", default, null);
            }

            (string argumentsError, object arguments) = GetFieldValue(persistentCall, persistentCall.GetType(), "m_Arguments");
            if (argumentsError != "")
            {
                return (argumentsError, default, null);
            }

            return ("", mode, arguments);
        }

        private static (string error, object value) GetFieldValue(object instance, Type targetType, string fieldName)
        {
            if (instance == null)
            {
                return ($"failed to get {fieldName}, instance is null", null);
            }

            FieldInfo fieldInfo = targetType.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            if (fieldInfo == null)
            {
                return ($"failed to get {fieldName} from {targetType}", null);
            }

            return ("", fieldInfo.GetValue(instance));
        }

        public static AutoRunnerFixerResult AutoRunFix(IPlayaMethodBindAttribute playaMethodBindAttribute, MethodInfo methodInfo, Object serializedTarget, object target)
        {
            string error;
            Action fixer;
            try
            {
                (error, fixer) =
                    CheckMethodBindInternal(playaMethodBindAttribute, methodInfo, serializedTarget, target);
            }
            catch (Exception e)
            {
                return new AutoRunnerFixerResult
                {
                    CanFix = false,
                    Error = "",
                    ExecError = e.ToString(),
                };
            }

            if (error != "")
            {
                return new AutoRunnerFixerResult
                {
                    CanFix = false,
                    Error = error,
                    ExecError = "",
                };
            }

            if (fixer == null)
            {
                return null;
            }

            return new AutoRunnerFixerResult
            {
                CanFix = true,
                Callback = fixer,
                Error = $"Method {methodInfo.Name} not bind to target",
                ExecError = "",
            };
        }

        private static UnityEngine.UI.Button GetButton(string by, object target)
        {
            if (by == null)
            {
                return TryFindButton(target);
            }

            (string error, MemberInfo _, object value) = Util.GetOf<object>(
                by,
                null,
                null,
                null,
                target,
                null);
            // Debug.Log($"find {value} path {by} on {target}");
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


        private (string error, Action fixer) RefreshCheckMethodBind()
        {
            return CheckMethodBind(_methodBindAttribute, FieldWithInfo);
        }

        public override void OnSearchField(string searchString)
        {
        }

        public override string ToString()
        {
            return $"<MethodBind {FieldWithInfo.RenderType} {FieldWithInfo.MethodInfo?.Name}/>";
        }

        private IEnumerator _imGuiEnumerator;
    }
}
