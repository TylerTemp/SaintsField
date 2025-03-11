using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.ButtonDrawers.PostFieldButtonDrawer
{
    public partial class PostFieldButtonAttributeDrawer
    {
        private const float PaddingWidth = 3f;

        protected override float GetPostFieldWidth(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, int index, OnGUIPayload onGuiPayload, FieldInfo info, object parent)
        {
            DecButtonAttribute decButtonAttribute = (DecButtonAttribute)saintsAttribute;

            object target = property.serializedObject.targetObject;
            (string xmlError, string labelXml) = RichTextDrawer.GetLabelXml(property, decButtonAttribute.ButtonLabel,
                decButtonAttribute.IsCallback, info, target);
            GetOrCreateButtonInfo(property).Error = xmlError;

            IReadOnlyList<RichTextDrawer.RichTextChunk> richChunks;
            // ReSharper disable once ConvertIfStatementToNullCoalescingAssignment
            if (labelXml is null)
            {
                labelXml = ObjectNames.NicifyVariableName(decButtonAttribute.FuncName);
                richChunks = new[]
                {
                    new RichTextDrawer.RichTextChunk
                    {
                        IsIcon = false,
                        Content = labelXml,
                    },
                };
            }
            else
            {
                richChunks = RichTextDrawer.ParseRichXml(labelXml, label.text, property, info, parent).ToArray();
            }

            return PaddingWidth * 2 + Mathf.Min(position.width,
                Mathf.Max(10, RichTextDrawer.GetWidth(label, position.height, richChunks)));
        }

        protected override bool DrawPostFieldImGui(Rect position, Rect fullRect, SerializedProperty property,
            GUIContent label,
            ISaintsAttribute saintsAttribute, int index, IReadOnlyList<PropertyAttribute> allAttributes,
            OnGUIPayload onGUIPayload, FieldInfo info, object parent)
        {
            // Debug.Log($"draw below {position}");
            // return Draw(position, property, label, saintsAttribute);
            // float width = GetPostFieldWidth(position, property, label, saintsAttribute);
            // (Rect useRect, Rect leftRect) = RectUtils.SplitWidthRect(position, width);
            Draw(position, property, label, saintsAttribute, info, parent);
            return true;
        }

        protected override bool WillDrawBelow(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            FieldInfo info,
            object parent)
        {
            return GetDisplayError(property) != "";
        }

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width,
            ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent)
        {
            string displayError = GetDisplayError(property);
            return displayError == "" ? 0 : ImGuiHelpBox.GetHeight(displayError, width, MessageType.Error);
        }

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, int index, IReadOnlyList<PropertyAttribute> allAttributes,
            OnGUIPayload onGuiPayload, FieldInfo info, object parent)
        {
            string displayError = GetDisplayError(property);
            return displayError == ""
                ? position
                : ImGuiHelpBox.Draw(position, displayError, MessageType.Error);
        }
    }
}
