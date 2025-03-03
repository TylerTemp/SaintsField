#if UNITY_2021_3_OR_NEWER
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.VisibilityDrawers.VisibilityDrawer
{
    public partial class VisibilityAttributeDrawer
    {

        private static string NameVisibility(SerializedProperty property, int index) => $"{property.propertyType}_{index}__Visibility";
        private static string ClassVisibility(SerializedProperty property) => $"{property.propertyType}__Visibility";
        private static string NameVisibilityHelpBox(SerializedProperty property, int index) => $"{property.propertyType}_{index}__Visibility_HelpBox";

        // private struct MetaInfo
        // {
        //     public bool Computed;
        //     public VisibilityAttribute Attribute;
        // }

        protected override VisualElement CreateAboveUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index,
            VisualElement container, FieldInfo info, object parent)
        {
            VisualElement root = new VisualElement
            {
                name = NameVisibility(property, index),
                userData = saintsAttribute,
            };
            root.AddToClassList(ClassVisibility(property));
            return root;
        }

        protected override VisualElement CreateBelowUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index,
            VisualElement container, FieldInfo info, object parent)
        {
            HelpBox helpBox = new HelpBox("", HelpBoxMessageType.Error)
            {
                name = NameVisibilityHelpBox(property, index),
                style =
                {
                    display = DisplayStyle.None,
                },
            };

            helpBox.AddToClassList(ClassAllowDisable);

            return helpBox;
        }

        protected override void OnUpdateUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index, VisualElement container, Action<object> onValueChangedCallback, FieldInfo info)
        {
            IReadOnlyList<VisualElement> visibilityElements = container.Query<VisualElement>(className: ClassVisibility(property)).ToList();
            VisualElement topElement = visibilityElements[0];
            if (topElement.name != NameVisibility(property, index))
            {
                return;
            }

            bool curShow = container.style.display != DisplayStyle.None;
            (string error, bool nowShow) = GetNowShowUIToolkit(property, info);

            if (curShow != nowShow)
            {
                container.style.display = nowShow ? DisplayStyle.Flex : DisplayStyle.None;
            }

            HelpBox helpBox = container.Q<HelpBox>(NameVisibilityHelpBox(property, index));
            // ReSharper disable once InvertIf
            if (helpBox.text != error)
            {
                helpBox.text = error;
                helpBox.style.display = error == "" ? DisplayStyle.None : DisplayStyle.Flex;
            }
        }

        private static (string error, bool show) GetNowShowUIToolkit(SerializedProperty property, FieldInfo info)
        {
            (ShowIfAttribute[] attributes, object parent) = SerializedUtils.GetAttributesAndDirectParent<ShowIfAttribute>(property);

            List<bool> showOrResults = new List<bool>();
            string error = "";
            foreach (ShowIfAttribute showIfAttribute in attributes)
            {
                (string error, bool shown) showResult = showIfAttribute.IsShow
                    ? ShowIfAttributeDrawer.HelperShowIfIsShown(showIfAttribute.ConditionInfos, property, info, parent)
                    : HideIfAttributeDrawer.HelperHideIfIsShown(showIfAttribute.ConditionInfos, property, info, parent);

                if (showResult.error != "")
                {
                    error = showResult.error;
                    break;
                }
                showOrResults.Add(showResult.shown);
            }

            if (error != "")
            {
                return (error, true);
            }

            Debug.Assert(showOrResults.Count > 0);
            return ("", showOrResults.Any(each => each));
        }


    }
}
#endif
