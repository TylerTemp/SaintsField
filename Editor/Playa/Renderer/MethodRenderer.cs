using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Playa.Renderer.BaseRenderer;
using SaintsField.Editor.Utils;
using SaintsField.Playa;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.Events;

namespace SaintsField.Editor.Playa.Renderer
{
    public partial class MethodRenderer: AbsRenderer
    {
        private readonly SerializedObject _serializedObject;

        public MethodRenderer(SerializedObject serializedObject, SaintsFieldWithInfo fieldWithInfo) : base(serializedObject, fieldWithInfo)
        {
            _serializedObject = serializedObject;
        }

        private void CheckMethodBind(IPlayaMethodBindAttribute playaMethodBindAttribute, SaintsFieldWithInfo fieldWithInfo)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }

            ParameterInfo[] methodParams = fieldWithInfo.MethodInfo.GetParameters();

            MethodBind methodBind = playaMethodBindAttribute.MethodBind;
            string eventTarget = playaMethodBindAttribute.EventTarget;
            object value = playaMethodBindAttribute.Value;

            UnityEventBase unityEventBase = null;
            UnityEngine.Object unityEventContainerObject = null;
            List<Type> invokeRequiredTypes = new List<Type>();
            string eventDisplayName;
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

                unityEventContainerObject = uiButton;
                unityEventBase = uiButton.onClick;
                eventDisplayName = $"{eventTarget ?? "Button"}.onClick";
            }
            else  // custom event at the moment
            {
                eventDisplayName = eventTarget;
                List<string> attrNames = new List<string>();
                if (eventTarget.Contains("."))
                {
                    attrNames.AddRange(eventTarget.Split(SerializedUtils.pathSplitSeparator));
                }
                else
                {
                    attrNames.Add(eventTarget);
                }

                object target = fieldWithInfo.Target;

                unityEventContainerObject = _serializedObject.targetObject;

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
                        if(foundValue is UnityEngine.Object foundUObject)
                        {
                            unityEventContainerObject = foundUObject;
                        }
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

            for (int eventIndex = 0; eventIndex < unityEventBase.GetPersistentEventCount(); eventIndex++)
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

            Undo.RecordObject(unityEventContainerObject, "AddEventListener");

#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_EDITOR_METHOD_RENDERER
            Debug.Log($"add `{fieldWithInfo.MethodInfo.Name}` to `{unityEventBase}` event on target {unityEventContainerObject}");
#endif

            // Undo.RecordObject(unityEventBase, "AddOnClick");
            if (methodParams.Length == 0)
            {
                UnityEventTools.AddVoidPersistentListener(
                    unityEventBase,
                    (UnityAction)Delegate.CreateDelegate(typeof(UnityAction),
                        fieldWithInfo.Target, fieldWithInfo.MethodInfo));
                EditorUtility.SetDirty(unityEventContainerObject);
                SaintsPropertyDrawer.EnqueueSceneViewNotification($"Bind callback `{fieldWithInfo.MethodInfo.Name}` to `{unityEventContainerObject}.{eventDisplayName}`");
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
            SaintsPropertyDrawer.EnqueueSceneViewNotification($"Bind callback `{fieldWithInfo.MethodInfo.Name}` to `{unityEventContainerObject}.{eventDisplayName}`({value})");
            EditorUtility.SetDirty(unityEventContainerObject);
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
            OnDestroyIMGUI();
        }

        public override string ToString()
        {
            return $"<{FieldWithInfo.RenderType} {FieldWithInfo.MethodInfo?.Name}/>";
        }

        private IEnumerator _imGuiEnumerator;
    }
}
