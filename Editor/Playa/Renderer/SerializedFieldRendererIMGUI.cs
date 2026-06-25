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
        private RichTextDrawer _imguiRichTextDrawer;

        private void EnsureInit()
        {
            // InAnyHorizontalLayout = true;
            if (_imguiInit)
            {
                return;
            }

            _imguiInit = true;

            _imguiRichTextDrawer = new RichTextDrawer();

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

            _imguiDrawer = IMGUIRawDraw.GetAndCacheDrawer(FieldWithInfo.SerializedProperty, _imguiAllAttributes, FieldWithInfo.FieldInfo, _imguiLabel);
        }

        public override void OnDestroyIMGUI()
        {
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

            return IMGUIRawDraw.GetPropertyHeight(
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

            GUIContent useGUIContent;
            IEnumerable<RichTextDrawer.RichTextChunk> richTextChunks;
            if (preCheckResult.HasRichLabel)
            {
                useGUIContent = new GUIContent(new string(' ', FieldWithInfo.SerializedProperty.displayName.Length),
                    tooltip: FieldWithInfo.SerializedProperty.tooltip);
                richTextChunks = ParseRichXmlWithProviderIMGUI(preCheckResult.RichLabelXml);
            }
            else
            {
                useGUIContent = _imguiContent;
                richTextChunks = null;
            }

            IMGUIRawDraw.OnGUI(_imguiDrawer,
                position,
                FieldWithInfo.SerializedProperty,
                _imguiAllAttributes,
                FieldWithInfo.FieldInfo.FieldType,
                useGUIContent,
                richTextChunks,
                FieldWithInfo.FieldInfo,
                InAnyHorizontalLayout,
                false);
        }
    }
}
