using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.VisibilityDrawers
{
    // [CustomPropertyDrawer(typeof(VisibilityAttribute))]
    // [CustomPropertyDrawer(typeof(ShowIfAttribute))]
    // [CustomPropertyDrawer(typeof(HideIfAttribute))]
    public abstract class VisibilityAttributeDrawer: SaintsPropertyDrawer
    {
        #region IMGUI
        protected override bool GetAndVisibility(SerializedProperty property,
            ISaintsAttribute saintsAttribute, FieldInfo info, object parent)
        {
            // VisibilityAttribute visibilityAttribute = (VisibilityAttribute)saintsAttribute;
            Type type = parent.GetType();

            (string error, bool show) = IsShown(property, saintsAttribute, info, type, parent);

            _error = error;
            return show;
        }

        protected abstract (string error, bool shown) IsShown(SerializedProperty property,
            ISaintsAttribute visibilityAttribute, FieldInfo info, Type type, object target);

        private string _error = "";

        protected override bool WillDrawBelow(SerializedProperty property, ISaintsAttribute saintsAttribute,
            FieldInfo info,
            object parent)
        {
            return _error != "";
        }

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, FieldInfo info, object parent)
        {
            if (_error == "")
            {
                return position;
            }

            // string error = string.Join("\n\n", _errors);

            (Rect errorRect, Rect leftRect) = RectUtils.SplitHeightRect(position, ImGuiHelpBox.GetHeight(_error, position.width, MessageType.Error));
            ImGuiHelpBox.Draw(errorRect, _error, MessageType.Error);
            return leftRect;
        }

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label,
            float width,
            ISaintsAttribute saintsAttribute, FieldInfo info, object parent)
        {
            // Debug.Log("check extra height!");
            if (_error == "")
            {
                return 0;
            }

            // Debug.Log(HelpBox.GetHeight(_error));
            return ImGuiHelpBox.GetHeight(string.Join("\n\n", _error), width, MessageType.Error);
        }
        #endregion

#if UNITY_2021_3_OR_NEWER

        #region UIToolkit

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
            int index, VisualElement container, Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            IReadOnlyList<VisualElement> visibilityElements = container.Query<VisualElement>(className: ClassVisibility(property)).ToList();
            VisualElement topElement = visibilityElements[0];
            if (topElement.name != NameVisibility(property, index))
            {
                return;
            }

            bool curShow = container.style.display != DisplayStyle.None;

            List<string> errors = new List<string>();
            bool nowShow = false;
            // bool isForHidden = ((VisibilityAttribute)saintsAttribute).IsForHide;
            foreach ((string error, bool show) in visibilityElements.Select(each => IsShown(property, (ISaintsAttribute)each.userData, info, parent.GetType(), parent)))
            {
                // bool invertedShow = isForHidden? !show: show;

                if (error != "")
                {
                    errors.Add(error);
                }

                if (show)
                {
                    nowShow = true;
                }
            }

            if (curShow != nowShow)
            {
                container.style.display = nowShow ? DisplayStyle.Flex : DisplayStyle.None;
            }

            HelpBox helpBox = container.Q<HelpBox>(NameVisibilityHelpBox(property, index));
            string joinedError = string.Join("\n\n", errors);
            // ReSharper disable once InvertIf
            if (helpBox.text != joinedError)
            {
                helpBox.text = joinedError;
                helpBox.style.display = joinedError == "" ? DisplayStyle.None : DisplayStyle.Flex;
            }
        }

        #endregion

#endif
    }
}
