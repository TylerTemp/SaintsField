using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Drawers.ExpandableDrawer;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.ProgressBarDrawer
{
    public partial class ProgressBarAttributeDrawer
    {
        private class ImGuiStatus
        {
            public string Error = "";
            public bool InArrayMousePressed;
        }

        private static readonly Dictionary<string, ImGuiStatus> ImGuiStatusCache =
            new Dictionary<string, ImGuiStatus>();

        private static ImGuiStatus EnsureKey(SerializedProperty property)
        {
            string key = SerializedUtils.GetUniqueId(property);
            if (ImGuiStatusCache.TryGetValue(key, out ImGuiStatus status))
            {
                return status;
            }

            NoLongerInspectingWatch(property.serializedObject.targetObject, key, () => ImGuiStatusCache.Remove(key));
            return ImGuiStatusCache[key] = new ImGuiStatus();
        }

        protected override float GetFieldHeight(SerializedProperty property, GUIContent label,
            float width,
            int index,
            ISaintsAttribute saintsAttribute,
            FieldInfo info,
            bool hasLabelWidth, object parent)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        protected override void DrawField(Rect position, SerializedProperty property, GUIContent label,
            int index,
            ISaintsAttribute saintsAttribute,
            IReadOnlyList<PropertyAttribute> allAttributes,
            FieldInfo info,
            object parent)
        {
            ImGuiStatus status = EnsureKey(property);
            status.Error = "";

            ProgressBarAttribute progressBarAttribute = (ProgressBarAttribute)saintsAttribute;

            int controlId = GUIUtility.GetControlID(FocusType.Passive, position);
            // Debug.Log(label.text.Length);
            Rect fieldRect = EditorGUI.PrefixLabel(position, controlId, label);
            Rect labelRect = new Rect(position)
            {
                width = position.width - fieldRect.width,
            };
            // EditorGUI.DrawRect(position, Color.yellow);

            MetaInfo metaInfo = GetMetaInfo(property, saintsAttribute, info, parent);
            status.Error = metaInfo.Error;

            EditorGUI.DrawRect(fieldRect, metaInfo.BackgroundColor);

            bool isInt = property.propertyType == SerializedPropertyType.Integer;
            float curValue = isInt
                ? property.intValue
                : property.floatValue;

            // float percent = Mathf.Clamp01(curValue / (metaInfo.Max - metaInfo.Min));
            float percent = Mathf.InverseLerp(metaInfo.Min, metaInfo.Max, curValue);
            // Debug.Log($"percent={percent:P}");
            Rect fillRect = new Rect(fieldRect)
            {
                width = fieldRect.width * percent,
            };

            EditorGUI.DrawRect(fillRect, metaInfo.Color);

            if (GUI.enabled)
            {
                EditorGUIUtility.AddCursorRect(labelRect, MouseCursor.Link);
                EditorGUIUtility.AddCursorRect(fieldRect, MouseCursor.SlideArrow);
            }

            Event e = Event.current;

// #if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_PROGRESS_BAR
//             Debug.Log($"{e.isMouse}, {e.mousePosition}");
// #endif
            // ReSharper disable once InvertIf
            // Debug.Log($"{e.type} {e.isMouse}, {e.button}, {e.mousePosition}");

            if (e.type == EventType.MouseUp && e.button == 0)
            {
                // GUIUtility.hotControl = 0;
                // Debug.Log($"UP!");
                status.InArrayMousePressed = false;
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_PROGRESS_BAR
                Debug.Log($"mouse up {property.propertyPath}: {status.InArrayMousePressed}");
#endif
            }

            if (e.type == EventType.MouseDown && e.button == 0)
            {
                // arrayMousePressed[arrayIndex] = position.Contains(e.mousePosition);
                status.InArrayMousePressed = position.Contains(e.mousePosition);
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_PROGRESS_BAR
                Debug.Log($"mouse down {position}: {status.InArrayMousePressed}/{property.propertyPath}");
#endif
            }

            (string titleError, string title) = GetTitle(property, progressBarAttribute.TitleCallback,
                progressBarAttribute.Step, curValue, metaInfo.Min, metaInfo.Max, parent);
            if (status.Error == "")
            {
                status.Error = titleError;
            }

            // string title = null;

#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_PROGRESS_BAR
            Debug.Log($"{property.propertyPath}/{status.InArrayMousePressed}/{GetHashCode()}");
#endif

            if (GUI.enabled && (e.type == EventType.MouseDown || e.type == EventType.MouseDrag) &&
                status.InArrayMousePressed)
            {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_PROGRESS_BAR
                Debug.Log($"{property.propertyPath}/{status.InArrayMousePressed}");
#endif
                float newPercent = (e.mousePosition.x - fieldRect.x) / fieldRect.width;
                float newValue = Mathf.Lerp(metaInfo.Min, metaInfo.Max, newPercent);
                float boundValue = BoundValue(newValue, metaInfo.Min, metaInfo.Max, progressBarAttribute.Step, isInt);

                // Debug.Log($"boundValue={boundValue}, newValue={newValue}");

                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (boundValue != curValue)
                {
                    if (isInt)
                    {
                        property.intValue = (int)boundValue;
                        TriggerChangedIMGUI(property, (int)boundValue);
                    }
                    else
                    {
                        property.floatValue = boundValue;
                        TriggerChangedIMGUI(property, boundValue);
                    }

                    property.serializedObject.ApplyModifiedProperties();

                    (string titleError, string title) changedTitle = GetTitle(property,
                        progressBarAttribute.TitleCallback, progressBarAttribute.Step, boundValue, metaInfo.Min,
                        metaInfo.Max, parent);
                    if (status.Error == "")
                    {
                        status.Error = changedTitle.titleError;
                    }

                    title = changedTitle.title;

                    // Debug.Log($"value={newValue}, title={title}");
                }
            }

            // _imGuiError = titleError;
            if (!string.IsNullOrEmpty(title))
            {
                EditorGUI.DropShadowLabel(fieldRect, title);
            }
        }
    }
}
