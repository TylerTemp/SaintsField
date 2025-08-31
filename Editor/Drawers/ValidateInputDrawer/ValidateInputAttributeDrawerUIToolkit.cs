#if UNITY_2021_3_OR_NEWER
using System;
using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using SaintsField.Utils;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.ValidateInputDrawer
{
    public partial class ValidateInputAttributeDrawer
    {
        private static string NameHelpBox(SerializedProperty property, int index) =>
            $"{property.propertyPath}_{index}__ValidateInput";

        protected override VisualElement CreateBelowUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index,
            IReadOnlyList<PropertyAttribute> allAttributes,
            VisualElement container, FieldInfo info, object parent)
        {
            HelpBox helpBox = new HelpBox("", HelpBoxMessageType.Error)
            {
                name = NameHelpBox(property, index),
                style =
                {
                    display = DisplayStyle.None,
                },
            };
            helpBox.AddToClassList(ClassAllowDisable);
            return helpBox;
        }

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
            IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container, Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            HelpBox helpBox = container.Q<HelpBox>(NameHelpBox(property, index));
            string callback = ((ValidateInputAttribute)saintsAttribute).Callback;
            CallValidate(helpBox, property, callback, info);

            helpBox.TrackSerializedObjectValue(property.serializedObject, _ => CallValidate(helpBox, property, callback, info));

            helpBox.RegisterCallback<AttachToPanelEvent>(_ => {
                SaintsAssetPostprocessor.OnAnyEvent.AddListener(OnUnityEventCallback);
                SaintsEditorApplicationChanged.OnAnyEvent.AddListener(OnUnityEventCallback);
            });
            helpBox.RegisterCallback<DetachFromPanelEvent>(_ =>
            {
                SaintsAssetPostprocessor.OnAnyEvent.RemoveListener(OnUnityEventCallback);
                SaintsEditorApplicationChanged.OnAnyEvent.RemoveListener(OnUnityEventCallback);
            });
            return;

            void OnUnityEventCallback()
            {
                CallValidate(helpBox, property, callback, info);
            }
        }

        protected override void OnUpdateUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            IReadOnlyList<PropertyAttribute> allAttributes,
            VisualElement container, Action<object> onValueChanged, FieldInfo info)
        {
            if (!SaintsFieldConfigUtil.GetValidateInputLoopCheckUIToolkit())
            {
                return;
            }

            string callback = ((ValidateInputAttribute)saintsAttribute).Callback;

            HelpBox helpBox = container.Q<HelpBox>(NameHelpBox(property, index));
            CallValidate(helpBox, property, callback, info);
        }

        private static void CallValidate(HelpBox helpBox, SerializedProperty property, string callback, MemberInfo info)
        {
            object parent = SerializedUtils.GetFieldInfoAndDirectParent(property).parent;
            if (parent == null)
            {
                Debug.LogWarning($"{property.propertyPath} parent disposed unexpectedly.");
                return;
            }

            string validateResult = CallValidateMethod(callback, property.displayName, property, info, parent);
            // Debug.Log($"call validate input {validateResult}");

            // ReSharper disable once InvertIf
            if (helpBox.text != validateResult)
            {
                helpBox.style.display = string.IsNullOrEmpty(validateResult) ? DisplayStyle.None : DisplayStyle.Flex;
                helpBox.text = validateResult;
            }
        }

        protected override void OnValueChanged(SerializedProperty property, ISaintsAttribute saintsAttribute, int index, VisualElement container,
            FieldInfo info, object parent, Action<object> onValueChangedCallback, object newValue)
        {
            HelpBox helpBox = container.Q<HelpBox>(NameHelpBox(property, index));
            CallValidate(helpBox, property, ((ValidateInputAttribute)saintsAttribute).Callback, info);
        }
    }
}
#endif
