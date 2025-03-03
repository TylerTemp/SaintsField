using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.PropRangeDrawer
{
    public partial class PropRangeAttributeDrawer
    {
        private string _error = "";



        protected override void DrawField(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, IReadOnlyList<PropertyAttribute> allAttributes, OnGUIPayload onGUIPayload,
            FieldInfo info, object parentTarget)
        {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_DRAW_PROCESS
            Debug.Log($"#PropRange# #DrawField# for {property.propertyPath}");
#endif

            MetaInfo metaInfo = GetMetaInfo(property, saintsAttribute, info, parentTarget);
            if (metaInfo.Error != "")
            {
                _error = metaInfo.Error;
                DefaultDrawer(position, property, label, info);
                return;
            }

            // ReSharper disable once ConvertToUsingDeclaration
            using (EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
            {
                bool isFloat = metaInfo.IsFloat;
                float curValue = isFloat ? property.floatValue : property.intValue;
                float minValue = metaInfo.MinValue;
                float maxValue = metaInfo.MaxValue;
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_DRAW_PROCESS
                Debug.Log($"#PropRange# #DrawField# for {property.propertyPath}: {minValue}~{maxValue} {curValue}");
#endif
                float newValue = EditorGUI.Slider(position, label, curValue, minValue, maxValue);
                // ReSharper disable once InvertIf
                if (changed.changed)
                {
                    float parsedValue = GetValue(metaInfo, newValue);
                    if (isFloat)
                    {
                        property.floatValue = parsedValue;
                    }
                    else
                    {
                        property.intValue = (int)parsedValue;
                    }
                }
            }
        }

        private static float GetValue(MetaInfo metaInfo, float newValue)
        {
            // property.floatValue = newValue;
            float step = metaInfo.Step;
            bool isFloat = metaInfo.IsFloat;
            // Debug.Log(step);
            if (step <= 0)
            {
                // return newValue;
                return Mathf.Clamp(newValue, metaInfo.MinValue, metaInfo.MaxValue);
            }

            if (isFloat)
            {
                return Util.BoundFloatStep(newValue, metaInfo.MinValue, metaInfo.MaxValue, step);
            }

            return Util.BoundIntStep(newValue, metaInfo.MinValue, metaInfo.MaxValue, (int)step);
        }

        protected override bool WillDrawBelow(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            FieldInfo info,
            object parent) => _error != "";

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width,
            ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent) =>
            _error == "" ? 0 : ImGuiHelpBox.GetHeight(_error, width, MessageType.Error);

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, int index, IReadOnlyList<PropertyAttribute> allAttributes,
            OnGUIPayload onGuiPayload, FieldInfo info, object parent) =>
            _error == "" ? position : ImGuiHelpBox.Draw(position, _error, MessageType.Error);
    }
}
