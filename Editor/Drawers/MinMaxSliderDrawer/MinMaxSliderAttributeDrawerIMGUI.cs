using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Drawers.PropRangeDrawer;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.MinMaxSliderDrawer
{
    public partial class MinMaxSliderAttributeDrawer
    {
        private class ImGuiInfo
        {
            public string Error = "";
        }

        private static readonly Dictionary<string, ImGuiInfo> ImGuiInfos = new Dictionary<string, ImGuiInfo>();

        private static ImGuiInfo EnsureKey(SerializedProperty property)
        {
            string key = SerializedUtils.GetUniqueId(property);
            if (ImGuiInfos.TryGetValue(key, out ImGuiInfo info))
            {
                return info;
            }

            NoLongerInspectingWatch(property.serializedObject.targetObject, key, () =>
            {
                ImGuiInfos.Remove(key);
            });

            return ImGuiInfos[key] = new ImGuiInfo();
        }

        protected override float GetFieldHeight(SerializedProperty property, GUIContent label,
            float width,
            int index,
            ISaintsAttribute saintsAttribute, FieldInfo info, bool hasLabelWidth, object parent)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        protected override void DrawField(Rect position, SerializedProperty property, GUIContent label,
            int index,
            ISaintsAttribute saintsAttribute, IReadOnlyList<PropertyAttribute> allAttributes,
            FieldInfo info, object parent)
        {
            ImGuiInfo cacheInfo = EnsureKey(property);
            cacheInfo.Error = "";

            MetaInfo metaInfo = GetMetaInfo(property, saintsAttribute, info, parent);
            if (cacheInfo.Error == "" && metaInfo.Error != "")
            {
                cacheInfo.Error = metaInfo.Error;
            }
            if (cacheInfo.Error != "")
            {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_MIN_MAX_IMGUI
                Debug.LogError(cacheInfo.Error);
#endif
                RawDefaultDrawer(position, property, allAttributes, label, info);
                return;
            }

            MinMaxSliderAttribute minMaxSliderAttribute = (MinMaxSliderAttribute)saintsAttribute;
            float minValue = metaInfo.MinValue;
            float maxValue = metaInfo.MaxValue;
            AdaptAttribute adaptAttribute = allAttributes.OfType<AdaptAttribute>().FirstOrDefault();

            float labelWidth = label.text == "" ? 0 : EditorGUIUtility.labelWidth;

            float leftFieldWidth = 50f;
            leftFieldWidth += 5f;
            float rightFieldWidth = 50;

            float sliderWidth = position.width - labelWidth - leftFieldWidth - rightFieldWidth;
            const float sliderPadding = 4f;
            const float rightPadding = 2f;

            (Rect labelWithMinFieldRect, Rect fieldRect) =
                RectUtils.SplitWidthRect(position, labelWidth + leftFieldWidth);

            (Rect sliderRect, Rect field3Rect) = RectUtils.SplitWidthRect(new Rect(fieldRect)
            {
                x = fieldRect.x + sliderPadding,
            }, sliderWidth - sliderPadding);

            (Rect maxFloatFieldRect, Rect _) = RectUtils.SplitWidthRect(new Rect(field3Rect)
            {
                x = field3Rect.x + sliderPadding,
            }, rightFieldWidth);
            maxFloatFieldRect.width -= rightPadding;

            if (property.propertyType == SerializedPropertyType.Vector2)
            {
                Vector2 sliderValue = property.vector2Value;
                bool hasChange = false;
                using (EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
                {
                    EditorGUI.MinMaxSlider(sliderRect, ref sliderValue.x, ref sliderValue.y, minValue, maxValue);
                    if (changed.changed)
                    {
                        sliderValue = RemapFloatValue(sliderValue, minMaxSliderAttribute.Step, minValue, maxValue);
                        hasChange = true;
                    }
                }

                (string error, float value) preMinValue = PropRangeAttributeDrawer.GetPreValue(sliderValue.x, adaptAttribute);
                if (cacheInfo.Error == "" && preMinValue.error != "")
                {
                    cacheInfo.Error = preMinValue.error;
                }
                if (preMinValue.error != "")
                {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_MIN_MAX_IMGUI
                    Debug.LogError(preMinValue.error);
#endif
                    RawDefaultDrawer(position, property, allAttributes, label, info);
                    return;
                }

                using (EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
                {
                    float sliderX = EditorGUI.FloatField(labelWithMinFieldRect, label, preMinValue.value);
                    if (changed.changed)
                    {
                        (string error, float value) postMinValue = PropRangeAttributeDrawer.GetPostValue(sliderX, adaptAttribute);
                        if (cacheInfo.Error == "" && postMinValue.error != "")
                        {
                            cacheInfo.Error = postMinValue.error;
                        }
                        if (postMinValue.error != "")
                        {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_MIN_MAX_IMGUI
                            Debug.LogError(postMinValue.error);
#endif
                            return;
                        }

                        sliderValue = RemapFloatValue(new Vector2(postMinValue.value, sliderValue.y),
                            minMaxSliderAttribute.Step, minValue, maxValue);
                        hasChange = true;
                    }
                }

                (string error, float value) preMaxValue = PropRangeAttributeDrawer.GetPreValue(sliderValue.y, adaptAttribute);
                if (cacheInfo.Error == "" && preMaxValue.error != "")
                {
                    cacheInfo.Error = preMaxValue.error;
                }
                if (preMaxValue.error != "")
                {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_MIN_MAX_IMGUI
                    Debug.LogError(preMaxValue.error);
#endif
                    RawDefaultDrawer(position, property, allAttributes, label, info);
                    return;
                }

                using (EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
                {
                    float sliderY = EditorGUI.FloatField(maxFloatFieldRect, preMaxValue.value);
                    if (changed.changed)
                    {
                        (string error, float value) postMaxValue = PropRangeAttributeDrawer.GetPostValue(sliderY, adaptAttribute);
                        if (cacheInfo.Error == "" && postMaxValue.error != "")
                        {
                            cacheInfo.Error = postMaxValue.error;
                        }
                        if (postMaxValue.error != "")
                        {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_MIN_MAX_IMGUI
                            Debug.LogError(postMaxValue.error);
#endif
                            return;
                        }

                        sliderValue = RemapFloatValue(new Vector2(sliderValue.x, postMaxValue.value),
                            minMaxSliderAttribute.Step, minValue, maxValue);
                        hasChange = true;
                    }
                }

                if (hasChange)
                {
                    property.vector2Value = sliderValue;
                    TriggerChangedIMGUI(property, sliderValue);
                    property.serializedObject.ApplyModifiedProperties();
                }
            }
            else if (property.propertyType == SerializedPropertyType.Vector2Int)
            {
                Vector2 sliderValue = property.vector2IntValue;
                bool hasChange = false;
                using (EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
                {
                    EditorGUI.MinMaxSlider(sliderRect, ref sliderValue.x, ref sliderValue.y, minValue, maxValue);
                    if (changed.changed)
                    {
                        sliderValue = RemapIntValue(Vector2Int.RoundToInt(sliderValue), minMaxSliderAttribute.Step,
                            minValue, maxValue);
                        hasChange = true;
                    }
                }

                (string error, int value) preMinValue = PropRangeAttributeDrawer.GetPreValue((int)sliderValue.x, adaptAttribute);
                if (cacheInfo.Error == "" && preMinValue.error != "")
                {
                    cacheInfo.Error = preMinValue.error;
                }
                if (preMinValue.error != "")
                {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_MIN_MAX_IMGUI
                    Debug.LogError(preMinValue.error);
#endif
                    RawDefaultDrawer(position, property, allAttributes, label, info);
                    return;
                }

                using (EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
                {
                    int sliderX = EditorGUI.IntField(labelWithMinFieldRect, label, preMinValue.value);
                    if (changed.changed)
                    {
                        (string error, int value) postMinValue = PropRangeAttributeDrawer.GetPostValue(sliderX, adaptAttribute);
                        if (cacheInfo.Error == "" && postMinValue.error != "")
                        {
                            cacheInfo.Error = postMinValue.error;
                        }
                        if (postMinValue.error != "")
                        {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_MIN_MAX_IMGUI
                            Debug.LogError(postMinValue.error);
#endif
                            return;
                        }

                        sliderValue = RemapIntValue(new Vector2Int(postMinValue.value, (int)sliderValue.y),
                            minMaxSliderAttribute.Step, minValue, maxValue);
                        hasChange = true;
                    }
                }

                (string error, int value) preMaxValue = PropRangeAttributeDrawer.GetPreValue((int)sliderValue.y, adaptAttribute);
                if (cacheInfo.Error == "" && preMaxValue.error != "")
                {
                    cacheInfo.Error = preMaxValue.error;
                }
                if (preMaxValue.error != "")
                {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_MIN_MAX_IMGUI
                    Debug.LogError(preMaxValue.error);
#endif
                    RawDefaultDrawer(position, property, allAttributes, label, info);
                    return;
                }

                using (EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
                {
                    int sliderY = EditorGUI.IntField(maxFloatFieldRect, preMaxValue.value);
                    if (changed.changed)
                    {
                        (string error, int value) postMaxValue = PropRangeAttributeDrawer.GetPostValue(sliderY, adaptAttribute);
                        if (cacheInfo.Error == "" && postMaxValue.error != "")
                        {
                            cacheInfo.Error = postMaxValue.error;
                        }
                        if (postMaxValue.error != "")
                        {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_MIN_MAX_IMGUI
                            Debug.LogError(postMaxValue.error);
#endif
                            return;
                        }

                        sliderValue = RemapIntValue(new Vector2Int((int)sliderValue.x, postMaxValue.value),
                            minMaxSliderAttribute.Step, minValue, maxValue);
                        hasChange = true;
                    }
                }

                if (hasChange)
                {
                    Vector2Int newValue = Vector2Int.RoundToInt(sliderValue);
                    property.vector2IntValue = newValue;
                    TriggerChangedIMGUI(property, newValue);
                    property.serializedObject.ApplyModifiedProperties();
                }
            }

        }

        protected override bool WillDrawBelow(SerializedProperty property,
            IReadOnlyList<PropertyAttribute> allAttributes, ISaintsAttribute saintsAttribute,
            int index,
            FieldInfo info,
            object parent) => EnsureKey(property).Error != "";

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width,
            IReadOnlyList<PropertyAttribute> allAttributes,
            ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent) =>
            EnsureKey(property).Error == "" ? 0 : ImGuiHelpBox.GetHeight(EnsureKey(property).Error, width, MessageType.Error);

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, int index, IReadOnlyList<PropertyAttribute> allAttributes,
            FieldInfo info, object parent) =>
            EnsureKey(property).Error == "" ? position : ImGuiHelpBox.Draw(position, EnsureKey(property).Error, MessageType.Error);

    }
}
