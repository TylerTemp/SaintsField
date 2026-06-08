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

        private readonly RichTextDrawer _richTextDrawer = new RichTextDrawer();

        protected override void ImGuiOnDispose()
        {
            base.ImGuiOnDispose();
            _richTextDrawer.Dispose();
        }

        protected override bool DrawOverlay(Rect position, SerializedProperty property,
            GUIContent label,
            ISaintsAttribute saintsAttribute, bool hasLabel, FieldInfo info, object parent)
        {
            ImGuiInfo cacheInfo = EnsureKey(property);
            cacheInfo.Error = "";
            string inputContent = GetContent(property);
            if (inputContent == null) // null=error
            {
                return false;
            }

            OverlayTextAttribute targetAttribute = (OverlayTextAttribute)saintsAttribute;

            float contentWidth = GetPlainTextWidth(inputContent) + targetAttribute.Padding;
            (string error, string labelXml) = RichTextDrawer.GetLabelXml(property, targetAttribute.RichTextXml,
                targetAttribute.IsCallback, info, parent);

            cacheInfo.Error = error;

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
            FieldInfo info, object parent) =>
            EnsureKey(property).Error == ""
                ? position
                : ImGuiHelpBox.Draw(position, EnsureKey(property).Error, MessageType.Error);

    }
}
