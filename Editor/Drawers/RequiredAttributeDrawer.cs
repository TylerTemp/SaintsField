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
        private static bool Truly(SerializedProperty property, object target)
        {
            // UnityEngine.Object target = property.serializedObject.targetObject;
            (ReflectUtils.GetPropType getPropType, object fieldOrMethodInfo) found = ReflectUtils.GetProp(target.GetType(), property.name);

            // Debug.Log($"found={found.getPropType}; {found.fieldOrMethodInfo} / {property.name}, {target}");

            if (found.getPropType == ReflectUtils.GetPropType.Property && found.fieldOrMethodInfo is PropertyInfo propertyInfo)
            {
                return ReflectUtils.Truly(propertyInfo.GetValue(target));
            }

            if (found.getPropType == ReflectUtils.GetPropType.Field && found.fieldOrMethodInfo is FieldInfo foundFieldInfo)
            {
                return ReflectUtils.Truly(foundFieldInfo.GetValue(target));
            }
            if (found.getPropType == ReflectUtils.GetPropType.NotFound || found.getPropType == ReflectUtils.GetPropType.Method)
            {
                throw new ArgumentOutOfRangeException(nameof(found.getPropType), found.getPropType, null);
            }
            // Handle any other cases here, if needed
            throw new NotImplementedException("Unexpected case");
        }

        #region IMGUI
        // private string _error = "";

//         protected override bool DrawPostFieldImGui(Rect position, SerializedProperty property, GUIContent label,
//             ISaintsAttribute saintsAttribute, bool valueChanged, FieldInfo info, object parent)
//         {
//             _error = ValidateType(property, info.FieldType);
//             if(_error != "")
//             {
// #if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_DRAW_PROCESS_REQUIRED
//                 Debug.Log($"get error=`{_error}`");
// #endif
//
//                 return true;
//             }
//
//             property.serializedObject.ApplyModifiedProperties();
//             bool isTruly = Truly(property, parent);
// #if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_DRAW_PROCESS_REQUIRED
//             Debug.Log(isTruly);
// #endif
//             if (isTruly)
//             {
//                 _error = "";
//                 return true;
//             }
//
//             string errorMessage = ((RequiredAttribute)saintsAttribute).ErrorMessage;
//             _error = errorMessage ?? $"{property.displayName} is required";
//             return true;
//         }

        private static string GetErrorImGui(SerializedProperty property, ISaintsAttribute saintsAttribute,
            FieldInfo info, object parent)
        {
            string error = ValidateType(property, info.FieldType);
            if(error != "")
            {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_DRAW_PROCESS_REQUIRED
                Debug.Log($"get error=`{error}`");
#endif

                return error;
            }

            // property.serializedObject.ApplyModifiedProperties();
            bool isTruly = Truly(property, parent);
// #if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_DRAW_PROCESS_REQUIRED
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
            FieldInfo info,
            object parent)
        {
            string error = GetErrorImGui(property, saintsAttribute, info, parent);
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_DRAW_PROCESS_REQUIRED
            Debug.Log($"WillDrawBelow error=`{error}`");
#endif
            return error != "";
        }

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width,
            ISaintsAttribute saintsAttribute, FieldInfo info, object parent)
        {
            string error = GetErrorImGui(property, saintsAttribute, info, parent);
            float belowHeight = error == "" ? 0 : ImGuiHelpBox.GetHeight(error, width, MessageType.Error);

#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_DRAW_PROCESS_REQUIRED
            Debug.Log($"belowHeight={belowHeight}/{MessageType.Error}; width={width}");
#endif
            // return 50;
            return belowHeight;
        }

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, FieldInfo info, object parent)
        {
            string error = GetErrorImGui(property, saintsAttribute, info, parent);
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_DRAW_PROCESS_REQUIRED
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
            public bool IsTruly;
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
                    IsTruly = true,
                },
            };

            helpBox.AddToClassList(ClassAllowDisable);
            return helpBox;
        }

        protected override void OnUpdateUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            VisualElement container, Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            HelpBox helpBox = container.Q<HelpBox>(NameRequiredBox(property, index));
            MetaInfo metaInfo = (MetaInfo)helpBox.userData;

            if (metaInfo.TypeError)
            {
                return;
            }

            bool isTruly = Truly(property, parent);
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_DRAW_PROCESS_REQUIRED
            Debug.Log(isTruly);
#endif

            // ReSharper disable once InvertIf
            if(isTruly != metaInfo.IsTruly)
            {

                // Debug.Log($"isTruly={isTruly}; meta.isTruly={metaInfo.IsTruly}");
                helpBox.style.display = isTruly ? DisplayStyle.None : DisplayStyle.Flex;

                string errorMessage = ((RequiredAttribute)saintsAttribute).ErrorMessage;
                string error = errorMessage ?? $"{property.displayName} is required";
                helpBox.text = error;

                helpBox.userData = new MetaInfo
                {
                    TypeError = false,
                    IsTruly = isTruly,
                };
            }


        }

        #endregion

#endif
    }
}
