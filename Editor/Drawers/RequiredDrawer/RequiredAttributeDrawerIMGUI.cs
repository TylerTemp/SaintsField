using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.RequiredDrawer
{
    public partial class RequiredAttributeDrawer
    {
        private static string GetErrorImGui(SerializedProperty property, ISaintsAttribute saintsAttribute,
            FieldInfo info, object parent)
        {
            Type rawType = SerializedUtils.PropertyPathIndex(property.propertyPath) < 0
                ? info.FieldType
                : ReflectUtils.GetElementType(info.FieldType);

            string error = ValidateType(property, rawType);
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

        protected override bool WillDrawBelow(SerializedProperty property,
            IReadOnlyList<PropertyAttribute> allAttributes, ISaintsAttribute saintsAttribute,
            int index,
            FieldInfo info,
            object parent)
        {
            if (allAttributes.Any(each => each is RequiredIfAttribute))
            {
                return false;
            }
            string error = GetErrorImGui(property, saintsAttribute, info, parent);
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_REQUIRED
            Debug.Log($"WillDrawBelow error=`{error}`");
#endif
            return error != "";
        }

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width,
            IReadOnlyList<PropertyAttribute> allAttributes,
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
            ISaintsAttribute saintsAttribute, int index, IReadOnlyList<PropertyAttribute> allAttributes,
            OnGUIPayload onGuiPayload, FieldInfo info, object parent)
        {
            if (allAttributes.Any(each => each is RequiredIfAttribute))
            {
                return position;
            }

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

            Rect leftOut = ImGuiHelpBox.Draw(position, error, ((RequiredAttribute) saintsAttribute).MessageType.GetMessageType());

            // EditorGUI.DrawRect(leftOut, Color.yellow);

            return leftOut;
        }
    }
}
