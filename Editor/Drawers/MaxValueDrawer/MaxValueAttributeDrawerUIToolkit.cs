#if UNITY_2021_3_OR_NEWER
using System;
using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.MaxValueDrawer
{
    public partial class MaxValueAttributeDrawer
    {

        private static string NameHelpBox(SerializedProperty property, int index) =>
            $"{property.propertyPath}_{index}__MaxValue_HelpBox";

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
            MaxValueAttribute maxValueAttribute = (MaxValueAttribute)saintsAttribute;

            Refresh();
            helpBox.TrackPropertyValue(property, _ => Refresh());
            SaintsEditorApplicationChanged.OnAnyEvent.AddListener(Refresh);
            helpBox.RegisterCallback<DetachFromPanelEvent>(_ =>
            {
                SaintsEditorApplicationChanged.OnAnyEvent.RemoveListener(Refresh);
                UIToolkitUtils.Unbind(helpBox);
            });
            return;

            void Refresh()
            {
                (IReadOnlyList<string> errors, IReadOnlyList<(string message, Action fix)> checkerResults) = CheckPropertyValue(property, maxValueAttribute, onValueChangedCallback, info, parent);
                foreach ((string _, Action fix)  in checkerResults)
                {
                    fix.Invoke();
                }
                UIToolkitUtils.SetHelpBox(helpBox, string.Join("\n", errors));
            }
        }
    }
}
#endif
