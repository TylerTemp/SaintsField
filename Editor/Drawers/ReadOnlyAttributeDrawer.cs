using System;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;
#if UNITY_2021_3_OR_NEWER
using System.Collections.Generic;
using UnityEngine.UIElements;
#endif

namespace SaintsField.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
    public class ReadOnlyAttributeDrawer: SaintsPropertyDrawer
    {
        #region IMGUI
        private string _error = "";

        protected override (bool isActive, Rect position) DrawPreLabelImGui(Rect position, SerializedProperty property,
            ISaintsAttribute saintsAttribute, object parent)
        {
            (string error, bool disabled) = IsDisabled(property, (ReadOnlyAttribute)saintsAttribute, parent);
            _error = error;
            if(disabled)
            {
                EditorGUI.BeginDisabledGroup(true);
            }
            return (true, position);
        }

        protected override bool DrawPostFieldImGui(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, bool valueChanged, FieldInfo info, object parent)
        {
            EditorGUI.EndDisabledGroup();
            return true;
        }

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
            return ImGuiHelpBox.GetHeight(_error, width, MessageType.Error);
        }

        private static (string error, bool disabled) IsDisabled(SerializedProperty property, ReadOnlyAttribute targetAttribute, object target)
        {
            string[] bys = targetAttribute.ReadOnlyBys;
            if(bys is null)
            {
                return ("", targetAttribute.ReadOnlyDirectValue);
            }

            foreach (string by in bys)
            {
                // (ReflectUtils.GetPropType getPropType, object fieldOrMethodInfo) found = ReflectUtils.GetProp(target.GetType(), by);
                // bool result;
                // switch (found)
                // {
                //     case (ReflectUtils.GetPropType.NotFound, _):
                //     {
                //         _error = $"No field or method named `{by}` found on `{target}`";
                //         Debug.LogError(_error);
                //         result = false;
                //     }
                //         break;
                //     case (ReflectUtils.GetPropType.Property, PropertyInfo propertyInfo):
                //     {
                //         result = ReflectUtils.Truly(propertyInfo.GetValue(target));
                //     }
                //         break;
                //     case (ReflectUtils.GetPropType.Field, FieldInfo foundFieldInfo):
                //     {
                //         result = ReflectUtils.Truly(foundFieldInfo.GetValue(target));
                //     }
                //         break;
                //     case (ReflectUtils.GetPropType.Method, MethodInfo methodInfo):
                //     {
                //         ParameterInfo[] methodParams = methodInfo.GetParameters();
                //         Debug.Assert(methodParams.All(p => p.IsOptional));
                //         // Debug.Assert(methodInfo.ReturnType == typeof(bool));
                //         result =  ReflectUtils.Truly(methodInfo.Invoke(target, methodParams.Select(p => p.DefaultValue).ToArray()));
                //     }
                //         break;
                //     default:
                //         throw new ArgumentOutOfRangeException(nameof(found), found, null);
                // }

                (ReflectUtils.GetPropType getPropType, object fieldOrMethodInfo) found = ReflectUtils.GetProp(target.GetType(), by);
                (string error, bool disabled) result;

                if (found.getPropType == ReflectUtils.GetPropType.NotFound)
                {
                    string error = $"No field or method named `{by}` found on `{target}`";
                    // Debug.LogError(_error);
                    result = (error, false);
                }
                else if (found.getPropType == ReflectUtils.GetPropType.Property && found.fieldOrMethodInfo is PropertyInfo propertyInfo)
                {
                    result = ("", ReflectUtils.Truly(propertyInfo.GetValue(target)));
                }
                else if (found.getPropType == ReflectUtils.GetPropType.Field && found.fieldOrMethodInfo is FieldInfo foundFieldInfo)
                {
                    result = ("", ReflectUtils.Truly(foundFieldInfo.GetValue(target)));
                }
                else if (found.getPropType == ReflectUtils.GetPropType.Method && found.fieldOrMethodInfo is MethodInfo methodInfo)
                {
                    ParameterInfo[] methodParams = methodInfo.GetParameters();
                    Debug.Assert(methodParams.All(p => p.IsOptional));
                    // Debug.Assert(methodInfo.ReturnType == typeof(bool));
                    result = ("", ReflectUtils.Truly(methodInfo.Invoke(target,
                        methodParams.Select(p => p.DefaultValue).ToArray())));
                }
                else
                {
                    throw new ArgumentOutOfRangeException(nameof(found.getPropType), found.getPropType, null);
                }

                if (result.error != "")
                {
                    return (result.error, false);
                }

                if (!result.disabled)
                {
                    return ("", false);
                }
            }
            return ("", true);
        }
        #endregion

#if UNITY_2021_3_OR_NEWER

        #region UIToolkit

        private static string NameReadOnly(SerializedProperty property, int index) => $"{property.propertyType}_{index}__ReadOnly";
        private static string ClassReadOnly(SerializedProperty property) => $"{property.propertyType}__ReadOnly";
        private static string NameReadOnlyHelpBox(SerializedProperty property, int index) => $"{property.propertyType}_{index}__ReadOnly_HelpBox";

        protected override VisualElement CreateAboveUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
            VisualElement container, object parent)
        {
            VisualElement root = new VisualElement
            {
                name = NameReadOnly(property, index),
                userData = (ReadOnlyAttribute) saintsAttribute,
            };
            root.AddToClassList(ClassReadOnly(property));
            return root;
        }

        protected override VisualElement CreateBelowUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index,
            VisualElement container, FieldInfo info, object parent)
        {
            return new HelpBox("", HelpBoxMessageType.Error)
            {
                name = NameReadOnlyHelpBox(property, index),
                style =
                {
                    display = DisplayStyle.None,
                },
            };
        }

        protected override void OnUpdateUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index, VisualElement container, Action<object> onValueChangedCallback, object parent)
        {
            IReadOnlyList<VisualElement> visibilityElements = container.Query<VisualElement>(className: ClassReadOnly(property)).ToList();
            VisualElement topElement = visibilityElements[0];

            if (topElement.name != NameReadOnly(property, index))
            {
                return;
            }

            bool curReadOnly = !container.enabledSelf;

            List<string> errors = new List<string>();
            bool nowReadOnly = false;
            foreach ((string error, bool readOnly) in visibilityElements.Select(each => IsDisabled(property, ((ReadOnlyAttribute)each.userData), parent)))
            {
                if (error != "")
                {
                    errors.Add(error);
                }

                if (readOnly)
                {
                    nowReadOnly = true;
                }
            }

            if (curReadOnly != nowReadOnly)
            {
                container.SetEnabled(false);
            }

            HelpBox helpBox = container.Q<HelpBox>(NameReadOnlyHelpBox(property, index));
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
