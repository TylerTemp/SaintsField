using System;
using System.Collections.Generic;
using System.Linq;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using SaintsField.Editor.Utils.IMGUIPlainDrawer;
using SaintsField.Playa;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Playa.Renderer
{
    public partial class SerializedFieldRenderer
    {
        private bool _imguiInit;
        private PropertyDrawer _imguiDrawer;
        private IReadOnlyList<Attribute> _imguiAllAttributes;
        private string _imguiLabel;
        private GUIContent _imguiContent;

        private void EnsureInit()
        {
            // InAnyHorizontalLayout = true;
            if (_imguiInit)
            {
                return;
            }

            _imguiInit = true;

            _imguiLabel = null;
            if (!NoLabel)
            {
                _imguiLabel = FieldWithInfo.SerializedProperty.displayName;
                if (_imguiLabel.EndsWith(Util.SaintsSerializedLabelSuffix) && FieldWithInfo.PlayaAttributes.Any(each => each is SaintsSerializedAttribute))
                {
                    _imguiLabel = _imguiLabel[..^Util.SaintsSerializedLabelSuffix.Length];
                }
            }

            _imguiContent = new GUIContent(_imguiLabel)
            {
                tooltip = FieldWithInfo.SerializedProperty.tooltip,
            };

            _imguiAllAttributes = ReflectCache.GetCustomAttributes<Attribute>(FieldWithInfo.FieldInfo);

            _imguiDrawer = IMGUIUtils.GetAndCacheDrawer(FieldWithInfo.SerializedProperty, _imguiAllAttributes, FieldWithInfo.FieldInfo, _imguiLabel);
        }

        protected override float GetFieldHeightIMGUI(float width, PreCheckResult preCheckResult)
        {
            EnsureInit();

            if (!preCheckResult.IsShown)
            {
                return 0f;
            }

            GUIContent useGUIContent = preCheckResult.HasRichLabel
                ? new GUIContent(new string(' ', FieldWithInfo.SerializedProperty.displayName.Length), tooltip: FieldWithInfo.SerializedProperty.tooltip)
                : _imguiContent;

            return IMGUIUtils.GetPropertyHeight(
                _imguiDrawer,
                useGUIContent,
                FieldWithInfo.SerializedProperty,
                _imguiAllAttributes,
                FieldWithInfo.FieldInfo.FieldType,
                FieldWithInfo.FieldInfo,
                InAnyHorizontalLayout
            );
        }

        // protected override void SerializedFieldRenderPositionTargetIMGUI(Rect position, PreCheckResult preCheckResult)
        protected override void RenderPositionTargetIMGUI(Rect position, PreCheckResult preCheckResult)
        {
            EnsureInit();

#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_SAINTS_EDITOR_SERIALIZED_FIELD_RENDERER
            Debug.Log($"SerField: {FieldWithInfo.SerializedProperty.displayName}->{FieldWithInfo.SerializedProperty.propertyPath}; arraySize={preCheckResult.ArraySize}");
#endif
            if (!preCheckResult.IsShown)
            {
                return;
            }

            GUIContent useGUIContent = preCheckResult.HasRichLabel
                ? new GUIContent(new string(' ', FieldWithInfo.SerializedProperty.displayName.Length), tooltip: FieldWithInfo.SerializedProperty.tooltip)
                : _imguiContent;

            IMGUIUtils.OnGUI(_imguiDrawer,
                position,
                FieldWithInfo.SerializedProperty,
                _imguiAllAttributes,
                FieldWithInfo.FieldInfo.FieldType,
                useGUIContent,
                FieldWithInfo.FieldInfo,
                InAnyHorizontalLayout,
                false);
        }

        protected override void RenderTargetIMGUI(float width, PreCheckResult preCheckResult)
        {
            float height = GetFieldHeightIMGUI(width, preCheckResult);
            if (height <= Mathf.Epsilon)
            {
                return;
            }
            Rect rect = EditorGUILayout.GetControlRect(true, height, GUILayout.ExpandWidth(true));
            RenderPositionTargetIMGUI(rect, preCheckResult);
        }
    }
}
