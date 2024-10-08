using System;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;
#if UNITY_2021_3_OR_NEWER
using UnityEngine.UIElements;
#endif

namespace SaintsField.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(RequiredAttribute))]
    public class RequiredAttributeDrawer: SaintsPropertyDrawer
    {
        private static (string error, bool result) Truly(SerializedProperty property, FieldInfo field, object target)
        {
            (string curError, int _, object curValue) = Util.GetValue(property, field, target);
            return curError != ""
                ? (curError, false)
                : ("", ReflectUtils.Truly(curValue));
        }

        #region IMGUI
        private static string GetErrorImGui(SerializedProperty property, ISaintsAttribute saintsAttribute,
            FieldInfo info, object parent)
        {
            string error = ValidateType(property, info.FieldType);
            if(error != "")
            {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_REQUIRED
                Debug.Log($"get error=`{error}`");
#endif

                return error;
            }

            // property.serializedObject.ApplyModifiedProperties();
            (string trulyError, bool isTruly) = Truly(property, info, parent);

            if(trulyError != "")
            {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_REQUIRED
                Debug.Log($"get error=`{error}`");
#endif

                return trulyError;
            }

// #if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_REQUIRED
//             Debug.Log($"truly?={isTruly}");
// #endif
            if (isTruly)
            {
                return "";
            }

            string errorMessage = ((RequiredAttribute)saintsAttribute).ErrorMessage;
            return errorMessage ?? $"{property.displayName} is required";
        }

        protected override bool WillDrawBelow(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            FieldInfo info,
            object parent)
        {
            string error = GetErrorImGui(property, saintsAttribute, info, parent);
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_REQUIRED
            Debug.Log($"WillDrawBelow error=`{error}`");
#endif
            return error != "";
        }

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width,
            ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent)
        {
            string error = GetErrorImGui(property, saintsAttribute, info, parent);
            float belowHeight = error == "" ? 0 : ImGuiHelpBox.GetHeight(error, width, MessageType.Error);

#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_REQUIRED
            Debug.Log($"belowHeight={belowHeight}/{MessageType.Error}; width={width}");
#endif
            // return 50;
            return belowHeight;
        }

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent)
        {
            string error = GetErrorImGui(property, saintsAttribute, info, parent);
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_REQUIRED
            Debug.Log($"belowHeight has={position.height}/width={position.width}");
#endif
            // EditorGUI.DrawRect(new Rect(position)
            // {
            //     height = 50,
            // }, new[]{Color.blue, Color.cyan, Color.magenta, }[UnityEngine.Random.Range(0, 3)]);
            if (error == "")
            {
                return position;
            }

            Rect leftOut = ImGuiHelpBox.Draw(position, error, MessageType.Error);

            // EditorGUI.DrawRect(leftOut, Color.yellow);

            return leftOut;
        }

        private static string ValidateType(SerializedProperty property, Type fieldType)
        {
            // if (property.propertyType == SerializedPropertyType.Integer)
            // {
            //     return $"`{property.displayName}` can not be a valued type: int";
            // }
            // if (property.propertyType == SerializedPropertyType.Float)
            // {
            //     return $"`{property.displayName}` can not be a valued type: float";
            // }

            // ReSharper disable once ConvertIfStatementToReturnStatement
            // Type curType = SerializedUtils.GetType(property);
            // an array, list, struct or class && not struct
            if (property.propertyType == SerializedPropertyType.Generic && fieldType.IsValueType)
            {
                return $"`{property.displayName}` can not be a valued type: {fieldType}";
            }

            return "";
        }

        #endregion

        private static string NameRequiredBox(SerializedProperty property, int index) =>
            $"{property.propertyPath}_{index}__Required";

        private struct MetaInfo
        {
            public bool TypeError;
            // public bool IsTruly;
        }

#if UNITY_2021_3_OR_NEWER

        #region UIToolkit

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

        #endregion

#endif
    }
}
