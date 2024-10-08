using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;
#if UNITY_2021_3_OR_NEWER
using UnityEngine.UIElements;
#endif

namespace SaintsField.Editor.Drawers
{

    [CustomPropertyDrawer(typeof(PostFieldButtonAttribute))]
    public class PostFieldButtonAttributeDrawer: DecButtonAttributeDrawer
    {
        private const float PaddingWidth = 3f;

        #region IMGUI

        protected override float GetPostFieldWidth(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, int index, OnGUIPayload onGuiPayload, FieldInfo info, object parent)
        {
            DecButtonAttribute decButtonAttribute = (DecButtonAttribute)saintsAttribute;

            object target = property.serializedObject.targetObject;
            (string xmlError, string labelXml) = RichTextDrawer.GetLabelXml(property, decButtonAttribute.ButtonLabel, decButtonAttribute.IsCallback, info, target);
            GetOrCreateErrorInfo(property).Error = xmlError;

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
                richChunks = RichTextDrawer.ParseRichXml(labelXml, label.text, info, parent).ToArray();
            }

            return PaddingWidth * 2 + Mathf.Min(position.width, Mathf.Max(10, RichTextDrawer.GetWidth(label, position.height, richChunks)));
        }

        protected override bool DrawPostFieldImGui(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, int index, OnGUIPayload onGUIPayload, FieldInfo info, object parent)
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
            ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent)
        {
            string displayError = GetDisplayError(property);
            return displayError == ""
                ? position
                : ImGuiHelpBox.Draw(position, displayError, MessageType.Error);
        }

        #endregion

#if UNITY_2021_3_OR_NEWER

        #region UIToolkit

        protected override VisualElement CreatePostFieldUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index, VisualElement container, FieldInfo info, object parent)
        {
            VisualElement element = DrawUIToolkit(property, saintsAttribute, index, info, parent, container);
            element.style.flexGrow = StyleKeyword.Null;
            return element;
        }

        protected override VisualElement CreateBelowUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index, VisualElement container, FieldInfo info, object parent)
        {
            VisualElement visualElement = new VisualElement
            {
                style =
                {
                    flexGrow = 1,
                },
            };
            visualElement.Add(DrawLabelError(property, index));
            visualElement.Add(DrawExecError(property, index));
            return visualElement;
        }

        #endregion

#endif
    }
}
