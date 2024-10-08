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
    public abstract class VisibilityAttributeDrawer: SaintsPropertyDrawer
    {
        #region IMGUI
        protected override bool GetThisDecoratorVisibility(ShowIfAttribute targetAttribute, SerializedProperty property, FieldInfo info, object target)
        {
            (string error, bool shown) = IsShown(targetAttribute, property, info, target);
            _error = error;
            return shown;
        }

        protected abstract (string error, bool shown) IsShown(ShowIfAttribute targetAttribute, SerializedProperty property, FieldInfo info, object target);

        private string _error = "";

        protected override bool WillDrawBelow(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            FieldInfo info,
            object parent)
        {
            return _error != "";
        }

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent)
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
            ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent)
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
            int index, VisualElement container, Action<object> onValueChangedCallback, FieldInfo info)
        {
            IReadOnlyList<VisualElement> visibilityElements = container.Query<VisualElement>(className: ClassVisibility(property)).ToList();
            VisualElement topElement = visibilityElements[0];
            if (topElement.name != NameVisibility(property, index))
            {
                return;
            }

            bool curShow = container.style.display != DisplayStyle.None;

            bool nowShow;
            (ShowIfAttribute[] attributes, object parent) = SerializedUtils.GetAttributesAndDirectParent<ShowIfAttribute>(property);

            List<bool> showOrResults = new List<bool>();
            string error = "";
            foreach (ShowIfAttribute showIfAttribute in attributes)
            {
                (string error, bool shown) showResult = showIfAttribute.IsShow
                    ? ShowIfAttributeDrawer.HelperShowIfIsShown(showIfAttribute.ConditionInfos, showIfAttribute.EditorMode, property, info, parent)
                    : HideIfAttributeDrawer.HelperHideIfIsShown(showIfAttribute.ConditionInfos, showIfAttribute.EditorMode, property, info, parent);

                if (showResult.error != "")
                {
                    error = showResult.error;
                    break;
                }
                showOrResults.Add(showResult.shown);
            }

            if (error != "")
            {
                nowShow = true;
            }
            else
            {
                Debug.Assert(showOrResults.Count > 0);
                nowShow = showOrResults.Any(each => each);
            }

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

        #endregion

#endif
    }
}
