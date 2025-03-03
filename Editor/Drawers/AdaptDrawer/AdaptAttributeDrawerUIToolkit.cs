using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

#if UNITY_2021_3_OR_NEWER

namespace SaintsField.Editor.Drawers.AdaptDrawer
{
    public partial class AdaptAttributeDrawer
    {
        private static string NameAdaptHelpBox(SerializedProperty property, int index) => $"{property.propertyPath}_{index}__AdaptHelpBox";

        protected override VisualElement CreateBelowUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
            VisualElement container, FieldInfo info, object parent)
        {
            return new HelpBox("", HelpBoxMessageType.None)
            {
                style =
                {
                    display = DisplayStyle.None,
                },
                name = NameAdaptHelpBox(property, index),
            };
        }

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
            IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container, Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            AdaptAttribute adaptAttribute = (AdaptAttribute)saintsAttribute;

            // first is ignore with adaptable input
            if (allAttributes.Any(each => each is IAdaptable) && allAttributes.OfType<AdaptAttribute>().First().Equals(adaptAttribute))
            {
                return;
            }

            HelpBox helpBox = container.Q<HelpBox>(NameAdaptHelpBox(property, index));

            helpBox.Q<Label>().enableRichText = true;

            helpBox.style.display = DisplayStyle.Flex;

            (string error, string display) initCheck = GetDisplay(property, adaptAttribute);
            if (!string.IsNullOrEmpty(initCheck.error))
            {
                helpBox.messageType = HelpBoxMessageType.Error;
                helpBox.text = initCheck.error;
                return;
            }

            helpBox.text = FormatString(initCheck.display);
            helpBox.TrackPropertyValue(property, _ =>
            {
                (string error, string display) = GetDisplay(property, adaptAttribute);
                if (error != "")
                {
                    helpBox.messageType = HelpBoxMessageType.Error;
                    helpBox.text = error;
                }
                else
                {
                    helpBox.messageType = HelpBoxMessageType.None;
                    helpBox.text = FormatString(display);
                }
            });
        }

        private static string FormatString(string display) => $" <color=#{ColorUtility.ToHtmlStringRGBA(EColor.Gray.GetColor())}>{display}</color>";
    }
}
#endif
