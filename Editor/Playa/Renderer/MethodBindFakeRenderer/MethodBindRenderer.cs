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
        private readonly SerializedObject _serializedObject;

        private readonly IPlayaMethodBindAttribute _methodBindAttribute;

        public MethodBindRenderer(IPlayaMethodBindAttribute methodBindAttribute, SerializedObject serializedObject, SaintsFieldWithInfo fieldWithInfo) : base(serializedObject, fieldWithInfo)
        {
            _serializedObject = serializedObject;
            _methodBindAttribute = methodBindAttribute;
        }

        private void CheckMethodBind(IPlayaMethodBindAttribute playaMethodBindAttribute, SaintsFieldWithInfo fieldWithInfo)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }

            Object targetObj;
            try
            {
                targetObj = _serializedObject.targetObject;
            }
            catch (ArgumentNullException)
            {
                return;
            }

            CheckMethodBindInternal(playaMethodBindAttribute, fieldWithInfo.MethodInfo, targetObj, fieldWithInfo.Targets[0]).fixer?.Invoke();
        }

        private static (string error, Action fixer) CheckMethodBindInternal(IPlayaMethodBindAttribute playaMethodBindAttribute, MethodInfo methodInfo, UnityEngine.Object serializedTarget, object target)
        {
            ParameterInfo[] methodParams = methodInfo.GetParameters();

            MethodBind methodBind = playaMethodBindAttribute.MethodBind;
            string eventTarget = playaMethodBindAttribute.EventTarget;
            object value = playaMethodBindAttribute.Value;

            UnityEventBase unityEventBase = null;
            UnityEngine.Object unityEventContainerObject;
            List<Type> invokeRequiredTypes = new List<Type>();
            string eventDisplayName;
            if (methodBind == MethodBind.ButtonOnClick)
            {
                UnityEngine.UI.Button uiButton = eventTarget is null
                    ? TryFindButton(target)
                    : GetButton(eventTarget, target);

#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_EDITOR_METHOD_RENDERER
                Debug.Log($"find button `{uiButton}`");
#endif

                if (!uiButton)
                {
                    return ("Button not found", null);
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

                object accTarget = target;

                unityEventContainerObject = serializedTarget;

                while (attrNames.Count > 0)
                {
                    string searchAttr = attrNames[0];
                    attrNames.RemoveAt(0);
                    if (attrNames.Count == 0)
                    {
                        (string error, UnityEventBase foundValue) =
                            Util.GetOfNoParams<UnityEventBase>(accTarget, searchAttr, null);
                        // Debug.Log($"{searchAttr}, {foundValue}");
                        if (error != "")
                        {
                            return (error, null);
                        }

                        unityEventBase = foundValue;
                    }
                    else
                    {
                        (string error, object foundValue) =
                            Util.GetOfNoParams<object>(accTarget, searchAttr, null);
                        // Debug.Log($"{searchAttr}, {foundValue}");
                        if (error != "")
                        {
                            return (error, null);
                        }

                        if (foundValue == null)
                        {
                            return (error, null);
                        }

                        accTarget = foundValue;
                        if(foundValue is UnityEngine.Object foundUObject)
                        {
                            unityEventContainerObject = foundUObject;
                        }
                    }
                }

                if (unityEventBase == null)
                {
                    return ("Event not found", null);
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
                if (ReferenceEquals(persistentTarget, target) && persistentMethodName == methodInfo.Name)
                {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_EDITOR_METHOD_RENDERER
                    Debug.Log($"`{persistentMethodName}` already added to `{unityEventBase}`");
#endif
                    return ("", null);
                }
            }

            // UnityAction action = (UnityAction) Delegate.CreateDelegate(typeof(UnityAction), fieldWithInfo.Target, fieldWithInfo.MethodInfo);



#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_EDITOR_METHOD_RENDERER
            Debug.Log($"add `{fieldWithInfo.MethodInfo.Name}` to `{unityEventBase}` event on target {unityEventContainerObject}");
#endif

            // Undo.RecordObject(unityEventBase, "AddOnClick");
            if (methodParams.Length == 0)
            {
                return ("", () =>
                {
                    Undo.RecordObject(unityEventContainerObject, "AddEventListener");
                    EditorUtility.SetDirty(unityEventContainerObject);
                    UnityEventTools.AddVoidPersistentListener(
                        unityEventBase,
                        (UnityAction)Delegate.CreateDelegate(typeof(UnityAction),
                            target, methodInfo));
                    SaintsPropertyDrawer.EnqueueSceneViewNotification(
                        $"Bind callback `{methodInfo.Name}` to `{unityEventContainerObject}.{eventDisplayName}`");
#if UNITY_2021_3_OR_NEWER
                    AssetDatabase.SaveAssetIfDirty(unityEventContainerObject);
#endif
                });
            }

            if (playaMethodBindAttribute.IsCallback)
            {
                (string error, object foundValue) = Util.GetOfNoParams<object>(target, (string)value, null);

                if (error != "")
                {
                    return (error, null);
                }

                value = foundValue;
            }

            return ("", () =>
            {
                Undo.RecordObject(unityEventContainerObject, "AddEventListener");
                EditorUtility.SetDirty(unityEventContainerObject);
                Util.BindEventWithValue(unityEventBase, methodInfo, invokeRequiredTypes.ToArray(), target, value);
                SaintsPropertyDrawer.EnqueueSceneViewNotification(
                    $"Bind callback `{methodInfo.Name}` to `{unityEventContainerObject}.{eventDisplayName}`({value})");
#if UNITY_2021_3_OR_NEWER
                AssetDatabase.SaveAssetIfDirty(unityEventContainerObject);
#endif
            });
        }

        public static AutoRunnerFixerResult AutoRunFix(IPlayaMethodBindAttribute playaMethodBindAttribute, MethodInfo methodInfo, UnityEngine.Object serializedTarget, object target)
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


        private void OnApplicationChanged()
        {
            CheckMethodBind(_methodBindAttribute, FieldWithInfo);
        }

        public override void OnSearchField(string searchString)
        {
        }

        public override void OnDestroy()
        {
            SaintsEditorApplicationChanged.OnAnyEvent.RemoveListener(OnApplicationChanged);
        }

        public override string ToString()
        {
            return $"<MethodBind {FieldWithInfo.RenderType} {FieldWithInfo.MethodInfo?.Name}/>";
        }

        private IEnumerator _imGuiEnumerator;
    }
}
