using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.RichLabelDrawer
{
    public partial class RichLabelAttributeDrawer
    {

        private string _error = "";

        protected override void ImGuiOnDispose()
        {
            base.ImGuiOnDispose();
            _richTextDrawer.Dispose();
        }

        protected override bool WillDrawLabel(SerializedProperty property, ISaintsAttribute saintsAttribute,
            FieldInfo info,
            object parent)
        {
            RichLabelAttribute targetAttribute = (RichLabelAttribute)saintsAttribute;
            (string error, string xml) = RichTextDrawer.GetLabelXml(property, targetAttribute.RichTextXml,
                targetAttribute.IsCallback, info, parent);
            // bool result = GetLabelXml(property, targetAttribute) != null;
            // Debug.Log($"richLabel willDraw={result}");
            // return result;
            _error = error;
            return xml != null;
        }

        protected override void DrawLabel(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, FieldInfo info, object parent)
        {
            RichLabelAttribute targetAttribute = (RichLabelAttribute)saintsAttribute;

            ImGuiEnsureDispose(property.serializedObject.targetObject);
            (string error, string labelXml) = RichTextDrawer.GetLabelXml(property, targetAttribute.RichTextXml,
                targetAttribute.IsCallback, info, parent);
            _error = error;

            if (labelXml is null)
            {
                return;
            }

            string labelText = label.text;
#if SAINTSFIELD_NAUGHYTATTRIBUTES
            labelText = property.displayName;
#endif

            RichTextDrawer.RichTextChunk[] parsedXmlNode =
                RichTextDrawer.ParseRichXml(labelXml, labelText, property, info, parent).ToArray();

            _richTextDrawer.DrawChunks(position, label, parsedXmlNode);
        }

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width,
            IReadOnlyList<PropertyAttribute> allAttributes,
            ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent)
        {
            return _error == "" ? 0 : ImGuiHelpBox.GetHeight(_error, width, MessageType.Error);
        }

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, int index, IReadOnlyList<PropertyAttribute> allAttributes,
            OnGUIPayload onGuiPayload, FieldInfo info, object parent)
        {
            return _error == "" ? position : ImGuiHelpBox.Draw(position, _error, MessageType.Error);
        }

    }
}
