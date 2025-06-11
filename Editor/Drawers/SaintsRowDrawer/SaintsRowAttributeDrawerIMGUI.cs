using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Playa;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.SaintsRowDrawer
{
    public partial class SaintsRowAttributeDrawer
    {
        private readonly Dictionary<int, IReadOnlyList<ISaintsRenderer>> _imGuiRenderers =
            new Dictionary<int, IReadOnlyList<ISaintsRenderer>>();

        private float _filedWidthCache = -1;

        private IEnumerable<ISaintsRenderer> ImGuiEnsureRenderers(FieldInfo info, SerializedProperty property)
        {
#if UNITY_2021_3_OR_NEWER
            if (property.propertyType == SerializedPropertyType.ManagedReference &&
                property.managedReferenceValue == null)
            {
                return Array.Empty<ISaintsRenderer>();
            }
#endif

            // string key = $"{property.serializedObject.targetObject.GetInstanceID()}:{property.propertyPath}";
            // if (GlobalCache.TryGetValue(key, out IReadOnlyList<ISaintsRenderer> result))
            // {
            //     return result;
            // }
            //
            // (object _, object current) = GetTargets(fieldInfo, property);
            // Dictionary<string, SerializedProperty> serializedFieldNames = GetSerializableFieldInfo(property).ToDictionary(each => each.name, each => each.property);
            // return GlobalCache[key] = SaintsEditor.GetRenderers(false, serializedFieldNames, property.serializedObject, current);
            int arrayIndex = SerializedUtils.PropertyPathIndex(property.propertyPath);

            if (_imGuiRenderers.TryGetValue(arrayIndex, out IReadOnlyList<ISaintsRenderer> result))
            {
                return result;
            }

            // Debug.Log($"create new for {property.propertyPath}");
            (string error, int index, object _, object current) = GetTargets(info, property);
            if (error == "")
            {
                Dictionary<string, SerializedProperty> serializedFieldNames = GetSerializableFieldInfo(property)
                    .ToDictionary(each => each.name, each => each.property);
                return _imGuiRenderers[index] =
                    SaintsEditor.HelperGetRenderers(serializedFieldNames, property.serializedObject, this, new[]{current});
            }

            Debug.LogWarning(error);
            return Array.Empty<ISaintsRenderer>();
        }

        protected override float GetFieldHeight(SerializedProperty property, GUIContent label, float width, ISaintsAttribute saintsAttribute,
            FieldInfo info, bool hasLabelWidth, object parent)
        {
#if UNITY_2021_3_OR_NEWER
            if (property.propertyType == SerializedPropertyType.ManagedReference &&
                property.managedReferenceValue == null)
            {
                return EditorGUIUtility.singleLineHeight;
            }
#endif

            float fullWidth = _filedWidthCache <= 0
                ? EditorGUIUtility.currentViewWidth - EditorGUI.indentLevel * 15
                : _filedWidthCache;

            SaintsRowAttribute saintsRowAttribute = (SaintsRowAttribute)saintsAttribute;
            float baseLineHeight = saintsRowAttribute.Inline ? 0 : SingleLineHeight;
            float fieldHeight = 0f;
            // ReSharper disable once InvertIf
            if (property.isExpanded || saintsRowAttribute.Inline)
            {
                // ReSharper disable once LoopCanBeConvertedToQuery
                foreach (ISaintsRenderer saintsRenderer in ImGuiEnsureRenderers(info, property))
                {
                    fieldHeight += saintsRenderer.GetHeightIMGUI(fullWidth);
                }
            }

            return baseLineHeight + fieldHeight;
        }

        protected override void DrawField(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute,
            IReadOnlyList<PropertyAttribute> allAttributes, OnGUIPayload onGUIPayload, FieldInfo info, object parent)
        {
            SaintsRowAttribute saintsRowAttribute = (SaintsRowAttribute)saintsAttribute;

            if (!saintsRowAttribute.Inline)
            {
                // Debug.Log($"render foldout {position.x}, {position.y}");
                property.isExpanded = EditorGUI.Foldout(new Rect(position)
                {
                    height = SaintsPropertyDrawer.SingleLineHeight,
                }, property.isExpanded, label, true);
            }

            if (!saintsRowAttribute.Inline && !property.isExpanded)
            {
                return;
            }

            Rect leftRect = saintsRowAttribute.Inline
                ? new Rect(position)
                : new Rect(position)
                {
                    x = position.x + SaintsPropertyDrawer.IndentWidth,
                    y = position.y + SaintsPropertyDrawer.SingleLineHeight,
                    height = position.height - SaintsPropertyDrawer.SingleLineHeight,
                    width = position.width - SaintsPropertyDrawer.IndentWidth,
                };

            float yAcc = leftRect.y;

            foreach (ISaintsRenderer saintsRenderer in ImGuiEnsureRenderers(info, property))
            {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTSROW
                    Debug.Log($"saintsRow: {saintsRenderer}");
#endif
                float height = saintsRenderer.GetHeightIMGUI(position.width);
                Rect rect = new Rect(leftRect)
                {
                    y = yAcc,
                    height = height,
                };
                saintsRenderer.RenderPositionIMGUI(rect);
                yAcc += height;
            }
        }
    }
}
