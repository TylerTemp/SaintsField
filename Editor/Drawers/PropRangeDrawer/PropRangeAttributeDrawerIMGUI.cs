using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.PropRangeDrawer
{
    public partial class PropRangeAttributeDrawer
    {
        private class ImGuiInfo
        {
            public float PreValue;
            public string Error;
        }

        private static readonly Dictionary<string, ImGuiInfo> ImGuiInfos = new Dictionary<string, ImGuiInfo>();

        private static ImGuiInfo EnsureKey(SerializedProperty property)
        {
            string key = SerializedUtils.GetUniqueId(property);

            if(ImGuiInfos.TryGetValue(key, out ImGuiInfo info))
            {
                return info;
            }

            return ImGuiInfos[key] = new ImGuiInfo
            {
                PreValue = (property.propertyType == SerializedPropertyType.Float
                    ? property.floatValue
                    : property.intValue) + 1f,  // trigger the change check
                Error = "",
            };
        }

        protected override void DrawField(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, IReadOnlyList<PropertyAttribute> allAttributes, OnGUIPayload onGUIPayload,
            FieldInfo info, object parentTarget)
        {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_DRAW_PROCESS
            Debug.Log($"#PropRange# #DrawField# for {property.propertyPath}");
#endif

            ImGuiInfo cacheInfo = EnsureKey(property);

            MetaInfo metaInfo = GetMetaInfo(property, saintsAttribute, info, parentTarget);
            if (metaInfo.Error != "")
            {
                cacheInfo.Error = metaInfo.Error;
                DefaultDrawer(position, property, label, info);
                return;
            }

            AdaptAttribute adaptAttribute = allAttributes.OfType<AdaptAttribute>().FirstOrDefault();

            // ReSharper disable once ConvertToUsingDeclaration
            using (EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
            {
                bool isFloat = metaInfo.IsFloat;
                float curPropValue = isFloat ? property.floatValue : property.intValue;
                (string error, double value) curValueInfo = GetPreValue(curPropValue, adaptAttribute);
                if (curValueInfo.error != "")
                {
                    metaInfo.Error = curValueInfo.error;
                    DefaultDrawer(position, property, label, info);
                    return;
                }
                float curValue = (float)curValueInfo.value;
                // float minValue = metaInfo.MinValue;
                // float maxValue = metaInfo.MaxValue;

                (string error, double value) minValueInfo = GetPreValue(metaInfo.MinValue, adaptAttribute);
                if (minValueInfo.error != "")
                {
                    metaInfo.Error = minValueInfo.error;
                    DefaultDrawer(position, property, label, info);
                    return;
                }
                float minValue = (float)minValueInfo.value;

                (string error, double value) maxValueInfo = GetPreValue(metaInfo.MaxValue, adaptAttribute);
                if (maxValueInfo.error != "")
                {
                    metaInfo.Error = maxValueInfo.error;
                    DefaultDrawer(position, property, label, info);
                    return;
                }
                float maxValue = (float)maxValueInfo.value;

#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_DRAW_PROCESS
                Debug.Log($"#PropRange# #DrawField# for {property.propertyPath}: {minValue}~{maxValue} {curValue}");
#endif
                float adaptedValue = EditorGUI.Slider(position, label, curValue, minValue, maxValue);
                (string error, double value) postValueInfo = GetPostValue(adaptedValue, adaptAttribute);
                if (postValueInfo.error != "")
                {
                    metaInfo.Error = postValueInfo.error;
                    return;
                }

                float parsedValue = GetValue(metaInfo, (float)postValueInfo.value);

                // ReSharper disable once InvertIf
                if (changed.changed || !Mathf.Approximately(cacheInfo.PreValue, parsedValue))
                {
                    cacheInfo.PreValue = parsedValue;
                    (string error, double value) preValueInfo = GetPreValue(parsedValue, adaptAttribute);
                    if (preValueInfo.error != "")
                    {
                        metaInfo.Error = preValueInfo.error;
                        return;
                    }

                    if (isFloat)
                    {
                        property.doubleValue = parsedValue;
                        onGUIPayload.SetValue(parsedValue);
                    }
                    else
                    {
                        property.intValue = (int)parsedValue;
                        onGUIPayload.SetValue((int)parsedValue);
                    }
                }
            }
        }



        protected override bool WillDrawBelow(SerializedProperty property,
            IReadOnlyList<PropertyAttribute> allAttributes, ISaintsAttribute saintsAttribute,
            int index,
            FieldInfo info,
            object parent)
        {
            return EnsureKey(property).Error != "";
        }

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width,
            IReadOnlyList<PropertyAttribute> allAttributes,
            ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent)
        {
            string error = EnsureKey(property).Error;
            return error == "" ? 0 : ImGuiHelpBox.GetHeight(error, width, MessageType.Error);
        }

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, int index, IReadOnlyList<PropertyAttribute> allAttributes,
            OnGUIPayload onGuiPayload, FieldInfo info, object parent)
        {
            string error = EnsureKey(property).Error;
            return error == "" ? position : ImGuiHelpBox.Draw(position, error, MessageType.Error);
        }
    }
}
