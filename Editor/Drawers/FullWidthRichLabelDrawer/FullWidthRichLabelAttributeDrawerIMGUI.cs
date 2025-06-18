using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.FullWidthRichLabelDrawer
{
    public partial class FullWidthRichLabelAttributeDrawer
    {
        private readonly RichTextDrawer _richTextDrawer = new RichTextDrawer();
        private string _error = "";

        protected override void ImGuiOnDispose()
        {
            base.ImGuiOnDispose();
            _richTextDrawer.Dispose();
        }

        protected override bool WillDrawAbove(SerializedProperty property, ISaintsAttribute saintsAttribute,
            FieldInfo info,
            object parent)
        {
            FullWidthRichLabelAttribute fullWidthRichLabelAttribute = (FullWidthRichLabelAttribute)saintsAttribute;
            (string error, string xml) =
                RichTextDrawer.GetLabelXml(property, fullWidthRichLabelAttribute.RichTextXml,
                    fullWidthRichLabelAttribute.IsCallback, info, parent);
            if (error != "")
            {
                _error = error;
            }

            if (string.IsNullOrEmpty(xml))
            {
                return false;
            }

            return fullWidthRichLabelAttribute.Above;
        }

        protected override float GetAboveExtraHeight(SerializedProperty property, GUIContent label, float width,
            ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent)
        {
            FullWidthRichLabelAttribute fullWidthRichLabelAttribute = (FullWidthRichLabelAttribute)saintsAttribute;
            if (!fullWidthRichLabelAttribute.Above)
            {
                return 0;
            }

            (string error, string xml) =
                RichTextDrawer.GetLabelXml(property, fullWidthRichLabelAttribute.RichTextXml,
                    fullWidthRichLabelAttribute.IsCallback, info, parent);
            if (error != "")
            {
                _error = error;
            }

            return string.IsNullOrEmpty(xml) ? 0 : EditorGUIUtility.singleLineHeight;
        }

        protected override Rect DrawAboveImGui(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, OnGUIPayload onGUIPayload, FieldInfo info, object parent)
        {
            FullWidthRichLabelAttribute fullWidthRichLabelAttribute = (FullWidthRichLabelAttribute)saintsAttribute;
            if (!fullWidthRichLabelAttribute.Above)
            {
                return position;
            }

            return DrawImGui(position, property, label, saintsAttribute, info, parent);
        }

        private Rect DrawImGui(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, FieldInfo info, object parent)
        {
            FullWidthRichLabelAttribute fullWidthRichLabelAttribute = (FullWidthRichLabelAttribute)saintsAttribute;

            (string error, string xml) =
                RichTextDrawer.GetLabelXml(property, fullWidthRichLabelAttribute.RichTextXml,
                    fullWidthRichLabelAttribute.IsCallback, info, parent);
            if (error != "")
            {
                _error = error;
                return position;
            }

            if (xml is null)
            {
                return position;
            }

            (Rect curRect, Rect leftRect) = RectUtils.SplitHeightRect(position, EditorGUIUtility.singleLineHeight);

            string labelText = label.text;
#if SAINTSFIELD_NAUGHYTATTRIBUTES
            labelText = property.displayName;
#endif

            ImGuiEnsureDispose(property.serializedObject.targetObject);
            _richTextDrawer.DrawChunks(curRect, label,
                RichTextDrawer.ParseRichXml(xml, labelText, property, info, parent));
            return leftRect;
        }

        protected override bool WillDrawBelow(SerializedProperty property,
            IReadOnlyList<PropertyAttribute> allAttributes, ISaintsAttribute saintsAttribute,
            int index,
            FieldInfo info,
            object parent)
        {
            FullWidthRichLabelAttribute fullWidthRichLabelAttribute = (FullWidthRichLabelAttribute)saintsAttribute;
            (string error, string xml) =
                RichTextDrawer.GetLabelXml(property, fullWidthRichLabelAttribute.RichTextXml,
                    fullWidthRichLabelAttribute.IsCallback, info, parent);
            if (error != "")
            {
                _error = error;
            }

            if (_error != "")
            {
                return true;
            }

            if (fullWidthRichLabelAttribute.Above)
            {
                return false;
            }

            return !string.IsNullOrEmpty(xml);
        }

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width,
            IReadOnlyList<PropertyAttribute> allAttributes,
            ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent)
        {
            FullWidthRichLabelAttribute fullWidthRichLabelAttribute = (FullWidthRichLabelAttribute)saintsAttribute;
            float xmlHeight = 0;
            if (!fullWidthRichLabelAttribute.Above)
            {
                (string error, string xml) =
                    RichTextDrawer.GetLabelXml(property, fullWidthRichLabelAttribute.RichTextXml,
                        fullWidthRichLabelAttribute.IsCallback, info, parent);
                if (error != "")
                {
                    _error = error;
                }

                xmlHeight = string.IsNullOrEmpty(xml) ? 0 : EditorGUIUtility.singleLineHeight;
            }

            float errorHeight = _error == "" ? 0 : ImGuiHelpBox.GetHeight(_error, width, MessageType.Error);
            // Debug.Log($"#FullWidthRichLabel# below height={errorHeight}+{xmlHeight}/property={property.propertyPath}");

            return errorHeight + xmlHeight;
        }

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, int index, IReadOnlyList<PropertyAttribute> allAttributes,
            OnGUIPayload onGuiPayload, FieldInfo info, object parent)
        {
            // EditorGUI.DrawRect(position, Color.green);
            Rect useRect = position;
            FullWidthRichLabelAttribute fullWidthRichLabelAttribute = (FullWidthRichLabelAttribute)saintsAttribute;
            if (!fullWidthRichLabelAttribute.Above)
            {
                useRect = DrawImGui(position, property, label, fullWidthRichLabelAttribute, info, parent);
            }

            return _error == ""
                ? useRect
                : ImGuiHelpBox.Draw(useRect, _error, MessageType.Error);
        }
    }
}
