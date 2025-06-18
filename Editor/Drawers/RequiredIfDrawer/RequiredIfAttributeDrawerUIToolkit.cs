#if UNITY_2021_3_OR_NEWER
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.RequiredIfDrawer
{
    public partial class RequiredIfAttributeDrawer
    {
        private static string NameRequiredIfBox(SerializedProperty property, int index) =>
            $"{property.propertyPath}_{index}__RequiredIf";

        protected override VisualElement CreateBelowUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index,
            IReadOnlyList<PropertyAttribute> allAttributes,
            VisualElement container, FieldInfo info, object parent)
        {
            Type rawType = SerializedUtils.PropertyPathIndex(property.propertyPath) < 0
                ? info.FieldType
                : ReflectUtils.GetElementType(info.FieldType);
            string typeError = ValidateType(property, rawType);

            RequiredAttribute requiredAttribute = allAttributes.OfType<RequiredAttribute>().FirstOrDefault();

            HelpBoxMessageType helpBoxMessageType = requiredAttribute?.MessageType.GetUIToolkitMessageType() ?? HelpBoxMessageType.Error;

            // Debug.Log(typeError);
            HelpBox helpBox = new HelpBox(typeError, helpBoxMessageType)
            {
                style =
                {
                    display = typeError == ""? DisplayStyle.None : DisplayStyle.Flex,
                },
                name = NameRequiredIfBox(property, index),
                userData = new MetaInfo
                {
                    TypeError = typeError != "",
                    // IsTruly = true,
                },
            };

            helpBox.AddToClassList(ClassAllowDisable);
            return helpBox;
        }

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
            IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container, Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            RequiredIfAttribute requiredIfAttribute = (RequiredIfAttribute)saintsAttribute;

            HelpBox helpBox = container.Q<HelpBox>(name: NameRequiredIfBox(property, index));

            helpBox.TrackPropertyValue(property, prop =>
            {
                MetaInfo metaInfo = (MetaInfo)helpBox.userData;

                if (metaInfo.TypeError)
                {
                    return;
                }

                (string trulyError, bool isTruly) = Truly(requiredIfAttribute, prop, info, parent);
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_REQUIRED
                Debug.Log(isTruly);
#endif

                string error;
                if (trulyError == "")
                {
                    // Debug.Log($"{isTruly}/{metaInfo.IsTruly}");
                    if(!isTruly)
                    {
                        string errorMessage = ((RequiredAttribute)saintsAttribute).ErrorMessage;
                        if (errorMessage == null)
                        {
                            int arrayIndex = SerializedUtils.PropertyPathIndex(property.propertyPath);
                            string propertyName = property.displayName;
                            if (arrayIndex != -1)
                            {
                                propertyName = $"{ObjectNames.NicifyVariableName(info.Name)}[{arrayIndex}]";
                            }
                            error = $"{propertyName} is required";
                        }
                        else
                        {
                            error = errorMessage;
                        }
                    }
                    else
                    {
                        error = "";
                    }
                }
                else
                {
                    error = trulyError;
                }

                if (error != helpBox.text)
                {
                    // Debug.Log($"Update error: {error}");
                    helpBox.style.display = error == "" ? DisplayStyle.None : DisplayStyle.Flex;
                    helpBox.text = error;

                    // helpBox.userData = new MetaInfo
                    // {
                    //     TypeError = false,
                    //     // IsTruly = isTruly,
                    // };

                }
            });
        }
    }
}
#endif
