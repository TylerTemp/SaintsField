using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEditor.Search;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(VisibilityAttribute))]
    [CustomPropertyDrawer(typeof(ShowIfAttribute))]
    [CustomPropertyDrawer(typeof(HideIfAttribute))]
    public class VisibilityAttributeDrawer: SaintsPropertyDrawer
    {
        #region IMGUI
        protected override (bool isForHide, bool orResult) GetAndVisibility(SerializedProperty property, ISaintsAttribute saintsAttribute)
        {
            VisibilityAttribute visibilityAttribute = (VisibilityAttribute)saintsAttribute;

            object target = GetParentTarget(property);
            Type type = target.GetType();

            _errors.Clear();
            List<bool> callbackTruly = new List<bool>();

            foreach (string andCallback in visibilityAttribute.andCallbacks)
            {
                (string error, bool isTruly) = IsTruly(target, type, andCallback);
                if (error != "")
                {
                    _errors.Add(error);
                }
                callbackTruly.Add(isTruly);
            }

            bool isForHide = visibilityAttribute.IsForHide;

            if (_errors.Count > 0)
            {
                return (isForHide, !isForHide);
            }

            return (isForHide, callbackTruly.All(each => each));

            // return (visibilityAttribute.IsForHide, visibilityAttribute.andCallbacks.All(callback => IsTruly(target, type, callback)));
        }

        private static (string error, bool isTruly) IsTruly(object target, Type type, string by)
        {
            (ReflectUtils.GetPropType getPropType, object fieldOrMethodInfo) = ReflectUtils.GetProp(type, by);

            if (getPropType == ReflectUtils.GetPropType.NotFound)
            {
                string error = $"No field or method named `{by}` found on `{target}`";
                // Debug.LogError(error);
                // _errors.Add(error);
                return (error, false);
            }

            if (getPropType == ReflectUtils.GetPropType.Property)
            {
                return ("", ReflectUtils.Truly(((PropertyInfo)fieldOrMethodInfo).GetValue(target)));
            }
            if (getPropType == ReflectUtils.GetPropType.Field)
            {
                return ("", ReflectUtils.Truly(((FieldInfo)fieldOrMethodInfo).GetValue(target)));
            }
            // ReSharper disable once InvertIf
            if (getPropType == ReflectUtils.GetPropType.Method)
            {
                MethodInfo methodInfo = (MethodInfo)fieldOrMethodInfo;
                ParameterInfo[] methodParams = methodInfo.GetParameters();
                Debug.Assert(methodParams.All(p => p.IsOptional));
                object methodResult;
                // try
                // {
                //     methodInfo.Invoke(target, methodParams.Select(p => p.DefaultValue).ToArray())
                // }
                try
                {
                    methodResult = methodInfo.Invoke(target, methodParams.Select(p => p.DefaultValue).ToArray());
                }
                catch (TargetInvocationException e)
                {
                    Debug.LogException(e);
                    Debug.Assert(e.InnerException != null);
                    return (e.InnerException.Message, false);

                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                    return (e.Message, false);
                }
                return ("", ReflectUtils.Truly(methodResult));
            }
            throw new ArgumentOutOfRangeException(nameof(getPropType), getPropType, null);
        }

        private readonly List<string> _errors = new List<string>();

        protected override bool WillDrawBelow(SerializedProperty property, ISaintsAttribute saintsAttribute)
        {
            return _errors.Count > 0;
        }

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute)
        {
            if (_errors.Count == 0)
            {
                return position;
            }

            string error = string.Join("\n\n", _errors);

            (Rect errorRect, Rect leftRect) = RectUtils.SplitHeightRect(position, ImGuiHelpBox.GetHeight(error, position.width, MessageType.Error));
            ImGuiHelpBox.Draw(errorRect, error, MessageType.Error);
            return leftRect;
        }

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label,
            float width,
            ISaintsAttribute saintsAttribute)
        {
            // Debug.Log("check extra height!");
            if (_errors.Count == 0)
            {
                return 0;
            }

            // Debug.Log(HelpBox.GetHeight(_error));
            return ImGuiHelpBox.GetHeight(string.Join("\n\n", _errors), width, MessageType.Error);
        }
        #endregion

        #region UIToolkit

        private static string NameVisibility(SerializedProperty property, int index) => $"{property.propertyType}_{index}__Visibility";
        private static string ClassVisibility(SerializedProperty property) => $"{property.propertyType}__Visibility";
        private static string NameVisibilityHelpBox(SerializedProperty property, int index) => $"{property.propertyType}_{index}__Visibility_HelpBox";

        // private struct MetaInfo
        // {
        //     public bool Computed;
        //     public VisibilityAttribute Attribute;
        // }

        protected override VisualElement CreateAboveUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
            VisualElement container, object parent)
        {
            VisualElement root = new VisualElement
            {
                name = NameVisibility(property, index),
                userData = (VisibilityAttribute) saintsAttribute,
            };
            root.AddToClassList(ClassVisibility(property));
            return root;
        }

        protected override VisualElement CreateBelowUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
            VisualElement container, object parent)
        {
            return new HelpBox("", HelpBoxMessageType.Error)
            {
                name = NameVisibilityHelpBox(property, index),
                style =
                {
                    display = DisplayStyle.None,
                },
            };
        }

        protected override void OnUpdateUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index, VisualElement container, Action<object> onValueChangedCallback, object parent)
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
            foreach ((string error, bool show) in visibilityElements.Select(each => GetShow((VisibilityAttribute)each.userData, parent.GetType(), parent)))
            {
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

            //
            //
            // bool curShow = (bool)visibilityElement.userData;
            //
            // (string error, bool show) = GetShow(saintsAttribute, parent.GetType(), parent);
            // // Debug.Log(show);
            // if (curShow != show)
            // {
            //     // Debug.Log($"error={error}, disabled={disabled}");
            //     visibilityElement.userData = show;
            //     container.style.display = show? DisplayStyle.Flex: DisplayStyle.None;
            // }
            //
            HelpBox helpBox = container.Q<HelpBox>(NameVisibilityHelpBox(property, index));
            string joinedError = string.Join("\n\n", errors);
            // ReSharper disable once InvertIf
            if (helpBox.text != joinedError)
            {
                helpBox.text = joinedError;
                helpBox.style.display = joinedError == "" ? DisplayStyle.None : DisplayStyle.Flex;
            }
        }

        private static (string error, bool show) GetShow(VisibilityAttribute visibilityAttribute, Type type, object target)
        {
            // VisibilityAttribute visibilityAttribute = (VisibilityAttribute)saintsAttribute;

            List<bool> callbackTruly = new List<bool>();
            List<string> errors = new List<string>();

            foreach (string andCallback in visibilityAttribute.andCallbacks)
            {
                (string error, bool isTruly) = IsTruly(target, type, andCallback);
                if (error != "")
                {
                    errors.Add(error);
                }
                callbackTruly.Add(isTruly);
            }

            if (errors.Count > 0)
            {
                return (string.Join("\n\n", errors), true);
            }

            bool truly = callbackTruly.All(each => each);
            if (visibilityAttribute.IsForHide)
            {
                truly = !truly;
            }

            return ("", truly);
        }

        #endregion
    }
}
