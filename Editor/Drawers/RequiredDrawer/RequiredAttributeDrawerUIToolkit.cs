#if UNITY_2021_3_OR_NEWER
using System;
using System.Reflection;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.RequiredDrawer
{
    public partial class RequiredAttributeDrawer
    {

        private static string NameRequiredBox(SerializedProperty property, int index) =>
            $"{property.propertyPath}_{index}__Required";


        protected override VisualElement CreateBelowUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
            VisualElement container, FieldInfo info, object parent)
        {
            string typeError = ValidateType(property, info.FieldType);

            // Debug.Log(typeError);
            HelpBox helpBox = new HelpBox(typeError, HelpBoxMessageType.Error)
            {
                style =
                {
                    display = typeError == ""? DisplayStyle.None : DisplayStyle.Flex,
                },
                name = NameRequiredBox(property, index),
                userData = new MetaInfo
                {
                    TypeError = typeError != "",
                    // IsTruly = true,
                },
            };

            helpBox.AddToClassList(ClassAllowDisable);
            return helpBox;
        }

        protected override void OnUpdateUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            VisualElement container, Action<object> onValueChangedCallback, FieldInfo info)
        {
            object parent = SerializedUtils.GetFieldInfoAndDirectParent(property).parent;
            if (parent == null)
            {
                Debug.LogWarning($"{property.propertyPath} parent disposed unexpectedly.");
                return;
            }

            HelpBox helpBox = container.Q<HelpBox>(NameRequiredBox(property, index));
            MetaInfo metaInfo = (MetaInfo)helpBox.userData;

            if (metaInfo.TypeError)
            {
                return;
            }

            (string trulyError, bool isTruly) = Truly(property, info, parent);
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
        }

    }
}
#endif
