using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.OverlayRichLabelDrawer
{
    public partial class OverlayRichLabelAttributeDrawer
    {
        private readonly RichTextDrawer _richTextDrawer = new RichTextDrawer();

        protected override void ImGuiOnDispose()
        {
            base.ImGuiOnDispose();
            _richTextDrawer.Dispose();
        }

        private string _error = "";

        protected override bool DrawOverlay(Rect position, SerializedProperty property,
            GUIContent label,
            ISaintsAttribute saintsAttribute, bool hasLabel, FieldInfo info, object parent)
        {
            string inputContent = GetContent(property);
            if (inputContent == null) // null=error
            {
                return false;
            }

            OverlayRichLabelAttribute targetAttribute = (OverlayRichLabelAttribute)saintsAttribute;

            float contentWidth = GetPlainTextWidth(inputContent) + targetAttribute.Padding;
            (string error, string labelXml) = RichTextDrawer.GetLabelXml(property, targetAttribute.RichTextXml,
                targetAttribute.IsCallback, info, parent);

            _error = error;

            if (labelXml is null)
            {
                return false;
            }

            float labelWidth = hasLabel ? EditorGUIUtility.labelWidth : 0;

            RichTextDrawer.RichTextChunk[] payloads =
                RichTextDrawer.ParseRichXml(labelXml, label.text, property, info, parent).ToArray();
            float overlayWidth = _richTextDrawer.GetWidth(label, position.height, payloads);

            float leftWidth = position.width - labelWidth - contentWidth;

            bool hasEnoughSpace = !targetAttribute.End && leftWidth > overlayWidth;

            float useWidth = hasEnoughSpace ? overlayWidth : leftWidth;
            float useOffset = hasEnoughSpace ? labelWidth + contentWidth : position.width - overlayWidth;

            Rect overlayRect = new Rect(position)
            {
                x = position.x + useOffset,
                width = useWidth,
            };

            ImGuiEnsureDispose(property.serializedObject.targetObject);
            _richTextDrawer.DrawChunks(overlayRect, label, payloads);

            return true;
        }

        private static string GetContent(SerializedProperty property)
        {
            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
            switch (property.propertyType)
            {
                case SerializedPropertyType.Integer:
                    return property.longValue.ToString();
                case SerializedPropertyType.Float:
                    // return $"{property.doubleValue}";
                    return property.doubleValue.ToString(CultureInfo.InvariantCulture);
                case SerializedPropertyType.String:
                    return property.stringValue ?? "";
                default:
                    return null;
                // throw new ArgumentOutOfRangeException(nameof(property.propertyType), property.propertyType, null);
            }
        }

        private static float GetPlainTextWidth(string plainContent)
        {
            return EditorStyles.textField.CalcSize(new GUIContent(plainContent)).x;
        }

        protected override bool WillDrawBelow(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            FieldInfo info,
            object parent)
        {
            SerializedPropertyType propType = property.propertyType;
            bool notOk = propType != SerializedPropertyType.Integer && propType != SerializedPropertyType.Float &&
                         propType != SerializedPropertyType.String;
            if (notOk)
            {
                _error = $"Expect int/float/string, get {propType}";
            }

            return notOk;
        }

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width,
            ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent)
        {
            return _error == "" ? 0 : ImGuiHelpBox.GetHeight(_error, width, MessageType.Error);
        }

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, int index, IReadOnlyList<PropertyAttribute> allAttributes,
            OnGUIPayload onGuiPayload, FieldInfo info, object parent) =>
            _error == ""
                ? position
                : ImGuiHelpBox.Draw(position, _error, MessageType.Error);

    }
}
