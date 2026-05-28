#if UNITY_2021_3_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE
using System;
using System.Linq;
using SaintsField.Editor.Core;
using SaintsField.Editor.UIToolkitElements;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine.Events;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace SaintsField.Editor.Playa.Renderer.MethodBindFakeRenderer
{
    public partial class MethodBindRenderer
    {
        private class OnEventWithErrorElement : VisualElement
        {
            public readonly OnEventElement OnEventElement;
            private readonly HelpBox _helpBox;

            public OnEventWithErrorElement(string title, string targetEventName, string viewDataKey)
            {
                hierarchy.Add(OnEventElement = new OnEventElement(title, targetEventName)
                {
                    viewDataKey = viewDataKey,
                });
                hierarchy.Add(_helpBox = new HelpBox("", HelpBoxMessageType.Error)
                {
                    style =
                    {
                        display = DisplayStyle.None,
                    },
                });

                style.marginTop = 1;
                style.marginBottom = 1;
            }

            public bool SetError(string error)
            {
                bool hasError = !string.IsNullOrEmpty(error);
                UIToolkitUtils.SetHelpBox(_helpBox, error);
                DisplayStyle eventElementDisplay = hasError
                    ? DisplayStyle.None
                    : DisplayStyle.Flex;
                UIToolkitUtils.SetDisplayStyle(OnEventElement, eventElementDisplay);
                return hasError;
            }
        }

        private OnEventWithErrorElement _onEventWithErrorElement;

        protected override (VisualElement target, bool needUpdate) CreateTargetUIToolkit(VisualElement inspectorRoot,
            VisualElement container)
        {
            Object targetObj = _serializedObject.targetObject;
            (string error, bool _, Object __, UnityEventBase unityEventBase, object ___, UnityEventCallState ____, bool _____, Type ______, object _______) =
                FindAlreadyAddedCallback(_methodBindAttribute, FieldWithInfo.MethodInfo, targetObj, FieldWithInfo.Targets[0]);
            if (!string.IsNullOrEmpty(error))
            {
                return (new HelpBox(error, HelpBoxMessageType.Error), false);
            }

            string eventSuffix = "";
            if (unityEventBase is not UnityEvent)
            {
                Type type = unityEventBase.GetType();
                if (type.IsGenericType)
                {
                    Type[] genericTypes = type.GetGenericArguments();
                    if (genericTypes.Length > 0)
                    {
                        string[] typeNames = new string[genericTypes.Length];
                        for (int i = 0; i < genericTypes.Length; i++)
                        {
                            typeNames[i] = ReflectUtils.StringifyType(genericTypes[i]);
                        }

                        eventSuffix = $" ({string.Join(", ", typeNames)})";
                    }
                }
            }

            string targetEventName = $"{_methodBindAttribute.EventTarget.Split('.').Last()}{eventSuffix}";

            _onEventWithErrorElement = new OnEventWithErrorElement(ObjectNames.NicifyVariableName(FieldWithInfo.MethodInfo.Name), targetEventName, $"{FieldWithInfo.MemberId}[{targetEventName}]");
            _onEventWithErrorElement.RegisterCallback<AttachToPanelEvent>(_ =>
            {
                BindAndCheckElement();
                SaintsEditorApplicationChanged.OnAnyEvent.AddListener(BindAndCheckElement);
            });
            _onEventWithErrorElement.RegisterCallback<DetachFromPanelEvent>(_ =>
            {
                SaintsEditorApplicationChanged.OnAnyEvent.RemoveListener(BindAndCheckElement);
            });

            return (_onEventWithErrorElement, false);
        }

        private void RefreshElement()
        {
            Object targetObj = _serializedObject.targetObject;
            (string error, bool _, Object unityEventContainerObject, UnityEventBase _, object _, UnityEventCallState unityEventCallState, bool hasValue, Type valueType, object value) =
                FindAlreadyAddedCallback(_methodBindAttribute, FieldWithInfo.MethodInfo, targetObj, FieldWithInfo.Targets[0]);

            if(!_onEventWithErrorElement.SetError(error))
            {
                _onEventWithErrorElement.OnEventElement.Refresh(unityEventContainerObject, unityEventCallState,
                    hasValue, valueType, value);
            }
        }

        private void BindAndCheckElement()
        {
            (string error, Action fixer) = RefreshCheckMethodBind();
            // ReSharper disable once InvertIf
            if (!_onEventWithErrorElement.SetError(error))
            {
                fixer?.Invoke();
                RefreshElement();
            }
        }
    }
}
#endif
